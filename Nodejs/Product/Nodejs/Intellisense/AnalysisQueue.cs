//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Intellisense {
    sealed partial class VsProjectAnalyzer {
        /// <summary>
        /// Provides a single threaded analysis queue.  Items can be enqueued into the
        /// analysis at various priorities.  
        /// </summary>
        sealed class AnalysisQueue : IDisposable {
            private readonly Thread _workThread;
            private readonly AutoResetEvent _workEvent;
            private readonly VsProjectAnalyzer _analyzer;
            private readonly object _queueLock = new object();
            private readonly List<IAnalyzable>[] _queue;
            private readonly HashSet<IGroupableAnalysisProject> _enqueuedGroups;
            [NonSerialized]
            private DateTime _lastSave;
            private readonly CancellationTokenSource _cancel;
            private bool _isAnalyzing;
            private int _analysisPending;
            private static readonly TimeSpan _SaveAnalysisTime = TimeSpan.FromMinutes(15);

            private const int PriorityCount = (int)AnalysisPriority.High + 1;

            internal AnalysisQueue(VsProjectAnalyzer analyzer) {
                _workEvent = new AutoResetEvent(false);
                _cancel = new CancellationTokenSource();
                _analyzer = analyzer;

                // save the analysis once it's ready, but give us a little time to be
                // initialized and start processing stuff...
                _lastSave = DateTime.Now - _SaveAnalysisTime + TimeSpan.FromSeconds(10);

                _queue = new List<IAnalyzable>[PriorityCount];
                for (int i = 0; i < PriorityCount; i++) {
                    _queue[i] = new List<IAnalyzable>();
                }
                _enqueuedGroups = new HashSet<IGroupableAnalysisProject>();

                _workThread = new Thread(Worker);
                _workThread.Name = "Node.js Analysis Queue";
                _workThread.Priority = ThreadPriority.BelowNormal;
                _workThread.IsBackground = true;

                // start the thread, wait for our synchronization context to be created
                using (AutoResetEvent threadStarted = new AutoResetEvent(false)) {
                    _workThread.Start(threadStarted);
                    threadStarted.WaitOne();
                }
            }

            public void Enqueue(IAnalyzable item, AnalysisPriority priority) {
                int iPri = (int)priority;

                if (iPri < 0 || iPri > _queue.Length) {
                    throw new ArgumentException("priority");
                }

                lock (_queueLock) {
                    // see if we have the item in the queue anywhere...
                    for (int i = 0; i < _queue.Length; i++) {
                        if (_queue[i].Remove(item)) {
                            Interlocked.Decrement(ref _analysisPending);

                            AnalysisPriority oldPri = (AnalysisPriority)i;

                            if (oldPri > priority) {
                                // if it was at a higher priority then our current
                                // priority go ahead and raise the new entry to our
                                // old priority
                                priority = oldPri;
                            }

                            break;
                        }
                    }

                    // enqueue the work item
                    Interlocked.Increment(ref _analysisPending);
                    if (priority == AnalysisPriority.High) {
                        // always try and process high pri items immediately
                        _queue[iPri].Insert(0, item);
                    } else {
                        _queue[iPri].Add(item);
                    }
                    _workEvent.Set();
                }
            }

            public void Stop() {
                _cancel.Cancel();
                if (_workThread.IsAlive) {
                    _workEvent.Set();
                    _workThread.Join();
                }
            }

            public bool IsAnalyzing {
                get {
                    lock (_queueLock) {
                        return _isAnalyzing || _analysisPending > 0;
                    }
                }
            }

            public int AnalysisPending {
                get {
                    return _analysisPending;
                }
            }

            #region IDisposable Members

            public void Dispose() {
                Stop();

                _cancel.Dispose();
                _workEvent.Dispose();
            }

            #endregion

            private IAnalyzable GetNextItem(out AnalysisPriority priority) {
                for (int i = PriorityCount - 1; i >= 0; i--) {
                    if (_queue[i].Count > 0) {
                        var res = _queue[i][0];
                        _queue[i].RemoveAt(0);
                        Interlocked.Decrement(ref _analysisPending);
                        priority = (AnalysisPriority)i;
                        return res;
                    }
                }
                priority = AnalysisPriority.None;
                return null;
            }

            private void Worker(object threadStarted) {
                ((AutoResetEvent)threadStarted).Set();
                Stopwatch watch = new Stopwatch();
                watch.Start();
                long startTime = watch.ElapsedMilliseconds;
                bool analyzedAnything = false;
                while (!_cancel.IsCancellationRequested) {
                    IAnalyzable workItem;

                    AnalysisPriority pri;
                    lock (_queueLock) {
                        workItem = GetNextItem(out pri);
                        _isAnalyzing = true;
                    }
                    if (workItem != null) {
                        analyzedAnything = true;
                        var groupable = workItem as IGroupableAnalysisProjectEntry;
                        if (groupable != null) {
                            bool added = _enqueuedGroups.Add(groupable.AnalysisGroup);
                            if (added) {
                                Enqueue(new GroupAnalysis(groupable.AnalysisGroup, this), pri);
                            }

                            groupable.Analyze(_cancel.Token, true);
                        } else {
                            workItem.Analyze(_cancel.Token);
                        }
                    } else {
                        // Don't update status or save to disk for implicit projects,
                        // otherwise we end up overloading the status bar with conflicting messages.
                        if (!_analyzer._implicitProject) {
                            var elapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds - startTime);
                            UpdateAnalysisStatusAndSaveToDisk(analyzedAnything, elapsedTime);
                        }

                        _isAnalyzing = false;
                        WaitHandle.SignalAndWait(
                            _analyzer._queueActivityEvent,
                            _workEvent
                        );
                        startTime = watch.ElapsedMilliseconds;
                    }
                }
                _isAnalyzing = false;
            }

            private void UpdateAnalysisStatusAndSaveToDisk(bool analyzedAnything, TimeSpan elapsedTime) {
                string statsMessage = null;
                if (_analyzer._jsAnalyzer != null) {
                    int count = _analyzer._jsAnalyzer.GetAndClearAnalysisCount();
                    if (count != 0) {
                        statsMessage = SR.GetString(SR.StatusAnalysisUpToDate, count, FormatTime(elapsedTime));
                    }
                }

                if (_analyzer._saveToDisk && analyzedAnything && (DateTime.Now - _lastSave) > _SaveAnalysisTime) {
                    var statusbar = (IVsStatusbar)NodejsPackage.GetGlobalService(typeof(SVsStatusbar));
                    if (statusbar != null) {
                        statusbar.SetText(SR.GetString(SR.StatusAnalysisSaving) + " " + statsMessage);
                    }

                    _analyzer.SaveAnalysis();
                    _lastSave = DateTime.Now;

                    if (statusbar != null) {
                        statusbar.SetText(SR.GetString(SR.StatusAnalysisSaved) + " " + statsMessage);
                    }
                } else if (statsMessage != null) {
                    var statusbar = (IVsStatusbar)NodejsPackage.GetGlobalService(typeof(SVsStatusbar));
                    if (statusbar != null) {
                        statusbar.SetText(statsMessage);
                    }
                }
            }

            private static string FormatTime(TimeSpan elapsedTime) {
                if (elapsedTime.TotalMilliseconds < 1000) {
                    return elapsedTime.TotalMilliseconds + " " + SR.GetString(SR.Milliseconds);
                } else if (elapsedTime.TotalSeconds < 60) {
                    return elapsedTime.TotalSeconds + " " + SR.GetString(SR.Seconds);
                }
                return elapsedTime.ToString("g");
            }

            sealed class GroupAnalysis : IAnalyzable {
                private readonly IGroupableAnalysisProject _project;
                private readonly AnalysisQueue _queue;

                public GroupAnalysis(IGroupableAnalysisProject project, AnalysisQueue queue) {
                    _project = project;
                    _queue = queue;
                }

                #region IAnalyzable Members

                public void Analyze(CancellationToken cancel) {
                    _queue._enqueuedGroups.Remove(_project);
                    _project.AnalyzeQueuedEntries(cancel);
                }

                #endregion
            }

            internal void ResetLastSaveTime() {
                //Set the last save time to a value in the past beyond the timespan between saves
                //  The next edit will trigger a save
                _lastSave = DateTime.Now - _SaveAnalysisTime - TimeSpan.FromMinutes(30);
            }
        }
    }
}

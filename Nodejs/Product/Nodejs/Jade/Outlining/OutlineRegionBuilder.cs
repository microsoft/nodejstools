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
using System.Threading;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Jade
{
    internal abstract class OutlineRegionBuilder : IDisposable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public EventHandler<OutlineRegionsChangedEventArgs> RegionsChanged;

        protected OutlineRegionCollection CurrentRegions { get; set; }
        protected IdleTimeAsyncTask BackgroundTask { get; set; }
        protected ITextBuffer TextBuffer { get; set; }

        private long _disposed = 0;
        private object _regionsLock = new object();

        protected OutlineRegionBuilder(ITextBuffer textBuffer)
        {
            CurrentRegions = new OutlineRegionCollection(0);

            TextBuffer = textBuffer;
            TextBuffer.Changed += OnTextBufferChanged;

            BackgroundTask = new IdleTimeAsyncTask(TaskAction, MainThreadAction);
            BackgroundTask.DoTaskOnIdle(300);
        }

        protected virtual void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // In order to provide nicer experience when user presser and holds
            // ENTER or DELETE or just types really fast, we are going to track
            // regions optimistically and report changes without going through
            // async or idle processing. Idle/async is still going to hit later.

            if (e.Changes.Count > 0)
            {
                int start, oldLength, newLength;
                TextUtility.CombineChanges(e, out start, out oldLength, out newLength);

                int changeStart = Int32.MaxValue;
                int changeEnd = 0;

                lock (_regionsLock)
                {
                    // Remove affected regions and shift the remaining ones. Outlining 
                    // regions are not sorted and can overlap. Hence linear search.

                    for (int i = 0; i < CurrentRegions.Count; i++)
                    {
                        var region = CurrentRegions[i];

                        if (region.End <= start)
                        {
                            continue;
                        }

                        if (region.Contains(start) && region.Contains(start + oldLength))
                        {
                            region.Expand(0, newLength - oldLength);
                        }
                        else if (region.Start >= start + oldLength)
                        {
                            region.Shift(newLength - oldLength);
                        }
                        else
                        {
                            CurrentRegions.RemoveAt(i);
                            i--;
                        }

                        changeStart = Math.Min(changeStart, region.Start);
                        changeEnd = Math.Max(changeEnd, region.End);
                    }

                    if (changeStart < Int32.MaxValue)
                        CurrentRegions.TextBufferVersion = TextBuffer.CurrentSnapshot.Version.VersionNumber;
                }

                if (changeStart < Int32.MaxValue)
                {
                    if (RegionsChanged != null)
                        RegionsChanged(this, new OutlineRegionsChangedEventArgs(CurrentRegions, TextRange.FromBounds(changeStart, changeEnd)));
                }
            }
        }

        protected abstract void BuildRegions(OutlineRegionCollection newRegions);

        protected bool IsDisposed()
        {
            return Interlocked.Read(ref _disposed) > 0;
        }

        protected virtual object TaskAction()
        {
            if (!IsDisposed())
            {
                var snapshot = TextBuffer.CurrentSnapshot;
                var newRegions = new OutlineRegionCollection(snapshot.Version.VersionNumber);

                BuildRegions(newRegions);

                lock (_regionsLock)
                {
                    var changedRange = CompareRegions(newRegions, CurrentRegions, snapshot.Length);
                    return new OutlineRegionsChange(changedRange, newRegions);
                }
            }

            return null;
        }

        protected virtual void MainThreadAction(object backgroundProcessingResult)
        {
            if (!IsDisposed())
            {
                var result = backgroundProcessingResult as OutlineRegionsChange;

                if (result != null && TextRange.IsValid(result.ChangedRange))
                {
                    lock (_regionsLock)
                    {
                        CurrentRegions = result.NewRegions;
                    }

                    if (RegionsChanged != null)
                    {
                        RegionsChanged(this,
                            new OutlineRegionsChangedEventArgs(CurrentRegions.Clone() as OutlineRegionCollection,
                            result.ChangedRange)
                         );
                    }
                }
            }
        }

        protected static ITextRange CompareRegions(
            OutlineRegionCollection newRegions,
            OutlineRegionCollection oldRegions, int upperBound)
        {
            TextRangeCollection<OutlineRegion> oldClone = null;
            TextRangeCollection<OutlineRegion> newClone = null;

            if (oldRegions != null)
            {
                oldClone = oldRegions.Clone() as OutlineRegionCollection;
                oldClone.Sort();
            }

            newClone = newRegions.Clone() as OutlineRegionCollection;
            newClone.Sort();

            return newClone.RangeDifference(oldClone, 0, upperBound);
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Read(ref _disposed) == 0)
            {
                Interlocked.Exchange(ref _disposed, 1);
                if (TextBuffer != null)
                {
                    TextBuffer.Changed -= OnTextBufferChanged;
                    TextBuffer = null;
                }

                if (BackgroundTask != null)
                {
                    BackgroundTask.Dispose();
                    BackgroundTask = null;
                }
            }
        }
        #endregion
    }
}

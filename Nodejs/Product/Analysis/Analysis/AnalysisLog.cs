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
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.Analysis.Values;

namespace Microsoft.NodejsTools.Analysis {
    internal class AnalysisLog {
        DateTime StartTime = DateTime.UtcNow;
        TimeSpan? LastDisplayedTime;
        Deque<LogItem> LogItems = new Deque<LogItem>();
        int LogIndex;

        public int MaxItems { get; set; }

        struct LogItem {
            public TimeSpan Time;
            public string Event;
            public object[] Args;

            public override string ToString() {
                return string.Format("[{0}] {1}: {2}", Time, Event, string.Join(", ", Args));
            }
        }

        public void Dump(TextWriter output, bool asCsv = false) {
            // we don't enumerate here because that can throw if the log is being updated
            // by another thread in the background.  We should either catch the new events
            // being appended at the end, or hopefully we stay in front of the updates
            // which are coming at the beginning.
            for (int i = LogIndex; i < LogItems.Count; i++) {
                DumpItem(output, asCsv, LogItems[i]);
            }

            for (int i = 0; i < LogIndex; i++) {
                DumpItem(output, asCsv, LogItems[i]);
            }
            output.Flush();
        }

        private void DumpItem(TextWriter output, bool asCsv, LogItem item) {
            if (!LastDisplayedTime.HasValue || item.Time.Subtract(LastDisplayedTime.GetValueOrDefault()) > TimeSpan.FromMilliseconds(100)) {
                LastDisplayedTime = item.Time;
                output.WriteLine(asCsv ? "TS, {0}, {1}" : "[TS] {0}, {1}", item.Time.TotalMilliseconds, item.Time);
            } else if (item.Time.Subtract(LastDisplayedTime.GetValueOrDefault()) < TimeSpan.Zero) {
                LastDisplayedTime = item.Time;
                // racing with the analysis...
                output.WriteLine(asCsv ? "TSW" : "[TSW]");
            }

            try {
                if (asCsv) {
                    output.WriteLine("{0}, {1}", item.Event, string.Join(", ", AsCsvStrings(item.Args)));
                } else {
                    output.WriteLine("[{0}] {1}", item.Event, string.Join(", ", item.Args));
                }
            } catch { }
        }

        static IEnumerable<string> AsCsvStrings(IEnumerable<object> items) {
            foreach (var item in items) {
                var str = item.ToString();
                if (str.Contains(',') || str.Contains('"')) {
                    str = "\"" + str.Replace("\"", "\"\"") + "\"";
                }
                yield return str;
            }
        }

        private void Add(string Event, params object[] Args) {
            int max = MaxItems;

            if (max != 0) {
                if (LogItems.Count >= max) {
                    while (LogItems.Count > max) {
                        // if the queue was set to shrink remove any old items.
                        LogItems.PopLeft();
                    }
                    if (LogIndex >= max) {
                        LogIndex = 0;
                    }
                    LogItems[LogIndex++] = new LogItem { Time = Time, Event = Event, Args = Args };
                } else {
                    LogItems.Append(new LogItem { Time = Time, Event = Event, Args = Args });
                }
            }
        }

        TimeSpan Time {
            get {
                return DateTime.UtcNow - StartTime;
            }
        }

        internal void Enqueue(Deque<AnalysisUnit> deque, AnalysisUnit unit) {
            if (MaxItems != 0) {
                Add("E", unit.DeclaringModuleEnvironment.ProjectEntry.FilePath, deque.Count);
            }
        }

        internal void Dequeue(Deque<AnalysisUnit> deque, AnalysisUnit unit) {
            if (MaxItems != 0) {
                Add("D", unit.DeclaringModuleEnvironment.ProjectEntry.FilePath, deque.Count);
            }
        }

        internal class AnalysisUnitWrapper {
            private readonly WeakReference _unit;

            public AnalysisUnitWrapper(AnalysisUnit unit) {
                _unit = new WeakReference(unit);
            }

            public override string ToString() {
                var value = _unit.Target as AnalysisUnit;
                if(value != null) {
                    return String.Format("{0} {1}", value.FullName, value.ToString());
                }
                return "<collected unit>";
            }
        }

        internal void NewUnit(AnalysisUnit unit) {
            if (MaxItems != 0) {
                Add("N", unit.DeclaringModuleEnvironment.ProjectEntry.FilePath, new AnalysisUnitWrapper(unit));
            }
        }

        internal void EndOfQueue(int beforeLength, int afterLength) {
            if (MaxItems != 0) {
                Add("Q", beforeLength, afterLength, afterLength - beforeLength);
            }
        }

        internal void ExceedsTypeLimit(string variableDefType, int total, string contents) {
            if (MaxItems != 0) {
                Add("X", variableDefType, total, contents);
            }
        }

        internal void Cancelled(Deque<AnalysisUnit> queue) {
            if (MaxItems != 0) {
                Add("Cancel", queue.Count);
            }
        }

        internal static void Assert(bool condition, string message = null) {
            if (!condition) {
                try {
                    throw new InvalidOperationException(message);
                } catch (InvalidOperationException) {
                }
            }
        }
    }
}

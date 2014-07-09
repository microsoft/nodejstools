/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.NodejsTools.Analysis.Values;

namespace Microsoft.NodejsTools.Analysis {
    public static class AnalysisLog {
        static DateTime StartTime = DateTime.UtcNow;
        static TimeSpan? LastDisplayedTime;
        static Deque<LogItem> LogItems = new Deque<LogItem>();
        static int LogIndex;

        public static int MaxItems { get; set; }

        struct LogItem {
            public TimeSpan Time;
            public string Event;
            public object[] Args;

            public override string ToString() {
                return string.Format("[{0}] {1}: {2}", Time, Event, string.Join(", ", Args));
            }
        }

        public static void Dump(TextWriter output, bool asCsv = false) {
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

        private static void DumpItem(TextWriter output, bool asCsv, LogItem item) {
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

        private static void Add(string Event, params object[] Args) {
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

        static TimeSpan Time {
            get {
                return DateTime.UtcNow - StartTime;
            }
        }

        internal static void Enqueue(Deque<AnalysisUnit> deque, AnalysisUnit unit) {
            if (MaxItems != 0) {
                Add("E", unit.Id, deque.Count);
            }
        }

        internal static void Dequeue(Deque<AnalysisUnit> deque, AnalysisUnit unit) {
            if (MaxItems != 0) {
                Add("D", unit.Id, deque.Count);
            }
        }

        class AnalysisUnitWrapper {
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

        internal static void NewUnit(AnalysisUnit unit) {
            if (MaxItems != 0) {
                Add("N", unit.Id, new AnalysisUnitWrapper(unit));
            }
        }

        internal static void EndOfQueue(int beforeLength, int afterLength) {
            if (MaxItems != 0) {
                Add("Q", beforeLength, afterLength, afterLength - beforeLength);
            }
        }

        internal static void ExceedsTypeLimit(string variableDefType, int total, string contents) {
            if (MaxItems != 0) {
                Add("X", variableDefType, total, contents);
            }
        }

        internal static void Cancelled(Deque<AnalysisUnit> queue) {
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

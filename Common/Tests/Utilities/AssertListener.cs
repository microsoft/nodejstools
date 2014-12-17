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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities {
    public class AssertListener : TraceListener {
        private readonly SynchronizationContext _testContext;

        private AssertListener() {
            _testContext = SynchronizationContext.Current;
        }

        public override string Name {
            get { return "AssertListener"; }
            set { }
        }

        public static void Initialize() {
            if (null == Debug.Listeners["AssertListener"]) {
                Debug.Listeners.Add(new AssertListener());
                Debug.Listeners.Remove("Default");

                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            }
        }

        static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e) {
            if (e.Exception is NullReferenceException || e.Exception is ObjectDisposedException) {
                // Exclude safe handle messages because they are noisy
                if (!e.Exception.Message.Contains("Safe handle has been closed")) {
                    var log = new EventLog("Application");
                    log.Source = "Application Error";
                    log.WriteEntry(
                        "First-chance exception: " + e.Exception.ToString(),
                        EventLogEntryType.Warning
                    );
                }
            }
        }

        public override void Fail(string message) {
            Fail(message, null);
        }

        public override void Fail(string message, string detailMessage) {
            Trace.WriteLine("Debug.Assert failed");
            if (!string.IsNullOrEmpty(message)) {
                Trace.WriteLine(message);
            } else {
                Trace.WriteLine("(No message provided)");
            }
            if (!string.IsNullOrEmpty(detailMessage)) {
                Trace.WriteLine(detailMessage);
            }
            var trace = new StackTrace(true);
            bool seenDebugAssert = false;
            foreach (var frame in trace.GetFrames()) {
                var mi = frame.GetMethod();
                if (!seenDebugAssert) {
                    seenDebugAssert = (mi.DeclaringType == typeof(Debug) && mi.Name == "Assert");
                } else if (mi.DeclaringType == typeof(System.RuntimeMethodHandle)) {
                    break;
                } else {
                    Trace.WriteLine(string.Format(
                        " at {0}.{1}({2}) in {3}:line {4}",
                        mi.DeclaringType.FullName,
                        mi.Name,
                        string.Join(", ", mi.GetParameters().Select(p => p.ToString())),
                        frame.GetFileName(),
                        frame.GetFileLineNumber()
                    ));
                    try {
                        Trace.WriteLine(
                            "    " +
                            File.ReadLines(frame.GetFileName()).ElementAt(frame.GetFileLineNumber() - 1).Trim()
                        );
                    } catch {
                    }
                }
            }

            message = string.IsNullOrEmpty(message) ? "Debug.Assert failed" : message;
            if (Debugger.IsAttached) {
                Debugger.Break();
            }

            if (_testContext != null && _testContext != SynchronizationContext.Current) {
                _testContext.Post(_ => Assert.Fail(message), null);
            } else {
                Assert.Fail(message);
            }
        }

        public override void WriteLine(string message) {
        }

        public override void Write(string message) {
        }
    }
}

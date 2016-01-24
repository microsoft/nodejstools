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
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.VSTestHost;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class DebuggerUITests : NodejsProjectTest {
        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Debugging UI")]
        [HostType("VSTestHost")]
        public void TestWaitOnExit() {
            foreach (var debug in new[] { false, true }) {
                foreach (var exitCode in new[] { 0, 1 }) {
                    foreach (var waitOnAbnormal in new[] { true, false }) {
                        foreach (var waitOnNormal in new[] { true, false }) {
                            Console.WriteLine("Testing {0} {1} {2} {3}", debug, exitCode, waitOnAbnormal, waitOnNormal);
                            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());

                            var project = Project("WaitOnExit",

                                Compile("server", @"
require('fs').writeFileSync('" + filename.Replace("\\", "\\\\") + @"', 'started')
process.exit(" + exitCode + ");"),
                                Property(CommonConstants.StartupFile, "server.js")
                            );

                            using (var solution = project.Generate().ToVs()) {
                                var waitingOnProcess = (waitOnAbnormal || waitOnNormal) ? "Microsoft.NodejsTools.PressAnyKey" : "node";
                                var beginningProcesses = System.Diagnostics.Process.GetProcessesByName(
                                    waitingOnProcess
                                ).Select(x => x.Id);

                                NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit = waitOnAbnormal;
                                NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit = waitOnNormal;

                                if (debug) {
                                    solution.ExecuteCommand("Debug.Start");
                                } else {
                                    solution.ExecuteCommand("Debug.StartWithoutDebugging");
                                }

                                for (int i = 0; i < 10 && !File.Exists(filename); i++) {
                                    System.Threading.Thread.Sleep(1000);
                                }

                                System.Threading.Thread.Sleep(1000); // Give process a chance to exit...

                                var currentProcesses = System.Diagnostics.Process.GetProcessesByName(
                                    waitingOnProcess
                                ).Select(x => x.Id);

                                var newProcesses = currentProcesses.Except(beginningProcesses).ToArray();
                                if (exitCode == 0) {
                                    Assert.AreEqual(
                                        waitOnNormal ? 1 : 0,
                                        newProcesses.Length,
                                        "wrong count on normal " + newProcesses.Length
                                    );
                                } else {
                                    Assert.AreEqual(
                                        waitOnAbnormal ? 1 : 0,
                                        newProcesses.Length,
                                        "wrong count on abnormal exit" + newProcesses.Length
                                    );
                                }

                                if (debug) {
                                    solution.WaitForMode(dbgDebugMode.dbgDesignMode);
                                }

                                foreach (var proc in newProcesses) {
                                    System.Diagnostics.Process.GetProcessById(proc).Kill();
                                }

                                File.Delete(filename);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Verifies that we can launch node.exe in a way where debugging doesn't
        /// start (in this case -v is passed to display the version).  VS shouldn't crash.
        /// </summary>
        [Ignore]
        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Debugging UI")]
        [HostType("VSTestHost")]
        public void TestNoDebugging() {
            var project = Project("NoDebugging",
                Compile("server"),
                Property(CommonConstants.StartupFile, "server.js"),
                Property(NodejsConstants.NodeExeArguments, "-v")
            );

            using (var solution = project.Generate().ToVs()) {
                bool waitOnAbnormal, waitOnNormal;
                waitOnAbnormal = NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit;
                waitOnNormal = NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit;
                try {
                    NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit = true;
                    NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit = true;

                    var beginningProcesses = System.Diagnostics.Process.GetProcessesByName(
                        "Microsoft.NodejsTools.PressAnyKey"
                    ).Select(x => x.Id);

                    solution.ExecuteCommand("Debug.Start");

                    bool foundNewProcesses = false;
                    for (int i = 0; i < 10; i++) {
                        var currentProcesses = System.Diagnostics.Process.GetProcessesByName(
                            "Microsoft.NodejsTools.PressAnyKey"
                        ).Select(x => x.Id);
                        var newProcesses = currentProcesses.Except(beginningProcesses).ToArray();
                        if (newProcesses.Length > 0) {
                            // we shouldn't have gotten into debug mode
                            Assert.AreEqual(
                                VSTestContext.DTE.Debugger.CurrentMode,
                                dbgDebugMode.dbgDesignMode
                            );

                            Assert.AreEqual(1, newProcesses.Length);
                            foreach (var proc in newProcesses) {
                                System.Diagnostics.Process.GetProcessById(proc).Kill();
                            }

                            foundNewProcesses = true;
                            break;
                        }
                        System.Threading.Thread.Sleep(2000);
                    }

                    Assert.IsTrue(foundNewProcesses, "failed to find new processes");
                } finally {
                    NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit = waitOnAbnormal;
                    NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit = waitOnNormal;
                }
            }
        }
    }
}

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

using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudioTools;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class DebuggerUITests : NodejsProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestWaitOnExit() {
            foreach (var debug in new[] { false, true }) {
                foreach (var exitCode in new[] { 0, 1 }) {
                    foreach (var waitOnAbnormal in new[] { true, false }) {
                        foreach (var waitOnNormal in new[] { true, false }) {
                            var filename = Path.Combine(TestData.GetTempPath(), Path.GetRandomFileName());

                            var project = Project("WaitOnExit",

                                Compile("server", @"
require('fs').writeFileSync('" + filename.Replace("\\", "\\\\") + @"', 'started')
process.exit(" + exitCode + ");"),
                                Property(CommonConstants.StartupFile, "server.js")
                            );

                            using (var solution = project.Generate().ToVs()) {
                                var waitingOnProcess = (waitOnAbnormal || waitOnNormal) ? "cmd" : "node";
                                var beginningProcesses = System.Diagnostics.Process.GetProcessesByName(
                                    waitingOnProcess
                                ).Select(x => x.Id);

                                NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit = waitOnAbnormal;
                                NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit = waitOnNormal;

                                if (debug) {
                                    solution.App.Dte.ExecuteCommand("Debug.Start");
                                } else {
                                    solution.App.Dte.ExecuteCommand("Debug.StartWithoutDebugging");
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
                                    solution.App.WaitForMode(dbgDebugMode.dbgDesignMode);
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
    }
}

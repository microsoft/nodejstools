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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using EnvDTE;
using EnvDTE90;
using EnvDTE90a;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Profiling;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.UI;
using TestUtilities.UI.Nodejs;
using Task = System.Threading.Tasks.Task;

namespace ProfilingUITests {
    [TestClass]
    public class ProfilingTests {
        public const string NodejsProfileTest = "TestData\\NodejsProfileTest\\NodejsProfileTest.sln";
        public const string NodejsTypeScriptProfileTest = "TestData\\NodejsTypeScriptProfileTest\\NodejsProfileTest.sln";
        public const string NodejsTypeScriptProfileTestNeedsBuild = "TestData\\NodejsTypeScriptProfileTestNeedsBuild\\NodejsProfileTest.sln";
        public const string NodejsTypeScriptProfileTestWithErrors = "TestData\\NodejsTypeScriptProfileTestWithErrors\\NodejsProfileTest.sln";

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        private static INodeProfileSession LaunchSession(
            VisualStudioApp app,
            Func<INodeProfileSession> creator,
            string saveDirectory
        ) {
            INodeProfileSession session = null;
            var task = Task.Factory.StartNew(() => {
                session = creator();
                // Must fault the task to abort the wait
                throw new Exception();
            });
            var dialog = app.WaitForDialog(task);
            if (dialog != IntPtr.Zero) {
                SavePerfFile(saveDirectory, app, dialog);
                try {
                    task.Wait(TimeSpan.FromSeconds(5.0));
                    Assert.Fail("Task did not fault");
                } catch (AggregateException) {
                }
            }
            Assert.IsNotNull(session, "Session was not correctly initialized");
            return session;
        }

        private static string SavePerfFile(string saveDirectory, VisualStudioApp app, IntPtr? dialog = null) {
            string destName;
            using (var saveDialog = new SaveDialog(app, AutomationElement.FromHandle(dialog ?? app.WaitForDialog()))) {
                var originalDestName = Path.Combine(saveDirectory, Path.GetFileName(saveDialog.FileName));
                destName = originalDestName;

                while (File.Exists(destName)) {
                    destName = string.Format("{0} {1}{2}",
                        Path.GetFileNameWithoutExtension(originalDestName),
                        Guid.NewGuid(),
                        Path.GetExtension(originalDestName)
                    );
                }

                saveDialog.FileName = destName;
                saveDialog.Save();
            }
            return destName;
        }

        private static INodeProfileSession LaunchProcess(
            VisualStudioApp app,
            INodeProfiling profiling,
            string interpreterPath,
            string filename,
            string directory,
            string arguments,
            bool openReport
        ) {
            return LaunchSession(app,
                () => profiling.LaunchProcess(
                    interpreterPath,
                    filename,
                    directory,
                    String.Empty,
                    openReport
                ),
                directory
            );
        }

        private static INodeProfileSession LaunchProject(
            VisualStudioApp app,
            INodeProfiling profiling,
            EnvDTE.Project project,
            string directory,
            bool openReport
        ) {
            return LaunchSession(app, () => profiling.LaunchProject(project, openReport), directory);
        }

        private NodejsVisualStudioApp OpenProfileTestProject(
            out EnvDTE.Project project,
            out INodeProfiling profiling,
            string projectFile = NodejsProfileTest
        ) {
            var app = new NodejsVisualStudioApp();
            try {
                profiling = (INodeProfiling)app.Dte.GetObject("NodejsProfiling");

                // no sessions yet
                Assert.IsNull(profiling.GetSession(1));

                if (string.IsNullOrEmpty(projectFile)) {
                    project = null;
                } else {
                    project = app.OpenProject(projectFile);
                }

                var res = app;
                app = null;
                return res;
            } finally {
                if (app != null) {
                    app.Dispose();
                }
            }
        }

        private static void WaitForReport(INodeProfiling profiling, INodeProfileSession session, out INodePerformanceReport report, NodejsVisualStudioApp app, out AutomationElement child) {
            report = WaitForReportIndex(session, 1);
            Assert.IsTrue(report.Filename.Contains("NodejsProfileTest"));

            app.OpenNodejsPerformance();
            var pyPerf = app.NodejsPerformanceExplorerTreeView;
            Assert.AreNotEqual(null, pyPerf);

            var item = pyPerf.FindItem("NodejsProfileTest *", "Reports");
            child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);
            var childName = child.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;
            Assert.IsTrue(childName.StartsWith("NodejsProfileTest"));

            AutomationWrapper.EnsureExpanded(child);
        }

        private static INodePerformanceReport WaitForReportIndex(INodeProfileSession session, int index) {
            var report = session.GetReport(index);
            for (int trial = 0; trial < 20 && report == null; ++trial) {
                System.Threading.Thread.Sleep(500);
                report = session.GetReport(index);
            }
            WaitForFileExistenceOnDisk(report.Filename);
            return report;
        }

        private static void WaitForFileExistenceOnDisk(string filename) {
            for (int trial = 0; trial < 20; ++trial) {
                if (!File.Exists(filename)) {
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        private static void WaitForFileNonExistenceOnDisk(string filename) {
            for (int trial = 0; trial < 20; ++trial) {
                if (File.Exists(filename)) {
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void NewProfilingSession() {
            using (new JustMyCodeSetting(false)) {
                using (var app = new NodejsVisualStudioApp()) {
                    app.Dte.Solution.Close(false);

                    app.OpenNodejsPerformance();
                    app.NodejsPerformanceExplorerToolBar.NewPerfSession();

                    var profiling = (INodeProfiling)app.Dte.GetObject("NodejsProfiling");

                    var perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");
                    Debug.Assert(perf != null);
                    var session = profiling.GetSession(1);
                    Assert.AreNotEqual(session, null);

                    NodejsPerfTarget perfTarget = null;
                    try {
                        Mouse.MoveTo(perf.GetClickablePoint());
                        Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                        // wait for the dialog, set some settings, save them.
                        perfTarget = new NodejsPerfTarget(app.WaitForDialog());

                        perfTarget.SelectProfileScript();
                        perfTarget.InterpreterPath = NodeExePath;
                        perfTarget.ScriptName = TestData.GetPath(@"TestData\NodejsProfileTest\program.js");

                        try {
                            perfTarget.Ok();
                            perfTarget = null;
                        } catch (ElementNotEnabledException) {
                            Assert.Fail("Settings were invalid:\n  ScriptName = {0}\n",
                                perfTarget.ScriptName);
                        }
                        app.WaitForDialogDismissed();

                        Mouse.MoveTo(perf.GetClickablePoint());
                        Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                        // re-open the dialog, verify the settings
                        perfTarget = new NodejsPerfTarget(app.WaitForDialog());

                        //Assert.AreEqual("Python 2.6", perfTarget.SelectedInterpreter);
                        Assert.AreEqual(TestData.GetPath(@"TestData\NodejsProfileTest\program.js"), perfTarget.ScriptName);

                    } finally {
                        if (perfTarget != null) {
                            perfTarget.Cancel();
                            app.WaitForDialogDismissed();
                        }
                        profiling.RemoveSession(session, true);
                    }
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/26
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestStartAnalysisDebugMenu() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                app.Dte.ExecuteCommand("Debug.StartNode.jsPerformanceAnalysis");
                while (profiling.GetSession(1) == null) {
                    System.Threading.Thread.Sleep(100);
                }
                var session = profiling.GetSession(1);
                try {
                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(500);
                    }

                    var report = session.GetReport(1);
                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.f");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/26
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestStartAnalysisDebugMenuNoProject() {
            using (new JustMyCodeSetting(false))
            using (var app = new NodejsVisualStudioApp()) {
                bool ok = true;
                try {
                    app.Dte.ExecuteCommand("Debug.StartNode.jsPerformanceAnalysis");
                    ok = false;
                } catch {
                }
                Assert.IsTrue(ok, "Could start perf analysis w/o a project open");
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/149
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void LaunchNewProfilingSession() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {

                app.OpenNodejsPerformance();
                app.NodejsPerformanceExplorerToolBar.NewPerfSession();


                var perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");
                Debug.Assert(perf != null);
                var session = profiling.GetSession(1);
                Assert.AreNotEqual(session, null);

                NodejsPerfTarget perfTarget = null;
                string savedFile = null;
                try {
                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    // wait for the dialog, set some settings, save them.
                    perfTarget = new NodejsPerfTarget(app.WaitForDialog());

                    perfTarget.SelectProfileScript();
                    perfTarget.InterpreterPath = NodeExePath;
                    perfTarget.ScriptName = TestData.GetPath(@"TestData\NodejsProfileTest\program.js");
                    string a = perfTarget.ScriptName;

                    try {
                        perfTarget.Ok();
                        perfTarget = null;
                    } catch (ElementNotEnabledException) {
                        Assert.Fail("Settings were invalid:\n  ScriptName = {0}\n",
                            perfTarget.ScriptName);
                    }
                    app.WaitForDialogDismissed();

                    perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");
                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.Click(System.Windows.Input.MouseButton.Right);
                    Keyboard.Type("S");
                    savedFile = SavePerfFile(TestData.GetPath(@"TestData\NodejsProfileTest"), app);

                    var item = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *", "Reports");
                    AutomationElement child = null;
                    for (int i = 0; i < 20; i++) {
                        child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);
                        if (child != null) {
                            break;
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                    Assert.IsNotNull(child, "node not added");
                    System.Threading.Thread.Sleep(7500);    // give time for report to open
                } finally {
                    if (perfTarget != null) {
                        perfTarget.Cancel();
                        app.WaitForDialogDismissed();
                    }
                    if (!String.IsNullOrEmpty(savedFile)) {
                        File.Delete(savedFile);
                    }
                    profiling.RemoveSession(session, true);
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/145
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestMultipleSessions() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                app.OpenNodejsPerformance();
                app.NodejsPerformanceExplorerToolBar.NewPerfSession();

                var perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");
                Debug.Assert(perf != null);

                app.NodejsPerformanceExplorerToolBar.NewPerfSession();
                perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance1 *");
                Debug.Assert(perf != null);
                var session = profiling.GetSession(1);
                Assert.AreNotEqual(session, null);

                NodejsPerfTarget perfTarget = null;
                try {
                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    // wait for the dialog, set some settings, save them.
                    perfTarget = new NodejsPerfTarget(app.WaitForDialog());

                    perfTarget.SelectProfileScript();
                    perfTarget.InterpreterPath = NodeExePath;
                    perfTarget.ScriptName = TestData.GetPath(@"TestData\NodejsProfileTest\program.js");

                    try {
                        perfTarget.Ok();
                        perfTarget = null;
                    } catch (ElementNotEnabledException) {
                        Assert.Fail("Settings were invalid:\n  ScriptName = {0}\n",
                            perfTarget.ScriptName);
                    }
                    app.WaitForDialogDismissed();

                    perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance1 *");
                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.Click(System.Windows.Input.MouseButton.Right);
                    Keyboard.Type("S");
                    SavePerfFile(TestData.GetPath(@"TestData\NodejsProfileTest"), app);
                    SavePerfFile(TestData.GetPath(@"TestData\NodejsProfileTest"), app);

                    var item = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance1 *", "Reports");
                    AutomationElement child = null;
                    for (int i = 0; i < 20; i++) {
                        child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);
                        if (child != null) {
                            break;
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                    Assert.IsNotNull(child, "node not added");
                } finally {
                    if (perfTarget != null) {
                        perfTarget.Cancel();
                        app.WaitForDialogDismissed();
                    }
                    profiling.RemoveSession(session, true);
                    profiling.RemoveSession(profiling.GetSession(1), true);
                }
            }
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/145
        /// 
        /// Same as TestMultipleSessions, but reversing the order of which one we launch from
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestMultipleSessions2() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                app.OpenNodejsPerformance();
                app.NodejsPerformanceExplorerToolBar.NewPerfSession();

                var perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");
                Debug.Assert(perf != null);

                app.NodejsPerformanceExplorerToolBar.NewPerfSession();
                perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");
                Debug.Assert(perf != null);
                var session = profiling.GetSession(1);
                Assert.AreNotEqual(session, null);

                NodejsPerfTarget perfTarget = null;
                try {
                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    // wait for the dialog, set some settings, save them.
                    perfTarget = new NodejsPerfTarget(app.WaitForDialog());

                    perfTarget.SelectProfileScript();
                    perfTarget.InterpreterPath = NodeExePath;
                    perfTarget.ScriptName = TestData.GetPath(@"TestData\NodejsProfileTest\program.js");

                    try {
                        perfTarget.Ok();
                        perfTarget = null;
                    } catch (ElementNotEnabledException) {
                        Assert.Fail("Settings were invalid:\n  ScriptName = {0}\n",
                            perfTarget.ScriptName);
                    }
                    app.WaitForDialogDismissed();

                    perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");
                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.Click(System.Windows.Input.MouseButton.Right);
                    Keyboard.Type("S");
                    SavePerfFile(TestData.GetPath(@"TestData\NodejsProfileTest"), app);
                    SavePerfFile(TestData.GetPath(@"TestData\NodejsProfileTest"), app);

                    var item = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *", "Reports");
                    AutomationElement child = null;
                    for (int i = 0; i < 20; i++) {
                        child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);
                        if (child != null) {
                            break;
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                    Assert.IsNotNull(child, "node not added");
                } finally {
                    if (perfTarget != null) {
                        perfTarget.Cancel();
                        app.WaitForDialogDismissed();
                    }
                    profiling.RemoveSession(session, true);
                    profiling.RemoveSession(profiling.GetSession(1), true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void NewProfilingSessionOpenSolution() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                app.OpenNodejsPerformance();
                app.NodejsPerformanceExplorerToolBar.NewPerfSession();

                var perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance");
                if (perf == null) {
                    var tmpSession = profiling.GetSession(1);
                    if (tmpSession != null) {
                        profiling.RemoveSession(tmpSession, true);
                    }
                    Debug.Fail("failed to find performance session, found " + tmpSession != null ? tmpSession.Name : "<nothing>");
                }

                var session = profiling.GetSession(1);
                Assert.AreNotEqual(session, null);

                NodejsPerfTarget perfTarget = null;
                try {
                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    // wait for the dialog, set some settings, save them.
                    perfTarget = new NodejsPerfTarget(app.WaitForDialog());

                    perfTarget.SelectProfileProject();

                    perfTarget.SelectedProjectComboBox.SelectItem("NodejsProfileTest");

                    try {
                        perfTarget.Ok();
                        perfTarget = null;
                    } catch (ElementNotEnabledException) {
                        Assert.Fail("Settings were invalid:\n  SelectedProject = {0}",
                            perfTarget.SelectedProjectComboBox.GetSelectedItemName());
                    }
                    app.WaitForDialogDismissed();

                    Mouse.MoveTo(perf.GetClickablePoint());
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    // re-open the dialog, verify the settings
                    perfTarget = new NodejsPerfTarget(app.WaitForDialog());

                    Assert.AreEqual("NodejsProfileTest", perfTarget.SelectedProject);
                } finally {
                    if (perfTarget != null) {
                        perfTarget.Cancel();
                        app.WaitForDialogDismissed();
                    }
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void LaunchNodejsProfilingWizard() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                app.LaunchNodejsProfiling();

                // wait for the dialog, set some settings, save them.
                var perfTarget = new NodejsPerfTarget(app.WaitForDialog());
                try {
                    perfTarget.SelectProfileProject();

                    perfTarget.SelectedProjectComboBox.SelectItem("NodejsProfileTest");

                    try {
                        perfTarget.Ok();
                        perfTarget = null;
                    } catch (ElementNotEnabledException) {
                        Assert.Fail("Settings were invalid:\n  SelectedProject = {0}",
                            perfTarget.SelectedProjectComboBox.GetSelectedItemName());
                    }
                } finally {
                    if (perfTarget != null) {
                        perfTarget.Cancel();
                        app.WaitForDialogDismissed();
                        perfTarget = null;
                    }
                }
                app.WaitForDialogDismissed();

                var session = profiling.GetSession(1);

                try {
                    Assert.AreNotEqual(null, app.NodejsPerformanceExplorerTreeView.WaitForItem("NodejsProfileTest *"));

                    while (profiling.IsProfiling) {
                        // wait for profiling to finish...
                        System.Threading.Thread.Sleep(500);
                    }
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void LaunchProject() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    var report = WaitForReportIndex(session, 1);

                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.f");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void LaunchMappedProject() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, NodejsTypeScriptProfileTest)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsTypeScriptProfileTest"), false);
                try {
                    var report = WaitForReportIndex(session, 1);

                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.Greeter.f");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void LaunchMappedProjectNeedsBuild() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, NodejsTypeScriptProfileTestNeedsBuild)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsTypeScriptProfileTest"), false);
                try {
                    var report = WaitForReportIndex(session, 1);

                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.Greeter.f");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void LaunchMappedProjectNeedsBuildWithErrors() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, NodejsTypeScriptProfileTestWithErrors)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsTypeScriptProfileTest"), false);
                try {
                    app.WaitForDialog();
                    VisualStudioApp.CheckMessageBox(MessageBoxButton.No, "Failed to build project, do you want to launch profiling anyway?");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestSaveDirtySession() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(500);
                    }

                    var report = session.GetReport(1);
                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    app.OpenNodejsPerformance();
                    var pyPerf = app.NodejsPerformanceExplorerTreeView;
                    Assert.AreNotEqual(null, pyPerf);

                    var item = pyPerf.FindItem("NodejsProfileTest *", "Reports");
                    var child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);
                    var childName = child.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;

                    Assert.IsTrue(childName.StartsWith("NodejsProfileTest"));

                    // select the dirty session node and save it
                    var perfSessionItem = pyPerf.FindItem("NodejsProfileTest *");
                    perfSessionItem.SetFocus();
                    app.SaveSelection();

                    // now it should no longer be dirty
                    perfSessionItem = pyPerf.WaitForItem("NodejsProfileTest");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestDeleteReport() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    INodePerformanceReport report;
                    AutomationElement child;
                    WaitForReport(profiling, session, out report, app, out child);

                    child.SetFocus();
                    Keyboard.PressAndRelease(System.Windows.Input.Key.Delete);

                    new RemoveItemDialog(app.WaitForDialog()).Delete();
                    app.WaitForDialogDismissed();

                    Assert.IsTrue(!File.Exists(report.Filename)); // Delete permanently
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestCompareReports() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    WaitForReportIndex(session, 1);

                    session.Launch(false);
                    WaitForReportIndex(session, 2);

                    var pyPerf = app.NodejsPerformanceExplorerTreeView;

                    var child = pyPerf.WaitForItem(
                        "NodejsProfileTest *",
                        "Reports",
                        Path.GetFileNameWithoutExtension(profiling.GetSession(1).GetReport(1).Filename)
                    );

                    child.SetFocus();

                    Keyboard.PressAndRelease(System.Windows.Input.Key.Apps);
                    Keyboard.PressAndRelease(System.Windows.Input.Key.C);

                    var cmpReports = new ComparePerfReports(app.WaitForDialog());
                    cmpReports.ComparisonFile = session.GetReport(2).Filename;
                    try {
                        cmpReports.Ok();
                        cmpReports = null;
                    } catch (ElementNotEnabledException) {
                        Assert.Fail("Settings were invalid:\n  BaselineFile = {0}\n  ComparisonFile = {1}",
                            cmpReports.BaselineFile, cmpReports.ComparisonFile);
                    } finally {
                        if (cmpReports != null) {
                            cmpReports.Cancel();
                        }
                    }

                    app.WaitForDialogDismissed();

                    // verify the difference file opens....
                    bool foundDiff = false;
                    for (int j = 0; j < 100 && !foundDiff; j++) {
                        for (int i = 0; i < app.Dte.Documents.Count; i++) {
                            var doc = app.Dte.Documents.Item(i + 1);
                            string name = doc.FullName;

                            if (name.StartsWith("vsp://diff/?baseline=")) {
                                foundDiff = true;
                                System.Threading.Thread.Sleep(5000);    // let the file get opened and processed
                                ThreadPool.QueueUserWorkItem(x => doc.Close(EnvDTE.vsSaveChanges.vsSaveChangesNo));
                                break;
                            }
                        }
                        if (!foundDiff) {
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                    Assert.IsTrue(foundDiff);
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestRemoveReport() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    INodePerformanceReport report;
                    AutomationElement child;
                    WaitForReport(profiling, session, out report, app, out child);

                    child.SetFocus();
                    Keyboard.PressAndRelease(System.Windows.Input.Key.Delete);

                    new RemoveItemDialog(app.WaitForDialog()).Remove();
                    app.WaitForDialogDismissed();

                    Assert.IsTrue(File.Exists(report.Filename)); // Removed but not deleted
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestOpenReport() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    INodePerformanceReport report;
                    AutomationElement child;
                    WaitForReport(profiling, session, out report, app, out child);

                    var clickPoint = child.GetClickablePoint();
                    Mouse.MoveTo(clickPoint);
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    Assert.AreNotEqual(null, app.WaitForDocument(report.Filename));

                    app.Dte.Documents.CloseAll(EnvDTE.vsSaveChanges.vsSaveChangesNo);
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestOpenReportCtxMenu() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    INodePerformanceReport report;
                    AutomationElement child;
                    WaitForReport(profiling, session, out report, app, out child);

                    var clickPoint = child.GetClickablePoint();
                    Mouse.MoveTo(clickPoint);
                    Mouse.Click(System.Windows.Input.MouseButton.Right);
                    Keyboard.Press(System.Windows.Input.Key.O);

                    Assert.AreNotEqual(null, app.WaitForDocument(report.Filename));
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestTargetPropertiesForProject() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    var report = WaitForReportIndex(session, 1);

                    app.OpenNodejsPerformance();
                    var pyPerf = app.NodejsPerformanceExplorerTreeView;

                    var item = pyPerf.FindItem("NodejsProfileTest *");

                    Mouse.MoveTo(item.GetClickablePoint());
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    var perfTarget = new NodejsPerfTarget(app.WaitForDialog());
                    Assert.AreEqual("NodejsProfileTest", perfTarget.SelectedProject);

                    perfTarget.Cancel();

                    app.WaitForDialogDismissed();
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestTargetPropertiesForExecutable() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                var session = LaunchProcess(app,
                    profiling,
                    NodeExePath,
                    TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                    TestData.GetPath(@"TestData\NodejsProfileTest"),
                    String.Empty,
                    false
                );

                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(500);
                }
                NodejsPerfTarget perfTarget = null;
                try {
                    app.OpenNodejsPerformance();
                    var pyPerf = app.NodejsPerformanceExplorerTreeView;

                    var item = pyPerf.FindItem("program *");

                    Mouse.MoveTo(item.GetClickablePoint());
                    Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                    perfTarget = new NodejsPerfTarget(app.WaitForDialog());
                    Assert.AreEqual(NodeExePath, perfTarget.InterpreterPath);
                    Assert.AreEqual("", perfTarget.Arguments);
                    Assert.IsTrue(perfTarget.ScriptName.EndsWith("program.js"));
                    Assert.IsTrue(perfTarget.ScriptName.StartsWith(perfTarget.WorkingDir));
                } finally {
                    if (perfTarget != null) {
                        perfTarget.Cancel();
                        app.WaitForDialogDismissed();
                    }
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestStopProfiling() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                var session = LaunchProcess(app,
                    profiling,
                    NodeExePath,
                    TestData.GetPath(@"TestData\NodejsProfileTest\infiniteProfile.js"),
                    TestData.GetPath(@"TestData\NodejsProfileTest"),
                    String.Empty,
                    false
                );

                try {
                    System.Threading.Thread.Sleep(1000);
                    Assert.IsTrue(profiling.IsProfiling);
                    app.OpenNodejsPerformance();
                    app.NodejsPerformanceExplorerToolBar.StopProfiling();

                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(100);
                    }

                    var report = session.GetReport(1);

                    Assert.AreNotEqual(null, report);
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("VSTestHost")]
        public void TestTwoProfilesAtTheSameTime() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                var session = LaunchProcess(app,
                    profiling,
                    NodeExePath,
                    TestData.GetPath(@"TestData\NodejsProfileTest\infiniteProfile.js"),
                    TestData.GetPath(@"TestData\NodejsProfileTest"),
                    String.Empty,
                    false
                );

                try {
                    // TODO: Review this test.
                    // This does not seem to a correct way to check the profiling process is up
                    // and running. If the profiling process runs too fast, it may completes
                    // before we even reach this line.
                    for (int i = 0; i < 100 && !profiling.IsProfiling; i++) {
                        System.Threading.Thread.Sleep(100);
                    }
                    Assert.IsTrue(profiling.IsProfiling);
                    app.OpenNodejsPerformance();

                    try {
                        app.Dte.ExecuteCommand("Analyze.LaunchNode.jsProfiling");
                        Assert.Fail();
                    } catch (COMException) {
                    }
                    app.NodejsPerformanceExplorerToolBar.StopProfiling();

                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(100);
                    }
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void MultipleTargets() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                INodeProfileSession session2 = null;
                try {
                    {
                        var report = WaitForReportIndex(session, 1);
                        var filename = report.Filename;
                        Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                        Assert.AreEqual(session.GetReport(2), null);

                        Assert.AreNotEqual(session.GetReport(report.Filename), null);

                        VerifyReport(report, "program.f");
                    }

                    {
                        session2 = LaunchProcess(app,
                            profiling,
                            NodeExePath,
                                    TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                                    TestData.GetPath(@"TestData\NodejsProfileTest"),
                                    String.Empty,
                                    false
                                );

                        var report = WaitForReportIndex(session2, 1);
                        var filename = report.Filename;
                        Assert.IsTrue(filename.Contains("program"));

                        Assert.AreEqual(session2.GetReport(2), null);

                        Assert.AreNotEqual(session2.GetReport(report.Filename), null);

                        VerifyReport(report, "program.f");
                    }
                } finally {
                    profiling.RemoveSession(session, true);
                    if (session2 != null) {
                        profiling.RemoveSession(session2, true);
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void MultipleTargetsWithProjectHome() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                INodeProfileSession session2 = null;
                try {
                    {
                        var report = WaitForReportIndex(session, 1);
                        var filename = report.Filename;
                        Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                        Assert.AreEqual(session.GetReport(2), null);

                        Assert.AreNotEqual(session.GetReport(report.Filename), null);

                        VerifyReport(report, "program.f");
                    }

                    {
                        session2 = LaunchProcess(app,
                            profiling,
                            NodeExePath,
                            TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                            TestData.GetPath(@"TestData\NodejsProfileTest"),
                            String.Empty,
                            false
                        );

                        var report = WaitForReportIndex(session2, 1);
                        var filename = report.Filename;
                        Assert.IsTrue(filename.Contains("program"));

                        Assert.AreEqual(session2.GetReport(2), null);

                        Assert.AreNotEqual(session2.GetReport(report.Filename), null);

                        VerifyReport(report, "program.f");
                    }

                } finally {
                    profiling.RemoveSession(session, true);
                    if (session2 != null) {
                        profiling.RemoveSession(session2, true);
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void MultipleReports() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling)) {
                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    var report = WaitForReportIndex(session, 1);
                    while (report == null) {
                        System.Threading.Thread.Sleep(500);
                        report = session.GetReport(1);
                    }

                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.f");

                    session.Launch();

                    report = WaitForReportIndex(session, 1);
                    VerifyReport(report, "program.f");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }


        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void LaunchExecutable() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                var session = LaunchProcess(app,
                    profiling,
                    NodeExePath,
                    TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                    TestData.GetPath(@"TestData\NodejsProfileTest"),
                    String.Empty,
                    false
                );
                try {
                    var report = WaitForReportIndex(session, 1);

                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("program"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.f");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestJustMyCode() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(true))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                var session = LaunchProcess(app,
                    profiling,
                    NodeExePath,
                    TestData.GetPath(@"TestData\NodejsProfileTest\JustMyCode.js"),
                    TestData.GetPath(@"TestData\NodejsProfileTest"),
                    String.Empty,
                    false
                );
                try {
                    var report = WaitForReportIndex(session, 1);
                    var filename = report.Filename;

                    Assert.AreEqual(session.GetReport(2), null);

                    VerifyReportMissing(report, "fs.fs.writeFileSync");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestJustMyCodeOff() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                var session = LaunchProcess(app,
                    profiling,
                    NodeExePath,
                    TestData.GetPath(@"TestData\NodejsProfileTest\JustMyCode.js"),
                    TestData.GetPath(@"TestData\NodejsProfileTest"),
                    String.Empty,
                    false
                );
                try {
                    var report = WaitForReportIndex(session, 1);

                    Assert.AreEqual(session.GetReport(2), null);

                    VerifyReport(report, "fs.fs.writeFileSync");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestProjectProperties() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {
                var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
                if (File.Exists(testFile)) {
                    File.Delete(testFile);
                }

                project = app.OpenProject(@"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server.js");
                project.Properties.Item("WorkingDirectory").Value = Path.GetTempPath();

                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    WaitForFileExistenceOnDisk(testFile);

                    Assert.IsTrue(File.Exists(testFile), "test file not created");
                    var lines = File.ReadAllLines(testFile);

                    Assert.IsTrue(lines[0].Contains("scriptargs"), "no scriptargs");
                    Assert.IsTrue(lines[0].Contains("server.js"), "missing filename");
                    Assert.IsFalse(lines[0].Contains("--harmony"), "interpreter argument leaked to script");
                    Assert.IsTrue(lines[1].Contains("--harmony"), "missing interpreter argument");
                    Assert.AreEqual("port: 1234", lines[2]);
                    Assert.AreEqual("cwd: " + Path.GetTempPath().Substring(0, Path.GetTempPath().Length - 1), lines[3]);
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void TestBrowserLaunch() {
            EnvDTE.Project project;
            INodeProfiling profiling;
            using (new JustMyCodeSetting(false))
            using (var app = OpenProfileTestProject(out project, out profiling, null)) {

                var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
                if (File.Exists(testFile)) {
                    File.Delete(testFile);
                }

                project = app.OpenProject(@"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server2.js");
                project.Properties.Item("WorkingDirectory").Value = Path.GetTempPath();

                var session = LaunchProject(app, profiling, project, TestData.GetPath("TestData\\NodejsProfileTest"), false);
                try {
                    WaitForFileExistenceOnDisk(testFile);

                    Assert.IsTrue(File.Exists(testFile), "test file not created");
                } finally {
                    profiling.RemoveSession(session, true);
                }
            }
        }

        private static void VerifyReport(INodePerformanceReport report, params string[] expectedFunctions) {
            bool[] expected = FindFunctions(report, expectedFunctions);

            foreach (var found in expected) {
                Assert.IsTrue(found);
            }
        }

        private static void VerifyReportMissing(INodePerformanceReport report, params string[] missingFunctions) {
            bool[] expected = FindFunctions(report, missingFunctions);

            foreach (var found in expected) {
                Assert.IsFalse(found);
            }
        }

        private static bool[] FindFunctions(INodePerformanceReport report, string[] expectedFunctions) {
            // run vsperf
            string[] lines = OpenPerformanceReportAsCsv(report);
            bool[] expected = new bool[expectedFunctions.Length];
            string[] altExpected = new string[expectedFunctions.Length];
            for (int i = 0; i < expectedFunctions.Length; i++) {
                altExpected[i] = expectedFunctions[i] + " (recompiled)";
            }

            // quote the function names so they match the CSV
            for (int i = 0; i < expectedFunctions.Length; i++) {
                expectedFunctions[i] = "\"" + expectedFunctions[i] + "\"";
                altExpected[i] = "\"" + altExpected[i] + "\"";
            }

            foreach (var line in lines) {
                Console.WriteLine(line);
                for (int i = 0; i < expectedFunctions.Length; i++) {
                    if (line.StartsWith(expectedFunctions[i]) || line.StartsWith(altExpected[i])) {
                        expected[i] = true;
                    }
                }
            }
            return expected;
        }

        private static int _counter;

        private static string[] OpenPerformanceReportAsCsv(INodePerformanceReport report) {
            var perfReportPath = Path.Combine(GetPerfToolsPath(false), "vsperfreport.exe");

            for (int i = 0; i < 11; i++) {
                string csvFilename;
                do {
                    csvFilename = Path.Combine(Path.GetTempPath(), "test") + DateTime.Now.Ticks + "_" + _counter++;
                } while (File.Exists(csvFilename + "_FunctionSummary.csv"));

                var psi = new ProcessStartInfo(perfReportPath, "\"" + report.Filename + "\"" + " /output:" + csvFilename + " /summary:function");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                var process = System.Diagnostics.Process.Start(psi);
                var output = new StringBuilder();
                process.OutputDataReceived += (sender, args) => {
                    output.Append(args.Data);
                };
                process.ErrorDataReceived += (sender, args) => {
                    output.Append(args.Data);
                };
                process.WaitForExit();
                if (process.ExitCode != 0) {
                    if (i == 10) {
                        string msg = "Output: " + process.StandardOutput.ReadToEnd() + Environment.NewLine +
                            "Error: " + process.StandardError.ReadToEnd() + Environment.NewLine;
                        Assert.Fail(msg);
                    } else {
                        Console.WriteLine("Failed to convert: {0}", output.ToString());
                        Console.WriteLine("--------------");
                        System.Threading.Thread.Sleep(1000);
                        continue;
                    }
                }

                string[] res = null;
                for (int j = 0; j < 100; j++) {
                    try {
                        res = File.ReadAllLines(csvFilename + "_FunctionSummary.csv");
                        break;
                    } catch {
                        System.Threading.Thread.Sleep(100);
                    }
                }
                File.Delete(csvFilename + "_FunctionSummary.csv");
                return res ?? new string[0];
            }
            Assert.Fail("Unable to convert to CSV");
            return null;
        }

        private static string GetPerfToolsPath(bool x64) {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\" + AssemblyVersionInfo.VSVersion);
            var shFolder = key.GetValue("ShellFolder") as string;
            if (shFolder == null) {
                throw new InvalidOperationException("Cannot find shell folder for Visual Studio");
            }

            string perfToolsPath;
            if (x64) {
                perfToolsPath = @"Team Tools\Performance Tools\x64";
            } else {
                perfToolsPath = @"Team Tools\Performance Tools\";
            }
            perfToolsPath = Path.Combine(shFolder, perfToolsPath);
            return perfToolsPath;
        }

        public string NodeExePath {
            get {
                Assert.IsNotNull(Nodejs.NodeExePath, "Node isn't installed");
                return Nodejs.NodeExePath;
            }
        }

        class JustMyCodeSetting : IDisposable {
            private readonly bool _initialState;

            public JustMyCodeSetting(bool enabled) {
                using (var vsperfKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).OpenSubKey("VSPerf", true)) {
                    var value = vsperfKey.GetValue("tools.options.justmycode");
                    int jmcSetting;
                    if (value != null && value is string && Int32.TryParse((string)value, out jmcSetting)) {
                        _initialState = jmcSetting != 0;
                    }
                    vsperfKey.SetValue("tools.options.justmycode", enabled ? "1" : "0");
                }

            }

            public void Dispose() {
                using (var vsperfKey = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings, true).OpenSubKey("VSPerf", true)) {
                    vsperfKey.SetValue("tools.options.justmycode", _initialState ? "1" : "0");
                }
            }
        }
    }
}

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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using EnvDTE;
using EnvDTE90;
using EnvDTE90a;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Profiling;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TestUtilities;
using TestUtilities.UI;
using TestUtilities.UI.Nodejs;

namespace ProfilingUITests {
    [TestClass]
    public class ProfilingTests {
        public const string NodejsProfileTest = "TestData\\NodejsProfileTest\\NodejsProfileTest.sln";

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            TestData.Deploy();
        }

        [TestCleanup]
        public void MyTestCleanup() {
            for (int i = 0; i < 100; i++) {
                try {
                    VsIdeTestHostContext.Dte.Solution.Close(false);
                    break;
                } catch {
                    VsIdeTestHostContext.Dte.Documents.CloseAll(EnvDTE.vsSaveChanges.vsSaveChangesNo);
                    System.Threading.Thread.Sleep(200);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void NewProfilingSession() {
            VsIdeTestHostContext.Dte.Solution.Close(false);

            var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);

            app.NodejsPerformanceExplorerToolBar.NewPerfSession();

            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            app.OpenNodejsPerformance();
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
                perfTarget.InterpreterPath = NodePackage.NodePath;
                perfTarget.ScriptName = TestData.GetPath(@"TestData\NodejsProfileTest\program.js");

                try {
                    perfTarget.Ok();
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
                }
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void NewProfilingSessionOpenSolution() {
            VsIdeTestHostContext.Dte.Solution.Close(false);
            var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);

            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            app.OpenNodejsPerformance();
            var perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance *");

            app.NodejsPerformanceExplorerToolBar.NewPerfSession();
            perf = app.NodejsPerformanceExplorerTreeView.WaitForItem("Performance");
            Debug.Assert(perf != null);

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
                }
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void LaunchNodejsProfilingWizard() {
            var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
            var project = OpenProject(NodejsProfileTest);

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
                    perfTarget = null;
                }
            }
            app.WaitForDialogDismissed();

            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");
            var session = profiling.GetSession(1);

            try {
                Assert.AreNotEqual(null, app.NodejsPerformanceExplorerTreeView.WaitForItem("NodejsProfileTest *"));

                while (profiling.IsProfiling) {
                    // wait for profiling to finish...
                    System.Threading.Thread.Sleep(100);
                }
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void LaunchProject() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
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

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestSaveDirtySession() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                var report = session.GetReport(1);
                var filename = report.Filename;
                Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
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

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestDeleteReport() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                NodejsVisualStudioApp app;
                string reportFilename;
                WaitForReport(profiling, session, out app, out reportFilename);

                new RemoveItemDialog(app.WaitForDialog()).Delete();

                app.WaitForDialogDismissed();

                Assert.IsTrue(!File.Exists(reportFilename));
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestCompareReports() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                for (int i = 0; i < 100 && profiling.IsProfiling; i++) {
                    System.Threading.Thread.Sleep(100);
                }

                session.Launch(false);
                for (int i = 0; i < 100 && profiling.IsProfiling; i++) {
                    System.Threading.Thread.Sleep(100);
                }

                var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
                var pyPerf = app.NodejsPerformanceExplorerTreeView;
                var item = pyPerf.FindItem("NodejsProfileTest *", "Reports");
                var child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);

                AutomationWrapper.EnsureExpanded(child);
                child.SetFocus();

                Mouse.MoveTo(child.GetClickablePoint());
                Mouse.Click(System.Windows.Input.MouseButton.Right);
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
                            System.Threading.Thread.Sleep(1000);
                            doc.Close(EnvDTE.vsSaveChanges.vsSaveChangesNo);
                            break;
                        }
                    }
                    if (!foundDiff) {
                        System.Threading.Thread.Sleep(300);
                    }
                }
                Assert.IsTrue(foundDiff);
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestRemoveReport() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                NodejsVisualStudioApp app;
                string reportFilename;
                WaitForReport(profiling, session, out app, out reportFilename);

                new RemoveItemDialog(app.WaitForDialog()).Remove();

                app.WaitForDialogDismissed();

                Assert.IsTrue(File.Exists(reportFilename));
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestOpenReport() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                INodePerformanceReport report;
                NodejsVisualStudioApp app;
                AutomationElement child;
                WaitForReport(profiling, session, out report, out app, out child);

                var clickPoint = child.GetClickablePoint();
                Mouse.MoveTo(clickPoint);
                Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                Assert.AreNotEqual(null, app.WaitForDocument(report.Filename));

                app.Dte.Documents.CloseAll(EnvDTE.vsSaveChanges.vsSaveChangesNo);
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        private static void WaitForReport(INodeProfiling profiling, INodeProfileSession session, out INodePerformanceReport report, out NodejsVisualStudioApp app, out AutomationElement child) {
            while (profiling.IsProfiling) {
                System.Threading.Thread.Sleep(100);
            }

            report = session.GetReport(1);
            var filename = report.Filename;
            Assert.IsTrue(filename.Contains("NodejsProfileTest"));

            app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenNodejsPerformance();
            var pyPerf = app.NodejsPerformanceExplorerTreeView;
            Assert.AreNotEqual(null, pyPerf);

            var item = pyPerf.FindItem("NodejsProfileTest *", "Reports");
            child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);
            var childName = child.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;

            Assert.IsTrue(childName.StartsWith("NodejsProfileTest"));

            AutomationWrapper.EnsureExpanded(child);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestOpenReportCtxMenu() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                INodePerformanceReport report;
                NodejsVisualStudioApp app;
                AutomationElement child;
                WaitForReport(profiling, session, out report, out app, out child);

                var clickPoint = child.GetClickablePoint();
                Mouse.MoveTo(clickPoint);
                Mouse.Click(System.Windows.Input.MouseButton.Right);
                Keyboard.Press(System.Windows.Input.Key.O);

                Assert.AreNotEqual(null, app.WaitForDocument(report.Filename));
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestTargetPropertiesForProject() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {
                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
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

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestTargetPropertiesForExecutable() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var session = profiling.LaunchProcess(NodePackage.NodePath,
                TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                TestData.GetPath(@"TestData\NodejsProfileTest"),
                "",
                false
            );

            NodejsVisualStudioApp app = null;
            NodejsPerfTarget perfTarget = null;
            try {
                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
                app.OpenNodejsPerformance();
                var pyPerf = app.NodejsPerformanceExplorerTreeView;

                var item = pyPerf.FindItem("program *");

                Mouse.MoveTo(item.GetClickablePoint());
                Mouse.DoubleClick(System.Windows.Input.MouseButton.Left);

                perfTarget = new NodejsPerfTarget(app.WaitForDialog());
                Assert.AreEqual(NodePackage.NodePath, perfTarget.InterpreterPath);
                Assert.AreEqual("", perfTarget.Arguments);
                Assert.IsTrue(perfTarget.ScriptName.EndsWith("program.js"));
                Assert.IsTrue(perfTarget.ScriptName.StartsWith(perfTarget.WorkingDir));

            } finally {
                if (perfTarget != null) {
                    perfTarget.Cancel();
                    if (app != null) {
                        app.WaitForDialogDismissed();
                    }
                }
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestStopProfiling() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var session = profiling.LaunchProcess(NodePackage.NodePath,
                TestData.GetPath(@"TestData\NodejsProfileTest\infiniteProfile.js"),
                TestData.GetPath(@"TestData\NodejsProfileTest"),
                "",
                false
            );

            try {
                System.Threading.Thread.Sleep(1000);
                Assert.IsTrue(profiling.IsProfiling);
                var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
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

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestTwoProfilesAtTheSameTime() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var session = profiling.LaunchProcess(NodePackage.NodePath,
                TestData.GetPath(@"TestData\NodejsProfileTest\infiniteProfile.js"),
                TestData.GetPath(@"TestData\NodejsProfileTest"),
                "",
                false
            );

            try {
                for (int i = 0; i < 100 && !profiling.IsProfiling; i++) {
                    System.Threading.Thread.Sleep(100);
                }
                Assert.IsTrue(profiling.IsProfiling);
                var app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);

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

        private static void WaitForReport(INodeProfiling profiling, INodeProfileSession session, out NodejsVisualStudioApp app, out string reportFilename) {
            while (profiling.IsProfiling) {
                System.Threading.Thread.Sleep(100);
            }

            var report = session.GetReport(1);
            var filename = report.Filename;
            Assert.IsTrue(filename.Contains("NodejsProfileTest"));

            app = new NodejsVisualStudioApp(VsIdeTestHostContext.Dte);
            app.OpenNodejsPerformance();
            var pyPerf = app.NodejsPerformanceExplorerTreeView;
            Assert.AreNotEqual(null, pyPerf);

            var item = pyPerf.FindItem("NodejsProfileTest *", "Reports");
            var child = item.FindFirst(System.Windows.Automation.TreeScope.Descendants, Condition.TrueCondition);
            var childName = child.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;

            reportFilename = report.Filename;
            Assert.IsTrue(childName.StartsWith("NodejsProfileTest"));

            child.SetFocus();
            Keyboard.PressAndRelease(System.Windows.Input.Key.Delete);
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MultipleTargets() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            INodeProfileSession session2 = null;
            try {
                {
                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(100);
                    }

                    var report = session.GetReport(1);
                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.f");
                }

                {
                    session2 = profiling.LaunchProcess(NodePackage.NodePath,
                        TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                        TestData.GetPath(@"TestData\NodejsProfileTest"),
                        "",
                        false
                    );

                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(100);
                    }

                    var report = session2.GetReport(1);
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

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MultipleTargetsWithProjectHome() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            INodeProfileSession session2 = null;
            try {
                {
                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(100);
                    }

                    var report = session.GetReport(1);
                    var filename = report.Filename;
                    Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                    Assert.AreEqual(session.GetReport(2), null);

                    Assert.AreNotEqual(session.GetReport(report.Filename), null);

                    VerifyReport(report, "program.f");
                }

                {
                    session2 = profiling.LaunchProcess(NodePackage.NodePath,
                        TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                        TestData.GetPath(@"TestData\NodejsProfileTest"),
                        "",
                        false
                    );

                    while (profiling.IsProfiling) {
                        System.Threading.Thread.Sleep(100);
                    }

                    var report = session2.GetReport(1);
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

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void MultipleReports() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var project = OpenProject(NodejsProfileTest);

            var session = profiling.LaunchProject(project, false);
            try {

                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                var report = session.GetReport(1);
                var filename = report.Filename;
                Assert.IsTrue(filename.Contains("NodejsProfileTest"));

                Assert.AreEqual(session.GetReport(2), null);

                Assert.AreNotEqual(session.GetReport(report.Filename), null);

                VerifyReport(report, "program.f");

                session.Launch();

                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                report = session.GetReport(2);
                VerifyReport(report, "program.f");
            } finally {
                profiling.RemoveSession(session, true);
            }
        }


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void LaunchExecutable() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            // no sessions yet
            Assert.AreEqual(profiling.GetSession(1), null);

            var session = profiling.LaunchProcess(NodePackage.NodePath,
                TestData.GetPath(@"TestData\NodejsProfileTest\program.js"),
                TestData.GetPath(@"TestData\NodejsProfileTest"),
                "",
                false
            );
            try {
                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                var report = session.GetReport(1);
                var filename = report.Filename;
                Assert.IsTrue(filename.Contains("program"));

                Assert.AreEqual(session.GetReport(2), null);

                Assert.AreNotEqual(session.GetReport(report.Filename), null);

                VerifyReport(report, "program.f");
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestProjectProperties() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
            if (File.Exists(testFile)) {
                File.Delete(testFile);
            }

            var project = OpenProject(@"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server.js");
            project.Properties.Item("WorkingDirectory").Value = Path.GetTempPath();

            var session = profiling.LaunchProject(project, false);
            try {
                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                Assert.IsTrue(File.Exists(testFile), "test file not created");
                var lines = File.ReadAllLines(testFile);

                Assert.IsTrue(lines[0].Contains("scriptargs"), "no scriptargs");
                Assert.IsTrue(lines[0].Contains("server.js"), "missing filename");
                Assert.AreEqual(lines[1], "port: 1234");
                Assert.AreEqual(lines[2], "cwd: " + Path.GetTempPath().Substring(0, Path.GetTempPath().Length - 1));

            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestBrowserLaunch() {
            var profiling = (INodeProfiling)VsIdeTestHostContext.Dte.GetObject("NodeProfiling");

            var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
            if (File.Exists(testFile)) {
                File.Delete(testFile);
            }

            var project = OpenProject(@"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server2.js");
            project.Properties.Item("WorkingDirectory").Value = Path.GetTempPath();

            var session = profiling.LaunchProject(project, false);
            try {
                while (profiling.IsProfiling) {
                    System.Threading.Thread.Sleep(100);
                }

                Assert.IsTrue(File.Exists(testFile), "test file not created");
            } finally {
                profiling.RemoveSession(session, true);
            }
        }

        private static void VerifyReport(INodePerformanceReport report, params string[] expectedFunctions) {
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
                for (int i = 0; i < expectedFunctions.Length; i++) {
                    if (line.StartsWith(expectedFunctions[i]) || line.StartsWith(altExpected[i])) {
                        expected[i] = true;
                    }
                }
            }

            foreach (var found in expected) {
                Assert.IsTrue(found);
            }            
        }

        private static int _counter;

        private static string[] OpenPerformanceReportAsCsv(INodePerformanceReport report) {
            var perfReportPath = Path.Combine(GetPerfToolsPath(false), "vsperfreport.exe");

            for (int i = 0; i < 11; i++) {
                string csvFilename;
                do {
                    csvFilename = Path.Combine(Path.GetTempPath(), "test") + DateTime.Now.Ticks + "_" + _counter++;
                } while (File.Exists(csvFilename + "_FunctionSummary.csv"));

                var psi = new ProcessStartInfo(perfReportPath, report.Filename + " /output:" + csvFilename + " /summary:function");
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                var process = System.Diagnostics.Process.Start(psi);
                process.Start();
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
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\" + VSUtility.Version);
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

        internal static Project OpenProject(string projName, string startItem = null, int expectedProjects = 1, string projectName = null, bool setStartupItem = true) {
            string fullPath = TestData.GetPath(projName);
            Assert.IsTrue(File.Exists(fullPath), "Cannot find " + fullPath);
            VsIdeTestHostContext.Dte.Solution.Open(fullPath);

            Assert.IsTrue(VsIdeTestHostContext.Dte.Solution.IsOpen, "The solution is not open");

            int count = VsIdeTestHostContext.Dte.Solution.Projects.Count;
            if (expectedProjects != count) {
                // if we have other files open we can end up with a bonus project...
                int i = 0;
                foreach (EnvDTE.Project proj in VsIdeTestHostContext.Dte.Solution.Projects) {
                    if (proj.Name != "Miscellaneous Files") {
                        i++;
                    }
                }

                Assert.IsTrue(i == expectedProjects, String.Format("Loading project resulted in wrong number of loaded projects, expected 1, received {0}", VsIdeTestHostContext.Dte.Solution.Projects.Count));
            }

            var iter = VsIdeTestHostContext.Dte.Solution.Projects.GetEnumerator();
            iter.MoveNext();

            Project project = (Project)iter.Current;
            if (projectName != null) {
                while (project.Name != projectName) {
                    if (!iter.MoveNext()) {
                        Assert.Fail("Failed to find project named " + projectName);
                    }
                    project = (Project)iter.Current;
                }
            }

            if (startItem != null && setStartupItem) {
                project.SetStartupFile(startItem);
            }

            DeleteAllBreakPoints();

            return project;
        }


        private static void DeleteAllBreakPoints() {
            var debug3 = (Debugger3)VsIdeTestHostContext.Dte.Debugger;
            if (debug3.Breakpoints != null) {
                foreach (var bp in debug3.Breakpoints) {
                    ((Breakpoint3)bp).Delete();
                }
            }
        }
    }
}

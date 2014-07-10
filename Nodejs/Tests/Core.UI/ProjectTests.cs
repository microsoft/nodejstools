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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.UI;
using TestUtilities.UI.Nodejs;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class ProjectTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        /// <summary>
        /// https://nodejstools.codeplex.com/workitem/270
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestSnippetsDisabled() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                Window window;
                var openFile = OpenProjectItem("server.js", out window);

                openFile.MoveCaret(7, 1);
                
                // we need to ensure the snippets are initialized by starting
                // and dismissing an intellisense session.
                Keyboard.Type(Keyboard.CtrlSpace.ToString());
                Keyboard.PressAndRelease(System.Windows.Input.Key.Escape);

                Keyboard.Type("functio");
                System.Threading.Thread.Sleep(2000);
                Keyboard.Type("\t");
                openFile.WaitForText(@"var http = require('http');

var port = process.env.port || 1337;
var mymod = require('./mymod.js');
var mutatemod = require('./mutatemod.js');

function
http.createServer(function (req, res) {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
}).listen(port);
");
            }
        }


        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestNoAutoFormattingEnter() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                Window window;
                var openFile = OpenProjectItem("server.js", out window);

                openFile.MoveCaret(8, 40);
                Keyboard.Type("\r");
                openFile.WaitForText(@"var http = require('http');

var port = process.env.port || 1337;
var mymod = require('./mymod.js');
var mutatemod = require('./mutatemod.js');


http.createServer(function (req, res) {

    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
}).listen(port);
");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestNoAutoFormattingCloseFunction() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                Window window;
                var openFile = OpenProjectItem("server.js", out window);

                openFile.MoveCaret(8, 40);
                Keyboard.Type("\rfunction f() { }");
                var text = openFile.Text;
                openFile.WaitForText(@"var http = require('http');

var port = process.env.port || 1337;
var mymod = require('./mymod.js');
var mutatemod = require('./mutatemod.js');


http.createServer(function (req, res) {
    function f() { }
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
}).listen(port);
");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestNoAutoFormattingPaste() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                Window window;
                var openFile = OpenProjectItem("server.js", out window);

                openFile.MoveCaret(8, 40);
                openFile.Invoke(() => System.Windows.Clipboard.SetText("\r\n"));
                Keyboard.ControlV();
                openFile.WaitForText(@"var http = require('http');

var port = process.env.port || 1337;
var mymod = require('./mymod.js');
var mutatemod = require('./mutatemod.js');


http.createServer(function (req, res) {

    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
}).listen(port);
");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestNoReferences() {
            Window window;
            var openFile = OpenProjectItem("server.js", out window);

            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                app.OpenSolutionExplorer();
                var solutionExplorer = app.SolutionExplorerTreeView;
                solutionExplorer.WaitForItemRemoved("Solution 'NodeAppWithModule' (1 project)", "NodeAppWithModule", "References");
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void GlobalIntellisense() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                Window window;
                var openFile = OpenProjectItem("server.js", out window);

                openFile.MoveCaret(6, 1);

                Keyboard.Type("process.");
                using (var session = openFile.WaitForSession<ICompletionSession>()) {

                    var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                    Assert.IsTrue(completions.Contains("abort"));
                    Assert.IsTrue(completions.Contains("chdir"));
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void RequireIntellisenseExpanded() {

            var testCases = new[] {
                new { File="server.js", Line = 4, Type = "mymod.", Expected = "mymod_export" },
                new { File="server.js", Line = 8, Type = "mymod2.", Expected = "mymod_export" },
                new { File="server.js", Line = 12, Type = "mymod3.", Expected = "__filename" },
                new { File="server.js", Line = 19, Type = "foo.", Expected = "foo_export" },
                new { File="server.js", Line = 22, Type = "bar.", Expected = "bar_entry" },
                new { File="server.js", Line = 25, Type = "bar2.", Expected = "bar2_entry" },
                new { File="server.js", Line = 28, Type = "dup.", Expected = "node_modules_dup" },
                new { File="server.js", Line = 31, Type = "dup1.", Expected = "top_level" },
                new { File="server.js", Line = 34, Type = "dup2.", Expected = "top_level" },
                new { File="server.js", Line = 37, Type = "baz_dup.", Expected = "baz_dup" },
                new { File="server.js", Line = 40, Type = "baz_dup2.", Expected = "baz_dup" },
                new { File="server.js", Line = 42, Type = "recursive.", Expected = "recursive1" },
                new { File="server.js", Line = 42, Type = "recursive.", Expected = "recursive2" },
                new { File="server.js", Line = 48, Type = "nested.", Expected = "__filename" },
                new { File="server.js", Line = 54, Type = "indexfolder.", Expected = "indexfolder" },
                new { File="server.js", Line = 56, Type = "indexfolder2.", Expected = "indexfolder" },
                new { File="server.js", Line = 60, Type = "resolve_path.", Expected = "indexfolder" },

                new { File="node_modules\\mymod.js", Line = 5, Type = "dup.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 8, Type = "dup0.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 11, Type = "dup1.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 14, Type = "dup2.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 17, Type = "dup3.", Expected = "dup" },

                new { File="node_modules\\foo\\index.js", Line = 5, Type = "dup.", Expected = "foo_node_modules" },
                new { File="node_modules\\foo\\index.js", Line = 8, Type = "dup1.", Expected = "dup" },
                new { File="node_modules\\foo\\index.js", Line = 11, Type = "dup2.", Expected = "dup" },
                new { File="node_modules\\foo\\index.js", Line = 14, Type = "other.", Expected = "other" },
                new { File="node_modules\\foo\\index.js", Line = 17, Type = "other2.", Expected = "other" },
                new { File="node_modules\\foo\\index.js", Line = 20, Type = "other3.", Expected = "__filename" },
                new { File="node_modules\\foo\\index.js", Line = 27, Type = "other4.", Expected = "__filename" },

                new { File="baz\\dup.js", Line = 3, Type = "parent_dup.", Expected = "top_level" },
                new { File="baz\\dup.js", Line = 6, Type = "bar.", Expected = "bar_entry" },
                new { File="baz\\dup.js", Line = 9, Type = "parent_dup2.", Expected = "top_level" },
            };

            string text = null;
            string curFile = null;
            Window window;
            EditorWindow openFile = null;
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {

                foreach (var testCase in testCases) {
                    if (testCase.File != curFile) {
                        openFile = OpenProjectItem(testCase.File, out window, @"TestData\RequireTestApp\RequireTestApp.sln");
                        app.OpenSolutionExplorer();
                        app.SolutionExplorerTreeView.WaitForItem("Solution 'RequireTestApp' (1 project)", "RequireTestApp", "dup.js");
                        text = openFile.Text;
                        curFile = testCase.File;
                    }

                    Console.WriteLine("{0} {1}", testCase.Line, testCase.Type);

                    openFile.MoveCaret(testCase.Line, 1);
                    openFile.Invoke(() => openFile.TextView.Caret.EnsureVisible());
                    openFile.SetFocus();
                    Keyboard.Type(testCase.Type);
                    using (var session = openFile.WaitForSession<ICompletionSession>()) {
                        var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                        Assert.IsTrue(completions.Contains(testCase.Expected));
                        Keyboard.Type(System.Windows.Input.Key.Escape);
                        for (int i = 0; i < testCase.Type.Length; i++) {
                            Keyboard.Type(System.Windows.Input.Key.Back);
                        }

                        openFile.WaitForText(text);
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void GlobalIntellisenseProjectReload() {
            Window window;
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                app.OpenProject(Path.GetFullPath(@"TestData\NodeAppWithModule2\NodeAppWithModule.sln"));

                var projectName = "NodeAppWithModule";
                var project = app.SolutionExplorerTreeView.WaitForItem(
                    "Solution '" + projectName + "' (1 project)",
                    projectName);

                var projectNode = new TreeNode(project);
                projectNode.SetFocus();

                System.Threading.Thread.Sleep(2000);

                VsIdeTestHostContext.Dte.ExecuteCommand("Project.UnloadProject");

                project = app.SolutionExplorerTreeView.WaitForItem(
                    "Solution '" + projectName + "' (0 projects)",
                    projectName + " (unavailable)");

                projectNode = new TreeNode(project);
                projectNode.Select();

                System.Threading.Thread.Sleep(2000);

                VsIdeTestHostContext.Dte.ExecuteCommand("Project.ReloadProject");

                Assert.IsNotNull(
                    app.SolutionExplorerTreeView.WaitForItem(
                        "Solution '" + projectName + "' (1 project)",
                        projectName,
                        "server.js"
                    ),
                    "project not reloaded"
                );

                var openFile = OpenItem("server.js", VsIdeTestHostContext.Dte.Solution.Projects.Item(1), out window);

                openFile.MoveCaret(6, 1);
                Keyboard.Type("process.");
                using (var session = openFile.WaitForSession<ICompletionSession>()) {

                    var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                    Assert.IsTrue(completions.Contains("abort"));
                    Assert.IsTrue(completions.Contains("chdir"));
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void UserModule() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                Window window;
                var openFile = OpenProjectItem("server.js", out window);

                openFile.MoveCaret(6, 1);
                Keyboard.Type("mymod.");
                using (var session = openFile.WaitForSession<ICompletionSession>()) {

                    var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                    Assert.IsTrue(completions.Contains("area"));
                    Assert.IsTrue(completions.Contains("circumference"));
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void AddNewItem() {
            Window window;
            var openFile = OpenProjectItem("server.js", out window);

            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                var dialog = app.OpenDialogWithDteExecuteCommand("Project.AddNewItem");

                var newItem = new NewItemDialog(AutomationElement.FromHandle(dialog));
                newItem.FileName = "NewJSFile.js";
                newItem.ClickOK();

                System.Threading.Thread.Sleep(250);

                var solutionFolder = app.Dte.Solution.Projects.Item(1).ProjectItems;
                Assert.AreNotEqual(null, solutionFolder.Item("NewJSFile.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void EnterCompletion() {
            Window window;
            var openFile = OpenProjectItem("server.js", out window);

            openFile.MoveCaret(6, 1);
            Keyboard.Type("http.");
            System.Threading.Thread.Sleep(3000);
            Keyboard.Type("Cli\r");
            openFile.WaitForText(@"var http = require('http');

var port = process.env.port || 1337;
var mymod = require('./mymod.js');
var mutatemod = require('./mutatemod.js');
http.ClientRequest

http.createServer(function (req, res) {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
}).listen(port);
");
        }

        /// <summary>
        /// Tests completions against builtin node modules.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ModuleCompletions() {
            Window window;
            var openFile = OpenProjectItem("intellisensemod.js", out window);

            openFile.MoveCaret(3, 1);
            Keyboard.Type("server.");
            System.Threading.Thread.Sleep(3000);
            Keyboard.Type("lis\r");
            openFile.WaitForText(@"var http = require('http');
var server = http.createServer(null); // server.listen
server.listen

var sd = require('stringdecoder');  // sd.StringDecoder();


");

            openFile.MoveCaret(6, 1);
            Keyboard.Type("sd.");
            System.Threading.Thread.Sleep(3000);
            Keyboard.Type("Str\r");
            openFile.WaitForText(@"var http = require('http');
var server = http.createServer(null); // server.listen
server.listen

var sd = require('stringdecoder');  // sd.StringDecoder();
sd.StringDecoder

");

        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestNewProject() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                using (var newProjDialog = app.FileNewProject()) {
                newProjDialog.FocusLanguageNode("JavaScript");

                var nodejsApp = newProjDialog.ProjectTypes.FindItem(NodejsVisualStudioApp.JavascriptWebAppTemplate);
                nodejsApp.Select();

                    newProjDialog.OK();
                }

                // wait for new solution to load...            
                for (int i = 0; i < 40 && app.Dte.Solution.Projects.Count == 0; i++) {
                    System.Threading.Thread.Sleep(250);
                }

                app.SolutionExplorerTreeView.WaitForItem(
                    "Solution '" + app.Dte.Solution.Projects.Item(1).Name + "' (1 project)",
                    app.Dte.Solution.Projects.Item(1).Name,
                    "server.js"
                );
                var projItem = app.SolutionExplorerTreeView.WaitForItemRemoved(
                    "Solution '" + app.Dte.Solution.Projects.Item(1).Name + "' (1 project)",
                    app.Dte.Solution.Projects.Item(1).Name,
                    "Web.config"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestNewAzureProject() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                using (var newProjDialog = app.FileNewProject()) {
                newProjDialog.FocusLanguageNode("JavaScript");

                var azureApp = newProjDialog.ProjectTypes.FindItem(NodejsVisualStudioApp.JavaScriptAzureWebAppTemplate);
                azureApp.Select();
                    newProjDialog.OK();
                }

                // wait for new solution to load...
                for (int i = 0; i < 40 && app.Dte.Solution.Projects.Count == 0; i++) {
                    System.Threading.Thread.Sleep(250);
                }

                app.SolutionExplorerTreeView.WaitForItem(
                    "Solution '" + app.Dte.Solution.Projects.Item(1).Name + "' (1 project)",
                    app.Dte.Solution.Projects.Item(1).Name,
                    "server.js"
                );
                app.SolutionExplorerTreeView.WaitForItem(
                    "Solution '" + app.Dte.Solution.Projects.Item(1).Name + "' (1 project)",
                    app.Dte.Solution.Projects.Item(1).Name,
                    "Web.config"
                );
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestAutomationProject() {
            try {
                var project = OpenProject();

                Assert.AreEqual("{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}", project.Kind.ToUpper());
                // we don't yet expose a VSProject interface here, if we did we'd need tests for it, but it doesn't support
                // any functionality we care about/implement yet.
                Assert.AreEqual(typeof(NodejsProjectNode), project.Object.GetType());

                Assert.AreEqual(true, project.Saved);
                project.Saved = false;
                Assert.AreEqual(false, project.Saved);
                project.Saved = true;

                Assert.AreEqual(null, project.Globals);
                Assert.AreEqual("{04726c27-8125-471a-bac0-2301d273db5e}", project.ExtenderCATID);
                var extNames = project.ExtenderNames;
                Assert.AreEqual(typeof(string[]), extNames.GetType());
                Assert.AreEqual(2, ((string[])extNames).Length);
                Assert.AreEqual(null, project.ParentProjectItem);
                Assert.AreEqual(null, project.CodeModel);
                AssertError<ArgumentNullException>(() => project.get_Extender(null));
                AssertError<COMException>(() => project.get_Extender("DoesNotExist"));
                Assert.AreEqual(null, project.Collection);

                foreach (ProjectItem item in project.ProjectItems) {
                    Assert.AreEqual(item.Name, project.ProjectItems.Item(1).Name);
                    break;
                }

                Assert.AreEqual(VsIdeTestHostContext.Dte, project.ProjectItems.DTE);
                Assert.AreEqual(project, project.ProjectItems.Parent);
                Assert.AreEqual(null, project.ProjectItems.Kind);

                AssertError<ArgumentException>(() => project.ProjectItems.Item(-1));
                AssertError<ArgumentException>(() => project.ProjectItems.Item(0));
            } finally {
                VsIdeTestHostContext.Dte.Solution.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void SetAsStartupFile() {
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                var project = OpenProject();

                // wait for new solution to load...
                for (int i = 0; i < 40 && app.Dte.Solution.Projects.Count == 0; i++) {
                    System.Threading.Thread.Sleep(250);
                }

                var item = app.SolutionExplorerTreeView.WaitForItem(
                    "Solution '" + app.Dte.Solution.Projects.Item(1).Name + "' (1 project)",
                    app.Dte.Solution.Projects.Item(1).Name,
                    "mymod.js"
                );

                AutomationWrapper.Select(item);
                app.Dte.ExecuteCommand("Project.SetasNode.jsStartupFile");

                string startupFile = null;
                for (int i = 0; i < 40; i++) {
                    startupFile = (string)project.Properties.Item("StartupFile").Value;
                    if (startupFile == "mymod.js") {
                        break;
                    }
                    System.Threading.Thread.Sleep(250);
                }
                Assert.AreEqual(startupFile, Path.Combine(Environment.CurrentDirectory, @"TestData\NodeAppWithModule\NodeAppWithModule", "mymod.js"));
            }
        }

        private static void AssertError<T>(Action action) where T : Exception {
            try {
                action();
                Assert.Fail();
            } catch (T) {
            }
        }

        private static EditorWindow OpenProjectItem(string startItem, out Window window, string projectName = @"TestData\NodeAppWithModule\NodeAppWithModule.sln") {
            var project = OpenProject(projectName, startItem);

            return OpenItem(startItem, project, out window);
        }

        private static EditorWindow OpenItem(string startItem, Project project, out Window window) {
            EnvDTE.ProjectItem item = null;
            if (startItem.IndexOf('\\') != -1) {
                var items = project.ProjectItems;
                foreach (var itemName in startItem.Split('\\')) {
                    Console.WriteLine(itemName);
                    item = items.Item(itemName);
                    items = item.ProjectItems;
                }
            } else {
                item = project.ProjectItems.Item(startItem);
            }

            Assert.IsNotNull(item);

            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                app.SuppressCloseAllOnDispose();

                window = item.Open();
                window.Activate();
                return app.GetDocument(item.Document.FullName);
            }
        }

        internal static Project OpenProject(string projName = @"TestData\NodeAppWithModule\NodeAppWithModule.sln", string startItem = null, int expectedProjects = 1, string projectName = null, bool setStartupItem = true) {
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

            return project;
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestProjectProperties() {
            for (int mode = 0; mode < 2; mode++) {
                using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                    var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
                    if (File.Exists(testFile)) {
                        File.Delete(testFile);
                    }

                    var project = OpenProjectAndRun(@"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server.js", true, debug: mode == 0);

                    for (int i = 0; i < 30 && !File.Exists(testFile); i++) {
                        System.Threading.Thread.Sleep(250);
                    }

                    Assert.IsTrue(File.Exists(testFile), "test file not created");
                    var lines = File.ReadAllLines(testFile);

                    Assert.IsTrue(lines[0].Contains("scriptargs"), "no scriptargs");
                    Assert.IsTrue(lines[0].Contains("server.js"), "missing filename");
                    Assert.IsFalse(lines[0].Contains("--harmony"), "interpreter argument leaked to script");
                    Assert.IsTrue(lines[1].Contains("--harmony"), "missing interpreter argument");
                    Assert.AreEqual("port: 1234", lines[2]);
                    Assert.AreEqual("cwd: C:\\", lines[3]);
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void TestBrowserLaunch() {
            for (int mode = 0; mode < 2; mode++) {
                var startingProcesses = System.Diagnostics.Process.GetProcessesByName("iexplore").Select(x => x.Id).ToSet();

                using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
                    var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
                    if (File.Exists(testFile)) {
                        File.Delete(testFile);
                    }

                    var project = OpenProjectAndRun(@"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server2.js", true, debug: mode == 0);

                    for (int i = 0; i < 30 && !File.Exists(testFile); i++) {
                        System.Threading.Thread.Sleep(250);
                    }

                    Assert.IsTrue(File.Exists(testFile), "test file not created");
                }

                System.Threading.Thread.Sleep(2000);
                var endingProcesses = System.Diagnostics.Process.GetProcessesByName("iexplore").Select(x => x.Id);
                var newProcesses = endingProcesses.Except(startingProcesses).ToArray();

                if (mode == 0) {
                    // new processes should have been shutdown when debugging stopped
                    Assert.AreEqual(0, newProcesses.Length);
                } else {
                    // no debugging, process will hang around
                    Assert.IsTrue(newProcesses.Length > 0);
                    foreach (var proc in newProcesses) {
                        Console.WriteLine("Killing process {0}", proc);
                        System.Diagnostics.Process.GetProcessById(proc).Kill();
                    }
                }
            }
        }

        internal static Project OpenProjectAndRun(string projName, string filename, bool setStartupItem = true, bool debug = true) {
            var project = OpenProject(projName, filename, setStartupItem: setStartupItem);

            if (debug) {
                VsIdeTestHostContext.Dte.ExecuteCommand("Debug.Start");
            } else {
                VsIdeTestHostContext.Dte.ExecuteCommand("Debug.StartWithoutDebugging");
            }

            return project;
        }
    }
}

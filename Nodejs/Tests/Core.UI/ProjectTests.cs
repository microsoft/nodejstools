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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using EnvDTE;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
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
        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void SnippetsDisabled() {
            using (var app = new VisualStudioApp()) {
                Window window;
                var openFile = OpenProjectItem(app, "server.js", out window);

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


        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void NoAutoFormattingEnter() {
            using (var app = new VisualStudioApp()) {
                Window window;
                var openFile = OpenProjectItem(app, "server.js", out window);

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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void NoAutoFormattingCloseFunction() {
            using (var app = new VisualStudioApp()) {
                Window window;
                var openFile = OpenProjectItem(app, "server.js", out window);

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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void NoAutoFormattingPaste() {
            using (var app = new VisualStudioApp()) {
                Window window;
                var openFile = OpenProjectItem(app, "server.js", out window);

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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void NoReferences() {
            Window window;
            using (var app = new VisualStudioApp()) {
                var openFile = OpenProjectItem(app, "server.js", out window);
                using (new NodejsOptionHolder(NodejsPackage.Instance.GeneralOptionsPage, "ShowBrowserAndNodeLabels", false)) {
                    var solutionExplorer = app.OpenSolutionExplorer();
                    solutionExplorer.WaitForItemRemoved("Solution 'NodeAppWithModule' (1 project)", "NodeAppWithModule", "References");
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void GlobalIntellisense() {
            using (var app = new VisualStudioApp()) {
                Window window;
                var openFile = OpenProjectItem(app, "server.js", out window);

                openFile.MoveCaret(6, 1);

                Keyboard.Type("process.");
                using (var session = openFile.WaitForSession<ICompletionSession>()) {

                    var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                    Assert.IsTrue(completions.Contains("abort"));
                    Assert.IsTrue(completions.Contains("chdir"));
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
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
            using (var app = new VisualStudioApp()) {

                foreach (var testCase in testCases) {
                    if (testCase.File != curFile) {
                        openFile = OpenProjectItem(app, testCase.File, out window, @"TestData\RequireTestApp\RequireTestApp.sln");
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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void GlobalIntellisenseProjectReload() {
            Window window;
            using (var app = new VisualStudioApp()) {
                app.OpenProject(Path.GetFullPath(@"TestData\NodeAppWithModule2\NodeAppWithModule.sln"));

                using (new NodejsOptionHolder(NodejsPackage.Instance.GeneralOptionsPage, "ShowBrowserAndNodeLabels", false)) {
                    app.OpenSolutionExplorer();
                    var projectName = "NodeAppWithModule";
                    var project = app.SolutionExplorerTreeView.WaitForItem(
                        "Solution '" + projectName + "' (1 project)",
                        projectName);

                    var projectNode = new TreeNode(project);
                    projectNode.SetFocus();

                    System.Threading.Thread.Sleep(2000);

                    app.Dte.ExecuteCommand("Project.UnloadProject");

                    project = app.SolutionExplorerTreeView.WaitForItem(
                        "Solution '" + projectName + "' (0 projects)",
                        projectName + " (unavailable)");

                    projectNode = new TreeNode(project);
                    projectNode.Select();

                    System.Threading.Thread.Sleep(2000);

                    app.Dte.ExecuteCommand("Project.ReloadProject");

                    Assert.IsNotNull(
                        app.SolutionExplorerTreeView.WaitForItem(
                            "Solution '" + projectName + "' (1 project)",
                            projectName,
                            "server.js"
                        ),
                        "project not reloaded"
                    );

                    var openFile = OpenItem(app, "server.js", app.Dte.Solution.Projects.Item(1), out window);

                    openFile.MoveCaret(6, 1);
                    Keyboard.Type("process.");
                    using (var session = openFile.WaitForSession<ICompletionSession>()) {

                        var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                        Assert.IsTrue(completions.Contains("abort"));
                        Assert.IsTrue(completions.Contains("chdir"));
                    }
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void UserModule() {
            using (var app = new VisualStudioApp()) {
                Window window;
                var openFile = OpenProjectItem(app, "server.js", out window);

                openFile.MoveCaret(6, 1);
                Keyboard.Type("mymod.");
                using (var session = openFile.WaitForSession<ICompletionSession>()) {

                    var completions = session.Session.CompletionSets.First().Completions.Select(x => x.InsertionText);
                    Assert.IsTrue(completions.Contains("area"));
                    Assert.IsTrue(completions.Contains("circumference"));
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void AddNewItem() {
            Window window;

            using (var app = new VisualStudioApp()) {
                var openFile = OpenProjectItem(app, "server.js", out window);

                using (var newItem = NewItemDialog.FromDte(app)) {
                    newItem.FileName = "NewJSFile.js";
                    newItem.OK();
                }

                System.Threading.Thread.Sleep(250);

                var solutionFolder = app.Dte.Solution.Projects.Item(1).ProjectItems;
                Assert.AreNotEqual(null, solutionFolder.Item("NewJSFile.js"));
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void EnterCompletion() {
            Window window;
            using (var app = new VisualStudioApp()) {
                var openFile = OpenProjectItem(app, "server.js", out window);

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
        }

        /// <summary>
        /// Tests completions against builtin node modules.
        /// </summary>
        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void ModuleCompletions() {
            Window window;

            using (var app = new VisualStudioApp()) {
                var openFile = OpenProjectItem(app, "intellisensemod.js", out window);

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
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void NewProject() {
            using (var app = new VisualStudioApp()) {
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

                app.OpenSolutionExplorer();
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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void NewAzureProject() {
            using (var app = new VisualStudioApp()) {
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

                app.OpenSolutionExplorer();
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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void AutomationProject() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodeAppWithModule\NodeAppWithModule.sln");

                using (new NodejsOptionHolder(NodejsPackage.Instance.GeneralOptionsPage, "ShowBrowserAndNodeLabels", false)) {
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
    
                    Assert.AreEqual(app.Dte, project.ProjectItems.DTE);
                    Assert.AreEqual(project, project.ProjectItems.Parent);
                    Assert.AreEqual(null, project.ProjectItems.Kind);
    
                    AssertError<ArgumentException>(() => project.ProjectItems.Item(-1));
                    AssertError<ArgumentException>(() => project.ProjectItems.Item(0));
                }
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void SetAsStartupFile() {
            using (var app = new VisualStudioApp()) {
                var project = app.OpenProject(@"TestData\NodeAppWithModule\NodeAppWithModule.sln");

                using (new NodejsOptionHolder(NodejsPackage.Instance.GeneralOptionsPage, "ShowBrowserAndNodeLabels", false)) {
                    // wait for new solution to load...
                    for (int i = 0; i < 40 && app.Dte.Solution.Projects.Count == 0; i++) {
                        System.Threading.Thread.Sleep(250);
                    }

                    app.OpenSolutionExplorer();
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
        }

        private static void AssertError<T>(Action action) where T : Exception {
            try {
                action();
                Assert.Fail();
            } catch (T) {
            }
        }

        private static EditorWindow OpenProjectItem(VisualStudioApp app, string startItem, out Window window, string projectName = @"TestData\NodeAppWithModule\NodeAppWithModule.sln") {
            var project = app.OpenProject(projectName, startItem);

            return OpenItem(app, startItem, project, out window);
        }

        private static EditorWindow OpenItem(VisualStudioApp app, string startItem, Project project, out Window window) {
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

            window = item.Open();
            window.Activate();
            return app.GetDocument(item.Document.FullName);
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void ProjectProperties() {
            for (int mode = 0; mode < 2; mode++) {
                using (var app = new VisualStudioApp()) {
                    var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
                    if (File.Exists(testFile)) {
                        File.Delete(testFile);
                    }

                    var project = OpenProjectAndRun(app, @"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server.js", true, debug: mode == 0);

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

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void BrowserLaunch() {
            for (int mode = 0; mode < 2; mode++) {
                var startingProcesses = System.Diagnostics.Process.GetProcessesByName("iexplore").Select(x => x.Id).ToSet();

                using (var app = new VisualStudioApp()) {
                    var testFile = Path.Combine(Path.GetTempPath(), "nodejstest.txt");
                    if (File.Exists(testFile)) {
                        File.Delete(testFile);
                    }

                    var project = OpenProjectAndRun(app, @"TestData\NodejsProjectPropertiesTest\NodejsProjectPropertiesTest.sln", "server2.js", true, debug: mode == 0);

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

        internal static Project OpenProjectAndRun(VisualStudioApp app, string projName, string filename, bool setStartupItem = true, bool debug = true) {
            var project = app.OpenProject(projName, filename, setStartupItem: setStartupItem);

            if (debug) {
                app.Dte.ExecuteCommand("Debug.Start");
            } else {
                app.Dte.ExecuteCommand("Debug.StartWithoutDebugging");
            }

            return project;
        }

        private string CreateTempPathOfLength(int length) {
            var path = TestData.GetTempPath(randomSubPath: true) + "\\";

            int padding = length - path.Length;
            if (padding <= 0) {
                Assert.Inconclusive("Could not obtain a base directory with a sufficiently short path.");
            }

            path += new string('a', padding);
            Directory.CreateDirectory(path);

            return path;
        }

        private void DeleteLongPath(string path) {
            if (!path.StartsWith(@"\\?\")) {
                path = @"\\?\" + path;
            }

            Microsoft.VisualStudioTools.Project.WIN32_FIND_DATA wfd;
            IntPtr hFind = Microsoft.VisualStudioTools.Project.NativeMethods.FindFirstFile(path + "\\*", out wfd);
            if (hFind == Microsoft.VisualStudioTools.Project.NativeMethods.INVALID_HANDLE_VALUE) {
                return;
            }

            try {
                do {
                    if (wfd.cFileName == "." || wfd.cFileName == "..") {
                        continue;
                    }

                    string childPath = path;
                    if (childPath != "") {
                        childPath += "\\";
                    }
                    childPath += wfd.cFileName;

                    bool isDirectory = (wfd.dwFileAttributes & Microsoft.VisualStudioTools.Project.NativeMethods.FILE_ATTRIBUTE_DIRECTORY) != 0;
                    if (isDirectory) {
                        DeleteLongPath(childPath);
                    } else {
                        Console.WriteLine("DeleteFile " + childPath);
                        if (!Microsoft.VisualStudioTools.Project.NativeMethods.DeleteFile(childPath)) {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }
                } while (Microsoft.VisualStudioTools.Project.NativeMethods.FindNextFile(hFind, out wfd));

                Console.WriteLine("RemoveDirectory " + path);
                if (!Microsoft.VisualStudioTools.Project.NativeMethods.RemoveDirectory(path)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            } finally {
                Microsoft.VisualStudioTools.Project.NativeMethods.FindClose(hFind);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core")]
        public void GetLongSubPaths() {
            string
                basePath = CreateTempPathOfLength(248 - 1 - @"\d\d".Length),
                shortFile = basePath + @"\f",
                shortDir = basePath + @"\d",
                shortShortFile = shortDir + @"\f",
                shortShortDir = shortDir + @"\d",
                shortLongDir = shortDir + @"\dd",
                shortLongFile = shortDir + @"\f.ffffffffffffff",
                longFile = basePath + @"\f.ffffffffffffff",
                longDir = basePath + @"\d.dd",
                longLongFile = longDir + @"\ff",
                longLongDir = longDir + @"\dd";
            try {
                foreach (var path in new[] { shortDir, shortShortDir, shortLongDir, longDir, longLongDir }) {
                    Console.WriteLine("CreateDirectory {0}", path);
                    Assert.IsTrue(NativeMethods.CreateDirectory(@"\\?\" + path, IntPtr.Zero), path);
                }

                File.WriteAllText(shortFile, "");
                foreach (var path in new[] { shortShortFile, shortLongFile, longFile, longLongFile }) {
                    Console.WriteLine("CopyFile {0}", path);
                    Assert.IsTrue(NativeMethods.CopyFile(shortFile, @"\\?\" + path, true), path);
                }

                var longPaths = NodejsProjectNode.GetLongSubPaths(basePath).ToList();

                // Single() acts as assert here (throws if it doesn't find the element matching the condition).
                longPaths.Remove(longPaths.Single(lpi => lpi.FullPath == shortLongFile && !lpi.IsDirectory));
                longPaths.Remove(longPaths.Single(lpi => lpi.FullPath == shortLongDir && lpi.IsDirectory));
                longPaths.Remove(longPaths.Single(lpi => lpi.FullPath == longFile && !lpi.IsDirectory));
                longPaths.Remove(longPaths.Single(lpi => lpi.FullPath == longDir && lpi.IsDirectory));
                // longLongFile and longLongDir should not be reported, because their parent longDir is already reported.

                // There should be no other elements reported.
                Assert.AreEqual(0, longPaths.Count);
            } finally {
                DeleteLongPath(basePath);
            }
        }

        [TestMethod, Priority(0), TestCategory("Core"), TestCategory("Ignore")]
        [HostType("VSTestHost")]
        public void LongPathCheck() {
            string[] expectedLongPaths = {
                @"node_modules\azure\node_modules\azure-common\node_modules\xml2js\node_modules\sax\test\trailing-attribute-no-value.js",
                @"node_modules\azure\node_modules\azure-common\node_modules\xml2js\node_modules\sax\test\xmlns-xml-default-prefix-attribute.js",
                @"node_modules\azure\node_modules\azure-common\node_modules\xml2js\node_modules\sax\test\xmlns-xml-default-redefine.js",
                @"node_modules\azure\node_modules\request\node_modules\form-data\node_modules\combined-stream\node_modules",
                @"node_modules\azure\node_modules\request\node_modules\http-signature\node_modules\ctype\tst\ctio\uint\tst.roundtrip.js",
            };                                         

            string projectDir = CreateTempPathOfLength(248 - 1 - expectedLongPaths.Min(s => s.Length));
            try {
                foreach (var fileName in new[] { "HelloWorld.njsproj", "HelloWorld.sln", "package.json", "README.md", "server.js" }) {
                    File.Copy(TestData.GetPath(@"TestData\HelloWorld\" + fileName), projectDir + "\\" + fileName);
                }

                using (var app = new NodejsVisualStudioApp()) {
                    try {
                        NodejsPackage.Instance.GeneralOptionsPage.CheckForLongPaths = true;

                        var project = app.OpenProject(projectDir + "\\HelloWorld.sln");

                        // Wait for new solution to load.
                        for (int i = 0; i < 40 && app.Dte.Solution.Projects.Count == 0; i++) {
                            System.Threading.Thread.Sleep(250);
                        }

                        const string interpreterDescription = "Node.js Interactive Window";
                        app.Dte.ExecuteCommand("View.Node.jsInteractiveWindow");
                        var interactive = app.GetInteractiveWindow(interpreterDescription);
                        if (interactive == null) {
                            Assert.Inconclusive("Need " + interpreterDescription);
                        }

                        interactive.WaitForIdleState();
                        var npmTask = interactive.ReplWindow.ExecuteCommand(".npm install azure@0.9.12");

                        using (var dialog = new AutomationDialog(app, AutomationElement.FromHandle(app.WaitForDialog(npmTask)))) {
                            // The option to offer "npm dedupe" should be there.
                            var firstCommandLink = dialog.FindByAutomationId("CommandLink_1000");
                            Assert.IsNotNull(firstCommandLink);
                            Assert.IsTrue(firstCommandLink.Current.Name.Contains("npm dedupe"));

                            dialog.ClickButtonByAutomationId("ExpandoButton");
                            var detailsText = dialog.FindByAutomationId("ExpandedFooterTextLink");
                            Assert.IsNotNull(detailsText);

                            var reportedLongPaths =
                                (from line in detailsText.Current.Name.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                 where line.StartsWith("•")
                                 let nbspIndex = line.IndexOf('\u00A0')
                                 where nbspIndex >= 0
                                 select line.Substring(1, nbspIndex - 1).Trim()
                                ).ToArray();
                            Console.WriteLine("Reported paths:");
                            foreach (var path in reportedLongPaths) {
                                Console.WriteLine("\t" + path);
                            }

                            reportedLongPaths = reportedLongPaths.Except(expectedLongPaths).ToArray();
                            if (reportedLongPaths.Length != 0) {
                                Console.WriteLine("Unexpected paths:");
                                foreach (var path in reportedLongPaths) {
                                    Console.WriteLine("\t" + path);
                                }
                                Assert.Fail("Unexpected long paths reported.");
                            }

                            dialog.WaitForClosed(TimeSpan.FromSeconds(1), () => {
                                // Click the first command button (dedupe).
                                firstCommandLink.SetFocus();
                                Keyboard.Press(System.Windows.Input.Key.Enter);
                            });
                        }

                        // Clicking on that button should not change the option.
                        Assert.IsTrue(NodejsPackage.Instance.GeneralOptionsPage.CheckForLongPaths);

                        interactive.WaitForTextContainsAll("dedupe successfully completed");
                        interactive.WaitForIdleState();

                        // npm dedupe won't be able to fix the problem, so we should get the dialog once again.
                        using (var dialog = new AutomationDialog(app, AutomationElement.FromHandle(app.WaitForDialog(npmTask)))) {
                            // The option to offer "npm dedupe" should not be there anymore.
                            var firstCommandLink = dialog.FindByAutomationId("CommandLink_1000");
                            Assert.IsNotNull(firstCommandLink);
                            Assert.IsFalse(firstCommandLink.Current.Name.Contains("npm dedupe"));

                            dialog.WaitForClosed(TimeSpan.FromSeconds(1), () => {
                                Keyboard.Type("\r"); // click the first command button (do nothing, but warn next time)
                            });
                        }

                        npmTask.Wait(1000);
                        Assert.IsTrue(npmTask.IsCompleted);

                        // Clicking on that button should not change the option.
                        Assert.IsTrue(NodejsPackage.Instance.GeneralOptionsPage.CheckForLongPaths);

                        // Try again to see that the dialog still appears. Any npm command triggers the check.
                        // and since we didn't do anything to fix the problem, we should still get the dialog.
                        interactive.WaitForIdleState();
                        npmTask = interactive.ReplWindow.ExecuteCommand(".npm list");

                        using (var dialog = new AutomationDialog(app, AutomationElement.FromHandle(app.WaitForDialog(npmTask)))) {
                            // The option to offer "npm dedupe" should be there again, since this is a new check.
                            var firstCommandLink = dialog.FindByAutomationId("CommandLink_1000");
                            Assert.IsNotNull(firstCommandLink);
                            Assert.IsTrue(firstCommandLink.Current.Name.Contains("npm dedupe"));

                            dialog.WaitForClosed(TimeSpan.FromSeconds(1), () => {
                                // Click the third command button (do nothing, do not warn anymore)
                                firstCommandLink.SetFocus();
                                Keyboard.Press(System.Windows.Input.Key.Tab);
                                Keyboard.Press(System.Windows.Input.Key.Tab);
                                Keyboard.Press(System.Windows.Input.Key.Enter);
                            });
                        }

                        npmTask.Wait(1000);
                        Assert.IsTrue(npmTask.IsCompleted);

                        // Clicking on that button should change the option to false.
                        Assert.IsFalse(NodejsPackage.Instance.GeneralOptionsPage.CheckForLongPaths);

                        // Try again to see that the dialog does not appear anymore.
                        interactive.WaitForIdleState();
                        npmTask = interactive.ReplWindow.ExecuteCommand(".npm list");
                        app.WaitForNoDialog(TimeSpan.FromSeconds(3));
                    } finally {
                        NodejsPackage.Instance.GeneralOptionsPage.CheckForLongPaths = true;
                    }
                }
            } finally {
                DeleteLongPath(projectDir);
            }
        }
    }
}

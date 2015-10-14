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
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project.ImportWizard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NodejsTests {
    [TestClass]
    public class ImportWizardTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, Priority(0)]
        public void ImportWizardSimple() {
            DispatcherTest(async () => {
                var settings = new ImportSettings();

                settings.SourcePath = TestData.GetPath("TestData\\HelloWorld\\");
                settings.Filters = "*.js;*.njsproj";
                settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");
                await Dispatcher.Yield();
                Assert.AreEqual("server.js", settings.StartupFile);

                var path = settings.CreateRequestedProject();

                Assert.AreEqual(settings.ProjectPath, path);
                var proj = XDocument.Load(path);

                Assert.AreEqual("..\\..\\HelloWorld\\", proj.Descendant("ProjectHome").Value);
                AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                    "server.js");
                AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Content")).Select(x => x.Attribute("Include").Value),
                    "HelloWorld.njsproj");
            });
        }

        [TestMethod, Priority(0)]
        public void ImportWizardSimpleApp() {
            DispatcherTest(async () => {                
                var settings = new ImportSettings();

                settings.SourcePath = TestData.GetPath("TestData\\HelloWorldApp\\");
                settings.Filters = "*.js;*.njsproj";
                settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");
                await Dispatcher.Yield();
                Assert.AreEqual("app.js", settings.StartupFile);

                var path = settings.CreateRequestedProject();

                Assert.AreEqual(settings.ProjectPath, path);
                var proj = XDocument.Load(path);

                Assert.AreEqual("..\\..\\HelloWorldApp\\", proj.Descendant("ProjectHome").Value);
                AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                    "app.js");
                AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Content")).Select(x => x.Attribute("Include").Value),
                    "HelloWorld.njsproj");
            });
        }

        [TestMethod, Priority(0)]
        public void ImportWizardSimpleOther() {
            DispatcherTest(async () => {
                var settings = new ImportSettings();

                settings.SourcePath = TestData.GetPath("TestData\\HelloWorldOther\\");
                settings.Filters = "*.js;*.njsproj";
                settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");
                await Dispatcher.Yield();
                Assert.AreEqual("other.js", settings.StartupFile);

                var path = settings.CreateRequestedProject();

                Assert.AreEqual(settings.ProjectPath, path);
                var proj = XDocument.Load(path);

                Assert.AreEqual("..\\..\\HelloWorldOther\\", proj.Descendant("ProjectHome").Value);
                AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                    "other.js");
                AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Content")).Select(x => x.Attribute("Include").Value),
                    "HelloWorld.njsproj");
            });
        }

        [TestMethod, Priority(0)]
        public void ImportWizardFiltered() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld\\");
            settings.Filters = "*.js";
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            Assert.AreEqual("..\\..\\HelloWorld\\", proj.Descendant("ProjectHome").Value);
            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js");
            Assert.AreEqual(0, proj.Descendants(proj.GetName("Content")).Count());
        }

        [TestMethod, Priority(0)]
        public void ImportWizardFolders() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld2\\");
            settings.Filters = "*";
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            Assert.AreEqual("..\\..\\HelloWorld2\\", proj.Descendant("ProjectHome").Value);
            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js",
                "TestFolder\\SubItem.js",
                "TestFolder2\\SubItem.js",
                "TestFolder3\\SubItem.js");

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Folder")).Select(x => x.Attribute("Include").Value),
                "TestFolder",
                "TestFolder2",
                "TestFolder3");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardStartupFile() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld\\");
            settings.Filters = "*.js;*.njsproj";
            settings.StartupFile = "server.js";
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            Assert.AreEqual("server.js", proj.Descendant("StartupFile").Value);
        }

        [TestMethod, Priority(0)]
        public void ImportWizardIncludeNodeModules() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld3\\");
            settings.Filters = "*.js;*.njsproj";
            settings.StartupFile = "server.js";
            settings.ExcludeNodeModules = false;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js",
                "node_modules\\mymod.js");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardExcludeNodeModules() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld3\\");
            settings.Filters = "*.js;*.njsproj";
            settings.StartupFile = "server.js";
            settings.ExcludeNodeModules = true;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardExcludeDotPrefixedFolders() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld5\\");
            settings.Filters = "*.js;*.njsproj";
            settings.StartupFile = "server.js";
            settings.ExcludeNodeModules = true;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Compile")).Select(x => x.Attribute("Include").Value),
                "server.js",
                "TestFolder\\SubItem.js",
                "TestFolderWith.Extension\\SubItem.js");

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Folder")).Select(x => x.Attribute("Include").Value),
                "TestFolder",
                "TestFolderWith.Extension");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardEmptyFolders() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld4");
            settings.Filters = "*.js";
            settings.StartupFile = "server.js";
            settings.ExcludeNodeModules = true;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Folder")).Select(x => x.Attribute("Include").Value),
                "Baz");
        }

        [TestMethod, Priority(0)]
        public void ImportWizardEmptyFoldersDontExcludeNodeModules() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorld4\\");
            settings.Filters = "*.js";
            settings.StartupFile = "server.js";
            settings.ExcludeNodeModules = false;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\Subdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("Folder")).Select(x => x.Attribute("Include").Value),
                "Baz", "node_modules");
        }

        [TestMethod, Priority(0)]
        public void ProjectFileAlreadyExists() {
            DispatcherTest(async () => {
                var settings = new ImportSettings();

                settings.SourcePath = TestData.GetPath("TestData\\HelloWorld3");
                settings.Filters = "*.js";
                settings.StartupFile = "server.js";
                settings.ExcludeNodeModules = true;
                await Dispatcher.Yield();
                Assert.AreEqual("HelloWorld31.njsproj", Path.GetFileName(settings.ProjectPath));
            });
        }

        [TestMethod, Priority(0)]
        public void ImportWizardTypeScript() {
            var settings = new ImportSettings();

            settings.SourcePath = TestData.GetPath("TestData\\HelloWorldTypeScript\\");
            settings.Filters = "*.ts;*.md;*.json";
            settings.StartupFile = "server.ts";
            settings.ExcludeNodeModules = false;
            settings.ProjectPath = TestData.GetPath("TestData\\TestDestination\\TsSubdirectory\\ProjectName.njsproj");

            var path = settings.CreateRequestedProject();

            Assert.AreEqual(settings.ProjectPath, path);
            var proj = XDocument.Load(path);

            AssertUtil.ContainsExactly(proj.Descendants(proj.GetName("TypeScriptCompile")).Select(x => x.Attribute("Include").Value),
                "server.ts");

            Assert.AreEqual("server.ts", proj.Descendant("StartupFile").Value);
        }

        /// <summary>
        /// Creates a new window so that we have an active dispatcher for
        /// test cases which depend upon posting async events back and having
        /// them get processed at some point.
        /// 
        /// Test cases need to do a await Dispatcher.Yield() in order to have
        /// any actual message processing occur.
        /// </summary>
        /// <param name="testCase"></param>
        private static void DispatcherTest(Action testCase) {
            var window = new Window();
            window.ShowInTaskbar = false;
            window.MaxHeight = 0;
            window.MaxWidth = 0;
            window.WindowStyle = WindowStyle.None;
            window.Activated += (sender, args) => {
                testCase();
                window.Close();
            };
            window.ShowDialog();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}

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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;

using MSBuild = Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudioTools;

using Microsoft.NodejsTools.SourceMapping;

namespace Microsoft.NodejsTools.TestAdapter {
    [FileExtension(NodejsConstants.NodejsProjectExtension)]
    [DefaultExecutorUri(TestExecutor.ExecutorUriString)]
    class TestDiscoverer : ITestDiscoverer {
        public TestDiscoverer() {
        }

        /// <summary>
        /// ITestDiscover, Given a list of test sources this method pulls out the test cases
        /// </summary>
        /// <param name="sources">List of test sources passed from client (Client can be VS or command line)</param>
        /// <param name="discoveryContext">Context and runSettings for current run.  Discoverer pulls out the tests based on current context</param>
        /// <param name="logger">Used to relay messages to registered loggers</param>
        /// <param name="discoverySink">Callback used to notify client upon discovery of test cases</param>
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink) {
            ValidateArg.NotNull(sources, "sources");
            ValidateArg.NotNull(discoverySink, "discoverySink");
            ValidateArg.NotNull(logger, "logger");

            using (var buildEngine = new MSBuild.ProjectCollection()) {
                try {
                    // Load all the test containers passed in (.njsproj msbuild files)
                    foreach (string source in sources) {
                        buildEngine.LoadProject(source);
                    }

                    foreach (var proj in buildEngine.LoadedProjects) {

                        var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));

                        Dictionary<string, List<TestFileEntry>> testItems = new Dictionary<string,List<TestFileEntry>>(StringComparer.OrdinalIgnoreCase);
                        // Provide all files to the test analyzer
                        foreach (var item in proj.Items.Where(item => item.ItemType == "Compile" || item.ItemType == "TypeScriptCompile")) {

                            //Check to see if this is a TestCase
                            string value = item.GetMetadataValue("TestFramework");
                            if (!TestContainerDiscoverer.IsValidTestFramework(value)) {
                                continue;
                            }
                            string fileAbsolutePath = CommonUtils.GetAbsoluteFilePath(projectHome, item.EvaluatedInclude);
                            bool typeScriptTest = TypeScript.TypeScriptHelpers.IsTypeScriptFile(fileAbsolutePath);
                            if(typeScriptTest){
                                fileAbsolutePath = TypeScript.TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(proj,fileAbsolutePath);
                            } else if (!Path.GetExtension(fileAbsolutePath).Equals(".js", StringComparison.OrdinalIgnoreCase)) {
                                continue;
                            }
                            List<TestFileEntry> fileList;
                            if (!testItems.TryGetValue(value, out fileList)){
                                fileList = new List<TestFileEntry>();
                                testItems.Add(value, fileList);
                            }
                            fileList.Add(new TestFileEntry(fileAbsolutePath,typeScriptTest));
                        }

                        //Debug.Fail("Before Discover");
                        DiscoverTests(testItems, proj, discoverySink, logger);
                    }
                } finally {
                    // Disposing buildEngine does not clear the document cache in
                    // VS 2013, so manually unload all projects before disposing.
                    buildEngine.UnloadAllProjects();
                }
            }
        }

        private void DiscoverTests(Dictionary<string, List<TestFileEntry>> testItems, MSBuild.Project proj, ITestCaseDiscoverySink discoverySink, IMessageLogger logger) {
            List<TestFrameworks.NodejsTestInfo> result = new List<TestFrameworks.NodejsTestInfo>();
            var projectHome = Path.GetFullPath(Path.Combine(proj.DirectoryPath, "."));
            var projSource = ((MSBuild.Project)proj).FullPath;

            var nodeExePath =
                Nodejs.GetAbsoluteNodeExePath(
                    projectHome,
                    proj.GetPropertyValue(NodejsConstants.NodeExePath));

            if (!File.Exists(nodeExePath)) {
                logger.SendMessage(TestMessageLevel.Error, String.Format("Node.exe was not found.  Please install Node.js before running tests."));
                return;
            }

            int testCount = 0;
            foreach (string testFx in testItems.Keys) {
                TestFrameworks.TestFramework testFramework = GetTestFrameworkObject(testFx);
                if (testFramework == null) {
                    logger.SendMessage(TestMessageLevel.Warning, String.Format("Ignoring unsupported test framework {0}", testFx));
                    continue;
                }

                List<TestFileEntry> fileList = testItems[testFx];
                string files = string.Join(";", fileList.Select(p=>p.File));
                logger.SendMessage(TestMessageLevel.Informational, String.Format("Processing: {0}", files));

                List<TestFrameworks.NodejsTestInfo> discoveredTestCases = testFramework.FindTests(fileList.Select(p => p.File), nodeExePath, logger, projectHome);
                testCount += discoveredTestCases.Count;
                foreach (TestFrameworks.NodejsTestInfo discoveredTest in discoveredTestCases) {
                    string qualifiedName = discoveredTest.FullyQualifiedName;
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("  " /*indent*/ + "Creating TestCase:{0}", qualifiedName));
                    //figure out the test source info such as line number
                    string filePath = discoveredTest.ModulePath;
                    TestFileEntry entry = fileList.Find(p => p.File.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                    FunctionInformation fi = null;
                    if (entry.IsTypeScriptTest) {
                        fi = SourceMapper.MaybeMap(new FunctionInformation(String.Empty,
                                                                           discoveredTest.TestName,
                                                                           discoveredTest.SourceLine,
                                                                           entry.File));
                    }
                    discoverySink.SendTestCase(
                        new TestCase(qualifiedName, TestExecutor.ExecutorUri, projSource) {
                            CodeFilePath = (fi != null) ? fi.Filename : filePath,
                            LineNumber = (fi != null && fi.LineNumber.HasValue) ? fi.LineNumber.Value : discoveredTest.SourceLine,
                            DisplayName = discoveredTest.TestName
                        });                                    

                }
                logger.SendMessage(TestMessageLevel.Informational, string.Format("Processing finished for framework of {0}", testFx));
            }
            if (testCount == 0) {
                logger.SendMessage(TestMessageLevel.Warning, String.Format("Discovered 0 testcases."));
            }
        }

        private TestFrameworks.TestFramework GetTestFrameworkObject(string testFramework) {
            TestFrameworks.FrameworkDiscover discover = new TestFrameworks.FrameworkDiscover();
            return discover.Get(testFramework);
        }

        internal static MSBuild.Project LoadProject(MSBuild.ProjectCollection buildEngine, string fullProjectPath) {
            var buildProject = buildEngine.GetLoadedProjects(fullProjectPath).FirstOrDefault();

            if (buildProject != null) {
                buildEngine.UnloadProject(buildProject);
            }
            return buildEngine.LoadProject(fullProjectPath);
        }

        private class TestFileEntry {
            public string File { get; set; }
            public bool IsTypeScriptTest { get; set; }

            public TestFileEntry(string file, bool isTypeScriptTest) {
                File = file;
                IsTypeScriptTest = isTypeScriptTest;
            }
        }
    }
}

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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.NodejsTools.TestAdapter.TestFrameworks;
using Microsoft.NodejsTools.TestFrameworks;

namespace TestAdapterTests
{
    [TestClass]
    public class FrameworkDiscoverTest
    {
        [TestMethod, Priority(0)]
        public void InitializeAllFrameworks()
        {
            //Arrange and Act
            string[] frameworkDirectories = new string[] {
                "c:\\nodejstools\\" + TestFrameworkDirectories.ExportRunnerFramework,
                "c:\\nodejstools\\" + "mocha"
             };
            FrameworkDiscover discover = new FrameworkDiscover(frameworkDirectories);

            //Assert
            TestFramework defaultOne = discover.Get(TestFrameworkDirectories.ExportRunnerFramework);
            Assert.IsNotNull(defaultOne);
            TestFramework mocha = discover.Get("moCHA");//searching on name is case insensitive
            Assert.IsNotNull(mocha);
            TestFramework nonSenseOne = discover.Get("NonSense");
            Assert.IsNull(nonSenseOne);
        }

        [TestMethod, Priority(0)]
        public void DefaultFramework_HasCorrectFolderInformation()
        {
            //Arrange
            string testName = "dummyUT";
            string testFile = "dummyTestFile.js";
            string vsixInstallFolder = "c:\\dummyFolder";
            string workingFolder = "c:\\DummyNodejsProject";
            string framework = TestFrameworkDirectories.ExportRunnerFramework;
            string testFrameworkDirectory = vsixInstallFolder + "\\" + framework;
            FrameworkDiscover discover = new FrameworkDiscover(new string[] { testFrameworkDirectory });

            //Act
            TestFramework defaultOne = discover.Get(TestFrameworkDirectories.ExportRunnerFramework);
            string[] args = defaultOne.ArgumentsToRunTests(testName, testFile, workingFolder, workingFolder);

            //Assert
            Assert.AreEqual("\"" + vsixInstallFolder + "\\run_tests.js" + "\"", args[0]);
            Assert.AreEqual(framework, args[1]);
            Assert.AreEqual("\"" + testName + "\"", args[2]);
            Assert.AreEqual("\"" + testFile + "\"", args[3]);
            Assert.AreEqual("\"" + workingFolder + "\"", args[4]);
        }
    }
}

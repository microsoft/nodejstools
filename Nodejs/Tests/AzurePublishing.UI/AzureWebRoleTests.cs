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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.UI;
using TestUtilities.UI.Nodejs;

namespace AzurePublishingUITests {
    [TestClass]
    public class AzureWebRoleTests {
        private string _cloudServiceToDelete;
        private static string publishSettingsFilePath;

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();

            // The tests currently only support Azure Tools v2.2.
            // Support for other versions will be added later.
            var azureToolsVersion = AzureUtility.ToolsVersion.V22;
            if (!AzureUtility.AzureToolsInstalled(azureToolsVersion)) {
                Assert.Inconclusive(string.Format("Azure Tools v{0} required", azureToolsVersion));
            }

            publishSettingsFilePath = Environment.GetEnvironmentVariable("TEST_AZURE_SUBSCRIPTION_FILE");
            Assert.IsFalse(string.IsNullOrEmpty(publishSettingsFilePath), "TEST_AZURE_SUBSCRIPTION_FILE environment variable must be set to the path of a .publishSettings file for the Azure subscription.");
            Assert.IsTrue(File.Exists(publishSettingsFilePath), "Azure subscription settings file does not exist '{0}'.", publishSettingsFilePath);
        }

        [TestCleanup]
        public void Cleanup() {
            if (!string.IsNullOrEmpty(_cloudServiceToDelete)) {
                Assert.IsTrue(AzureUtility.DeleteCloudServiceWithRetry(publishSettingsFilePath, _cloudServiceToDelete), "Failed to delete cloud service.");
            }
        }

        public TestContext TestContext { get; set; }

        private void TestPublishToWebRole(
            string languageName,
            string templateName,
            string projectName,
            string expectedProjectItem,
            string textInResponse,
            int publishTimeout
        ) {
            using (var app = new VisualStudioApp()) {
                AzureWebSiteTests.CreateProject(
                    app,
                    languageName,
                    templateName,
                    TestData.GetTempPath(),
                    projectName,
                    expectedProjectItem
                );

                app.Dte.ExecuteCommand("Project.ConverttoWindowsAzureCloudServiceProject");

                _cloudServiceToDelete = Guid.NewGuid().ToString("N");
                var siteUri = app.PublishToAzureCloudService(_cloudServiceToDelete, publishSettingsFilePath);
                app.WaitForBuildComplete(publishTimeout);

                app.AzureActivityLog.WaitForPublishComplete(_cloudServiceToDelete, publishTimeout);

                string text = WebDownloadUtility.GetString(siteUri);
                Console.WriteLine("Response from {0}", siteUri);
                Console.WriteLine(text);
                Assert.IsTrue(text.Contains(textInResponse), text);
            }
        }

        const int JavaScriptWebAppPublishTimeout = 20 * 60 * 1000;

        [TestMethod, Priority(0), TestCategory("Core"), Timeout(JavaScriptWebAppPublishTimeout)]
        [HostType("VSTestHost")]
        public void JavaScriptWebAppPublish() {
            TestPublishToWebRole(
                NodejsVisualStudioApp.JavaScriptTemplateLanguageName,
                NodejsVisualStudioApp.JavaScriptAzureWebAppTemplate,
                "webproj",
                "server.js",
                "Hello World",
                JavaScriptWebAppPublishTimeout
            );
        }

        const int TypeScriptWebAppPublishTimeout = 20 * 60 * 1000;

        [TestMethod, Priority(0), TestCategory("Core"), Timeout(TypeScriptWebAppPublishTimeout)]
        [HostType("VSTestHost")]
        public void TypeScriptWebAppPublish() {
            TestPublishToWebRole(
                NodejsVisualStudioApp.TypeScriptTemplateLanguageName,
                NodejsVisualStudioApp.TypeScriptAzureWebAppTemplate,
                "typeproj",
                "server.ts",
                "Hello World",
                TypeScriptWebAppPublishTimeout
            );
        }
    }
}

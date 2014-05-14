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
using System.Threading;
using Microsoft.TC.TestHostAdapters;
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
            using (var app = new VisualStudioApp(VsIdeTestHostContext.Dte)) {
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
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
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
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
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

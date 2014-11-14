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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;
using TestUtilities.UI;
using TestUtilities.UI.Nodejs;

namespace AzurePublishingUITests {
    [TestClass]
    public class AzureWebSiteTests {
        private string _webSiteToDelete;
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
            if (!string.IsNullOrEmpty(_webSiteToDelete)) {
                Assert.IsTrue(AzureUtility.DeleteWebSiteWithRetry(publishSettingsFilePath, _webSiteToDelete));
            }
        }

        public TestContext TestContext { get; set; }

        internal static void CreateProject(VisualStudioApp app, string languageName, string templateName, string location, string projectName, string expectedProjectItem) {
            using (var newProjDialog = app.FileNewProject()) {
                newProjDialog.FocusLanguageNode(languageName);
                newProjDialog.Location = location;
                newProjDialog.ProjectName = projectName;

                var djangoApp = newProjDialog.ProjectTypes.FindItem(templateName);
                djangoApp.Select();
                newProjDialog.OK();
            }

            // wait for new solution to load...
            for (int i = 0; i < 40 && app.Dte.Solution.Projects.Count == 0; i++) {
                System.Threading.Thread.Sleep(250);
            }

            app.SolutionExplorerTreeView.WaitForItem(
                "Solution '" + app.Dte.Solution.Projects.Item(1).Name + "' (1 project)",
                app.Dte.Solution.Projects.Item(1).Name,
                expectedProjectItem
            );
        }

        private void TestPublishToWebSite(
            string languageName,
            string templateName,
            string projectName,
            string expectedProjectItem,
            string textInResponse,
            int publishTimeout
        ) {
            using (var app = new VisualStudioApp()) {
                CreateProject(
                    app,
                    languageName,
                    templateName,
                    TestData.GetTempPath(),
                    projectName,
                    expectedProjectItem
                );

                _webSiteToDelete = Guid.NewGuid().ToString("N");
                var siteUri = app.PublishToAzureWebSite(_webSiteToDelete, publishSettingsFilePath);
                app.WaitForBuildComplete(publishTimeout);

                string text = WebDownloadUtility.GetString(siteUri);
                Console.WriteLine("Response from {0}", siteUri);
                Console.WriteLine(text);
                Assert.IsTrue(text.Contains(textInResponse), text);
            }
        }

        const int JavaScriptWebAppPublishTimeout = 2 * 60 * 1000;

        [TestMethod, Priority(0), TestCategory("Core"), Timeout(JavaScriptWebAppPublishTimeout)]
        [HostType("VSTestHost")]
        public void JavaScriptWebAppPublish() {
            TestPublishToWebSite(
                NodejsVisualStudioApp.JavaScriptTemplateLanguageName,
                NodejsVisualStudioApp.JavaScriptAzureWebAppTemplate,
                "webproj",
                "server.js",
                "Hello World",
                JavaScriptWebAppPublishTimeout
            );
        }

        const int TypeScriptWebAppPublishTimeout = 2 * 60 * 1000;

        [TestMethod, Priority(0), TestCategory("Core"), Timeout(TypeScriptWebAppPublishTimeout)]
        [HostType("VSTestHost")]
        public void TypeScriptWebAppPublish() {
            TestPublishToWebSite(
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

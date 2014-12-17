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
using System.Windows.Automation;

namespace TestUtilities.UI {
    public class AzureWebSitePublishDialog : AutomationDialog {
        public AzureWebSitePublishDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
        }

        public static AzureWebSitePublishDialog FromDte(VisualStudioApp app) {
            var publishDialogHandle = app.OpenDialogWithDteExecuteCommand("Build.PublishSelection");
            return new AzureWebSitePublishDialog(app, AutomationElement.FromHandle(publishDialogHandle));
        }

        public AzureWebSiteImportPublishSettingsDialog ClickImportSettings() {
            WaitForInputIdle();
            ClickButtonByAutomationId("ImportSettings");
            return new AzureWebSiteImportPublishSettingsDialog(App, AutomationElement.FromHandle(App.WaitForDialogToReplace(Element)));
        }

        public void ClickPublish() {
            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(10.0), () => ClickButtonByAutomationId("PublishButton"));
        }
    }
}

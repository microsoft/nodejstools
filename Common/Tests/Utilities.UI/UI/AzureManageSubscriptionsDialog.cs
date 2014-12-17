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
    public class AzureManageSubscriptionsDialog : AutomationDialog {
        public AzureManageSubscriptionsDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
        }

        public void ClickCertificates() {
            WaitForInputIdle();
            CertificatesTab().Select();
        }

        public AzureImportSubscriptionDialog ClickImport() {
            WaitForInputIdle();
            ClickButtonByAutomationId("ImportButton");

            return new AzureImportSubscriptionDialog(App, AutomationElement.FromHandle(App.WaitForDialogToReplace(Element)));
        }

        public void ClickRemove() {
            WaitForInputIdle();
            var button = new Button(FindByAutomationId("DeleteButton"));
            WaitFor(button, btn => btn.Element.Current.IsEnabled);
            button.Click();
        }

        public void Close() {
            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(10.0), () => ClickButtonByName("Close"));
        }

        public ListBox SubscriptionsListBox {
            get {
                return new ListBox(FindByAutomationId("SubscriptionsListBox"));
            }
        }

        private AutomationElement CertificatesTab() {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "CertificatesTab"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem)
                )
            );
        }
    }
}

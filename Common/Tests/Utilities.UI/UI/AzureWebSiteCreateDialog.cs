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
    public class AzureWebSiteCreateDialog : AutomationDialog {
        public AzureWebSiteCreateDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
        }

        public void ClickCreate() {
            // Wait for the create button to be enabled
            WaitFor(CreateButton, btn => btn.Element.Current.IsEnabled);

            // Wait for Locations and Databases to have a selection
            // (the create button may be enabled before they are populated)
            WaitFor(LocationComboBox, combobox => combobox.GetSelectedItemName() != null);
            WaitFor(DatabaseComboBox, combobox => combobox.GetSelectedItemName() != null);

            WaitForInputIdle();
            WaitForClosed(TimeSpan.FromSeconds(180.0), () => CreateButton.Click());
        }

        public string SiteName {
            get {
                return GetSiteNameBox().GetValuePattern().Current.Value;
            }
            set {
                WaitForInputIdle();
                GetSiteNameBox().GetValuePattern().SetValue(value);
            }
        }

        private Button CreateButton {
            get {
                return new Button(FindByName("Create"));
            }
        }

        private ComboBox LocationComboBox {
            get {
                return new ComboBox(FindByAutomationId("_azureSiteLocation"));
            }
        }

        private ComboBox DatabaseComboBox {
            get {
                return new ComboBox(FindByAutomationId("_azureDatabaseServer"));
            }
        }

        private AutomationElement GetSiteNameBox() {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "_azureSiteName"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
                )
            );
        }
    }
}

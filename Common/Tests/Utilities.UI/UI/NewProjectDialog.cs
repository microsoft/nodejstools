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
    /// <summary>
    /// Wrapps VS's Project->Add Item dialog.
    /// </summary>
    public class NewItemDialog : AutomationDialog {
        private readonly VisualStudioApp _app;
        private Table _projectTypesTable;

        public NewItemDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
            _app = app;
        }

        public static NewItemDialog FromDte(VisualStudioApp app) {
            return new NewItemDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Project.AddNewItem"))
            );
        }

        /// <summary>
        /// Clicks the OK button on the dialog.
        /// </summary>
        public override void OK() {
            ClickButtonAndClose("btn_OK", nameIsAutomationId: true);
        }

        /// <summary>
        /// Gets the project types table which enables selecting an individual project type.
        /// </summary>
        public Table ProjectTypes {
            get {
                if (_projectTypesTable == null) {
                    var extensions = Element.FindAll(
                        TreeScope.Descendants,
                        new PropertyCondition(
                            AutomationElement.AutomationIdProperty,
                            "lvw_Extensions"
                        )
                    );

                    if (extensions.Count != 1) {
                        throw new Exception("multiple controls match");
                    }
                    _projectTypesTable = new Table(extensions[0]);

                }
                return _projectTypesTable;
            }
        }

        public string FileName {
            get {
                var patterns = GetFileNameBox().GetSupportedPatterns();
                var filename = (ValuePattern)GetFileNameBox().GetCurrentPattern(ValuePattern.Pattern);
                return filename.Current.Value;
            }
            set {
                var patterns = GetFileNameBox().GetSupportedPatterns();
                var filename = (ValuePattern)GetFileNameBox().GetCurrentPattern(ValuePattern.Pattern);
                filename.SetValue(value);
            }
        }

        private AutomationElement GetFileNameBox() {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "txt_Name"),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit)
                )
            );
        }
    }
}

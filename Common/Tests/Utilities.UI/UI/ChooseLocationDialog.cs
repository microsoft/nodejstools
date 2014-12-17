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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI {
    public class ChooseLocationDialog : AutomationDialog {
        public ChooseLocationDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
        }

        public static ChooseLocationDialog FromDte(VisualStudioApp app) {
            return new ChooseLocationDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("File.ProjectPickerMoveInto"))
            );
        }

        public void SelectProject(string name) {
            var item = Element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, name));
            Assert.IsNotNull(item, "Did not find item " + name);
            item.GetSelectionItemPattern().Select();
        }

    }
}

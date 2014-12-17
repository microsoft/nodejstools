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
    /// Wraps the Delete/Remove/Cancel dialog displayed when removing something from a hierarchy window (such as the solution explorer).
    /// </summary>
    public class RemoveItemDialog : AutomationDialog {
        public RemoveItemDialog(IntPtr hwnd)
            : base(null, AutomationElement.FromHandle(hwnd)) {
        }

        public RemoveItemDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
        }

        public static RemoveItemDialog FromDte(VisualStudioApp app) {
            return new RemoveItemDialog(app, AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Edit.Delete")));
        }

        public override void OK() {
            throw new NotSupportedException();
        }

        public void Remove() {
            WaitForInputIdle();
            WaitForClosed(DefaultTimeout, () => ClickButtonByName("Remove"));
        }

        public void Delete() {
            WaitForInputIdle();
            WaitForClosed(DefaultTimeout, () => ClickButtonByName("Delete"));
        }
    }
}

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
using System.Threading;
using System.Windows.Automation;
using System.Windows.Input;

namespace TestUtilities.UI {
    public class SaveDialog : AutomationDialog {
        public SaveDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
        }

        public static SaveDialog FromDte(VisualStudioApp app) {
            return new SaveDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("File.SaveSelectedItemsAs"))
            );
        }

        public void Save() {
            WaitForInputIdle();
            // The Save button on this dialog is broken and so UIA cannot invoke
            // it (though somehow Inspect is able to...). We use the keyboard
            // instead.
            WaitForClosed(DefaultTimeout, () => Keyboard.PressAndRelease(Key.S, Key.LeftAlt));
        }

        public override void OK() {
            Save();
        }

        public string FileName { 
            get {
                return GetFilenameEditBox().GetValuePattern().Current.Value;
            }
            set {
                GetFilenameEditBox().GetValuePattern().SetValue(value);
            }
        }

        private AutomationElement GetFilenameEditBox() {
            return FindByAutomationId("FileNameControlHost");
        }
    }
}

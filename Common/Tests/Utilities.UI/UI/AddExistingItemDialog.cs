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

using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;

namespace TestUtilities.UI {
    public class AddExistingItemDialog : AutomationDialog {
        public AddExistingItemDialog(VisualStudioApp app, AutomationElement element)
            : base(app, element) {
        }

        public static AddExistingItemDialog FromDte(VisualStudioApp app) {
            return new AddExistingItemDialog(
                app,
                AutomationElement.FromHandle(app.OpenDialogWithDteExecuteCommand("Project.AddExistingItem"))
            );
        }

        public override void OK() {
            Add();
        }

        public void Add() {
            WaitForClosed(DefaultTimeout, () => Keyboard.PressAndRelease(Key.A, Key.LeftAlt));
        }

        public void AddLink() {
            var addButton = Element.FindFirst(TreeScope.Children,
                new AndCondition(
                    new PropertyCondition(AutomationElement.NameProperty, "Add"),
                    new PropertyCondition(AutomationElement.ClassNameProperty, "Button")
                )
            );

            // click the chevron to open the menu
            var bottomRight = addButton.Current.BoundingRectangle.BottomRight;
            Mouse.MoveTo(new Point(bottomRight.X - 3, bottomRight.Y - 3));

            Mouse.Click(MouseButton.Left);

            // type the keyboard short cut for Add to Link
            Keyboard.Type(Key.L);

            WaitForClosed(DefaultTimeout);
        }

        public string FileName { 
            get {
                var filename = (ValuePattern)GetFilenameEditBox().GetCurrentPattern(ValuePattern.Pattern);
                return filename.Current.Value;
            }
            set { 
                var filename = (ValuePattern)GetFilenameEditBox().GetCurrentPattern(ValuePattern.Pattern);
                filename.SetValue(value);
            }
        }

        private AutomationElement GetFilenameEditBox() {
            return Element.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.ClassNameProperty, "Edit"),
                    new PropertyCondition(AutomationElement.NameProperty, "File name:")
                )
            );
        }
    }
}

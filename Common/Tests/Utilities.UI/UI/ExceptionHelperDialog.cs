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

using System.Windows.Automation;

namespace TestUtilities.UI {
    public class ExceptionHelperDialog : AutomationWrapper {
        public ExceptionHelperDialog(AutomationElement element)
            : base(element) {
        }

        public string Title {
            get {
                // this is just the 1st child pane, and it's name is the same as the text it has.
                var exceptionNamePane = Element.FindFirst(
                    TreeScope.Children,
                    new PropertyCondition(
                        AutomationElement.ControlTypeProperty,
                        ControlType.Pane
                    )
                );

                return exceptionNamePane.Current.Name;
            }
        }

        public string Description {
            get {
                var desc = FindByName("Exception Description:");
                return (((TextPattern)desc.GetCurrentPattern(TextPattern.Pattern)).DocumentRange.GetText(-1).ToString());
            }
            
        }

        public void Cancel() {
            ClickButtonByName("Cancel Button");
        }
    }
}

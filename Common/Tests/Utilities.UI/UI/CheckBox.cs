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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI {
    public class CheckBox : AutomationWrapper {
        public string Name { get; set; }

        public CheckBox(AutomationElement element, CheckListView parent)
            : base(element) {
            Name = (string)Element.GetCurrentPropertyValue(AutomationElement.NameProperty);
        }

        public void SetSelected() {
            Assert.IsTrue((bool)Element.GetCurrentPropertyValue(AutomationElement.IsTogglePatternAvailableProperty), "Element is not a check box");
            TogglePattern pattern = (TogglePattern)Element.GetCurrentPattern(TogglePattern.Pattern);

            if (pattern.Current.ToggleState != ToggleState.On) pattern.Toggle();
            if (pattern.Current.ToggleState != ToggleState.On) pattern.Toggle();

            Assert.AreEqual(pattern.Current.ToggleState, ToggleState.On, "Could not toggle " + Name + " to On.");
        }

        public void SetUnselected() {
            Assert.IsTrue((bool)Element.GetCurrentPropertyValue(AutomationElement.IsTogglePatternAvailableProperty), "Element is not a check box");
            TogglePattern pattern = (TogglePattern)Element.GetCurrentPattern(TogglePattern.Pattern);

            if (pattern.Current.ToggleState != ToggleState.Off) pattern.Toggle();
            if (pattern.Current.ToggleState != ToggleState.Off) pattern.Toggle();
            Assert.AreEqual(pattern.Current.ToggleState, ToggleState.Off, "Could not toggle " + Name + " to Off.");
        }
    }
}

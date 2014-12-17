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
    public class ListBox : AutomationWrapper {
        public ListBox(AutomationElement element)
            : base(element) {
        }

        public ListBoxItem this[int index] {
            get {
                var items = FindAllByControlType(ControlType.ListItem);
                Assert.IsTrue(0 <= index && index < items.Count, "Index {0} is out of range of item count {1}", index, items.Count);
                return new ListBoxItem(items[index], this);
            }
        }

        public int Count {
            get {
                var items = FindAllByControlType(ControlType.ListItem);
                return items.Count;
            }
        }
    }
}

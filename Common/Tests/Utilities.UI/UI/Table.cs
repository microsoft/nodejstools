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
    public class Table : AutomationWrapper {
        private readonly GridPattern _pattern;

        public Table(AutomationElement element)
            : base(element) {
                _pattern = (GridPattern)element.GetCurrentPattern(GridPattern.Pattern);

        }

        public AutomationElement this[int row, int column] {
            get {
                return _pattern.GetItem(row, column);
            }
        }

        public AutomationElement FindItem(string name) {
            return Element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, name));
        }
    }
}

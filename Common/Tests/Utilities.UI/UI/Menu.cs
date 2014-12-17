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

using System.Collections.Generic;
using System.Windows.Automation;

namespace TestUtilities.UI
{
    class Menu : AutomationWrapper
    {
        public Menu(AutomationElement element)
            : base(element) {
        }

        public List<MenuItem> Items
        {
            get
            {
                Condition con = new PropertyCondition(
                                    AutomationElement.LocalizedControlTypeProperty,
                                    "menu item"
                                );
                AutomationElementCollection ell = Element.FindAll(TreeScope.Children, con);
                List<MenuItem> items = new List<MenuItem>();
                for (int i = 0; i < ell.Count; i++)
                {
                    items.Add(new MenuItem(ell[i]));
                }
                return items;
            }
        }
    }
}

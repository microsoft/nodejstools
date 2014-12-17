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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI {
    public class Header : AutomationWrapper {
        private Dictionary<string, int> _columns = new Dictionary<string, int>();
        public Dictionary<string, int> Columns {
            get {
                return _columns;
            }
        }

        public Header(AutomationElement element) : base(element) {
            AutomationElementCollection headerItems = FindAllByControlType(ControlType.HeaderItem);
            for (int i = 0; i < headerItems.Count; i++) {
                string colName = headerItems[i].GetCurrentPropertyValue(AutomationElement.NameProperty) as string;
                if (colName != null && !_columns.ContainsKey(colName)) _columns[colName] = i;
            }            
        }

        public int this[string colName] {
            get {
                Assert.IsTrue(_columns.ContainsKey(colName), "Header does not define header item {0}", colName);
                return _columns[colName];
            }
        }
        
    }
}

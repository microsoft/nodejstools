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
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestUtilities.UI
{
    class ProjectPropertiesWindow : AutomationWrapper
    {
        public ProjectPropertiesWindow(IntPtr element)
            : base(AutomationElement.FromHandle(element)) { 
        }

        public AutomationElement this[Guid tabGuid] {
            get {
                
                var tabItem = FindByAutomationId("PropPage_" + tabGuid.ToString("n").ToLower());
                Assert.IsNotNull(tabItem, "Failed to find page");
                
                AutomationWrapper.DumpElement(tabItem);
                foreach (var p in tabItem.GetSupportedPatterns()) {
                    Console.WriteLine("Supports {0}", p.ProgrammaticName);
                }

                try {
                    tabItem.GetInvokePattern().Invoke();
                } catch (InvalidOperationException) {
                    AutomationWrapper.DoDefaultAction(tabItem);
                }

                return FindByAutomationId("PageHostingPanel");
            }
        }
    }
}

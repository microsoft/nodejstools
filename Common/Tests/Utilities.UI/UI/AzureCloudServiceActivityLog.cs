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

namespace TestUtilities.UI {
    public class AzureCloudServiceActivityLog : AutomationWrapper {
        public AzureCloudServiceActivityLog(AutomationElement element)
            : base(element) {
        }

        public void WaitForPublishComplete(string serviceName, int timeout) {
            var rowElement = WaitForRow(serviceName, 5000);
            WaitForRowComplete(rowElement, timeout);
        }

        private AutomationElement WaitForRow(string serviceName, int timeout) {
            const int interval = 200;
            for (int i = 0; i < timeout; i += interval) {
                var res = FindRowElement(serviceName);
                if (res != null) {
                    return res;
                }

                Thread.Sleep(interval);
            }

            throw new TimeoutException("Timed out waiting for publish to start.");
        }

        private void WaitForRowComplete(AutomationElement row, int timeout) {
            const int interval = 1000;
            for (int i = 0; i < timeout; i += interval) {
                if (HasCompleted(row)) {
                    return;
                }

                Thread.Sleep(interval);
            }

            throw new TimeoutException("Timed out waiting for publish to complete.");
        }

        private AutomationElement FindRowElement(string serviceName) {
            var rows = Element.FindAll(TreeScope.Descendants, new AndCondition(
                new PropertyCondition(
                    AutomationElement.ClassNameProperty,
                    "DataGridRow"
                ),

                new PropertyCondition(
                    AutomationElement.NameProperty,
                    "Microsoft.Cct.StatusWindow.DispatchedStatusItemContainer"
                )
            ));

            foreach (AutomationElement row in rows) {
                var columns = row.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "DataGridCell")
                );

                foreach (AutomationElement column in columns) {
                    if (column.Current.Name.Contains(serviceName)) {
                        return row;
                    }
                }
            }

            return null;
        }

        private bool HasCompleted(AutomationElement row) {
            return row.FindFirst(TreeScope.Descendants, new AndCondition(
                new PropertyCondition(AutomationElement.NameProperty, "Completed"),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text)
            )) != null;
        }
    }
}

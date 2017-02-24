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
using System.Windows.Automation;

namespace TestUtilities.UI.Nodejs
{
    public class ComparePerfReports : AutomationWrapper
    {
        public ComparePerfReports(IntPtr hwnd)
            : base(AutomationElement.FromHandle(hwnd))
        {
        }

        public void Ok()
        {
            ClickButtonByName("OK");
        }

        public void Cancel()
        {
            ClickButtonByName("Cancel");
        }

        public string ComparisonFile
        {
            get
            {
                return ComparisonFileTextBox.GetValue();
            }
            set
            {
                ComparisonFileTextBox.SetValue(value);
            }
        }

        private AutomationWrapper ComparisonFileTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("ComparisonFile"));
            }
        }

        public string BaselineFile
        {
            get
            {
                return BaselineFileTextBox.GetValue();
            }
            set
            {
                BaselineFileTextBox.SetValue(value);
            }
        }

        private AutomationWrapper BaselineFileTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("BaselineFile"));
            }
        }
    }
}

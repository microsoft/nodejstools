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

namespace TestUtilities.UI
{
    class MenuItem : AutomationWrapper
    {
        public MenuItem(AutomationElement element)
            : base(element) {
        }

        public string Value
        {
            get
            {
                return this.Element.Current.Name.ToString();
            }
        }

        public bool ToggleStatus
        {
            get
            {
                var pat = (TogglePattern)Element.GetCurrentPattern(TogglePattern.Pattern);
                if (pat.Current.ToggleState == ToggleState.On)
                    return true;
                return false;
            }
        }

        public void Check()
        {
            var pat = (TogglePattern)Element.GetCurrentPattern(TogglePattern.Pattern);
            if (pat.Current.ToggleState == ToggleState.Off)
            {
                try
                {
                    pat.Toggle();
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        public void Uncheck()
        {
            var pat = (TogglePattern)Element.GetCurrentPattern(TogglePattern.Pattern);
            if (pat.Current.ToggleState == ToggleState.On)
            {
                try
                {
                    pat.Toggle();
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }
    }
}

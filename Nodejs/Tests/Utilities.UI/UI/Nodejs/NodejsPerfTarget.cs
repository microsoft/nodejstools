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
    public class NodejsPerfTarget : AutomationWrapper
    {
        public NodejsPerfTarget(IntPtr hwnd)
            : base(AutomationElement.FromHandle(hwnd))
        {
        }

        /// <summary>
        /// Checks the Profile Project radio box
        /// </summary>
        public void SelectProfileProject()
        {
            Select(FindByAutomationId("ProfileProject"));
        }

        /// <summary>
        /// Checks the Profile Script radio box
        /// </summary>
        public void SelectProfileScript()
        {
            var elem = FindByAutomationId("ProfileScript");
            var pats = elem.GetSupportedPatterns();
            string[] names = new string[pats.Length];
            for (int i = 0; i < pats.Length; i++)
            {
                names[i] = pats[i].ProgrammaticName;
            }
            Select(FindByAutomationId("ProfileScript"));
        }

        public string SelectedProject
        {
            get
            {
                return SelectedProjectComboBox.GetSelectedItemName();
            }
        }

        public ComboBox SelectedProjectComboBox
        {
            get
            {
                return new ComboBox(FindByAutomationId("Project"));
            }
        }

        /// <summary>
        /// Returns the string the user entered into the interpreter combo box.
        /// </summary>
        public string InterpreterPath
        {
            get
            {
                return InterpreterPathTextBox.GetValue();
            }
            set
            {
                InterpreterPathTextBox.SetValue(value);
            }
        }

        private AutomationWrapper InterpreterPathTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("Standalone.InterpreterPath"));
            }
        }

        public string WorkingDir
        {
            get
            {
                return WorkingDirectoryTextBox.GetValue();
            }
            set
            {
                WorkingDirectoryTextBox.SetValue(value);
            }
        }

        private AutomationWrapper WorkingDirectoryTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("Standalone.WorkingDirectory"));
            }
        }

        public string ScriptName
        {
            get
            {
                return ScriptNameTextBox.GetValue();
            }
            set
            {
                ScriptNameTextBox.SetValue(value);
            }
        }

        private AutomationWrapper ScriptNameTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("Standalone.ScriptPath"));
            }
        }

        public string Arguments
        {
            get
            {
                return ArgumentsTextBox.GetValue();
            }
            set
            {
                ArgumentsTextBox.SetValue(value);
            }
        }

        private AutomationWrapper ArgumentsTextBox
        {
            get
            {
                return new AutomationWrapper(FindByAutomationId("Standalone.Arguments"));
            }
        }

        public void Ok()
        {
            Invoke(FindButton("Ok"));
        }

        public void Cancel()
        {
            Invoke(FindButton("Cancel"));
        }
    }
}

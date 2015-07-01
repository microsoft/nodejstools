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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.VisualStudioTools;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.ProjectWizard {
    public sealed class WindowsSDKWizard : IWizard {
        public void ProjectFinishedGenerating(EnvDTE.Project project) {}
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) { }
        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) { }
        public void RunFinished() { }

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams
        ) {
            string winSDKVersion = string.Empty;
            try {
                string keyValue = string.Empty;
                // Attempt to get the installation folder of the Windows 10 SDK
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows Kits\Installed Roots");
                if (null != key) {
                    keyValue = (string)key.GetValue("KitsRoot10") + "Include";
                }
                // Get the latest SDK version from the name of the directory in the Include path of the SDK installation
                if (!string.IsNullOrEmpty(keyValue)) {
                    string dirName = Directory.GetDirectories(keyValue).OrderByDescending(x => x).FirstOrDefault();
                    winSDKVersion = Path.GetFileName(dirName);
                }
            } catch(Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }
            }
            
            if(string.IsNullOrEmpty(winSDKVersion)){
                winSDKVersion = "10.0.0.0"; // Default value to put in project file
            }

            replacementsDictionary.Add("$winsdkversion$", winSDKVersion);
            replacementsDictionary.Add("$winsdkminversion$", winSDKVersion);
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }
    }
}
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
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    class NodejsPackageParametersExtension : IWizard {
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            var projectName = replacementsDictionary["$projectname$"];

            // Remove all leading url-invalid, underscore, and period characters from the string
            var npmProjectNameTransform = Regex.Replace(projectName, "^[^a-zA-Z0-9-~]*", string.Empty);

            // Replace all invalid characters with a dash
            npmProjectNameTransform = Regex.Replace(npmProjectNameTransform, "[^a-zA-Z0-9-_~.]", "-");

            replacementsDictionary.Add("$npmsafeprojectname$", npmProjectNameTransform);
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
            return;
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {
            return;
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem) {
            return;
        }

        public void RunFinished() {
            return;
        }
    }
}

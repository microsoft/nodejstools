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
using System.Diagnostics;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    /// <summary>
    /// Provides a project wizard extension which will optionally do an
    /// npm install after the project is created.
    /// </summary>
    public sealed class NpmWizardExtension : IWizard {
        #region IWizard Members

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
            Debug.Assert(project != null && project.Object != null);
            Debug.Assert(project.Object is INodePackageModulesCommands);

            ((INodePackageModulesCommands)project.Object).InstallMissingModulesAsync();
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) {
        }

        public void RunFinished() {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
#if DEV14
            replacementsDictionary.Add("$dev14$", "true");
#endif
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }

#endregion
    }
}

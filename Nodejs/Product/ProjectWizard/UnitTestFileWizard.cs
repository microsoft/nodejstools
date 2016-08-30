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
using EnvDTE;
using Microsoft.NodejsTools.TestFrameworks;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    public sealed class UnitTestFileWizard : IWizard {
        private string _framework;

        public void BeforeOpeningFile(ProjectItem projectItem) {
            EnvDTE.Project project = projectItem.ContainingProject;
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) { }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {
            EnvDTE.Property property = projectItem.Properties.Item("TestFramework");
            property.Value = _framework;
        }

        public void RunFinished() { }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            if (!replacementsDictionary.TryGetValue("$wizarddata$", out _framework) ||
                String.IsNullOrWhiteSpace(_framework)) {
                _framework = TestFrameworkDirectories.ExportRunnerFramework;
            }
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }
    }
}


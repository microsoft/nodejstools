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
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.ProjectWizard {
    /// <summary>
    /// Provides a project wizard extension which will replace the node arguments
    /// in a Dockerfile with properties from the project file.
    /// </summary>
    public sealed class DockerfileWizardExtension : IWizard {
        public void BeforeOpeningFile(ProjectItem projectItem) {
            return;
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
            return;
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {
            return;
        }

        public void RunFinished() {
            return;
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            string arguments = GetFullNodeArguments(automationObject);
            replacementsDictionary.Add("$nodearguments$", arguments);
        }

        private static string GetFullNodeArguments(object automationObject) {
            string arguments = null;
            if (automationObject is DTE) {
                DTE dte = (DTE)automationObject;
                Array activeProjects = (Array)dte.ActiveSolutionProjects;

                if (activeProjects.Length > 0) {
                    EnvDTE.Project activeProject = (EnvDTE.Project)activeProjects.GetValue(0);
                    string startupFileName = CommonUtils.GetRelativeFilePath(
                        (string)activeProject.Properties.Item("ProjectHome").Value,
                        (string)activeProject.Properties.Item("StartupFile").Value ?? string.Empty).Replace("\\", "/");

                    string nodeExeArguments = (string)activeProject.Properties.Item("NodeExeArguments").Value ?? string.Empty;
                    string scriptArguments = (string)activeProject.Properties.Item("ScriptArguments").Value ?? string.Empty;
                    
                    arguments = String.Format("{0} {1} {2}",
                        nodeExeArguments.Trim(), 
                        startupFileName.Trim(), 
                        scriptArguments.Trim()
                    ).Trim();
                }
            }
            return arguments;
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }
    }
}

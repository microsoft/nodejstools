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
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    public sealed class NewProjectFromExistingWizard : IWizard {
        public static Boolean IsAddNewProjectCmd { get; set; }
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) { }
        public void ProjectFinishedGenerating(EnvDTE.Project project) { }
        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) { }
        public void RunFinished() { }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            try {
                Directory.Delete(replacementsDictionary["$destinationdirectory$"]);
                Directory.Delete(replacementsDictionary["$solutiondirectory$"]);
            } catch {
                // If it fails (doesn't exist/contains files/read-only), let the directory stay.
            }

            var dte = automationObject as DTE;
            if (dte == null) {
                var provider = automationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
                if (provider != null) {
                    dte = new ServiceProvider(provider).GetService(typeof(DTE)) as DTE;
                }
            }

            bool addingNewProject = false;
            if (dte == null) {
                MessageBox.Show("Unable to start wizard: no automation object available.", "Node.js Tools for Visual Studio");
            } else {
                // https://nodejstools.codeplex.com/workitem/462
                // we need to make sure our package is loaded before invoking our command
                Guid packageGuid = new Guid(Guids.NodejsPackageString);
                IVsPackage package;
                ((IVsShell)Package.GetGlobalService(typeof(SVsShell))).LoadPackage(
                    ref packageGuid,
                    out package
                );

                System.Threading.Tasks.Task.Factory.StartNew(() => {
                    string projName = replacementsDictionary["$projectname$"];
                    string solnName = replacementsDictionary["$specifiedsolutionname$"];
                    string directory;
                    if (String.IsNullOrWhiteSpace(solnName)) {
                        // Create directory is unchecked, destinationdirectory is the
                        // directory name the user entered plus the project name, we want
                        // to remove the project name.
                        directory = Path.GetDirectoryName(replacementsDictionary["$destinationdirectory$"]);
                    } else {
                        // Create directory is checked, the destinationdirectory is the
                        // directory the user entered plus the project name plus the
                        // solution name - we want to remove both extra folders
                        directory = Path.GetDirectoryName(Path.GetDirectoryName(replacementsDictionary["$destinationdirectory$"]));
                    }

                    var context = addingNewProject ? 
                        (int)VSConstants.VSStd97CmdID.AddExistingProject : 
                        (int)VSConstants.VSStd97CmdID.OpenProject;
                    object inObj = projName + "|" + directory + "|" + context, outObj = null;
                    dte.Commands.Raise(Guids.NodejsCmdSet.ToString("B"), (int)PkgCmdId.cmdidImportWizard, ref inObj, ref outObj);
                });
            }
            addingNewProject = IsAddNewProjectCmd;
            throw new WizardCancelledException();
        }

        public bool ShouldAddProjectItem(string filePath) {
            return false;
        }
    }
}

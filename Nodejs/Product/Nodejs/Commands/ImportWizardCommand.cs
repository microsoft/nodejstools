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
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands {
    /// <summary>
    /// Provides the command to import a project from existing code.
    /// </summary>
    class ImportWizardCommand : Command {

        public override void DoCommand(object sender, EventArgs args) {
            var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
            statusBar.SetText("Importing project...");

            var dlg = new Microsoft.NodejsTools.Project.ImportWizard.ImportWizard();
            int commandIdToRaise = (int)VSConstants.VSStd97CmdID.OpenProject;
            
            Microsoft.VisualStudio.Shell.OleMenuCmdEventArgs oleArgs = args as Microsoft.VisualStudio.Shell.OleMenuCmdEventArgs;
            if (oleArgs != null) {
                string projectArgs = oleArgs.InValue as string;
                if (projectArgs != null) {
                    var argItems = projectArgs.Split('|');
                    if (argItems.Length == 3) {
                        dlg.ImportSettings.ProjectPath = CommonUtils.GetAvailableFilename(
                            argItems[1], 
                            argItems[0], 
                            ".njsproj"
                        );
                        dlg.ImportSettings.SourcePath = argItems[1];
                        commandIdToRaise = int.Parse(argItems[2]);
                    }
                }
            }
            
            if (dlg.ShowModal() ?? false) {
                var settings = dlg.ImportSettings;

                settings.CreateRequestedProjectAsync()
                    .ContinueWith(t => {
                        string path;
                        try {
                            path = t.Result;
                        } catch (AggregateException ex) {
                            if (ex.InnerException is UnauthorizedAccessException) {
                                MessageBox.Show(
                                    "Some file paths could not be accessed." + Environment.NewLine +
                                    "Try moving your source code to a location where you " +
                                    "can read and write files.",
                                    SR.ProductName
                                );
                            } else {
                                string exName = String.Empty;
                                if (ex.InnerException != null) {
                                    exName = "(" + ex.InnerException.GetType().Name + ") ";
                                }

                                MessageBox.Show(
                                    "An unexpected error " + exName +
                                    "occurred while creating your project.",
                                    SR.ProductName
                                );
                            }
                            return;
                        }
                        if (File.Exists(path)) {
                            object outRef = null, pathRef = "\"" + path + "\"";
                            NodejsPackage.Instance.DTE.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString("B"), commandIdToRaise, ref pathRef, ref outRef);
                            statusBar.SetText(String.Empty);
                        } else {
                            statusBar.SetText("An error occurred and your project was not created.");
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.HideScheduler,
                    System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            } else {
                statusBar.SetText("");
            }
        }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidImportWizard; }
        }
    }
}

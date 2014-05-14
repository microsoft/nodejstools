/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.IO;
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
            
            Microsoft.VisualStudio.Shell.OleMenuCmdEventArgs oleArgs = args as Microsoft.VisualStudio.Shell.OleMenuCmdEventArgs;
            if (oleArgs != null) {
                string projectArgs = oleArgs.InValue as string;
                if (projectArgs != null) {
                    var argItems = projectArgs.Split('|');
                    if (argItems.Length == 2) {
                        dlg.ImportSettings.ProjectPath = CommonUtils.GetAvailableFilename(
                            argItems[1], 
                            argItems[0], 
                            ".njsproj"
                        );
                        dlg.ImportSettings.SourcePath = argItems[1];
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
                                string exName = "";
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
                            NodejsPackage.Instance.DTE.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString("B"), (int)VSConstants.VSStd97CmdID.OpenProject, ref pathRef, ref outRef);
                            statusBar.SetText("");
                        } else {
                            statusBar.SetText("An error occurred and your project was not created.");
                        }
                    }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            } else {
                statusBar.SetText("");
            }
        }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidImportWizard; }
        }
    }
}

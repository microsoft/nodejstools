// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.ProjectWizard;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands
{
    /// <summary>
    /// Provides the command to import a project from existing code.
    /// </summary>
    internal class ImportWizardCommand : Command
    {
        public override void DoCommand(object sender, EventArgs args)
        {
            var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
            statusBar.SetText(Resources.ImportingProjectStatusText);

            var dlg = new Project.ImportWizard.ImportWizard();
            var commandIdToRaise = (int)VSConstants.VSStd97CmdID.OpenProject;

            if (args is VisualStudio.Shell.OleMenuCmdEventArgs oleArgs)
            {
                if (oleArgs.InValue is string projectArgs)
                {
                    var argItems = projectArgs.Split('|');
                    Debug.Assert(argItems.Length == 4, "expected 4 arguments");

                    dlg.ImportSettings.ProjectName = argItems[0];

                    var projectLanguage = ProjectLanguage.JavaScript;
                    if (int.TryParse(argItems[3], out var langId))
                    {
                        projectLanguage = (ProjectLanguage)langId;
                    }

                    dlg.ImportSettings.SourcePath = argItems[1];
                    dlg.ImportSettings.ProjectLanguage = projectLanguage;
                    commandIdToRaise = int.Parse(argItems[2], CultureInfo.InvariantCulture);
                }
            }

            if (dlg.ShowModal() ?? false)
            {
                var settings = dlg.ImportSettings;

                settings.CreateRequestedProjectAsync()
                    .ContinueWith(t =>
                    {
                        string path;
                        try
                        {
                            path = t.Result;
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerException is UnauthorizedAccessException)
                            {
                                MessageBox.Show(
                                    string.Format(CultureInfo.CurrentCulture, Resources.ImportingProjectAccessErrorStatusText, Environment.NewLine),
                                    SR.ProductName);
                            }
                            else
                            {
                                var exName = string.Empty;
                                if (ex.InnerException != null)
                                {
                                    exName = "(" + ex.InnerException.GetType().Name + ")";
                                }

                                MessageBox.Show(
                                    string.Format(CultureInfo.CurrentCulture, Resources.ImportingProjectUnexpectedErrorMessage, exName),
                                    SR.ProductName);
                            }
                            return;
                        }
                        if (File.Exists(path))
                        {
                            object outRef = null, pathRef = "\"" + path + "\"";
                            NodejsPackage.Instance.DTE.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString("B"), commandIdToRaise, ref pathRef, ref outRef);
                            statusBar.SetText(string.Empty);
                        }
                        else
                        {
                            statusBar.SetText(Resources.ImportingProjectErrorStatusText);
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.HideScheduler,
                    System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                statusBar.SetText("");
            }
        }

        public override int CommandId => (int)PkgCmdId.cmdidImportWizard;
    }
}

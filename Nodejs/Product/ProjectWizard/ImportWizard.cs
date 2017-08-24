// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.ProjectWizard
{
    public sealed class NewProjectFromExistingWizard : IWizard
    {
        public static bool IsAddNewProjectCmd { get; set; }
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) { }
        public void ProjectFinishedGenerating(EnvDTE.Project project) { }
        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) { }
        public void RunFinished() { }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            var provider = automationObject as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
            if (provider == null)
            {
                MessageBox.Show(ProjectWizardResources.ErrorNoDte, SR.ProductName);
                throw new WizardBackoutException();
            }

            using (var serviceProvider = new ServiceProvider(provider))
            {
                var hr = EnsurePackageLoaded(serviceProvider);
                if (ErrorHandler.Failed(hr))
                {
                    MessageBox.Show(string.Format(ProjectWizardResources.ImportWizardCouldNotStartFailedToLoadPackage, hr), "Visual Studio");
                    throw new WizardBackoutException();
                }
                var uiShell = (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));

                var projName = replacementsDictionary["$projectname$"];
                replacementsDictionary.TryGetValue("$specifiedsolutionname$", out var solnName);
                string directory;
                if (string.IsNullOrWhiteSpace(solnName))
                {
                    // Create directory is unchecked, destinationdirectory is the
                    // directory name the user entered plus the project name, we want
                    // to remove the solution directory.
                    directory = Path.GetDirectoryName(replacementsDictionary["$destinationdirectory$"]);
                }
                else
                {
                    // Create directory is checked, the destinationdirectory is the
                    // directory the user entered plus the project name plus the
                    // solution name - we want to remove both extra folders
                    directory = Path.GetDirectoryName(Path.GetDirectoryName(replacementsDictionary["$destinationdirectory$"]));
                }

                var context = IsAddNewProjectCmd ? (int)VSConstants.VSStd97CmdID.AddExistingProject : (int)VSConstants.VSStd97CmdID.OpenProject;

                object inObj = projName + "|" + directory + "|" + context;
                var guid = Guids.NodejsCmdSet;
                hr = uiShell.PostExecCommand(ref guid, (uint)PkgCmdId.cmdidImportWizard, 0, ref inObj);
                if (ErrorHandler.Failed(hr))
                {
                    MessageBox.Show(string.Format(ProjectWizardResources.ImportWizardCouldNotStartUnexpectedError, hr), "Visual Studio");
                }
            }

            throw new WizardCancelledException();
        }

        private static int EnsurePackageLoaded(IServiceProvider serviceProvider)
        {
            var shell = (IVsShell)serviceProvider.GetService(typeof(SVsShell));
            var pkgGuid = new Guid(Guids.NodejsPackageString);

            if (ErrorHandler.Failed(shell.IsPackageLoaded(ref pkgGuid, out var pkg)) || pkg == null)
            {
                return shell.LoadPackage(ref pkgGuid, out pkg);
            }
            return VSConstants.S_OK;
        }

        public bool ShouldAddProjectItem(string filePath) => false;
    }
}

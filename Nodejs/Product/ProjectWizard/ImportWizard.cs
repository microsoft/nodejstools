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
    public enum ProjectLanguage
    {
        JavaScript,
        TypeScript,
    }

    public sealed class NewProjectFromExistingJavaScriptWizard : NewProjectFromExistingWizard
    {
        public NewProjectFromExistingJavaScriptWizard()
            : base(ProjectLanguage.JavaScript)
        {
            // ...
        }
    }

    public sealed class NewProjectFromExistingTypeScriptWizard : NewProjectFromExistingWizard
    {
        public NewProjectFromExistingTypeScriptWizard()
            : base(ProjectLanguage.TypeScript)
        {
            // ...
        }
    }

    public abstract class NewProjectFromExistingWizard : IWizard
    {
        protected NewProjectFromExistingWizard(ProjectLanguage language)
        {
            this.projectLanguage = language;
        }

        private readonly ProjectLanguage projectLanguage;

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
                    MessageBox.Show(string.Format(ProjectWizardResources.ImportWizardCouldNotStartFailedToLoadPackage, hr), SR.ProductName);
                    throw new WizardBackoutException();
                }
                var uiShell = (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));

                string directory;
                var projName = replacementsDictionary["$projectname$"];
                if (!replacementsDictionary.TryGetValue("$specifiedsolutionname$", out var solnName) || string.IsNullOrWhiteSpace(solnName))
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

                object inObj = projName + "|" + directory + "|" + context + "|" + (int)this.projectLanguage;
                var guid = Guids.NodejsCmdSet;
                hr = uiShell.PostExecCommand(ref guid, (uint)PkgCmdId.cmdidImportWizard, 0, ref inObj);
                if (ErrorHandler.Failed(hr))
                {
                    MessageBox.Show(string.Format(ProjectWizardResources.ImportWizardCouldNotStartUnexpectedError, hr), SR.ProductName);
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

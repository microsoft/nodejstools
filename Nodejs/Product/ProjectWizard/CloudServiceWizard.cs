// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using ProjectItem = EnvDTE.ProjectItem;

namespace Microsoft.NodejsTools.ProjectWizard
{
    public sealed class CloudServiceWizard : IWizard
    {
        private IWizard _wizard;

        /// <summary>
        /// The settings collection where "Suppress{dialog}" settings are stored
        /// </summary>
        private const string DontShowUpgradeDialogAgainCollection = "NodejsTools\\Dialogs";
        private const string DontShowUpgradeDialogAgainProperty = "SuppressUpgradeAzureTools";

        public CloudServiceWizard()
        {
            try
            {
                // If we fail to find the wizard, we will redirect the user to
                // the WebPI download.
                var asm = Assembly.Load("Microsoft.VisualStudio.CloudService.Wizard,Version=1.0.0.0,Culture=neutral,PublicKeyToken=b03f5f7f11d50a3a");

                var type = asm.GetType("Microsoft.VisualStudio.CloudService.Wizard.CloudServiceWizard");
                _wizard = type.InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[0]) as IWizard;
            }
            catch (ArgumentException)
            {
            }
            catch (BadImageFormatException)
            {
            }
            catch (IOException)
            {
            }
            catch (MemberAccessException)
            {
            }
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
            if (_wizard != null)
            {
                _wizard.BeforeOpeningFile(projectItem);
            }
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
            if (_wizard != null)
            {
                _wizard.ProjectFinishedGenerating(project);
            }
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            if (_wizard != null)
            {
                _wizard.ProjectItemFinishedGenerating(projectItem);
            }
        }

        public void RunFinished()
        {
            if (_wizard != null)
            {
                _wizard.RunFinished();
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            if (_wizard != null)
            {
                return _wizard.ShouldAddProjectItem(filePath);
            }
            return false;
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            var provider = WizardHelpers.GetProvider(automationObject);

            if (_wizard == null)
            {
                try
                {
                    Directory.Delete(replacementsDictionary["$destinationdirectory$"]);
                    Directory.Delete(replacementsDictionary["$solutiondirectory$"]);
                }
                catch
                {
                    // If it fails (doesn't exist/contains files/read-only), let the directory stay.
                }

                var uiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
                Debug.Assert(uiShell != null, "uiShell was null.");
                uiShell.ShowMessageBox(0, Guid.Empty, SR.ProductName, ProjectWizardResources.AzureToolsRequired, null, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 1, out var result);

                // User cancelled, so go back to the New Project dialog
                throw new WizardBackoutException();
            }

            // Run the original wizard to get the right replacements
            _wizard.RunStarted(automationObject, replacementsDictionary, runKind, customParams);
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard
{
    /// <summary>
    /// Provides a project wizard extension which will optionally do an
    /// npm install after the project is created.
    /// </summary>
    public sealed class NpmWizardExtension : IWizard
    {
        #region IWizard Members

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
            Debug.Assert(project != null && project.Object != null);
            Debug.Assert(project.Object is INodePackageModulesCommands);

            // Create the "node_modules/@types" folder before opening any files (which creates the
            // context for the project). This allows tsserver to start watching for type definitions
            // before any are installed (which is needed as "npm install" runs async, so the modules
            // usually aren't installed before the project context is loaded).
            var fullname = project.FullName;
            var projectFolder = Path.GetDirectoryName(fullname);
            Directory.CreateDirectory(Path.Combine(projectFolder, @"node_modules\@types"));

            ((INodePackageModulesCommands)project.Object).InstallMissingModulesAsync();
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        #endregion
    }
}

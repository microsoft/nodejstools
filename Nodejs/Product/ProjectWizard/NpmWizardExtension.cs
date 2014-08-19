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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    /// <summary>
    /// Provides a project wizard extension which will optionally do an
    /// npm install after the project is created.
    /// </summary>
    public sealed class NpmWizardExtension : IWizard {
        #region IWizard Members

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
            Debug.Assert(project.Object != null);
            Debug.Assert(project.Object is INodePackageModulesCommands);
            // prompt the user to install dependencies
            var shouldDoInstall = MessageBox.Show(@"The newly created project has dependencies defined in package.json.

Do you want to run npm install to get the dependencies now?

This operation will run in the background.  
Results of this operation are available in the Output window.",
                "Node.js Tools for Visual Studio",
                MessageBoxButtons.YesNo
            );

            if (shouldDoInstall == DialogResult.Yes) {
                var t = ((INodePackageModulesCommands)project.Object).InstallMissingModulesAsync();
            }

        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) {
        }

        public void RunFinished() {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }

        #endregion
    }
}

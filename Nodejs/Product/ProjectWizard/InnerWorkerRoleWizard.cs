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

using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    /// <summary>
    /// Wizard for creating updating replacement values for the individual
    /// projects in the worker role.  These use the OuterWorkerRoleWizard
    /// instance to track the shared values.
    /// </summary>
    public sealed class InnerWorkerRoleWizard : IWizard {
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
            OuterWorkerRoleWizard.GetCurrentWizard().LastProjectName = project.Name;
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) {
        }

        public void RunFinished() {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            var curWizard = OuterWorkerRoleWizard.GetCurrentWizard();
            replacementsDictionary["$projguid1$"] = curWizard.SharedProjectGuid.ToString();
            if (curWizard.LastProjectName != null) {
                replacementsDictionary["$lastprojectname$"] = curWizard.LastProjectName;
            }
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }
    }
}

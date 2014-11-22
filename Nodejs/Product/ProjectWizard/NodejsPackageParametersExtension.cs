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
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    class NodejsPackageParametersExtension : IWizard {
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            var projectName = replacementsDictionary["$projectname$"];

            // Remove all leading url-invalid, underscore, and period characters from the string
            var npmProjectNameTransform = Regex.Replace(projectName, "^[^a-zA-Z0-9-~]*", string.Empty);

            // Replace all invalid characters with a dash
            npmProjectNameTransform = Regex.Replace(npmProjectNameTransform, "[^a-zA-Z0-9-_~.]", "-");

            replacementsDictionary.Add("$npmsafeprojectname$", npmProjectNameTransform);
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
            return;
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {
            return;
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem) {
            return;
        }

        public void RunFinished() {
            return;
        }
    }
}

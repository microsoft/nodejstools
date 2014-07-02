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
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.NodejsTools.TestFrameworks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    public sealed class UnitTestFileWizard : IWizard {
        private string _framework;

        public void BeforeOpeningFile(ProjectItem projectItem) {
            EnvDTE.Project project = projectItem.ContainingProject;
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) { }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) {
            EnvDTE.Property property = projectItem.Properties.Item("TestFramework");
            property.Value = _framework;
        }

        public void RunFinished() { }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            if (!replacementsDictionary.TryGetValue("$wizarddata$", out _framework) ||
                string.IsNullOrEmpty(_framework)) {
                _framework = TestFrameworkDirectories.ExportRunnerFramework;
            }
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }
    }
}


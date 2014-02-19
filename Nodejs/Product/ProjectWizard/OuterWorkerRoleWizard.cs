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
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    /// <summary>
    /// Wizard for multi-project worker role template.  This wizard is used
    /// for kicking off the ProjectGroup creation and is then used to share
    /// state between the wizards used in generating the individual projects
    /// which is InnerWorkerRoleWizard.
    /// 
    /// This just keeps track of our instances so the inner projects can
    /// get back to it and can have shared replacement values.
    /// </summary>
    public sealed class OuterWorkerRoleWizard : IWizard {
        internal readonly Guid SharedProjectGuid = Guid.NewGuid();
        internal string LastProjectName;

        [ThreadStatic]
        private static List<OuterWorkerRoleWizard> _curWizards;

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) {
        }

        public void RunFinished() {
            Debug.Assert(_curWizards != null);
            Debug.Assert(_curWizards.Count > 0);
            Debug.Assert(_curWizards[_curWizards.Count - 1] == this);
            _curWizards.RemoveAt(_curWizards.Count - 1);
            if (_curWizards.Count == 0) {
                _curWizards = null;
            }
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
            if (_curWizards == null) {
                _curWizards = new List<OuterWorkerRoleWizard>();
            }
            _curWizards.Add(this);
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }

        internal static OuterWorkerRoleWizard GetCurrentWizard() {
            if (_curWizards == null || _curWizards.Count == 0) {
                throw new InvalidOperationException("Outer wizard requested when not in use");
            }

            return _curWizards[_curWizards.Count - 1];
        }

    }
}

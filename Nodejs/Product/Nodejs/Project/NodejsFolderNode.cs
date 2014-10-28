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

using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    class NodejsFolderNode : CommonFolderNode {
        private readonly CommonProjectNode _project;

        public NodejsFolderNode(CommonProjectNode root, ProjectElement element) : base(root, element) {
            _project = root;
        }

        internal override int IncludeInProject(bool includeChildren) {
            // Include node_modules folder is generally unecessary and can cause VS to hang.
            // http://nodejstools.codeplex.com/workitem/1432
            // Check if the folder is node_modules, and warn the user to ensure they don't run into this issue or at least set expectations appropriately.
            string nodeModulesPath = Path.Combine(_project.FullPathToChildren, "node_modules");
            if (CommonUtils.IsSameDirectory(nodeModulesPath, ItemNode.Url) &&
                !ShouldIncludeNodeModulesFolderInProject()) {
                return VSConstants.S_OK;
            }
            return base.IncludeInProject(includeChildren);                
        }

        private bool ShouldIncludeNodeModulesFolderInProject() {
            var includeNodeModulesButton = new TaskDialogButton(SR.GetString(SR.IncludeNodeModulesIncludeTitle), SR.GetString(SR.IncludeNodeModulesIncludeDescription));
            var cancelOperationButton = new TaskDialogButton(SR.GetString(SR.IncludeNodeModulesCancelTitle));
            var taskDialog = new TaskDialog(_project.ProjectMgr.Site) {
                AllowCancellation = true,
                EnableHyperlinks = true,
                Title = SR.ProductName,
                MainIcon = TaskDialogIcon.Warning,
                Content = SR.GetString(SR.IncludeNodeModulesContent),
                Buttons = {
                    cancelOperationButton,
                    includeNodeModulesButton
                },
                FooterIcon = TaskDialogIcon.Information,
                Footer = SR.GetString(SR.IncludeNodeModulesInformation),
                SelectedButton = cancelOperationButton
            };

            var button = taskDialog.ShowModal();

            return button == includeNodeModulesButton;
        }
    }
}

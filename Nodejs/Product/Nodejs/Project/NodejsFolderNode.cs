//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.IO;
using System.Text;
using Microsoft.NodejsTools.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using VSLangProj;

namespace Microsoft.NodejsTools.Project {
    class NodejsFolderNode : CommonFolderNode {
        private readonly CommonProjectNode _project;
        private FolderContentType _contentType = FolderContentType.NotAssigned;
        private bool _containsNodeOrBrowserFiles = false;

        public NodejsFolderNode(CommonProjectNode root, ProjectElement element) : base(root, element) {
            _project = root;
        }

        public FolderContentType ContentType {
            get {
                if (_contentType == FolderContentType.NotAssigned) {
                    UpdateContentType();
                }

                return _contentType;
            }
        }

        public void UpdateContentType() {
            var oldContentType = _contentType;
            _contentType = FolderContentType.None;
            var parent = Parent as NodejsFolderNode;
            _containsNodeOrBrowserFiles = false;

            if (ItemNode.IsExcluded || ItemNode.Url.Contains(NodejsConstants.NodeModulesFolder)) {
                _contentType = FolderContentType.None;
            } else {
                // Iterate through all of the javascript files in a directory to determine whether
                // the build actions are Content, Compile, or a mix of the two.
                var nodejsFileNodes = EnumNodesOfType<NodejsFileNode>();
                FolderContentType contentType = FolderContentType.None;
                foreach (var fileNode in nodejsFileNodes) {
                    if (!fileNode.Url.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    var properties = fileNode.NodeProperties as IncludedFileNodeProperties;
                    if (properties != null) {
                        _containsNodeOrBrowserFiles = true;
                        switch (properties.BuildAction) {
                            case prjBuildAction.prjBuildActionContent:
                                contentType |= FolderContentType.Browser;
                                break;
                            case prjBuildAction.prjBuildActionCompile:
                                contentType |= FolderContentType.Node;
                                break;
                        }

                        if (contentType == FolderContentType.Mixed) {
                            break;
                        }
                    }
                }

                // If there are no relevant javascript files in the folder, then fall back to
                // the parent type. This enables us to provide good defaults in the event that
                // an item is added to the directory later.
                if (contentType == FolderContentType.None) {
                    // Set as parent content type 
                    if (parent != null) {
                        contentType = parent.ContentType;
                    }
                }

                _contentType = contentType;
                ProjectMgr.ReDrawNode(this, UIHierarchyElement.Caption);
            }

            // Update the caption of the parent folder accordingly
            if (parent != null && _contentType != oldContentType) {
                parent.UpdateContentType();
            }
        }

        public override string Caption {
            get {
                var res = base.Caption;

                if (NodejsPackage.Instance.GeneralOptionsPage.ShowBrowserAndNodeLabels &&
                    NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLevel != AnalysisLevel.Preview &&
                    _containsNodeOrBrowserFiles) {
                    res = AppendLabel(res, ContentType);
                }
                return res;
            }
        }

        public override void RemoveChild(HierarchyNode node) {
            base.RemoveChild(node);
            UpdateContentType();
        }

        public override void AddChild(HierarchyNode node) {
            base.AddChild(node);

            // If we are adding an immediate child to a directory, then set the content type
            // acording to the content type of the folder it is being moved to.
            var nodejsFileNode = node as NodejsFileNode;
            if (nodejsFileNode != null && nodejsFileNode.Url.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && nodejsFileNode.Parent == this) {
                var properties = nodejsFileNode.NodeProperties as IncludedFileNodeProperties;
                if (properties != null) {
                    switch (ContentType) {
                        case FolderContentType.Browser:
                            properties.ItemType = ProjectFileConstants.Content;
                            break;
                        case FolderContentType.Node:
                            properties.ItemType = ProjectFileConstants.Compile;
                            break;
                    }
                }
            }

            UpdateContentType();
        }

        /// <summary>
        /// Append a label denoting browser-side code, node, or both depending on the content type
        /// 
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string AppendLabel(string folderName, FolderContentType contentType) {
            switch (contentType) {
                case FolderContentType.Browser:
                    folderName += " (browser)";
                    break;
                case FolderContentType.Node:
                    folderName += " (node)";
                    break;
                case FolderContentType.Mixed:
                    folderName += " (node, browser)";
                    break;
            }
            return folderName;
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

        internal override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidSetAsContent:
                        if (_containsNodeOrBrowserFiles && ContentType.HasFlag(FolderContentType.Node)) {
                            result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                        }
                        return VSConstants.S_OK;
                    case PkgCmdId.cmdidSetAsCompile:
                        if (_containsNodeOrBrowserFiles && ContentType.HasFlag(FolderContentType.Browser)) {
                            result = QueryStatusResult.ENABLED | QueryStatusResult.SUPPORTED;
                        }
                        return VSConstants.S_OK;
                }
            }
            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        internal override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (cmdGroup == Guids.NodejsCmdSet) {
                switch (cmd) {
                    case PkgCmdId.cmdidSetAsContent:
                        SetItemTypeRecursively(prjBuildAction.prjBuildActionContent);
                        return VSConstants.S_OK;
                    case PkgCmdId.cmdidSetAsCompile:
                        SetItemTypeRecursively(prjBuildAction.prjBuildActionCompile);
                        return VSConstants.S_OK;
                }
            }
            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        internal void SetItemTypeRecursively(prjBuildAction buildAction) {
            var fileNodesEnumerator = this.EnumNodesOfType<NodejsFileNode>().GetEnumerator();
            while (fileNodesEnumerator.MoveNext()) {
                var includedFileNodeProperties = fileNodesEnumerator.Current.NodeProperties as IncludedFileNodeProperties;
                if (includedFileNodeProperties != null && includedFileNodeProperties.URL.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) {
                    includedFileNodeProperties.BuildAction = buildAction;
                }
            }
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

    internal enum FolderContentType {
        None = 0,
        Browser = 1,
        Node = 2,
        Mixed = 3,
        NotAssigned
    }
}

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
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools;
#if DEV14_OR_LATER
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
#endif

namespace Microsoft.NodejsTools.Project {
    class NodejsFileNode : CommonFileNode {
        public NodejsFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e) {
            if (Url.Contains(AnalysisConstants.NodeModulesFolder)) {
                root.EnqueueForDelayedAnalysis(this);
            } else {
                Analyze();
            }
        }

        protected override void OnParentSet(HierarchyNode parent) {
            if (ProjectMgr == null || ProjectMgr.Analyzer == null || !ProjectMgr.ShouldAcquireTypingsAutomatically) {
                return;
            }

            if (Url.EndsWith(NodejsConstants.TypeScriptDeclarationExtension, StringComparison.OrdinalIgnoreCase) && Url.IndexOf(@"\typings\", StringComparison.OrdinalIgnoreCase) >= 0) {
                ProjectMgr.Site.GetUIThread().Invoke(() => {
                    this.IncludeInProject(true);
                });
            }
        }

        internal void Analyze() {
            if (ProjectMgr != null && ProjectMgr.Analyzer != null && ShouldAnalyze) {
                ProjectMgr.Analyzer.AnalyzeFile(Url, !IsNonMemberItem);
                ProjectMgr._requireCompletionCache.Clear();
            }
            ItemNode.ItemTypeChanged += ItemNode_ItemTypeChanged;
        }

        internal bool ShouldAnalyze {
            get {
                // We analyze if we are a member item or the file is included
                // Also, it should either be marked as compile or not have an item type name (value is null for node_modules
                return !Url.Contains(NodejsConstants.NodeModulesStagingFolder) &&
                    !ProjectMgr.DelayedAnalysisQueue.Contains(this) &&
                    (!IsNonMemberItem || ProjectMgr.IncludeNodejsFile(this)) &&
                    (ItemNode.ItemTypeName == ProjectFileConstants.Compile || string.IsNullOrEmpty(ItemNode.ItemTypeName));

            }
        }

#if DEV14_OR_LATER
        protected override ImageMoniker CodeFileIconMoniker {
            get {
                return KnownMonikers.JSScript;
            }
        }
#endif

        internal override int IncludeInProject(bool includeChildren) {
            // Check if parent folder is designated as containing client-side code.
            var isContent = false;
            var folderNode = this.Parent as NodejsFolderNode;
            if (folderNode != null) {
                var contentType = folderNode.ContentType;
                switch (contentType) {
                    case FolderContentType.Browser:
                        isContent = true;
                        break;
                }
            }

            var includeInProject = base.IncludeInProject(includeChildren);

            if (isContent && Url.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) {
                this.ItemNode.ItemTypeName = ProjectFileConstants.Content;
            }

            ProjectMgr.Analyzer.AnalyzeFile(Url, ShouldAnalyze);

            UpdateParentContentType();
            ItemNode.ItemTypeChanged += ItemNode_ItemTypeChanged;

            if (ProjectMgr.ShouldAcquireTypingsAutomatically) {
                ProjectMgr.Site.GetUIThread().Invoke(() => {
                    ProjectMgr.OnItemAdded(this.Parent, this);
                });
            }

            return includeInProject;
        }

        internal override int ExcludeFromProject() {
            // Analyze on removing from a project so we have the most up to date sources for this.
            // Don't report errors since the file won't remain part of the project. This removes the errors from the list.
            ProjectMgr.Analyzer.AnalyzeFile(Url, false);
            var excludeFromProject = base.ExcludeFromProject();

            UpdateParentContentType();
            ItemNode.ItemTypeChanged -= ItemNode_ItemTypeChanged;

            return excludeFromProject;
        }

        protected override void RaiseOnItemRemoved(string documentToRemove, string[] filesToBeDeleted) {
            base.RaiseOnItemRemoved(documentToRemove, filesToBeDeleted);
            foreach (var file in filesToBeDeleted) {
                if (!File.Exists(file)) {
                    ProjectMgr.Analyzer.UnloadFile(file);
                }
            }
        }

        protected override void RenameChildNodes(FileNode parentNode) {
            base.RenameChildNodes(parentNode);
            this.ProjectMgr.Analyzer.ReloadComplete();
        }

        protected override NodeProperties CreatePropertiesObject() {
            if (IsLinkFile) {
                return new NodejsLinkFileNodeProperties(this);
            } else if (IsNonMemberItem) {
                return new ExcludedFileNodeProperties(this);
            }

            return new NodejsIncludedFileNodeProperties(this);
        }

        private void ItemNode_ItemTypeChanged(object sender, EventArgs e) {
            // item type node was changed...
            // if we have changed the type from compile to anything else, we should scrub
            ProjectMgr.Analyzer.AnalyzeFile(Url, ShouldAnalyze);

            UpdateParentContentType();
        }

        private void UpdateParentContentType() {
            var parent = this.Parent as NodejsFolderNode;
            if (parent != null) {
                parent.UpdateContentType();
            }
        }

        public new NodejsProjectNode ProjectMgr {
            get {
                return (NodejsProjectNode)base.ProjectMgr;
            }
        }

        public override void Remove(bool removeFromStorage) {
            ItemNode.ItemTypeChanged -= ItemNode_ItemTypeChanged;
            base.Remove(removeFromStorage);
        }

        public override void Close() {
            ItemNode.ItemTypeChanged -= ItemNode_ItemTypeChanged;
            base.Close();
        }
    }
}

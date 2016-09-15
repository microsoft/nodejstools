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
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.NodejsTools.Project {
    class NodejsFileNode : CommonFileNode {
        public NodejsFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e) {
        }

        protected override void OnParentSet(HierarchyNode parent) {
            if (ProjectMgr == null || !ProjectMgr.ShouldAcquireTypingsAutomatically) {
                return;
            }

            if (Url.EndsWith(NodejsConstants.TypeScriptDeclarationExtension, StringComparison.OrdinalIgnoreCase)
              && Url.StartsWith(Path.Combine(ProjectMgr.ProjectFolder, @"typings\"), StringComparison.OrdinalIgnoreCase)) {
                ProjectMgr.Site.GetUIThread().Invoke(() => {
                    this.IncludeInProject(true);
                });
            }
        }

        protected override ImageMoniker CodeFileIconMoniker {
            get {
                return KnownMonikers.JSScript;
            }
        }

        internal override int IncludeInProject(bool includeChildren) {
            if (!ItemNode.IsExcluded) {
                return 0;
            }

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

            if (ProjectMgr.ShouldAcquireTypingsAutomatically) {
                ProjectMgr.Site.GetUIThread().Invoke(() => {
                    ProjectMgr.OnItemAdded(this.Parent, this);
                });
            }

            return includeInProject;
        }

        protected override NodeProperties CreatePropertiesObject() {
            if (IsLinkFile) {
                return new NodejsLinkFileNodeProperties(this);
            } else if (IsNonMemberItem) {
                return new ExcludedFileNodeProperties(this);
            }

            return new NodejsIncludedFileNodeProperties(this);
        }

        public new NodejsProjectNode ProjectMgr {
            get {
                return (NodejsProjectNode)base.ProjectMgr;
            }
        }
    }
}

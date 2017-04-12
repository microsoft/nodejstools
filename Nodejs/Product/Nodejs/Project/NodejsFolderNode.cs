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

        public override string Caption {
            get {
                return base.Caption;
            }
        }

        public override void RemoveChild(HierarchyNode node) {
            base.RemoveChild(node);
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
            var includeNodeModulesButton = new TaskDialogButton(Resources.IncludeNodeModulesIncludeTitle, Resources.IncludeNodeModulesIncludeDescription);
            var cancelOperationButton = new TaskDialogButton(Resources.IncludeNodeModulesCancelTitle);
            var taskDialog = new TaskDialog(_project.ProjectMgr.Site) {
                AllowCancellation = true,
                EnableHyperlinks = true,
                Title = SR.ProductName,
                MainIcon = TaskDialogIcon.Warning,
                Content = Resources.IncludeNodeModulesContent,
                Buttons = {
                    cancelOperationButton,
                    includeNodeModulesButton
                },
                FooterIcon = TaskDialogIcon.Information,
                Footer = Resources.IncludeNodeModulesInformation,
                SelectedButton = cancelOperationButton
            };

            var button = taskDialog.ShowModal();

            return button == includeNodeModulesButton;
        }
    }
}

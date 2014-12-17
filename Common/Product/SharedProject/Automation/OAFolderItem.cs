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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;

namespace Microsoft.VisualStudioTools.Project.Automation {
    /// <summary>
    /// Represents an automation object for a folder in a project
    /// </summary>
    [ComVisible(true)]
    public class OAFolderItem : OAProjectItem {
        #region ctors
        internal OAFolderItem(OAProject project, FolderNode node)
            : base(project, node) {
        }

        #endregion

        private new FolderNode Node {
            get {
                return (FolderNode)base.Node;
            }
        }


        #region overridden methods
        public override ProjectItems Collection {
            get {
                ProjectItems items = new OAProjectItems(this.Project, this.Node.Parent);
                return items;
            }
        }

        public override ProjectItems ProjectItems {
            get {
                return new OAProjectItems(Project, Node);
            }
        }
        #endregion
    }
}

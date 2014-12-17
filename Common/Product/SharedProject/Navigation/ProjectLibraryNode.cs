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

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.VisualStudioTools.Navigation {
    class ProjectLibraryNode : LibraryNode {
        private readonly CommonProjectNode _project;

        public ProjectLibraryNode(CommonProjectNode project)
            : base(null, project.Caption, project.Caption, LibraryNodeType.PhysicalContainer) {
            _project = project;
        }

        public override uint CategoryField(LIB_CATEGORY category) {
            switch (category) {
                case LIB_CATEGORY.LC_NODETYPE:
                    return (uint)_LIBCAT_NODETYPE.LCNT_PROJECT;
            }
            return base.CategoryField(category);
        }

        public override VSTREEDISPLAYDATA DisplayData {
            get {
                var res = new VSTREEDISPLAYDATA();
                res.hImageList = _project.ImageHandler.ImageList.Handle;
                res.Image = res.SelectedImage = (ushort)_project.ImageIndex;
                return res;
            }
        }

        public override StandardGlyphGroup GlyphType {
            get {
                return StandardGlyphGroup.GlyphCoolProject;
            }
        }
    }
}

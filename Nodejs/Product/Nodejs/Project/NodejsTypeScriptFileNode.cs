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

using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.NodejsTools.Project
{
    internal class NodejsTypeScriptFileNode : NodejsFileNode
    {
        public NodejsTypeScriptFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        protected override ImageMoniker CodeFileIconMoniker
        {
            get
            {
                return KnownMonikers.TSFileNode;
            }
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            if (this.IsLinkFile)
            {
                return new NodejsTypeScriptLinkFileNodeProperties(this);
            }
            else if (this.IsNonMemberItem)
            {
                return new ExcludedFileNodeProperties(this);
            }

            return new NodejsTypeScriptFileNodeProperties(this);
        }

        public new NodejsProjectNode ProjectMgr
        {
            get
            {
                return (NodejsProjectNode)base.ProjectMgr;
            }
        }
    }
}

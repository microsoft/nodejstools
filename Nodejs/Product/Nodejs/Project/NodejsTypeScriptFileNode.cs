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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    class NodejsTypeScriptFileNode : CommonFileNode {
        public NodejsTypeScriptFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e) {
        }

        protected override NodeProperties CreatePropertiesObject() {
            if (IsLinkFile) {
                return new NodejsTypeScriptLinkFileNodeProperties(this);
            } else if (IsNonMemberItem) {
                return new ExcludedFileNodeProperties(this);
            }

            return new NodejsTypeScriptFileNodeProperties(this);
        }

        public new NodejsProjectNode ProjectMgr {
            get {
                return (NodejsProjectNode)base.ProjectMgr;
            }
        }
    }
}

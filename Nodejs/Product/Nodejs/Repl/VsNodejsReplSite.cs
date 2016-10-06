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

namespace Microsoft.NodejsTools.Repl {
    class VsNodejsReplSite : INodejsReplSite {
        internal static VsNodejsReplSite Site = new VsNodejsReplSite();

#region INodejsReplSite Members

        public CommonProjectNode GetStartupProject() {
            var nodeJsInstance = NodejsPackage.Instance;
            if (nodeJsInstance == null) {
                return null;
            }
            return NodejsPackage.GetStartupProject(nodeJsInstance);
        }

        public bool TryGetStartupFileAndDirectory(out string fileName, out string directory) {
            var nodeJsInstance = NodejsPackage.Instance;
            if (nodeJsInstance == null) {
                fileName = null;
                directory = null;
                return false;
            }
            return NodejsPackage.TryGetStartupFileAndDirectory(nodeJsInstance, out fileName, out directory);
        }

#endregion
    }
}

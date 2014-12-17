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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace Microsoft.NodejsTools {
    [Guid(Guids.NodejsProjectFactoryString)]
    class NodejsProjectFactory : FlavoredProjectFactoryBase {
        private NodejsPackage _package;

        public NodejsProjectFactory(NodejsPackage package) {
            _package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown) {
            var res = new NodejsProject();
            res._package = _package;
            return res;
        }
    }
}

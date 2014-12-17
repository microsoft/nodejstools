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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.Remote {
    internal class NodeRemoteEnumDebugPrograms : NodeRemoteEnumDebug<IDebugProgram2>, IEnumDebugPrograms2 {

        public NodeRemoteEnumDebugPrograms(NodeRemoteDebugProcess process)
            : base(new NodeRemoteDebugProgram(process)) {
        }

        public NodeRemoteEnumDebugPrograms(NodeRemoteEnumDebugPrograms programs)
            : base(programs.Element) {
        }

        public int Clone(out IEnumDebugPrograms2 ppEnum) {
            ppEnum = new NodeRemoteEnumDebugPrograms(this);
            return VSConstants.S_OK;
        }
    }
}

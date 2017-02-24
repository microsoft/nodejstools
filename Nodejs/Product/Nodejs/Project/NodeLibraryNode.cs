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

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Navigation;

namespace Microsoft.NodejsTools.Project
{
    internal class NodeLibraryNode : CommonLibraryNode
    {
        public NodeLibraryNode(LibraryNode parent, IScopeNode scope, string namePrefix, IVsHierarchy hierarchy, uint itemId) :
            base(parent, scope, namePrefix, hierarchy, itemId)
        {
        }
    }
}

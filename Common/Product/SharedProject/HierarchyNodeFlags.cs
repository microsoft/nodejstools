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

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Represents various boolean states for the HiearchyNode
    /// </summary>
    [Flags]
    enum HierarchyNodeFlags {
        None,
        ExcludeFromScc = 0x01,
        IsExpanded = 0x02,
        HasParentNodeNameRelation = 0x04,
        IsVisible = 0x08
    }
}

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


using System.Collections.Generic;
using Microsoft.NodejsTools.Analysis;

namespace Microsoft.NodejsTools.Interpreter {
    /// <summary>
    /// Provides the location of a member.  This should be implemented on a class
    /// which also implements IMember.
    /// Implementing this interface enables Goto Definition on the member.
    /// 
    /// New in v1.1.
    /// </summary>
    public interface ILocatedMember {
        /// <summary>
        /// Returns where the member is located or null if the location is not known.
        /// </summary>
        IEnumerable<LocationInfo> Locations {
            get;
        }
    }
}

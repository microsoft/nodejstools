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

namespace Microsoft.NodejsTools.Debugger {
    [Flags]
    enum NodeExpressionType {
        None = 0,

        /// <summary>
        /// Defines whether expression is property.
        /// </summary>
        Property = 0x1,

        /// <summary>
        /// Defines whether expression is function.
        /// </summary>
        Function = 0x2,

        /// <summary>
        /// Defines whether expression is boolean type.
        /// </summary>
        Boolean = 0x4,

        /// <summary>
        /// Defines whether expression is private member.
        /// </summary>
        Private = 0x8,

        /// <summary>
        /// Defines whether property is expandable.
        /// </summary>
        Expandable = 0x10,

        /// <summary>
        /// Defines whether property is readonly.
        /// </summary>
        ReadOnly = 0x20,

        /// <summary>
        /// Defines whether property is string type.
        /// </summary>
        String = 0x40
    }
}
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

namespace Microsoft.NodejsTools.Interpreter {
    /// <summary>
    /// Represents a Python module which members can be imported from.
    /// </summary>
    public interface IPythonModule : IMemberContainer, IMember {
        string Name {
            get;
        }

        IEnumerable<string> GetChildrenModules();

        void Imported(IModuleContext context);

        /// <summary>
        /// The documentation of the module
        /// 
        /// New in 1.1.
        /// </summary>
        string Documentation {
            get;
        }
    }
}

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

namespace Microsoft.NodejsTools.Analysis {
    public enum VariableType {
        None,
        /// <summary>
        /// A parameter to a function definition or assignment to a member or global.
        /// </summary>
        Definition,

        /// <summary>
        /// A read from a global, local, member variable.
        /// </summary>
        Reference,

        /// <summary>
        /// A reference to a value which is passed into a parameter.
        /// </summary>
        Value
    }
}

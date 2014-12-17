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
#if FALSE
    /// <summary>
    /// Represents an object which is a function.  Provides documentation for signature help.
    /// </summary>
    public interface IPythonFunction : IMember {
        string Name {
            get;
        }

        string Documentation {
            get;
        }

        bool IsBuiltin {
            get;            
        }
        
        /// <summary>
        /// False if binds instance when in a class, true if always static.
        /// </summary>
        bool IsStatic {
            get;
        }

        IList<IPythonFunctionOverload> Overloads {
            get;
        }

        IPythonType DeclaringType {
            get;
        }

        IPythonModule DeclaringModule {
            get;
        }
    }
#endif
}

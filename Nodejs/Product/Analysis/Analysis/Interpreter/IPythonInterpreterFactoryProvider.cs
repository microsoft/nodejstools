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
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Provides a source of Python interpreters.  This enables a single implementation
    /// to dynamically lookup the installed Python versions and provide multiple interpreters.
    /// </summary>
    public interface IPythonInterpreterFactoryProvider {
        /// <summary>
        /// Returns the interpreter factories that this provider supports.  
        /// 
        /// The factories returned should be the same instances for subsequent calls.  If the number 
        /// of available factories can change at runtime new factories can still be returned but the 
        /// existing instances should not be re-created.
        /// </summary>
        IEnumerable<IPythonInterpreterFactory> GetInterpreterFactories();

        /// <summary>
        /// Raised when the result of calling <see cref="GetInterpreterFactories"/> may have changed.
        /// </summary>
        /// <remarks>New in 2.0.</remarks>
        event EventHandler InterpreterFactoriesChanged;
    }
#endif
}

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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Analysis;

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Interface for providing an interpreter implementation for plugging into
    /// Python Tools for Visual Studio.
    /// 
    /// This interface provides information about Python types and modules,
    /// which will be used for program analysis and IntelliSense.
    /// 
    /// An interpreter is provided by an object implementing 
    /// <see cref="IPythonInterpreterFactory"/>.
    /// </summary>
    public interface IPythonInterpreter {
        /// <summary>
        /// Performs any interpreter-specific initialization that is required.
        /// </summary>
        /// <param name="state"></param>
        void Initialize(JavaScriptAnalyzer state);

        /// <summary>
        /// Gets a well known built-in type such as int, list, dict, etc...
        /// </summary>
        /// <param name="id">The built-in type to get</param>
        /// <returns>An IPythonType representing the type.</returns>
        /// <exception cref="KeyNotFoundException">
        /// The requested type cannot be resolved by this interpreter.
        /// </exception>
        IPythonType GetBuiltinType(BuiltinTypeId id);

        /// <summary>
        /// Returns a list of module names that can be imported by this
        /// interpreter.
        /// </summary>
        IList<string> GetModuleNames();

        /// <summary>
        /// The list of built-in module names has changed (usually because a
        /// background analysis of the standard library has completed).
        /// </summary>
        event EventHandler ModuleNamesChanged;

        /// <summary>
        /// Returns an IPythonModule for a given module name. Returns null if
        /// the module does not exist.
        /// </summary>
        IPythonModule ImportModule(string name);

        /// <summary>
        /// Provides interpreter-specific information which can be associated
        /// with a module.
        /// 
        /// Interpreters can return null if they have no per-module state.
        /// </summary>
        IModuleContext CreateModuleContext();
    }
#endif
}

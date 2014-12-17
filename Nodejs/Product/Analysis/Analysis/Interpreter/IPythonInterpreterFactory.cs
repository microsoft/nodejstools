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

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Provides a factory for creating IPythonInterpreters for a specific
    /// Python implementation.
    /// 
    /// The factory includes information about what type of interpreter will be
    /// created - this is used for displaying information to the user and for
    /// tracking per-interpreter settings.
    /// 
    /// It also contains a method for creating an interpreter. This allows for
    /// stateful interpreters that participate in analysis or track other state.
    /// </summary>
    public interface IPythonInterpreterFactory {
        /// <summary>
        /// A user friendly description of the interpreter.
        /// </summary>
        string Description {
            get;
        }

        /// <summary>
        /// Configuration settings for the interpreter.
        /// </summary>
        InterpreterConfiguration Configuration {
            get;
        }

        /// <summary>
        /// A stable ID for the interpreter used to track settings.
        /// </summary>
        Guid Id {
            get;
        }

        /// <summary>
        /// Creates an IPythonInterpreter instance.
        /// </summary>
        IPythonInterpreter CreateInterpreter();
    }
#endif
}

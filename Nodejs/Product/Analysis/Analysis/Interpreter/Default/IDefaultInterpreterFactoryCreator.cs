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

namespace Microsoft.NodejsTools.Interpreter.Default {
#if FALSE
    /// <summary>
    /// Provides a factory for creating a default Python interpreter factory which is configured to run against
    /// a custom Python interpreter.  By default the interpreter factory picks up all interpreters registered
    /// in the registry.  This provides a mechanism to create interpreters whose configuration is stored elsewhere.
    /// </summary>
    public interface IDefaultInterpreterFactoryCreator {
        /// <summary>
        /// Creates a new interpreter factory with the specified options.
        /// </summary>
        IPythonInterpreterFactory CreateInterpreterFactory(Dictionary<InterpreterFactoryOptions, object> options);
    }
#endif
}

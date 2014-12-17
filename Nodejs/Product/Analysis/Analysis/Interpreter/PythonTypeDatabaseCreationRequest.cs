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
    /// Provides information to PythonTypeDatabase on how to generate a
    /// database.
    /// </summary>
    public sealed class PythonTypeDatabaseCreationRequest {
        public PythonTypeDatabaseCreationRequest() {
            ExtraInputDatabases = new List<string>();
        }

        /// <summary>
        /// The interpreter factory to use. This will provide language version
        /// and source paths.
        /// </summary>
        public PythonInterpreterFactoryWithDatabase Factory { get; set; }

        /// <summary>
        /// A list of extra databases to load when analyzing the factory's
        /// library.
        /// </summary>
        public List<string> ExtraInputDatabases { get; private set; }

        /// <summary>
        /// The directory to write the database to.
        /// </summary>
        public string OutputPath { get; set; }

        /// <summary>
        /// True to avoid analyzing packages that are up to date; false to
        /// regenerate the entire database.
        /// </summary>
        public bool SkipUnchanged { get; set; }

        /// <summary>
        /// A factory to wait for before starting regeneration.
        /// </summary>
        public IPythonInterpreterFactoryWithDatabase WaitFor { get; set; }

        /// <summary>
        /// A function to call when the analysis process is completed. The value
        /// is an error code.
        /// </summary>
        public Action<int> OnExit { get; set; }
    }
#endif
}

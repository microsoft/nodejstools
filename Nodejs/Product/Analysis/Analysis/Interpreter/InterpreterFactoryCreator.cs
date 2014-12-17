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
using System.IO;
using System.Reflection;
using Microsoft.NodejsTools.Interpreter.Default;

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Provides a factory for creating a Python interpreter factory based on an
    /// executable file and cached completion database.
    /// </summary>
    public static class InterpreterFactoryCreator {
        /// <summary>
        /// Creates a new interpreter factory with the specified options. This
        /// interpreter always includes a cached completion database.
        /// </summary>
        public static PythonInterpreterFactoryWithDatabase CreateInterpreterFactory(InterpreterFactoryCreationOptions options) {
            var ver = options.LanguageVersion ?? new Version(2, 7);
            var description = options.Description ?? string.Format("Unknown Python {0}", ver);
            var prefixPath = options.PrefixPath;
            if (string.IsNullOrEmpty(prefixPath) && !string.IsNullOrEmpty(options.InterpreterPath)) {
                prefixPath = Path.GetDirectoryName(options.InterpreterPath);
            }

            return new CPythonInterpreterFactory(
                ver,
                (options.Id == default(Guid)) ? Guid.NewGuid() : options.Id,
                description,
                prefixPath ?? string.Empty,
                options.InterpreterPath ?? string.Empty,
                options.WindowInterpreterPath ?? string.Empty,
                options.LibraryPath ?? string.Empty,
                options.PathEnvironmentVariableName ?? "PYTHONPATH",
                options.Architecture,
                options.WatchLibraryForNewModules
            );
        }

        /// <summary>
        /// Creates a new interpreter factory with the specified database. This
        /// factory is suitable for analysis, but not execution.
        /// </summary>
        public static PythonInterpreterFactoryWithDatabase CreateAnalysisInterpreterFactory(
            Version languageVersion,
            PythonTypeDatabase database) {
            return new AnalysisOnlyInterpreterFactory(languageVersion, database);
        }

        /// <summary>
        /// Creates a new interpreter factory with the specified database path.
        /// This factory is suitable for analysis, but not execution.
        /// </summary>
        public static PythonInterpreterFactoryWithDatabase CreateAnalysisInterpreterFactory(
            Version languageVersion,
            string description,
            params string[] databasePaths) {
            return new AnalysisOnlyInterpreterFactory(languageVersion, databasePaths);
        }

        /// <summary>
        /// Creates a new interpreter factory with the default database. This
        /// factory is suitable for analysis, but not execution.
        /// </summary>
        public static PythonInterpreterFactoryWithDatabase CreateAnalysisInterpreterFactory(
            Version languageVersion,
            string description = null) {
            return new AnalysisOnlyInterpreterFactory(languageVersion, description);
        }
    }
#endif
}

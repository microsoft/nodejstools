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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Analysis;
//using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    public static class PythonInterpreterFactoryExtensions {
        /// <summary>
        /// Executes the interpreter with the specified arguments. Any output is
        /// captured and returned via the <see cref="ProcessOutput"/> object.
        /// </summary>
        internal static ProcessOutput Run(
            this IPythonInterpreterFactory factory,
            params string[] arguments) {
            return ProcessOutput.RunHiddenAndCapture(factory.Configuration.InterpreterPath, arguments);
        }

        /// <summary>
        /// Determines whether two interpreter factories are equivalent.
        /// </summary>
        public static bool IsEqual(this IPythonInterpreterFactory x, IPythonInterpreterFactory y) {
            if (x == null || y == null) {
                return x == null && y == null;
            }
            if (x.GetType() != y.GetType()) {
                return false;
            }

            return x.Id == y.Id && 
                x.Description == y.Description &&
                x.Configuration.Equals(y.Configuration);
        }

        /// <summary>
        /// Determines whether the interpreter factory contains the specified
        /// modules.
        /// </summary>
        /// <returns>The names of the modules that were found.</returns>
        internal static HashSet<string> FindModules(this IPythonInterpreterFactory factory, params string[] moduleNames) {
            var expected = new HashSet<string>(moduleNames);
            var result = new HashSet<string>();
            foreach (var mp in ModulePath.GetModulesInLib(factory)) {
                if (expected.Count == 0) {
                    break;
                }

                if (expected.Remove(mp.ModuleName)) {
                    result.Add(mp.ModuleName);
                }
            }
            return result;
        }

        /// <summary>
        /// Generates the completion database and returns a task that will
        /// complete when the database is regenerated.
        /// </summary>
        internal static Task<int> GenerateDatabaseAsync(
            this IPythonInterpreterFactoryWithDatabase factory,
            GenerateDatabaseOptions options
        ) {
            var tcs = new TaskCompletionSource<int>();
            factory.GenerateDatabase(options, tcs.SetResult);
            return tcs.Task;
        }
    }
#endif
}

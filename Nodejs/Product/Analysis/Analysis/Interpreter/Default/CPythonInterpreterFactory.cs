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

namespace Microsoft.NodejsTools.Interpreter.Default {
#if FALSE
    class CPythonInterpreterFactory : PythonInterpreterFactoryWithDatabase {
        public CPythonInterpreterFactory(
            Version version,
            Guid id,
            string description,
            string prefixPath,
            string pythonPath,
            string pythonwPath,
            string libPath,
            string pathEnvVar,
            ProcessorArchitecture arch,
            bool watchForNewModules)
            : base(
                id,
                description,
                new InterpreterConfiguration(
                    prefixPath,
                    pythonPath,
                    pythonwPath,
                    libPath,
                    pathEnvVar,
                    arch,
                    version),
                watchForNewModules) { }

        static string GetDirectoryName(string path) {
            if (CommonUtils.IsValidPath(path)) {
                return Path.GetDirectoryName(path);
            }
            return string.Empty;
        }
    }
#endif
}

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
using System.Reflection;

namespace Microsoft.NodejsTools.Interpreter.Default {
#if FALSE
    class CPythonInterpreterConfiguration : InterpreterConfiguration {
        private readonly string _pythonPath, _pythonwPath, _pathEnvVar;
        private readonly ProcessorArchitecture _arch;
        private readonly Version _version;
        
        public CPythonInterpreterConfiguration(string pythonPath, string pythonwPath, string pathEnvVar, ProcessorArchitecture arch, Version version) {
            _pythonPath = pythonPath;
            _pythonwPath = pythonwPath;
            _arch = arch;
            _version = version;
            _pathEnvVar = pathEnvVar;
        }

        public override string InterpreterPath {
            get { return _pythonPath; }
        }

        public override string WindowsInterpreterPath {
            get { return _pythonwPath; }
        }

        public override string PathEnvironmentVariable {
            get { return _pathEnvVar; }
        }

        public override ProcessorArchitecture Architecture {
            get { return _arch; }
        }

        public override Version Version {
            get { return _version; }
        }
    }
#endif
}

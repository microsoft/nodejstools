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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    public sealed class InterpreterConfiguration {
        readonly string _prefixPath;
        readonly string _interpreterPath;
        readonly string _windowsInterpreterPath;
        readonly string _libraryPath;
        readonly string _pathEnvironmentVariable;
        readonly ProcessorArchitecture _architecture;
        readonly Version _version;

        /// <summary>
        /// Creates a blank configuration with the specified language version.
        /// This is intended for use in placeholder implementations of
        /// <see cref="IPythonInterpreterFactory"/> for when a known interpreter
        /// is unavailable.
        /// </summary>
        public InterpreterConfiguration(Version version) {
            _version = version;
        }

        /// <summary>
        /// <para>Constructs a new interpreter configuration based on the
        /// provided values.</para>
        /// <para>No validation is performed on the parameters.</para>
        /// <para>If winPath is null or empty,
        /// <see cref="WindowsInterpreterPath"/> will be set to path.</para>
        /// <para>If libraryPath is null or empty and prefixPath is a valid
        /// file system path, <see cref="LibraryPath"/> will be set to
        /// prefixPath plus "Lib".</para>
        /// </summary>
        public InterpreterConfiguration(
            string prefixPath,
            string path,
            string winPath,
            string libraryPath,
            string pathVar,
            ProcessorArchitecture arch,
            Version version
        ) {
            _prefixPath = prefixPath;
            _interpreterPath = path;
            _windowsInterpreterPath = string.IsNullOrEmpty(winPath) ? path : winPath;
            _libraryPath = libraryPath;
            if (string.IsNullOrEmpty(_libraryPath) && !string.IsNullOrEmpty(_prefixPath)) {
                try {
                    _libraryPath = Path.Combine(_prefixPath, "Lib");
                } catch (ArgumentException) {
                }
            }
            _pathEnvironmentVariable = pathVar;
            _architecture = arch;
            _version = version;
            Debug.Assert(string.IsNullOrEmpty(_interpreterPath) || !string.IsNullOrEmpty(_prefixPath),
                "Anyone providing an interpreter should also specify the prefix path");
        }

        /// <summary>
        /// Returns the prefix path of the Python installation. All files
        /// related to the installation should be underneath this path.
        /// </summary>
        public string PrefixPath {
            get { return _prefixPath; }
        }

        /// <summary>
        /// Returns the path to the interpreter executable for launching Python
        /// applications.
        /// </summary>
        public string InterpreterPath {
            get { return _interpreterPath; }
        }

        /// <summary>
        /// Returns the path to the interpreter executable for launching Python
        /// applications which are windows applications (pythonw.exe, ipyw.exe).
        /// </summary>
        public string WindowsInterpreterPath {
            get { return _windowsInterpreterPath; }
        }

        /// <summary>
        /// The path to the standard library associated with this interpreter.
        /// This may be null if the interpreter does not support standard
        /// library analysis.
        /// </summary>
        public string LibraryPath {
            get { return _libraryPath; }
        }

        /// <summary>
        /// Gets the environment variable which should be used to set sys.path.
        /// </summary>
        public string PathEnvironmentVariable {
            get { return _pathEnvironmentVariable; }
        }

        /// <summary>
        /// The architecture of the interpreter executable.
        /// </summary>
        public ProcessorArchitecture Architecture {
            get { return _architecture; }
        }

        /// <summary>
        /// The language version of the interpreter (e.g. 2.7).
        /// </summary>
        public Version Version {
            get { return _version; }
        }

        public override bool Equals(object obj) {
            var other = obj as InterpreterConfiguration;
            if (other == null) {
                return false;
            }

            var cmp = StringComparer.OrdinalIgnoreCase;
            return cmp.Equals(PrefixPath, other.PrefixPath) &&
                cmp.Equals(InterpreterPath, other.InterpreterPath) &&
                cmp.Equals(WindowsInterpreterPath, other.WindowsInterpreterPath) &&
                cmp.Equals(LibraryPath, other.LibraryPath) &&
                cmp.Equals(PathEnvironmentVariable, other.PathEnvironmentVariable) &&
                Architecture == other.Architecture &&
                Version == other.Version;
        }

        public override int GetHashCode() {
            var cmp = StringComparer.OrdinalIgnoreCase;
            return cmp.GetHashCode(PrefixPath) ^
                cmp.GetHashCode(InterpreterPath) ^
                cmp.GetHashCode(WindowsInterpreterPath) ^
                cmp.GetHashCode(LibraryPath) ^
                cmp.GetHashCode(PathEnvironmentVariable) ^
                Architecture.GetHashCode() ^
                Version.GetHashCode();
        }
    }
#endif
}

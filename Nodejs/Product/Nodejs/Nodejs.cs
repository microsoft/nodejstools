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
#if !NO_WINDOWS
using System.Windows.Forms;
#endif
using Microsoft.Win32;
#if !NO_WINDOWS
using Microsoft.NodejsTools.Project;
using System.Security;
#endif

namespace Microsoft.NodejsTools {
    public sealed class Nodejs {
        private const string NodejsRegPath = "Software\\Node.js";
        private const string InstallPath = "InstallPath";

        public static string NodeExePath {
            get {
                return GetPathToNodeExecutable();
            }
        }

        public static string GetPathToNodeExecutable(string executable = "node.exe") {
            // Attempt to find Node.js/NPM in the Registry. (Currrent User)
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
            using (var node = baseKey.OpenSubKey(NodejsRegPath)) {
                if (node != null) {
                    string key = (node.GetValue(InstallPath) as string) ?? string.Empty;
                    var execPath = Path.Combine(key, executable);
                    if (File.Exists(execPath)) {
                        return execPath;
                    }
                }
            }

            // Attempt to find Node.js/NPM in the Registry. (Local Machine x64)
            if (Environment.Is64BitOperatingSystem) {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var node64 = baseKey.OpenSubKey(NodejsRegPath)) {
                    if (node64 != null) {
                        string key = (node64.GetValue(InstallPath) as string) ?? string.Empty;
                        var execPath = Path.Combine(key, executable);
                        if (File.Exists(execPath)) {
                            return execPath;
                        }
                    }
                }
            }

            // Attempt to find Node.js/NPM in the Registry. (Local Machine x86)
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var node = baseKey.OpenSubKey(NodejsRegPath)) {
                if (node != null) {
                    string key = (node.GetValue(InstallPath) as string) ?? string.Empty;
                    var execPath = Path.Combine(key, executable);
                    if (File.Exists(execPath)) {
                        return execPath;
                    }
                }
            }

            // If we didn't find node.js in the registry we should look at the user's path.
            foreach (var dir in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator)) {
                var execPath = Path.Combine(dir, executable);
                if (File.Exists(execPath)) {
                    return execPath;
                }
            }

            // It wasn't in the users path.  Check Program Files for the nodejs folder.
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", executable);
            if (File.Exists(path)) {
                return path;
            }

            // It wasn't in the users path.  Check Program Files x86 for the nodejs folder.
            var x86path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!String.IsNullOrEmpty(x86path)) {
                path = Path.Combine(x86path, "nodejs", executable);
                if (File.Exists(path)) {
                    return path;
                }
            }

            // we didn't find the path.
            return null;
        }

#if !NO_WINDOWS
        public static void ShowNodejsNotInstalled() {
            MessageBox.Show(
                SR.GetString(SR.NodejsNotInstalled),
                SR.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        public static void ShowNodejsPathNotFound(string path) {
            MessageBox.Show(
                SR.GetString(SR.NodeExeDoesntExist, path),
                SR.ProductName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
#endif
    }
}

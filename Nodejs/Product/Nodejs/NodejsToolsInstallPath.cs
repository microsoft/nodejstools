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
using Microsoft.Win32;

namespace Microsoft.NodejsTools {
    public static class NodejsToolsInstallPath {
        private static string GetFromAssembly(Assembly assembly, string filename) {
            string path = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                filename
            );
            if (File.Exists(path)) {
                return path;
            }
            return string.Empty;
        }

        private static string GetFromRegistry(string filename) {
            const string ROOT_KEY = "Software\\Microsoft\\NodejsTools\\" + AssemblyVersionInfo.VSVersion;

            string installDir = null;
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (var configKey = baseKey.OpenSubKey(ROOT_KEY)) {
                if (configKey != null) {
                    installDir = configKey.GetValue("InstallDir") as string;
                }
            }

            if (string.IsNullOrEmpty(installDir)) {
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                using (var configKey = baseKey.OpenSubKey(ROOT_KEY)) {
                    if (configKey != null) {
                        installDir = configKey.GetValue("InstallDir") as string;
                    }
                }
            }

            if (!String.IsNullOrEmpty(installDir)) {
                var path = Path.Combine(installDir, filename);
                if (File.Exists(path)) {
                    return path;
                }
            }

            return string.Empty;
        }

        public static string GetFile(string filename) {
            string path = GetFromAssembly(typeof(NodejsToolsInstallPath).Assembly, filename);
            if (!string.IsNullOrEmpty(path)) {
                return path;
            }

            path = GetFromRegistry(filename);
            if (!string.IsNullOrEmpty(path)) {
                return path;
            }

            throw new InvalidOperationException(
                "Unable to determine Node.js Tools installation path"
            );
        }
    }
}

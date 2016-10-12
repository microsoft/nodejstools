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
using System.Linq;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.NodejsTools.Npm;

using SR = Microsoft.NodejsTools.Project.SR;

namespace Microsoft.NodejsTools {
    internal class TypingsAcquisition {
        private const string TypingsTool = "ntvs_install_typings";
        private const string TypingsToolExe = TypingsTool + ".cmd";

        private const string TypingsDirectoryName = "typings";

        private static SemaphoreSlim typingsToolGlobalWorkSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Full path to the local copy of the typings acquision tool.
        /// </summary>
        private static string LocalTypingsToolPath {
            get {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                var root = Path.GetDirectoryName(path);

                return Path.Combine(
                    root,
                    "TypingsAcquisitionTool");
            }
        }

        /// <summary>
        /// Full path to the typings acquisition tool.
        /// </summary>
        private static string TypingsToolPath {
            get {
                return Path.Combine(
                    NodejsConstants.ExternalToolsPath,
                    "node_modules",
                    ".bin",
                    TypingsToolExe);
            }
        }

        private readonly INpmController _npmController;
        private readonly string _pathToRootProjectDirectory;

        private readonly Lazy<HashSet<string>> _acquiredTypingsPackageNames;
        private bool _didTryToInstallTypingsTool;

        public TypingsAcquisition(INpmController controller) {
            _npmController = controller;
            _pathToRootProjectDirectory = controller.RootPackage.Path;

            _acquiredTypingsPackageNames = new Lazy<HashSet<string>>(() => {
                return new HashSet<string>(CurrentTypingsPackages(_pathToRootProjectDirectory));
            });
        }

        public Task<bool> AcquireTypings(IEnumerable<string> packages, Redirector redirector) {
            return typingsToolGlobalWorkSemaphore.WaitAsync().ContinueWith(async _ => {
                try {
                    return await DownloadTypingsForPackages(packages, redirector) && await DownloadTypingsForProject(redirector);
                } finally {
                    typingsToolGlobalWorkSemaphore.Release();
                }
            }).Unwrap();
        }

        private async Task<bool> DownloadTypingsForProject(Redirector redirector) {
            if (!File.Exists(Path.Combine(_pathToRootProjectDirectory, "typings.json"))) {
                return true;
            }
            return await ExecuteTypingsTool(new string[] { }, redirector);
        }

        private async Task<bool> DownloadTypingsForPackages(IEnumerable<string> packages, Redirector redirector) {
            var typingsToAquire = GetNewTypingsToAcquire(packages);
            if (!typingsToAquire.Any()) {
                return true;
            }

            var success = await ExecuteTypingsTool(GetTypingsToolInstallArguments(packages), redirector);
            if (success) {
                _acquiredTypingsPackageNames.Value.UnionWith(typingsToAquire);
            }
            return success;
        }

        private IEnumerable<string> GetNewTypingsToAcquire(IEnumerable<string> packages) {
            var currentTypings = _acquiredTypingsPackageNames.Value;
            return packages
                .Where(name => {
                    if (name.Contains('@')) { // We don't support typings for scoped modules
                        return false;
                    }
                    return Uri.EscapeUriString(name).Equals(name, StringComparison.OrdinalIgnoreCase);
                })
                .Where(package => !currentTypings.Contains(package));
        }

        private async Task<bool> ExecuteTypingsTool(IEnumerable<string> arguments, Redirector redirector) {
            string typingsTool = await EnsureTypingsToolInstalled(redirector);
            if (string.IsNullOrEmpty(typingsTool)) {
                redirector?.WriteErrorLine(SR.GetString(SR.TypingsToolNotInstalledError));
                return false;
            }

            using (var process = ProcessOutput.Run(
                typingsTool,
                arguments,
                _pathToRootProjectDirectory,
                null,
                false,
                redirector,
                quoteArgs: true,
                outputEncoding: Encoding.UTF8,
                errorEncoding: Encoding.UTF8)) {
                if (!process.IsStarted) {
                    // Process failed to start, and any exception message has
                    // already been sent through the redirector
                    redirector?.WriteErrorLine(SR.GetString(SR.TypingsToolCouldNotStart));
                    return false;
                }
                var i = await process;
                if (i == 0) {
                    redirector?.WriteLine(SR.GetString(SR.TypingsToolTypingsInstallCompleted));
                    return true;
                } else {
                    process.Kill();
                    redirector?.WriteErrorLine(SR.GetString(SR.TypingsToolTypingsInstallErrorOccurred));
                    return false;
                }
            }
        }

        private async Task<string> EnsureTypingsToolInstalled(Redirector redirector) {
            if (File.Exists(TypingsToolPath)) {
                return TypingsToolPath;
            }

            if (_didTryToInstallTypingsTool) {
                return null;
            } 
            if (!await InstallTypingsTool()) {
                redirector?.WriteErrorLine(SR.GetString(SR.TypingsToolInstallFailed));
                return null;
            }
            return await EnsureTypingsToolInstalled(redirector);
        }

        private async Task<bool> InstallTypingsTool() {
            _didTryToInstallTypingsTool = true;

            Directory.CreateDirectory(NodejsConstants.ExternalToolsPath);

            // install typings
            using (var commander = _npmController.CreateNpmCommander()) {
                return await commander.InstallPackageToFolderByVersionAsync(
                    NodejsConstants.ExternalToolsPath,
                    string.Format(@"""{0}""", LocalTypingsToolPath),
                    string.Empty,
                    false);
            }
        }

        private static IEnumerable<string> GetTypingsToolInstallArguments(IEnumerable<string> packages) {
            var arguments = packages;
            if (NodejsPackage.Instance.IntellisenseOptionsPage.SaveChangesToConfigFile) {
                return arguments.Concat(new[] { "--save" });
            }
            return arguments;
        }

        private static IEnumerable<string> CurrentTypingsPackages(string pathToRootProjectDirectory) {
            var packages = new List<string>();
            var typingsDirectoryPath = Path.Combine(pathToRootProjectDirectory, TypingsDirectoryName);
            if (!Directory.Exists(typingsDirectoryPath)) {
                return packages;
            }
            try {
                foreach (var file in Directory.EnumerateFiles(typingsDirectoryPath, "*.d.ts", SearchOption.AllDirectories)) {
                    var directory = Directory.GetParent(file);
                    if (directory.FullName != typingsDirectoryPath && Path.GetFullPath(directory.FullName).StartsWith(typingsDirectoryPath)) {
                        packages.Add(directory.Name);
                    }
                }
            } catch (IOException) {
                // noop
            } catch (SecurityException) {
                // noop
            } catch (UnauthorizedAccessException) {
                // noop
            }
            return packages;
        }
    }
}

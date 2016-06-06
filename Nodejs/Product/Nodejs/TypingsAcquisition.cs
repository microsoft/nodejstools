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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.NodejsTools.Npm;

using SR = Microsoft.NodejsTools.Project.SR;

namespace Microsoft.NodejsTools {
    internal class TypingsAcquisition {
        private const string Tsd = "tsd";
        private const string TsdExe = Tsd + ".cmd";
        private const string TypingsDirectoryName = "typings";

        private static SemaphoreSlim tsdGlobalWorkSemaphore = new SemaphoreSlim(1);

        private readonly INpmController _npmController;
        private readonly string _pathToRootNpmDirectory;
        private readonly string _pathToRootProjectDirectory;

        private readonly Lazy<HashSet<string>> _acquiredTypingsPackageNames;
        private bool _didTryToInstallTsd;

        public TypingsAcquisition(INpmController controller) {
            _npmController = controller;
            _pathToRootNpmDirectory = controller.ListBaseDirectory;
            _pathToRootProjectDirectory = controller.RootPackage.Path;

            _acquiredTypingsPackageNames = new Lazy<HashSet<string>>(() => {
                return new HashSet<string>(CurrentTypingsPackages(_pathToRootProjectDirectory));
            });
        }

        public Task<bool> AcquireTypings(IEnumerable<string> packages, Redirector redirector) {
            return tsdGlobalWorkSemaphore.WaitAsync().ContinueWith(async _ => {
                var typingsToAquire = GetNewTypingsToAcquire(packages);
                var success = await DownloadTypings(typingsToAquire, redirector);
                if (success) {
                    _acquiredTypingsPackageNames.Value.UnionWith(typingsToAquire);
                }
                tsdGlobalWorkSemaphore.Release();
                return success;
            }).Unwrap();
        }

        private IEnumerable<string> GetNewTypingsToAcquire(IEnumerable<string> packages) {
            var currentTypings = _acquiredTypingsPackageNames.Value;
            return packages.Where(package => !currentTypings.Contains(package));
        }

        private async Task<bool> DownloadTypings(IEnumerable<string> packages, Redirector redirector) {
            if (!packages.Any()) {
                return true;
            }

            string tsdPath = await EnsureTsdInstalled();
            if (string.IsNullOrEmpty(tsdPath)) {
                if (redirector != null) {
                    redirector.WriteErrorLine(SR.GetString(SR.TsdNotInstalledError));
                }
                return false;
            }

            using (var process = ProcessOutput.Run(
                tsdPath,
                TsdInstallArguments(packages),
                _pathToRootProjectDirectory,
                null,
                false,
                redirector,
                quoteArgs: true)) {
                if (!process.IsStarted) {
                    // Process failed to start, and any exception message has
                    // already been sent through the redirector
                    if (redirector != null) {
                        redirector.WriteErrorLine("could not start tsd");
                    }
                    return false;
                }
                var i = await process;
                if (i == 0) {
                    if (redirector != null) {
                        redirector.WriteLine(SR.GetString(SR.TsdInstallCompleted));
                    }
                    return true;
                } else {
                    process.Kill();
                    if (redirector != null) {
                        redirector.WriteErrorLine(SR.GetString(SR.TsdInstallErrorOccurred));
                    }
                    return false;
                }
            }
        }

        private async Task<string> EnsureTsdInstalled() {
            var tsdPath = Path.Combine(_pathToRootNpmDirectory, TsdExe);
            if (File.Exists(tsdPath)) {
                return tsdPath;
            }

            if (_didTryToInstallTsd) {
                return null;
            } else {
                _didTryToInstallTsd = true;
                if (!await TryInstallTsd()) {
                    return null;
                }
                return await EnsureTsdInstalled();
            }
        }

        private async Task<bool> TryInstallTsd() {
            using (var commander = _npmController.CreateNpmCommander()) {
                return await commander.InstallGlobalPackageByVersionAsync(Tsd, "*");
            }
        }

        private static IEnumerable<string> TsdInstallArguments(IEnumerable<string> packages) {
            var arguments = new[] { "install", }.Concat(packages);
            if (NodejsPackage.Instance.IntellisenseOptionsPage.SaveChangesToConfigFile) {
                arguments = arguments.Concat(new[] { "--save" });
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

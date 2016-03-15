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

using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.NodejsTools.Npm;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using SR = Microsoft.NodejsTools.Project.SR;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools {
    static class TypingsAcquisition {
        private const string TsdExe = "tsd.cmd";
        private const string typingsDirecctory = "typings";
        private const string tsdJson = "tsd.json";

        public static async Task<bool> AcquireTypings(
            string pathToRootNpmDirectory,
            string pathToRootProjectDirectory,
            IEnumerable<IPackage> packages,
            Redirector redirector) {
            var newPackages = TypingsToAcquire(pathToRootProjectDirectory, packages);
            return await DownloadTypings(
                pathToRootNpmDirectory,
                pathToRootProjectDirectory,
                newPackages,
                redirector);
        }

        private static async Task<bool> DownloadTypings(
            string pathToRootNpmDirectory,
            string pathToRootProjectDirectory,
            IEnumerable<IPackage> packages,
            Redirector redirector) {
            if (!packages.Any()) {
                return true;
            }

            var tsdPath = GetTsdPath(pathToRootNpmDirectory);
            if (tsdPath == null) {
                if (redirector != null)
                    redirector.WriteErrorLine(SR.GetString(SR.TsdNotInstalledError));
                return false;
            }

            using (var process = ProcessOutput.Run(
                tsdPath,
                TsdInstallArguments(packages),
                pathToRootProjectDirectory,
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

        private static string GetTsdPath(string pathToRootNpmDirectory) {
            var path = Path.Combine(pathToRootNpmDirectory, TsdExe);
            return File.Exists(path) ? path : null;
        }

        private static string GetPackageTsdName(IPackage package) {
            return package.Name;
        }

        private static IEnumerable<string> TsdInstallArguments(IEnumerable<IPackage> packages) {
            return new[] { "install", }.Concat(packages.Select(GetPackageTsdName)).Concat(new[] { "--save" });
        }

        private static IEnumerable<IPackage> TypingsToAcquire(string pathToRootProjectDirectory, IEnumerable<IPackage> packages) {
            var current = CurrentTypingsPackages(pathToRootProjectDirectory);
            return packages.Where(package => !current.Contains(GetPackageTsdName(package)));
        }

        private static HashSet<string> CurrentTypingsPackages(string pathToRootProjectDirectory) {
            var tsdJsonFilePath = Path.Combine(pathToRootProjectDirectory, tsdJson);
            if (!File.Exists(tsdJsonFilePath)) {
                return new HashSet<string>();
            }

            using (var file = File.OpenText(tsdJsonFilePath)) {
                var json = JObject.Parse(file.ReadToEnd());
                var installed = (JObject)json["installed"];
                return new HashSet<string>(
                    installed.Properties()
                        .Select(prop => Regex.Match(prop.Name, @"^[^/]+"))
                        .Where(match => match.Success)
                        .Select(match => match.Value));
            }
        }
    }
}

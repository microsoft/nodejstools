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

namespace Microsoft.NodejsTools {
    static class TypingAcquisition {
        private const string TsdExe = "tsd.cmd";

        public static async Task<bool> AquireTypings(
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
                new[] { "install", "--save" }.Concat(packages.Select(GetPackageTsdName)),
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
            if (File.Exists(path))
                return path;
            return null;
        }

        private static string GetPackageTsdName(IPackage package) {
            return package.Name;
        }
    }
}

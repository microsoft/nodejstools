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

using System.Globalization;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmUninstallCommand : NpmCommand {
        public NpmUninstallCommand(
            string fullPathToRootPackageDirectory,
            string packageName,
            DependencyType type,
            bool global = false,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm) {
            Arguments = global
                            ? string.Format(CultureInfo.InvariantCulture, "uninstall {0} --g", packageName)
                            : string.Format(CultureInfo.InvariantCulture,
                                "uninstall {0} --{1}",
                                packageName,
                                (type == DependencyType.Standard
                                     ? "save"
                                     : (type == DependencyType.Development ? "save-dev" : "save-optional")));
        }
    }
}
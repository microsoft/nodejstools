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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm {
    public interface INpmCommander : INpmLogSource, IDisposable {

        /// <summary>
        /// Cancels the currently running command
        /// </summary>
        void CancelCurrentCommand();

        /// <summary>
        /// Executes npm install to install all packages in package.json.
        /// </summary>
        /// <returns></returns>
        Task<bool> Install();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="versionRange"></param>
        /// <param name="type"></param>
        /// <param name="saveToPackageJson"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type,
            bool saveToPackageJson);

        Task<bool> InstallPackageToFolderByVersionAsync(
            string pathToRootDirectory,
            string packageName,
            string versionRange,
            bool saveToPackageJson);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UninstallPackageAsync(string packageName);

        Task<IPackageCatalog> GetCatalogAsync(bool forceDownload, IProgress<string> progress);

        Task<bool> UpdatePackagesAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packages"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        Task<bool> ExecuteNpmCommandAsync(string arguments);
    }
}

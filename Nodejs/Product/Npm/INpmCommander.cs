using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface INpmCommander : INpmLogSource, IDisposable{

        /// <summary>
        /// Fired whenever a command is completed, regardless of whether or not it was successful.
        /// </summary>
        event EventHandler CommandCompleted;

        /// <summary>
        /// Cancels the currently running command
        /// </summary>
        void CancelCurrentCommand();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="versionRange"></param>
        /// <param name="type"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> InstallPackageByVersionAsync(
            string packageName,
            string versionRange,
            DependencyType type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="versionRange"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> InstallGlobalPackageByVersionAsync(
            string packageName,
            string versionRange);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UninstallPackageAsync(string packageName);

        Task<bool> UninstallGlobalPackageAsync(string packageName);

        Task<IEnumerable<IPackage>> SearchAsync(string searchText);

        Task<bool> UpdatePackagesAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packages"></param>
        /// <exception cref="PackageJsonException">If there is an error reading a package.json file when modules are refreshed.</exception>
        /// <returns></returns>
        Task<bool> UpdatePackagesAsync(IEnumerable<IPackage> packages);
    }
}

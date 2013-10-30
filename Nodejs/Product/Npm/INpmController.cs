using System;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface INpmController
    {
        event EventHandler StartingRefresh;
        void Refresh();
        event EventHandler FinishedRefresh;
        IRootPackage RootPackage { get; }
        Task<bool> InstallPackageByVersionAsync(string packageName, string versionRange, DependencyType type);
        Task<bool> UninstallPackageAsync(string packageName);
    }
}
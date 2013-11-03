using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmController : INpmController
    {
        private string _fullPathToRootPackageDirectory;
        private bool _showMissingDevOptionalSubPackages;
        private string _pathToNpm;

        public NpmController(
            string fullPathToRootPackageDirectory,
            bool showMissingDevOptionalSubPackages = false,
            string pathToNpm = null)
        {
            _fullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            _showMissingDevOptionalSubPackages = showMissingDevOptionalSubPackages;
            _pathToNpm = pathToNpm;
            Refresh();
        }

        public event EventHandler StartingRefresh;

        private void Fire(EventHandler handlers)
        {
            if (null != handlers)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void OnStartingRefresh()
        {
            Fire(StartingRefresh);
        }

        public event EventHandler FinishedRefresh;

        private void OnFinishedRefresh()
        {
            Fire(FinishedRefresh);
        }

        public void Refresh()
        {
            OnStartingRefresh();
            RootPackage = RootPackageFactory.Create(
                _fullPathToRootPackageDirectory,
                _showMissingDevOptionalSubPackages);
            OnFinishedRefresh();
        }

        public IRootPackage RootPackage { get; private set; }

        public async Task<bool> InstallPackageByVersionAsync(string packageName, string versionRange, DependencyType type)
        {
            var command = new NpmInstallCommand(
                _fullPathToRootPackageDirectory,
                packageName,
                versionRange,
                type,
                _pathToNpm);

            bool retVal = await command.ExecuteAsync();
            Refresh();
            return retVal;
        }

        public async Task<bool> UninstallPackageAsync(string packageName)
        {
            var command = new NpmUninstallCommand(
                _fullPathToRootPackageDirectory,
                packageName,
                _pathToNpm);

            bool retVal = await command.ExecuteAsync();
            Refresh();
            return retVal;
        }
    }
}

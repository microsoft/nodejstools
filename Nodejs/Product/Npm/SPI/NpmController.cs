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
        private string m_FullPathToRootPackageDirectory;
        private string m_PathToNpm;

        public NpmController(string fullPathToRootPackageDirectory, string pathToNpm = null)
        {
            m_FullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            m_PathToNpm = pathToNpm;
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
            RootPackage = RootPackageFactory.Create(m_FullPathToRootPackageDirectory);
            OnFinishedRefresh();
        }

        public IRootPackage RootPackage { get; private set; }

        //private void ExecuteNpmAndRefresh(string arguments)
        //{
            

        //    Refresh();
        //}

        public async Task<bool> InstallPackageByVersionAsync(string packageName, string versionRange, DependencyType type)
        {
            var command = new NpmInstallCommand(
                m_FullPathToRootPackageDirectory,
                packageName,
                versionRange,
                type,
                m_PathToNpm);

            bool retVal = await command.ExecuteAsync();
            Refresh();
            return retVal;
            ////  TODO: Method should be async?
            //ExecuteNpmAndRefresh(string.Format(
            //    "install {0} \"{1}\" --{2}",
            //    packageName,
            //    versionRange,
            //    type == DependencyType.Standard ? "save" : (type == DependencyType.Development ? "save-dev" : "save-optional")));
        }

        public async Task<bool> UninstallPackageAsync(string packageName)
        {
            var command = new NpmUninstallCommand(
                m_FullPathToRootPackageDirectory,
                packageName,
                m_PathToNpm);

            bool retVal = await command.ExecuteAsync();
            Refresh();
            return retVal;
            ////  TODO: Method should be async?
            //ExecuteNpmAndRefresh(string.Format(
            //    "uninstall {0) --save"));
        }
    }
}

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
        private IRootPackage _rootPackage;
        private readonly object _lock = new object();

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
            lock ( _lock )
            {
                RootPackage = RootPackageFactory.Create(
                    _fullPathToRootPackageDirectory,
                    _showMissingDevOptionalSubPackages );
            }
            OnFinishedRefresh();
        }

        public IRootPackage RootPackage
        {
            get
            {
                lock ( _lock )
                {
                    return _rootPackage;
                }
            }

            private set
            {
                lock ( _lock )
                {
                    _rootPackage = value;
                }
            }
        }

        //  TODO: events should be fired as data is logged, not in one massive barf at the end
        private void FireLogEvents( NpmCommand command )
        {
            OnOutputLogged( command.StandardOutput );
            OnErrorLogged( command.StandardError );
        }

        public async Task<bool> InstallPackageByVersionAsync(string packageName, string versionRange, DependencyType type)
        {
            var command = new NpmInstallCommand(
                _fullPathToRootPackageDirectory,
                packageName,
                versionRange,
                type,
                _pathToNpm);

            var retVal = await command.ExecuteAsync();
            FireLogEvents( command );
            Refresh();
            return retVal;
        }

        public async Task<bool> UninstallPackageAsync(string packageName)
        {
            var command = new NpmUninstallCommand(
                _fullPathToRootPackageDirectory,
                packageName,
                _pathToNpm);

            var retVal = await command.ExecuteAsync();
            FireLogEvents(command);
            Refresh();
            return retVal;
        }

        public async Task< IEnumerable< IPackage > > SearchAsync( string searchText )
        {
            var command = new NpmSearchCommand( _fullPathToRootPackageDirectory, searchText, _pathToNpm );
            var success = await command.ExecuteAsync();
            FireLogEvents( command );
            return success ? command.Results : new List< IPackage >();
        }

        public async Task< bool > UpdatePackagesAsync()
        {
            return await UpdatePackagesAsync( new List< IPackage >() );
        }

        public async Task< bool > UpdatePackagesAsync( IEnumerable< IPackage > packages )
        {
            var command = new NpmUpdateCommand( _fullPathToRootPackageDirectory, packages, _pathToNpm );
            var success = await command.ExecuteAsync();
            FireLogEvents( command );
            Refresh();
            return success;
        }

        private void FireNpmLogEvent( string logText, EventHandler< NpmLogEventArgs > handlers )
        {
            if (null != handlers && ! string.IsNullOrEmpty( logText ) )
            {
                handlers(this, new NpmLogEventArgs(logText));
            }
        }

        public event EventHandler< NpmLogEventArgs > OutputLogged;

        private void OnOutputLogged( string logText )
        {
            FireNpmLogEvent( logText, OutputLogged );
        }

        public event EventHandler< NpmLogEventArgs > ErrorLogged;

        private void OnErrorLogged( string logText )
        {
            FireNpmLogEvent( logText, ErrorLogged );
        }
    }
}

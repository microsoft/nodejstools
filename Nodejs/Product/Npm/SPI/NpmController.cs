/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmController : AbstractNpmLogSource, INpmController {

        //  *Really* don't want to retrieve this more than once:
        //  47,000 packages takes a while.
        private static IPackageCatalog _sRepoCatalogue;

        private string _fullPathToRootPackageDirectory;
        private bool _showMissingDevOptionalSubPackages;
        private INpmPathProvider _npmPathProvider;
        private bool _useFallbackIfNpmNotFound;
        private IRootPackage _rootPackage;
        private IGlobalPackages _globalPackage;
        private readonly object _lock = new object();

        public NpmController(
            string fullPathToRootPackageDirectory,
            bool showMissingDevOptionalSubPackages = false,
            INpmPathProvider npmPathProvider = null,
            bool useFallbackIfNpmNotFound = true) {
            _fullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            _showMissingDevOptionalSubPackages = showMissingDevOptionalSubPackages;
            _npmPathProvider = npmPathProvider;
            _useFallbackIfNpmNotFound = useFallbackIfNpmNotFound;
        }

        internal string FullPathToRootPackageDirectory {
            get { return _fullPathToRootPackageDirectory; }
        }

        internal string PathToNpm {
            get { return null == _npmPathProvider ? null : _npmPathProvider.PathToNpm; }
        }

        internal bool UseFallbackIfNpmNotFound {
            get { return _useFallbackIfNpmNotFound; }
        }

        public event EventHandler StartingRefresh;

        private void Fire(EventHandler handlers) {
            if (null != handlers) {
                handlers(this, EventArgs.Empty);
            }
        }

        private void OnStartingRefresh() {
            Fire(StartingRefresh);
        }

        public event EventHandler FinishedRefresh;

        private void OnFinishedRefresh() {
            Fire(FinishedRefresh);
        }

        public void Refresh() {
            OnStartingRefresh();
            lock (_lock) {
                try {
                    RootPackage = RootPackageFactory.Create(
                        _fullPathToRootPackageDirectory,
                        _showMissingDevOptionalSubPackages);

                    var command = new NpmLsCommand(_fullPathToRootPackageDirectory, true, PathToNpm,
                        _useFallbackIfNpmNotFound);

                    command.ExecuteAsync().ContinueWith(task => {
                        try {
                            GlobalPackages = task.Result
                                ? RootPackageFactory.Create(command.ListBaseDirectory)
                                : null;
                        } catch (IOException){} catch (AggregateException){}    //  Latter for npm not installed.
                        OnFinishedRefresh();
                    });
                } catch (IOException) {
                    // Can sometimes happen when packages are still installing because the file may still be used by another process
                }
            }
        }

        public IRootPackage RootPackage {
            get {
                lock (_lock) {
                    return _rootPackage;
                }
            }

            private set {
                lock (_lock) {
                    _rootPackage = value;
                }
            }
        }

        public IGlobalPackages GlobalPackages {
            get {
                lock (_lock) {
                    return _globalPackage;
                }
            }
            private set {
                lock (_lock) {
                    _globalPackage = value;
                }
            }
        }

        public INpmCommander CreateNpmCommander() {
            return new NpmCommander(this);
        }

        public void LogOutput(object sender, NpmLogEventArgs e) {
            OnOutputLogged(e.LogText);
        }

        public void LogError(object sender, NpmLogEventArgs e) {
            OnErrorLogged(e.LogText);
        }

        public void LogException(object sender, NpmExceptionEventArgs e) {
            OnExceptionLogged(e.Exception);
        }

        public async Task<IPackageCatalog> GetRepositoryCatalogueAsync(bool forceDownload) {
            //  This should really be thread-safe but await can't be inside a lock so
            //  we'll just have to hope and pray this doesn't happen concurrently. Worst
            //  case is we'll end up with two retrievals, one of which will be binned,
            //  which isn't the end of the world.
            if (null == _sRepoCatalogue || _sRepoCatalogue.Results.Count == 0 || forceDownload) {
                Exception ex = null;
                using (var commander = CreateNpmCommander()) {
                    commander.ExceptionLogged += (sender, args) => ex = args.Exception;
                    _sRepoCatalogue = await commander.GetCatalogueAsync(forceDownload);
                }
                if (null != ex) {
                    throw ex;
                }
            }
            return _sRepoCatalogue;
        }
    }
}
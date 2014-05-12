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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudioTools.Wpf;
using Microsoft.Windows.Design.Host;

namespace Microsoft.NodejsTools.NpmUI {
    internal class NpmPackageInstallViewModel : INotifyPropertyChanged {
        private enum Indices {
            IndexStandard = 0,
            IndexDev = 1,
            IndexOptional = 2,
            IndexGlobal = 3
        }

        public static readonly ICommand InstallCommand = new RoutedCommand();
        public static readonly ICommand RefreshCatalogCommand = new RoutedCommand();
        
        private INpmController _npmController;

        private string _lastCatalogUpdateTimeMessage = string.Empty;
        private Color _lastCatalogUpdateTimeColor = SystemColors.WindowTextColor;
        private FontWeight _lastCatalogUpdateFontWeight = FontWeights.Normal;

        private bool _isLoadingCatalog;
        private IPackageCatalog _allPackages;
        private readonly object _filteredPackagesLock = new object();
        private IList<PackageCatalogEntryViewModel> _filteredPackages = new List<PackageCatalogEntryViewModel>();
        private LastRefreshedMessageProvider _lastRefreshedMessage;
        private PackageCatalogEntryViewModel _selectedPackage;
        private bool _npmNotFound;
        private bool _isCatalogEmpty;
        private Visibility _catalogControlVisibility = Visibility.Visible;
        private string _catalogLoadingMessage = string.Empty;
        private Visibility _loadingCatalogControlVisibility = Visibility.Hidden;
        private Visibility _filteredCatalogListVisibility = Visibility.Visible;
        private int _selectedDependencyTypeIndex;

        private string _currentFilter;
        private string _filterText;
        private readonly Timer _filterTimer;

        private readonly Dispatcher _dispatcher;

        private NpmOutputControlViewModel _executeViewModel;

        private readonly InstallCommandPackageCatalogEntryViewModel _installCommand =
            new InstallCommandPackageCatalogEntryViewModel();
        
        public NpmPackageInstallViewModel(
            NpmOutputControlViewModel executeViewModel,
            Dispatcher dispatcher
        ) {
            _dispatcher = dispatcher;

            _executeViewModel = executeViewModel;
            _filterTimer = new Timer(FilterTimer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public INpmController NpmController {
            get { return _npmController; }
            set {
                if (null != _npmController) {
                    _npmController.FinishedRefresh -= NpmController_FinishedRefresh;
                }
                _npmController = value;
                OnPropertyChanged();
                if (null != _npmController) {
                    LoadCatalog();
                    _npmController.FinishedRefresh += NpmController_FinishedRefresh;
                }
            }
        }

        void NpmController_FinishedRefresh(object sender, EventArgs e) {
            StartFilter();
        }

        public NpmOutputControlViewModel ExecuteViewModel {
            get { return _executeViewModel; }
        }

        #region Catalog control and refresh

        public string LastCatalogUpdateTimeMessage {
            get { return _lastCatalogUpdateTimeMessage; }
            private set {
                _lastCatalogUpdateTimeMessage = value;
                OnPropertyChanged();
            }
        }


        public bool IsLoadingCatalog {
            get { return _isLoadingCatalog; }
            private set {
                _isLoadingCatalog = value;
                OnPropertyChanged();
                OnPropertyChanged("CanRefreshCatalog");
            }
        }

        public bool CanRefreshCatalog {
            get { return !IsLoadingCatalog; }
        }

        public bool NpmNotFound {
            get { return _npmNotFound; }
            private set {
                _npmNotFound = value;
                OnPropertyChanged();
            }
        }

        public bool IsCatalogEmpty {
            get { return _isCatalogEmpty; }
            private set {
                _isCatalogEmpty = value;
                OnPropertyChanged();
            }
        }

        public string LoadingCatalogMessage {
            get { return _catalogLoadingMessage; }
            private set {
                _catalogLoadingMessage = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadingCatalogControlVisibility {
            get { return _loadingCatalogControlVisibility; }
            set {
                _loadingCatalogControlVisibility = value;
                OnPropertyChanged();
                OnPropertyChanged("FilterControlsVisibility");
            }
        }

        public Visibility FilteredCatalogListVisibility {
            get { return _filteredCatalogListVisibility; }
            set {
                _filteredCatalogListVisibility = value;
                OnPropertyChanged();
            }
        }

        private async void LoadCatalog(bool forceRefresh) {
            IsLoadingCatalog = true;

            FilteredCatalogListVisibility = Visibility.Hidden;
            CatalogControlVisibility = Visibility.Hidden;
            LoadingCatalogControlVisibility = Visibility.Visible;
            LoadingCatalogMessage = SR.GetString(SR.CatalogLoadingDefault);

            LastRefreshedMessage = LastRefreshedMessageProvider.RefreshInProgress;

            bool showList = false;

            var controller = _npmController;
            controller.ErrorLogged += _executeViewModel.commander_ErrorLogged;
            controller.ExceptionLogged += _executeViewModel.commander_ExceptionLogged;
            _executeViewModel.SetCancellableSafe(false);
            try {
                _allPackages = await controller.GetRepositoryCatalogueAsync(forceRefresh);
                IsCatalogEmpty = false;
                showList = true;
            } catch (NpmNotFoundException) {
                LoadingCatalogMessage = SR.GetString(SR.CatalogLoadingNoNpm);
            } catch (NpmCatalogEmptyException) {
                IsCatalogEmpty = true;
                showList = true;
            } finally {
                controller.ErrorLogged -= _executeViewModel.commander_ErrorLogged;
                controller.ExceptionLogged -= _executeViewModel.commander_ExceptionLogged;
                _executeViewModel.SetCancellableSafe(true);
            }

            IsLoadingCatalog = false;

            if (showList) {
                LoadingCatalogControlVisibility = Visibility.Hidden;
                FilteredCatalogListVisibility = Visibility.Visible;
                StartFilter();
            }
        }

        public void LoadCatalog() {
            LoadCatalog(false);
        }

        public void RefreshCatalog() {
            LoadCatalog(true);
        }

        public Visibility CatalogControlVisibility {
            get { return _catalogControlVisibility; }
            set {
                _catalogControlVisibility = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Filtering

        public IList<PackageCatalogEntryViewModel> FilteredPackages {
            get {
                lock (_filteredPackagesLock) {
                    return _filteredPackages;
                }
            }
            set {
                lock (_filteredPackagesLock) {
                    _filteredPackages = value;
                }
                OnPropertyChanged();
            }
        }

        public string FilterText {
            get { return _filterText; }
            set {
                _filterText = value;
                _installCommand.Command = value;
                if (_currentFilter != _filterText) {
                    StartFilter();
                }
                OnPropertyChanged();
            }
        }

        private void StartFilter() {
            _filterTimer.Change(500, Timeout.Infinite);
        }

        private void FilterTimer_Elapsed(object state) {
            List<PackageCatalogEntryViewModel> newItems = null;

            var filterText = _filterText;
            var filtered = PackageCatalogFilterFactory.Create(_allPackages).Filter(filterText);

            if (filtered.Any()) {
                IRootPackage rootPackage = null;
                IGlobalPackages globalPackages = null;
                var controller = _npmController;
                if (controller != null) {
                    rootPackage = controller.RootPackage;
                    globalPackages = controller.GlobalPackages;
                }

                newItems = new List<PackageCatalogEntryViewModel> {
                    _installCommand
                };

                if (rootPackage != null && globalPackages != null) {
                newItems.AddRange(filtered.Select(package => new ReadOnlyPackageCatalogEntryViewModel(
                    package,
                    rootPackage != null ? rootPackage.Modules[package.Name] : null,
                    globalPackages != null ? globalPackages.Modules[package.Name] : null
                )));
            }
            }

            _dispatcher.BeginInvoke((Action)(() => {
                if (newItems != null) {
                    lock (_filteredPackagesLock) {
                        FilteredPackages = newItems;
                        _currentFilter = filterText;
                    }
                }
                SelectedPackage = _installCommand;

                LastRefreshedMessage = IsCatalogEmpty
                    ? LastRefreshedMessageProvider.RefreshFailed
                    : new LastRefreshedMessageProvider(_allPackages.LastRefreshed);
                CatalogControlVisibility = Visibility.Visible;
            }));
        }

        public LastRefreshedMessageProvider LastRefreshedMessage {
            get { return _lastRefreshedMessage; }
            set {
                _lastRefreshedMessage = value;
                OnPropertyChanged();
            }
        }

        private static string GetFilterStringPortion(string source) {
            if (string.IsNullOrEmpty(source)) {
                return source;
            }

            for (int index = 0, size = source.Length; index < size; ++index) {
                if (char.IsWhiteSpace(source[index])) {
                    return index == 0 ? string.Empty : source.Substring(0, index);
                }
            }

            return source;
        }


        public Visibility FilterControlsVisibility {
            get { return LoadingCatalogControlVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; }
        }

        #endregion

        #region Installation

        public int SelectedDependencyTypeIndex {
            get { return _selectedDependencyTypeIndex; }
            set {
                _selectedDependencyTypeIndex = value;
                OnPropertyChanged();
                OnPropertyChanged("GlobalWarningVisibility");
            }
        }

        public Visibility GlobalWarningVisibility {
            get {
                return Indices.IndexGlobal == (Indices) SelectedDependencyTypeIndex
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        internal bool CanInstall(PackageCatalogEntryViewModel package) {
            var install = package as InstallCommandPackageCatalogEntryViewModel;
            if (install != null) {
                return !string.IsNullOrEmpty(install.Command);
            }

            return package != null;
        }

        internal void Install(PackageCatalogEntryViewModel package) {
            var type = DependencyType.Standard;
            var global = false;
            switch ((Indices)SelectedDependencyTypeIndex) {
                case Indices.IndexDev:
                    type = DependencyType.Development;
                    break;

                case Indices.IndexOptional:
                    type = DependencyType.Optional;
                    break;

                case Indices.IndexGlobal:
                    global = true;
                    break;
            }

            var install = package as InstallCommandPackageCatalogEntryViewModel;
            if (install != null) {
                if (!string.IsNullOrEmpty(install.Command)) {
                    _executeViewModel.QueueCommand("install " + install.Command);
                }
            } else if (package != null) {
                if (global) {
                    _executeViewModel.QueueInstallGlobalPackage(package.Name, "*");
                } else {
                    _executeViewModel.QueueInstallPackage(package.Name, "*", type);
                }
            }
        }

        public PackageCatalogEntryViewModel SelectedPackage {
            get { return _selectedPackage; }
            set {
                _selectedPackage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Dialog control

        #endregion
    }
}

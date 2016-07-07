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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.NpmUI {
    internal class NpmPackageInstallViewModel : INotifyPropertyChanged {
        internal enum Indices {
            IndexStandard = 0,
            IndexDev = 1,
            IndexOptional = 2,
            IndexGlobal = 3
        }

        internal enum FilterState {
            NoFilterText,
            Filtering,
            ResultsAvailable,
            NoResults
        }

        public static readonly ICommand InstallCommand = new RoutedCommand();
        public static readonly ICommand OpenHomepageCommand = new RoutedCommand();
        public static readonly ICommand RefreshCatalogCommand = new RoutedCommand();
        
        private INpmController _npmController;

        private bool _isFiltering = false;
        private bool _isLoadingCatalog;
        private IPackageCatalog _allPackages;
        private readonly object _filteredPackagesLock = new object();
        private IList<PackageCatalogEntryViewModel> _filteredPackages = new List<PackageCatalogEntryViewModel>();
        private LastRefreshedMessageProvider _lastRefreshedMessage;
        private PackageCatalogEntryViewModel _selectedPackage;
        private bool _isCatalogEmpty;
        private Visibility _catalogControlVisibility = Visibility.Collapsed;
        private string _catalogLoadingMessage = string.Empty;
        private string _catalogLoadingProgressMessage = string.Empty;
        private Visibility _loadingCatalogControlVisibility = Visibility.Collapsed;
        private int _selectedDependencyTypeIndex;

        private string _filterText = string.Empty;
        private readonly Timer _filterTimer;
        private string _arguments = string.Empty;
        private bool _saveToPackageJson = true;
        private object _selectedVersion;

        private readonly Dispatcher _dispatcher;

        private readonly NpmOutputViewModel _executeViewModel;
        
        public NpmPackageInstallViewModel(
            NpmOutputViewModel executeViewModel,
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

        public NpmOutputViewModel ExecuteViewModel {
            get { return _executeViewModel; }
        }

        #region Catalog control and refresh
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

        public string LoadingCatalogProgressMessage {
            get { return _catalogLoadingProgressMessage; }
            private set {
                _catalogLoadingProgressMessage = value;
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
        
        private async void LoadCatalog(bool forceRefresh) {
            IsLoadingCatalog = true;

            CatalogControlVisibility = Visibility.Collapsed;
            LoadingCatalogControlVisibility = Visibility.Visible;
            LoadingCatalogMessage = SR.GetString(SR.CatalogLoadingDefault);

            LastRefreshedMessage = LastRefreshedMessageProvider.RefreshInProgress;

            var controller = _npmController;
            controller.ErrorLogged += _executeViewModel.commander_ErrorLogged;
            controller.ExceptionLogged += _executeViewModel.commander_ExceptionLogged;
            controller.OutputLogged += _executeViewModel.commander_OutputLogged;
            _executeViewModel.SetCancellableSafe(false);
            try {
                _allPackages = await controller.GetRepositoryCatalogAsync(
                    forceRefresh,
                    new Progress<string>(msg => LoadingCatalogProgressMessage = msg)
                );
                IsCatalogEmpty = false;
            } catch (NpmNotFoundException) {
                LastRefreshedMessage = LastRefreshedMessageProvider.NpmNotFound;
            } catch (NpmCatalogEmptyException) {
                IsCatalogEmpty = true;
                LastRefreshedMessage = new LastRefreshedMessageProvider(_allPackages.LastRefreshed);
            } catch (Exception ex) {
                if (IsCriticalException(ex)) {
                    throw;
                }

                LastRefreshedMessage = LastRefreshedMessageProvider.RefreshFailed;
                IsCatalogEmpty = true;
            } finally {
                IsLoadingCatalog = false;
                controller.ErrorLogged -= _executeViewModel.commander_ErrorLogged;
                controller.ExceptionLogged -= _executeViewModel.commander_ExceptionLogged;
                controller.OutputLogged -= _executeViewModel.commander_OutputLogged;
                _executeViewModel.SetCancellableSafe(true);

                // The catalog refresh operation spawns many long-lived Gen 2 objects,
                // so the garbage collector will take a while to get to them otherwise.
                GC.Collect();
            }

            // Reset the filter text, otherwise the results will be outdated.
            FilterText = string.Empty;

            // We want to show the catalog regardless of whether an exception was thrown so that the user has the chance to refresh it.
            LoadingCatalogControlVisibility = Visibility.Collapsed;
            
            StartFilter();
        }

        private static bool IsCriticalException(Exception ex) {
            return ex is StackOverflowException ||
                   ex is OutOfMemoryException ||
                   ex is ThreadAbortException ||
                   ex is AccessViolationException;
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
        public FilterState PackageFilterState {
            get {
                if (IsFiltering) {
                    return FilterState.Filtering;
                }
                if (string.IsNullOrEmpty(FilterText)) {
                    return FilterState.NoFilterText;
                }
                if (!FilteredPackages.Any()) {
                    return FilterState.NoResults;
                }
                return FilterState.ResultsAvailable;
            }
        }

        private bool IsFiltering {
            get { return _isFiltering; }
            set {
                _isFiltering = value;
                OnPropertyChanged();
                OnPropertyChanged("PackageFilterState");
            }
        }

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

                // PackageFilterState should be triggered before FilteredPackages
                // to allow the UI to update the status before the package list,
                // making for a smoother experience.
                OnPropertyChanged("PackageFilterState");
                OnPropertyChanged();
            }
        }

        public string FilterText {
            get { return _filterText; }
            set {
                _filterText = value;

                StartFilter();
                IsFiltering = !string.IsNullOrWhiteSpace(_filterText);

                OnPropertyChanged();
                OnPropertyChanged("PackageFilterState");
            }
        }

        private void StartFilter() {
            _filterTimer.Change(300, Timeout.Infinite);
        }

        private async void FilterTimer_Elapsed(object state) {
            if (_allPackages == null) {
                LastRefreshedMessage = LastRefreshedMessageProvider.RefreshFailed;
                IsFiltering = false;
                return;
            }

            var filterText = GetTrimmedTextSafe(_filterText);

            IEnumerable<IPackage> filtered;
            if (string.IsNullOrWhiteSpace(filterText)) {
                filtered = Enumerable.Empty<IPackage>();
            } else {
                try {
                    filtered = await _allPackages.GetCatalogPackagesAsync(filterText);
                } catch (Exception ex) {
                    LastRefreshedMessage = LastRefreshedMessageProvider.RefreshFailed;
                    if (IsCriticalException(ex)) {
                        throw;
                    }
                    StartFilter();
                    return;
                }
            }

            if (filtered == null) {
                // The database file must be in use. Display current results, but try again later.
                LastRefreshedMessage = LastRefreshedMessageProvider.RefreshInProgress;
                StartFilter();
                return;
            }

            var newItems = new List<PackageCatalogEntryViewModel>();
            if (filterText != GetTrimmedTextSafe(_filterText)) {
                return;
            }

            if (filtered.Any()) {
                IRootPackage rootPackage = null;
                var controller = _npmController;
                if (controller != null) {
                    rootPackage = controller.RootPackage;
                }

                newItems.AddRange(filtered.Select(package => new ReadOnlyPackageCatalogEntryViewModel(
                    package,
                    rootPackage != null ? rootPackage.Modules[package.Name] : null,
                    null)));
            }

            await _dispatcher.BeginInvoke((Action)(() => {
                if (filterText != GetTrimmedTextSafe(_filterText)) {
                    return;
                }

                var originalSelectedPackage = SelectedPackage;
                FilteredPackages = newItems;

                // Reassign originalSelectedPackage to the original selected package in the new list of filtered packages.
                if (originalSelectedPackage != null) {
                    originalSelectedPackage = FilteredPackages.FirstOrDefault(package => package.Name == originalSelectedPackage.Name);
                }

                // Maintain selection when the filter list refreshes (e.g. due to an installation running in the background)
                SelectedPackage = originalSelectedPackage ?? FilteredPackages.FirstOrDefault();

                LastRefreshedMessage = IsCatalogEmpty
                    ? LastRefreshedMessageProvider.RefreshFailed
                    : new LastRefreshedMessageProvider(_allPackages.LastRefreshed);
                CatalogControlVisibility = Visibility.Visible;
            }));

            IsFiltering = false;

            // The catalog refresh operation spawns many long-lived Gen 2 objects,
            // so the garbage collector will take a while to get to them otherwise.
            GC.Collect();
        }

        private string GetTrimmedTextSafe(string text) {
            return text != null ? text.Trim() : string.Empty;
        }

        public LastRefreshedMessageProvider LastRefreshedMessage {
            get { return _lastRefreshedMessage; }
            set {
                _lastRefreshedMessage = value;
                OnPropertyChanged();
            }
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

        public object SelectedVersion {
            get { return _selectedVersion; }
            set {
                _selectedVersion = value;
                OnPropertyChanged();
            }
        }

        public Visibility GlobalWarningVisibility {
            get {
                return Indices.IndexGlobal == (Indices) SelectedDependencyTypeIndex
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        internal bool CanInstall(PackageCatalogEntryViewModel package) {
            return package != null;
        }

        internal void Install(PackageCatalogEntryViewModel package) {
            var type = DependencyType.Standard;
            var isGlobal = false;
            switch ((Indices)SelectedDependencyTypeIndex) {
                case Indices.IndexDev:
                    type = DependencyType.Development;
                    break;

                case Indices.IndexOptional:
                    type = DependencyType.Optional;
                    break;

                case Indices.IndexGlobal:
                    isGlobal = true;
                    break;
            }

            if (!string.IsNullOrEmpty(package.Name)) {
                var selectedVersion = SelectedVersion is SemverVersion ? ((SemverVersion)SelectedVersion).ToString(): string.Empty;
                _executeViewModel.QueueCommand(
                    NpmArgumentBuilder.GetNpmInstallArguments(
                        package.Name, 
                        selectedVersion, 
                        type, 
                        isGlobal, 
                        SaveToPackageJson, 
                        Arguments));
            }
        }

        internal bool CanOpenHomepage(string homepage) {
            return !string.IsNullOrEmpty(homepage);
        }

        internal void OpenHomepage(string homepage) {
            if (!string.IsNullOrEmpty(homepage)) {
                Process.Start(homepage);
            }
        }

        public string Arguments {
            get { return _arguments; }
            set {
                _arguments = value;
                OnPropertyChanged();
            }
        }

        public bool SaveToPackageJson {
            get { return _saveToPackageJson; }
            set {
                _saveToPackageJson = value;
                OnPropertyChanged();
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
    }
}

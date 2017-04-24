// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    internal class NpmPackageInstallViewModel : INotifyPropertyChanged
    {
        internal enum Indices
        {
            IndexStandard = 0,
            IndexDev = 1,
            IndexOptional = 2
        }

        internal enum FilterState
        {
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
        )
        {
            this._dispatcher = dispatcher;

            this._executeViewModel = executeViewModel;
            this._filterTimer = new Timer(this.FilterTimer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public INpmController NpmController
        {
            get { return this._npmController; }
            set
            {
                if (null != this._npmController)
                {
                    this._npmController.FinishedRefresh -= this.NpmController_FinishedRefresh;
                }
                this._npmController = value;
                OnPropertyChanged();
                if (null != this._npmController)
                {
                    LoadCatalog();
                    this._npmController.FinishedRefresh += this.NpmController_FinishedRefresh;
                }
            }
        }

        private void NpmController_FinishedRefresh(object sender, EventArgs e)
        {
            StartFilter();
        }

        public NpmOutputViewModel ExecuteViewModel => this._executeViewModel;
        #region Catalog control and refresh
        public bool IsLoadingCatalog
        {
            get { return this._isLoadingCatalog; }
            private set
            {
                this._isLoadingCatalog = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRefreshCatalog));
            }
        }

        public bool CanRefreshCatalog => !this.IsLoadingCatalog;
        public bool IsCatalogEmpty
        {
            get { return this._isCatalogEmpty; }
            private set
            {
                this._isCatalogEmpty = value;
                OnPropertyChanged();
            }
        }

        public string LoadingCatalogMessage
        {
            get { return this._catalogLoadingMessage; }
            private set
            {
                this._catalogLoadingMessage = value;
                OnPropertyChanged();
            }
        }

        public string LoadingCatalogProgressMessage
        {
            get { return this._catalogLoadingProgressMessage; }
            private set
            {
                this._catalogLoadingProgressMessage = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadingCatalogControlVisibility
        {
            get { return this._loadingCatalogControlVisibility; }
            set
            {
                this._loadingCatalogControlVisibility = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilterControlsVisibility));
            }
        }

        private async void LoadCatalog(bool forceRefresh)
        {
            this.IsLoadingCatalog = true;

            this.CatalogControlVisibility = Visibility.Collapsed;
            this.LoadingCatalogControlVisibility = Visibility.Visible;
            this.LoadingCatalogMessage = Resources.CatalogLoadingDefault;

            this.LastRefreshedMessage = LastRefreshedMessageProvider.RefreshInProgress;

            var controller = this._npmController;
            controller.ErrorLogged += this._executeViewModel.commander_ErrorLogged;
            controller.ExceptionLogged += this._executeViewModel.commander_ExceptionLogged;
            controller.OutputLogged += this._executeViewModel.commander_OutputLogged;
            this._executeViewModel.SetCancellableSafe(false);
            try
            {
                this._allPackages = await controller.GetRepositoryCatalogAsync(
                    forceRefresh,
                    new Progress<string>(msg => this.LoadingCatalogProgressMessage = msg)
                );
                this.IsCatalogEmpty = false;
            }
            catch (NpmNotFoundException)
            {
                this.LastRefreshedMessage = LastRefreshedMessageProvider.NpmNotFound;
            }
            catch (NpmCatalogEmptyException)
            {
                this.IsCatalogEmpty = true;
                this.LastRefreshedMessage = new LastRefreshedMessageProvider(this._allPackages.LastRefreshed);
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                {
                    throw;
                }

                this.LastRefreshedMessage = LastRefreshedMessageProvider.RefreshFailed;
                this.IsCatalogEmpty = true;
            }
            finally
            {
                this.IsLoadingCatalog = false;
                controller.ErrorLogged -= this._executeViewModel.commander_ErrorLogged;
                controller.ExceptionLogged -= this._executeViewModel.commander_ExceptionLogged;
                controller.OutputLogged -= this._executeViewModel.commander_OutputLogged;
                this._executeViewModel.SetCancellableSafe(true);

                // The catalog refresh operation spawns many long-lived Gen 2 objects,
                // so the garbage collector will take a while to get to them otherwise.
                GC.Collect();
            }

            // Reset the filter text, otherwise the results will be outdated.
            this.FilterText = string.Empty;

            // We want to show the catalog regardless of whether an exception was thrown so that the user has the chance to refresh it.
            this.LoadingCatalogControlVisibility = Visibility.Collapsed;

            StartFilter();
        }

        private static bool IsCriticalException(Exception ex)
        {
            return ex is StackOverflowException ||
                   ex is OutOfMemoryException ||
                   ex is ThreadAbortException ||
                   ex is AccessViolationException;
        }

        public void LoadCatalog()
        {
            LoadCatalog(false);
        }

        public void RefreshCatalog()
        {
            LoadCatalog(true);
        }

        public Visibility CatalogControlVisibility
        {
            get { return this._catalogControlVisibility; }
            set
            {
                this._catalogControlVisibility = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Filtering
        public FilterState PackageFilterState
        {
            get
            {
                if (this.IsFiltering)
                {
                    return FilterState.Filtering;
                }
                if (string.IsNullOrEmpty(this.FilterText))
                {
                    return FilterState.NoFilterText;
                }
                if (!this.FilteredPackages.Any())
                {
                    return FilterState.NoResults;
                }
                return FilterState.ResultsAvailable;
            }
        }

        private bool IsFiltering
        {
            get { return this._isFiltering; }
            set
            {
                this._isFiltering = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PackageFilterState));
            }
        }

        public IList<PackageCatalogEntryViewModel> FilteredPackages
        {
            get
            {
                lock (this._filteredPackagesLock)
                {
                    return this._filteredPackages;
                }
            }
            set
            {
                lock (this._filteredPackagesLock)
                {
                    this._filteredPackages = value;
                }

                // PackageFilterState should be triggered before FilteredPackages
                // to allow the UI to update the status before the package list,
                // making for a smoother experience.
                OnPropertyChanged(nameof(PackageFilterState));
                OnPropertyChanged();
            }
        }

        public string FilterText
        {
            get { return this._filterText; }
            set
            {
                this._filterText = value;

                StartFilter();
                this.IsFiltering = !string.IsNullOrWhiteSpace(this._filterText);

                OnPropertyChanged();
                OnPropertyChanged(nameof(PackageFilterState));
            }
        }

        private void StartFilter()
        {
            this._filterTimer.Change(300, Timeout.Infinite);
        }

        private async void FilterTimer_Elapsed(object state)
        {
            if (this._allPackages == null)
            {
                this.LastRefreshedMessage = LastRefreshedMessageProvider.RefreshFailed;
                this.IsFiltering = false;
                return;
            }

            var filterText = GetTrimmedTextSafe(this._filterText);

            IEnumerable<IPackage> filtered;
            if (string.IsNullOrWhiteSpace(filterText))
            {
                filtered = Enumerable.Empty<IPackage>();
            }
            else
            {
                try
                {
                    filtered = await this._allPackages.GetCatalogPackagesAsync(filterText);
                }
                catch (Exception ex)
                {
                    this.LastRefreshedMessage = LastRefreshedMessageProvider.RefreshFailed;
                    if (IsCriticalException(ex))
                    {
                        throw;
                    }
                    StartFilter();
                    return;
                }
            }

            if (filtered == null)
            {
                // The database file must be in use. Display current results, but try again later.
                this.LastRefreshedMessage = LastRefreshedMessageProvider.RefreshInProgress;
                StartFilter();
                return;
            }

            var newItems = new List<PackageCatalogEntryViewModel>();
            if (filterText != GetTrimmedTextSafe(this._filterText))
            {
                return;
            }

            if (filtered.Any())
            {
                IRootPackage rootPackage = null;
                var controller = this._npmController;
                if (controller != null)
                {
                    rootPackage = controller.RootPackage;
                }

                newItems.AddRange(filtered.Select(package => new ReadOnlyPackageCatalogEntryViewModel(
                    package,
                    rootPackage != null ? rootPackage.Modules[package.Name] : null)));
            }

            await this._dispatcher.BeginInvoke((Action)(() =>
            {
                if (filterText != GetTrimmedTextSafe(this._filterText))
                {
                    return;
                }

                var originalSelectedPackage = this.SelectedPackage;
                this.FilteredPackages = newItems;

                // Reassign originalSelectedPackage to the original selected package in the new list of filtered packages.
                if (originalSelectedPackage != null)
                {
                    originalSelectedPackage = this.FilteredPackages.FirstOrDefault(package => package.Name == originalSelectedPackage.Name);
                }

                // Maintain selection when the filter list refreshes (e.g. due to an installation running in the background)
                this.SelectedPackage = originalSelectedPackage ?? this.FilteredPackages.FirstOrDefault();

                this.LastRefreshedMessage = this.IsCatalogEmpty
                    ? LastRefreshedMessageProvider.RefreshFailed
                    : new LastRefreshedMessageProvider(this._allPackages.LastRefreshed);
                this.CatalogControlVisibility = Visibility.Visible;
            }));

            this.IsFiltering = false;

            // The catalog refresh operation spawns many long-lived Gen 2 objects,
            // so the garbage collector will take a while to get to them otherwise.
            GC.Collect();
        }

        private string GetTrimmedTextSafe(string text)
        {
            return text != null ? text.Trim() : string.Empty;
        }

        public LastRefreshedMessageProvider LastRefreshedMessage
        {
            get { return this._lastRefreshedMessage; }
            set
            {
                this._lastRefreshedMessage = value;
                OnPropertyChanged();
            }
        }

        public Visibility FilterControlsVisibility => this.LoadingCatalogControlVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        #endregion

        #region Installation

        public int SelectedDependencyTypeIndex
        {
            get { return this._selectedDependencyTypeIndex; }
            set
            {
                this._selectedDependencyTypeIndex = value;
                OnPropertyChanged();
            }
        }

        public object SelectedVersion
        {
            get { return this._selectedVersion; }
            set
            {
                this._selectedVersion = value;
                OnPropertyChanged();
            }
        }

        internal bool CanInstall(PackageCatalogEntryViewModel package)
        {
            return package != null;
        }

        internal void Install(PackageCatalogEntryViewModel package)
        {
            var type = DependencyType.Standard;
            switch ((Indices)this.SelectedDependencyTypeIndex)
            {
                case Indices.IndexDev:
                    type = DependencyType.Development;
                    break;

                case Indices.IndexOptional:
                    type = DependencyType.Optional;
                    break;
            }

            if (!string.IsNullOrEmpty(package.Name))
            {
                var selectedVersion = this.SelectedVersion is SemverVersion ? ((SemverVersion)this.SelectedVersion).ToString() : string.Empty;
                this._executeViewModel.QueueCommand(
                    NpmArgumentBuilder.GetNpmInstallArguments(
                        package.Name,
                        selectedVersion,
                        type,
                        false,
                        this.SaveToPackageJson,
                        this.Arguments));
            }
        }

        internal bool CanOpenHomepage(string homepage)
        {
            return !string.IsNullOrEmpty(homepage);
        }

        internal void OpenHomepage(string homepage)
        {
            if (!string.IsNullOrEmpty(homepage))
            {
                Process.Start(homepage);
            }
        }

        public string Arguments
        {
            get { return this._arguments; }
            set
            {
                this._arguments = value;
                OnPropertyChanged();
            }
        }

        public bool SaveToPackageJson
        {
            get { return this._saveToPackageJson; }
            set
            {
                this._saveToPackageJson = value;
                OnPropertyChanged();
            }
        }

        public PackageCatalogEntryViewModel SelectedPackage
        {
            get { return this._selectedPackage; }
            set
            {
                this._selectedPackage = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}


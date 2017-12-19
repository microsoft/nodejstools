// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.NodejsTools.NpmUI
{
    internal class NpmPackageInstallViewModel : INotifyPropertyChanged, IDisposable
    {
        internal enum Indices
        {
            IndexStandard = 0,
            IndexDev = 1,
            IndexOptional = 2,
        }

        internal static class FilterState
        {
            public const string NoFilterText = nameof(NoFilterText);
            public const string Filtering = nameof(Filtering);
            public const string ResultsAvailable = nameof(ResultsAvailable);
            public const string NoResults = nameof(NoResults);
        }

        public static readonly ICommand InstallCommand = new RoutedCommand();
        public static readonly ICommand OpenHomepageCommand = new RoutedCommand();

        private INpmController npmController;

        private bool isFiltering = false;
        private IList<PackageCatalogEntryViewModel> filteredPackages = new List<PackageCatalogEntryViewModel>();
        private PackageCatalogEntryViewModel selectedPackage;

        private int selectedDependencyTypeIndex;
        private string filterText = string.Empty;
        private string arguments = string.Empty;
        private bool saveToPackageJson = true;
        private bool isExecutingCommand = false;
        private object selectedVersion;

        private readonly object filteredPackagesLock = new object();

        private readonly Timer filterTimer;
        private readonly Dispatcher dispatcher;
        private readonly NpmWorker npmWorker;

        private bool disposed = false;

        public NpmPackageInstallViewModel(
            NpmWorker npmWorker,
            Dispatcher dispatcher
        )
        {
            this.dispatcher = dispatcher;

            this.npmWorker = npmWorker;
            this.npmWorker.CommandStarted += this.NpmWorker_CommandStarted;
            this.npmWorker.CommandCompleted += this.NpmWorker_CommandCompleted;
            this.filterTimer = new Timer(this.FilterTimer_Elapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void NpmWorker_CommandStarted(object sender, EventArgs e)
        {
            this.IsExecutingCommand = true;
        }

        private void NpmWorker_CommandCompleted(object sender, NpmCommandCompletedEventArgs e)
        {
            this.IsExecutingCommand = false;
        }

        public INpmController NpmController
        {
            get { return this.npmController; }
            set
            {
                if (this.npmController != null)
                {
                    this.npmController.FinishedRefresh -= this.NpmController_FinishedRefresh;
                }
                this.npmController = value;
                OnPropertyChanged();
                if (this.npmController != null)
                {
                    LoadCatalog();
                    this.npmController.FinishedRefresh += this.NpmController_FinishedRefresh;
                }
            }
        }

        private void NpmController_FinishedRefresh(object sender, EventArgs e)
        {
            StartFilter();
        }

        #region Catalog control and refresh

        private void LoadCatalog()
        {
            // Reset the filter text, otherwise the results will be outdated.
            this.FilterText = string.Empty;
            StartFilter();
        }

        private static bool IsCriticalException(Exception ex)
        {
            return ex is StackOverflowException ||
                   ex is OutOfMemoryException ||
                   ex is ThreadAbortException ||
                   ex is AccessViolationException;
        }

        #endregion

        #region Filtering

        public string PackageFilterState
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
            get { return this.isFiltering; }
            set
            {
                this.isFiltering = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PackageFilterState));
            }
        }

        public IList<PackageCatalogEntryViewModel> FilteredPackages
        {
            get
            {
                lock (this.filteredPackagesLock)
                {
                    return this.filteredPackages;
                }
            }
            set
            {
                lock (this.filteredPackagesLock)
                {
                    this.filteredPackages = value;
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
            get { return this.filterText; }
            set
            {
                this.filterText = value;

                StartFilter();
                this.IsFiltering = !string.IsNullOrWhiteSpace(this.filterText);

                OnPropertyChanged();
                OnPropertyChanged(nameof(PackageFilterState));
            }
        }

        private void StartFilter()
        {
            this.filterTimer.Change(300, Timeout.Infinite);
        }

        private async void FilterTimer_Elapsed(object state)
        {
            var filterText = GetTrimmedTextSafe(this.filterText);

            IEnumerable<IPackage> filtered;
            if (string.IsNullOrWhiteSpace(filterText))
            {
                filtered = Enumerable.Empty<IPackage>();
            }
            else
            {
                try
                {
                    filtered = await this.npmWorker.GetCatalogPackagesAsync(filterText);
                }
                catch (Exception ex)
                {
                    if (IsCriticalException(ex))
                    {
                        throw;
                    }
                    StartFilter();
                    return;
                }
            }

            var newItems = new List<PackageCatalogEntryViewModel>();
            if (filterText != GetTrimmedTextSafe(this.filterText))
            {
                return;
            }

            if (filtered.Any())
            {
                newItems.AddRange(filtered.Select(package => new ReadOnlyPackageCatalogEntryViewModel(
                    package,
                    this.npmController?.RootPackage?.Modules[package.Name])));
            }

            await this.dispatcher.BeginInvoke((Action)(() =>
            {
                if (filterText != GetTrimmedTextSafe(this.filterText))
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
            }));

            this.IsFiltering = false;

            // The catalog refresh operation spawns many long-lived Gen 2 objects,
            // so the garbage collector will take a while to get to them otherwise.
            GC.Collect();
        }

        private string GetTrimmedTextSafe(string text) => text?.Trim() ?? string.Empty;

        public Visibility FilterControlsVisibility => Visibility.Visible;
        #endregion

        #region Installation

        public int SelectedDependencyTypeIndex
        {
            get { return this.selectedDependencyTypeIndex; }
            set
            {
                this.selectedDependencyTypeIndex = value;
                OnPropertyChanged();
            }
        }

        public object SelectedVersion
        {
            get { return this.selectedVersion; }
            set
            {
                this.selectedVersion = value;
                OnPropertyChanged();
            }
        }

        internal bool CanInstall(PackageCatalogEntryViewModel package) => package != null;

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

                TelemetryHelper.LogInstallNpmPackage();

                this.npmWorker.QueueCommand(
                    NpmArgumentBuilder.GetNpmInstallArguments(
                        package.Name,
                        selectedVersion,
                        type,
                        false,
                        this.SaveToPackageJson,
                        this.Arguments));
            }
        }

        internal bool CanOpenHomepage(string homepage) => !string.IsNullOrEmpty(homepage);

        internal void OpenHomepage(string homepage)
        {
            if (!string.IsNullOrEmpty(homepage))
            {
                VsShellUtilities.OpenBrowser(homepage);
            }
        }

        public string Arguments
        {
            get { return this.arguments; }
            set
            {
                this.arguments = value;
                OnPropertyChanged();
            }
        }

        public bool SaveToPackageJson
        {
            get { return this.saveToPackageJson; }
            set
            {
                this.saveToPackageJson = value;
                OnPropertyChanged();
            }
        }

        public PackageCatalogEntryViewModel SelectedPackage
        {
            get { return this.selectedPackage; }
            set
            {
                this.selectedPackage = value;
                OnPropertyChanged();
            }
        }

        public bool IsExecutingCommand
        {
            get
            {
                return this.isExecutingCommand;
            }
            set
            {
                if (this.isExecutingCommand != value)
                {
                    this.isExecutingCommand = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ExecutionProgressVisibility));
                }
            }
        }

        public Visibility ExecutionProgressVisibility
        {
            get { return IsExecutingCommand ? Visibility.Visible : Visibility.Collapsed; }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.filterTimer?.Dispose();

                    var controller = this.npmController;
                    if (controller != null)
                    {
                        controller.FinishedRefresh -= this.NpmController_FinishedRefresh;
                    }
                    var worker = this.npmWorker;
                    if (worker != null)
                    {
                        worker.CommandStarted -= this.NpmWorker_CommandStarted;
                        worker.CommandCompleted -= this.NpmWorker_CommandCompleted;
                    }
                }
                disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
    }
}

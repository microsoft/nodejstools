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
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.Windows.Design.Host;

namespace Microsoft.NodejsTools.NpmUI {
    internal class NpmPackageInstallViewModel : INotifyPropertyChanged {

        private abstract class ViewModelCommand : ICommand {
            protected NpmPackageInstallViewModel _owner;

            protected ViewModelCommand(NpmPackageInstallViewModel owner) {
                _owner = owner;
                _owner.PropertyChanged += Owner_PropertyChanged;
            }

            protected abstract void Owner_PropertyChanged(
                object sender,
                PropertyChangedEventArgs e);

            public abstract bool CanExecute(object parameter);
            public abstract void Execute(object parameter);
            public event EventHandler CanExecuteChanged;

            protected void OnCanExecuteChanged() {
                var handlers = CanExecuteChanged;
                if (null != handlers) {
                    handlers(this, EventArgs.Empty);
                }
            }
        }

        private enum Indices {
            IndexStandard = 0,
            IndexDev = 1,
            IndexOptional = 2,
            IndexGlobal = 3
        }
        
        private INpmController _npmController;

        private string _lastCatalogUpdateTimeMessage = string.Empty;
        private Color _lastCatalogUpdateTimeColor = SystemColors.WindowTextColor;
        private FontWeight _lastCatalogUpdateFontWeight = FontWeights.Normal;

        private bool _isLoadingCatalog;
        private IPackageCatalog _allPackages;
        private RefreshCatalogCommand _refreshCommand;
        private IList<PackageCatalogEntryViewModel> _filteredPackages = new List<PackageCatalogEntryViewModel>();
        private LastRefreshedMessageProvider _lastRefreshedMessage;
        private PackageCatalogEntryViewModel _selectedPackage;
        private InstallPackageCommand _installCommand;
        private bool _npmNotFound;
        private bool _isCatalogEmpty;
        private Visibility _catalogControlVisibility = Visibility.Visible;
        private string _catalogLoadingMessage = string.Empty;
        private Visibility _loadingCatalogControlVisibility = Visibility.Hidden;
        private Visibility _filteredCatalogListVisibility = Visibility.Visible;
        private string _filterLabelText = Resources.CatalogFilterLabelFilter;
        private string _rawFilterText;
        private DispatcherTimer _keypressFilterDelayTimer;
        private string _catalogFilterText;
        private string _argumentOnlyText;
        private bool _isExecuteNpmWithArgumentsMode;
        private int _selectedDependencyTypeIndex;

        private NpmOutputControlViewModel _executeViewModel;
        
        public NpmPackageInstallViewModel(NpmOutputControlViewModel executeViewModel) {
            _installCommand = new InstallPackageCommand(this);
            _refreshCommand = new RefreshCatalogCommand(this);

            _executeViewModel = executeViewModel;
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
                    LoadCatalogue();
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
                OnPropertyChanged("RefreshCatalogEnabled");
            }
        }

        public bool RefreshCatalogEnabled {
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

        private async void LoadCatalogue(bool forceRefresh) {
            IsLoadingCatalog = true;

            FilteredCatalogListVisibility = Visibility.Hidden;
            CatalogControlVisibility = Visibility.Hidden;
            LoadingCatalogControlVisibility = Visibility.Visible;
            LoadingCatalogMessage = Resources.CatalogLoadingDefault;

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
                LoadingCatalogMessage = Resources.CatalogLoadingNoNpm;
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

        public void LoadCatalogue() {
            LoadCatalogue(false);
        }

        public void RefreshCatalogue() {
            LoadCatalogue(true);
        }

        public Visibility CatalogControlVisibility {
            get { return _catalogControlVisibility; }
            set {
                _catalogControlVisibility = value;
                OnPropertyChanged();
            }
        }

        private class RefreshCatalogCommand : ViewModelCommand {
            public RefreshCatalogCommand(NpmPackageInstallViewModel owner) : base(owner) {}
            protected override void Owner_PropertyChanged(object sender, PropertyChangedEventArgs e) {
                switch (e.PropertyName) {
                    case "IsLoadingCatalog":
                        OnCanExecuteChanged();
                        break;
                }
            }

            public override bool CanExecute(object parameter) {
                return _owner.RefreshCatalogEnabled;
            }

            public override void Execute(object parameter) {
                _owner.RefreshCatalogue();
            }
        }

        public ICommand RefreshCommand {
            get {
                return _refreshCommand;
            }
        }

        #endregion

        #region Filtering

        public IList<PackageCatalogEntryViewModel> FilteredPackages {
            get { return _filteredPackages; }
            set {
                _filteredPackages = value;
                OnPropertyChanged();
                SelectedPackage = null != _filteredPackages && _filteredPackages.Count > 0 ? _filteredPackages[0] : null;
            }
        }

        public LastRefreshedMessageProvider LastRefreshedMessage {
            get { return _lastRefreshedMessage; }
            set {
                _lastRefreshedMessage = value;
                OnPropertyChanged();
            }
        }

        public string FilterLabelText {
            get { return _filterLabelText; }
            private set {
                _filterLabelText = value;
                OnPropertyChanged();
            }
        }

        private bool TreatAsArguments(string source) {
            return IsCatalogEmpty
                || (!string.IsNullOrEmpty(source)
                && (source.Any(char.IsWhiteSpace) || source.Any(ch => ch == '@')));
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

        private static string GetArgumentPortion(string source) {
            string filter = GetFilterStringPortion(source);
            return string.IsNullOrEmpty(filter)
                ? source
                : (source.Length > filter.Length ? source.Substring(filter.Length) : string.Empty);
        }

        public string RawFilterText {
            get { return _rawFilterText; }
            set {
                _rawFilterText = value;
                OnPropertyChanged();

                if (TreatAsArguments(_rawFilterText)) {
                    IsExecuteNpmWithArgumentsMode = true;
                    CatalogFilterText = GetFilterStringPortion(_rawFilterText);
                    ArgumentOnlyText = GetArgumentPortion(_rawFilterText);
                } else {
                    IsExecuteNpmWithArgumentsMode = false;
                    CatalogFilterText = value;
                }
                StartFilterTimer();
            }
        }

        private void StartFilterTimer() {
            if (null == _keypressFilterDelayTimer) {
                _keypressFilterDelayTimer = new DispatcherTimer();
                _keypressFilterDelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                _keypressFilterDelayTimer.Tick += _keypressFilterDelayTimer_Tick;
            } else {
                _keypressFilterDelayTimer.Stop();
            }
            _keypressFilterDelayTimer.Start();
        }

        private void _keypressFilterDelayTimer_Tick(object sender, EventArgs e) {
            _keypressFilterDelayTimer.Stop();
            _keypressFilterDelayTimer.Tick -= _keypressFilterDelayTimer_Tick;
            _keypressFilterDelayTimer = null;

            StartFilter();
        }

        private void StartFilter() {
            ThreadPool.QueueUserWorkItem(o => Filter(CatalogFilterText));
        }

        private void Filter(string filterString) {
            if (null == _allPackages) {
                return;
            }

            var filtered = PackageCatalogFilterFactory.Create(_allPackages).Filter(filterString);
            var controller = _npmController;
            var target = filtered.Select(package => new PackageCatalogEntryViewModel(
                package,
                null == controller ? null : controller.RootPackage,
                null == controller ? null : controller.GlobalPackages)).ToList();
            
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => SetListData(target)));
        }

        private void SetListData(IList<PackageCatalogEntryViewModel> filtered) {
            FilteredPackages = filtered;
            LastRefreshedMessage = IsCatalogEmpty
                ? LastRefreshedMessageProvider.RefreshFailed
                : new LastRefreshedMessageProvider(_allPackages.LastRefreshed);
            CatalogControlVisibility = Visibility.Visible;
        }

        public string CatalogFilterText {
            get { return _catalogFilterText; }
            private set {
                _catalogFilterText = value;
                OnPropertyChanged();
            }
        }

        public string ArgumentOnlyText {
            get { return _argumentOnlyText; }
            private set {
                _argumentOnlyText = value;
                OnPropertyChanged();
            }
        }

        public bool IsExecuteNpmWithArgumentsMode {
            get { return _isExecuteNpmWithArgumentsMode; }
            private set {
                _isExecuteNpmWithArgumentsMode = value;
                OnPropertyChanged();
                OnPropertyChanged("NonArgumentControlsVisibility");
                FilterLabelText = value
                    ? Resources.CatalogFilterLabelNpmInstall
                    : Resources.CatalogFilterLabelFilter;
            }
        }

        public Visibility NonArgumentControlsVisibility {
            get { return IsExecuteNpmWithArgumentsMode ? Visibility.Collapsed : Visibility.Visible; }
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

        private void Install() {
            if (IsExecuteNpmWithArgumentsMode) {
                _executeViewModel.QueueCommand("install", RawFilterText);
            } else if (null != _selectedPackage) {
                var type = DependencyType.Standard;
                var global = false;
                switch ((Indices) SelectedDependencyTypeIndex) {
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

                if (global) {
                    _executeViewModel.QueueInstallGlobalPackage(
                        _selectedPackage.Name,
                        "*");
                } else {
                    _executeViewModel.QueueInstallPackage(
                        _selectedPackage.Name,
                        "*",
                        type);
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

        private class InstallPackageCommand : ViewModelCommand {
            public InstallPackageCommand(NpmPackageInstallViewModel owner) : base(owner) {}

            protected override void Owner_PropertyChanged(
                object sender,
                PropertyChangedEventArgs e) {
                switch (e.PropertyName) {
                    case "IsExecuteNpmWithArgumentsMode":
                    case "SelectedPackage":
                        OnCanExecuteChanged();
                        break;
                }
            }

            public override bool CanExecute(object parameter) {
                return _owner.IsExecuteNpmWithArgumentsMode && !string.IsNullOrEmpty(_owner.RawFilterText)
                    || _owner.SelectedPackage != null;
            }

            public override void Execute(object parameter) {
                _owner.Install();
            }
        }

        public ICommand InstallCommand {
            get { return _installCommand; }
        }

        #endregion

        #region Dialog control

        #endregion
    }
}

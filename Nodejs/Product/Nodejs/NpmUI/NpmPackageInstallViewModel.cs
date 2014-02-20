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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI {
    internal class NpmPackageInstallViewModel : INotifyPropertyChanged {

        private const int IndexStandard = 0;
        private const int IndexDev = 1;
        private const int IndexOptional = 2;
        private const int IndexGlobal = 3;

        private INpmController _npmController;

        private string _lastCatalogUpdateTimeMessage = string.Empty;
        private Color _lastCatalogUpdateTimeColor = SystemColors.WindowTextColor;
        private FontWeight _lastCatalogUpdateFontWeight = FontWeights.Normal;

        private bool _isLoadingCatalog;
        private IPackageCatalog _allPackages;
        private IList<IPackage> _filteredPackages;
        private bool _npmNotFound;
        private bool _isCatalogEmpty;
        private Visibility _catalogControlVisibility = Visibility.Visible;
        private string _catalogLoadingMessage = string.Empty;
        private Visibility _loadingCatalogControlVisibility = Visibility.Hidden;
        private Visibility _filteredCatalogListVisibility = Visibility.Visible;
        private string _filterLabelText;
        private string _rawFilterText;
        private string _catalogFilterText;
        private string _argumentOnlyText;
        private bool _isExecuteNpmWithArgumentsMode;
        private int _selectedDependencyTypeIndex;

        private InstallPackageCommand _installCommand;

        public NpmPackageInstallViewModel() {
            _installCommand = new InstallPackageCommand(this);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public INpmController NpmController {
            get { return _npmController; }
            set {
                _npmController = value;
                OnPropertyChanged();
                LoadCatalogue();
            }
        }

        #region Catalog control and refresh

        public string LastCatalogUpdateTimeMessage {
            get { return _lastCatalogUpdateTimeMessage; }
            private set {
                _lastCatalogUpdateTimeMessage = value;
                OnPropertyChanged();
            }
        }

        public Color LastCatalogUpdateTimeColor {
            get { return _lastCatalogUpdateTimeColor; }
            private set {
                _lastCatalogUpdateTimeColor = value;
                OnPropertyChanged();
            }
        }

        public FontWeight LastCatalogUpdateFontWeight {
            get { return _lastCatalogUpdateFontWeight; }
            private set {
                _lastCatalogUpdateFontWeight = value;
                OnPropertyChanged();
            }
        }

        private void SetLastCatalogUpdateTimeMessage(
            string message,
            Color color,
            FontWeight weight) {
            LastCatalogUpdateTimeMessage = message;
            LastCatalogUpdateTimeColor = color;
            LastCatalogUpdateFontWeight = weight;
        }

        public bool IsLoadingCatalog {
            get { return _isLoadingCatalog; }
            private set {
                _isLoadingCatalog = value;
                OnPropertyChanged();
            }
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
            LoadingCatalogControlVisibility = Visibility.Hidden;
            LoadingCatalogMessage = Resources.CatalogLoadingNoNpm;
            
            SetLastCatalogUpdateTimeMessage(
                Resources.PackageCatalogRefreshing,
                SystemColors.WindowTextColor,
                FontWeights.Normal);

            bool showList = false;

            try {
                _allPackages = await _npmController.GetRepositoryCatalogueAsync(forceRefresh);
                IsCatalogEmpty = false;
                showList = true;
            } catch (NpmNotFoundException) {
                IsLoadingCatalog = false;
                LoadingCatalogMessage = Resources.CatalogLoadingNoNpm;
            } catch (NpmCatalogEmptyException) {
                IsLoadingCatalog = false;
                IsCatalogEmpty = true;
                showList = true;
            }

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

        #endregion

        #region Filtering

        public string FilterLabelText {
            get { return _filterLabelText; }
            private set {
                _filterLabelText = value;
                OnPropertyChanged();
            }
        }

        public string RawFilterText {
            get { return _rawFilterText; }
            set {
                _rawFilterText = value;
                OnPropertyChanged();

                //  TODO: start timer and update filter
                //  TODO: switch state if npm to be executed with arguments, and split text
            }
        }

        private void Filter() {
            //  TODO: implement this
        }

        public string CatalogFilterText {
            get { return _catalogFilterText; }
            private set {
                _catalogFilterText = value;
                OnPropertyChanged();

                //  TODO: call filter method
            }
        }

        public string ArgumentOnlyText {
            get { return _argumentOnlyText; }
            private set {
                _argumentOnlyText = value;
                OnPropertyChanged();
            }
        }

        //  Think this is redundant
        //public string FullNpmArgumentText {
            
        //}

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
            get { return IsExecuteNpmWithArgumentsMode ? Visibility.Hidden : Visibility.Visible; }
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
                return IndexGlobal == SelectedDependencyTypeIndex
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private void Install() {
            var type = DependencyType.Standard;
            var global = false;
            switch (SelectedDependencyTypeIndex) {
                case IndexDev:
                    type = DependencyType.Development;
                    break;

                case IndexOptional:
                    type = DependencyType.Optional;
                    break;

                case IndexGlobal:
                    global = true;
                    break;
            }

            //  TODO: install code goes here
        }

        private class InstallPackageCommand : ICommand {
            private NpmPackageInstallViewModel _owner;

            public InstallPackageCommand(NpmPackageInstallViewModel owner) {
                _owner = owner;
                _owner.PropertyChanged += Owner_PropertyChanged;
            }

            void Owner_PropertyChanged(object sender, PropertyChangedEventArgs e) {
                switch (e.PropertyName) {
                    case "IsExecuteNpmWithArgumentsMode":
                    case "SelectedPackage":
                        OnCanExecuteChanged();
                }
            }

            public bool CanExecute(object parameter) {
                return _owner.IsExecuteNpmWithArgumentsMode && !string.IsNullOrEmpty(_owner.RawFilterText)
                    || _owner.SelectedPackage != null;
            }

            public void Execute(object parameter) {
                _owner.Install();
            }

            public event EventHandler CanExecuteChanged;

            private void OnCanExecuteChanged() {
                var handlers = CanExecuteChanged;
                if (null != handlers) {
                    handlers();
                }
            }
        }

        public ICommand InstallCommand {
            get { return _installCommand; }
        }

        #endregion
    }
}

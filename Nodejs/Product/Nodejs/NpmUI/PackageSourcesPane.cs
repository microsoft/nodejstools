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
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI {
    internal partial class PackageSourcesPane : UserControl {
        private PackageView _selectedPackageView;

        public PackageSourcesPane() {
            InitializeComponent();
        }

        private void UpdateUIState() {
            var view = SelectedPackageView;
            _btnInstall.Text = view == PackageView.Local ? "Install Locally" : "Install Globally";
            _labelInstallAs.Enabled = view == PackageView.Local;
            _comboDepType.Enabled = view == PackageView.Local;
            _comboDepType.SelectedIndex = 0;

            if (_tabCtrlPackageSources.SelectedIndex == 0) {
                _btnInstall.Enabled = !string.IsNullOrEmpty(_paneInstallParms.PackageName) &&
                                      !string.IsNullOrEmpty(_paneInstallParms.PackageName.Trim());
            } else {
                _btnInstall.Enabled = _paneSearch.SelectedPackage != null;
            }
        }

        public INpmController NpmController {
            set { _paneSearch.NpmController = value; }
        }

        public PackageView SelectedPackageView {
            set {
                _selectedPackageView = value;
                UpdateUIState();
            }
            private get { return _selectedPackageView; }
        }

        public event EventHandler<PackageInstallEventArgs> InstallPackageRequested;

        private void OnInstallPackageRequested(
            string name,
            string version,
            DependencyType depType) {
            var handlers = InstallPackageRequested;
            if (null != handlers) {
                handlers(
                    this,
                    new PackageInstallEventArgs(
                        name.Trim(),
                        string.IsNullOrEmpty(version) ? version : version.Trim(),
                        depType));
            }
        }

        private void _tabCtrlPackageSources_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateUIState();
        }

        private void _paneInstallParms_PackageInstallParmsChanged(object sender, EventArgs e) {
            UpdateUIState();
        }

        private void _paneSearch_SelectedPackageChanged(object sender, EventArgs e) {
            UpdateUIState();
        }

        private void _btnInstall_Click(object sender, EventArgs e) {
            DependencyType dependencyType;
            switch (_comboDepType.SelectedIndex) {
                case 0:
                    dependencyType = DependencyType.Standard;
                    break;

                case 1:
                    dependencyType = DependencyType.Development;
                    break;

                default:
                    dependencyType = DependencyType.Optional;
                    break;
            }

            if (_tabCtrlPackageSources.SelectedIndex == 0) {
                OnInstallPackageRequested(
                    _paneInstallParms.PackageName,
                    _paneInstallParms.Version,
                    dependencyType);
            } else {
                var package = _paneSearch.SelectedPackage;
                if (null != package) {
                    OnInstallPackageRequested(
                        package.Name,
                        null,
                        //  Just want to install the latest version, at present - TODO: especially whilst version retrieval from npm catalogue is questionable
                        dependencyType);
                }
            }
        }
    }
}
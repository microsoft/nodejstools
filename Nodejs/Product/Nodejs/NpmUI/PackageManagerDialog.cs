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
using System.Threading;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI{
    public partial class PackageManagerDialog : Form{
        private readonly INpmController _npmController;
        private readonly object _lock = new object();

        public PackageManagerDialog(INpmController controller){
            InitializeComponent();

            _npmController = controller;
        }

        protected override void OnLoad(EventArgs e){
            base.OnLoad(e);

            _npmController.FinishedRefresh += _npmController_FinishedRefresh;
            LoadPackageInfo();
            UpdateUIState();
            _panePackageSources.NpmController = _npmController;
        }

        private void _btnClose_Click(object sender, EventArgs e){
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// This is a bit of a do everything method that updates the state of relevant controls based
        /// on which tabs, etc., are selected.
        /// </summary>
        private void UpdateUIState(){
            _labelWarning.Visible =
                _paneInstalledPackages.SelectedPackageView == PackageView.Global;
            _labelWarningText.Visible =
                _paneInstalledPackages.SelectedPackageView == PackageView.Global;

            _panePackageSources.SelectedPackageView =
                _paneInstalledPackages.SelectedPackageView;
        }

        private void LoadPackageInfo(){
            lock (_lock){
                _paneInstalledPackages.LocalPackages =
                    _npmController.RootPackage.Modules;
                var globals = _npmController.GlobalPackages;
                if (null != globals){
                    _paneInstalledPackages.GlobalPackages = globals.Modules;
                }
            }
        }

        private void _npmController_FinishedRefresh(object sender, EventArgs e){
            if (InvokeRequired){
                BeginInvoke(
                    new EventHandler(_npmController_FinishedRefresh),
                    sender,
                    e);
            } else{
                LoadPackageInfo();
            }
        }

        private void _paneInstalledPackages_SelectedPackageViewChanged(
            object sender,
            EventArgs e){
            UpdateUIState();
        }

        private void DoWithPopup(
            string popupMessage,
            Action action,
            INpmCommander commander){
            using (var popup = new BusyPopup()){
                popup.Message = popupMessage;
                popup.ShowPopup(
                    this,
                    commander,
                    action);
            }
        }

        private void _paneInstalledPackages_UninstallGloballPackageRequested(
            object sender,
            PackageEventArgs e){
            using (var commander = _npmController.CreateNpmCommander()){
                DoWithPopup(
                    string.Format(
                        "Uninstalling global package '{0}'...",
                        e.Package.Name),
                    async () => await commander.UninstallGlobalPackageAsync(e.Package.Name),
                    commander);
            }
        }

        private void _paneInstalledPackages_UninstallLocalPackageRequested(
            object sender,
            PackageEventArgs e){
            using (var commander = _npmController.CreateNpmCommander()){
                DoWithPopup(
                    string.Format(
                        "Uninstalling local package '{0}'...",
                        e.Package.Name),
                    async () => await commander.UninstallPackageAsync(e.Package.Name),
                    commander);
            }
        }

        private void _panePackageSources_InstallPackageRequested(
            object sender,
            PackageInstallEventArgs e){
            using (var commander = _npmController.CreateNpmCommander()){
                if (_paneInstalledPackages.SelectedPackageView == PackageView.Global){
                    DoWithPopup(
                        string.Format("Installing global package '{0}'...", e.Name),
                        async () => await commander.InstallGlobalPackageByVersionAsync(
                                e.Name,
                                e.Version),
                        commander);
                } else{
                    DoWithPopup(
                        string.Format("Installing local package '{0}'...", e.Name),
                        async () => await commander.InstallPackageByVersionAsync(
                                    e.Name,
                                    e.Version,
                                    e.DependencyType,
                                    true),
                        commander);
                }
            }
        }
    }
}
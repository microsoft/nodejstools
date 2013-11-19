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
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI{
    internal partial class InstalledPackagesPane : UserControl{
        public InstalledPackagesPane(){
            InitializeComponent();
        }

        public event EventHandler SelectedPackageViewChanged;

        private void OnSelectedPackageViewChanged(){
            var handlers = SelectedPackageViewChanged;
            if (null != handlers){
                handlers(this, EventArgs.Empty);
            }
        }

        public PackageView SelectedPackageView{
            get{
                return _tabCtrlInstalledPackages.SelectedIndex == 0
                           ? PackageView.Local
                           : PackageView.Global;
            }
        }

        private void _tabCtrlInstalledPackages_SelectedIndexChanged(
            object sender,
            EventArgs e){
            OnSelectedPackageViewChanged();
        }

        public IEnumerable<IPackage> LocalPackages{
            set { _listLocalPackages.Packages = value; }
        }

        public IEnumerable<IPackage> GlobalPackages{
            set { _listGlobalPackages.Packages = value; }
        }

        private void FirePackageEvent(
            EventHandler<PackageEventArgs> handlers,
            PackageEventArgs e){
            if (null != handlers){
                handlers(this, e);
            }
        }

        public event EventHandler<PackageEventArgs>
            UninstallLocalPackageRequested;

        private void OnUninstallLocalPackageRequested(PackageEventArgs e){
            FirePackageEvent(UninstallLocalPackageRequested, e);
        }

        private void _listLocalPackages_UninstallPackageRequested(
            object sender,
            PackageEventArgs e){
            OnUninstallLocalPackageRequested(e);
        }

        public event EventHandler<PackageEventArgs>
            UninstallGloballPackageRequested;

        private void OnUninstallGlobalPackageRequested(PackageEventArgs e){
            FirePackageEvent(UninstallGloballPackageRequested, e);
        }

        private void _listGlobalPackages_UninstallPackageRequested(
            object sender,
            PackageEventArgs e){
            OnUninstallGlobalPackageRequested(e);
        }
    }
}
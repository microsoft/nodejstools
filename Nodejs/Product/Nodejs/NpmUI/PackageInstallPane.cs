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

namespace Microsoft.NodejsTools.NpmUI{
    public partial class PackageInstallPane : UserControl{
        public PackageInstallPane(){
            InitializeComponent();
        }

        public event EventHandler PackageInstallParmsChanged;

        private void OnPackageInstallParmsChanged(){
            var handlers = PackageInstallParmsChanged;
            if (null != handlers){
                handlers(this, EventArgs.Empty);
            }
        }

        private void _txtPackageName_KeyUp(object sender, KeyEventArgs e){
            OnPackageInstallParmsChanged();
        }

        private void _txtVersionTag_KeyUp(object sender, KeyEventArgs e){
            OnPackageInstallParmsChanged();
        }

        public string PackageName{
            get { return _txtPackageName.Text; }
        }

        public string Version{
            get { return _txtVersionTag.Text; }
        }
    }
}
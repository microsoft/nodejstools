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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    partial class NodejsGeneralPropertyPageControl : UserControl {
        private readonly NodejsGeneralPropertyPage _propPage;
        private const string _exeFilter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

        public NodejsGeneralPropertyPageControl() {
            InitializeComponent();

            SetCueBanner();

            AddToolTips();
        }

        public NodejsGeneralPropertyPageControl(NodejsGeneralPropertyPage page) : this() {
            _propPage = page;
        }

        private void AddToolTips() {
            _tooltip.SetToolTip(_nodeExePath, Resources.NodeExePathToolTip);
            _tooltip.SetToolTip(_nodeExeArguments, Resources.NodeExeArgumentsToolTip);
            _tooltip.SetToolTip(_scriptArguments, Resources.ScriptArgumentsToolTip);
            _tooltip.SetToolTip(_nodejsPort, Resources.NodejsPortToolTip);
            _tooltip.SetToolTip(_startBrowser, Resources.StartBrowserToolTip);
            _tooltip.SetToolTip(_workingDir, Resources.WorkingDirToolTip);
            _tooltip.SetToolTip(_launchUrl, Resources.LaunchUrlToolTip);
        }

        public string NodeExePath {
            get {
                return _nodeExePath.Text;
            }
            set {
                _nodeExePath.Text = value;
            }
        }

        public string NodeExeArguments {
            get {
                return _nodeExeArguments.Text;
            }
            set {
                _nodeExeArguments.Text = value;
            }
        }

        public string ScriptArguments {
            get {
                return _scriptArguments.Text;
            }
            set {
                _scriptArguments.Text = value;
            }
        }

        public string NodejsPort {
            get {
                return _nodejsPort.Text;
            }
            set {
                _nodejsPort.Text = value;
            }
        }

        public bool StartWebBrowser {
            get {
                return _startBrowser.Checked;
            }
            set {
                _startBrowser.Checked = value;
            }
        }


        public string WorkingDirectory {
            get {
                return _workingDir.Text;
            }
            set {
                _workingDir.Text = value;
            }
        }

        public string LaunchUrl {
            get {
                return _launchUrl.Text;
            }
            set {
                _launchUrl.Text = value;
            }
        }

        private void Changed(object sender, EventArgs e) {
            _propPage.IsDirty = true;
        }

        private void SetCueBanner() {
            string cueBanner = Nodejs.NodeExePath;
            if (String.IsNullOrEmpty(cueBanner)) {
                cueBanner = Resources.NodejsNotInstalledShort;
            }

            NativeMethods.SendMessageW(
                _nodeExePath.Handle,
                NativeMethods.EM_SETCUEBANNER,
                new IntPtr(1),  // fDrawFocused == true
                cueBanner
            );
        }

        private void NodeExePathValidated(object sender, EventArgs e) {
            if (String.IsNullOrEmpty(_nodeExePath.Text) || File.Exists(_nodeExePath.Text)) {
                _nodeExeErrorProvider.SetError(_nodeExePath, String.Empty);
            } else {
                _nodeExeErrorProvider.SetError(_nodeExePath, Resources.NodeExePathNotFound);
            }
        }
        
        private void BrowsePathClick(object sender, EventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = _exeFilter;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _nodeExePath.Text = dialog.FileName;
                _nodeExePath.ForeColor = SystemColors.ControlText;
            }
        }

        private void BrowseDirectoryClick(object sender, EventArgs e) {
            string dir = _workingDir.Text;
            if (string.IsNullOrEmpty(dir)) {
                dir = _propPage.Project.ProjectHome;
            }
            var path = NodejsPackage.Instance.BrowseForDirectory(Handle, dir);
            if (!string.IsNullOrEmpty(path)) {
                _workingDir.Text = path;
            }
        }

        private void NodejsPortValidated(object sender, EventArgs e) {
            if (_nodejsPort.Text.Any(ch => !Char.IsDigit(ch))) {
                _nodeExeErrorProvider.SetError(_nodejsPort, Resources.InvalidPortNumber);
            } else {
                _nodeExeErrorProvider.SetError(_nodejsPort, String.Empty);
            }

        }
    }
}

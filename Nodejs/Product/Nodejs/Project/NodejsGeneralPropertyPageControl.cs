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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            _nodeExeErrorProvider.SetIconAlignment(_nodeExePath, ErrorIconAlignment.MiddleLeft);
            _nodeExeErrorProvider.SetIconAlignment(_workingDir, ErrorIconAlignment.MiddleLeft);
        }

        public NodejsGeneralPropertyPageControl(NodejsGeneralPropertyPage page) : this() {
            _propPage = page;
        }

        public bool HasErrors {
            get {
                return !String.IsNullOrEmpty(_nodeExeErrorProvider.GetError(_nodejsPort)) ||
                       !String.IsNullOrEmpty(_nodeExeErrorProvider.GetError(_nodeExePath));
            }
        }

        private void AddToolTips() {
            _tooltip.SetToolTip(_nodeExePath, SR.GetString(SR.NodeExePathToolTip));
            _tooltip.SetToolTip(_nodeExeArguments, SR.GetString(SR.NodeExeArgumentsToolTip));
            _tooltip.SetToolTip(_scriptArguments, SR.GetString(SR.ScriptArgumentsToolTip));
            _tooltip.SetToolTip(_nodejsPort, SR.GetString(SR.NodejsPortToolTip));
            _tooltip.SetToolTip(_startBrowser, SR.GetString(SR.StartBrowserToolTip));
            _tooltip.SetToolTip(_workingDir, SR.GetString(SR.WorkingDirToolTip));
            _tooltip.SetToolTip(_launchUrl, SR.GetString(SR.LaunchUrlToolTip));
            _tooltip.SetToolTip(_debuggerPort, SR.GetString(SR.DebuggerPort));
            _tooltip.SetToolTip(_envVars, SR.GetString(SR.EnvironmentVariables));
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

        private static Regex lfToCrLfRegex = new Regex(@"(?<!\r)\n");

        public string Environment {
            get {
                return _envVars.Text;
            }
            set {
                // TextBox requires \r\n for line separators, but XML can have either \n or \r\n, and we should treat those equally.
                // (It will always have \r\n when we write it out, but users can edit it by other means.)
                _envVars.Text = lfToCrLfRegex.Replace(value ?? "", "\r\n");
            }
        }

        public string DebuggerPort {
            get {
                return _debuggerPort.Text;
            }
            set {
                _debuggerPort.Text = value;
            }
        }

        private void Changed(object sender, EventArgs e) {
            _propPage.IsDirty = true;
        }

        private void SetCueBanner() {
            string cueBanner = Nodejs.NodeExePath;
            if (String.IsNullOrEmpty(cueBanner)) {
                cueBanner = SR.GetString(SR.NodejsNotInstalledShort);
            }

            NativeMethods.SendMessageW(
                _nodeExePath.Handle,
                NativeMethods.EM_SETCUEBANNER,
                new IntPtr(1),  // fDrawFocused == true
                cueBanner
            );
        }

        private void NodeExePathChanged(object sender, EventArgs e) {
            if (String.IsNullOrEmpty(_nodeExePath.Text) || File.Exists(_nodeExePath.Text)) {
                _nodeExeErrorProvider.SetError(_nodeExePath, String.Empty);
            } else {
                _nodeExeErrorProvider.SetError(_nodeExePath, SR.GetString(SR.NodeExePathNotFound));
            }
            Changed(sender, e);
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

        private void PortChanged(object sender, EventArgs e) {
            var textSender = (TextBox)sender;

            if (textSender.Text.Any(ch => !Char.IsDigit(ch))) {
                _nodeExeErrorProvider.SetError(textSender, SR.GetString(SR.InvalidPortNumber));
            } else {
                _nodeExeErrorProvider.SetError(textSender, String.Empty);
            }
            Changed(sender, e);
        }

        private void WorkingDirTextChanged(object sender, EventArgs e) {
            if (_workingDir.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1 || !Directory.Exists(_workingDir.Text)) {
                _nodeExeErrorProvider.SetError(_workingDir, SR.GetString(SR.WorkingDirInvalidOrMissing));
            } else {
                _nodeExeErrorProvider.SetError(_workingDir, String.Empty);
            }
            Changed(sender, e);
        }
    }
}

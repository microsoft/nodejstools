//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
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

        private void AddToolTips() {
            _tooltip.SetToolTip(_nodeExePath, Resources.NodeExePathToolTip);
            _tooltip.SetToolTip(_nodeExeArguments, Resources.NodeExeArgumentsToolTip);
            _tooltip.SetToolTip(_scriptFile, Resources.ScriptFileTooltip);
            _tooltip.SetToolTip(_scriptArguments, Resources.ScriptArgumentsToolTip);
            _tooltip.SetToolTip(_nodejsPort, Resources.NodejsPortToolTip);
            _tooltip.SetToolTip(_startBrowser, Resources.StartBrowserToolTip);
            _tooltip.SetToolTip(_workingDir, Resources.WorkingDirToolTip);
            _tooltip.SetToolTip(_launchUrl, Resources.LaunchUrlToolTip);
            _tooltip.SetToolTip(_debuggerPort, Resources.DebuggerPort);
            _tooltip.SetToolTip(_envVars, Resources.EnvironmentVariables);
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

        public string ScriptFile  {
            get {
                return this._scriptFile.Text;
            }
            set {
                this._scriptFile.Text = value;
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
                _envVars.Text = lfToCrLfRegex.Replace(value ?? String.Empty, "\r\n");
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
                cueBanner = Resources.NodejsNotInstalledShort;
            }

            NativeMethods.SendMessageW(
                _nodeExePath.Handle,
                NativeMethods.EM_SETCUEBANNER,
                new IntPtr(1),  // fDrawFocused == true
                cueBanner
            );
        }

        private void NodeExePathChanged(object sender, EventArgs e) {
            if (String.IsNullOrEmpty(_nodeExePath.Text) || _nodeExePath.Text.Contains("$(") ||
                File.Exists(Nodejs.GetAbsoluteNodeExePath(_propPage.Project.ProjectHome, _nodeExePath.Text))) {
                _nodeExeErrorProvider.SetError(_nodeExePath, String.Empty);
            } else {
                _nodeExeErrorProvider.SetError(_nodeExePath, Resources.NodeExePathNotFound);
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
            if (!textSender.Text.Contains("$(") && 
                textSender.Text.Any(ch => !Char.IsDigit(ch))) {
                _nodeExeErrorProvider.SetError(textSender, Resources.InvalidPortNumber);
            } else {
                _nodeExeErrorProvider.SetError(textSender, String.Empty);
            }
            Changed(sender, e);
        }

        private void WorkingDirTextChanged(object sender, EventArgs e) {
            if (!_workingDir.Text.Contains("$(") && !Directory.Exists(_workingDir.Text)) {
                _nodeExeErrorProvider.SetError(_workingDir, Resources.WorkingDirInvalidOrMissing);
            } else {
                _nodeExeErrorProvider.SetError(_workingDir, String.Empty);
            }
            Changed(sender, e);
        }
    }
}

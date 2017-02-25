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

namespace Microsoft.NodejsTools.Project
{
    internal partial class NodejsGeneralPropertyPageControl : UserControl
    {
        private readonly NodejsGeneralPropertyPage _propPage;
        private const string _exeFilter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

        public NodejsGeneralPropertyPageControl()
        {
            InitializeComponent();

            SetCueBanner();

            AddToolTips();
            this._nodeExeErrorProvider.SetIconAlignment(this._nodeExePath, ErrorIconAlignment.MiddleLeft);
            this._nodeExeErrorProvider.SetIconAlignment(this._workingDir, ErrorIconAlignment.MiddleLeft);
        }

        public NodejsGeneralPropertyPageControl(NodejsGeneralPropertyPage page) : this()
        {
            this._propPage = page;
        }

        private void AddToolTips()
        {
            this._tooltip.SetToolTip(this._nodeExePath, Resources.NodeExePathToolTip);
            this._tooltip.SetToolTip(this._nodeExeArguments, Resources.NodeExeArgumentsToolTip);
            this._tooltip.SetToolTip(this._scriptFile, Resources.ScriptFileTooltip);
            this._tooltip.SetToolTip(this._scriptArguments, Resources.ScriptArgumentsToolTip);
            this._tooltip.SetToolTip(this._nodejsPort, Resources.NodejsPortToolTip);
            this._tooltip.SetToolTip(this._startBrowser, Resources.StartBrowserToolTip);
            this._tooltip.SetToolTip(this._workingDir, Resources.WorkingDirToolTip);
            this._tooltip.SetToolTip(this._launchUrl, Resources.LaunchUrlToolTip);
            this._tooltip.SetToolTip(this._debuggerPort, Resources.DebuggerPort);
            this._tooltip.SetToolTip(this._envVars, Resources.EnvironmentVariables);
        }

        public string NodeExePath
        {
            get
            {
                return this._nodeExePath.Text;
            }
            set
            {
                this._nodeExePath.Text = value;
            }
        }

        public string NodeExeArguments
        {
            get
            {
                return this._nodeExeArguments.Text;
            }
            set
            {
                this._nodeExeArguments.Text = value;
            }
        }

        public string ScriptFile
        {
            get
            {
                return this._scriptFile.Text;
            }
            set
            {
                this._scriptFile.Text = value;
            }
        }

        public string ScriptArguments
        {
            get
            {
                return this._scriptArguments.Text;
            }
            set
            {
                this._scriptArguments.Text = value;
            }
        }

        public string NodejsPort
        {
            get
            {
                return this._nodejsPort.Text;
            }
            set
            {
                this._nodejsPort.Text = value;
            }
        }

        public bool StartWebBrowser
        {
            get
            {
                return this._startBrowser.Checked;
            }
            set
            {
                this._startBrowser.Checked = value;
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return this._workingDir.Text;
            }
            set
            {
                this._workingDir.Text = value;
            }
        }

        public string LaunchUrl
        {
            get
            {
                return this._launchUrl.Text;
            }
            set
            {
                this._launchUrl.Text = value;
            }
        }

        private static Regex lfToCrLfRegex = new Regex(@"(?<!\r)\n");

        public string Environment
        {
            get
            {
                return this._envVars.Text;
            }
            set
            {
                // TextBox requires \r\n for line separators, but XML can have either \n or \r\n, and we should treat those equally.
                // (It will always have \r\n when we write it out, but users can edit it by other means.)
                this._envVars.Text = lfToCrLfRegex.Replace(value ?? String.Empty, "\r\n");
            }
        }

        public string DebuggerPort
        {
            get
            {
                return this._debuggerPort.Text;
            }
            set
            {
                this._debuggerPort.Text = value;
            }
        }

        private void Changed(object sender, EventArgs e)
        {
            this._propPage.IsDirty = true;
        }

        private void SetCueBanner()
        {
            var cueBanner = Nodejs.NodeExePath;
            if (String.IsNullOrEmpty(cueBanner))
            {
                cueBanner = Resources.NodejsNotInstalledShort;
            }

            NativeMethods.SendMessageW(
                this._nodeExePath.Handle,
                NativeMethods.EM_SETCUEBANNER,
                new IntPtr(1),  // fDrawFocused == true
                cueBanner
            );
        }

        private void NodeExePathChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this._nodeExePath.Text) || this._nodeExePath.Text.Contains("$(") ||
                File.Exists(Nodejs.GetAbsoluteNodeExePath(this._propPage.Project.ProjectHome, this._nodeExePath.Text)))
            {
                this._nodeExeErrorProvider.SetError(this._nodeExePath, String.Empty);
            }
            else
            {
                this._nodeExeErrorProvider.SetError(this._nodeExePath, Resources.NodeExePathNotFound);
            }
            Changed(sender, e);
        }

        private void BrowsePathClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = _exeFilter;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this._nodeExePath.Text = dialog.FileName;
                this._nodeExePath.ForeColor = SystemColors.ControlText;
            }
        }

        private void BrowseDirectoryClick(object sender, EventArgs e)
        {
            var dir = this._workingDir.Text;
            if (string.IsNullOrEmpty(dir))
            {
                dir = this._propPage.Project.ProjectHome;
            }
            var path = NodejsPackage.Instance.BrowseForDirectory(this.Handle, dir);
            if (!string.IsNullOrEmpty(path))
            {
                this._workingDir.Text = path;
            }
        }

        private void PortChanged(object sender, EventArgs e)
        {
            var textSender = (TextBox)sender;
            if (!textSender.Text.Contains("$(") &&
                textSender.Text.Any(ch => !Char.IsDigit(ch)))
            {
                this._nodeExeErrorProvider.SetError(textSender, Resources.InvalidPortNumber);
            }
            else
            {
                this._nodeExeErrorProvider.SetError(textSender, String.Empty);
            }
            Changed(sender, e);
        }

        private void WorkingDirTextChanged(object sender, EventArgs e)
        {
            if (!this._workingDir.Text.Contains("$(") && !Directory.Exists(this._workingDir.Text))
            {
                this._nodeExeErrorProvider.SetError(this._workingDir, Resources.WorkingDirInvalidOrMissing);
            }
            else
            {
                this._nodeExeErrorProvider.SetError(this._workingDir, String.Empty);
            }
            Changed(sender, e);
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualStudio.Editors.PropertyPages;

namespace Microsoft.NodejsTools.Project
{
    internal sealed partial class NodejsGeneralPropertyPageControl : PropPageUserControlBase
    {
        private readonly NodejsGeneralPropertyPage _propPage;
        private const string _exeFilter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

        public NodejsGeneralPropertyPageControl()
        {
            InitializeComponent();

            LocalizeLabels();
            AddToolTips();

            this._nodeExeErrorProvider.SetIconAlignment(this._nodeExePath, ErrorIconAlignment.MiddleLeft);
            this._nodeExeErrorProvider.SetIconAlignment(this._workingDir, ErrorIconAlignment.MiddleLeft);
        }

        public NodejsGeneralPropertyPageControl(NodejsGeneralPropertyPage page) : this()
        {
            this._propPage = page;
        }

        private void LocalizeLabels()
        {
            // There's a bug in winforms, where if you use the default localization infra structure
            // the control is correctly sized in the property page.

            this._nodeExePathLabel.Text = Resources.PropertiesNodeExePath;
            this._nodeArgumentsLabel.Text = Resources.PropertiesNodeExeOptions;
            this._scriptLabel.Text = Resources.PropertiesScript;
            this._scriptArgsLabel.Text = Resources.PropertiesScriptArgs;
            this._workingDirLabel.Text = Resources.PropertiesWorkingDir;
            this._launchUrlLabel.Text = Resources.PropertiesLaunchUrl;
            this._nodePortLabel.Text = Resources.PropertiesNodePort;
            this._debuggerPortLabel.Text = Resources.PropertiesDebuggerPort;
            this._envVarsLabel.Text = Resources.PropertiesEnvVars;
            this._startBrowserCheckBox.Text = Resources.PropertiesStartBrowser;
            this._startActionHeaderLabel.Text = Resources.StartActionHeader;
            this._nodeHeaderLabel.Text = Resources.NodeHeader;
            this._storeInProjectFileCheckBox.Text = Resources.StoreNodeSettingsInProjectFile;

            this._browsePath.AccessibleName = Resources.PropertiesBrowsePathAccessibleName;
            this._browseDirectory.AccessibleName = Resources.PropertiesBrowseDirectoryAccessibleName;
        }

        private void AddToolTips()
        {
            this._tooltip.SetToolTip(this._nodeExePath, Resources.NodeExePathToolTip);
            this._tooltip.SetToolTip(this._nodeExeArguments, Resources.NodeExeArgumentsToolTip);
            this._tooltip.SetToolTip(this._scriptFile, Resources.ScriptFileTooltip);
            this._tooltip.SetToolTip(this._scriptArguments, Resources.ScriptArgumentsToolTip);
            this._tooltip.SetToolTip(this._nodejsPort, Resources.NodejsPortToolTip);
            this._tooltip.SetToolTip(this._startBrowserCheckBox, Resources.StartBrowserToolTip);
            this._tooltip.SetToolTip(this._workingDir, Resources.WorkingDirToolTip);
            this._tooltip.SetToolTip(this._launchUrl, Resources.LaunchUrlToolTip);
            this._tooltip.SetToolTip(this._debuggerPort, Resources.DebuggerPort);
            this._tooltip.SetToolTip(this._envVars, Resources.EnvironmentVariables);
        }

        protected override bool DisableOnBuild => false;

        protected override bool DisableOnDebug => false;

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
                return this._startBrowserCheckBox.Checked;
            }
            set
            {
                this._startBrowserCheckBox.Checked = value;
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

        private static readonly Regex lfToCrLfRegex = new Regex(@"(?<!\r)\n");

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
                this._envVars.Text = lfToCrLfRegex.Replace(value ?? string.Empty, "\r\n");
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

        public bool StoreNodeSettingsInProject
        {
            get
            {
                return this._storeInProjectFileCheckBox.Checked;
            }
            set
            {
                this._storeInProjectFileCheckBox.Checked = value;
            }
        }

        private void Changed(object sender, EventArgs e)
        {
            this.IsDirty = true;
        }

        private void NodeExePathChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this._nodeExePath.Text) || this._nodeExePath.Text.Contains("$(") ||
                File.Exists(Nodejs.GetAbsoluteNodeExePath(this._propPage.Project.ProjectHome, this._nodeExePath.Text)))
            {
                this._nodeExeErrorProvider.SetError(this._nodeExePath, string.Empty);
            }
            else
            {
                this._nodeExeErrorProvider.SetError(this._nodeExePath, Resources.NodeExePathNotFound);
            }
            Changed(sender, e);
        }

        private void BrowsePathClick(object sender, EventArgs e)
        {
            var nodeExePath = this._nodeExePath.Text;
            if (this.GetFileViaBrowse(nodeExePath, ref nodeExePath, _exeFilter) && !string.IsNullOrEmpty(nodeExePath))
            {
                this._nodeExePath.Text = nodeExePath;
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

            if (this.GetDirectoryViaBrowseRelative(dir, this._propPage.Project.ProjectHome, Resources.BrowseWorkingDirDialogTitle, ref dir))
            {
                this._workingDir.Text = string.IsNullOrEmpty(dir) ? "." : dir;
            }
        }

        private void PortChanged(object sender, EventArgs e)
        {
            var textSender = (TextBox)sender;
            if (!textSender.Text.Contains("$(") &&
                textSender.Text.Any(ch => !char.IsDigit(ch)))
            {
                this._nodeExeErrorProvider.SetError(textSender, Resources.InvalidPortNumber);
            }
            else
            {
                this._nodeExeErrorProvider.SetError(textSender, string.Empty);
            }
            Changed(sender, e);
        }

        private void WorkingDirChanged(object sender, EventArgs e)
        {
            var errorMessage = ValidateWorkingDir(this._workingDir.Text) ? "" : Resources.WorkingDirInvalidOrMissing;
            this._nodeExeErrorProvider.SetError(this._workingDir, errorMessage);

            Changed(sender, e);

            bool ValidateWorkingDir(string workingDir)
            {
                if (workingDir.Contains("$("))
                {
                    return true;
                }

                var fullPath = Path.IsPathRooted(workingDir) ? workingDir : Path.Combine(this._propPage.Project.ProjectHome, workingDir);
                return Directory.Exists(fullPath);
            }
        }
    }
}

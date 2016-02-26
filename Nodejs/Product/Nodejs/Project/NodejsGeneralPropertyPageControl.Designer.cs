namespace Microsoft.NodejsTools.Project {
    partial class NodejsGeneralPropertyPageControl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this._nodeExePathLabel = new System.Windows.Forms.Label();
            this._nodeArguments = new System.Windows.Forms.Label();
            this._startBrowser = new System.Windows.Forms.CheckBox();
            this._nodeExePath = new System.Windows.Forms.TextBox();
            this._nodeExeArguments = new System.Windows.Forms.TextBox();
            this._nodejsPort = new System.Windows.Forms.TextBox();
            this._nodePortLabel = new System.Windows.Forms.Label();
            this._scriptArgsLabel = new System.Windows.Forms.Label();
            this._scriptArguments = new System.Windows.Forms.TextBox();
            this._workingDirLabel = new System.Windows.Forms.Label();
            this._workingDir = new System.Windows.Forms.TextBox();
            this._launchUrl = new System.Windows.Forms.TextBox();
            this._launchUrlLabel = new System.Windows.Forms.Label();
            this._tooltip = new System.Windows.Forms.ToolTip(this.components);
            this._envVars = new System.Windows.Forms.TextBox();
            this._browsePath = new System.Windows.Forms.Button();
            this._browseDirectory = new System.Windows.Forms.Button();
            this._nodeExeErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this._debuggerPortLabel = new System.Windows.Forms.Label();
            this._debuggerPort = new System.Windows.Forms.TextBox();
            this._envVarsLabel = new System.Windows.Forms.Label();
            this._scriptLabel = new System.Windows.Forms.Label();
            this._scriptFile = new System.Windows.Forms.TextBox();
            this._overrideDefaultBrowser = new System.Windows.Forms.CheckBox();
            this._browserArgumentsLabel = new System.Windows.Forms.Label();
            this._browserExecutableLabel = new System.Windows.Forms.Label();
            this._browseBrowser = new System.Windows.Forms.Button();
            this._browserExecutable = new System.Windows.Forms.TextBox();
            this._browserArguments = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this._nodeExeErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // _nodeExePathLabel
            // 
            this._nodeExePathLabel.AutoSize = true;
            this._nodeExePathLabel.Location = new System.Drawing.Point(19, 27);
            this._nodeExePathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._nodeExePathLabel.Name = "_nodeExePathLabel";
            this._nodeExePathLabel.Size = new System.Drawing.Size(80, 13);
            this._nodeExePathLabel.TabIndex = 0;
            this._nodeExePathLabel.Text = "Node.exe &path:";
            // 
            // _nodeArguments
            // 
            this._nodeArguments.AutoSize = true;
            this._nodeArguments.Location = new System.Drawing.Point(19, 59);
            this._nodeArguments.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._nodeArguments.Name = "_nodeArguments";
            this._nodeArguments.Size = new System.Drawing.Size(93, 13);
            this._nodeArguments.TabIndex = 3;
            this._nodeArguments.Text = "N&ode.exe options:";
            // 
            // _startBrowser
            // 
            this._startBrowser.AutoSize = true;
            this._startBrowser.Location = new System.Drawing.Point(23, 350);
            this._startBrowser.Margin = new System.Windows.Forms.Padding(4);
            this._startBrowser.Name = "_startBrowser";
            this._startBrowser.Size = new System.Drawing.Size(161, 17);
            this._startBrowser.TabIndex = 20;
            this._startBrowser.Text = "St&art web browser on launch";
            this._startBrowser.UseVisualStyleBackColor = true;
            this._startBrowser.CheckedChanged += new System.EventHandler(this.Changed);
            // 
            // _nodeExePath
            // 
            this._nodeExePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._nodeExePath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._nodeExePath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this._nodeExePath.Location = new System.Drawing.Point(180, 23);
            this._nodeExePath.Margin = new System.Windows.Forms.Padding(4);
            this._nodeExePath.Name = "_nodeExePath";
            this._nodeExePath.Size = new System.Drawing.Size(409, 20);
            this._nodeExePath.TabIndex = 1;
            this._nodeExePath.TextChanged += new System.EventHandler(this.NodeExePathChanged);
            // 
            // _nodeExeArguments
            // 
            this._nodeExeArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._nodeExeArguments.Location = new System.Drawing.Point(180, 55);
            this._nodeExeArguments.Margin = new System.Windows.Forms.Padding(4);
            this._nodeExeArguments.Name = "_nodeExeArguments";
            this._nodeExeArguments.Size = new System.Drawing.Size(452, 20);
            this._nodeExeArguments.TabIndex = 4;
            this._nodeExeArguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _nodejsPort
            // 
            this._nodejsPort.Location = new System.Drawing.Point(180, 217);
            this._nodejsPort.Margin = new System.Windows.Forms.Padding(4);
            this._nodejsPort.Name = "_nodejsPort";
            this._nodejsPort.Size = new System.Drawing.Size(139, 20);
            this._nodejsPort.TabIndex = 15;
            this._nodejsPort.TextChanged += new System.EventHandler(this.PortChanged);
            // 
            // _nodePortLabel
            // 
            this._nodePortLabel.AutoSize = true;
            this._nodePortLabel.Location = new System.Drawing.Point(19, 220);
            this._nodePortLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._nodePortLabel.Name = "_nodePortLabel";
            this._nodePortLabel.Size = new System.Drawing.Size(67, 13);
            this._nodePortLabel.TabIndex = 14;
            this._nodePortLabel.Text = "Node.&js port:";
            // 
            // _scriptArgsLabel
            // 
            this._scriptArgsLabel.AutoSize = true;
            this._scriptArgsLabel.Location = new System.Drawing.Point(19, 123);
            this._scriptArgsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._scriptArgsLabel.Name = "_scriptArgsLabel";
            this._scriptArgsLabel.Size = new System.Drawing.Size(89, 13);
            this._scriptArgsLabel.TabIndex = 7;
            this._scriptArgsLabel.Text = "Script ar&guments:";
            // 
            // _scriptArguments
            // 
            this._scriptArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._scriptArguments.Location = new System.Drawing.Point(180, 119);
            this._scriptArguments.Margin = new System.Windows.Forms.Padding(4);
            this._scriptArguments.Name = "_scriptArguments";
            this._scriptArguments.Size = new System.Drawing.Size(452, 20);
            this._scriptArguments.TabIndex = 8;
            this._scriptArguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _workingDirLabel
            // 
            this._workingDirLabel.AutoSize = true;
            this._workingDirLabel.Location = new System.Drawing.Point(19, 155);
            this._workingDirLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._workingDirLabel.Name = "_workingDirLabel";
            this._workingDirLabel.Size = new System.Drawing.Size(93, 13);
            this._workingDirLabel.TabIndex = 9;
            this._workingDirLabel.Text = "Working director&y:";
            // 
            // _workingDir
            // 
            this._workingDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._workingDir.Location = new System.Drawing.Point(180, 151);
            this._workingDir.Margin = new System.Windows.Forms.Padding(4);
            this._workingDir.Name = "_workingDir";
            this._workingDir.Size = new System.Drawing.Size(409, 20);
            this._workingDir.TabIndex = 10;
            this._workingDir.TextChanged += new System.EventHandler(this.WorkingDirTextChanged);
            // 
            // _launchUrl
            // 
            this._launchUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._launchUrl.Location = new System.Drawing.Point(180, 185);
            this._launchUrl.Margin = new System.Windows.Forms.Padding(4);
            this._launchUrl.Name = "_launchUrl";
            this._launchUrl.Size = new System.Drawing.Size(452, 20);
            this._launchUrl.TabIndex = 13;
            this._launchUrl.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _launchUrlLabel
            // 
            this._launchUrlLabel.AutoSize = true;
            this._launchUrlLabel.Location = new System.Drawing.Point(19, 188);
            this._launchUrlLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._launchUrlLabel.Name = "_launchUrlLabel";
            this._launchUrlLabel.Size = new System.Drawing.Size(71, 13);
            this._launchUrlLabel.TabIndex = 12;
            this._launchUrlLabel.Text = "Launch &URL:";
            // 
            // _envVars
            // 
            this._envVars.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._envVars.Location = new System.Drawing.Point(180, 281);
            this._envVars.Margin = new System.Windows.Forms.Padding(4);
            this._envVars.Multiline = true;
            this._envVars.Name = "_envVars";
            this._envVars.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._envVars.Size = new System.Drawing.Size(452, 61);
            this._envVars.TabIndex = 19;
            this._envVars.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _browsePath
            // 
            this._browsePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browsePath.Location = new System.Drawing.Point(599, 21);
            this._browsePath.Margin = new System.Windows.Forms.Padding(4);
            this._browsePath.Name = "_browsePath";
            this._browsePath.Size = new System.Drawing.Size(35, 28);
            this._browsePath.TabIndex = 2;
            this._browsePath.Text = "...";
            this._browsePath.UseVisualStyleBackColor = true;
            this._browsePath.Click += new System.EventHandler(this.BrowsePathClick);
            // 
            // _browseDirectory
            // 
            this._browseDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseDirectory.Location = new System.Drawing.Point(599, 149);
            this._browseDirectory.Margin = new System.Windows.Forms.Padding(4);
            this._browseDirectory.Name = "_browseDirectory";
            this._browseDirectory.Size = new System.Drawing.Size(35, 28);
            this._browseDirectory.TabIndex = 11;
            this._browseDirectory.Text = "...";
            this._browseDirectory.UseVisualStyleBackColor = true;
            this._browseDirectory.Click += new System.EventHandler(this.BrowseDirectoryClick);
            // 
            // _nodeExeErrorProvider
            // 
            this._nodeExeErrorProvider.ContainerControl = this;
            // 
            // _debuggerPortLabel
            // 
            this._debuggerPortLabel.AutoSize = true;
            this._debuggerPortLabel.Location = new System.Drawing.Point(19, 252);
            this._debuggerPortLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._debuggerPortLabel.Name = "_debuggerPortLabel";
            this._debuggerPortLabel.Size = new System.Drawing.Size(78, 13);
            this._debuggerPortLabel.TabIndex = 16;
            this._debuggerPortLabel.Text = "&Debugger port:";
            // 
            // _debuggerPort
            // 
            this._debuggerPort.Location = new System.Drawing.Point(180, 249);
            this._debuggerPort.Margin = new System.Windows.Forms.Padding(4);
            this._debuggerPort.Name = "_debuggerPort";
            this._debuggerPort.Size = new System.Drawing.Size(139, 20);
            this._debuggerPort.TabIndex = 17;
            this._debuggerPort.TextChanged += new System.EventHandler(this.PortChanged);
            // 
            // _envVarsLabel
            // 
            this._envVarsLabel.AutoSize = true;
            this._envVarsLabel.Location = new System.Drawing.Point(19, 284);
            this._envVarsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._envVarsLabel.Name = "_envVarsLabel";
            this._envVarsLabel.Size = new System.Drawing.Size(115, 13);
            this._envVarsLabel.TabIndex = 18;
            this._envVarsLabel.Text = "Environment &Variables:";
            // 
            // _scriptLabel
            // 
            this._scriptLabel.AutoSize = true;
            this._scriptLabel.Location = new System.Drawing.Point(19, 91);
            this._scriptLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._scriptLabel.Name = "_scriptLabel";
            this._scriptLabel.Size = new System.Drawing.Size(94, 13);
            this._scriptLabel.TabIndex = 5;
            this._scriptLabel.Text = "Script (startup file):";
            // 
            // _scriptFile
            // 
            this._scriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._scriptFile.Location = new System.Drawing.Point(180, 87);
            this._scriptFile.Margin = new System.Windows.Forms.Padding(4);
            this._scriptFile.Name = "_scriptFile";
            this._scriptFile.Size = new System.Drawing.Size(452, 20);
            this._scriptFile.TabIndex = 6;
            this._scriptFile.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _overrideDefaultBrowser
            // 
            this._overrideDefaultBrowser.AutoSize = true;
            this._overrideDefaultBrowser.Location = new System.Drawing.Point(23, 374);
            this._overrideDefaultBrowser.Name = "_overrideDefaultBrowser";
            this._overrideDefaultBrowser.Size = new System.Drawing.Size(141, 17);
            this._overrideDefaultBrowser.TabIndex = 21;
            this._overrideDefaultBrowser.Text = "Override default browser";
            this._overrideDefaultBrowser.UseVisualStyleBackColor = true;
            this._overrideDefaultBrowser.CheckedChanged += new System.EventHandler(this.Changed);
            // 
            // _browserArgumentsLabel
            // 
            this._browserArgumentsLabel.AutoSize = true;
            this._browserArgumentsLabel.Location = new System.Drawing.Point(20, 430);
            this._browserArgumentsLabel.Name = "_browserArgumentsLabel";
            this._browserArgumentsLabel.Size = new System.Drawing.Size(100, 13);
            this._browserArgumentsLabel.TabIndex = 23;
            this._browserArgumentsLabel.Text = "Browser arguments:";
            // 
            // _browserExecutableLabel
            // 
            this._browserExecutableLabel.AutoSize = true;
            this._browserExecutableLabel.Location = new System.Drawing.Point(20, 400);
            this._browserExecutableLabel.Name = "_browserExecutableLabel";
            this._browserExecutableLabel.Size = new System.Drawing.Size(103, 13);
            this._browserExecutableLabel.TabIndex = 22;
            this._browserExecutableLabel.Text = "Browser executable:";
            // 
            // _browseBrowser
            // 
            this._browseBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._browseBrowser.Location = new System.Drawing.Point(597, 392);
            this._browseBrowser.Margin = new System.Windows.Forms.Padding(4);
            this._browseBrowser.Name = "_browseBrowser";
            this._browseBrowser.Size = new System.Drawing.Size(35, 28);
            this._browseBrowser.TabIndex = 25;
            this._browseBrowser.Text = "...";
            this._browseBrowser.UseVisualStyleBackColor = true;
            this._browseBrowser.Click += new System.EventHandler(this.BrowseBrowserClick);
            // 
            // _browserExecutable
            // 
            this._browserExecutable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._browserExecutable.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._browserExecutable.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this._browserExecutable.Location = new System.Drawing.Point(180, 397);
            this._browserExecutable.Name = "_browserExecutable";
            this._browserExecutable.Size = new System.Drawing.Size(409, 20);
            this._browserExecutable.TabIndex = 24;
            this._browserExecutable.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _browserArguments
            // 
            this._browserArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._browserArguments.Location = new System.Drawing.Point(180, 427);
            this._browserArguments.Name = "_browserArguments";
            this._browserArguments.Size = new System.Drawing.Size(452, 20);
            this._browserArguments.TabIndex = 26;
            this._browserArguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // NodejsGeneralPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._browserArguments);
            this.Controls.Add(this._browseBrowser);
            this.Controls.Add(this._browserExecutable);
            this.Controls.Add(this._browserArgumentsLabel);
            this.Controls.Add(this._browserExecutableLabel);
            this.Controls.Add(this._overrideDefaultBrowser);
            this.Controls.Add(this._scriptLabel);
            this.Controls.Add(this._scriptFile);
            this.Controls.Add(this._envVarsLabel);
            this.Controls.Add(this._envVars);
            this.Controls.Add(this._debuggerPort);
            this.Controls.Add(this._debuggerPortLabel);
            this.Controls.Add(this._browseDirectory);
            this.Controls.Add(this._browsePath);
            this.Controls.Add(this._nodePortLabel);
            this.Controls.Add(this._launchUrlLabel);
            this.Controls.Add(this._workingDirLabel);
            this.Controls.Add(this._scriptArgsLabel);
            this.Controls.Add(this._nodeArguments);
            this.Controls.Add(this._nodeExePathLabel);
            this.Controls.Add(this._startBrowser);
            this.Controls.Add(this._nodejsPort);
            this.Controls.Add(this._launchUrl);
            this.Controls.Add(this._workingDir);
            this.Controls.Add(this._scriptArguments);
            this.Controls.Add(this._nodeExeArguments);
            this.Controls.Add(this._nodeExePath);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(404, 373);
            this.Name = "NodejsGeneralPropertyPageControl";
            this.Size = new System.Drawing.Size(651, 464);
            ((System.ComponentModel.ISupportInitialize)(this._nodeExeErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _nodeExePathLabel;
        private System.Windows.Forms.Label _nodeArguments;
        private System.Windows.Forms.CheckBox _startBrowser;
        private System.Windows.Forms.TextBox _nodeExePath;
        private System.Windows.Forms.TextBox _nodeExeArguments;
        private System.Windows.Forms.TextBox _nodejsPort;
        private System.Windows.Forms.Label _nodePortLabel;
        private System.Windows.Forms.Label _scriptArgsLabel;
        private System.Windows.Forms.TextBox _scriptArguments;
        private System.Windows.Forms.Label _workingDirLabel;
        private System.Windows.Forms.TextBox _workingDir;
        private System.Windows.Forms.TextBox _launchUrl;
        private System.Windows.Forms.Label _launchUrlLabel;
        private System.Windows.Forms.ToolTip _tooltip;
        private System.Windows.Forms.Button _browsePath;
        private System.Windows.Forms.Button _browseDirectory;
        private System.Windows.Forms.ErrorProvider _nodeExeErrorProvider;
        private System.Windows.Forms.Label _envVarsLabel;
        private System.Windows.Forms.TextBox _envVars;
        private System.Windows.Forms.TextBox _debuggerPort;
        private System.Windows.Forms.Label _debuggerPortLabel;
        private System.Windows.Forms.Label _scriptLabel;
        private System.Windows.Forms.TextBox _scriptFile;
        private System.Windows.Forms.TextBox _browserArguments;
        private System.Windows.Forms.Button _browseBrowser;
        private System.Windows.Forms.TextBox _browserExecutable;
        private System.Windows.Forms.Label _browserArgumentsLabel;
        private System.Windows.Forms.Label _browserExecutableLabel;
        private System.Windows.Forms.CheckBox _overrideDefaultBrowser;
    }
}

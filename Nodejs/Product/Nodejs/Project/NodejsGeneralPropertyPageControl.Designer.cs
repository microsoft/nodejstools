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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodejsGeneralPropertyPageControl));
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
            ((System.ComponentModel.ISupportInitialize)(this._nodeExeErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // _nodeExePathLabel
            // 
            resources.ApplyResources(this._nodeExePathLabel, "_nodeExePathLabel");
            this._nodeExePathLabel.Name = "_nodeExePathLabel";
            // 
            // _nodeArguments
            // 
            resources.ApplyResources(this._nodeArguments, "_nodeArguments");
            this._nodeArguments.Name = "_nodeArguments";
            // 
            // _startBrowser
            // 
            resources.ApplyResources(this._startBrowser, "_startBrowser");
            this._startBrowser.Name = "_startBrowser";
            this._startBrowser.UseVisualStyleBackColor = true;
            this._startBrowser.CheckedChanged += new System.EventHandler(this.Changed);
            // 
            // _nodeExePath
            // 
            resources.ApplyResources(this._nodeExePath, "_nodeExePath");
            this._nodeExePath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._nodeExePath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this._nodeExePath.Name = "_nodeExePath";
            this._nodeExePath.TextChanged += new System.EventHandler(this.NodeExePathChanged);
            // 
            // _nodeExeArguments
            // 
            resources.ApplyResources(this._nodeExeArguments, "_nodeExeArguments");
            this._nodeExeArguments.Name = "_nodeExeArguments";
            this._nodeExeArguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _nodejsPort
            // 
            resources.ApplyResources(this._nodejsPort, "_nodejsPort");
            this._nodejsPort.Name = "_nodejsPort";
            this._nodejsPort.TextChanged += new System.EventHandler(this.PortChanged);
            // 
            // _nodePortLabel
            // 
            resources.ApplyResources(this._nodePortLabel, "_nodePortLabel");
            this._nodePortLabel.Name = "_nodePortLabel";
            // 
            // _scriptArgsLabel
            // 
            resources.ApplyResources(this._scriptArgsLabel, "_scriptArgsLabel");
            this._scriptArgsLabel.Name = "_scriptArgsLabel";
            // 
            // _scriptArguments
            // 
            resources.ApplyResources(this._scriptArguments, "_scriptArguments");
            this._scriptArguments.Name = "_scriptArguments";
            this._scriptArguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _workingDirLabel
            // 
            resources.ApplyResources(this._workingDirLabel, "_workingDirLabel");
            this._workingDirLabel.Name = "_workingDirLabel";
            // 
            // _workingDir
            // 
            resources.ApplyResources(this._workingDir, "_workingDir");
            this._workingDir.Name = "_workingDir";
            this._workingDir.TextChanged += new System.EventHandler(this.WorkingDirTextChanged);
            // 
            // _launchUrl
            // 
            resources.ApplyResources(this._launchUrl, "_launchUrl");
            this._launchUrl.Name = "_launchUrl";
            this._launchUrl.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _launchUrlLabel
            // 
            resources.ApplyResources(this._launchUrlLabel, "_launchUrlLabel");
            this._launchUrlLabel.Name = "_launchUrlLabel";
            // 
            // _envVars
            // 
            resources.ApplyResources(this._envVars, "_envVars");
            this._envVars.Name = "_envVars";
            this._envVars.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _browsePath
            // 
            resources.ApplyResources(this._browsePath, "_browsePath");
            this._browsePath.Name = "_browsePath";
            this._browsePath.UseVisualStyleBackColor = true;
            this._browsePath.Click += new System.EventHandler(this.BrowsePathClick);
            // 
            // _browseDirectory
            // 
            resources.ApplyResources(this._browseDirectory, "_browseDirectory");
            this._browseDirectory.Name = "_browseDirectory";
            this._browseDirectory.UseVisualStyleBackColor = true;
            this._browseDirectory.Click += new System.EventHandler(this.BrowseDirectoryClick);
            // 
            // _nodeExeErrorProvider
            // 
            this._nodeExeErrorProvider.ContainerControl = this;
            // 
            // _debuggerPortLabel
            // 
            resources.ApplyResources(this._debuggerPortLabel, "_debuggerPortLabel");
            this._debuggerPortLabel.Name = "_debuggerPortLabel";
            // 
            // _debuggerPort
            // 
            resources.ApplyResources(this._debuggerPort, "_debuggerPort");
            this._debuggerPort.Name = "_debuggerPort";
            this._debuggerPort.TextChanged += new System.EventHandler(this.PortChanged);
            // 
            // _envVarsLabel
            // 
            resources.ApplyResources(this._envVarsLabel, "_envVarsLabel");
            this._envVarsLabel.Name = "_envVarsLabel";
            // 
            // _scriptLabel
            // 
            resources.ApplyResources(this._scriptLabel, "_scriptLabel");
            this._scriptLabel.Name = "_scriptLabel";
            // 
            // _scriptFile
            // 
            resources.ApplyResources(this._scriptFile, "_scriptFile");
            this._scriptFile.Name = "_scriptFile";
            this._scriptFile.TextChanged += new System.EventHandler(this.Changed);
            // 
            // NodejsGeneralPropertyPageControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
            this.Name = "NodejsGeneralPropertyPageControl";
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
    }
}

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
            this.SuspendLayout();
            // 
            // _nodeLocLabel
            // 
            this._nodeExePathLabel.AutoSize = true;
            this._nodeExePathLabel.Location = new System.Drawing.Point(14, 22);
            this._nodeExePathLabel.Name = "_nodeExePathLabel";
            this._nodeExePathLabel.Size = new System.Drawing.Size(96, 13);
            this._nodeExePathLabel.TabIndex = 0;
            this._nodeExePathLabel.Text = "Node.exe &path:";
            // 
            // _nodeArguments
            // 
            this._nodeArguments.AutoSize = true;
            this._nodeArguments.Location = new System.Drawing.Point(14, 48);
            this._nodeArguments.Name = "_nodeArguments";
            this._nodeArguments.Size = new System.Drawing.Size(108, 13);
            this._nodeArguments.TabIndex = 2;
            this._nodeArguments.Text = "N&ode.exe arguments:";
            // 
            // _startBrowser
            // 
            this._startBrowser.AutoSize = true;
            this._startBrowser.Location = new System.Drawing.Point(17, 176);
            this._startBrowser.Name = "_startBrowser";
            this._startBrowser.Size = new System.Drawing.Size(161, 17);
            this._startBrowser.TabIndex = 12;
            this._startBrowser.Text = "St&art web browser on launch";
            this._startBrowser.UseVisualStyleBackColor = true;
            this._startBrowser.CheckedChanged += new System.EventHandler(this.Changed);
            // 
            // _nodeExeLocation
            // 
            this._nodeExePath.Location = new System.Drawing.Point(135, 19);
            this._nodeExePath.Name = "_nodeExePath";
            this._nodeExePath.Size = new System.Drawing.Size(290, 20);
            this._nodeExePath.TabIndex = 1;
            this._nodeExePath.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _nodeExeArguments
            // 
            this._nodeExeArguments.Location = new System.Drawing.Point(135, 45);
            this._nodeExeArguments.Name = "_nodeExeArguments";
            this._nodeExeArguments.Size = new System.Drawing.Size(290, 20);
            this._nodeExeArguments.TabIndex = 3;
            this._nodeExeArguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _nodejsPort
            // 
            this._nodejsPort.Location = new System.Drawing.Point(135, 150);
            this._nodejsPort.Name = "_nodejsPort";
            this._nodejsPort.Size = new System.Drawing.Size(100, 20);
            this._nodejsPort.TabIndex = 11;
            this._nodejsPort.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _nodePortLabel
            // 
            this._nodePortLabel.AutoSize = true;
            this._nodePortLabel.Location = new System.Drawing.Point(14, 153);
            this._nodePortLabel.Name = "_nodePortLabel";
            this._nodePortLabel.Size = new System.Drawing.Size(67, 13);
            this._nodePortLabel.TabIndex = 10;
            this._nodePortLabel.Text = "Node.&js port:";
            // 
            // _scriptArgsLabel
            // 
            this._scriptArgsLabel.AutoSize = true;
            this._scriptArgsLabel.Location = new System.Drawing.Point(14, 74);
            this._scriptArgsLabel.Name = "_scriptArgsLabel";
            this._scriptArgsLabel.Size = new System.Drawing.Size(89, 13);
            this._scriptArgsLabel.TabIndex = 4;
            this._scriptArgsLabel.Text = "Script ar&guments:";
            // 
            // _scriptArguments
            // 
            this._scriptArguments.Location = new System.Drawing.Point(135, 71);
            this._scriptArguments.Name = "_scriptArguments";
            this._scriptArguments.Size = new System.Drawing.Size(290, 20);
            this._scriptArguments.TabIndex = 5;
            this._scriptArguments.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _workingDirLabel
            // 
            this._workingDirLabel.AutoSize = true;
            this._workingDirLabel.Location = new System.Drawing.Point(14, 100);
            this._workingDirLabel.Name = "_workingDirLabel";
            this._workingDirLabel.Size = new System.Drawing.Size(95, 13);
            this._workingDirLabel.TabIndex = 6;
            this._workingDirLabel.Text = "Working director&y:";
            // 
            // _workingDir
            // 
            this._workingDir.Location = new System.Drawing.Point(135, 97);
            this._workingDir.Name = "_workingDir";
            this._workingDir.Size = new System.Drawing.Size(290, 20);
            this._workingDir.TabIndex = 7;
            this._workingDir.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _launchUrl
            // 
            this._launchUrl.Location = new System.Drawing.Point(135, 124);
            this._launchUrl.Name = "_launchUrl";
            this._launchUrl.Size = new System.Drawing.Size(290, 20);
            this._launchUrl.TabIndex = 9;
            this._launchUrl.TextChanged += new System.EventHandler(this.Changed);
            // 
            // _launchUrlLabel
            // 
            this._launchUrlLabel.AutoSize = true;
            this._launchUrlLabel.Location = new System.Drawing.Point(14, 127);
            this._launchUrlLabel.Name = "_launchUrlLabel";
            this._launchUrlLabel.Size = new System.Drawing.Size(71, 13);
            this._launchUrlLabel.TabIndex = 8;
            this._launchUrlLabel.Text = "Launch &URL:";
            // 
            // NodejsGeneralPropertyPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
            this.Size = new System.Drawing.Size(513, 302);
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
    }
}

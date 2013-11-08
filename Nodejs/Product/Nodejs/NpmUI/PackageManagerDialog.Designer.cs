namespace Microsoft.NodejsTools.NpmUI
{
    partial class PackageManagerDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._panelFooter = new System.Windows.Forms.Panel();
            this._labelWarningText = new System.Windows.Forms.Label();
            this._labelWarning = new System.Windows.Forms.Label();
            this._btnClose = new System.Windows.Forms.Button();
            this._panePackageSources = new Microsoft.NodejsTools.NpmUI.PackageSourcesPane();
            this._paneInstalledPackages = new Microsoft.NodejsTools.NpmUI.InstalledPackagesPane();
            this._panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelFooter
            // 
            this._panelFooter.BackColor = System.Drawing.SystemColors.Control;
            this._panelFooter.Controls.Add(this._labelWarningText);
            this._panelFooter.Controls.Add(this._labelWarning);
            this._panelFooter.Controls.Add(this._btnClose);
            this._panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panelFooter.Location = new System.Drawing.Point(0, 516);
            this._panelFooter.Name = "_panelFooter";
            this._panelFooter.Size = new System.Drawing.Size(784, 45);
            this._panelFooter.TabIndex = 0;
            // 
            // _labelWarningText
            // 
            this._labelWarningText.AutoSize = true;
            this._labelWarningText.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelWarningText.Location = new System.Drawing.Point(69, 16);
            this._labelWarningText.Name = "_labelWarningText";
            this._labelWarningText.Size = new System.Drawing.Size(403, 13);
            this._labelWarningText.TabIndex = 2;
            this._labelWarningText.Text = "Packages will be installed globally when the Global Packages tab is selected.";
            // 
            // _labelWarning
            // 
            this._labelWarning.AutoSize = true;
            this._labelWarning.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelWarning.ForeColor = System.Drawing.Color.Red;
            this._labelWarning.Location = new System.Drawing.Point(6, 16);
            this._labelWarning.Name = "_labelWarning";
            this._labelWarning.Size = new System.Drawing.Size(65, 13);
            this._labelWarning.TabIndex = 1;
            this._labelWarning.Text = "WARNING:";
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(698, 10);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(75, 25);
            this._btnClose.TabIndex = 0;
            this._btnClose.Text = "Close";
            this._btnClose.UseVisualStyleBackColor = true;
            this._btnClose.Click += new System.EventHandler(this._btnClose_Click);
            // 
            // _panePackageSources
            // 
            this._panePackageSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panePackageSources.Location = new System.Drawing.Point(300, 0);
            this._panePackageSources.Name = "_panePackageSources";
            this._panePackageSources.Size = new System.Drawing.Size(484, 516);
            this._panePackageSources.TabIndex = 2;
            this._panePackageSources.InstallPackageRequested += new System.EventHandler<Microsoft.NodejsTools.NpmUI.PackageInstallEventArgs>(this._panePackageSources_InstallPackageRequested);
            // 
            // _paneInstalledPackages
            // 
            this._paneInstalledPackages.Dock = System.Windows.Forms.DockStyle.Left;
            this._paneInstalledPackages.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._paneInstalledPackages.Location = new System.Drawing.Point(0, 0);
            this._paneInstalledPackages.Name = "_paneInstalledPackages";
            this._paneInstalledPackages.Size = new System.Drawing.Size(300, 516);
            this._paneInstalledPackages.TabIndex = 1;
            this._paneInstalledPackages.SelectedPackageViewChanged += new System.EventHandler(this._paneInstalledPackages_SelectedPackageViewChanged);
            this._paneInstalledPackages.UninstallLocalPackageRequested += new System.EventHandler<Microsoft.NodejsTools.NpmUI.PackageEventArgs>(this._paneInstalledPackages_UninstallLocalPackageRequested);
            this._paneInstalledPackages.UninstallGloballPackageRequested += new System.EventHandler<Microsoft.NodejsTools.NpmUI.PackageEventArgs>(this._paneInstalledPackages_UninstallGloballPackageRequested);
            // 
            // PackageManagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this._panePackageSources);
            this.Controls.Add(this._paneInstalledPackages);
            this.Controls.Add(this._panelFooter);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.Name = "PackageManagerDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PackageManagerDialog";
            this._panelFooter.ResumeLayout(false);
            this._panelFooter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelFooter;
        private System.Windows.Forms.Button _btnClose;
        private InstalledPackagesPane _paneInstalledPackages;
        private PackageSourcesPane _panePackageSources;
        private System.Windows.Forms.Label _labelWarningText;
        private System.Windows.Forms.Label _labelWarning;
    }
}
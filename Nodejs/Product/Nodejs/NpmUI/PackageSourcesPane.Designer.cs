namespace Microsoft.NodejsTools.NpmUI
{
    partial class PackageSourcesPane
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._labelHeader = new System.Windows.Forms.Label();
            this._panelFooter = new System.Windows.Forms.Panel();
            this._labelInstallAs = new System.Windows.Forms.Label();
            this._comboDepType = new System.Windows.Forms.ComboBox();
            this._btnInstall = new System.Windows.Forms.Button();
            this._tabCtrlPackageSources = new System.Windows.Forms.TabControl();
            this._tabPackageDetails = new System.Windows.Forms.TabPage();
            this._tabSearchRepository = new System.Windows.Forms.TabPage();
            this.packageInstallPane1 = new Microsoft.NodejsTools.NpmUI.PackageInstallPane();
            this.packageSearchPane1 = new Microsoft.NodejsTools.NpmUI.PackageSearchPane();
            this._panelFooter.SuspendLayout();
            this._tabCtrlPackageSources.SuspendLayout();
            this._tabPackageDetails.SuspendLayout();
            this._tabSearchRepository.SuspendLayout();
            this.SuspendLayout();
            // 
            // _labelHeader
            // 
            this._labelHeader.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this._labelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this._labelHeader.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._labelHeader.Location = new System.Drawing.Point(0, 0);
            this._labelHeader.Name = "_labelHeader";
            this._labelHeader.Padding = new System.Windows.Forms.Padding(4);
            this._labelHeader.Size = new System.Drawing.Size(484, 27);
            this._labelHeader.TabIndex = 1;
            this._labelHeader.Text = "Package Sources";
            this._labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _panelFooter
            // 
            this._panelFooter.Controls.Add(this._labelInstallAs);
            this._panelFooter.Controls.Add(this._comboDepType);
            this._panelFooter.Controls.Add(this._btnInstall);
            this._panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panelFooter.Location = new System.Drawing.Point(0, 566);
            this._panelFooter.Name = "_panelFooter";
            this._panelFooter.Size = new System.Drawing.Size(484, 34);
            this._panelFooter.TabIndex = 2;
            // 
            // _labelInstallAs
            // 
            this._labelInstallAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._labelInstallAs.AutoSize = true;
            this._labelInstallAs.Location = new System.Drawing.Point(4, 8);
            this._labelInstallAs.Name = "_labelInstallAs";
            this._labelInstallAs.Size = new System.Drawing.Size(52, 13);
            this._labelInstallAs.TabIndex = 2;
            this._labelInstallAs.Text = "Install As:";
            // 
            // _comboDepType
            // 
            this._comboDepType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._comboDepType.FormattingEnabled = true;
            this._comboDepType.Items.AddRange(new object[] {
            "Standard Dependency",
            "Dev Dependency",
            "Optional Dependency"});
            this._comboDepType.Location = new System.Drawing.Point(62, 5);
            this._comboDepType.Name = "_comboDepType";
            this._comboDepType.Size = new System.Drawing.Size(169, 21);
            this._comboDepType.TabIndex = 1;
            // 
            // _btnInstall
            // 
            this._btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._btnInstall.Location = new System.Drawing.Point(237, 4);
            this._btnInstall.Name = "_btnInstall";
            this._btnInstall.Size = new System.Drawing.Size(91, 23);
            this._btnInstall.TabIndex = 0;
            this._btnInstall.Text = "Install Local";
            this._btnInstall.UseVisualStyleBackColor = true;
            // 
            // _tabCtrlPackageSources
            // 
            this._tabCtrlPackageSources.Controls.Add(this._tabPackageDetails);
            this._tabCtrlPackageSources.Controls.Add(this._tabSearchRepository);
            this._tabCtrlPackageSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabCtrlPackageSources.Location = new System.Drawing.Point(0, 27);
            this._tabCtrlPackageSources.Name = "_tabCtrlPackageSources";
            this._tabCtrlPackageSources.Padding = new System.Drawing.Point(6, 8);
            this._tabCtrlPackageSources.SelectedIndex = 0;
            this._tabCtrlPackageSources.Size = new System.Drawing.Size(484, 539);
            this._tabCtrlPackageSources.TabIndex = 3;
            this._tabCtrlPackageSources.SelectedIndexChanged += new System.EventHandler(this._tabCtrlPackageSources_SelectedIndexChanged);
            // 
            // _tabPackageDetails
            // 
            this._tabPackageDetails.BackColor = System.Drawing.Color.Transparent;
            this._tabPackageDetails.Controls.Add(this.packageInstallPane1);
            this._tabPackageDetails.Location = new System.Drawing.Point(4, 32);
            this._tabPackageDetails.Name = "_tabPackageDetails";
            this._tabPackageDetails.Padding = new System.Windows.Forms.Padding(3);
            this._tabPackageDetails.Size = new System.Drawing.Size(476, 503);
            this._tabPackageDetails.TabIndex = 0;
            this._tabPackageDetails.Text = "Package Details";
            this._tabPackageDetails.UseVisualStyleBackColor = true;
            // 
            // _tabSearchRepository
            // 
            this._tabSearchRepository.Controls.Add(this.packageSearchPane1);
            this._tabSearchRepository.Location = new System.Drawing.Point(4, 32);
            this._tabSearchRepository.Name = "_tabSearchRepository";
            this._tabSearchRepository.Padding = new System.Windows.Forms.Padding(3);
            this._tabSearchRepository.Size = new System.Drawing.Size(492, 503);
            this._tabSearchRepository.TabIndex = 1;
            this._tabSearchRepository.Text = "Search npm Repository";
            this._tabSearchRepository.UseVisualStyleBackColor = true;
            // 
            // packageInstallPane1
            // 
            this.packageInstallPane1.BackColor = System.Drawing.Color.Transparent;
            this.packageInstallPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageInstallPane1.Location = new System.Drawing.Point(3, 3);
            this.packageInstallPane1.Name = "packageInstallPane1";
            this.packageInstallPane1.Size = new System.Drawing.Size(470, 497);
            this.packageInstallPane1.TabIndex = 0;
            // 
            // packageSearchPane1
            // 
            this.packageSearchPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageSearchPane1.Location = new System.Drawing.Point(3, 3);
            this.packageSearchPane1.Name = "packageSearchPane1";
            this.packageSearchPane1.Size = new System.Drawing.Size(486, 497);
            this.packageSearchPane1.TabIndex = 0;
            // 
            // PackageSourcesPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._tabCtrlPackageSources);
            this.Controls.Add(this._panelFooter);
            this.Controls.Add(this._labelHeader);
            this.Name = "PackageSourcesPane";
            this.Size = new System.Drawing.Size(484, 600);
            this._panelFooter.ResumeLayout(false);
            this._panelFooter.PerformLayout();
            this._tabCtrlPackageSources.ResumeLayout(false);
            this._tabPackageDetails.ResumeLayout(false);
            this._tabSearchRepository.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _labelHeader;
        private System.Windows.Forms.Panel _panelFooter;
        private System.Windows.Forms.Label _labelInstallAs;
        private System.Windows.Forms.ComboBox _comboDepType;
        private System.Windows.Forms.Button _btnInstall;
        private System.Windows.Forms.TabControl _tabCtrlPackageSources;
        private System.Windows.Forms.TabPage _tabPackageDetails;
        private System.Windows.Forms.TabPage _tabSearchRepository;
        private PackageInstallPane packageInstallPane1;
        private PackageSearchPane packageSearchPane1;
    }
}

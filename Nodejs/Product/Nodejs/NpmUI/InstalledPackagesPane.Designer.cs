namespace Microsoft.NodejsTools.NpmUI
{
    partial class InstalledPackagesPane
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
            this._tabCtrlInstalledPackages = new System.Windows.Forms.TabControl();
            this._tabLocalPackages = new System.Windows.Forms.TabPage();
            this.installedPackageListControl1 = new Microsoft.NodejsTools.NpmUI.InstalledPackageListControl();
            this._tabGlobalPackages = new System.Windows.Forms.TabPage();
            this.installedPackageListControl2 = new Microsoft.NodejsTools.NpmUI.InstalledPackageListControl();
            this._tabCtrlInstalledPackages.SuspendLayout();
            this._tabLocalPackages.SuspendLayout();
            this._tabGlobalPackages.SuspendLayout();
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
            this._labelHeader.Size = new System.Drawing.Size(300, 27);
            this._labelHeader.TabIndex = 0;
            this._labelHeader.Text = "Installed Packages";
            this._labelHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tabCtrlInstalledPackages
            // 
            this._tabCtrlInstalledPackages.Controls.Add(this._tabLocalPackages);
            this._tabCtrlInstalledPackages.Controls.Add(this._tabGlobalPackages);
            this._tabCtrlInstalledPackages.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabCtrlInstalledPackages.Location = new System.Drawing.Point(0, 27);
            this._tabCtrlInstalledPackages.Name = "_tabCtrlInstalledPackages";
            this._tabCtrlInstalledPackages.Padding = new System.Drawing.Point(6, 8);
            this._tabCtrlInstalledPackages.SelectedIndex = 0;
            this._tabCtrlInstalledPackages.Size = new System.Drawing.Size(300, 573);
            this._tabCtrlInstalledPackages.TabIndex = 1;
            this._tabCtrlInstalledPackages.SelectedIndexChanged += new System.EventHandler(this._tabCtrlInstalledPackages_SelectedIndexChanged);
            // 
            // _tabLocalPackages
            // 
            this._tabLocalPackages.Controls.Add(this.installedPackageListControl1);
            this._tabLocalPackages.Location = new System.Drawing.Point(4, 32);
            this._tabLocalPackages.Name = "_tabLocalPackages";
            this._tabLocalPackages.Padding = new System.Windows.Forms.Padding(3);
            this._tabLocalPackages.Size = new System.Drawing.Size(292, 537);
            this._tabLocalPackages.TabIndex = 0;
            this._tabLocalPackages.Text = "Local Packages";
            this._tabLocalPackages.UseVisualStyleBackColor = true;
            // 
            // installedPackageListControl1
            // 
            this.installedPackageListControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installedPackageListControl1.Location = new System.Drawing.Point(3, 3);
            this.installedPackageListControl1.Name = "installedPackageListControl1";
            this.installedPackageListControl1.Size = new System.Drawing.Size(286, 531);
            this.installedPackageListControl1.TabIndex = 0;
            // 
            // _tabGlobalPackages
            // 
            this._tabGlobalPackages.Controls.Add(this.installedPackageListControl2);
            this._tabGlobalPackages.Location = new System.Drawing.Point(4, 32);
            this._tabGlobalPackages.Name = "_tabGlobalPackages";
            this._tabGlobalPackages.Padding = new System.Windows.Forms.Padding(3);
            this._tabGlobalPackages.Size = new System.Drawing.Size(292, 537);
            this._tabGlobalPackages.TabIndex = 1;
            this._tabGlobalPackages.Text = "Global Packages";
            this._tabGlobalPackages.UseVisualStyleBackColor = true;
            // 
            // installedPackageListControl2
            // 
            this.installedPackageListControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.installedPackageListControl2.Location = new System.Drawing.Point(3, 3);
            this.installedPackageListControl2.Name = "installedPackageListControl2";
            this.installedPackageListControl2.Size = new System.Drawing.Size(286, 531);
            this.installedPackageListControl2.TabIndex = 0;
            // 
            // InstalledPackagesPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._tabCtrlInstalledPackages);
            this.Controls.Add(this._labelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "InstalledPackagesPane";
            this.Size = new System.Drawing.Size(300, 600);
            this._tabCtrlInstalledPackages.ResumeLayout(false);
            this._tabLocalPackages.ResumeLayout(false);
            this._tabGlobalPackages.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _labelHeader;
        private System.Windows.Forms.TabControl _tabCtrlInstalledPackages;
        private System.Windows.Forms.TabPage _tabLocalPackages;
        private System.Windows.Forms.TabPage _tabGlobalPackages;
        private InstalledPackageListControl installedPackageListControl1;
        private InstalledPackageListControl installedPackageListControl2;
    }
}

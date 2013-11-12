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
            this._listLocalPackages = new Microsoft.NodejsTools.NpmUI.InstalledPackageListControl();
            this._tabGlobalPackages = new System.Windows.Forms.TabPage();
            this._listGlobalPackages = new Microsoft.NodejsTools.NpmUI.InstalledPackageListControl();
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
            this._tabLocalPackages.Controls.Add(this._listLocalPackages);
            this._tabLocalPackages.Location = new System.Drawing.Point(4, 32);
            this._tabLocalPackages.Name = "_tabLocalPackages";
            this._tabLocalPackages.Padding = new System.Windows.Forms.Padding(3);
            this._tabLocalPackages.Size = new System.Drawing.Size(292, 537);
            this._tabLocalPackages.TabIndex = 0;
            this._tabLocalPackages.Text = "Local Packages";
            this._tabLocalPackages.UseVisualStyleBackColor = true;
            // 
            // _listLocalPackages
            // 
            this._listLocalPackages.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listLocalPackages.Location = new System.Drawing.Point(3, 3);
            this._listLocalPackages.Name = "_listLocalPackages";
            this._listLocalPackages.Size = new System.Drawing.Size(286, 531);
            this._listLocalPackages.TabIndex = 0;
            this._listLocalPackages.UninstallPackageRequested += new System.EventHandler<Microsoft.NodejsTools.NpmUI.PackageEventArgs>(this._listLocalPackages_UninstallPackageRequested);
            // 
            // _tabGlobalPackages
            // 
            this._tabGlobalPackages.Controls.Add(this._listGlobalPackages);
            this._tabGlobalPackages.Location = new System.Drawing.Point(4, 32);
            this._tabGlobalPackages.Name = "_tabGlobalPackages";
            this._tabGlobalPackages.Padding = new System.Windows.Forms.Padding(3);
            this._tabGlobalPackages.Size = new System.Drawing.Size(292, 537);
            this._tabGlobalPackages.TabIndex = 1;
            this._tabGlobalPackages.Text = "Global Packages";
            this._tabGlobalPackages.UseVisualStyleBackColor = true;
            // 
            // _listGlobalPackages
            // 
            this._listGlobalPackages.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listGlobalPackages.Location = new System.Drawing.Point(3, 3);
            this._listGlobalPackages.Name = "_listGlobalPackages";
            this._listGlobalPackages.Size = new System.Drawing.Size(286, 531);
            this._listGlobalPackages.TabIndex = 0;
            this._listGlobalPackages.UninstallPackageRequested += new System.EventHandler<Microsoft.NodejsTools.NpmUI.PackageEventArgs>(this._listGlobalPackages_UninstallPackageRequested);
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
        private InstalledPackageListControl _listLocalPackages;
        private InstalledPackageListControl _listGlobalPackages;
    }
}

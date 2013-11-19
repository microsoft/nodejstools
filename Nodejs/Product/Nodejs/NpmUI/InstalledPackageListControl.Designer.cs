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
    partial class InstalledPackageListControl
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
            this._btnUninstall = new System.Windows.Forms.Button();
            this._listPackages = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // _btnUninstall
            // 
            this._btnUninstall.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._btnUninstall.Location = new System.Drawing.Point(0, 520);
            this._btnUninstall.Name = "_btnUninstall";
            this._btnUninstall.Size = new System.Drawing.Size(296, 23);
            this._btnUninstall.TabIndex = 0;
            this._btnUninstall.Text = "Uninstall";
            this._btnUninstall.UseVisualStyleBackColor = true;
            this._btnUninstall.Click += new System.EventHandler(this._btnUninstall_Click);
            // 
            // _listPackages
            // 
            this._listPackages.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this._listPackages.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listPackages.FullRowSelect = true;
            this._listPackages.Location = new System.Drawing.Point(0, 0);
            this._listPackages.MultiSelect = false;
            this._listPackages.Name = "_listPackages";
            this._listPackages.OwnerDraw = true;
            this._listPackages.ShowGroups = false;
            this._listPackages.Size = new System.Drawing.Size(296, 520);
            this._listPackages.TabIndex = 1;
            this._listPackages.UseCompatibleStateImageBehavior = false;
            this._listPackages.View = System.Windows.Forms.View.Details;
            this._listPackages.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this._listPackages_DrawItem);
            this._listPackages.SelectedIndexChanged += new System.EventHandler(this._listPackages_SelectedIndexChanged);
            // 
            // InstalledPackageListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._listPackages);
            this.Controls.Add(this._btnUninstall);
            this.Name = "InstalledPackageListControl";
            this.Size = new System.Drawing.Size(296, 543);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _btnUninstall;
        private System.Windows.Forms.ListView _listPackages;
    }
}

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
    partial class PackageInstallPane
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
            this.components = new System.ComponentModel.Container();
            this._labelPackageName = new System.Windows.Forms.Label();
            this._labelVersionTag = new System.Windows.Forms.Label();
            this._txtPackageName = new System.Windows.Forms.TextBox();
            this._txtVersionTag = new System.Windows.Forms.TextBox();
            this._tooltip = new System.Windows.Forms.ToolTip(this.components);
            this._labelNameRequired = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _labelPackageName
            // 
            this._labelPackageName.AutoSize = true;
            this._labelPackageName.Location = new System.Drawing.Point(8, 18);
            this._labelPackageName.Name = "_labelPackageName";
            this._labelPackageName.Size = new System.Drawing.Size(84, 13);
            this._labelPackageName.TabIndex = 0;
            this._labelPackageName.Text = "Package &Name:";
            // 
            // _labelVersionTag
            // 
            this._labelVersionTag.AutoSize = true;
            this._labelVersionTag.Location = new System.Drawing.Point(8, 59);
            this._labelVersionTag.Name = "_labelVersionTag";
            this._labelVersionTag.Size = new System.Drawing.Size(84, 13);
            this._labelVersionTag.TabIndex = 1;
            this._labelVersionTag.Text = "&Version or Tag:";
            // 
            // _txtPackageName
            // 
            this._txtPackageName.Location = new System.Drawing.Point(94, 15);
            this._txtPackageName.Name = "_txtPackageName";
            this._txtPackageName.Size = new System.Drawing.Size(351, 22);
            this._txtPackageName.TabIndex = 2;
            this._tooltip.SetToolTip(this._txtPackageName, "Name of package. This must be an exact match.");
            this._txtPackageName.KeyUp += new System.Windows.Forms.KeyEventHandler(this._txtPackageName_KeyUp);
            // 
            // _txtVersionTag
            // 
            this._txtVersionTag.Location = new System.Drawing.Point(94, 56);
            this._txtVersionTag.Name = "_txtVersionTag";
            this._txtVersionTag.Size = new System.Drawing.Size(351, 22);
            this._txtVersionTag.TabIndex = 3;
            this._tooltip.SetToolTip(this._txtVersionTag, "Version, version range, or tag of required package.");
            this._txtVersionTag.KeyUp += new System.Windows.Forms.KeyEventHandler(this._txtVersionTag_KeyUp);
            // 
            // _labelNameRequired
            // 
            this._labelNameRequired.AutoSize = true;
            this._labelNameRequired.ForeColor = System.Drawing.Color.Red;
            this._labelNameRequired.Location = new System.Drawing.Point(448, 18);
            this._labelNameRequired.Name = "_labelNameRequired";
            this._labelNameRequired.Size = new System.Drawing.Size(12, 13);
            this._labelNameRequired.TabIndex = 4;
            this._labelNameRequired.Text = "*";
            // 
            // PackageInstallPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this._labelNameRequired);
            this.Controls.Add(this._txtVersionTag);
            this.Controls.Add(this._txtPackageName);
            this.Controls.Add(this._labelVersionTag);
            this.Controls.Add(this._labelPackageName);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PackageInstallPane";
            this.Size = new System.Drawing.Size(470, 112);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _labelPackageName;
        private System.Windows.Forms.Label _labelVersionTag;
        private System.Windows.Forms.TextBox _txtPackageName;
        private System.Windows.Forms.ToolTip _tooltip;
        private System.Windows.Forms.TextBox _txtVersionTag;
        private System.Windows.Forms.Label _labelNameRequired;
    }
}

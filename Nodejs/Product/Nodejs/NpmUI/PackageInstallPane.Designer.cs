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
            this._labelVersionTag.Location = new System.Drawing.Point(14, 59);
            this._labelVersionTag.Name = "_labelVersionTag";
            this._labelVersionTag.Size = new System.Drawing.Size(79, 13);
            this._labelVersionTag.TabIndex = 1;
            this._labelVersionTag.Text = "&Version or Tag:";
            // 
            // _txtPackageName
            // 
            this._txtPackageName.Location = new System.Drawing.Point(94, 15);
            this._txtPackageName.Name = "_txtPackageName";
            this._txtPackageName.Size = new System.Drawing.Size(351, 20);
            this._txtPackageName.TabIndex = 2;
            this._tooltip.SetToolTip(this._txtPackageName, "Name of package. This must be an exact match.");
            // 
            // _txtVersionTag
            // 
            this._txtVersionTag.Location = new System.Drawing.Point(94, 56);
            this._txtVersionTag.Name = "_txtVersionTag";
            this._txtVersionTag.Size = new System.Drawing.Size(351, 20);
            this._txtVersionTag.TabIndex = 3;
            this._tooltip.SetToolTip(this._txtVersionTag, "Version, version range, or tag of required package.");
            // 
            // _labelNameRequired
            // 
            this._labelNameRequired.AutoSize = true;
            this._labelNameRequired.ForeColor = System.Drawing.Color.Red;
            this._labelNameRequired.Location = new System.Drawing.Point(448, 18);
            this._labelNameRequired.Name = "_labelNameRequired";
            this._labelNameRequired.Size = new System.Drawing.Size(11, 13);
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

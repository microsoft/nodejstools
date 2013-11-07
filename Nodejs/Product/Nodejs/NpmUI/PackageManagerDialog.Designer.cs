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
            this._btnClose = new System.Windows.Forms.Button();
            this.packageSourcesPane1 = new Microsoft.NodejsTools.NpmUI.PackageSourcesPane();
            this.installedPackagesPane1 = new Microsoft.NodejsTools.NpmUI.InstalledPackagesPane();
            this._panelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelFooter
            // 
            this._panelFooter.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this._panelFooter.Controls.Add(this._btnClose);
            this._panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panelFooter.Location = new System.Drawing.Point(0, 516);
            this._panelFooter.Name = "_panelFooter";
            this._panelFooter.Size = new System.Drawing.Size(784, 45);
            this._panelFooter.TabIndex = 0;
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
            // 
            // packageSourcesPane1
            // 
            this.packageSourcesPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageSourcesPane1.Location = new System.Drawing.Point(300, 0);
            this.packageSourcesPane1.Name = "packageSourcesPane1";
            this.packageSourcesPane1.Size = new System.Drawing.Size(484, 516);
            this.packageSourcesPane1.TabIndex = 2;
            // 
            // installedPackagesPane1
            // 
            this.installedPackagesPane1.Dock = System.Windows.Forms.DockStyle.Left;
            this.installedPackagesPane1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installedPackagesPane1.Location = new System.Drawing.Point(0, 0);
            this.installedPackagesPane1.Name = "installedPackagesPane1";
            this.installedPackagesPane1.Size = new System.Drawing.Size(300, 516);
            this.installedPackagesPane1.TabIndex = 1;
            // 
            // PackageManagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.packageSourcesPane1);
            this.Controls.Add(this.installedPackagesPane1);
            this.Controls.Add(this._panelFooter);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.Name = "PackageManagerDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PackageManagerDialog";
            this._panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelFooter;
        private System.Windows.Forms.Button _btnClose;
        private InstalledPackagesPane installedPackagesPane1;
        private PackageSourcesPane packageSourcesPane1;
    }
}
namespace Microsoft.NodejsTools.NpmUI
{
    partial class PackageSearchPane
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

            if (disposing && null != _busy)
            {
                _busy.Dispose();
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
            this._panelFind = new System.Windows.Forms.Panel();
            this._txtFind = new System.Windows.Forms.TextBox();
            this._labelFind = new System.Windows.Forms.Label();
            this._listResults = new System.Windows.Forms.ListView();
            this._panelFind.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelFind
            // 
            this._panelFind.Controls.Add(this._txtFind);
            this._panelFind.Controls.Add(this._labelFind);
            this._panelFind.Dock = System.Windows.Forms.DockStyle.Top;
            this._panelFind.Location = new System.Drawing.Point(0, 0);
            this._panelFind.Name = "_panelFind";
            this._panelFind.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this._panelFind.Size = new System.Drawing.Size(636, 24);
            this._panelFind.TabIndex = 0;
            // 
            // _txtFind
            // 
            this._txtFind.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtFind.Location = new System.Drawing.Point(35, 0);
            this._txtFind.Name = "_txtFind";
            this._txtFind.Size = new System.Drawing.Size(601, 20);
            this._txtFind.TabIndex = 1;
            // 
            // _labelFind
            // 
            this._labelFind.Dock = System.Windows.Forms.DockStyle.Left;
            this._labelFind.Location = new System.Drawing.Point(0, 0);
            this._labelFind.Name = "_labelFind";
            this._labelFind.Size = new System.Drawing.Size(35, 20);
            this._labelFind.TabIndex = 0;
            this._labelFind.Text = "Find:";
            this._labelFind.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _listResults
            // 
            this._listResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listResults.Location = new System.Drawing.Point(0, 24);
            this._listResults.Name = "_listResults";
            this._listResults.OwnerDraw = true;
            this._listResults.Size = new System.Drawing.Size(636, 433);
            this._listResults.TabIndex = 1;
            this._listResults.UseCompatibleStateImageBehavior = false;
            // 
            // PackageSearchPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._listResults);
            this.Controls.Add(this._panelFind);
            this.Name = "PackageSearchPane";
            this.Size = new System.Drawing.Size(636, 457);
            this._panelFind.ResumeLayout(false);
            this._panelFind.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelFind;
        private System.Windows.Forms.TextBox _txtFind;
        private System.Windows.Forms.Label _labelFind;
        private System.Windows.Forms.ListView _listResults;
    }
}

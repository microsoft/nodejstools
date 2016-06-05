namespace Microsoft.NodejsTools.Options {
    partial class SalsaLsIntellisenseOptionsControl {
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolTip toolTip;
            this.outerLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.typingsAcquisitionLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._enableAutomaticTypingsAcquisition = new System.Windows.Forms.CheckBox();
            this.typingsAcquisitionLabel = new System.Windows.Forms.Label();
            this._showTypingsInfoBar = new System.Windows.Forms.CheckBox();
            toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.outerLayoutPanel.SuspendLayout();
            this.typingsAcquisitionLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // outerLayoutPanel
            // 
            this.outerLayoutPanel.AutoSize = true;
            this.outerLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.outerLayoutPanel.ColumnCount = 1;
            this.outerLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.outerLayoutPanel.Controls.Add(this.typingsAcquisitionLayoutPanel, 0, 5);
            this.outerLayoutPanel.Controls.Add(this.typingsAcquisitionLabel, 0, 4);
            this.outerLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.outerLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.outerLayoutPanel.Name = "outerLayoutPanel";
            this.outerLayoutPanel.RowCount = 6;
            this.outerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.outerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.outerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.outerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.outerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.outerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.outerLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.outerLayoutPanel.Size = new System.Drawing.Size(842, 111);
            this.outerLayoutPanel.TabIndex = 6;
            // 
            // typingsAcquisitionLayoutPanel
            // 
            this.typingsAcquisitionLayoutPanel.AutoSize = true;
            this.typingsAcquisitionLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.typingsAcquisitionLayoutPanel.ColumnCount = 1;
            this.typingsAcquisitionLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.typingsAcquisitionLayoutPanel.Controls.Add(this._showTypingsInfoBar, 0, 1);
            this.typingsAcquisitionLayoutPanel.Controls.Add(this._enableAutomaticTypingsAcquisition, 0, 0);
            this.typingsAcquisitionLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typingsAcquisitionLayoutPanel.Location = new System.Drawing.Point(6, 33);
            this.typingsAcquisitionLayoutPanel.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.typingsAcquisitionLayoutPanel.Name = "typingsAcquisitionLayoutPanel";
            this.typingsAcquisitionLayoutPanel.RowCount = 2;
            this.typingsAcquisitionLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.typingsAcquisitionLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.typingsAcquisitionLayoutPanel.Size = new System.Drawing.Size(830, 70);
            this.typingsAcquisitionLayoutPanel.TabIndex = 6;
            // 
            // _enableAutomaticTypingsAcquisition
            // 
            this._enableAutomaticTypingsAcquisition.AutoSize = true;
            this._enableAutomaticTypingsAcquisition.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._enableAutomaticTypingsAcquisition.Location = new System.Drawing.Point(12, 3);
            this._enableAutomaticTypingsAcquisition.Margin = new System.Windows.Forms.Padding(12, 3, 12, 3);
            this._enableAutomaticTypingsAcquisition.Name = "_enableAutomaticTypingsAcquisition";
            this._enableAutomaticTypingsAcquisition.Size = new System.Drawing.Size(647, 29);
            this._enableAutomaticTypingsAcquisition.TabIndex = 0;
            this._enableAutomaticTypingsAcquisition.Text = "Automatically download IntelliSense typings to Node.js projects";
            this._enableAutomaticTypingsAcquisition.UseVisualStyleBackColor = true;
            // 
            // typingsAcquisitionLabel
            // 
            this.typingsAcquisitionLabel.AutoSize = true;
            this.typingsAcquisitionLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.typingsAcquisitionLabel.Location = new System.Drawing.Point(0, 0);
            this.typingsAcquisitionLabel.Margin = new System.Windows.Forms.Padding(0);
            this.typingsAcquisitionLabel.Name = "typingsAcquisitionLabel";
            this.typingsAcquisitionLabel.Size = new System.Drawing.Size(199, 25);
            this.typingsAcquisitionLabel.TabIndex = 5;
            this.typingsAcquisitionLabel.Text = "Typings Acquisition";
            // 
            // _showTypingsInfoBar
            // 
            this._showTypingsInfoBar.AutoSize = true;
            this._showTypingsInfoBar.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showTypingsInfoBar.Location = new System.Drawing.Point(12, 38);
            this._showTypingsInfoBar.Margin = new System.Windows.Forms.Padding(12, 3, 12, 3);
            this._showTypingsInfoBar.Name = "_showTypingsInfoBar";
            this._showTypingsInfoBar.Size = new System.Drawing.Size(593, 29);
            this._showTypingsInfoBar.TabIndex = 1;
            this._showTypingsInfoBar.Text = "Show status bar after adding new typings folder to project";
            this._showTypingsInfoBar.UseVisualStyleBackColor = true;
            // 
            // SalsaLsIntellisenseOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.outerLayoutPanel);
            this.Name = "SalsaLsIntellisenseOptionsControl";
            this.Size = new System.Drawing.Size(842, 482);
            this.outerLayoutPanel.ResumeLayout(false);
            this.outerLayoutPanel.PerformLayout();
            this.typingsAcquisitionLayoutPanel.ResumeLayout(false);
            this.typingsAcquisitionLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel outerLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel typingsAcquisitionLayoutPanel;
        private System.Windows.Forms.CheckBox _enableAutomaticTypingsAcquisition;
        private System.Windows.Forms.Label typingsAcquisitionLabel;
        private System.Windows.Forms.CheckBox _showTypingsInfoBar;
    }
}

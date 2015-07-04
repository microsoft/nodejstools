namespace Microsoft.NodejsTools.Options {
    partial class NodejsGeneralOptionsControl {
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
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._surveyNewsCheckLabel = new System.Windows.Forms.Label();
            this._surveyNewsCheckCombo = new System.Windows.Forms.ComboBox();
            this._topOptionsPanel = new System.Windows.Forms.Panel();
            this._checkForLongPaths = new System.Windows.Forms.CheckBox();
            this._editAndContinue = new System.Windows.Forms.CheckBox();
            this._waitOnNormalExit = new System.Windows.Forms.CheckBox();
            this._waitOnAbnormalExit = new System.Windows.Forms.CheckBox();
            this._showBrowserAndNodeLabels = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel3.SuspendLayout();
            this._topOptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this._surveyNewsCheckLabel, 0, 7);
            this.tableLayoutPanel3.Controls.Add(this._surveyNewsCheckCombo, 1, 7);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 149);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 9;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(508, 208);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // _surveyNewsCheckLabel
            // 
            this._surveyNewsCheckLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._surveyNewsCheckLabel.AutoSize = true;
            this._surveyNewsCheckLabel.Location = new System.Drawing.Point(8, 7);
            this._surveyNewsCheckLabel.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this._surveyNewsCheckLabel.Name = "_surveyNewsCheckLabel";
            this._surveyNewsCheckLabel.Size = new System.Drawing.Size(150, 17);
            this._surveyNewsCheckLabel.TabIndex = 7;
            this._surveyNewsCheckLabel.Text = "&Check for survey/news";
            this._surveyNewsCheckLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _surveyNewsCheckCombo
            // 
            this._surveyNewsCheckCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._surveyNewsCheckCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._surveyNewsCheckCombo.DropDownWidth = 172;
            this._surveyNewsCheckCombo.FormattingEnabled = true;
            this._surveyNewsCheckCombo.Items.AddRange(new object[] {
            "Never",
            "Once a day",
            "Once a week",
            "Once a month"});
            this._surveyNewsCheckCombo.Location = new System.Drawing.Point(174, 4);
            this._surveyNewsCheckCombo.Margin = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this._surveyNewsCheckCombo.Name = "_surveyNewsCheckCombo";
            this._surveyNewsCheckCombo.Size = new System.Drawing.Size(326, 24);
            this._surveyNewsCheckCombo.TabIndex = 8;
            // 
            // _topOptionsPanel
            // 
            this._topOptionsPanel.Controls.Add(this._showBrowserAndNodeLabels);
            this._topOptionsPanel.Controls.Add(this._checkForLongPaths);
            this._topOptionsPanel.Controls.Add(this._editAndContinue);
            this._topOptionsPanel.Controls.Add(this._waitOnNormalExit);
            this._topOptionsPanel.Controls.Add(this._waitOnAbnormalExit);
            this._topOptionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._topOptionsPanel.Location = new System.Drawing.Point(0, 0);
            this._topOptionsPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._topOptionsPanel.Name = "_topOptionsPanel";
            this._topOptionsPanel.Size = new System.Drawing.Size(508, 149);
            this._topOptionsPanel.TabIndex = 1;
            // 
            // _checkForLongPaths
            // 
            this._checkForLongPaths.AutoSize = true;
            this._checkForLongPaths.Location = new System.Drawing.Point(5, 90);
            this._checkForLongPaths.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._checkForLongPaths.Name = "_checkForLongPaths";
            this._checkForLongPaths.Size = new System.Drawing.Size(379, 21);
            this._checkForLongPaths.TabIndex = 4;
            this._checkForLongPaths.Text = "Check for paths that exceed the &MAX_PATH length limit";
            this._checkForLongPaths.UseVisualStyleBackColor = true;
            // 
            // _editAndContinue
            // 
            this._editAndContinue.AutoSize = true;
            this._editAndContinue.Location = new System.Drawing.Point(5, 62);
            this._editAndContinue.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._editAndContinue.Name = "_editAndContinue";
            this._editAndContinue.Size = new System.Drawing.Size(190, 21);
            this._editAndContinue.TabIndex = 3;
            this._editAndContinue.Text = "Enable &Edit and Continue";
            this._editAndContinue.UseVisualStyleBackColor = true;
            // 
            // _waitOnNormalExit
            // 
            this._waitOnNormalExit.AutoSize = true;
            this._waitOnNormalExit.Location = new System.Drawing.Point(5, 33);
            this._waitOnNormalExit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._waitOnNormalExit.Name = "_waitOnNormalExit";
            this._waitOnNormalExit.Size = new System.Drawing.Size(294, 21);
            this._waitOnNormalExit.TabIndex = 2;
            this._waitOnNormalExit.Text = "Wai&t for input when process exits normally";
            this._waitOnNormalExit.UseVisualStyleBackColor = true;
            // 
            // _waitOnAbnormalExit
            // 
            this._waitOnAbnormalExit.AutoSize = true;
            this._waitOnAbnormalExit.Location = new System.Drawing.Point(5, 5);
            this._waitOnAbnormalExit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._waitOnAbnormalExit.Name = "_waitOnAbnormalExit";
            this._waitOnAbnormalExit.Size = new System.Drawing.Size(310, 21);
            this._waitOnAbnormalExit.TabIndex = 1;
            this._waitOnAbnormalExit.Text = "&Wait for input when process exits abnormally";
            this._waitOnAbnormalExit.UseVisualStyleBackColor = true;
            // 
            // _showBrowserAndNodeLabels
            // 
            this._showBrowserAndNodeLabels.AutoSize = true;
            this._showBrowserAndNodeLabels.Location = new System.Drawing.Point(5, 119);
            this._showBrowserAndNodeLabels.Margin = new System.Windows.Forms.Padding(4);
            this._showBrowserAndNodeLabels.Name = "_showBrowserAndNodeLabels";
            this._showBrowserAndNodeLabels.Size = new System.Drawing.Size(471, 21);
            this._showBrowserAndNodeLabels.TabIndex = 6;
            this._showBrowserAndNodeLabels.Text = "Show &labels denoting browser and Node.js code in Solution Explorer";
            this._showBrowserAndNodeLabels.UseVisualStyleBackColor = true;
            // 
            // NodejsGeneralOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this._topOptionsPanel);
            this.Margin = new System.Windows.Forms.Padding(8, 10, 8, 10);
            this.Name = "NodejsGeneralOptionsControl";
            this.Size = new System.Drawing.Size(508, 357);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this._topOptionsPanel.ResumeLayout(false);
            this._topOptionsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label _surveyNewsCheckLabel;
        private System.Windows.Forms.ComboBox _surveyNewsCheckCombo;
        private System.Windows.Forms.Panel _topOptionsPanel;
        private System.Windows.Forms.CheckBox _waitOnNormalExit;
        private System.Windows.Forms.CheckBox _waitOnAbnormalExit;
        private System.Windows.Forms.CheckBox _editAndContinue;
        private System.Windows.Forms.CheckBox _checkForLongPaths;
        private System.Windows.Forms.CheckBox _showBrowserAndNodeLabels;
    }
}

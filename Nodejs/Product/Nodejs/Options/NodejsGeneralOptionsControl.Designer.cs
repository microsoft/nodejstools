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
            this._editAndContinue = new System.Windows.Forms.CheckBox();
            this._waitOnNormalExit = new System.Windows.Forms.CheckBox();
            this._waitOnAbnormalExit = new System.Windows.Forms.CheckBox();
            this._showOutputWhenRunningNpm = new System.Windows.Forms.CheckBox();
            this._checkForLongPaths = new System.Windows.Forms.CheckBox();
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
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 121);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(381, 169);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // _surveyNewsCheckLabel
            // 
            this._surveyNewsCheckLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._surveyNewsCheckLabel.AutoSize = true;
            this._surveyNewsCheckLabel.Location = new System.Drawing.Point(6, 7);
            this._surveyNewsCheckLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this._surveyNewsCheckLabel.Name = "_surveyNewsCheckLabel";
            this._surveyNewsCheckLabel.Size = new System.Drawing.Size(117, 13);
            this._surveyNewsCheckLabel.TabIndex = 6;
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
            this._surveyNewsCheckCombo.Location = new System.Drawing.Point(135, 3);
            this._surveyNewsCheckCombo.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this._surveyNewsCheckCombo.Name = "_surveyNewsCheckCombo";
            this._surveyNewsCheckCombo.Size = new System.Drawing.Size(240, 21);
            this._surveyNewsCheckCombo.TabIndex = 7;
            this._surveyNewsCheckCombo.SelectedIndexChanged += new System.EventHandler(this._surveyNewsCheckCombo_SelectedIndexChanged);
            // 
            // _topOptionsPanel
            // 
            this._topOptionsPanel.Controls.Add(this._checkForLongPaths);
            this._topOptionsPanel.Controls.Add(this._editAndContinue);
            this._topOptionsPanel.Controls.Add(this._waitOnNormalExit);
            this._topOptionsPanel.Controls.Add(this._waitOnAbnormalExit);
            this._topOptionsPanel.Controls.Add(this._showOutputWhenRunningNpm);
            this._topOptionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._topOptionsPanel.Location = new System.Drawing.Point(0, 0);
            this._topOptionsPanel.Name = "_topOptionsPanel";
            this._topOptionsPanel.Size = new System.Drawing.Size(381, 121);
            this._topOptionsPanel.TabIndex = 1;
            // 
            // _editAndContinue
            // 
            this._editAndContinue.AutoSize = true;
            this._editAndContinue.Location = new System.Drawing.Point(4, 73);
            this._editAndContinue.Name = "_editAndContinue";
            this._editAndContinue.Size = new System.Drawing.Size(146, 17);
            this._editAndContinue.TabIndex = 3;
            this._editAndContinue.Text = "Enable &Edit and Continue";
            this._editAndContinue.UseVisualStyleBackColor = true;
            this._editAndContinue.CheckedChanged += new System.EventHandler(this._editAndContinue_CheckedChanged);
            // 
            // _waitOnNormalExit
            // 
            this._waitOnNormalExit.AutoSize = true;
            this._waitOnNormalExit.Location = new System.Drawing.Point(4, 50);
            this._waitOnNormalExit.Name = "_waitOnNormalExit";
            this._waitOnNormalExit.Size = new System.Drawing.Size(223, 17);
            this._waitOnNormalExit.TabIndex = 2;
            this._waitOnNormalExit.Text = "Wai&t for input when process exits normally";
            this._waitOnNormalExit.UseVisualStyleBackColor = true;
            this._waitOnNormalExit.CheckedChanged += new System.EventHandler(this._waitOnNormalExit_CheckedChanged);
            // 
            // _waitOnAbnormalExit
            // 
            this._waitOnAbnormalExit.AutoSize = true;
            this._waitOnAbnormalExit.Location = new System.Drawing.Point(4, 27);
            this._waitOnAbnormalExit.Name = "_waitOnAbnormalExit";
            this._waitOnAbnormalExit.Size = new System.Drawing.Size(235, 17);
            this._waitOnAbnormalExit.TabIndex = 1;
            this._waitOnAbnormalExit.Text = "&Wait for input when process exits abnormally";
            this._waitOnAbnormalExit.UseVisualStyleBackColor = true;
            this._waitOnAbnormalExit.CheckedChanged += new System.EventHandler(this._waitOnAbnormalExit_CheckedChanged);
            // 
            // _showOutputWhenRunningNpm
            // 
            this._showOutputWhenRunningNpm.AutoSize = true;
            this._showOutputWhenRunningNpm.Location = new System.Drawing.Point(4, 4);
            this._showOutputWhenRunningNpm.Name = "_showOutputWhenRunningNpm";
            this._showOutputWhenRunningNpm.Size = new System.Drawing.Size(228, 17);
            this._showOutputWhenRunningNpm.TabIndex = 0;
            this._showOutputWhenRunningNpm.Text = "Show Output window when executing &npm";
            this._showOutputWhenRunningNpm.UseVisualStyleBackColor = true;
            this._showOutputWhenRunningNpm.CheckedChanged += new System.EventHandler(this._showOutputWhenRunningNpm_CheckedChanged);
            // 
            // _checkForLongPaths
            // 
            this._checkForLongPaths.AutoSize = true;
            this._checkForLongPaths.Location = new System.Drawing.Point(4, 96);
            this._checkForLongPaths.Name = "_checkForLongPaths";
            this._checkForLongPaths.Size = new System.Drawing.Size(291, 17);
            this._checkForLongPaths.TabIndex = 4;
            this._checkForLongPaths.Text = "Check for paths that exceed the &MAX_PATH length limit";
            this._checkForLongPaths.UseVisualStyleBackColor = true;
            this._checkForLongPaths.CheckedChanged += new System.EventHandler(this._checkForLongPaths_CheckedChanged);
            // 
            // NodejsGeneralOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this._topOptionsPanel);
            this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.Name = "NodejsGeneralOptionsControl";
            this.Size = new System.Drawing.Size(381, 290);
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
        private System.Windows.Forms.CheckBox _showOutputWhenRunningNpm;
        private System.Windows.Forms.CheckBox _waitOnNormalExit;
        private System.Windows.Forms.CheckBox _waitOnAbnormalExit;
        private System.Windows.Forms.CheckBox _editAndContinue;
        private System.Windows.Forms.CheckBox _checkForLongPaths;
    }
}

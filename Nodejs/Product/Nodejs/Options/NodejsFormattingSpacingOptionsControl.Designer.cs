namespace Microsoft.NodejsTools.Options {
    partial class NodejsFormattingSpacingOptionsControl {
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
            this._controlTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this._spacingGroupBox = new System.Windows.Forms.GroupBox();
            this._nonEmptyParenthesis = new System.Windows.Forms.CheckBox();
            this._spaceAfterFunction = new System.Windows.Forms.CheckBox();
            this._spacesAfterKeywordsInControlFlow = new System.Windows.Forms.CheckBox();
            this._binaryOperators = new System.Windows.Forms.CheckBox();
            this._afterSemicolonFor = new System.Windows.Forms.CheckBox();
            this._spaceAfterCommaDelimiter = new System.Windows.Forms.CheckBox();
            this._topOptionsPanel = new System.Windows.Forms.Panel();
            this._showOutputWhenRunningNpm = new System.Windows.Forms.CheckBox();
            this._controlTableLayout.SuspendLayout();
            this._spacingGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel3
            // 
            this._controlTableLayout.AutoSize = true;
            this._controlTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._controlTableLayout.ColumnCount = 2;
            this._controlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._controlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._controlTableLayout.Controls.Add(this._spacingGroupBox, 1, 8);
            this._controlTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._controlTableLayout.Location = new System.Drawing.Point(0, 0);
            this._controlTableLayout.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._controlTableLayout.Name = "_controlTableLayout";
            this._controlTableLayout.RowCount = 9;
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._controlTableLayout.Size = new System.Drawing.Size(381, 290);
            this._controlTableLayout.TabIndex = 0;
            // 
            // groupBox1
            // 
            this._spacingGroupBox.Controls.Add(this._nonEmptyParenthesis);
            this._spacingGroupBox.Controls.Add(this._spaceAfterFunction);
            this._spacingGroupBox.Controls.Add(this._spacesAfterKeywordsInControlFlow);
            this._spacingGroupBox.Controls.Add(this._binaryOperators);
            this._spacingGroupBox.Controls.Add(this._afterSemicolonFor);
            this._spacingGroupBox.Controls.Add(this._spaceAfterCommaDelimiter);
            this._spacingGroupBox.Location = new System.Drawing.Point(3, 3);
            this._spacingGroupBox.Name = "_spacingGroupBox";
            this._spacingGroupBox.Size = new System.Drawing.Size(375, 145);
            this._spacingGroupBox.TabIndex = 0;
            this._spacingGroupBox.TabStop = false;
            this._spacingGroupBox.Text = "Spacing";
            // 
            // _nonEmptyParenthesis
            // 
            this._nonEmptyParenthesis.AutoSize = true;
            this._nonEmptyParenthesis.Location = new System.Drawing.Point(7, 120);
            this._nonEmptyParenthesis.Name = "_nonEmptyParenthesis";
            this._nonEmptyParenthesis.Size = new System.Drawing.Size(353, 17);
            this._nonEmptyParenthesis.TabIndex = 5;
            this._nonEmptyParenthesis.Text = "Insert spaces after opening and before closing non-empty &parenthesis";
            this._nonEmptyParenthesis.UseVisualStyleBackColor = true;
            this._nonEmptyParenthesis.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // _spaceAfterFunction
            // 
            this._spaceAfterFunction.AutoSize = true;
            this._spaceAfterFunction.Location = new System.Drawing.Point(7, 100);
            this._spaceAfterFunction.Name = "_spaceAfterFunction";
            this._spaceAfterFunction.Size = new System.Drawing.Size(315, 17);
            this._spaceAfterFunction.TabIndex = 4;
            this._spaceAfterFunction.Text = "Insert spaces after function keyword &for anonymous functions";
            this._spaceAfterFunction.UseVisualStyleBackColor = true;
            this._spaceAfterFunction.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // _spacesAfterKeywordsInControlFlow
            // 
            this._spacesAfterKeywordsInControlFlow.AutoSize = true;
            this._spacesAfterKeywordsInControlFlow.Location = new System.Drawing.Point(7, 80);
            this._spacesAfterKeywordsInControlFlow.Name = "_spacesAfterKeywordsInControlFlow";
            this._spacesAfterKeywordsInControlFlow.Size = new System.Drawing.Size(283, 17);
            this._spacesAfterKeywordsInControlFlow.TabIndex = 3;
            this._spacesAfterKeywordsInControlFlow.Text = "Insert spaces after &keywords in control flow statements";
            this._spacesAfterKeywordsInControlFlow.UseVisualStyleBackColor = true;
            this._spacesAfterKeywordsInControlFlow.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // _binaryOperators
            // 
            this._binaryOperators.AutoSize = true;
            this._binaryOperators.Location = new System.Drawing.Point(7, 60);
            this._binaryOperators.Name = "_binaryOperators";
            this._binaryOperators.Size = new System.Drawing.Size(245, 17);
            this._binaryOperators.TabIndex = 2;
            this._binaryOperators.Text = "Insert spaces before and after binary operators";
            this._binaryOperators.UseVisualStyleBackColor = true;
            this._binaryOperators.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // _afterSemicolonFor
            // 
            this._afterSemicolonFor.AutoSize = true;
            this._afterSemicolonFor.Location = new System.Drawing.Point(7, 40);
            this._afterSemicolonFor.Name = "_afterSemicolonFor";
            this._afterSemicolonFor.Size = new System.Drawing.Size(247, 17);
            this._afterSemicolonFor.TabIndex = 1;
            this._afterSemicolonFor.Text = "Insert spaces after &semicolon in \'for\' statements";
            this._afterSemicolonFor.UseVisualStyleBackColor = true;
            this._afterSemicolonFor.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // _afterCommaDelimiter
            // 
            this._spaceAfterCommaDelimiter.AutoSize = true;
            this._spaceAfterCommaDelimiter.Location = new System.Drawing.Point(7, 20);
            this._spaceAfterCommaDelimiter.Name = "_afterCommaDelimiter";
            this._spaceAfterCommaDelimiter.Size = new System.Drawing.Size(191, 17);
            this._spaceAfterCommaDelimiter.TabIndex = 0;
            this._spaceAfterCommaDelimiter.Text = "Insert spaces after comma &delimiter";
            this._spaceAfterCommaDelimiter.UseVisualStyleBackColor = true;
            this._spaceAfterCommaDelimiter.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // _topOptionsPanel
            // 
            this._topOptionsPanel.Location = new System.Drawing.Point(0, 0);
            this._topOptionsPanel.Name = "_topOptionsPanel";
            this._topOptionsPanel.Size = new System.Drawing.Size(200, 100);
            this._topOptionsPanel.TabIndex = 1;
            // 
            // _showOutputWhenRunningNpm
            // 
            this._showOutputWhenRunningNpm.Location = new System.Drawing.Point(0, 0);
            this._showOutputWhenRunningNpm.Name = "_showOutputWhenRunningNpm";
            this._showOutputWhenRunningNpm.Size = new System.Drawing.Size(104, 24);
            this._showOutputWhenRunningNpm.TabIndex = 0;
            // 
            // NodejsFormattingSpacingOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._controlTableLayout);
            this.Controls.Add(this._topOptionsPanel);
            this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.Name = "NodejsFormattingSpacingOptionsControl";
            this.Size = new System.Drawing.Size(381, 290);
            this._controlTableLayout.ResumeLayout(false);
            this._spacingGroupBox.ResumeLayout(false);
            this._spacingGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _controlTableLayout;
        private System.Windows.Forms.Panel _topOptionsPanel;
        private System.Windows.Forms.CheckBox _showOutputWhenRunningNpm;
        private System.Windows.Forms.GroupBox _spacingGroupBox;
        private System.Windows.Forms.CheckBox _nonEmptyParenthesis;
        private System.Windows.Forms.CheckBox _spaceAfterFunction;
        private System.Windows.Forms.CheckBox _spacesAfterKeywordsInControlFlow;
        private System.Windows.Forms.CheckBox _binaryOperators;
        private System.Windows.Forms.CheckBox _afterSemicolonFor;
        private System.Windows.Forms.CheckBox _spaceAfterCommaDelimiter;
    }
}

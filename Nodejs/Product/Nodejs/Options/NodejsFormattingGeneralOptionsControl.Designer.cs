namespace Microsoft.NodejsTools.Options {
    partial class NodejsFormattingGeneralOptionsControl {
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
            this._controlLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._automaticFormattingGroupBox = new System.Windows.Forms.GroupBox();
            this._formatOnPaste = new System.Windows.Forms.CheckBox();
            this._formatOnCloseBrace = new System.Windows.Forms.CheckBox();
            this._formatOnSemicolon = new System.Windows.Forms.CheckBox();
            this._formatOnEnter = new System.Windows.Forms.CheckBox();
            this._controlLayoutPanel.SuspendLayout();
            this._automaticFormattingGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel3
            // 
            this._controlLayoutPanel.AutoSize = true;
            this._controlLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._controlLayoutPanel.ColumnCount = 2;
            this._controlLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._controlLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._controlLayoutPanel.Controls.Add(this._automaticFormattingGroupBox, 1, 8);
            this._controlLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._controlLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._controlLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._controlLayoutPanel.Name = "_controlLayoutPanel";
            this._controlLayoutPanel.RowCount = 9;
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._controlLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this._controlLayoutPanel.Size = new System.Drawing.Size(381, 290);
            this._controlLayoutPanel.TabIndex = 1;
            // 
            // groupBox1
            // 
            this._automaticFormattingGroupBox.Controls.Add(this._formatOnPaste);
            this._automaticFormattingGroupBox.Controls.Add(this._formatOnCloseBrace);
            this._automaticFormattingGroupBox.Controls.Add(this._formatOnSemicolon);
            this._automaticFormattingGroupBox.Controls.Add(this._formatOnEnter);
            this._automaticFormattingGroupBox.Location = new System.Drawing.Point(3, 3);
            this._automaticFormattingGroupBox.Name = "_automaticFormattingGroupBox";
            this._automaticFormattingGroupBox.Size = new System.Drawing.Size(375, 107);
            this._automaticFormattingGroupBox.TabIndex = 0;
            this._automaticFormattingGroupBox.TabStop = false;
            this._automaticFormattingGroupBox.Text = "Automatic Formatting";
            // 
            // _formatOnPaste
            // 
            this._formatOnPaste.AutoSize = true;
            this._formatOnPaste.Location = new System.Drawing.Point(7, 80);
            this._formatOnPaste.Name = "_formatOnPaste";
            this._formatOnPaste.Size = new System.Drawing.Size(102, 17);
            this._formatOnPaste.TabIndex = 3;
            this._formatOnPaste.Text = "Format on &paste";
            this._formatOnPaste.UseVisualStyleBackColor = true;
            // 
            // _formatOnCloseBrace
            // 
            this._formatOnCloseBrace.AutoSize = true;
            this._formatOnCloseBrace.Location = new System.Drawing.Point(7, 60);
            this._formatOnCloseBrace.Name = "_formatOnCloseBrace";
            this._formatOnCloseBrace.Size = new System.Drawing.Size(161, 17);
            this._formatOnCloseBrace.TabIndex = 2;
            this._formatOnCloseBrace.Text = "Format completed &block on }";
            this._formatOnCloseBrace.UseVisualStyleBackColor = true;
            // 
            // _formatOnSemicolon
            // 
            this._formatOnSemicolon.AutoSize = true;
            this._formatOnSemicolon.Location = new System.Drawing.Point(7, 40);
            this._formatOnSemicolon.Name = "_formatOnSemicolon";
            this._formatOnSemicolon.Size = new System.Drawing.Size(180, 17);
            this._formatOnSemicolon.TabIndex = 1;
            this._formatOnSemicolon.Text = "Format completed &statement on ;";
            this._formatOnSemicolon.UseVisualStyleBackColor = true;
            // 
            // _formatOnEnter
            // 
            this._formatOnEnter.AutoSize = true;
            this._formatOnEnter.Location = new System.Drawing.Point(7, 20);
            this._formatOnEnter.Name = "_formatOnEnter";
            this._formatOnEnter.Size = new System.Drawing.Size(172, 17);
            this._formatOnEnter.TabIndex = 0;
            this._formatOnEnter.Text = "Format completed line on &Enter";
            this._formatOnEnter.UseVisualStyleBackColor = true;
            // 
            // NodejsFormattingGeneralOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._controlLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.Name = "NodejsFormattingGeneralOptionsControl";
            this.Size = new System.Drawing.Size(381, 290);
            this._controlLayoutPanel.ResumeLayout(false);
            this._automaticFormattingGroupBox.ResumeLayout(false);
            this._automaticFormattingGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _controlLayoutPanel;
        private System.Windows.Forms.GroupBox _automaticFormattingGroupBox;
        private System.Windows.Forms.CheckBox _formatOnPaste;
        private System.Windows.Forms.CheckBox _formatOnCloseBrace;
        private System.Windows.Forms.CheckBox _formatOnSemicolon;
        private System.Windows.Forms.CheckBox _formatOnEnter;
    }
}

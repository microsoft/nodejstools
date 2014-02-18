namespace Microsoft.NodejsTools.Options {
    partial class NodejsFormattingBracesOptionsControl {
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._newLineForControlBlocks = new System.Windows.Forms.CheckBox();
            this._newLineForFunctions = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.groupBox1, 1, 8);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(381, 290);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._newLineForControlBlocks);
            this.groupBox1.Controls.Add(this._newLineForFunctions);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(375, 71);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Braces";
            // 
            // _newLineForControlBlocks
            // 
            this._newLineForControlBlocks.AutoSize = true;
            this._newLineForControlBlocks.Location = new System.Drawing.Point(7, 40);
            this._newLineForControlBlocks.Name = "_newLineForControlBlocks";
            this._newLineForControlBlocks.Size = new System.Drawing.Size(251, 17);
            this._newLineForControlBlocks.TabIndex = 1;
            this._newLineForControlBlocks.Text = "Place open brace on new line for &control blocks";
            this._newLineForControlBlocks.UseVisualStyleBackColor = true;
            this._newLineForControlBlocks.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // _newLineAfterFunctions
            // 
            this._newLineForFunctions.AutoSize = true;
            this._newLineForFunctions.Location = new System.Drawing.Point(7, 20);
            this._newLineForFunctions.Name = "_newLineForFunctions";
            this._newLineForFunctions.Size = new System.Drawing.Size(237, 17);
            this._newLineForFunctions.TabIndex = 0;
            this._newLineForFunctions.Text = "Place open brace on new line after &functions";
            this._newLineForFunctions.UseVisualStyleBackColor = true;
            this._newLineForFunctions.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // NodejsFormattingBracesOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel3);
            this.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.Name = "NodejsFormattingBracesOptionsControl";
            this.Size = new System.Drawing.Size(381, 290);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox _newLineForControlBlocks;
        private System.Windows.Forms.CheckBox _newLineForFunctions;

    }
}

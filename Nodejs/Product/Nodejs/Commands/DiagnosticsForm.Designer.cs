namespace Microsoft.NodejsTools.Commands {
    partial class DiagnosticsForm {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticsForm));
            this._textBox = new System.Windows.Forms.TextBox();
            this._ok = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._copy = new System.Windows.Forms.Button();
            this._diagnosticLoggingCheckbox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _textBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this._textBox, 3);
            resources.ApplyResources(this._textBox, "_textBox");
            this._textBox.Name = "_textBox";
            this._textBox.ReadOnly = true;
            // 
            // _ok
            // 
            resources.ApplyResources(this._ok, "_ok");
            this._ok.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._ok.Name = "_ok";
            this._ok.UseVisualStyleBackColor = true;
            this._ok.Click += new System.EventHandler(this._ok_Click);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this._textBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._ok, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this._copy, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this._diagnosticLoggingCheckbox, 0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // _copy
            // 
            resources.ApplyResources(this._copy, "_copy");
            this._copy.Name = "_copy";
            this._copy.UseVisualStyleBackColor = true;
            this._copy.Click += new System.EventHandler(this._copy_Click);
            // 
            // _diagnosticLoggingCheckbox
            // 
            resources.ApplyResources(this._diagnosticLoggingCheckbox, "_diagnosticLoggingCheckbox");
            this._diagnosticLoggingCheckbox.Name = "_diagnosticLoggingCheckbox";
            this._diagnosticLoggingCheckbox.UseVisualStyleBackColor = true;
            this._diagnosticLoggingCheckbox.CheckedChanged += new System.EventHandler(this._diagnosticLoggingCheckbox_CheckedChanged);
            // 
            // DiagnosticsForm
            // 
            this.AcceptButton = this._ok;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._ok;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DiagnosticsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _textBox;
        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button _copy;
        private System.Windows.Forms.CheckBox _diagnosticLoggingCheckbox;
    }
}
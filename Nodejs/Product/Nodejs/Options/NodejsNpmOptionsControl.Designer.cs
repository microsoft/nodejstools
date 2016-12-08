namespace Microsoft.NodejsTools.Options {
    partial class NodejsNpmOptionsControl {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodejsNpmOptionsControl));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._showOutputWhenRunningNpm = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ClearCacheButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this._cacheClearedSuccessfully = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this._showOutputWhenRunningNpm, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // _showOutputWhenRunningNpm
            // 
            resources.ApplyResources(this._showOutputWhenRunningNpm, "_showOutputWhenRunningNpm");
            this._showOutputWhenRunningNpm.Name = "_showOutputWhenRunningNpm";
            this._showOutputWhenRunningNpm.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.ClearCacheButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._cacheClearedSuccessfully, 2, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // ClearCacheButton
            // 
            resources.ApplyResources(this.ClearCacheButton, "ClearCacheButton");
            this.ClearCacheButton.Name = "ClearCacheButton";
            this.ClearCacheButton.UseVisualStyleBackColor = true;
            this.ClearCacheButton.Click += new System.EventHandler(this.ClearCacheButton_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // _cacheClearedSuccessfully
            // 
            resources.ApplyResources(this._cacheClearedSuccessfully, "_cacheClearedSuccessfully");
            this._cacheClearedSuccessfully.Name = "_cacheClearedSuccessfully";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // NodejsNpmOptionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "NodejsNpmOptionsControl";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button ClearCacheButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox _showOutputWhenRunningNpm;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label _cacheClearedSuccessfully;

    }
}

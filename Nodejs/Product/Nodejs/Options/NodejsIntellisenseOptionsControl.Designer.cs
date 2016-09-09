namespace Microsoft.NodejsTools.Options {
    partial class NodejsIntellisenseOptionsControl {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodejsIntellisenseOptionsControl));
            this.outerLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.advancedOptionsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._salsaLsIntellisenseOptionsControl = new Microsoft.NodejsTools.Options.SalsaLsIntellisenseOptionsControl();
            toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.outerLayoutPanel.SuspendLayout();
            this.advancedOptionsLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // outerLayoutPanel
            // 
            resources.ApplyResources(this.outerLayoutPanel, "outerLayoutPanel");
            this.outerLayoutPanel.Controls.Add(this.advancedOptionsLayoutPanel, 0, 2);
            this.outerLayoutPanel.Name = "outerLayoutPanel";
            // 
            // advancedOptionsLayoutPanel
            // 
            resources.ApplyResources(this.advancedOptionsLayoutPanel, "advancedOptionsLayoutPanel");
            this.advancedOptionsLayoutPanel.Controls.Add(this._salsaLsIntellisenseOptionsControl, 0, 0);
            this.advancedOptionsLayoutPanel.Name = "advancedOptionsLayoutPanel";
            // 
            // _salsaLsIntellisenseOptionsControl
            // 
            resources.ApplyResources(this._salsaLsIntellisenseOptionsControl, "_salsaLsIntellisenseOptionsControl");
            this._salsaLsIntellisenseOptionsControl.Name = "_salsaLsIntellisenseOptionsControl";
            // 
            // NodejsIntellisenseOptionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.outerLayoutPanel);
            this.Name = "NodejsIntellisenseOptionsControl";
            this.outerLayoutPanel.ResumeLayout(false);
            this.outerLayoutPanel.PerformLayout();
            this.advancedOptionsLayoutPanel.ResumeLayout(false);
            this.advancedOptionsLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel outerLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel advancedOptionsLayoutPanel;
        private SalsaLsIntellisenseOptionsControl _salsaLsIntellisenseOptionsControl;
    }
}

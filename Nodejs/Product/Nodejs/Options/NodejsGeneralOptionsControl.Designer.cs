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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodejsGeneralOptionsControl));
            this._topOptionsPanel = new System.Windows.Forms.Panel();
            this._checkForLongPaths = new System.Windows.Forms.CheckBox();
            this._editAndContinue = new System.Windows.Forms.CheckBox();
            this._waitOnNormalExit = new System.Windows.Forms.CheckBox();
            this._waitOnAbnormalExit = new System.Windows.Forms.CheckBox();
            this._topOptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _topOptionsPanel
            // 
            this._topOptionsPanel.Controls.Add(this._checkForLongPaths);
            this._topOptionsPanel.Controls.Add(this._editAndContinue);
            this._topOptionsPanel.Controls.Add(this._waitOnNormalExit);
            this._topOptionsPanel.Controls.Add(this._waitOnAbnormalExit);
            resources.ApplyResources(this._topOptionsPanel, "_topOptionsPanel");
            this._topOptionsPanel.Name = "_topOptionsPanel";
            // 
            // _checkForLongPaths
            // 
            resources.ApplyResources(this._checkForLongPaths, "_checkForLongPaths");
            this._checkForLongPaths.Name = "_checkForLongPaths";
            this._checkForLongPaths.UseVisualStyleBackColor = true;
            // 
            // _editAndContinue
            // 
            resources.ApplyResources(this._editAndContinue, "_editAndContinue");
            this._editAndContinue.Name = "_editAndContinue";
            this._editAndContinue.UseVisualStyleBackColor = true;
            // 
            // _waitOnNormalExit
            // 
            resources.ApplyResources(this._waitOnNormalExit, "_waitOnNormalExit");
            this._waitOnNormalExit.Name = "_waitOnNormalExit";
            this._waitOnNormalExit.UseVisualStyleBackColor = true;
            // 
            // _waitOnAbnormalExit
            // 
            resources.ApplyResources(this._waitOnAbnormalExit, "_waitOnAbnormalExit");
            this._waitOnAbnormalExit.Name = "_waitOnAbnormalExit";
            this._waitOnAbnormalExit.UseVisualStyleBackColor = true;
            // 
            // NodejsGeneralOptionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._topOptionsPanel);
            this.Name = "NodejsGeneralOptionsControl";
            this._topOptionsPanel.ResumeLayout(false);
            this._topOptionsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Panel _topOptionsPanel;
        private System.Windows.Forms.CheckBox _waitOnNormalExit;
        private System.Windows.Forms.CheckBox _waitOnAbnormalExit;
        private System.Windows.Forms.CheckBox _editAndContinue;
        private System.Windows.Forms.CheckBox _checkForLongPaths;
    }
}

namespace Microsoft.NodejsTools.Options {
    partial class NodejsAdvancedOptionsControl {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodejsAdvancedOptionsControl));
            this._codeAnalysisLevelLabel = new System.Windows.Forms.Label();
            this._codeAnalysisLevel = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // _codeAnalysisLevelLabel
            // 
            resources.ApplyResources(this._codeAnalysisLevelLabel, "_codeAnalysisLevelLabel");
            this._codeAnalysisLevelLabel.Name = "_codeAnalysisLevelLabel";
            // 
            // _codeAnalysisLevel
            // 
            this._codeAnalysisLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._codeAnalysisLevel.FormattingEnabled = true;
            this._codeAnalysisLevel.Items.AddRange(new object[] {
            resources.GetString("_codeAnalysisLevel.Items"),
            resources.GetString("_codeAnalysisLevel.Items1"),
            resources.GetString("_codeAnalysisLevel.Items2")});
            resources.ApplyResources(this._codeAnalysisLevel, "_codeAnalysisLevel");
            this._codeAnalysisLevel.Name = "_codeAnalysisLevel";
            // 
            // NodejsAdvancedOptionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._codeAnalysisLevel);
            this.Controls.Add(this._codeAnalysisLevelLabel);
            this.Name = "NodejsAdvancedOptionsControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _codeAnalysisLevelLabel;
        private System.Windows.Forms.ComboBox _codeAnalysisLevel;
    }
}

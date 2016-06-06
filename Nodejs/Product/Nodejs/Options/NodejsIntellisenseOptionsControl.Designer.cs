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
            this._nodejsES5IntelliSenseOptionsControl = new Microsoft.NodejsTools.Options.NodeLsIntellisenseOptionsControl();
            this._salsaLsIntellisenseOptionsControl = new Microsoft.NodejsTools.Options.SalsaLsIntellisenseOptionsControl();
            this.intelliSenseModeLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._intelliSenseModeDropdown = new System.Windows.Forms.ComboBox();
            this.intelliSenseModeLabel = new System.Windows.Forms.Label();
            this._analysisPreviewFeedbackLinkLabel = new System.Windows.Forms.LinkLabel();
            this._es5DeprecatedWarning = new System.Windows.Forms.Label();
            this.horizontalDivider = new System.Windows.Forms.Label();
            toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.outerLayoutPanel.SuspendLayout();
            this.advancedOptionsLayoutPanel.SuspendLayout();
            this.intelliSenseModeLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // outerLayoutPanel
            // 
            resources.ApplyResources(this.outerLayoutPanel, "outerLayoutPanel");
            this.outerLayoutPanel.Controls.Add(this.advancedOptionsLayoutPanel, 0, 2);
            this.outerLayoutPanel.Controls.Add(this.intelliSenseModeLayoutPanel, 0, 0);
            this.outerLayoutPanel.Controls.Add(this.horizontalDivider, 0, 1);
            this.outerLayoutPanel.Name = "outerLayoutPanel";
            // 
            // advancedOptionsLayoutPanel
            // 
            resources.ApplyResources(this.advancedOptionsLayoutPanel, "advancedOptionsLayoutPanel");
            this.advancedOptionsLayoutPanel.Controls.Add(this._nodejsES5IntelliSenseOptionsControl, 0, 1);
            this.advancedOptionsLayoutPanel.Controls.Add(this._salsaLsIntellisenseOptionsControl, 0, 0);
            this.advancedOptionsLayoutPanel.Name = "advancedOptionsLayoutPanel";
            // 
            // _nodejsES5IntelliSenseOptionsControl
            // 
            resources.ApplyResources(this._nodejsES5IntelliSenseOptionsControl, "_nodejsES5IntelliSenseOptionsControl");
            this._nodejsES5IntelliSenseOptionsControl.Name = "_nodejsES5IntelliSenseOptionsControl";
            // 
            // _salsaLsIntellisenseOptionsControl
            // 
            resources.ApplyResources(this._salsaLsIntellisenseOptionsControl, "_salsaLsIntellisenseOptionsControl");
            this._salsaLsIntellisenseOptionsControl.Name = "_salsaLsIntellisenseOptionsControl";
            // 
            // intelliSenseModeLayoutPanel
            // 
            resources.ApplyResources(this.intelliSenseModeLayoutPanel, "intelliSenseModeLayoutPanel");
            this.intelliSenseModeLayoutPanel.Controls.Add(this._intelliSenseModeDropdown, 1, 0);
            this.intelliSenseModeLayoutPanel.Controls.Add(this.intelliSenseModeLabel, 0, 0);
            this.intelliSenseModeLayoutPanel.Controls.Add(this._analysisPreviewFeedbackLinkLabel, 1, 2);
            this.intelliSenseModeLayoutPanel.Controls.Add(this._es5DeprecatedWarning, 1, 1);
            this.intelliSenseModeLayoutPanel.Name = "intelliSenseModeLayoutPanel";
            // 
            // _intelliSenseModeDropdown
            // 
            this._intelliSenseModeDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._intelliSenseModeDropdown.FormattingEnabled = true;
            resources.ApplyResources(this._intelliSenseModeDropdown, "_intelliSenseModeDropdown");
            this._intelliSenseModeDropdown.Name = "_intelliSenseModeDropdown";
            this._intelliSenseModeDropdown.SelectedValueChanged += new System.EventHandler(this._intelliSenseModeDropdown_SelectedValueChanged);
            // 
            // intelliSenseModeLabel
            // 
            resources.ApplyResources(this.intelliSenseModeLabel, "intelliSenseModeLabel");
            this.intelliSenseModeLabel.Name = "intelliSenseModeLabel";
            // 
            // _analysisPreviewFeedbackLinkLabel
            // 
            resources.ApplyResources(this._analysisPreviewFeedbackLinkLabel, "_analysisPreviewFeedbackLinkLabel");
            this._analysisPreviewFeedbackLinkLabel.Name = "_analysisPreviewFeedbackLinkLabel";
            this._analysisPreviewFeedbackLinkLabel.TabStop = true;
            this._analysisPreviewFeedbackLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._analysisPreviewFeedbackLinkLabel_LinkClicked);
            // 
            // _es5DeprecatedWarning
            // 
            resources.ApplyResources(this._es5DeprecatedWarning, "_es5DeprecatedWarning");
            this._es5DeprecatedWarning.Name = "_es5DeprecatedWarning";
            // 
            // horizontalDivider
            // 
            this.horizontalDivider.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.horizontalDivider, "horizontalDivider");
            this.horizontalDivider.ForeColor = System.Drawing.SystemColors.Control;
            this.horizontalDivider.Name = "horizontalDivider";
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
            this.intelliSenseModeLayoutPanel.ResumeLayout(false);
            this.intelliSenseModeLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel outerLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel advancedOptionsLayoutPanel;
        private NodeLsIntellisenseOptionsControl _nodejsES5IntelliSenseOptionsControl;
        private SalsaLsIntellisenseOptionsControl _salsaLsIntellisenseOptionsControl;
        private System.Windows.Forms.TableLayoutPanel intelliSenseModeLayoutPanel;
        private System.Windows.Forms.Label intelliSenseModeLabel;
        private System.Windows.Forms.Label horizontalDivider;
        private System.Windows.Forms.ComboBox _intelliSenseModeDropdown;
        private System.Windows.Forms.LinkLabel _analysisPreviewFeedbackLinkLabel;
        private System.Windows.Forms.Label _es5DeprecatedWarning;
    }
}

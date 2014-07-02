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
            System.Windows.Forms.GroupBox intellisenseLevelGroupBox;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodejsIntellisenseOptionsControl));
            System.Windows.Forms.ToolTip toolTip;
            this._noIntelliSenseRadioButton = new System.Windows.Forms.RadioButton();
            this._limitedIntelliSenseRadioButton = new System.Windows.Forms.RadioButton();
            this._fullIntelliSenseRadioButton = new System.Windows.Forms.RadioButton();
            intellisenseLevelGroupBox = new System.Windows.Forms.GroupBox();
            toolTip = new System.Windows.Forms.ToolTip(this.components);
            intellisenseLevelGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // intellisenseLevelGroupBox
            // 
            resources.ApplyResources(intellisenseLevelGroupBox, "intellisenseLevelGroupBox");
            intellisenseLevelGroupBox.Controls.Add(this._noIntelliSenseRadioButton);
            intellisenseLevelGroupBox.Controls.Add(this._limitedIntelliSenseRadioButton);
            intellisenseLevelGroupBox.Controls.Add(this._fullIntelliSenseRadioButton);
            intellisenseLevelGroupBox.Name = "intellisenseLevelGroupBox";
            intellisenseLevelGroupBox.TabStop = false;
            // 
            // _noIntelliSenseRadioButton
            // 
            resources.ApplyResources(this._noIntelliSenseRadioButton, "_noIntelliSenseRadioButton");
            this._noIntelliSenseRadioButton.Name = "_noIntelliSenseRadioButton";
            this._noIntelliSenseRadioButton.TabStop = true;
            toolTip.SetToolTip(this._noIntelliSenseRadioButton, resources.GetString("_noIntelliSenseRadioButton.ToolTip"));
            this._noIntelliSenseRadioButton.UseVisualStyleBackColor = true;
            // 
            // _limitedIntelliSenseRadioButton
            // 
            resources.ApplyResources(this._limitedIntelliSenseRadioButton, "_limitedIntelliSenseRadioButton");
            this._limitedIntelliSenseRadioButton.Name = "_limitedIntelliSenseRadioButton";
            this._limitedIntelliSenseRadioButton.TabStop = true;
            toolTip.SetToolTip(this._limitedIntelliSenseRadioButton, resources.GetString("_limitedIntelliSenseRadioButton.ToolTip"));
            this._limitedIntelliSenseRadioButton.UseVisualStyleBackColor = true;
            // 
            // _fullIntelliSenseRadioButton
            // 
            resources.ApplyResources(this._fullIntelliSenseRadioButton, "_fullIntelliSenseRadioButton");
            this._fullIntelliSenseRadioButton.Name = "_fullIntelliSenseRadioButton";
            this._fullIntelliSenseRadioButton.TabStop = true;
            toolTip.SetToolTip(this._fullIntelliSenseRadioButton, resources.GetString("_fullIntelliSenseRadioButton.ToolTip"));
            this._fullIntelliSenseRadioButton.UseVisualStyleBackColor = true;
            // 
            // NodejsIntellisenseOptionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(intellisenseLevelGroupBox);
            this.Name = "NodejsIntellisenseOptionsControl";
            intellisenseLevelGroupBox.ResumeLayout(false);
            intellisenseLevelGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton _fullIntelliSenseRadioButton;
        private System.Windows.Forms.RadioButton _noIntelliSenseRadioButton;
        private System.Windows.Forms.RadioButton _limitedIntelliSenseRadioButton;

    }
}

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
            System.Windows.Forms.GroupBox intellisenseLevelGroupBox;
            System.Windows.Forms.GroupBox saveToDiskGroupBox;
            this._fullIntelliSenseRadioButton = new System.Windows.Forms.RadioButton();
            this._noIntelliSenseRadioButton = new System.Windows.Forms.RadioButton();
            this._saveToDiskDisabledRadioButton = new System.Windows.Forms.RadioButton();
            this._saveToDiskEnabledRadioButton = new System.Windows.Forms.RadioButton();
            this._mediumIntelliSenseRadioButton = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._analysisLogMax = new System.Windows.Forms.ComboBox();
            this._analysisLogMaxLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._selectionInCompletionListGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this._completionCommittedBy = new System.Windows.Forms.TextBox();
            this._completionCommittedByLabel = new System.Windows.Forms.Label();
            toolTip = new System.Windows.Forms.ToolTip(this.components);
            intellisenseLevelGroupBox = new System.Windows.Forms.GroupBox();
            saveToDiskGroupBox = new System.Windows.Forms.GroupBox();
            intellisenseLevelGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            saveToDiskGroupBox.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this._selectionInCompletionListGroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _fullIntelliSenseRadioButton
            // 
            resources.ApplyResources(this._fullIntelliSenseRadioButton, "_fullIntelliSenseRadioButton");
            this._fullIntelliSenseRadioButton.Name = "_fullIntelliSenseRadioButton";
            this._fullIntelliSenseRadioButton.TabStop = true;
            toolTip.SetToolTip(this._fullIntelliSenseRadioButton, resources.GetString("_fullIntelliSenseRadioButton.ToolTip"));
            this._fullIntelliSenseRadioButton.UseVisualStyleBackColor = true;
            // 
            // _noIntelliSenseRadioButton
            // 
            resources.ApplyResources(this._noIntelliSenseRadioButton, "_noIntelliSenseRadioButton");
            this._noIntelliSenseRadioButton.Name = "_noIntelliSenseRadioButton";
            this._noIntelliSenseRadioButton.TabStop = true;
            toolTip.SetToolTip(this._noIntelliSenseRadioButton, resources.GetString("_noIntelliSenseRadioButton.ToolTip"));
            this._noIntelliSenseRadioButton.UseVisualStyleBackColor = true;
            // 
            // _saveToDiskDisabledRadioButton
            // 
            resources.ApplyResources(this._saveToDiskDisabledRadioButton, "_saveToDiskDisabledRadioButton");
            this._saveToDiskDisabledRadioButton.Name = "_saveToDiskDisabledRadioButton";
            this._saveToDiskDisabledRadioButton.TabStop = true;
            toolTip.SetToolTip(this._saveToDiskDisabledRadioButton, resources.GetString("_saveToDiskDisabledRadioButton.ToolTip"));
            this._saveToDiskDisabledRadioButton.UseVisualStyleBackColor = true;
            // 
            // _saveToDiskEnabledRadioButton
            // 
            resources.ApplyResources(this._saveToDiskEnabledRadioButton, "_saveToDiskEnabledRadioButton");
            this._saveToDiskEnabledRadioButton.Name = "_saveToDiskEnabledRadioButton";
            this._saveToDiskEnabledRadioButton.TabStop = true;
            toolTip.SetToolTip(this._saveToDiskEnabledRadioButton, resources.GetString("_saveToDiskEnabledRadioButton.ToolTip"));
            this._saveToDiskEnabledRadioButton.UseVisualStyleBackColor = true;
            // 
            // _mediumIntelliSenseRadioButton
            // 
            resources.ApplyResources(this._mediumIntelliSenseRadioButton, "_mediumIntelliSenseRadioButton");
            this._mediumIntelliSenseRadioButton.Name = "_mediumIntelliSenseRadioButton";
            this._mediumIntelliSenseRadioButton.TabStop = true;
            toolTip.SetToolTip(this._mediumIntelliSenseRadioButton, resources.GetString("_mediumIntelliSenseRadioButton.ToolTip"));
            this._mediumIntelliSenseRadioButton.UseVisualStyleBackColor = true;
            // 
            // intellisenseLevelGroupBox
            // 
            resources.ApplyResources(intellisenseLevelGroupBox, "intellisenseLevelGroupBox");
            intellisenseLevelGroupBox.Controls.Add(this.tableLayoutPanel2);
            intellisenseLevelGroupBox.Name = "intellisenseLevelGroupBox";
            intellisenseLevelGroupBox.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this._mediumIntelliSenseRadioButton, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this._analysisLogMax, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this._noIntelliSenseRadioButton, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this._fullIntelliSenseRadioButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._analysisLogMaxLabel, 0, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // _analysisLogMax
            // 
            resources.ApplyResources(this._analysisLogMax, "_analysisLogMax");
            this._analysisLogMax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._analysisLogMax.FormattingEnabled = true;
            this._analysisLogMax.Items.AddRange(new object[] {
            resources.GetString("_analysisLogMax.Items"),
            resources.GetString("_analysisLogMax.Items1"),
            resources.GetString("_analysisLogMax.Items2"),
            resources.GetString("_analysisLogMax.Items3"),
            resources.GetString("_analysisLogMax.Items4"),
            resources.GetString("_analysisLogMax.Items5"),
            resources.GetString("_analysisLogMax.Items6")});
            this._analysisLogMax.Name = "_analysisLogMax";
            // 
            // _analysisLogMaxLabel
            // 
            resources.ApplyResources(this._analysisLogMaxLabel, "_analysisLogMaxLabel");
            this._analysisLogMaxLabel.Name = "_analysisLogMaxLabel";
            // 
            // saveToDiskGroupBox
            // 
            resources.ApplyResources(saveToDiskGroupBox, "saveToDiskGroupBox");
            saveToDiskGroupBox.Controls.Add(this.tableLayoutPanel4);
            saveToDiskGroupBox.Name = "saveToDiskGroupBox";
            saveToDiskGroupBox.TabStop = false;
            // 
            // tableLayoutPanel4
            // 
            resources.ApplyResources(this.tableLayoutPanel4, "tableLayoutPanel4");
            this.tableLayoutPanel4.Controls.Add(this._saveToDiskDisabledRadioButton, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this._saveToDiskEnabledRadioButton, 0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this._selectionInCompletionListGroupBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(saveToDiskGroupBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(intellisenseLevelGroupBox, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // _selectionInCompletionListGroupBox
            // 
            resources.ApplyResources(this._selectionInCompletionListGroupBox, "_selectionInCompletionListGroupBox");
            this._selectionInCompletionListGroupBox.Controls.Add(this.tableLayoutPanel3);
            this._selectionInCompletionListGroupBox.Name = "_selectionInCompletionListGroupBox";
            this._selectionInCompletionListGroupBox.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this._completionCommittedBy, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this._completionCommittedByLabel, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            // 
            // _completionCommittedBy
            // 
            resources.ApplyResources(this._completionCommittedBy, "_completionCommittedBy");
            this._completionCommittedBy.Name = "_completionCommittedBy";
            // 
            // _completionCommittedByLabel
            // 
            resources.ApplyResources(this._completionCommittedByLabel, "_completionCommittedByLabel");
            this._completionCommittedByLabel.Name = "_completionCommittedByLabel";
            // 
            // NodejsIntellisenseOptionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "NodejsIntellisenseOptionsControl";
            intellisenseLevelGroupBox.ResumeLayout(false);
            intellisenseLevelGroupBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            saveToDiskGroupBox.ResumeLayout(false);
            saveToDiskGroupBox.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this._selectionInCompletionListGroupBox.ResumeLayout(false);
            this._selectionInCompletionListGroupBox.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox _selectionInCompletionListGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton _noIntelliSenseRadioButton;
        private System.Windows.Forms.RadioButton _fullIntelliSenseRadioButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox _completionCommittedBy;
        private System.Windows.Forms.Label _completionCommittedByLabel;
        private System.Windows.Forms.Label _analysisLogMaxLabel;
        private System.Windows.Forms.ComboBox _analysisLogMax;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.RadioButton _saveToDiskDisabledRadioButton;
        private System.Windows.Forms.RadioButton _saveToDiskEnabledRadioButton;
        private System.Windows.Forms.RadioButton _mediumIntelliSenseRadioButton;
    }
}

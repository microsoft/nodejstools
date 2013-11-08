namespace Microsoft.NodejsTools.NpmUI
{
    partial class BusyControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._progress = new System.Windows.Forms.ProgressBar();
            this._labelMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _progress
            // 
            this._progress.Location = new System.Drawing.Point(31, 65);
            this._progress.Name = "_progress";
            this._progress.Size = new System.Drawing.Size(497, 23);
            this._progress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this._progress.TabIndex = 0;
            // 
            // _labelMessage
            // 
            this._labelMessage.AutoSize = true;
            this._labelMessage.Location = new System.Drawing.Point(197, 46);
            this._labelMessage.Name = "_labelMessage";
            this._labelMessage.Size = new System.Drawing.Size(162, 13);
            this._labelMessage.TabIndex = 1;
            this._labelMessage.Text = "Hey, I\'m busy doing some work...";
            // 
            // BusyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._labelMessage);
            this.Controls.Add(this._progress);
            this.Name = "BusyControl";
            this.Size = new System.Drawing.Size(556, 166);
            this.SizeChanged += new System.EventHandler(this.BusyControl_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar _progress;
        private System.Windows.Forms.Label _labelMessage;
    }
}

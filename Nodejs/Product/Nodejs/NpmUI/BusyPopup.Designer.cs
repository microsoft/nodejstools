/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace Microsoft.NodejsTools.NpmUI
{
    partial class BusyPopup
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._panelButtons = new System.Windows.Forms.Panel();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._textOutput = new System.Windows.Forms.RichTextBox();
            this._busyControl = new Microsoft.NodejsTools.NpmUI.BusyControl();
            this._panelOutput = new System.Windows.Forms.Panel();
            this._panelPadRight = new System.Windows.Forms.Panel();
            this._panelLeftPd = new System.Windows.Forms.Panel();
            this._panelButtons.SuspendLayout();
            this._panelOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // _panelButtons
            // 
            this._panelButtons.Controls.Add(this._btnClose);
            this._panelButtons.Controls.Add(this._btnCancel);
            this._panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panelButtons.Location = new System.Drawing.Point(0, 286);
            this._panelButtons.Name = "_panelButtons";
            this._panelButtons.Size = new System.Drawing.Size(608, 49);
            this._panelButtons.TabIndex = 1;
            // 
            // _btnClose
            // 
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(440, 14);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(75, 23);
            this._btnClose.TabIndex = 1;
            this._btnClose.Text = "Close";
            this._btnClose.UseVisualStyleBackColor = true;
            this._btnClose.Click += new System.EventHandler(this._btnClose_Click);
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCancel.Location = new System.Drawing.Point(521, 14);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 0;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            this._btnCancel.Click += new System.EventHandler(this._btnCancel_Click);
            // 
            // _textOutput
            // 
            this._textOutput.BackColor = System.Drawing.SystemColors.WindowText;
            this._textOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this._textOutput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._textOutput.ForeColor = System.Drawing.SystemColors.Window;
            this._textOutput.Location = new System.Drawing.Point(8, 0);
            this._textOutput.Name = "_textOutput";
            this._textOutput.Size = new System.Drawing.Size(592, 202);
            this._textOutput.TabIndex = 2;
            this._textOutput.Text = "";
            // 
            // _busyControl
            // 
            this._busyControl.Dock = System.Windows.Forms.DockStyle.Top;
            this._busyControl.Finished = false;
            this._busyControl.Location = new System.Drawing.Point(0, 0);
            this._busyControl.Message = "Hey, I\'m busy doing some work...";
            this._busyControl.Name = "_busyControl";
            this._busyControl.Size = new System.Drawing.Size(608, 84);
            this._busyControl.TabIndex = 0;
            // 
            // _panelOutput
            // 
            this._panelOutput.Controls.Add(this._textOutput);
            this._panelOutput.Controls.Add(this._panelPadRight);
            this._panelOutput.Controls.Add(this._panelLeftPd);
            this._panelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panelOutput.Location = new System.Drawing.Point(0, 84);
            this._panelOutput.Name = "_panelOutput";
            this._panelOutput.Size = new System.Drawing.Size(608, 202);
            this._panelOutput.TabIndex = 3;
            // 
            // _panelPadRight
            // 
            this._panelPadRight.Dock = System.Windows.Forms.DockStyle.Right;
            this._panelPadRight.Location = new System.Drawing.Point(600, 0);
            this._panelPadRight.Name = "_panelPadRight";
            this._panelPadRight.Size = new System.Drawing.Size(8, 202);
            this._panelPadRight.TabIndex = 1;
            // 
            // _panelLeftPd
            // 
            this._panelLeftPd.Dock = System.Windows.Forms.DockStyle.Left;
            this._panelLeftPd.Location = new System.Drawing.Point(0, 0);
            this._panelLeftPd.Name = "_panelLeftPd";
            this._panelLeftPd.Size = new System.Drawing.Size(8, 202);
            this._panelLeftPd.TabIndex = 0;
            // 
            // BusyPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 335);
            this.ControlBox = false;
            this.Controls.Add(this._panelOutput);
            this.Controls.Add(this._panelButtons);
            this.Controls.Add(this._busyControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "BusyPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "npm Status";
            this._panelButtons.ResumeLayout(false);
            this._panelOutput.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private BusyControl _busyControl;
        private System.Windows.Forms.Panel _panelButtons;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.RichTextBox _textOutput;
        private System.Windows.Forms.Panel _panelOutput;
        private System.Windows.Forms.Panel _panelPadRight;
        private System.Windows.Forms.Panel _panelLeftPd;
    }
}
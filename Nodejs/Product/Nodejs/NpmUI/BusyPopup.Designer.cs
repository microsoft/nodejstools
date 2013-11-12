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
            this._busyControl = new Microsoft.NodejsTools.NpmUI.BusyControl();
            this.SuspendLayout();
            // 
            // _busyControl
            // 
            this._busyControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._busyControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._busyControl.Location = new System.Drawing.Point(0, 0);
            this._busyControl.Message = "Hey, I\'m busy doing some work...";
            this._busyControl.Name = "_busyControl";
            this._busyControl.Size = new System.Drawing.Size(608, 119);
            this._busyControl.TabIndex = 0;
            // 
            // BusyPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 119);
            this.Controls.Add(this._busyControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "BusyPopup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BusyPopup";
            this.ResumeLayout(false);

        }

        #endregion

        private BusyControl _busyControl;
    }
}
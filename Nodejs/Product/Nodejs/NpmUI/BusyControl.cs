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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.NpmUI {
    public partial class BusyControl : UserControl {

        private bool _finished;

        public BusyControl() {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            ArrangeControls();
        }

        private void ArrangeControls() {
            var size = Size;
            var childSize = _labelMessage.Size;

            _labelMessage.Location = new Point(
                (size.Width - childSize.Width) / 2,
                size.Height / 2 - childSize.Height - 3);
            //  Place just above centre; progress bar will go just below centre

            childSize = _progress.Size;
            childSize.Width = Size.Width - 32;
            _progress.Size = childSize;
            _progress.Location = new Point(
                16,
                size.Height / 2 + 3);
        }

        private void BusyControl_SizeChanged(object sender, EventArgs e) {
            ArrangeControls();
        }

        public string Message {
            get { return _labelMessage.Text; }
            set { _labelMessage.Text = value; }
        }

        public bool Finished {
            get { return _finished; }
            set{
                _finished = value;
                if (_finished) {
                    _progress.Style = ProgressBarStyle.Continuous;
                    _progress.Value = 100;
                } else {
                    _progress.Style = ProgressBarStyle.Marquee;
                }
            }
        }
    }
}
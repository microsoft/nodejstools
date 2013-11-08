using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.NpmUI
{
    public partial class BusyControl : UserControl
    {
        public BusyControl()
        {
            InitializeComponent();
            ArrangeControls();
        }

        private void ArrangeControls()
        {
            var size = Size;
            var childSize = _labelMessage.Size;

            _labelMessage.Location = new Point(
                ( size.Width - childSize.Width ) / 2,
                size.Height / 2 - childSize.Height - 3 );   //  Place just above centre; progress bar will go just below centre

            childSize = _progress.Size;
            childSize.Width = Size.Width - 32;
            _progress.Size = childSize;
            _progress.Location = new Point(
                16,
                size.Height / 2 + 3 );
        }

        private void BusyControl_SizeChanged(object sender, EventArgs e)
        {
            ArrangeControls();
        }

        public string Message
        {
            get { return _labelMessage.Text; }
            set { _labelMessage.Text = value; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.NpmUI
{
    public partial class BusyPopup : Form
    {
        public BusyPopup()
        {
            InitializeComponent();
        }

        public string Message
        {
            get { return _busyControl.Message; }
            set { _busyControl.Message = value; }
        }

        private void Completed()
        {
            if ( InvokeRequired )
            {
                BeginInvoke( new Action( Completed ) );
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        public void ShowPopup( IWin32Window parent, Action action )
        {
            Task t = new Task( action );
            t.ContinueWith( t2 => Completed() );
            t.Start();

            ShowDialog( parent );
        }
    }
}

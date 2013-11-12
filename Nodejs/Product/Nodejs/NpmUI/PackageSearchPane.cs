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
    public partial class PackageSearchPane : UserControl
    {
        private BusyControl _busy;

        public PackageSearchPane()
        {
            InitializeComponent();

            _listResults.Hide();
            _busy = new BusyControl
            {
                Message = "Loading published package list...",
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_busy);
        }
    }
}

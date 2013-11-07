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
    internal partial class PackageSourcesPane : UserControl
    {
        public PackageSourcesPane()
        {
            InitializeComponent();
        }

        public PackageView SelectedPackageView
        {
            set
            {
                _btnInstall.Text = value == PackageView.Local ? "Install Locally" : "Install Globally";
            }
        }
    }
}

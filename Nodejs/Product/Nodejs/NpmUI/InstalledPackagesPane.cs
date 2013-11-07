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
    internal partial class InstalledPackagesPane : UserControl
    {
        public InstalledPackagesPane()
        {
            InitializeComponent();
        }

        public event EventHandler SelectedPackageViewChanged;

        private void OnSelectedPackageViewChanged()
        {
            var handlers = SelectedPackageViewChanged;
            if ( null != handlers )
            {
                handlers(this, EventArgs.Empty);
            }
        }

        public PackageView SelectedPackageView
        {
            get
            {
                return _tabCtrlInstalledPackages.SelectedIndex == 0 ? PackageView.Local : PackageView.Global;
            }
        }

        private void _tabCtrlInstalledPackages_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedPackageViewChanged();
        }
    }
}

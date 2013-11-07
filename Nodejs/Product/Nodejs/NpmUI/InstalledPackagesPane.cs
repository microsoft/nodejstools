using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

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

        public IEnumerable< IPackage > LocalPackages
        {
            set
            {
                _listLocalPackages.Packages = value;
            }
        }

        public IEnumerable< IPackage > GlobalPackages
        {
            set
            {
                _listGlobalPackages.Packages = value;
            }
        }

        private void FirePackageEvent( EventHandler< PackageEventArgs > handlers, PackageEventArgs e )
        {
            if ( null != handlers )
            {
                handlers( this, e );
            }
        }

        public event EventHandler< PackageEventArgs > UninstallLocalPackageRequested;

        private void OnUninstallLocalPackageRequested(PackageEventArgs e)
        {
            FirePackageEvent( UninstallLocalPackageRequested, e );
        }

        private void _listLocalPackages_UninstallPackageRequested(object sender, PackageEventArgs e)
        {
            OnUninstallLocalPackageRequested( e );
        }

        public event EventHandler<PackageEventArgs> UninstallGloballPackageRequested;

        private void OnUninstallGlobalPackageRequested( PackageEventArgs e )
        {
            FirePackageEvent( UninstallGloballPackageRequested, e );
        }

        private void _listGlobalPackages_UninstallPackageRequested(object sender, PackageEventArgs e)
        {
            OnUninstallGlobalPackageRequested( e );
        }
    }
}

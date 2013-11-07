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
    internal partial class PackageSourcesPane : UserControl
    {

        private PackageView _selectedPackageView;

        public PackageSourcesPane()
        {
            InitializeComponent();
        }

        private void UpdateUIState()
        {
            var view = SelectedPackageView;
            _btnInstall.Text = view == PackageView.Local ? "Install Locally" : "Install Globally";
            _labelInstallAs.Enabled = view == PackageView.Local;
            _comboDepType.Enabled = view == PackageView.Local;

            //  TODO: update state of install button based on which tab is selected and whether there is valid input
        }

        public PackageView SelectedPackageView
        {
            set
            {
                _selectedPackageView = value;
                UpdateUIState();
            }
            private get
            {
                return _selectedPackageView;
            }
        }

        public event EventHandler< PackageInstallEventArgs > InstallPackageRequested;

        private void OnInstallPackageRequested(
            string name,
            string version,
            DependencyType depType )
        {
            var handlers = InstallPackageRequested;
            if ( null != handlers )
            {
                handlers( this, new PackageInstallEventArgs( name, version, depType ) );
            }
        }

        private void _tabCtrlPackageSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }
    }
}

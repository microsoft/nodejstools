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
            _comboDepType.SelectedIndex = 0;

            if ( _tabCtrlPackageSources.SelectedIndex == 0 )
            {
                _btnInstall.Enabled = !string.IsNullOrEmpty( _paneInstallParms.PackageName ) && !string.IsNullOrEmpty( _paneInstallParms.PackageName.Trim() );
            }
            else
            {
                //  TODO: deal with selected search result
            }
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
                handlers( this, new PackageInstallEventArgs(
                    name.Trim(),
                    string.IsNullOrEmpty( version ) ? version : version.Trim(),
                    depType ) );
            }
        }

        private void _tabCtrlPackageSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void _paneInstallParms_PackageInstallParmsChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void _btnInstall_Click(object sender, EventArgs e)
        {
            DependencyType dependencyType;
            switch ( _comboDepType.SelectedIndex )
            {
                case 0:
                    dependencyType = DependencyType.Standard;
                    break;

                case 1:
                    dependencyType = DependencyType.Development;
                    break;

                default:
                    dependencyType = DependencyType.Optional;
                    break;
            }

            if ( _tabCtrlPackageSources.SelectedIndex == 0 )
            {
                OnInstallPackageRequested(
                    _paneInstallParms.PackageName,
                    _paneInstallParms.Version,
                    dependencyType);
            }
            else
            {
                //  TODO: fire request for search results
            }
        }
    }
}

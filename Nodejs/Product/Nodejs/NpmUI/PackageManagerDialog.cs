using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    public partial class PackageManagerDialog : Form
    {

        private INpmController _npmController;

        public PackageManagerDialog(INpmController controller)
        {
            InitializeComponent();

            _npmController = controller;
            _npmController.FinishedRefresh += _npmController_FinishedRefresh;

            UpdateUIState();
        }

        /// <summary>
        /// This is a bit of a do everything method that updates the state of relevant controls based
        /// on which tabs, etc., are selected.
        /// </summary>
        private void UpdateUIState()
        {
            _labelWarning.Visible = _paneInstalledPackages.SelectedPackageView == PackageView.Global;
            _labelWarningText.Visible = _paneInstalledPackages.SelectedPackageView == PackageView.Global;

            _panePackageSources.SelectedPackageView = _paneInstalledPackages.SelectedPackageView;
        }

        void _npmController_FinishedRefresh(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(_npmController_FinishedRefresh), sender, e);
            }
            else
            {
                _paneInstalledPackages.LocalPackages = _npmController.RootPackage.Modules;
                _paneInstalledPackages.GlobalPackages = _npmController.GlobalPackages;
            }
        }

        private void _paneInstalledPackages_SelectedPackageViewChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void _paneInstalledPackages_UninstallGloballPackageRequested(object sender, PackageEventArgs e)
        {
            _npmController.UninstallGlobalPackageAsync(e.Package.Name);
        }

        private void _paneInstalledPackages_UninstallLocalPackageRequested(object sender, PackageEventArgs e)
        {
            _npmController.UninstallPackageAsync( e.Package.Name );
        }

        private void _panePackageSources_InstallPackageRequested(object sender, PackageInstallEventArgs e)
        {
            if ( _paneInstalledPackages.SelectedPackageView == PackageView.Global )
            {
                _npmController.InstallGlobalPackageByVersionAsync(e.Name, e.Version);
            }
            else
            {
                _npmController.InstallPackageByVersionAsync( e.Name, e.Version, e.DependencyType );
            }
        }
    }
}

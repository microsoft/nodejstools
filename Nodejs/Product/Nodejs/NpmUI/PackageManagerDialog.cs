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
            //  TODO: reload lists of locally and globally installed packages
        }

        private void _paneInstalledPackages_SelectedPackageViewChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }
    }
}

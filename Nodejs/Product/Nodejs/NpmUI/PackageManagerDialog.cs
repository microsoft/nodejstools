using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    public partial class PackageManagerDialog : Form
    {

        private readonly INpmController _npmController;
        private bool _wait;
        private readonly object _lock = new object();

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

        private void SetWait()
        {
            lock (_lock)
            {
                _wait = true;
                Monitor.PulseAll(_lock);
            }
        }

        private void ClearWait()
        {
            lock (_lock)
            {
                _wait = false;
                Monitor.PulseAll(_lock);
            }
        }

        private void WaitForClearWait()
        {
            lock (_lock)
            {
                while (_wait)
                {
                    Monitor.Wait(_lock);
                }
            }
        }

        void _npmController_FinishedRefresh(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(_npmController_FinishedRefresh), sender, e);
            }
            else
            {
                lock (_lock)
                {
                    _paneInstalledPackages.LocalPackages = _npmController.RootPackage.Modules;
                    _paneInstalledPackages.GlobalPackages = _npmController.GlobalPackages;
                    ClearWait();
                }
            }
        }

        private void _paneInstalledPackages_SelectedPackageViewChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void _paneInstalledPackages_UninstallGloballPackageRequested(object sender, PackageEventArgs e)
        {
            using ( var popup = new BusyPopup() )
            {
                SetWait();
                popup.Message = string.Format( "Uninstalling package '{0}'...", e.Package.Name );
                popup.ShowPopup(this, () =>
                {
                    _npmController.UninstallGlobalPackageAsync(e.Package.Name);
                    WaitForClearWait();
                });
            }
        }

        private void _paneInstalledPackages_UninstallLocalPackageRequested(object sender, PackageEventArgs e)
        {
            using ( var popup = new BusyPopup() )
            {
                SetWait();
                popup.Message = string.Format( "Uninstalling package '{0}'...", e.Package.Name );
                popup.ShowPopup(this, () =>
                {
                    _npmController.UninstallPackageAsync(e.Package.Name);
                    WaitForClearWait();
                });
            }
        }

        private void _panePackageSources_InstallPackageRequested(
            object sender,
            PackageInstallEventArgs e)
        {
            using ( var popup = new BusyPopup() )
            {
                SetWait();
                popup.Message = string.Format( "Installing package '{0}'...", e.Name );
                if (_paneInstalledPackages.SelectedPackageView == PackageView.Global)
                {
                    popup.ShowPopup(
                        this,
                        () =>
                        {
                            _npmController.InstallGlobalPackageByVersionAsync(e.Name, e.Version);
                            WaitForClearWait();
                        });
                }
                else
                {
                    popup.ShowPopup(
                        this,
                        () =>
                        {
                            _npmController.InstallPackageByVersionAsync(e.Name, e.Version, e.DependencyType);
                            WaitForClearWait();
                        });
                }
            }
        }
    }
}

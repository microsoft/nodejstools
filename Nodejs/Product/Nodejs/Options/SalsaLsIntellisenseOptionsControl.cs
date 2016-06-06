using System.Windows.Forms;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Options {
    public partial class SalsaLsIntellisenseOptionsControl : UserControl {
        public SalsaLsIntellisenseOptionsControl() {
            InitializeComponent();
        }

        internal void SyncPageWithControlSettings(NodejsIntellisenseOptionsPage page) {
            page.EnableAutomaticTypingsAcquisition = _enableAutomaticTypingsAcquisition.Checked;
            page.ShowTypingsInfoBar = _showTypingsInfoBar.Checked;
            page.SaveChangesToConfigFile = _saveChangesToConfigFile.Checked;
        }

        internal void SyncControlWithPageSettings(NodejsIntellisenseOptionsPage page) {
            _enableAutomaticTypingsAcquisition.Checked = page.EnableAutomaticTypingsAcquisition;
            _showTypingsInfoBar.Checked = page.ShowTypingsInfoBar;
            _saveChangesToConfigFile.Checked = page.SaveChangesToConfigFile;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("http://aka.ms/NtvsEs6Preview");
        }
    }
}

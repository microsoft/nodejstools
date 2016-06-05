using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Options {
    public partial class SalsaLsIntellisenseOptionsControl : UserControl {
        public SalsaLsIntellisenseOptionsControl() {
            InitializeComponent();
        }

        internal void SyncPageWithControlSettings(NodejsIntellisenseOptionsPage page) {
            page.EnableAutomaticTypingsAcquisition = _enableAutomaticTypingsAcquisition.Checked;
            page.ShowTypingsInfoBar = _showTypingsInfoBar.Checked;
        }

        internal void SyncControlWithPageSettings(NodejsIntellisenseOptionsPage page) {
            _enableAutomaticTypingsAcquisition.Checked = page.EnableAutomaticTypingsAcquisition;
            _showTypingsInfoBar.Checked = page.ShowTypingsInfoBar;
        }

    }
}

using System;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.NpmUI{
    public partial class PackageInstallPane : UserControl{
        public PackageInstallPane(){
            InitializeComponent();
        }

        public event EventHandler PackageInstallParmsChanged;

        private void OnPackageInstallParmsChanged(){
            var handlers = PackageInstallParmsChanged;
            if (null != handlers){
                handlers(this, EventArgs.Empty);
            }
        }

        private void _txtPackageName_KeyUp(object sender, KeyEventArgs e){
            OnPackageInstallParmsChanged();
        }

        private void _txtVersionTag_KeyUp(object sender, KeyEventArgs e){
            OnPackageInstallParmsChanged();
        }

        public string PackageName{
            get { return _txtPackageName.Text; }
        }

        public string Version{
            get { return _txtVersionTag.Text; }
        }
    }
}
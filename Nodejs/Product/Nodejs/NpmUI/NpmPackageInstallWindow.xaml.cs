using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.NodejsTools.NpmUI {
    /// <summary>
    /// Interaction logic for NpmPackageInstallWindow.xaml
    /// </summary>
    public partial class NpmPackageInstallWindow : Window {
        internal NpmPackageInstallWindow(NpmPackageInstallViewModel vm) {
            InitializeComponent();

            DataContext = vm;
        }

        private void ExecuteClose(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }

        private void CanExecuteClose(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        public NpmOutputControl NpmExecuteControl {
            get { return this.ExecuteControl; }
        }
    }
}

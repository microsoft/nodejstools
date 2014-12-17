//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.IO;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.Profiling {
    /// <summary>
    /// Interaction logic for CompareReportsWindow.xaml
    /// </summary>
    public partial class CompareReportsWindow : DialogWindowVersioningWorkaround {
        private readonly CompareReportsView _viewModel;
        
        public CompareReportsWindow(CompareReportsView viewModel) {
            _viewModel = viewModel;

            InitializeComponent();

            DataContext = viewModel;
        }

        private void OkClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private string OpenFileDialog(string initialDir) {
            OpenFileDialog dialog = new OpenFileDialog();
            if (initialDir != null) {
                dialog.InitialDirectory = initialDir;
            }
            dialog.Filter = _viewModel.PerformanceFileFilter;
            dialog.CheckFileExists = true;
            bool res = dialog.ShowDialog() ?? false;
            if (res) {
                return dialog.FileName;
            }
            return null;
        }

        private void BaselineBrowseClick(object sender, RoutedEventArgs e) {
            var newFile = OpenFileDialog(Path.GetDirectoryName(_viewModel.BaselineFile));
            if (!string.IsNullOrEmpty(newFile)) {
                _viewModel.BaselineFile = newFile;
            }
        }

        private void CompareBrowseClick(object sender, RoutedEventArgs e) {
            // if we don't yet have a comparison file, use the baseline
            // file which we start off with all the time.
            string baseDir;
            if (!String.IsNullOrWhiteSpace(_viewModel.ComparisonFile)) {
                baseDir = _viewModel.ComparisonFile;
            } else {
                baseDir = _viewModel.BaselineFile;
            }
            baseDir = Path.GetDirectoryName(baseDir);

            var newFile = OpenFileDialog(baseDir);
            if (!string.IsNullOrEmpty(newFile)) {
                _viewModel.ComparisonFile = newFile;
            }
        }
    }
}

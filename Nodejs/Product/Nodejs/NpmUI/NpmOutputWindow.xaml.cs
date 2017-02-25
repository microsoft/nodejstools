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
using System.Windows;

namespace Microsoft.NodejsTools.NpmUI
{
    /// <summary>
    /// Interaction logic for NpmOutputControl.xaml
    /// </summary>
    public partial class NpmOutputWindow : Window
    {
        public NpmOutputWindow()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "DataContext":
                    var vm = this.DataContext as NpmOutputViewModel;
                    if (null != vm)
                    {
                        this._textBox.Document = vm.Output;
                        vm.OutputWritten += this.vm_OutputWritten;
                    }
                    break;
            }

            base.OnPropertyChanged(e);
        }

        private void vm_OutputWritten(object sender, EventArgs e)
        {
            this._textBox.ScrollToEnd();
        }

        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as NpmOutputViewModel;
            if (null != vm)
            {
                vm.Cancel();
            }
        }
    }
}

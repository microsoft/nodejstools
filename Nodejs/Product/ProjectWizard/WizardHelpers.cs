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
using System.Windows.Forms;
using EnvDTE;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.NodejsTools.ProjectWizard {
    static class WizardHelpers {
        public static IServiceProvider GetProvider(object automationObject) {
            var oleProvider = automationObject as IOleServiceProvider;
            if (oleProvider != null) {
                return new ServiceProvider(oleProvider);
            }
            MessageBox.Show(SR.GetString(SR.ErrorNoDte), SR.ProductName);
            return null;
        }

        public static DTE GetDTE(object automationObject) {
            var dte = automationObject as DTE;
            if (dte == null) {
                var provider = GetProvider(automationObject);
                if (provider != null) {
                    dte = provider.GetService(typeof(DTE)) as DTE;
                }
            }
            if (dte == null) {
                MessageBox.Show(SR.GetString(SR.ErrorNoDte), SR.ProductName);
            }
            return dte;
        }
    }
}

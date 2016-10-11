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
using System.Text;
using System.Windows;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.NpmUI
{
    internal static class ErrorHelper {
        private static Exception _lastNpmNotFoundException;

        public static string GetExceptionDetailsText(Exception e) {
            var buff = new StringBuilder();
            var current = e;
            do {
                if (buff.Length > 0) {
                    buff.Append("Caused by:\r\n");
                }
                buff.Append(current.Message);
                buff.Append("\r\n");
#if DEBUG
                buff.Append(current.GetType());
                buff.Append("\r\n");
                buff.Append(current.StackTrace);
#endif
                current = current.InnerException;
            } while (null != current);
            return buff.ToString();
        }

        private static Exception GetNpmNotFoundException(Exception source) {
            do {
                if (source is NpmNotFoundException) {
                    return source;
                }
                source = source.InnerException;
            } while (null != source);
            return null;
        }

        public static void ReportNpmNotInstalled(
            Window owner,
            Exception ex) {
            var nnfe = GetNpmNotFoundException(ex);
            if (null == nnfe) {
                nnfe = ex;
            } else {
                //  Don't want to keep bombarding user with same message - there's a real danger this popup
                //  could appear quite a lot if changes are made to the filesystem.
                bool report = (null == _lastNpmNotFoundException || ex.Message != _lastNpmNotFoundException.Message);

                _lastNpmNotFoundException = nnfe;

                if (!report) {
                    return;
                }
            }

            var message = SR.GetString(SR.NpmNotInstalledMessageText, nnfe.Message);
            if (null == owner) {
                MessageBox.Show(
                    message,
                    SR.GetString(SR.NpmNotInstalledMessageCaption),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            } else {
                MessageBox.Show(
                    owner,
                    message,
                    SR.GetString(SR.NpmNotInstalledMessageCaption),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

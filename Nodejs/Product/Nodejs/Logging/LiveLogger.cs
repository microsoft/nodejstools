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

using Microsoft.NodejsTools.Options;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Logging {

    /// <summary>
    /// An efficient logger that logs diagnostic messages using Debug.WriteLine.
    /// Additionally logs messages to the NTVS Diagnostics task pane if option is enabled.
    /// </summary>
    internal sealed class LiveLogger {
        private static Guid LiveDiagnosticLogPaneGuid = new Guid("{66386208-2E7E-4B93-A852-D1A32EE00107}");
        private const string LiveDiagnosticLogPaneName = "Node.js Tools Live Diagnostics";

        private static volatile LiveLogger _instance;
        private static object _loggerLock = new object();

        private NodejsDiagnosticsOptionsPage _diagnosticsOptions;

        private LiveLogger() {
        }

        private NodejsDiagnosticsOptionsPage DiagnosticsOptions {
            get {
                if (_diagnosticsOptions == null && NodejsPackage.Instance != null) {
                    _diagnosticsOptions = NodejsPackage.Instance.DiagnosticsOptionsPage;
                }
                return _diagnosticsOptions;
            }
        }

        private static LiveLogger Instance {
            get {
                if (_instance == null) {
                    lock (_loggerLock) {
                        if (_instance == null) {
                            _instance = new LiveLogger();
                        }
                    }
                }
                return _instance;
            }
        }

        public static void WriteLine(string message, Type category) {
            WriteLine("{0}: {1}", category.Name, message);
        }

        public static void WriteLine(string message) {
            var str = String.Format("[{0}] {1}", DateTime.UtcNow.TimeOfDay, message);
            Instance.LogMessage(str);
        }

        public static void WriteLine(string format, params object[] args) {
            var str = String.Format(format, args);
            WriteLine(str);
        }

        private void LogMessage(string message) {
            Debug.WriteLine(message);

            if (DiagnosticsOptions != null && DiagnosticsOptions.IsLiveDiagnosticsEnabled) {
                var pane = OutputWindowRedirector.Get(VisualStudio.Shell.ServiceProvider.GlobalProvider, LiveDiagnosticLogPaneGuid, LiveDiagnosticLogPaneName);
                if (pane != null) {
                    pane.WriteLine(message);
                }
            }
        }
    }
}

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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Telemetry {
    /// <summary>
    /// Chooses the best telemetry provider to use out of available ones
    /// </summary>
    public class TelemetrySetup : IPartImportsSatisfiedNotification {
        private static TelemetrySetup _instance = new TelemetrySetup();
        private ITelemetryFactory _telemetryFactory;

        [ImportMany]
        private List<Lazy<ITelemetryFactory, IOrderable>> _unOrderedTelemetryFactoryExports = null;

        private TelemetrySetup() {
        }

        private ITelemetryFactory TelemetryFactory {
            get {
                if (_telemetryFactory == null) {
                    IComponentModel componentModel = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
                    if (componentModel != null) {
                        componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
                    }
                }
                // Use dummy one if no telemetry provider was found
                return _telemetryFactory ?? (_telemetryFactory = new DummyTelemetryFactory());
            }
        }

        public void OnImportsSatisfied() {
            try {
                // Pick the top most one
                var lazyFactory = Orderer.Order(_unOrderedTelemetryFactoryExports).FirstOrDefault();
                if (lazyFactory != null) {
                    _telemetryFactory = lazyFactory.Value;
                }
            }
            catch {
                Debug.Fail("Failed to instantiate ITelemetryFactory.");
            }
        }

        public static ITelemetryLogger GetLogger() {
            return _instance.TelemetryFactory.GetLogger();
        }

        /// <summary>
        /// Set some common properties and log package loaded event
        /// </summary>
        /// <param name="logger">logger instance</param>
        /// <param name="assembly">assembly containing vs package</param>
        /// <param name="hostVersion">host version</param>
        public static void LogPackageLoad(ITelemetryLogger logger, string packageName, Assembly assembly, string hostVersion) {
            // Set common properties to be sent with all ntvs events
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            // NTVS version
            logger.SetCommonProperty(new DataPoint(TelemetryProperties.NtvsVersion, versionInfo.ProductVersion.ToString()));
            // VS version
            logger.SetCommonProperty(new DataPoint(TelemetryProperties.HostVersion, hostVersion));

            // Log package loaded event
            logger.ReportEvent(TelemetryEvents.PackageLoaded, TelemetryProperties.PackageName, packageName);
        }
    }
}

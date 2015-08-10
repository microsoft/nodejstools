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
                var lazyFActory = Orderer.Order(_unOrderedTelemetryFactoryExports).FirstOrDefault();
                if (lazyFActory != null) {
                    _telemetryFactory = lazyFActory.Value;
                }
            }
            catch {
                Debug.Fail("Failed to instantiate ITelemetryFactory.");
            }
        }

        public static ITelemetryLogger GetLogger() {
            return _instance.TelemetryFactory.GetLogger();
        }
    }
}

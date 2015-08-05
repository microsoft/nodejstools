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

using System.Diagnostics;

namespace Microsoft.NodejsTools.Telemetry {
    /// <summary>
    /// Dummy telemetry logger to be used if no other logger is found.
    /// </summary>
    public class DefaultTelemetryLogger : ITelemetryLogger {
        public void ReportEvent(string eventName) {
            Debug.WriteLine("Telemetry event posted: " + eventName);
        }

        public void ReportEvent(string eventName, string propertyName, object propertyValue, bool isPersonallyIdentifiable = false) {
            Debug.WriteLine("Telemetry event posted: " + eventName);
        }

        public void ReportEvent(string eventName, DataPointCollection properties) {
            Debug.WriteLine("Telemetry event posted: " + eventName);
        }

        public void SetCommonProperty(DataPoint property) {
            Debug.WriteLine(string.Format("Set common property '{0}' to '{1}'", property.Name, property.Value.ToString()));
        }

        public void RemoveCommonProperty(string propertyName) {
            Debug.WriteLine(string.Format("Remove common property '{0}''", propertyName));
        }
    }
}
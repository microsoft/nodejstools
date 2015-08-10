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

namespace Microsoft.NodejsTools.Telemetry {
    public interface ITelemetryLogger : IDisposable {
        /// <summary>
        /// Records event with name
        /// </summary>
        ///<param name="eventName">Event name</param>
        void ReportEvent(string eventName);

        /// <summary>
        /// Records event with single property
        /// </summary>
        /// <param name="eventName">Event Name</param>
        /// <param name="propertyName">Property Name</param>
        /// <param name="propertyValue">Property Value</param>
        /// <param name="isPersonallyIdentifiable">Whether property has personally identifiable information</param>
        void ReportEvent(string eventName, string propertyName, object propertyValue, bool isPersonallyIdentifiable = false);

        /// <summary>
        /// Records event with multiple parameters
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="properties">Event properties</param>
        void ReportEvent(string eventName, DataPointCollection properties);

        /// <summary>
        /// Set common property to be sent with all events.
        /// </summary>
        /// <param name="property">Property to set</param>
        void SetCommonProperty(DataPoint property);

        /// <summary>
        /// Remove common property if it exists.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        void RemoveCommonProperty(string propertyName);
    }
}

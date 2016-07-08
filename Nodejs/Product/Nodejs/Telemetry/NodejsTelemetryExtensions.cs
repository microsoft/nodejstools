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
    /// <summary>
    /// Extensions for logging telemetry events.
    /// </summary>
    internal static class NodejsTelemetryExtensions {
        public static void LogAnalysisActivatedForProject(this ITelemetryLogger logger, Guid projectGuid, Options.AnalysisLevel newAnalysisLevel) {
            logger.ReportEvent(
                TelemetryEvents.AnalysisActivatedForProject,
                new DataPointCollection(
                    new DataPoint(TelemetryProperties.ProjectGuid, projectGuid.ToString("B")),
                    new DataPoint(TelemetryProperties.AnalysisLevel, newAnalysisLevel.ToString())));
        }

        public static void LogAnalysisLevelChanged(this ITelemetryLogger logger, Options.AnalysisLevel newAnalysisLevel) {
            logger.ReportEvent(
                TelemetryEvents.AnalysisLevelChanged,
                TelemetryProperties.AnalysisLevel,
                newAnalysisLevel.ToString());
        }

        public static void LogProjectImported(this ITelemetryLogger logger, Guid projectGuid) {
            logger.ReportEvent(
                TelemetryEvents.ProjectImported,
                TelemetryProperties.ProjectGuid,
                projectGuid.ToString("B"));
        }

        public static void LogFileTypeInfoForProject(this ITelemetryLogger logger, Guid projectGuid, string extension, int fileCount) {
            logger.ReportEvent(
                TelemetryEvents.FileTypeInfoForProject,
                new DataPointCollection(
                    new DataPoint(TelemetryProperties.ProjectGuid, projectGuid.ToString("B")),
                    new DataPoint(TelemetryProperties.FileType, extension),
                    new DataPoint(TelemetryProperties.FileCount, fileCount)));
        }
    }
}

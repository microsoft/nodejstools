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

namespace Microsoft.NodejsTools.Logging {
    /// <summary>
    /// Main entry point for logging events.  A single instance of this logger is created
    /// by our package and can be used to dispatch log events to all installed loggers.
    /// </summary>
    class NodejsToolsLogger {
        private readonly INodejsToolsLogger[] _loggers;

        public NodejsToolsLogger(INodejsToolsLogger[] loggers) {
            _loggers = loggers;
        }

        public void LogEvent(NodejsToolsLogEvent logEvent, object data = null) {
            foreach (var logger in _loggers) {
                logger.LogEvent(logEvent, data);
            }
        }
    }
}

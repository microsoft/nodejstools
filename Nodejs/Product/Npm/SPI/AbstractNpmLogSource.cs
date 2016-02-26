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

namespace Microsoft.NodejsTools.Npm.SPI {
    internal abstract class AbstractNpmLogSource : INpmLogSource {
        public event EventHandler CommandStarted;

        protected void OnCommandStarted() {
            var handlers = CommandStarted;
            if (null != handlers) {
                handlers(this, EventArgs.Empty);
            }
        }

        protected void FireNpmLogEvent(string logText, EventHandler<NpmLogEventArgs> handlers) {
            if (null != handlers && !string.IsNullOrEmpty(logText)) {
                handlers(this, new NpmLogEventArgs(logText));
            }
        }

        public event EventHandler<NpmLogEventArgs> OutputLogged;

        protected void OnOutputLogged(string logText) {
            FireNpmLogEvent(logText, OutputLogged);
        }

        public event EventHandler<NpmLogEventArgs> ErrorLogged;

        protected void OnErrorLogged(string logText) {
            FireNpmLogEvent(logText, ErrorLogged);
        }

        public event EventHandler<NpmExceptionEventArgs> ExceptionLogged;

        protected void OnExceptionLogged(Exception e) {
            var handlers = ExceptionLogged;
            if (null != handlers) {
                handlers(this, new NpmExceptionEventArgs(e));
            }
        }

        public event EventHandler<NpmCommandCompletedEventArgs> CommandCompleted;

        protected void OnCommandCompleted(
            string arguments,
            bool withErrors,
            bool cancelled) {
            var handlers = CommandCompleted;
            if (null != handlers) {
                handlers(this, new NpmCommandCompletedEventArgs(arguments, withErrors, cancelled));
            }
        }
    }
}

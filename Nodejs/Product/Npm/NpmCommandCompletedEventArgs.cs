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

namespace Microsoft.NodejsTools.Npm {

    /// <summary>
    /// Fired when an attempt to execute an npm command is completed, whether
    /// successfully or not.
    /// </summary>
    public class NpmCommandCompletedEventArgs : EventArgs {
        public NpmCommandCompletedEventArgs(string arguments, bool withErrors, bool cancelled) {
            Arguments = arguments;
            WithErrors = withErrors;
            Cancelled = cancelled;
        }

        public string Arguments { get; private set; }

        public string CommandText {
            get { return string.IsNullOrEmpty(Arguments) ? "npm" : string.Format("npm {0}", Arguments); }
        }

        /// <summary>
        /// Indicates whether or not there were errors whilst executing npm.
        /// </summary>
        public bool WithErrors { get; private set; }

        /// <summary>
        /// Indicates whether or not the command was cancelled, with or without errors.
        /// </summary>
        public bool Cancelled { get; private set; }
    }
}

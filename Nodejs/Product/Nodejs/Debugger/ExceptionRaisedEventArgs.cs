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

namespace Microsoft.NodejsTools.Debugger {
    class ExceptionRaisedEventArgs : EventArgs {
        private readonly NodeException _exception;
        private readonly NodeThread _thread;
        private readonly bool _isUnhandled;

        public ExceptionRaisedEventArgs(NodeThread thread, NodeException exception, bool isUnhandled) {
            _thread = thread;
            _exception = exception;
            _isUnhandled = isUnhandled;
        }

        public NodeException Exception {
            get {
                return _exception;
            }
        }

        public NodeThread Thread {
            get {
                return _thread;
            }
        }

        public bool IsUnhandled {
            get {
                return _isUnhandled;
            }
        }
    }
}

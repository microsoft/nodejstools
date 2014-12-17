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
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools {
    /// <summary>
    /// Registers an exception in the Debug->Exceptions window.
    /// 
    /// Supports hierarchy registration but all elements of the hierarchy also need
    /// to be registered independently (to provide their code/state settings).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class ProvideDebugExceptionAttribute : RegistrationAttribute {
        private readonly string _engineGuid;
        private readonly string[] _path;
        private int _code;
        private enum_EXCEPTION_STATE _state;

        public ProvideDebugExceptionAttribute(string engineGuid, params string[] path) {
            _engineGuid = engineGuid;
            _path = path;
            _state = enum_EXCEPTION_STATE.EXCEPTION_JUST_MY_CODE_SUPPORTED | enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT;
        }

        public int Code {
            get {
                return _code;
            }
            set {
                _code = value;
            }
        }

        public enum_EXCEPTION_STATE State {
            get {
                return _state;
            }
            set {
                _state = value;
            }
        }

        public override void Register(RegistrationAttribute.RegistrationContext context) {
            var engineKey = context.CreateKey("AD7Metrics\\Exception\\" + _engineGuid);
            var key = engineKey;
            foreach (var pathElem in _path) {
                key = key.CreateSubkey(pathElem);
            }

            key.SetValue("Code", _code);
            key.SetValue("State", (int)_state);
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context) {
        }
    }
}

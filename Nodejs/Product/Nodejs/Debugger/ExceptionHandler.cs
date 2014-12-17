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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Debugger {
    sealed class ExceptionHandler {
        private ExceptionHitTreatment _defaultExceptionTreatment = ExceptionHitTreatment.BreakNever;
        private Dictionary<string, ExceptionHitTreatment> _exceptionTreatments;

        public ExceptionHandler() {
            _exceptionTreatments = GetDefaultExceptionTreatments();
        }

        public bool BreakOnAllExceptions {
            get {
                return _defaultExceptionTreatment != ExceptionHitTreatment.BreakNever ||
                       _exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakNever);
            }
        }

        public bool SetExceptionTreatments(ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments) {
            bool updated = false;
            foreach (var exceptionTreatment in exceptionTreatments) {
                ExceptionHitTreatment treatmentValue;
                if (!_exceptionTreatments.TryGetValue(exceptionTreatment.Key, out treatmentValue) ||
                    (exceptionTreatment.Value != treatmentValue)) {
                    _exceptionTreatments[exceptionTreatment.Key] = exceptionTreatment.Value;
                    updated = true;
                }
            }
            return updated;
        }

        public bool ClearExceptionTreatments(ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments) {
            bool updated = false;
            foreach (var exceptionTreatment in exceptionTreatments) {
                ExceptionHitTreatment treatmentValue;
                if (_exceptionTreatments.TryGetValue(exceptionTreatment.Key, out treatmentValue)) {
                    _exceptionTreatments.Remove(exceptionTreatment.Key);
                    updated = true;
                }
            }
            return updated;
        }

        public bool ResetExceptionTreatments() {
            bool updated = false;
            if (_exceptionTreatments.Values.Any(value => value != _defaultExceptionTreatment)) {
                _exceptionTreatments = GetDefaultExceptionTreatments();
                updated = true;
            }
            return updated;
        }

        public bool SetDefaultExceptionHitTreatment(ExceptionHitTreatment exceptionTreatment) {
            if (_defaultExceptionTreatment != exceptionTreatment) {
                _defaultExceptionTreatment = exceptionTreatment;
                return true;
            }
            return false;
        }

        public ExceptionHitTreatment GetExceptionHitTreatment(string exceptionName) {
            ExceptionHitTreatment exceptionTreatment;
            if (!_exceptionTreatments.TryGetValue(exceptionName, out exceptionTreatment)) {
                exceptionTreatment = _defaultExceptionTreatment;
            }
            return exceptionTreatment;
        }

        private Dictionary<string, ExceptionHitTreatment> GetDefaultExceptionTreatments() {
            // Keep exception types in sync with those declared in ProvideDebugExceptionAttribute's in NodePackage.Debugger.cs
#if FALSE
            string[] exceptionTypes = {
            };
#endif

            string[] breakNeverTypes = {
                // should probably be break on unhandled when we have just my code support
                "Error",
                "Error(EACCES)",
                "Error(EADDRINUSE)",
                "Error(EADDRNOTAVAIL)",
                "Error(EAFNOSUPPORT)",
                "Error(EAGAIN)",
                "Error(EWOULDBLOCK)",
                "Error(EALREADY)",
                "Error(EBADF)",
                "Error(EBADMSG)",
                "Error(EBUSY)",
                "Error(ECANCELED)",
                "Error(ECHILD)",
                "Error(ECONNABORTED)",
                "Error(ECONNREFUSED)",
                "Error(ECONNRESET)",
                "Error(EDEADLK)",
                "Error(EDESTADDRREQ)",
                "Error(EDOM)",
                "Error(EEXIST)",
                "Error(EFAULT)",
                "Error(EFBIG)",
                "Error(EHOSTUNREACH)",
                "Error(EIDRM)",
                "Error(EILSEQ)",
                "Error(EINPROGRESS)",
                "Error(EINTR)",
                "Error(EINVAL)",
                "Error(EIO)",
                "Error(EISCONN)",
                "Error(EISDIR)",
                "Error(ELOOP)",
                "Error(EMFILE)",
                "Error(EMLINK)",
                "Error(EMSGSIZE)",
                "Error(ENAMETOOLONG)",
                "Error(ENETDOWN)",
                "Error(ENETRESET)",
                "Error(ENETUNREACH)",
                "Error(ENFILE)",
                "Error(ENOBUFS)",
                "Error(ENODATA)",
                "Error(ENODEV)",
                "Error(ENOENT)",
                "Error(ENOEXEC)",
                "Error(ENOLINK)",
                "Error(ENOLCK)",
                "Error(ENOMEM)",
                "Error(ENOMSG)",
                "Error(ENOPROTOOPT)",
                "Error(ENOSPC)",
                "Error(ENOSR)",
                "Error(ENOSTR)",
                "Error(ENOSYS)",
                "Error(ENOTCONN)",
                "Error(ENOTDIR)",
                "Error(ENOTEMPTY)",
                "Error(ENOTSOCK)",
                "Error(ENOTSUP)",
                "Error(ENOTTY)",
                "Error(ENXIO)",
                "Error(EOVERFLOW)",
                "Error(EPERM)",
                "Error(EPIPE)",
                "Error(EPROTO)",
                "Error(EPROTONOSUPPORT)",
                "Error(EPROTOTYPE)",
                "Error(ERANGE)",
                "Error(EROFS)",
                "Error(ESPIPE)",
                "Error(ESRCH)",
                "Error(ETIME)",
                "Error(ETIMEDOUT)",
                "Error(ETXTBSY)",
                "Error(EXDEV)",
                "Error(MODULE_NOT_FOUND)",
                "Error(SIGHUP)",
                "Error(SIGINT)",
                "Error(SIGILL)",
                "Error(SIGABRT)",
                "Error(SIGFPE)",
                "Error(SIGKILL)",
                "Error(SIGSEGV)",
                "Error(SIGTERM)",
                "Error(SIGBREAK)",
                "Error(SIGWINCH)",
                "EvalError",
                "RangeError",
                "ReferenceError",
                "SyntaxError",
                "TypeError",
                "URIError",
            };

            var defaultExceptionTreatments = new Dictionary<string, ExceptionHitTreatment>();
#if FALSE
            foreach (string exceptionType in exceptionTypes) {
                defaultExceptionTreatments[exceptionType] = ExceptionHitTreatment.BreakAlways;
            }
#endif

            foreach (string exceptionType in breakNeverTypes) {
                defaultExceptionTreatments[exceptionType] = ExceptionHitTreatment.BreakNever;
            }

            return defaultExceptionTreatments;
        }
    }
}
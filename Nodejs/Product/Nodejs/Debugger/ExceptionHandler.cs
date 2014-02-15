/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Debugger {
    sealed class ExceptionHandler {
        private ExceptionHitTreatment _defaultExceptionTreatment = ExceptionHitTreatment.BreakAlways;
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
            // Keep exception types in sync with those declared in ProvideDebugExceptionAttribute's in NodePackage.cs
            string[] exceptionTypes = {
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
                "URIError"
            };

            string[] breakNeverTypes = {
                // should probably be break on unhandled when we have just my code support
                "Error(ENOENT)",
                "SyntaxError"
            };

            var defaultExceptionTreatments = new Dictionary<string, ExceptionHitTreatment>();
            foreach (string exceptionType in exceptionTypes) {
                defaultExceptionTreatments[exceptionType] = ExceptionHitTreatment.BreakAlways;
            }

            foreach (string exceptionType in breakNeverTypes) {
                defaultExceptionTreatments[exceptionType] = ExceptionHitTreatment.BreakNever;
            }

            return defaultExceptionTreatments;
        }
    }
}
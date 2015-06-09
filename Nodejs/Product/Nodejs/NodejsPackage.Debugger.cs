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

using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools {
    // Keep declared exceptions in sync with those given default values in NodeDebugger.GetDefaultExceptionTreatments()
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
#if DEV14_OR_LATER
    // VS2015's exception manager uses a different nesting structure, so it's necessary to register Error() explicitly.
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error()", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
#endif
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EACCES)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EADDRINUSE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EADDRNOTAVAIL)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EAFNOSUPPORT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EAGAIN)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EWOULDBLOCK)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EALREADY)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EBADF)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EBADMSG)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EBUSY)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECANCELED)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECHILD)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECONNABORTED)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECONNREFUSED)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ECONNRESET)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EDEADLK)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EDESTADDRREQ)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EDOM)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EEXIST)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EFAULT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EFBIG)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EHOSTUNREACH)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EIDRM)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EILSEQ)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EINPROGRESS)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EINTR)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EINVAL)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EIO)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EISCONN)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EISDIR)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ELOOP)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EMFILE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EMLINK)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EMSGSIZE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENAMETOOLONG)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENETDOWN)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENETRESET)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENETUNREACH)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENFILE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOBUFS)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENODATA)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENODEV)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOENT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOEXEC)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOLINK)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOLCK)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOMEM)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOMSG)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOPROTOOPT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSPC)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSR)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSTR)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOSYS)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTCONN)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTDIR)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTEMPTY)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTSOCK)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTSUP)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENOTTY)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ENXIO)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EOVERFLOW)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPERM)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPIPE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPROTO)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPROTONOSUPPORT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EPROTOTYPE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ERANGE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EROFS)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ESPIPE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ESRCH)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ETIME)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ETIMEDOUT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(ETXTBSY)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(EXDEV)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(MODULE_NOT_FOUND)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGHUP)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGINT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGILL)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGABRT)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGFPE)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGKILL)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGSEGV)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGTERM)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGBREAK)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "Error", "Error(SIGWINCH)", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "EvalError", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "RangeError", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "ReferenceError", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "SyntaxError", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "TypeError", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    [ProvideDebugException(AD7Engine.DebugEngineId, "Node.js Exceptions", "URIError", State = enum_EXCEPTION_STATE.EXCEPTION_NONE)]
    partial class NodejsPackage : CommonPackage {
    }
}

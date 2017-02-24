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

using System.Linq;
using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools
{
    internal sealed class ProvideNodeDebugExceptionAttribute : ProvideDebugExceptionAttribute
    {
        public readonly string ExceptionName;

        public ProvideNodeDebugExceptionAttribute(params string[] exceptionPath) : base(AD7Engine.DebugEngineId, "Node.js Exceptions", exceptionPath)
        {
            State = enum_EXCEPTION_STATE.EXCEPTION_NONE;
            ExceptionName = exceptionPath.LastOrDefault();
        }
    }

    // Keep declared exceptions in sync with those given default values in NodeDebugger.GetDefaultExceptionTreatments()
    [ProvideNodeDebugException()]
    [ProvideNodeDebugException("Error")]
    // VS2015's exception manager uses a different nesting structure, so it's necessary to register Error explicitly.
    [ProvideNodeDebugException("Error", "Error")]
    [ProvideNodeDebugException("Error", "Error(EACCES)")]
    [ProvideNodeDebugException("Error", "Error(EADDRINUSE)")]
    [ProvideNodeDebugException("Error", "Error(EADDRNOTAVAIL)")]
    [ProvideNodeDebugException("Error", "Error(EAFNOSUPPORT)")]
    [ProvideNodeDebugException("Error", "Error(EAGAIN)")]
    [ProvideNodeDebugException("Error", "Error(EWOULDBLOCK)")]
    [ProvideNodeDebugException("Error", "Error(EALREADY)")]
    [ProvideNodeDebugException("Error", "Error(EBADF)")]
    [ProvideNodeDebugException("Error", "Error(EBADMSG)")]
    [ProvideNodeDebugException("Error", "Error(EBUSY)")]
    [ProvideNodeDebugException("Error", "Error(ECANCELED)")]
    [ProvideNodeDebugException("Error", "Error(ECHILD)")]
    [ProvideNodeDebugException("Error", "Error(ECONNABORTED)")]
    [ProvideNodeDebugException("Error", "Error(ECONNREFUSED)")]
    [ProvideNodeDebugException("Error", "Error(ECONNRESET)")]
    [ProvideNodeDebugException("Error", "Error(EDEADLK)")]
    [ProvideNodeDebugException("Error", "Error(EDESTADDRREQ)")]
    [ProvideNodeDebugException("Error", "Error(EDOM)")]
    [ProvideNodeDebugException("Error", "Error(EEXIST)")]
    [ProvideNodeDebugException("Error", "Error(EFAULT)")]
    [ProvideNodeDebugException("Error", "Error(EFBIG)")]
    [ProvideNodeDebugException("Error", "Error(EHOSTUNREACH)")]
    [ProvideNodeDebugException("Error", "Error(EIDRM)")]
    [ProvideNodeDebugException("Error", "Error(EILSEQ)")]
    [ProvideNodeDebugException("Error", "Error(EINPROGRESS)")]
    [ProvideNodeDebugException("Error", "Error(EINTR)")]
    [ProvideNodeDebugException("Error", "Error(EINVAL)")]
    [ProvideNodeDebugException("Error", "Error(EIO)")]
    [ProvideNodeDebugException("Error", "Error(EISCONN)")]
    [ProvideNodeDebugException("Error", "Error(EISDIR)")]
    [ProvideNodeDebugException("Error", "Error(ELOOP)")]
    [ProvideNodeDebugException("Error", "Error(EMFILE)")]
    [ProvideNodeDebugException("Error", "Error(EMLINK)")]
    [ProvideNodeDebugException("Error", "Error(EMSGSIZE)")]
    [ProvideNodeDebugException("Error", "Error(ENAMETOOLONG)")]
    [ProvideNodeDebugException("Error", "Error(ENETDOWN)")]
    [ProvideNodeDebugException("Error", "Error(ENETRESET)")]
    [ProvideNodeDebugException("Error", "Error(ENETUNREACH)")]
    [ProvideNodeDebugException("Error", "Error(ENFILE)")]
    [ProvideNodeDebugException("Error", "Error(ENOBUFS)")]
    [ProvideNodeDebugException("Error", "Error(ENODATA)")]
    [ProvideNodeDebugException("Error", "Error(ENODEV)")]
    [ProvideNodeDebugException("Error", "Error(ENOENT)")]
    [ProvideNodeDebugException("Error", "Error(ENOEXEC)")]
    [ProvideNodeDebugException("Error", "Error(ENOLINK)")]
    [ProvideNodeDebugException("Error", "Error(ENOLCK)")]
    [ProvideNodeDebugException("Error", "Error(ENOMEM)")]
    [ProvideNodeDebugException("Error", "Error(ENOMSG)")]
    [ProvideNodeDebugException("Error", "Error(ENOPROTOOPT)")]
    [ProvideNodeDebugException("Error", "Error(ENOSPC)")]
    [ProvideNodeDebugException("Error", "Error(ENOSR)")]
    [ProvideNodeDebugException("Error", "Error(ENOSTR)")]
    [ProvideNodeDebugException("Error", "Error(ENOSYS)")]
    [ProvideNodeDebugException("Error", "Error(ENOTCONN)")]
    [ProvideNodeDebugException("Error", "Error(ENOTDIR)")]
    [ProvideNodeDebugException("Error", "Error(ENOTEMPTY)")]
    [ProvideNodeDebugException("Error", "Error(ENOTSOCK)")]
    [ProvideNodeDebugException("Error", "Error(ENOTSUP)")]
    [ProvideNodeDebugException("Error", "Error(ENOTTY)")]
    [ProvideNodeDebugException("Error", "Error(ENXIO)")]
    [ProvideNodeDebugException("Error", "Error(EOVERFLOW)")]
    [ProvideNodeDebugException("Error", "Error(EPERM)")]
    [ProvideNodeDebugException("Error", "Error(EPIPE)")]
    [ProvideNodeDebugException("Error", "Error(EPROTO)")]
    [ProvideNodeDebugException("Error", "Error(EPROTONOSUPPORT)")]
    [ProvideNodeDebugException("Error", "Error(EPROTOTYPE)")]
    [ProvideNodeDebugException("Error", "Error(ERANGE)")]
    [ProvideNodeDebugException("Error", "Error(EROFS)")]
    [ProvideNodeDebugException("Error", "Error(ESPIPE)")]
    [ProvideNodeDebugException("Error", "Error(ESRCH)")]
    [ProvideNodeDebugException("Error", "Error(ETIME)")]
    [ProvideNodeDebugException("Error", "Error(ETIMEDOUT)")]
    [ProvideNodeDebugException("Error", "Error(ETXTBSY)")]
    [ProvideNodeDebugException("Error", "Error(EXDEV)")]
    [ProvideNodeDebugException("Error", "Error(MODULE_NOT_FOUND)")]
    [ProvideNodeDebugException("Error", "Error(SIGHUP)")]
    [ProvideNodeDebugException("Error", "Error(SIGINT)")]
    [ProvideNodeDebugException("Error", "Error(SIGILL)")]
    [ProvideNodeDebugException("Error", "Error(SIGABRT)")]
    [ProvideNodeDebugException("Error", "Error(SIGFPE)")]
    [ProvideNodeDebugException("Error", "Error(SIGKILL)")]
    [ProvideNodeDebugException("Error", "Error(SIGSEGV)")]
    [ProvideNodeDebugException("Error", "Error(SIGTERM)")]
    [ProvideNodeDebugException("Error", "Error(SIGBREAK)")]
    [ProvideNodeDebugException("Error", "Error(SIGWINCH)")]
    [ProvideNodeDebugException("EvalError")]
    [ProvideNodeDebugException("RangeError")]
    [ProvideNodeDebugException("ReferenceError")]
    [ProvideNodeDebugException("SyntaxError")]
    [ProvideNodeDebugException("TypeError")]
    [ProvideNodeDebugException("URIError")]
    internal partial class NodejsPackage : CommonPackage
    {
    }
}

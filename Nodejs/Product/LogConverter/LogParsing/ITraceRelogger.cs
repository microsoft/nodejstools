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
using System.Runtime.InteropServices;

namespace NodeLogConverter.LogParsing {
    [Guid("F754AD43-3BCC-4286-8009-9C5DA214E84E")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ITraceRelogger {
        void AddLogfileTraceStream(
            [MarshalAs(UnmanagedType.BStr)]
            /* [in] */ string LogfileName,
            /* [in] */ IntPtr UserContext,
            /* [retval][out] */ out ulong TraceHandle);

        void AddRealtimeTraceStream(
            [MarshalAs(UnmanagedType.BStr)]
            /* [in] */ string LoggerName,
            /* [in] */ IntPtr UserContext,
            /* [retval][out] */ out ulong TraceHandle);

        void RegisterCallback(
            /* [in] */ ITraceEventCallback Callback);

        void Inject(

            /* [in] */ ITraceEvent Event);

        void CreateEventInstance(
            /* [in] */ ulong TraceHandle,
            /* [in] */ uint Flags,
            /* [retval][out] */ out ITraceEvent Event);

        void ProcessTrace();

        void SetOutputFilename(
            [MarshalAs(UnmanagedType.BStr)]
            /* [in] */ string LogfileName);

        void SetCompressionMode(
            [MarshalAs(UnmanagedType.Bool)]
            /* [in] */ bool CompressionMode);

        void Cancel();
    }
}

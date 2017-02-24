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

namespace NodeLogConverter.LogParsing
{
    [Guid("8CC97F40-9028-4FF3-9B62-7D1F79CA7BCB")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITraceEvent
    {
        void Clone(
            /* [retval][out] */ out ITraceEvent NewEvent);

        void GetUserContext(
            /* [retval][out] */ out IntPtr UserContext);

        void GetEventRecord(
            /* [retval][out] */ out IntPtr EventRecord);

        void SetPayload(
            /* [size_is][in] */ IntPtr Payload,
            /* [in] */ uint PayloadSize);

        void SetEventDescriptor(
            /* [in] */ IntPtr /*PCEVENT_DESCRIPTOR*/ EventDescriptor);

        void SetProcessId(
            /* [in] */ uint ProcessId);

        void SetProcessorIndex(
            /* [in] */ uint ProcessorIndex);

        void SetThreadId(
            /* [in] */ uint ThreadId);

        void SetThreadTimes(
            /* [in] */ uint KernelTime,
            /* [in] */ uint UserTime);

        void SetActivityId(
            /* [in] */ ref Guid ActivityId);

        void SetTimeStamp(
            /* [in] */ ref long TimeStamp);

        void SetProviderId(
            /* [in] */ ref Guid ProviderId);
    }
}

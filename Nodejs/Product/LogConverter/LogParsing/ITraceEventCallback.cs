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
    [Guid("3ED25501-593F-43E9-8F38-3AB46F5A4A52")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITraceEventCallback
    {
        void OnBeginProcessTrace(
            /* [in] */ ITraceEvent HeaderEvent,
            /* [in] */ ITraceRelogger Relogger);

        void OnFinalizeProcessTrace(
            /* [in] */ ITraceRelogger Relogger);

        void OnEvent( /* [in] */ ITraceEvent Event, /* [in] */ ITraceRelogger Relogger);
    }
}

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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    // This class represents the information that describes a bound breakpoint.
    internal sealed class AD7BreakpointResolution : IDebugBreakpointResolution2
    {
        private readonly AD7Engine _engine;
        private readonly NodeBreakpointBinding _binding;
        private readonly AD7DocumentContext _documentContext;

        public AD7BreakpointResolution(AD7Engine engine, NodeBreakpointBinding address, AD7DocumentContext documentContext)
        {
            _engine = engine;
            _binding = address;
            _documentContext = documentContext;
        }

        #region IDebugBreakpointResolution2 Members

        // Gets the type of the breakpoint represented by this resolution. 
        int IDebugBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBpType)
        {
            // The sample engine only supports code breakpoints.
            pBpType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }

        // Gets the breakpoint resolution information that describes this breakpoint.
        int IDebugBreakpointResolution2.GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBpResolutionInfo)
        {
            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) != 0)
            {
                // The sample engine only supports code breakpoints.
                var location = new BP_RESOLUTION_LOCATION { bpType = (uint)enum_BP_TYPE.BPT_CODE };

                // The debugger will not QI the IDebugCodeContex2 interface returned here. We must pass the pointer
                // to IDebugCodeContex2 and not IUnknown.
                var codeContext = new AD7MemoryAddress(_engine, _binding.Target.FileName, _binding.Target.Line, _binding.Target.Column);
                codeContext.SetDocumentContext(_documentContext);
                location.unionmember1 = Marshal.GetComInterfaceForObject(codeContext, typeof(IDebugCodeContext2));
                pBpResolutionInfo[0].bpResLocation = location;
                pBpResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;
            }

            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_PROGRAM) != 0)
            {
                pBpResolutionInfo[0].pProgram = _engine;
                pBpResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_PROGRAM;
            }

            return VSConstants.S_OK;
        }

        #endregion
    }
}

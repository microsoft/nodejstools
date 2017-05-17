// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
            this._engine = engine;
            this._binding = address;
            this._documentContext = documentContext;
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
                var codeContext = new AD7MemoryAddress(this._engine, this._binding.Target.FileName, this._binding.Target.Line, this._binding.Target.Column);
                codeContext.SetDocumentContext(this._documentContext);
                location.unionmember1 = Marshal.GetComInterfaceForObject(codeContext, typeof(IDebugCodeContext2));
                pBpResolutionInfo[0].bpResLocation = location;
                pBpResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;
            }

            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_PROGRAM) != 0)
            {
                pBpResolutionInfo[0].pProgram = this._engine;
                pBpResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_PROGRAM;
            }

            return VSConstants.S_OK;
        }

        #endregion
    }
}


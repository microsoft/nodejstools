// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    // This class implements IDebugProgramNode2.
    // This interface represents a program that can be debugged.
    // A debug engine (DE) or a custom port supplier implements this interface to represent a program that can be debugged. 
    internal class AD7ProgramNode : IDebugProgramNode2
    {
        private readonly int m_processId;

        public AD7ProgramNode(int processId)
        {
            this.m_processId = processId;
        }

        #region IDebugProgramNode2 Members

        // Gets the name and identifier of the DE running this program.
        int IDebugProgramNode2.GetEngineInfo(out string engineName, out Guid engineGuid)
        {
            engineName = "Node Engine";
            engineGuid = AD7Engine.DebugEngineGuid;

            return VSConstants.S_OK;
        }

        // Gets the system process identifier for the process hosting a program.
        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            // Return the process id of the debugged process
            pHostProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            pHostProcessId[0].dwProcessId = (uint)this.m_processId;

            return VSConstants.S_OK;
        }

        // Gets the name of the process hosting a program.
        int IDebugProgramNode2.GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string processName)
        {
            // Since we are using default transport and don't want to customize the process name, this method doesn't need
            // to be implemented.
            processName = null;
            return VSConstants.E_NOTIMPL;
        }

        // Gets the name of a program.
        int IDebugProgramNode2.GetProgramName(out string programName)
        {
            // Since we are using default transport and don't want to customize the process name, this method doesn't need
            // to be implemented.
            programName = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Deprecated interface methods
        // These methods are not called by the Visual Studio debugger, so they don't need to be implemented

        int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.DetachDebugger_V7()
        {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetHostMachineName_V7(out string hostMachineName)
        {
            Debug.Fail("This function is not called by the debugger");

            hostMachineName = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}

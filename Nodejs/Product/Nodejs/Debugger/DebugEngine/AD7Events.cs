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

// This file contains the various event objects that are sent to the debugger from the sample engine via IDebugEventCallback2::Event.
// These are used in EngineCallback.cs.
// The events are how the engine tells the debugger about what is happening in the debuggee process. 
// There are three base classe the other events derive from: AD7AsynchronousEvent, AD7StoppingEvent, and AD7SynchronousEvent. These 
// each implement the IDebugEvent2.GetAttributes method for the type of event they represent. 
// Most events sent the debugger are asynchronous events.

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    #region Event base classes

    internal class AD7AsynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    internal class AD7StoppingEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_ASYNC_STOP;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    internal class AD7SynchronousEvent : IDebugEvent2
    {
        public const uint Attributes = (uint)enum_EVENTATTRIBUTES.EVENT_SYNCHRONOUS;

        int IDebugEvent2.GetAttributes(out uint eventAttributes)
        {
            eventAttributes = Attributes;
            return VSConstants.S_OK;
        }
    }

    #endregion

    // The debug engine (DE) sends this interface to the session debug manager (SDM) when an instance of the DE is created.
    internal sealed class AD7EngineCreateEvent : AD7AsynchronousEvent, IDebugEngineCreateEvent2
    {
        public const string IID = "FE5B734C-759D-4E59-AB04-F103343BDD06";
        private IDebugEngine2 m_engine;

        private AD7EngineCreateEvent(AD7Engine engine)
        {
            this.m_engine = engine;
        }

        public static void Send(AD7Engine engine)
        {
            var eventObject = new AD7EngineCreateEvent(engine);
            engine.Send(eventObject, IID, null, null);
        }

        int IDebugEngineCreateEvent2.GetEngine(out IDebugEngine2 engine)
        {
            engine = this.m_engine;

            return VSConstants.S_OK;
        }
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is attached to.
    internal sealed class AD7ProgramCreateEvent : AD7AsynchronousEvent, IDebugProgramCreateEvent2
    {
        public const string IID = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";

        internal static void Send(AD7Engine engine)
        {
            var eventObject = new AD7ProgramCreateEvent();
            engine.Send(eventObject, IID, null);
        }
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is attached to.
    internal sealed class AD7ExpressionEvaluationCompleteEvent : AD7AsynchronousEvent, IDebugExpressionEvaluationCompleteEvent2
    {
        public const string IID = "C0E13A85-238A-4800-8315-D947C960A843";
        private readonly IDebugExpression2 _expression;
        private readonly IDebugProperty2 _property;

        public AD7ExpressionEvaluationCompleteEvent(IDebugExpression2 expression, IDebugProperty2 property)
        {
            this._expression = expression;
            this._property = property;
        }

        #region IDebugExpressionEvaluationCompleteEvent2 Members

        public int GetExpression(out IDebugExpression2 ppExpr)
        {
            ppExpr = this._expression;
            return VSConstants.S_OK;
        }

        public int GetResult(out IDebugProperty2 ppResult)
        {
            ppResult = this._property;
            return VSConstants.S_OK;
        }

        #endregion
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a module is loaded or unloaded.
    internal sealed class AD7ModuleLoadEvent : AD7AsynchronousEvent, IDebugModuleLoadEvent2
    {
        public const string IID = "989DB083-0D7C-40D1-A9D9-921BF611A4B2";

        private readonly AD7Module m_module;
        private readonly bool m_fLoad;

        public AD7ModuleLoadEvent(AD7Module module, bool fLoad)
        {
            this.m_module = module;
            this.m_fLoad = fLoad;
        }

        int IDebugModuleLoadEvent2.GetModule(out IDebugModule2 module, ref string debugMessage, ref int fIsLoad)
        {
            module = this.m_module;

            if (this.m_fLoad)
            {
                debugMessage = null; //String.Concat("Loaded '", m_module.DebuggedModule.Name, "'");
                fIsLoad = 1;
            }
            else
            {
                debugMessage = null; // String.Concat("Unloaded '", m_module.DebuggedModule.Name, "'");
                fIsLoad = 0;
            }

            return VSConstants.S_OK;
        }
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program has run to completion
    // or is otherwise destroyed.
    internal sealed class AD7ProgramDestroyEvent : AD7SynchronousEvent, IDebugProgramDestroyEvent2
    {
        public const string IID = "E147E9E3-6440-4073-A7B7-A65592C714B5";

        private readonly uint m_exitCode;
        public AD7ProgramDestroyEvent(uint exitCode)
        {
            this.m_exitCode = exitCode;
        }

        #region IDebugProgramDestroyEvent2 Members

        int IDebugProgramDestroyEvent2.GetExitCode(out uint exitCode)
        {
            exitCode = this.m_exitCode;

            return VSConstants.S_OK;
        }

        #endregion
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread is created in a program being debugged.
    internal sealed class AD7ThreadCreateEvent : AD7AsynchronousEvent, IDebugThreadCreateEvent2
    {
        public const string IID = "2090CCFC-70C5-491D-A5E8-BAD2DD9EE3EA";
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a thread has exited.
    internal sealed class AD7ThreadDestroyEvent : AD7AsynchronousEvent, IDebugThreadDestroyEvent2
    {
        public const string IID = "2C3B7532-A36F-4A6E-9072-49BE649B8541";

        private readonly uint m_exitCode;
        public AD7ThreadDestroyEvent(uint exitCode)
        {
            this.m_exitCode = exitCode;
        }

        #region IDebugThreadDestroyEvent2 Members

        int IDebugThreadDestroyEvent2.GetExitCode(out uint exitCode)
        {
            exitCode = this.m_exitCode;

            return VSConstants.S_OK;
        }

        #endregion
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is loaded, but before any code is executed.
    internal sealed class AD7LoadCompleteEvent : AD7StoppingEvent, IDebugLoadCompleteEvent2
    {
        public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";

        public AD7LoadCompleteEvent()
        {
        }
    }

    // This interface is sent by the debug engine (DE) to the session debug manager (SDM) when a program is loaded, but running.
    internal sealed class AD7LoadCompleteRunningEvent : AD7AsynchronousEvent, IDebugLoadCompleteEvent2
    {
        public const string IID = "B1844850-1349-45D4-9F12-495212F5EB0B";

        public AD7LoadCompleteRunningEvent()
        {
        }
    }

    // This interface tells the session debug manager (SDM) that an asynchronous break has been successfully completed.
    internal sealed class AD7AsyncBreakCompleteEvent : AD7StoppingEvent, IDebugBreakEvent2
    {
        public const string IID = "c7405d1d-e24b-44e0-b707-d8a5a4e1641b";
    }

    // This interface tells the session debug manager (SDM) that an asynchronous break has been successfully completed.
    internal sealed class AD7SteppingCompleteEvent : AD7StoppingEvent, IDebugStepCompleteEvent2
    {
        public const string IID = "0F7F24C1-74D9-4EA6-A3EA-7EDB2D81441D";
    }

    // This interface is sent when a pending breakpoint has been bound in the debuggee.
    internal sealed class AD7BreakpointBoundEvent : AD7AsynchronousEvent, IDebugBreakpointBoundEvent2
    {
        public const string IID = "1dddb704-cf99-4b8a-b746-dabb01dd13a0";

        private AD7PendingBreakpoint m_pendingBreakpoint;
        private AD7BoundBreakpoint m_boundBreakpoint;

        public AD7BreakpointBoundEvent(AD7PendingBreakpoint pendingBreakpoint, AD7BoundBreakpoint boundBreakpoint)
        {
            this.m_pendingBreakpoint = pendingBreakpoint;
            this.m_boundBreakpoint = boundBreakpoint;
        }

        #region IDebugBreakpointBoundEvent2 Members

        int IDebugBreakpointBoundEvent2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            var boundBreakpoints = new IDebugBoundBreakpoint2[1];
            boundBreakpoints[0] = this.m_boundBreakpoint;
            ppEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
            return VSConstants.S_OK;
        }

        int IDebugBreakpointBoundEvent2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = this.m_pendingBreakpoint;
            return VSConstants.S_OK;
        }

        #endregion
    }

    // This interface is sent when a bound breakpoint has been unbound in the debuggee.
    internal sealed class AD7BreakpointUnboundEvent : AD7AsynchronousEvent, IDebugBreakpointUnboundEvent2
    {
        public const string IID = "78d1db4f-c557-4dc5-a2dd-5369d21b1c8c";

        private AD7BoundBreakpoint m_boundBreakpoint;

        public AD7BreakpointUnboundEvent(AD7BoundBreakpoint boundBreakpoint)
        {
            this.m_boundBreakpoint = boundBreakpoint;
        }

        #region IDebugBreakpointUnboundEvent2 Members

        int IDebugBreakpointUnboundEvent2.GetBreakpoint(out IDebugBoundBreakpoint2 ppBP)
        {
            ppBP = this.m_boundBreakpoint;
            return VSConstants.S_OK;
        }

        int IDebugBreakpointUnboundEvent2.GetReason(enum_BP_UNBOUND_REASON[] pdwUnboundReason)
        {
            pdwUnboundReason[0] = enum_BP_UNBOUND_REASON.BPUR_BREAKPOINT_REBIND;
            return VSConstants.S_OK;
        }

        #endregion
    }

    internal sealed class AD7BreakpointErrorEvent : AD7AsynchronousEvent, IDebugBreakpointErrorEvent2, IDebugErrorBreakpoint2, IDebugErrorBreakpointResolution2
    {
        public const string IID = "abb0ca42-f82b-4622-84e4-6903ae90f210";

        private AD7Engine m_engine;
        private AD7PendingBreakpoint m_pendingBreakpoint;

        public AD7BreakpointErrorEvent(AD7PendingBreakpoint pendingBreakpoint, AD7Engine engine)
        {
            this.m_engine = engine;
            this.m_pendingBreakpoint = pendingBreakpoint;
        }

        #region IDebugBreakpointErrorEvent2 Members

        int IDebugBreakpointErrorEvent2.GetErrorBreakpoint(out IDebugErrorBreakpoint2 ppErrorBP)
        {
            ppErrorBP = this;
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugErrorBreakpoint2 Members

        int IDebugErrorBreakpoint2.GetBreakpointResolution(out IDebugErrorBreakpointResolution2 ppErrorResolution)
        {
            ppErrorResolution = this;
            return VSConstants.S_OK;
        }

        int IDebugErrorBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            ppPendingBreakpoint = this.m_pendingBreakpoint;
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugErrorBreakpoint2 Members

        int IDebugErrorBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }

        int IDebugErrorBreakpointResolution2.GetResolutionInfo(enum_BPERESI_FIELDS dwFields, BP_ERROR_RESOLUTION_INFO[] pErrorResolutionInfo)
        {
            pErrorResolutionInfo[0].dwFields = dwFields;
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_PROGRAM) != 0)
            {
                pErrorResolutionInfo[0].pProgram = (IDebugProgram2)this.m_engine;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_THREAD) != 0)
            {
                pErrorResolutionInfo[0].pThread = (IDebugThread2)this.m_engine.MainThread;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_TYPE) != 0)
            {
                pErrorResolutionInfo[0].dwType = enum_BP_ERROR_TYPE.BPET_GENERAL_WARNING;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_BPRESLOCATION) != 0)
            {
                pErrorResolutionInfo[0].bpResLocation = new BP_RESOLUTION_LOCATION();
                pErrorResolutionInfo[0].bpResLocation.bpType = (uint)enum_BP_TYPE.BPT_CODE;
            }
            if ((dwFields & enum_BPERESI_FIELDS.BPERESI_MESSAGE) != 0)
            {
                pErrorResolutionInfo[0].bstrMessage = "No code has been loaded for this code location.";
            }
            return VSConstants.S_OK;
        }

        #endregion
    }

    // This interface is sent when the entry point has been hit.
    internal sealed class AD7EntryPointEvent : AD7StoppingEvent, IDebugEntryPointEvent2
    {
        public const string IID = "e8414a3e-1642-48ec-829e-5f4040e16da9";
    }

    // This Event is sent when a breakpoint is hit in the debuggee
    internal sealed class AD7BreakpointEvent : AD7StoppingEvent, IDebugBreakpointEvent2
    {
        public const string IID = "501C1E21-C557-48B8-BA30-A1EAB0BC4A74";

        private IEnumDebugBoundBreakpoints2 m_boundBreakpoints;

        public AD7BreakpointEvent(IEnumDebugBoundBreakpoints2 boundBreakpoints)
        {
            this.m_boundBreakpoints = boundBreakpoints;
        }

        #region IDebugBreakpointEvent2 Members

        int IDebugBreakpointEvent2.EnumBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = this.m_boundBreakpoints;
            return VSConstants.S_OK;
        }

        #endregion
    }

    internal sealed class AD7DebugExceptionEvent : AD7StoppingEvent, IDebugExceptionEvent2
    {
        public const string IID = "51A94113-8788-4A54-AE15-08B74FF922D0";
        private readonly string _exception, _description;
        private readonly bool _isUnhandled;
        private AD7Engine _engine;

        public AD7DebugExceptionEvent(string typeName, string description, bool isUnhandled, AD7Engine engine)
        {
            this._exception = typeName;
            this._description = description;
            this._isUnhandled = isUnhandled;
            this._engine = engine;
        }

        #region IDebugExceptionEvent2 Members

        public int CanPassToDebuggee()
        {
            return VSConstants.S_FALSE;
        }

        public int GetException(EXCEPTION_INFO[] pExceptionInfo)
        {
            pExceptionInfo[0].pProgram = this._engine;
            pExceptionInfo[0].guidType = AD7Engine.DebugEngineGuid;
            pExceptionInfo[0].bstrExceptionName = this._exception;
            if (this._isUnhandled)
            {
                pExceptionInfo[0].dwState = enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT;
            }
            else
            {
                pExceptionInfo[0].dwState = enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE;
            }
            return VSConstants.S_OK;
        }

        public int GetExceptionDescription(out string pbstrDescription)
        {
            pbstrDescription = this._description;
            return VSConstants.S_OK;
        }

        public int PassToDebuggee(int fPass)
        {
            if (fPass != 0)
            {
                return VSConstants.S_OK;
            }
            return VSConstants.E_FAIL;
        }

        #endregion
    }

    internal sealed class AD7DebugOutputStringEvent2 : AD7AsynchronousEvent, IDebugOutputStringEvent2
    {
        public const string IID = "569C4BB1-7B82-46FC-AE28-4536DDAD753E";
        private readonly string _output;

        public AD7DebugOutputStringEvent2(string output)
        {
            this._output = output;
        }
        #region IDebugOutputStringEvent2 Members

        public int GetString(out string pbstrString)
        {
            pbstrString = this._output;
            return VSConstants.S_OK;
        }

        #endregion
    }
}

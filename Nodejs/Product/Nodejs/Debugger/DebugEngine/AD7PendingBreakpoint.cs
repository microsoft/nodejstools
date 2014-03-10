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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Debugger.DebugEngine {
    // This class represents a pending breakpoint which is an abstract representation of a breakpoint before it is bound.
    // When a user creates a new breakpoint, the pending breakpoint is created and is later bound. The bound breakpoints
    // become children of the pending breakpoint.
    class AD7PendingBreakpoint : IDebugPendingBreakpoint2 {
        // The breakpoint request that resulted in this pending breakpoint being created.
        private readonly IDebugBreakpointRequest2 _bpRequest;
        private BP_REQUEST_INFO _bpRequestInfo;
        private readonly AD7Engine _engine;
        private readonly BreakpointManager _bpManager;

        private readonly List<AD7BoundBreakpoint> _boundBreakpoints = new List<AD7BoundBreakpoint>();
        private readonly List<AD7BreakpointErrorEvent> _breakpointErrors = new List<AD7BreakpointErrorEvent>();

        private bool _enabled;
        private readonly bool _deleted;
        private string _documentName;

        public AD7PendingBreakpoint(IDebugBreakpointRequest2 pBpRequest, AD7Engine engine, BreakpointManager bpManager) {
            _bpRequest = pBpRequest;
            var requestInfo = new BP_REQUEST_INFO[1];
            EngineUtils.CheckOk(_bpRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION | enum_BPREQI_FIELDS.BPREQI_CONDITION | enum_BPREQI_FIELDS.BPREQI_ALLFIELDS, requestInfo));
            _bpRequestInfo = requestInfo[0];
            
            _engine = engine;
            _bpManager = bpManager;

            _enabled = true;
            _deleted = false;
        }

        private bool CanBind() {
            // Reject binding breakpoints which are deleted, not code file line, and on condition changed
            if (_deleted ||
                _bpRequestInfo.bpLocation.bpLocationType != (uint)enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE ||
                _bpRequestInfo.bpCondition.styleCondition == enum_BP_COND_STYLE.BP_COND_WHEN_CHANGED
            ) {
                return false;
            }

            return true;
        }

        public void AddBreakpointError(AD7BreakpointErrorEvent breakpointError) {
            _breakpointErrors.Add(breakpointError);
        }

        // Remove all of the bound breakpoints for this pending breakpoint
        public void ClearBreakpointBindingResults() {
            lock (_boundBreakpoints) {
                for (int i = _boundBreakpoints.Count - 1; i >= 0; i--) {
                    ((IDebugBoundBreakpoint2)_boundBreakpoints[i]).Delete();
                }
            }
            _breakpointErrors.Clear();
        }

        // Called by bound breakpoints when they are being deleted.
        public void OnBoundBreakpointDeleted(AD7BoundBreakpoint boundBreakpoint) {
            lock (_boundBreakpoints) {
                _boundBreakpoints.Remove(boundBreakpoint);
            }
        }

        public BP_PASSCOUNT PassCount {
            get {
                return _bpRequestInfo.bpPassCount;
            }
        }

        #region IDebugPendingBreakpoint2 Members

        // Binds this pending breakpoint to one or more code locations.
        int IDebugPendingBreakpoint2.Bind() {
            if (CanBind()) {
                // Get the location in the document that the breakpoint is in.
                var startPosition = new TEXT_POSITION[1];
                var endPosition = new TEXT_POSITION[1];
                string fileName;
                var docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(_bpRequestInfo.bpLocation.unionmember2));
                EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));
                EngineUtils.CheckOk(docPosition.GetFileName(out fileName));

                var breakpoint =
                    _engine.Process.AddBreakpoint(
                        fileName,
                        (int)startPosition[0].dwLine,
                        (int)startPosition[0].dwColumn,
                        _enabled,
                        AD7BoundBreakpoint.GetBreakOnForPassCount(_bpRequestInfo.bpPassCount),
                        _bpRequestInfo.bpCondition.bstrCondition
                    );
                _bpManager.AddPendingBreakpoint(breakpoint, this);
                breakpoint.BindAsync().Wait(2000);

                return VSConstants.S_OK;
            }

            // The breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
            // The sample engine does not support this, but a real world engine will want to send an instance of IDebugBreakpointErrorEvent2 to the
            // UI and return a valid instance of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints. The debugger will then
            // display information about why the breakpoint did not bind to the user.
            return VSConstants.S_FALSE;            
        }

        // Determines whether this pending breakpoint can bind to a code location.
        int IDebugPendingBreakpoint2.CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum) {
            ppErrorEnum = null;

            if (!CanBind()) {
                // Called to determine if a pending breakpoint can be bound. 
                // The breakpoint may not be bound for many reasons such as an invalid location, an invalid expression, etc...
                // The sample engine does not support this, but a real world engine will want to return a valid enumeration of IDebugErrorBreakpoint2.
                // The debugger will then display information about why the breakpoint did not bind to the user.
                return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }

        // Deletes this pending breakpoint and all breakpoints bound from it.
        int IDebugPendingBreakpoint2.Delete() {
            ClearBreakpointBindingResults();
            return VSConstants.S_OK;
        }

        // Toggles the enabled state of this pending breakpoint.
        int IDebugPendingBreakpoint2.Enable(int fEnable) {
            lock (_boundBreakpoints) {
                _enabled = fEnable != 0;

                foreach (AD7BoundBreakpoint bp in _boundBreakpoints) {
                    ((IDebugBoundBreakpoint2)bp).Enable(fEnable);
                }
            }

            return VSConstants.S_OK;
        }

        // Enumerates all breakpoints bound from this pending breakpoint
        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum) {
            lock (_boundBreakpoints) {
                IDebugBoundBreakpoint2[] boundBreakpoints = _boundBreakpoints.ToArray();
                ppEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
            }
            return VSConstants.S_OK;
        }

        // Enumerates all error breakpoints that resulted from this pending breakpoint.
        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum) {
            // Called when a pending breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
            // Return a valid enumeration of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints, allowing the debugger to
            // display information about why the breakpoint did not bind to the user.
            lock (_breakpointErrors) {
                IDebugErrorBreakpoint2[] breakpointErrors = _breakpointErrors.ToArray();
                ppEnum = new AD7ErrorBreakpointsEnum(breakpointErrors);
            }
            return VSConstants.S_OK;
        }

        // Gets the breakpoint request that was used to create this pending breakpoint
        int IDebugPendingBreakpoint2.GetBreakpointRequest(out IDebugBreakpointRequest2 ppBpRequest) {
            ppBpRequest = _bpRequest;
            return VSConstants.S_OK;
        }

        // Gets the state of this pending breakpoint.
        int IDebugPendingBreakpoint2.GetState(PENDING_BP_STATE_INFO[] pState) {
            if (_deleted) {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DELETED;
            } else if (_enabled) {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_ENABLED;
            } else {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DISABLED;
            }

            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.SetCondition(BP_CONDITION bpCondition) {
            if (bpCondition.styleCondition == enum_BP_COND_STYLE.BP_COND_WHEN_CHANGED) {
                return VSConstants.E_NOTIMPL;
            }

            _bpRequestInfo.bpCondition = bpCondition;
            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount) {
            _bpRequestInfo.bpPassCount = bpPassCount;
            return VSConstants.S_OK;
        }

        // Toggles the virtualized state of this pending breakpoint. When a pending breakpoint is virtualized, 
        // the debug engine will attempt to bind it every time new code loads into the program.
        // The sample engine will does not support this.
        int IDebugPendingBreakpoint2.Virtualize(int fVirtualize) {
            return VSConstants.S_OK;
        }

        #endregion

        public string DocumentName {
            get {
                if (_documentName == null) {
                    var docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(_bpRequestInfo.bpLocation.unionmember2));
                    EngineUtils.CheckOk(docPosition.GetFileName(out _documentName));
                }
                return _documentName;
            }
        }
    }
}

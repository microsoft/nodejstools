// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    // This class represents a pending breakpoint which is an abstract representation of a breakpoint before it is bound.
    // When a user creates a new breakpoint, the pending breakpoint is created and is later bound. The bound breakpoints
    // become children of the pending breakpoint.
    internal class AD7PendingBreakpoint : IDebugPendingBreakpoint2
    {
        // The breakpoint request that resulted in this pending breakpoint being created.
        private readonly BreakpointManager _bpManager;
        private readonly IDebugBreakpointRequest2 _bpRequest;
        private readonly List<AD7BreakpointErrorEvent> _breakpointErrors = new List<AD7BreakpointErrorEvent>();
        private readonly AD7Engine _engine;
        private BP_REQUEST_INFO _bpRequestInfo;
        private NodeBreakpoint _breakpoint;
        private string _documentName;
        private bool _enabled, _deleted;

        public AD7PendingBreakpoint(IDebugBreakpointRequest2 pBpRequest, AD7Engine engine, BreakpointManager bpManager)
        {
            this._bpRequest = pBpRequest;
            var requestInfo = new BP_REQUEST_INFO[1];
            EngineUtils.CheckOk(this._bpRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION | enum_BPREQI_FIELDS.BPREQI_CONDITION | enum_BPREQI_FIELDS.BPREQI_ALLFIELDS, requestInfo));
            this._bpRequestInfo = requestInfo[0];

            this._engine = engine;
            this._bpManager = bpManager;

            this._enabled = true;
            this._deleted = false;
        }

        public BP_PASSCOUNT PassCount => this._bpRequestInfo.bpPassCount;
        public string DocumentName
        {
            get
            {
                if (this._documentName == null)
                {
                    var docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(this._bpRequestInfo.bpLocation.unionmember2));
                    EngineUtils.CheckOk(docPosition.GetFileName(out this._documentName));
                }
                return this._documentName;
            }
        }

        #region IDebugPendingBreakpoint2 Members

        // Binds this pending breakpoint to one or more code locations.
        int IDebugPendingBreakpoint2.Bind()
        {
            if (CanBind())
            {
                // Get the location in the document that the breakpoint is in.
                var startPosition = new TEXT_POSITION[1];
                var endPosition = new TEXT_POSITION[1];
                string fileName;
                var docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(this._bpRequestInfo.bpLocation.unionmember2));
                EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));
                EngineUtils.CheckOk(docPosition.GetFileName(out fileName));

                this._breakpoint = this._engine.Process.AddBreakpoint(
                    fileName,
                    (int)startPosition[0].dwLine,
                    (int)startPosition[0].dwColumn,
                    this._enabled,
                    AD7BoundBreakpoint.GetBreakOnForPassCount(this._bpRequestInfo.bpPassCount),
                    this._bpRequestInfo.bpCondition.bstrCondition);

                this._bpManager.AddPendingBreakpoint(this._breakpoint, this);
                this._breakpoint.BindAsync().WaitAsync(TimeSpan.FromSeconds(2)).Wait();

                return VSConstants.S_OK;
            }

            // The breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
            // The sample engine does not support this, but a real world engine will want to send an instance of IDebugBreakpointErrorEvent2 to the
            // UI and return a valid instance of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints. The debugger will then
            // display information about why the breakpoint did not bind to the user.
            return VSConstants.S_FALSE;
        }

        // Determines whether this pending breakpoint can bind to a code location.
        int IDebugPendingBreakpoint2.CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            ppErrorEnum = null;

            if (!CanBind())
            {
                // Called to determine if a pending breakpoint can be bound. 
                // The breakpoint may not be bound for many reasons such as an invalid location, an invalid expression, etc...
                // The sample engine does not support this, but a real world engine will want to return a valid enumeration of IDebugErrorBreakpoint2.
                // The debugger will then display information about why the breakpoint did not bind to the user.
                return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }

        // Deletes this pending breakpoint and all breakpoints bound from it.
        int IDebugPendingBreakpoint2.Delete()
        {
            ClearBreakpointBindingResults();
            this._deleted = true;
            return VSConstants.S_OK;
        }

        // Toggles the enabled state of this pending breakpoint.
        int IDebugPendingBreakpoint2.Enable(int fEnable)
        {
            this._enabled = fEnable != 0;

            if (this._breakpoint != null)
            {
                lock (this._breakpoint)
                {
                    foreach (var binding in this._breakpoint.GetBindings())
                    {
                        var boundBreakpoint = (IDebugBoundBreakpoint2)this._bpManager.GetBoundBreakpoint(binding);
                        boundBreakpoint.Enable(fEnable);
                    }
                }
            }

            return VSConstants.S_OK;
        }

        // Enumerates all breakpoints bound from this pending breakpoint
        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            ppEnum = null;

            if (this._breakpoint != null)
            {
                lock (this._breakpoint)
                {
                    var boundBreakpoints = this._breakpoint.GetBindings()
                        .Select(binding => this._bpManager.GetBoundBreakpoint(binding))
                        .Cast<IDebugBoundBreakpoint2>().ToArray();

                    ppEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
                }
            }

            return VSConstants.S_OK;
        }

        // Enumerates all error breakpoints that resulted from this pending breakpoint.
        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            // Called when a pending breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
            // Return a valid enumeration of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints, allowing the debugger to
            // display information about why the breakpoint did not bind to the user.
            lock (this._breakpointErrors)
            {
                var breakpointErrors = this._breakpointErrors.Cast<IDebugErrorBreakpoint2>().ToArray();
                ppEnum = new AD7ErrorBreakpointsEnum(breakpointErrors);
            }

            return VSConstants.S_OK;
        }

        // Gets the breakpoint request that was used to create this pending breakpoint
        int IDebugPendingBreakpoint2.GetBreakpointRequest(out IDebugBreakpointRequest2 ppBpRequest)
        {
            ppBpRequest = this._bpRequest;
            return VSConstants.S_OK;
        }

        // Gets the state of this pending breakpoint.
        int IDebugPendingBreakpoint2.GetState(PENDING_BP_STATE_INFO[] pState)
        {
            if (this._deleted)
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DELETED;
            }
            else if (this._enabled)
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_ENABLED;
            }
            else
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DISABLED;
            }

            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            if (bpCondition.styleCondition == enum_BP_COND_STYLE.BP_COND_WHEN_CHANGED)
            {
                return VSConstants.E_NOTIMPL;
            }

            this._bpRequestInfo.bpCondition = bpCondition;
            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            this._bpRequestInfo.bpPassCount = bpPassCount;
            return VSConstants.S_OK;
        }

        // Toggles the virtualized state of this pending breakpoint. When a pending breakpoint is virtualized, 
        // the debug engine will attempt to bind it every time new code loads into the program.
        // The sample engine will does not support this.
        int IDebugPendingBreakpoint2.Virtualize(int fVirtualize)
        {
            return VSConstants.S_OK;
        }

        #endregion

        private bool CanBind()
        {
            // Reject binding breakpoints which are deleted, not code file line, and on condition changed
            if (this._deleted ||
                this._bpRequestInfo.bpLocation.bpLocationType != (uint)enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE ||
                this._bpRequestInfo.bpCondition.styleCondition == enum_BP_COND_STYLE.BP_COND_WHEN_CHANGED)
            {
                return false;
            }

            return true;
        }

        public void AddBreakpointError(AD7BreakpointErrorEvent breakpointError)
        {
            this._breakpointErrors.Add(breakpointError);
        }

        // Remove all of the bound breakpoints for this pending breakpoint
        public void ClearBreakpointBindingResults()
        {
            if (this._breakpoint != null)
            {
                lock (this._breakpoint)
                {
                    foreach (var binding in this._breakpoint.GetBindings())
                    {
                        var boundBreakpoint = (IDebugBoundBreakpoint2)this._bpManager.GetBoundBreakpoint(binding);
                        if (boundBreakpoint != null)
                        {
                            boundBreakpoint.Delete();
                            binding.Remove().WaitAndUnwrapExceptions();
                        }
                    }
                }

                this._bpManager.RemovePendingBreakpoint(this._breakpoint);
                this._breakpoint.Deleted = true;
                this._breakpoint = null;
            }

            this._breakpointErrors.Clear();
        }
    }
}

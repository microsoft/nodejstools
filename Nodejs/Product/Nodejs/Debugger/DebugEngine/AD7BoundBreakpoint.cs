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

using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.DebugEngine {
    // This class represents a breakpoint that has been bound to a location in the debuggee. It is a child of the pending breakpoint
    // that creates it. Unless the pending breakpoint only has one bound breakpoint, each bound breakpoint is displayed as a child of the
    // pending breakpoint in the breakpoints window. Otherwise, only one is displayed.
    class AD7BoundBreakpoint : IDebugBoundBreakpoint2 {
        private readonly NodeBreakpointBinding _breakpointBinding;
        private readonly AD7BreakpointResolution _breakpointResolution;
        private readonly bool _enabled;
        private readonly AD7PendingBreakpoint _pendingBreakpoint;
        private bool _deleted;

        public AD7BoundBreakpoint(
            NodeBreakpointBinding breakpointBinding,
            AD7PendingBreakpoint pendingBreakpoint,
            AD7BreakpointResolution breakpointResolution,
            bool enabled) {
            _breakpointBinding = breakpointBinding;
            _pendingBreakpoint = pendingBreakpoint;
            _breakpointResolution = breakpointResolution;
            _enabled = enabled;
            _deleted = false;
        }

        #region IDebugBoundBreakpoint2 Members

        // Called when the breakpoint is being deleted by the user.
        int IDebugBoundBreakpoint2.Delete() {
            AssertMainThread();

            if (!_deleted) {
                _deleted = true;
                _breakpointBinding.Remove().GetAwaiter().GetResult();
            }

            return VSConstants.S_OK;
        }

        // Called by the debugger UI when the user is enabling or disabling a breakpoint.
        int IDebugBoundBreakpoint2.Enable(int fEnable) {
            AssertMainThread();

            if (!_breakpointBinding.SetEnabledAsync(fEnable != 0).GetAwaiter().GetResult()) {
                return VSConstants.E_FAIL;
            }

            return VSConstants.S_OK;
        }

        // Return the breakpoint resolution which describes how the breakpoint bound in the debuggee.
        int IDebugBoundBreakpoint2.GetBreakpointResolution(out IDebugBreakpointResolution2 ppBpResolution) {
            ppBpResolution = _breakpointResolution;
            return VSConstants.S_OK;
        }

        // Return the pending breakpoint for this bound breakpoint.
        int IDebugBoundBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint) {
            ppPendingBreakpoint = _pendingBreakpoint;
            return VSConstants.S_OK;
        }

        // 
        int IDebugBoundBreakpoint2.GetState(enum_BP_STATE [] pState) {
            pState[0] = 0;

            if (_deleted) {
                pState[0] = enum_BP_STATE.BPS_DELETED;
            } else if (_enabled) {
                pState[0] = enum_BP_STATE.BPS_ENABLED;
            } else if (!_enabled) {
                pState[0] = enum_BP_STATE.BPS_DISABLED;
            }

            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.GetHitCount(out uint pdwHitCount) {
            pdwHitCount = _breakpointBinding.GetHitCount();
            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.SetCondition(BP_CONDITION bpCondition) {
            if (bpCondition.styleCondition == enum_BP_COND_STYLE.BP_COND_WHEN_CHANGED) {
                return VSConstants.E_NOTIMPL;
            }

            if (!_breakpointBinding.SetConditionAsync(bpCondition.bstrCondition).GetAwaiter().GetResult()) {
                return VSConstants.E_FAIL;
            }

            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.SetHitCount(uint dwHitCount) {
            AssertMainThread();

            if (!_breakpointBinding.SetHitCountAsync(dwHitCount).GetAwaiter().GetResult()) {
                return VSConstants.E_FAIL;
            }

            return VSConstants.S_OK;
        }

        int IDebugBoundBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount) {
            AssertMainThread();

            BreakOn breakOn = GetBreakOnForPassCount(bpPassCount);
            if (!_breakpointBinding.SetBreakOnAsync(breakOn).GetAwaiter().GetResult()) {
                return VSConstants.E_FAIL;
            }

            return VSConstants.S_OK;
        }

        [ Conditional("DEBUG") ]
        private static void AssertMainThread() {
            //Debug.Assert(Worker.MainThreadId == Worker.CurrentThreadId);
        }

        internal static BreakOn GetBreakOnForPassCount(BP_PASSCOUNT bpPassCount) {
            BreakOn breakOn;
            uint count = bpPassCount.dwPassCount;
            switch (bpPassCount.stylePassCount) {
                case enum_BP_PASSCOUNT_STYLE.BP_PASSCOUNT_NONE:
                    breakOn = new BreakOn(BreakOnKind.Always, count);
                    break;
                case enum_BP_PASSCOUNT_STYLE.BP_PASSCOUNT_EQUAL:
                    breakOn = new BreakOn(BreakOnKind.Equal, count);
                    break;
                case enum_BP_PASSCOUNT_STYLE.BP_PASSCOUNT_EQUAL_OR_GREATER:
                    breakOn = new BreakOn(BreakOnKind.GreaterThanOrEqual, count);
                    break;
                case enum_BP_PASSCOUNT_STYLE.BP_PASSCOUNT_MOD:
                    breakOn = new BreakOn(BreakOnKind.Mod, count);
                    break;
                default:
                    breakOn = new BreakOn(BreakOnKind.Always, count);
                    break;
            }
            return breakOn;
        }

        #endregion
    }
}
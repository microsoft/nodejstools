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

using System.Collections.Generic;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.DebugEngine {
    // This class manages breakpoints for the engine. 
    class BreakpointManager {
        private AD7Engine m_engine;
        private System.Collections.Generic.List<AD7PendingBreakpoint> m_pendingBreakpoints;
        private Dictionary<NodeBreakpoint, AD7PendingBreakpoint> _breakpointMap = new Dictionary<NodeBreakpoint, AD7PendingBreakpoint>();
        private Dictionary<NodeBreakpointBinding, AD7BoundBreakpoint> _breakpointBindingMap = new Dictionary<NodeBreakpointBinding, AD7BoundBreakpoint>();

        public BreakpointManager(AD7Engine engine) {
            m_engine = engine;
            m_pendingBreakpoints = new System.Collections.Generic.List<AD7PendingBreakpoint>();
        }

        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP) {
            AD7PendingBreakpoint pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, m_engine, this);
            ppPendingBP = (IDebugPendingBreakpoint2)pendingBreakpoint;
            m_pendingBreakpoints.Add(pendingBreakpoint);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBreakpointBindingResults() {
            foreach (AD7PendingBreakpoint pendingBreakpoint in m_pendingBreakpoints) {
                pendingBreakpoint.ClearBreakpointBindingResults();
            }
        }

        public void AddPendingBreakpoint(NodeBreakpoint breakpoint, AD7PendingBreakpoint pendingBreakpoint) {
            _breakpointMap[breakpoint] = pendingBreakpoint;
        }

        public void RemovePendingBreakpoint(NodeBreakpoint breakpoint) {
            _breakpointMap.Remove(breakpoint);
        }

        public AD7PendingBreakpoint GetPendingBreakpoint(NodeBreakpoint breakpoint) {
            return _breakpointMap[breakpoint];
        }

        public void AddBoundBreakpoint(NodeBreakpointBinding breakpointBinding, AD7BoundBreakpoint boundBreakpoint) {
            _breakpointBindingMap[breakpointBinding] = boundBreakpoint;
        }

        public void RemoveBoundBreakpoint(NodeBreakpointBinding breakpointBinding) {
            _breakpointBindingMap.Remove(breakpointBinding);
        }

        public AD7BoundBreakpoint GetBoundBreakpoint(NodeBreakpointBinding breakpointBinding) {
            AD7BoundBreakpoint boundBreakpoint;
            return _breakpointBindingMap.TryGetValue(breakpointBinding, out boundBreakpoint) ? boundBreakpoint : null;
        }
    }
}

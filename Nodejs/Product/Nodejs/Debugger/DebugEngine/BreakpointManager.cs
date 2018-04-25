// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.DebugEngine
{
    // This class manages breakpoints for the engine. 
    internal sealed class BreakpointManager
    {
        private readonly AD7Engine engine;
        private readonly List<AD7PendingBreakpoint> pendingBreakpoints = new List<AD7PendingBreakpoint>();
        private readonly Dictionary<NodeBreakpoint, AD7PendingBreakpoint> breakpointMap = new Dictionary<NodeBreakpoint, AD7PendingBreakpoint>();
        private readonly Dictionary<NodeBreakpointBinding, AD7BoundBreakpoint> breakpointBindingMap = new Dictionary<NodeBreakpointBinding, AD7BoundBreakpoint>();

        public BreakpointManager(AD7Engine engine)
        {
            this.engine = engine;
        }

        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = new AD7PendingBreakpoint(pBPRequest, this.engine, this);
            this.pendingBreakpoints.Add((AD7PendingBreakpoint)ppPendingBP);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBreakpointBindingResults()
        {
            foreach (var pendingBreakpoint in this.pendingBreakpoints)
            {
                pendingBreakpoint.ClearBreakpointBindingResults();
            }
        }

        public void AddPendingBreakpoint(NodeBreakpoint breakpoint, AD7PendingBreakpoint pendingBreakpoint)
        {
            this.breakpointMap[breakpoint] = pendingBreakpoint;
        }

        public void RemovePendingBreakpoint(NodeBreakpoint breakpoint)
        {
            this.breakpointMap.Remove(breakpoint);
        }

        public AD7PendingBreakpoint GetPendingBreakpoint(NodeBreakpoint breakpoint)
        {
            return this.breakpointMap.TryGetValue(breakpoint, out var pendingBreakpoint) ? pendingBreakpoint : null;
        }

        public void AddBoundBreakpoint(NodeBreakpointBinding breakpointBinding, AD7BoundBreakpoint boundBreakpoint)
        {
            this.breakpointBindingMap[breakpointBinding] = boundBreakpoint;
        }

        public void RemoveBoundBreakpoint(NodeBreakpointBinding breakpointBinding)
        {
            this.breakpointBindingMap.Remove(breakpointBinding);
        }

        public AD7BoundBreakpoint GetBoundBreakpoint(NodeBreakpointBinding breakpointBinding)
        {
            return this.breakpointBindingMap.TryGetValue(breakpointBinding, out var boundBreakpoint) ? boundBreakpoint : null;
        }
    }
}

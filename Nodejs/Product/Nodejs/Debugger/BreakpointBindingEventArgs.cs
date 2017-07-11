// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class BreakpointBindingEventArgs : EventArgs
    {
        public readonly NodeBreakpoint Breakpoint;
        public readonly NodeBreakpointBinding BreakpointBinding;

        public BreakpointBindingEventArgs(NodeBreakpoint breakpoint, NodeBreakpointBinding breakpointBinding)
        {
            this.Breakpoint = breakpoint;
            this.BreakpointBinding = breakpointBinding;
        }
    }
}

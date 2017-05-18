// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class BreakpointHitEventArgs : EventArgs
    {
        public readonly NodeBreakpointBinding BreakpointBinding;
        public readonly NodeThread Thread;

        public BreakpointHitEventArgs(NodeBreakpointBinding breakpoint, NodeThread thread)
        {
            this.BreakpointBinding = breakpoint;
            this.Thread = thread;
        }
    }
}

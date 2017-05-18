// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.NodejsTools.Debugger.Events;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class BreakpointEventArgs : EventArgs
    {
        public BreakpointEventArgs(BreakpointEvent breakpointEvent)
        {
            this.BreakpointEvent = breakpointEvent;
        }

        public BreakpointEvent BreakpointEvent { get; private set; }
    }
}


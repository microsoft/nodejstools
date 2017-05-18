// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class ClearBreakpointCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public ClearBreakpointCommand(int id, int breakpointId) : base(id, "clearbreakpoint")
        {
            this._arguments = new Dictionary<string, object> {
                { "breakpoint", breakpointId }
            };
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
    }
}

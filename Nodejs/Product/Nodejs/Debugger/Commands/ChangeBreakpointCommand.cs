// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class ChangeBreakpointCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public ChangeBreakpointCommand(int id, int breakpointId, bool? enabled = null, string condition = null, int? ignoreCount = null)
            : base(id, "changebreakpoint")
        {
            this._arguments = new Dictionary<string, object> { { "breakpoint", breakpointId } };

            if (enabled != null)
            {
                this._arguments["enabled"] = enabled.Value;
            }

            if (condition != null)
            {
                this._arguments["condition"] = condition;
            }

            if (ignoreCount != null)
            {
                this._arguments["ignoreCount"] = ignoreCount.Value;
            }
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
    }
}

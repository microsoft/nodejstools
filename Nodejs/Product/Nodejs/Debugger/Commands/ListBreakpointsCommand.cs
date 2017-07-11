// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class ListBreakpointsCommand : DebuggerCommand
    {
        public ListBreakpointsCommand(int id) : base(id, "listbreakpoints")
        {
        }

        public Dictionary<int, int> Breakpoints { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            var body = response["body"];

            var breakpoints = (JArray)body["breakpoints"] ?? new JArray();
            this.Breakpoints = new Dictionary<int, int>(breakpoints.Count);

            foreach (var breakpoint in breakpoints)
            {
                var breakpointId = (int)breakpoint["number"];
                var hitCount = (int)breakpoint["hit_count"];

                this.Breakpoints.Add(breakpointId, hitCount);
            }
        }
    }
}

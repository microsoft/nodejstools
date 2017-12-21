// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events
{
    internal sealed class BreakpointEvent : IDebuggerEvent
    {
        public BreakpointEvent(JObject message)
        {
            this.Running = false;
            this.Line = (int)message["body"]["sourceLine"];
            this.Column = (int)message["body"]["sourceColumn"];

            var scriptId = (int)message["body"]["script"]["id"];
            var fileName = (string)message["body"]["script"]["name"];

            this.Module = new NodeModule(scriptId, fileName);

            var breakpoints = message["body"]["breakpoints"];
            this.Breakpoints = breakpoints != null
                ? breakpoints.Values<int>().ToList()
                : new List<int>();
        }

        public List<int> Breakpoints { get; }
        public NodeModule Module { get; }
        public int Line { get; }
        public int Column { get; }
        public bool Running { get; }
    }
}

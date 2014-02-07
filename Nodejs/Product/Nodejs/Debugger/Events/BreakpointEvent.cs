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
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events {
    sealed class BreakpointEvent : IDebuggerEvent {
        public BreakpointEvent(JObject message) {
            Running = (bool)message["running"];
            Line = (int)message["body"]["sourceLine"];
            Column = (int)message["body"]["sourceColumn"];

            var scriptId = (int)message["body"]["script"]["id"];
            var filename = (string)message["body"]["script"]["name"];

            Module = new NodeModule(scriptId, filename);
            Breakpoints = message["body"]["breakpoints"].Values<int>().ToList();
        }

        public List<int> Breakpoints { get; private set; }

        public NodeModule Module { get; private set; }
        public int Line { get; private set; }
        public int Column { get; set; }
        public bool Running { get; private set; }
    }
}
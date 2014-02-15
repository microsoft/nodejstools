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
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class ListBreakpointsCommand : DebuggerCommand {
        public ListBreakpointsCommand(int id) : base(id, "listbreakpoints") { }

        public Dictionary<int, int> Breakpoints { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JToken body = response["body"];
            var breakpoints = (JArray)body["breakpoints"];

            var result = new Dictionary<int, int>(breakpoints.Count);
            foreach (JToken breakpoint in breakpoints) {
                var breakpointId = (int)breakpoint["number"];
                var hitCount = (int)breakpoint["hit_count"];

                result.Add(breakpointId, hitCount);
            }

            Breakpoints = result;
        }
    }
}
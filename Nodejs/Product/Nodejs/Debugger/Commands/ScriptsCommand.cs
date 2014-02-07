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
    sealed class ScriptsCommand : DebuggerCommandBase {
        public ScriptsCommand(int id, int? moduleId = null) : base(id) {
            CommandName = "scripts";
            Arguments = new Dictionary<string, object> {
                { "includeSource", true },
            };

            if (moduleId != null) {
                Arguments["ids"] = new object[] { moduleId };
            }

            Modules = new List<NodeModule>();
        }

        public List<NodeModule> Modules { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            var body = (JArray)response["body"];
            foreach (JToken module in body) {
                var id = (int)module["id"];
                var source = (string)module["source"];
                var name = (string)module["name"];

                Modules.Add(new NodeModule(id, name, source));
            }
        }
    }
}
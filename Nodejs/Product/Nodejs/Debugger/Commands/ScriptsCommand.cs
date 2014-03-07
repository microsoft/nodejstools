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
    sealed class ScriptsCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;
        private readonly NodeDebugger _debugger;

        public ScriptsCommand(int id, NodeDebugger debugger, bool includeSource = false, int? moduleId = null) : base(id, "scripts") {
            _debugger = debugger;
            _arguments = new Dictionary<string, object> {
                { "includeSource", includeSource }
            };

            if (moduleId != null) {
                _arguments["ids"] = new object[] { moduleId };
            }
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }

        public List<NodeModule> Modules { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            JArray body = (JArray)response["body"] ?? new JArray();
            Modules = new List<NodeModule>(body.Count);

            foreach (JToken module in body) {
                var id = (int)module["id"];
                var source = (string)module["source"];
                if (!string.IsNullOrEmpty(source) &&
                    source.StartsWith(NodeConstants.ScriptWrapBegin) && 
                    source.EndsWith(NodeConstants.ScriptWrapEnd)) {
                    source = source.Substring(
                        NodeConstants.ScriptWrapBegin.Length, 
                        source.Length - NodeConstants.ScriptWrapBegin.Length - NodeConstants.ScriptWrapEnd.Length);
                }

                var javaScriptFileName = (string)module["name"];
                string fileName = _debugger.GetModuleFileName(javaScriptFileName);

                Modules.Add(new NodeModule(id, fileName, javaScriptFileName) { Source = source });
            }
        }
    }
}
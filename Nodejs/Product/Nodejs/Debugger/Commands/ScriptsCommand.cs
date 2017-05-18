// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class ScriptsCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public ScriptsCommand(int id, bool includeSource = false, int? moduleId = null) : base(id, "scripts")
        {
            this._arguments = new Dictionary<string, object> {
                { "includeSource", includeSource }
            };

            if (moduleId != null)
            {
                this._arguments["ids"] = new object[] { moduleId };
            }
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
        public List<NodeModule> Modules { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            var body = (JArray)response["body"] ?? new JArray();
            this.Modules = new List<NodeModule>(body.Count);

            foreach (var module in body)
            {
                var fileName = (string)module["name"];
                if (fileName == null)
                {
                    continue;
                }

                var id = (int)module["id"];
                var source = (string)module["source"];
                if (!string.IsNullOrEmpty(source) &&
                    source.StartsWith(NodeConstants.ScriptWrapBegin, StringComparison.Ordinal) &&
                    source.EndsWith(NodeConstants.ScriptWrapEnd, StringComparison.Ordinal))
                {
                    source = source.Substring(
                        NodeConstants.ScriptWrapBegin.Length,
                        source.Length - NodeConstants.ScriptWrapBegin.Length - NodeConstants.ScriptWrapEnd.Length);
                }

                this.Modules.Add(new NodeModule(id, fileName) { Source = source });
            }
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class ChangeLiveCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public ChangeLiveCommand(int id, NodeModule module) : base(id, "changelive")
        {
            // Wrap script contents as following https://github.com/joyent/node/blob/v0.10.26-release/src/node.js#L880
            var source = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
                NodeConstants.ScriptWrapBegin,
                module.Source,
                NodeConstants.ScriptWrapEnd);

            this._arguments = new Dictionary<string, object> {
                { "script_id", module.Id },
                { "new_source", source },
                { "preview_only", false },
            };
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
        public bool Updated { get; private set; }
        public bool StackModified { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            var result = response["body"]["result"];
            this.Updated = (bool)result["updated"];
            this.StackModified = (bool)result["stack_modified"];
        }
    }
}

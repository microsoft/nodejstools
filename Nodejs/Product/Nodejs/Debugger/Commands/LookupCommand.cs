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
using System.Globalization;
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class LookupCommand : DebuggerCommandBase {
        private readonly NodeEvaluationResult _parent;
        private readonly IEvaluationResultFactory _resultFactory;

        public LookupCommand(int id, IEvaluationResultFactory resultFactory, int[] handles, NodeEvaluationResult parent = null) : base(id) {
            _resultFactory = resultFactory;
            _parent = parent;

            CommandName = "lookup";
            Arguments = new Dictionary<string, object> {
                { "handles", handles },
                { "includeSource", false }
            };
        }

        public List<NodeEvaluationResult> Results { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            // Retrieve references
            var refs = (JArray)response["refs"];
            var references = new Dictionary<int, JToken>(refs.Count);
            for (int i = 0; i < refs.Count; i++) {
                JToken reference = refs[i];
                var id = (int)reference["handle"];
                references.Add(id, reference);
            }

            // Retrieve properties
            JToken body = response["body"];
            string handle = _parent.Handle.ToString(CultureInfo.InvariantCulture);
            JToken objectData = body[handle];
            var properties = new List<NodeEvaluationResult>();

            var props = (JArray)objectData["properties"];
            if (props != null) {
                for (int i = 0; i < props.Count; i++) {
                    JToken property = props[i];
                    var variableProvider = new NodeLookupVariable(_parent, property, references);
                    NodeEvaluationResult result = _resultFactory.Create(variableProvider);
                    properties.Add(result);
                }
            }

            // Try to get prototype
            JToken prototype = objectData["protoObject"];
            if (prototype != null) {
                var variableProvider = new NodePrototypeVariable(_parent, prototype, references);
                NodeEvaluationResult result = _resultFactory.Create(variableProvider);
                properties.Add(result);
            }

            Results = properties;
        }
    }
}
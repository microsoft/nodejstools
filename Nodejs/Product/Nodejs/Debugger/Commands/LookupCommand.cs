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
using System.Linq;
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class LookupCommand : DebuggerCommandBase {
        private readonly int[] _handles;
        private readonly Dictionary<int, NodeEvaluationResult> _parents;
        private readonly IEvaluationResultFactory _resultFactory;

        public LookupCommand(int id, IEvaluationResultFactory resultFactory, List<NodeEvaluationResult> parents)
            : this(id, resultFactory, parents.Select(p => p.Handle).ToArray()) {
            _parents = parents.ToDictionary(p => p.Handle);
        }

        public LookupCommand(int id, IEvaluationResultFactory resultFactory, int[] handles) : base(id) {
            _resultFactory = resultFactory;
            _handles = handles;

            CommandName = "lookup";
            Arguments = new Dictionary<string, object> {
                { "handles", handles },
                { "includeSource", false }
            };

            Results = new Dictionary<int, List<NodeEvaluationResult>>();
        }

        public Dictionary<int, List<NodeEvaluationResult>> Results { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            // Retrieve references
            var refs = (JArray)response["refs"];
            var references = new Dictionary<int, JToken>(refs.Count);
            foreach (JToken reference in refs) {
                var id = (int)reference["handle"];
                references.Add(id, reference);
            }

            // Retrieve properties
            JToken body = response["body"];

            foreach (int handle in _handles) {
                JToken data = body[handle.ToString(CultureInfo.InvariantCulture)];
                if (data == null) {
                    continue;
                }

                NodeEvaluationResult parent = null;
                if (_parents != null) {
                    _parents.TryGetValue(handle, out parent);
                }

                Results.Add(handle, GetProperties(data, parent, references));
            }
        }

        private List<NodeEvaluationResult> GetProperties(JToken data, NodeEvaluationResult parent, Dictionary<int, JToken> references) {
            var properties = new List<NodeEvaluationResult>();

            var props = (JArray)data["properties"];
            if (props != null) {
                for (int i = 0; i < props.Count; i++) {
                    JToken property = props[i];
                    var variableProvider = new NodeLookupVariable(parent, property, references);
                    NodeEvaluationResult result = _resultFactory.Create(variableProvider);
                    properties.Add(result);
                }
            }

            // Try to get prototype
            JToken prototype = data["protoObject"];
            if (prototype != null) {
                var variableProvider = new NodePrototypeVariable(parent, prototype, references);
                NodeEvaluationResult result = _resultFactory.Create(variableProvider);
                properties.Add(result);
            }

            return properties;
        }
    }
}
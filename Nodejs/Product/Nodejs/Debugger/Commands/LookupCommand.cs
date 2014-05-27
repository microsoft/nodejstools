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
    sealed class LookupCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;
        private readonly int[] _handles;
        private readonly Dictionary<int, NodeEvaluationResult> _parents;
        private readonly IEvaluationResultFactory _resultFactory;

        public LookupCommand(int id, IEvaluationResultFactory resultFactory, IEnumerable<NodeEvaluationResult> parents)
            : this(id, resultFactory, EnsureUniqueHandles(ref parents).Select(p => p.Handle).ToArray()) {
            _parents = parents.ToDictionary(p => p.Handle);
        }

        public LookupCommand(int id, IEvaluationResultFactory resultFactory, int[] handles)
            : base(id, "lookup") {
            _resultFactory = resultFactory;
            _handles = handles;

            _arguments = new Dictionary<string, object> {
                { "handles", handles },
                { "includeSource", false }
            };
        }

        private class HandleEqualityComparer : EqualityComparer<NodeEvaluationResult> {
            public static readonly HandleEqualityComparer Instance = new HandleEqualityComparer();

            public override bool Equals(NodeEvaluationResult x, NodeEvaluationResult y) {
                return x.Handle == y.Handle;
            }

            public override int GetHashCode(NodeEvaluationResult obj) {
                return obj.Handle.GetHashCode();
            }
        }

        private static IEnumerable<NodeEvaluationResult> EnsureUniqueHandles(ref IEnumerable<NodeEvaluationResult> parents) {
            return parents = parents.Distinct(HandleEqualityComparer.Instance);
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }

        public Dictionary<int, List<NodeEvaluationResult>> Results { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            // Retrieve references
            JArray refs = (JArray)response["refs"] ?? new JArray();
            var references = new Dictionary<int, JToken>(refs.Count);

            foreach (JToken reference in refs) {
                var id = (int)reference["handle"];
                references.Add(id, reference);
            }

            // Retrieve properties
            JToken body = response["body"];
            Results = new Dictionary<int, List<NodeEvaluationResult>>(_handles.Length);

            foreach (int handle in _handles) {
                string id = handle.ToString(CultureInfo.InvariantCulture);
                JToken data = body[id];
                if (data == null) {
                    continue;
                }

                NodeEvaluationResult parent = null;
                if (_parents != null) {
                    _parents.TryGetValue(handle, out parent);
                }

                List<NodeEvaluationResult> properties = GetProperties(data, parent, references);
                if (properties.Count == 0) {
                    // Primitive javascript type
                    var variable = new NodeEvaluationVariable(null, id, data);
                    NodeEvaluationResult property = _resultFactory.Create(variable);
                    properties.Add(property);
                }

                Results.Add(handle, properties);
            }
        }

        private List<NodeEvaluationResult> GetProperties(JToken data, NodeEvaluationResult parent, Dictionary<int, JToken> references) {
            var properties = new List<NodeEvaluationResult>();

            var props = (JArray)data["properties"];
            if (props != null) {
                properties.AddRange(props.Select(property => new NodeLookupVariable(parent, property, references))
                    .Select(variableProvider => _resultFactory.Create(variableProvider)));
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
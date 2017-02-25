//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class LookupCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;
        private readonly int[] _handles;
        private readonly Dictionary<int, NodeEvaluationResult> _parents;
        private readonly IEvaluationResultFactory _resultFactory;

        public LookupCommand(int id, IEvaluationResultFactory resultFactory, IEnumerable<NodeEvaluationResult> parents)
            : this(id, resultFactory, EnsureUniqueHandles(ref parents).Select(p => p.Handle).ToArray())
        {
            this._parents = parents.ToDictionary(p => p.Handle);
        }

        public LookupCommand(int id, IEvaluationResultFactory resultFactory, int[] handles)
            : base(id, "lookup")
        {
            this._resultFactory = resultFactory;
            this._handles = handles;

            this._arguments = new Dictionary<string, object> {
                { "handles", handles },
                { "includeSource", false }
            };
        }

        private class HandleEqualityComparer : EqualityComparer<NodeEvaluationResult>
        {
            public static readonly HandleEqualityComparer Instance = new HandleEqualityComparer();

            public override bool Equals(NodeEvaluationResult x, NodeEvaluationResult y)
            {
                return x.Handle == y.Handle;
            }

            public override int GetHashCode(NodeEvaluationResult obj)
            {
                return obj.Handle.GetHashCode();
            }
        }

        private static IEnumerable<NodeEvaluationResult> EnsureUniqueHandles(ref IEnumerable<NodeEvaluationResult> parents)
        {
            return parents = parents.Distinct(HandleEqualityComparer.Instance);
        }

        protected override IDictionary<string, object> Arguments
        {
            get { return this._arguments; }
        }

        public Dictionary<int, List<NodeEvaluationResult>> Results { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            // Retrieve references
            JArray refs = (JArray)response["refs"] ?? new JArray();
            var references = new Dictionary<int, JToken>(refs.Count);

            foreach (JToken reference in refs)
            {
                var id = (int)reference["handle"];
                references.Add(id, reference);
            }

            // Retrieve properties
            JToken body = response["body"];
            this.Results = new Dictionary<int, List<NodeEvaluationResult>>(this._handles.Length);

            foreach (int handle in this._handles)
            {
                string id = handle.ToString(CultureInfo.InvariantCulture);
                JToken data = body[id];
                if (data == null)
                {
                    continue;
                }

                NodeEvaluationResult parent = null;
                if (this._parents != null)
                {
                    this._parents.TryGetValue(handle, out parent);
                }

                List<NodeEvaluationResult> properties = GetProperties(data, parent, references);
                if (properties.Count == 0)
                {
                    // Primitive javascript type
                    var variable = new NodeEvaluationVariable(null, id, data);
                    NodeEvaluationResult property = this._resultFactory.Create(variable);
                    properties.Add(property);
                }

                this.Results.Add(handle, properties);
            }
        }

        private List<NodeEvaluationResult> GetProperties(JToken data, NodeEvaluationResult parent, Dictionary<int, JToken> references)
        {
            var properties = new List<NodeEvaluationResult>();

            var props = (JArray)data["properties"];
            if (props != null)
            {
                properties.AddRange(props.Select(property => new NodeLookupVariable(parent, property, references))
                    .Select(variableProvider => this._resultFactory.Create(variableProvider)));
            }

            // Try to get prototype
            JToken prototype = data["protoObject"];
            if (prototype != null)
            {
                var variableProvider = new NodePrototypeVariable(parent, prototype, references);
                NodeEvaluationResult result = this._resultFactory.Create(variableProvider);
                properties.Add(result);
            }

            return properties;
        }
    }
}
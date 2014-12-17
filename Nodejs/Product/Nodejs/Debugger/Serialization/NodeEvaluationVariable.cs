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

using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization {
    sealed class NodeEvaluationVariable : INodeVariable {
        public NodeEvaluationVariable(NodeStackFrame stackFrame, string name, JToken message) {
            Utilities.ArgumentNotNull("name", name);
            Utilities.ArgumentNotNull("message", message);

            Id = (int)message["handle"];
            Parent = null;
            StackFrame = stackFrame;
            Name = name;
            TypeName = (string)message["type"];
            Value = (string)message["value"];
            Class = (string)message["className"];
            Text = (string)message["text"];
            Attributes = NodePropertyAttributes.None;
            Type = NodePropertyType.Normal;
        }

        public int Id { get; private set; }
        public NodeEvaluationResult Parent { get; private set; }
        public string Name { get; private set; }
        public string TypeName { get; private set; }
        public string Value { get; private set; }
        public string Class { get; private set; }
        public string Text { get; private set; }
        public NodePropertyAttributes Attributes { get; private set; }
        public NodePropertyType Type { get; private set; }
        public NodeStackFrame StackFrame { get; private set; }
    }
}
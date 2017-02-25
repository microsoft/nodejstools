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

using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal sealed class NodeSetValueVariable : INodeVariable
    {
        public NodeSetValueVariable(NodeStackFrame stackFrame, string name, JToken message)
        {
            this.Id = (int)message["body"]["newValue"]["handle"];
            this.StackFrame = stackFrame;
            this.Parent = null;
            this.Name = name;
            this.TypeName = (string)message["body"]["newValue"]["type"];
            this.Value = (string)message["body"]["newValue"]["value"];
            this.Class = (string)message["body"]["newValue"]["className"];
            this.Text = (string)message["body"]["newValue"]["text"];
            this.Attributes = NodePropertyAttributes.None;
            this.Type = NodePropertyType.Normal;
        }

        public int Id { get; private set; }
        public NodeEvaluationResult Parent { get; private set; }
        public NodeStackFrame StackFrame { get; private set; }
        public string Name { get; private set; }
        public string TypeName { get; private set; }
        public string Value { get; private set; }
        public string Class { get; private set; }
        public string Text { get; private set; }
        public NodePropertyAttributes Attributes { get; private set; }
        public NodePropertyType Type { get; private set; }
    }
}
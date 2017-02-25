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

using System;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    internal sealed class NodeBacktraceVariable : INodeVariable
    {
        public NodeBacktraceVariable(NodeStackFrame stackFrame, JToken parameter)
        {
            Utilities.ArgumentNotNull("stackFrame", stackFrame);
            Utilities.ArgumentNotNull("parameter", parameter);

            var value = parameter["value"];
            this.Id = (int)value["ref"];
            this.Parent = null;
            this.StackFrame = stackFrame;
            this.Name = (string)parameter["name"] ?? NodeVariableType.AnonymousVariable;
            this.TypeName = (string)value["type"];
            this.Value = (string)value["value"];
            this.Class = (string)value["className"];
            try
            {
                this.Text = (string)value["text"];
            }
            catch (ArgumentException)
            {
                this.Text = String.Empty;
            }
            this.Attributes = NodePropertyAttributes.None;
            this.Type = NodePropertyType.Normal;
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
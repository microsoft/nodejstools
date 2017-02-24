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

using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.Serialization;

namespace NodejsTests.Mocks
{
    internal class MockNodeVariable : INodeVariable
    {
        public int Id { get; set; }
        public NodeEvaluationResult Parent { get; set; }
        public NodeStackFrame StackFrame { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public string Class { get; set; }
        public string Text { get; set; }
        public NodePropertyAttributes Attributes { get; set; }
        public NodePropertyType Type { get; set; }
    }
}
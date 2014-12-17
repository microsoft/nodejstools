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

namespace Microsoft.NodejsTools.Debugger.Serialization {
    /// <summary>
    /// Contains VS type aliases for v8 types.
    /// </summary>
    sealed class NodeVariableType {
        public const string Unknown = "Unknown";
        public const string Undefined = "Undefined";
        public const string Null = "Null";
        public const string Number = "Number";
        public const string Boolean = "Boolean";
        public const string Regexp = "Regular Expression";
        public const string Function = "Function";
        public const string String = "String";
        public const string Object = "Object";
        public const string Error = "Error";
        public const string AnonymousFunction = "(anonymous function)";
        public const string AnonymousVariable = "(anonymous variable)";
        public const string UnknownModule = "<unknown>";
        public const string Prototype = "__proto__";
    }
}
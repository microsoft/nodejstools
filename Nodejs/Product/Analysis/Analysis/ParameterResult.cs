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
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Analysis {
    [Serializable]
    internal class ParameterResult : IEquatable<ParameterResult> {
        public string Name { get; private set; }
        public string Documentation { get; private set; }
        public string Type { get; private set; }
        public bool IsOptional { get; private set; }
        public IEnumerable<IAnalysisVariable> Variables { get; private set; }

        public ParameterResult(string name)
            : this(name, String.Empty, "object") {
        }
        public ParameterResult(string name, string doc)
            : this(name, doc, "object") {
        }
        public ParameterResult(string name, string doc, string type)
            : this(name, doc, type, false) {
        }
        public ParameterResult(string name, string doc, string type, bool isOptional)
            : this(name, doc, type, isOptional, null) {
        }
        public ParameterResult(string name, string doc, string type, bool isOptional, IEnumerable<IAnalysisVariable> variable) :
            this(name, doc, type, isOptional, variable, null) {
        }
        public ParameterResult(string name, string doc, string type, bool isOptional, IEnumerable<IAnalysisVariable> variable, string defaultValue) {
            Name = name;
            Documentation = doc;
            Type = type;
            Variables = variable;
            IsOptional = isOptional;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ParameterResult);
        }

        public override int GetHashCode() {
            return Name.GetHashCode() ^
                (Type ?? "").GetHashCode();
        }

        public bool Equals(ParameterResult other) {
            return other != null &&
                Name == other.Name &&
                Documentation == other.Documentation &&
                Type == other.Type;
        }
    }
}

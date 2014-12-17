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
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class BuiltinObjectValue : ObjectValue {
        private readonly HashSet<string> _immutableMembers = new HashSet<string>();

        public BuiltinObjectValue(ProjectEntry projectEntry, string description = null)
            : base(projectEntry, null, description) {
        }

        public override VariableDef Add(string name, IAnalysisSet value) {
            _immutableMembers.Add(name);
            return base.Add(name, value);
        }

        public override void AddProperty(MemberAddInfo member) {
            _immutableMembers.Add(member.Name);
            base.AddProperty(member);
        }

        public override bool IsMutable(string name) {
            return !_immutableMembers.Contains(name);
        }
    }

    [Serializable]
    class BuiltinObjectPrototypeValue : BuiltinObjectValue {

        public BuiltinObjectPrototypeValue(ProjectEntry projectEntry, string description = null)
            : base(projectEntry, description) {
        }

        public override string ToString() {
            return "<Standard Object prototype>";
        }
    }
}

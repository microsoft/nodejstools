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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Represents an environment record for a contiguous set of
    /// statements.  These forward all of their accesses to the
    /// outer environment and therefore have no significant affect
    /// on variable access.  They exist so that we can have interleaving
    /// records which do not forward all of their accesses to the
    /// outermost scope.
    /// </summary>
    [Serializable]
    abstract class StatementEnvironmentRecord : EnvironmentRecord {
        public int _startIndex, _endIndex;

        public StatementEnvironmentRecord(int index, EnvironmentRecord outerScope)
            : base(outerScope) {
            _startIndex = _endIndex = index;
        }

        public override string Name {
            get { return "<statements>"; }
        }

        public override int GetStart(JsAst ast) {
            return _startIndex;
        }

        public override int GetStop(JsAst ast) {
            return _endIndex;
        }

        public int EndIndex {
            set {
                _endIndex = value;
            }
        }

        public override int GetBodyStart(JsAst ast) {
            return _startIndex;
        }
        
        public override IAnalysisSet ThisValue {
            get {
                return Parent.ThisValue;
            }
        }

        public override IEnumerable<KeyValuePair<string, VariableDef>> Variables {
            get { return Parent.Variables; }
        }

        public override IEnumerable<KeyValuePair<string, VariableDef>> LocalVariables {
            get { return new KeyValuePair<string, VariableDef>[0]; }
        }

        public override bool TryGetVariable(string name, out VariableDef variable) {
            return Parent.TryGetVariable(name, out variable);
        }

        public override bool ContainsVariable(string name) {
            return Parent.ContainsVariable(name);
        }

        public override VariableDef GetVariable(string name) {
            return Parent.GetVariable(name);
        }

        public override VariableDef GetVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return Parent.GetVariable(node, unit, name, addRef);
        }

        public override VariableDef CreateVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return Parent.CreateVariable(node, unit, name, addRef);
        }

        public override VariableDef CreateEphemeralVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return Parent.CreateEphemeralVariable(node, unit, name, addRef);
        }

        public override VariableDef GetOrAddVariable(string name) {
            return Parent.GetOrAddVariable(name);
        }

        public override VariableDef AddVariable(string name, VariableDef variable = null) {
            return Parent.AddVariable(name, variable);
        }

        internal override bool RemoveVariable(string name) {
            return Parent.RemoveVariable(name);
        }

        internal override bool RemoveVariable(string name, out VariableDef value) {
            return Parent.RemoveVariable(name, out value);
        }

        internal override void ClearVariables() {
            Parent.ClearVariables();
        }

        public override void ClearLinkedVariables() {
            Parent.ClearLinkedVariables();
        }

        internal override HashSet<VariableDef> GetLinkedVariables(string saveName) {
            return Parent.GetLinkedVariables(saveName);
        }

        internal override HashSet<VariableDef> GetLinkedVariablesNoCreate(string saveName) {
            return Parent.GetLinkedVariablesNoCreate(saveName);
        }
    }
}

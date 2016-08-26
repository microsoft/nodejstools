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
using System.Diagnostics;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Implements an environment record that is connected to some lexical scope -
    /// either a module or a function.  That lexical scope corresponds with a single
    /// AST node.
    /// 
    /// These environment records have a large
    /// number of variables and have caches of children scopes and nodes which
    /// are associated with their analysis.
    /// </summary>
    [Serializable]
    abstract class DeclarativeEnvironmentRecord : EnvironmentRecord {
        private readonly AnalysisDictionary<string, VariableDef> _variables;
        private Dictionary<string, HashSet<VariableDef>> _linkedVariables;
        private readonly Node _node;

        public DeclarativeEnvironmentRecord(Node ast, EnvironmentRecord outerScope)
            : base(outerScope) {
            _node = ast;
            _variables = new AnalysisDictionary<string, VariableDef>();
        }
        
        public DeclarativeEnvironmentRecord(EnvironmentRecord outerScope)
            : this(null, outerScope) { }

        protected DeclarativeEnvironmentRecord(DeclarativeEnvironmentRecord cloned, bool isCloned)
            : base(cloned, isCloned) {
            _variables = cloned._variables;
            if (cloned._linkedVariables == null) {
                // linkedVariables could be created later, and we need to share them if it.
                cloned._linkedVariables = new Dictionary<string, HashSet<VariableDef>>();
            }
            _linkedVariables = cloned._linkedVariables;
        }

        /// <summary>
        /// Gets the index in the file/buffer that the scope actually starts on.  This is the index where the colon
        /// is on for the start of the body if we're a function or class definition.
        /// </summary>
        public override int GetBodyStart(JsAst ast) {
            return GetStart(ast);
        }

        /// <summary>
        /// Gets the index in the file/buffer that this scope starts at.  This is the index which includes
        /// the definition it's self (e.g. def fob(...) or class fob(...)).
        /// </summary>
        public sealed override int GetStart(JsAst ast) {
            if (_node == null) {
                return 1;
            }
            return _node.GetStart(ast.LocationResolver).Index;
        }

        /// <summary>
        /// Gets the index in the file/buffer that this scope ends at.
        /// </summary>
        public sealed override int GetStop(JsAst ast) {
            if (_node == null) {
                return int.MaxValue;
            }
            return _node.GetEnd(ast.LocationResolver).Index;
        }

        public Node Node {
            get {
                return _node;
            }
        }

        #region Variable Access

        public sealed override IEnumerable<KeyValuePair<string, VariableDef>> LocalVariables {
            get {
                return _variables;
            }
        }

        public sealed override IEnumerable<KeyValuePair<string, VariableDef>> Variables {
            get {
                return _variables;
            }
        }

        public sealed override bool TryGetVariable(string name, out VariableDef variable) {
            return _variables.TryGetValue(name, out variable);
        }

        public sealed override VariableDef GetVariable(string name) {
            Debug.Assert(_variables.ContainsKey(name));
            return _variables[name];
        }

        public sealed override bool ContainsVariable(string name) {
            return _variables.ContainsKey(name);
        }

        public sealed override VariableDef GetVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            VariableDef res;
            if (_variables.TryGetValue(name, out res)) {
                if (addRef) {
                    res.AddReference(node, unit);
                }
                return res;
            }
            return null;
        }

        public sealed override VariableDef CreateVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            var res = GetVariable(node, unit, name, addRef);
            if (res == null) {
                res = AddVariable(name);
            }
            if (addRef) {
                res.AddReference(node, unit);
            }
            return res;
        }

        public sealed override VariableDef CreateEphemeralVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            var res = GetVariable(node, unit, name, addRef);
            if (res == null) {
                res = AddVariable(name, new EphemeralVariableDef());
                if (addRef) {
                    res.AddReference(node, unit);
                }
            }
            return res;
        }

        public sealed override VariableDef GetOrAddVariable(string name) {
            VariableDef res;
            if (_variables.TryGetValue(name, out res)) {
                return res;
            }
            return AddVariable(name);
        }

        public sealed override VariableDef AddVariable(string name, VariableDef variable = null) {
            return _variables[name] = (variable ?? new VariableDef());
        }

        internal sealed override bool RemoveVariable(string name) {
            return _variables.Remove(name);
        }

        internal sealed override void ReplaceVariable(string name, VariableDef def) {
            _variables[name] = def;
        }

        internal sealed override bool RemoveVariable(string name, out VariableDef value) {
            if (_variables.TryGetValue(name, out value)) {
                return _variables.Remove(name);
            }
            value = null;
            return false;
        }

        internal sealed override void ClearVariables() {
            _variables.Clear();
        }

        public sealed override void ClearLinkedVariables() {
            if (_linkedVariables != null) {
                _linkedVariables.Clear();
            }
        }

        internal sealed override HashSet<VariableDef> GetLinkedVariables(string saveName) {
            if (_linkedVariables == null) {
                _linkedVariables = new Dictionary<string, HashSet<VariableDef>>();
            }
            HashSet<VariableDef> links;
            if (!_linkedVariables.TryGetValue(saveName, out links)) {
                _linkedVariables[saveName] = links = new HashSet<VariableDef>();
            }
            return links;
        }

        internal sealed override HashSet<VariableDef> GetLinkedVariablesNoCreate(string saveName) {
            HashSet<VariableDef> linkedVars;
            if (_linkedVariables == null || !_linkedVariables.TryGetValue(saveName, out linkedVars)) {
                return null;
            }
            return linkedVars;
        }

        #endregion

    }
}

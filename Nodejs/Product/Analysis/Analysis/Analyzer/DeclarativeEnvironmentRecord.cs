/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    abstract class DeclarativeEnvironmentRecord : EnvironmentRecord {
        private readonly Dictionary<Node, EnvironmentRecord> _nodeScopes;
        private readonly Dictionary<Node, IAnalysisSet> _nodeValues;
        private readonly Dictionary<string, VariableDef> _variables;
        private Dictionary<string, HashSet<VariableDef>> _linkedVariables;
        private readonly Node _node;

        public DeclarativeEnvironmentRecord(Node ast, EnvironmentRecord outerScope)
            : base(outerScope) {
            _node = ast;
            _nodeScopes = new Dictionary<Node, EnvironmentRecord>();
            _nodeValues = new Dictionary<Node, IAnalysisSet>();
            _variables = new Dictionary<string, VariableDef>();
        }
        
        public DeclarativeEnvironmentRecord(EnvironmentRecord outerScope)
            : this(null, outerScope) { }

        protected DeclarativeEnvironmentRecord(DeclarativeEnvironmentRecord cloned, bool isCloned)
            : base(cloned, isCloned) {
            _nodeScopes = cloned._nodeScopes;
            _nodeValues = cloned._nodeValues;
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
            return _node.GetStart(ast).Index;
        }

        /// <summary>
        /// Gets the index in the file/buffer that this scope ends at.
        /// </summary>
        public sealed override int GetStop(JsAst ast) {
            if (_node == null) {
                return int.MaxValue;
            }
            return _node.GetEnd(ast).Index;
        }

        public Node Node {
            get {
                return _node;
            }
        }

        #region Variable Access

        public sealed override IEnumerable<KeyValuePair<string, VariableDef>> Variables {
            get {
                return _variables;
            }
        }

        public sealed override bool TryGetVariable(string name, out VariableDef variable) {
            return _variables.TryGetValue(name, out variable);
        }

        public sealed override VariableDef GetVariable(string name) {
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

        public override IEnumerable<KeyValuePair<string, VariableDef>> GetAllMergedVariables() {
            return _variables;
        }

        public override IEnumerable<VariableDef> GetMergedVariables(string name) {
            VariableDef res;
            if (_variables.TryGetValue(name, out res)) {
                yield return res;
            }
        }

        public override IAnalysisSet GetMergedVariableTypes(string name) {
            var res = AnalysisSet.Empty;
            foreach (var val in GetMergedVariables(name)) {
                res = res.Union(val.Types);
            }

            return res;
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

        #region Node Environments

        public sealed override IEnumerable<KeyValuePair<Node, EnvironmentRecord>> NodeEnvironments {
            get {
                return _nodeScopes;
            }
        }

        internal sealed override bool TryGetLocalNodeEnvironment(Node node, out EnvironmentRecord scope) {
            return _nodeScopes.TryGetValue(node, out scope);
        }

        public sealed override EnvironmentRecord AddNodeEnvironment(Node node, EnvironmentRecord scope) {
            return _nodeScopes[node] = scope;
        }

        internal sealed override bool RemoveNodeEnvironment(Node node) {
            return _nodeScopes.Remove(node);
        }

        internal sealed override void ClearNodeEnvironments() {
            _nodeScopes.Clear();
        }

        #endregion

        #region Node Values

        public sealed override IAnalysisSet AddNodeValue(Node node, IAnalysisSet variable) {
            return _nodeValues[node] = variable;
        }

        internal sealed override bool RemoveNodeValue(Node node) {
            return _nodeValues.Remove(node);
        }

        internal sealed override void ClearNodeValues() {
            _nodeValues.Clear();
        }

        internal sealed override bool TryGetLocalNodeValue(Node node, out IAnalysisSet variable) {
            return _nodeValues.TryGetValue(node, out variable);
        }

        #endregion
    }
}

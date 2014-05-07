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

        public override AnalysisValue AnalysisValue {
            get { return null; }
        }

        public int EndIndex {
            set {
                _endIndex = value;
            }
        }

        public override int GetBodyStart(JsAst ast) {
            return _startIndex;
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

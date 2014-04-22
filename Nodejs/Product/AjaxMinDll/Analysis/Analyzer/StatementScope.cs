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



using Microsoft.NodejsTools.Parsing;
namespace Microsoft.NodejsTools.Analysis.Analyzer {
    class StatementScope : EnvironmentRecord {
        public int _startIndex, _endIndex;

        public StatementScope(int index, EnvironmentRecord outerScope)
            : base(null, outerScope) {
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

        // Forward variable handling to the outer scope.

        public override VariableDef CreateVariable(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return OuterScope.CreateVariable(node, unit, name, addRef);
        }

        public override VariableDef AddVariable(string name, VariableDef variable = null) {
            return OuterScope.AddVariable(name, variable);
        }

        internal override bool RemoveVariable(string name) {
            return OuterScope.RemoveVariable(name);
        }

        internal override void ClearVariables() {
            OuterScope.ClearVariables();
        }
    }
}

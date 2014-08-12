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
using System.Diagnostics;
using System.Linq;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Performs the 1st pass over the AST to gather all of the classes and
    /// function definitions.
    /// </summary>
    internal partial class OverviewWalker : AstVisitor {
        private EnvironmentRecord _scope;
        private readonly ProjectEntry _entry;
        private readonly Stack<AnalysisUnit> _analysisStack = new Stack<AnalysisUnit>();
        private readonly JsAst _tree;
        private AnalysisUnit _curUnit;
        private Block _curSuite;
        private readonly bool _isNested;

        public OverviewWalker(ProjectEntry entry, AnalysisUnit topAnalysis, JsAst tree, bool isNested = false) {
            _entry = entry;
            _curUnit = topAnalysis;
            _isNested = isNested;
            _tree = tree;

            _scope = topAnalysis.Environment;
        }

        public override bool Walk(FunctionExpression node) {
            if (WalkFunction(node.Function, true)) {
                node.Function.Body.Walk(this);

                EnvironmentRecord funcScope;
                VariableDef funcVarDef;
                if (node.Function.Name != null &&
                    _scope.GlobalEnvironment.TryGetNodeEnvironment(node.Function, out funcScope) &&
                    !funcScope.TryGetVariable(node.Function.Name, out funcVarDef)) {
                    // the function variable gets added if it's not
                    // already declared.
                    var funcDef = funcScope.AddLocatedVariable(
                        node.Function.Name,
                        node.Function,
                        _curUnit
                    );
                    funcDef.AddTypes(
                        _curUnit,
                        ((FunctionEnvironmentRecord)funcScope).Function.Proxy
                    );
                }
                PostWalk(node.Function);
            }
           return false;
        }

        public override bool Walk(FunctionObject node) {
            return WalkFunction(node, false);
        }

        private bool WalkFunction(FunctionObject node, bool isExpression) {
            var functionAnalysis = AddFunction(node, _curUnit, isExpression);
            if (functionAnalysis != null) {
                _analysisStack.Push(_curUnit);
                _curUnit = functionAnalysis;
                Debug.Assert(_scope.EnumerateTowardsGlobal.Contains(functionAnalysis.Environment.Parent));
                _scope = functionAnalysis.Environment;
                return true;
            }

            return false;
        }

        [Serializable]
        internal class ObjectLiteralKey {
            public readonly string[] PropertyNames;
            public readonly int HashCode;

            public ObjectLiteralKey(string[] propNames) {
                PropertyNames = propNames;
                HashCode = 6551;
                for (int i = 0; i < propNames.Length; i++) {
                    if (propNames[i] != null) {
                        HashCode ^= propNames[i].GetHashCode();
                    }
                }
            }

            public override bool Equals(object obj) {
                ObjectLiteralKey other = obj as ObjectLiteralKey;
                if (other != null) {
                    if (other.PropertyNames.Length != PropertyNames.Length) {
                        return false;
                    }

                    for (int i = 0; i < PropertyNames.Length; i++) {
                        if (PropertyNames[i] != other.PropertyNames[i]) {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }

            public override int GetHashCode() {
                return HashCode;
            }
        }

        public override bool Walk(ObjectLiteral node) {
            // If a single file has a bunch of duplicate ObjectLiteral values with the same 
            // property names we will merge them together into a single ObjectLiteralValue to
            // avoid an explosion in object literals.  We only do this for literals with
            // at least one member defined.
            if (node.Properties.Length > 0) {
                // first see if we have a object literal that we should share with...
                string[] propNames = new string[node.Properties.Length];
                for (int i = 0; i < node.Properties.Length; i++) {
                    string propName = null;
                    if (node.Properties[i].Name.Value != null) {
                        propName = node.Properties[i].Name.Value.ToString();
                    }

                    propNames[i] = propName;
                }

                var key = new ObjectLiteralKey(propNames);
                IAnalysisSet value;
                if (_scope.GlobalEnvironment.TryGetNodeValue(
                    NodeEnvironmentKind.ObjectLiteralValue,
                    key,
                    out value)) {
                    // cache the value under our node...
                    _scope.GlobalEnvironment.AddNodeValue(
                        NodeEnvironmentKind.ObjectLiteralValue,
                        node,
                        value
                    );
                } else {
                    // create the value and cache it under oru node and the 
                    // shared key.
                    var objLiteral = new ObjectLiteralValue(_entry, node);

                    _scope.GlobalEnvironment.AddNodeValue(
                        NodeEnvironmentKind.ObjectLiteralValue,
                        node,
                        objLiteral.Proxy
                    );
                    _scope.GlobalEnvironment.AddNodeValue(
                        NodeEnvironmentKind.ObjectLiteralValue,
                        key,
                        objLiteral.Proxy
                    );
                }
            } else {
                _scope.GlobalEnvironment.AddNodeValue(
                    NodeEnvironmentKind.ObjectLiteralValue,
                    node,
                    new ObjectLiteralValue(_entry, node).Proxy
                );
            }

            return base.Walk(node);
        }

        public override void PostWalk(FunctionObject node) {
            if (node.Body != null) {
                Debug.Assert(_scope is DeclarativeEnvironmentRecord && ((DeclarativeEnvironmentRecord)_scope).Node == node);
                Debug.Assert(!(_scope.Parent is DeclarativeEnvironmentRecord) || ((DeclarativeEnvironmentRecord)_scope.Parent).Node != node);
                _scope = _scope.Parent;
                _curUnit = _analysisStack.Pop();
                Debug.Assert(_scope.EnumerateTowardsGlobal.Contains(_curUnit.Environment));
            }
        }

        private VariableDef CreateVariableInDeclaredScope(Lookup name) {
            var reference = name.VariableField;

            if (reference != null) {
                var declNode = reference.Scope;
                var declScope = _scope.EnumerateTowardsGlobal.FirstOrDefault(s => s is DeclarativeEnvironmentRecord && ((DeclarativeEnvironmentRecord)s).Node == declNode);
                if (declScope != null) {
                    return declScope.CreateVariable(name, _curUnit, name.Name, false);
                }
            }

            return _scope.CreateVariable(name, _curUnit, name.Name, false);
        }

        internal FunctionAnalysisUnit AddFunction(FunctionObject node, AnalysisUnit outerUnit, bool isExpression = false) {
            EnvironmentRecord scope;
            if (!_scope.GlobalEnvironment.TryGetNodeEnvironment(node, out scope)) {
                if (node.Body == null) {
                    return null;
                }

                IAnalysisSet functionObj;
                UserFunctionValue func = null;
                if (!_scope.GlobalEnvironment.TryGetNodeValue(NodeEnvironmentKind.UserFunctionValue, node, out functionObj)) {
                    func = CreateUserFunction(node, outerUnit);
                } else {
                    func = (UserFunctionValue)functionObj;
                }

                var funcScope = GetFunctionEnvironment(func);
                scope = funcScope;

                VariableDef[] parameters = new VariableDef[node.ParameterDeclarations != null ? node.ParameterDeclarations.Length : 0];
                for (int i = 0; i < parameters.Length; i++) {
                    parameters[i] = funcScope.AddLocatedVariable(
                        node.ParameterDeclarations[i].Name,
                        node.ParameterDeclarations[i],
                        _curUnit.ProjectEntry
                    );
                }

                _scope.Children.Add(scope);
                _scope.GlobalEnvironment.AddNodeEnvironment(node, scope);

                if (!isExpression && node.Name != null) {
                    // lambdas don't have their names published
                    var funcVar = _scope.AddLocatedVariable(node.Name, node, funcScope.AnalysisUnit);
                    funcVar.AddTypes(funcScope.AnalysisUnit, func.SelfSet);
                }

                funcScope.AnalysisUnit.Enqueue();
            }

            return ((FunctionEnvironmentRecord)scope).AnalysisUnit;
        }

        protected virtual FunctionEnvironmentRecord GetFunctionEnvironment(UserFunctionValue function) {
            return (FunctionEnvironmentRecord)function.AnalysisUnit.Environment;
        }

        private UserFunctionValue CreateUserFunction(FunctionObject node, AnalysisUnit outerUnit) {
            UserFunctionValue func = null;
            FunctionSpecialization specialization;
            var funcName = node.Name ?? node.NameGuess;
            if (funcName != null &&
                _specializations.TryGetValue(funcName, out specialization)) {
                MatchState state = new MatchState(node);
                if (specialization.Body.IsMatch(state, node.Body)) {
                    func = new SpecializedUserFunctionValue(
                        specialization.Specialization,
                        node,
                        outerUnit,
                        _scope,
                        specialization.CallBase
                    );
                }
            }

            if (func == null) {
                func = new UserFunctionValue(node, outerUnit, _scope, _isNested);
            }

            _scope.GlobalEnvironment.AddNodeValue(NodeEnvironmentKind.UserFunctionValue, node, func.Proxy);
            return func;
        }

        private void UpdateChildRanges(Node node) {
            var declScope = _curUnit.Environment;
            var prevScope = declScope.HasChildren ? declScope.Children.Last() : null;
            StatementEnvironmentRecord prevStmtScope;

            if ((prevStmtScope = prevScope as StatementEnvironmentRecord) != null) {
                prevStmtScope.EndIndex = node.GetEndIndex(_tree.LocationResolver);
            } else {
                //declScope.Children.Add(new StatementEnvironmentRecord(node.StartIndex, declScope));
            }
        }

        internal static KeyValuePair<Lookup, Expression>[] GetIsInstanceNamesAndExpressions(Expression node) {
            List<KeyValuePair<Lookup, Expression>> names = null;
            GetIsInstanceNamesAndExpressions(ref names, node);
            if (names != null) {
                return names.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Gets the names which should be in a new scope for isinstance(...) checks.  We don't
        /// use a walker here because we only support a very limited set of assertions (e.g. isinstance(x, type) and ...
        /// or a bare isinstance(...).
        /// </summary>
        internal static void GetIsInstanceNamesAndExpressions(ref List<KeyValuePair<Lookup, Expression>> names, Expression node) {
            CallNode callExpr = node as CallNode;
            if (callExpr != null && callExpr.Arguments.Length == 2) {
                Lookup nameExpr = callExpr.Function as Lookup;
                if (nameExpr != null && nameExpr.Name == "isinstance") {
                    nameExpr = callExpr.Arguments[0] as Lookup;
                    if (nameExpr != null) {
                        if (names == null) {
                            names = new List<KeyValuePair<Lookup, Expression>>();
                        }
                        var type = callExpr.Arguments[1];
                        names.Add(new KeyValuePair<Lookup, Expression>(nameExpr, type));
                    }
                }
            }
        }

        private void PushDefiniteAssignmentEnvironmentRecord(Node node, string name) {
            EnvironmentRecord scope;
            if (!_scope.GlobalEnvironment.TryGetNodeEnvironment(node, out scope)) {
                // find our parent scope, it may not be just the last entry in _scopes
                // because that can be a StatementScope and we would start a new range.
                var declScope = _scope;

                scope = new DefinitiveAssignmentEnvironmentRecord(node.GetEndIndex(_tree.LocationResolver), name, declScope);
                
                declScope.Children.Add(scope);
                declScope.GlobalEnvironment.AddNodeEnvironment(node, scope);
                _scope = scope;
            }
        }

        public override bool Walk(VariableDeclaration node) {
            _scope.AddLocatedVariable(node.Name, node, _curUnit);
            return base.Walk(node);
        }

        public override bool Walk(UnaryOperator node)
        {
          // Delete, etc...
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(BinaryOperator node)
        {
            if (node.OperatorToken == JSToken.Assign) {
                if (node.Operand1 is Lookup) {
                    var declScope = _curUnit.Environment;
                    var prevScope = declScope.HasChildren ? declScope.Children.Last() : null;
                    StatementEnvironmentRecord prevStmtScope;
                    if ((prevStmtScope = prevScope as StatementEnvironmentRecord) != null) {
                        prevStmtScope.EndIndex = node.GetStartIndex(_tree.LocationResolver);
                    }

                    var nameExpr = node.Operand1 as Lookup;
                    PushDefiniteAssignmentEnvironmentRecord(node, nameExpr.Name);

                    _scope.AddVariable(nameExpr.Name, CreateVariableInDeclaredScope(nameExpr));
                } else {
                    UpdateChildRanges(node);
                }
            } else if (node.OperatorToken > JSToken.Assign && node.OperatorToken <= JSToken.LastAssign) {
                UpdateChildRanges(node);
            }
            return true;
        }

        public override bool Walk(Break node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(ContinueNode node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(ForNode node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(IfNode node) {
            UpdateChildRanges(node);
            return true;
        }
      
        public override bool Walk(ThrowNode node) {          
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(ReturnNode node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(TryNode node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(WhileNode node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(WithNode node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }

        public override bool Walk(Block node) {
            var prevScope = _scope;
            var prevSuite = _curSuite;
            _curSuite = node;

            try {
                // recursively walk the statements in the suite
                for (int i = 0; i < node.Count; i++) {
                    if (DDG.IsGwtCode(node, i)) {
                        return false;
                    }
                    node[i].Walk(this);
                }
            } finally {
                _curSuite = prevSuite;
                while (_scope != prevScope) {
                    StatementEnvironmentRecord stmtRec = _scope as StatementEnvironmentRecord;
                    if (stmtRec != null) {
                        stmtRec.EndIndex = node.GetEndIndex(_tree.LocationResolver);
                    }
                    _scope = _scope.Parent;
                }
            }
            return false;
        }

        public override void PostWalk(Block node) {
            base.PostWalk(node);
        }
    }
}

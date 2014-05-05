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
        private AnalysisUnit _curUnit;
        private Block _curSuite;

        public OverviewWalker(ProjectEntry entry, AnalysisUnit topAnalysis) {
            _entry = entry;
            _curUnit = topAnalysis;

            _scope = topAnalysis.Environment;
        }

        public override bool Walk(FunctionExpression node) {
            if (WalkFunction(node.Function, true)) {
                node.Function.Body.Walk(this);

                EnvironmentRecord funcScope;
                VariableDef funcVarDef;
                if (node.Function.Name != null &&
                    _scope.TryGetNodeEnvironment(node.Function, out funcScope) &&
                    !funcScope.TryGetVariable(node.Function.Name, out funcVarDef)) {
                    // the function variable gets added if it's not
                    // already declared.
                    var funcDef = funcScope.AddLocatedVariable(
                        node.Function.Name,
                        node,
                        _curUnit
                    );
                    funcDef.AddTypes(
                        _curUnit,
                        ((FunctionEnvironmentRecord)funcScope).Function
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
            var function = AddFunction(node, _curUnit, isExpression);
            if (function != null) {
                _analysisStack.Push(_curUnit);
                _curUnit = function.AnalysisUnit;
                Debug.Assert(_scope.EnumerateTowardsGlobal.Contains(function.AnalysisUnit.Environment.Parent));
                _scope = function.AnalysisUnit.Environment;
                return true;
            }

            return false;
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

        internal UserFunctionValue AddFunction(FunctionObject node, AnalysisUnit outerUnit, bool isExpression = false) {
            return AddFunction(node, outerUnit, _scope, isExpression);
        }

        internal static UserFunctionValue AddFunction(FunctionObject node, AnalysisUnit outerUnit, EnvironmentRecord prevScope, bool isExpression = false) {
            EnvironmentRecord scope;
            if (!prevScope.TryGetNodeEnvironment(node, out scope)) {
                if (node.Body == null) {
                    return null;
                }

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
                            prevScope,
                            specialization.CallBase
                        );
                    }
                }

                if (func == null) {
                    func = new UserFunctionValue(node, outerUnit, prevScope);
                }

                var unit = func.AnalysisUnit;
                scope = unit.Environment;

                prevScope.Children.Add(scope);
                prevScope.AddNodeEnvironment(node, scope);

                if (!isExpression && node.Name != null) 
                {
                    // lambdas don't have their names published
                    var funcVar = prevScope.AddLocatedVariable(node.Name, node, unit);
                    funcVar.AddTypes(unit, func.SelfSet);
                }

                unit.Enqueue();
            }
            return scope.AnalysisValue as UserFunctionValue;
        }

        private void UpdateChildRanges(Node node) {
            var declScope = _curUnit.Environment;
            var prevScope = declScope.HasChildren ? declScope.Children.Last() : null;
            StatementEnvironmentRecord prevStmtScope;

            if ((prevStmtScope = prevScope as StatementEnvironmentRecord) != null) {
                prevStmtScope.EndIndex = node.EndIndex;
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
            if (callExpr != null && callExpr.Arguments.Count == 2) {
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
            if (!_scope.TryGetNodeEnvironment(node, out scope)) {
                // find our parent scope, it may not be just the last entry in _scopes
                // because that can be a StatementScope and we would start a new range.
                var declScope = _scope;

                scope = new DefinitiveAssignmentEnvironmentRecord(node.StartIndex, name, declScope);
                
                declScope.Children.Add(scope);
                declScope.AddNodeEnvironment(node, scope);
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
                        prevStmtScope.EndIndex = node.StartIndex;
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
                        stmtRec.EndIndex = node.EndIndex;
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

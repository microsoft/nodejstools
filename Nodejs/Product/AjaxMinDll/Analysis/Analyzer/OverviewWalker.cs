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
    internal class OverviewWalker : AstVisitor {
        private EnvironmentRecord _scope;
        private readonly ProjectEntry _entry;
        private readonly Stack<AnalysisUnit> _analysisStack = new Stack<AnalysisUnit>();
        private AnalysisUnit _curUnit;
        private Block _curSuite;

        public OverviewWalker(ProjectEntry entry, AnalysisUnit topAnalysis) {
            _entry = entry;
            _curUnit = topAnalysis;

            _scope = topAnalysis.Scope;
        }

#if FALSE
        // TODO: What about names being redefined?
        // remember classes/functions as they start new scopes
        public override bool Walk(ClassDefinition node) {
            var cls = AddClass(node, _curUnit);
            if (cls != null) {
                _analysisStack.Push(_curUnit);
                _curUnit = cls.AnalysisUnit;
                Debug.Assert(_scope.EnumerateTowardsGlobal.Contains(cls.AnalysisUnit.Scope.OuterScope));
                _scope = cls.AnalysisUnit.Scope;
                return true;
            }
            return false;
        }

        internal ClassInfo AddClass(ClassDefinition node, AnalysisUnit outerUnit) {
            InterpreterScope scope;
            var declScope = outerUnit.Scope;
            if (!declScope.TryGetNodeScope(node, out scope)) {
                if (node.Body == null || node.Name == null) {
                    return null;
                }

                var unit = new ClassAnalysisUnit(node, declScope, outerUnit);
                var classScope = (ClassScope)unit.Scope;

                var classVar = declScope.AddLocatedVariable(node.Name, node.Lookup, unit);
                classVar.AddTypes(unit, classScope.Class.SelfSet);

                declScope.Children.Add(classScope);
                declScope.AddNodeScope(node, classScope);

                unit.Enqueue();
                scope = classScope;
            }
            return scope.AnalysisValue as ClassInfo;
        }

        public override void PostWalk(ClassDefinition node) {
            if (node.Body != null && node.Name != null) {
                Debug.Assert(_scope.Node == node);
                Debug.Assert(_scope.OuterScope.Node != node);
                _scope = _scope.OuterScope;
                _curUnit = _analysisStack.Pop();
                Debug.Assert(_scope.EnumerateTowardsGlobal.Contains(_curUnit.Scope));
            }
        }
#endif

        public override bool Walk(FunctionObject node) {
            var function = AddFunction(node, _curUnit);
            if (function != null) {
                _analysisStack.Push(_curUnit);
                _curUnit = function.AnalysisUnit;
                Debug.Assert(_scope.EnumerateTowardsGlobal.Contains(function.AnalysisUnit.Scope.OuterScope));
                _scope = function.AnalysisUnit.Scope;
                return true;
            }
            return false;
        }

        public override void PostWalk(FunctionObject node)
        {
            if (node.Body != null) {
                Debug.Assert(_scope.Node == node);
                Debug.Assert(_scope.OuterScope.Node != node);
                _scope = _scope.OuterScope;
                _curUnit = _analysisStack.Pop();
                Debug.Assert(_scope.EnumerateTowardsGlobal.Contains(_curUnit.Scope));
            }
        }

#if FALSE
        public override bool Walk(GlobalStatement node) {
            foreach (var name in node.Names) {
                if (name.Name != null) {
                    // set the variable in the local scope to be the real variable in the global scope
                    _scope.AddVariable(name.Name, _scope.GlobalScope.CreateVariable(node, _curUnit, name.Name, false));
                }
            }
            return false;
        }



        public override bool Walk(NonlocalStatement node) {
            foreach (var name in node.Names) {
                if (name.Name != null) {
                    _scope.AddVariable(name.Name, CreateVariableInDeclaredScope(name));
                }
            }
            return false;
        }
#endif
        private VariableDef CreateVariableInDeclaredScope(Lookup name) {

            var reference = name.VariableField;// name.GetVariableReference(_entry.Tree);

            if (reference != null) {
                var declNode = reference.OwningScope;
                var declScope = _scope.EnumerateTowardsGlobal.FirstOrDefault(s => s.Node != null && s.Node.EnclosingScope == declNode);
                if (declScope != null) {
                    return declScope.CreateVariable(name, _curUnit, name.Name, false);
                }
            }

            return _scope.CreateVariable(name, _curUnit, name.Name, false);
        }

        internal UserFunctionValue AddFunction(FunctionObject node, AnalysisUnit outerUnit) {
            return AddFunction(node, outerUnit, _scope);
        }

        internal static UserFunctionValue AddFunction(FunctionObject node, AnalysisUnit outerUnit, EnvironmentRecord prevScope) {
            EnvironmentRecord scope;
            if (!prevScope.TryGetNodeScope(node, out scope)) {
                if (node.Body == null) {
                    return null;
                }

                var func = new UserFunctionValue(node, outerUnit, prevScope);
                var unit = func.AnalysisUnit;
                scope = unit.Scope;

                prevScope.Children.Add(scope);
                prevScope.AddNodeScope(node, scope);

                if (node.Name != null) 
                {
                    // lambdas don't have their names published

                    var funcVar = prevScope.AddLocatedVariable(node.Name, node.NameContext, unit);
                      funcVar.AddTypes(unit, func.SelfSet);
                }

                unit.Enqueue();
            }
            return scope.AnalysisValue as UserFunctionValue;
        }

#if FALSE
        public override bool Walk(GeneratorExpression node) {
            EnsureComprehensionScope(node, MakeGeneratorComprehensionScope);
            Debug.Assert(_scope is ComprehensionScope);

            base.Visit(node);
        }

        public override void PostWalk(GeneratorExpression node) {
            Debug.Assert(_scope is ComprehensionScope);
            _scope = _scope.OuterScope;

            base.PostWalk(node);
        }

        public override bool Walk(ListComprehension node) {
            // List comprehension runs in a new scope in 3.x, runs in the same
            // scope in 2.x.  But these don't get their own analysis units
            // because they are still just expressions.
            if (_curUnit.ProjectState.LanguageVersion.Is3x()) {
                EnsureComprehensionScope(node, MakeListComprehensionScope);
            }

            base.Visit(node);
        }

        public override void PostWalk(ListComprehension node) {
            if (_curUnit.ProjectState.LanguageVersion.Is3x()) {
                Debug.Assert(_scope is ComprehensionScope);
                _scope = _scope.OuterScope;
            }
            base.PostWalk(node);
        }

        public override bool Walk(SetComprehension node) {
            EnsureComprehensionScope(node, MakeSetComprehensionScope);
            Debug.Assert(_scope is ComprehensionScope);

            base.Visit(node);
        }

        public override void PostWalk(SetComprehension node) {
            Debug.Assert(_scope is ComprehensionScope);
            _scope = _scope.OuterScope;

            base.PostWalk(node);
        }

        public override bool Walk(DictionaryComprehension node) {
            EnsureComprehensionScope(node, MakeDictComprehensionScope);
            Debug.Assert(_scope is ComprehensionScope);

            base.Visit(node);
        }

        public override void PostWalk(DictionaryComprehension node) {
            Debug.Assert(_scope is ComprehensionScope);
            _scope = _scope.OuterScope;

            base.PostWalk(node);
        }

        /// <summary>
        /// Makes sure we create a scope for a comprehension (generator, set, dict, or list comprehension in 3.x) where
        /// the variables which are assigned will be stored.  
        /// </summary>
        private void EnsureComprehensionScope(Comprehension node, Func<Comprehension, ComprehensionScope> makeScope) {
            InterpreterScope scope, declScope = _scope;
            if (!declScope.TryGetNodeScope(node, out scope)) {
                scope = makeScope(node);
                
                declScope.AddNodeScope(node, scope);
                declScope.Children.Add(scope);
            }
            _scope = scope;
        }

        private ComprehensionScope MakeGeneratorComprehensionScope(Comprehension node) {
            var unit = new GeneratorComprehensionAnalysisUnit(node, _entry.Tree, _curUnit, _scope);
            unit.Enqueue();
            return (ComprehensionScope)unit.Scope;
        }

        private ComprehensionScope MakeListComprehensionScope(Comprehension node) {
            var unit = new ListComprehensionAnalysisUnit(node, _entry.Tree, _curUnit, _scope);
            unit.Enqueue();
            return (ComprehensionScope)unit.Scope;
        }

        private ComprehensionScope MakeSetComprehensionScope(Comprehension node) {
            var unit = new SetComprehensionAnalysisUnit(node, _entry.Tree, _curUnit, _scope);
            unit.Enqueue();
            return (ComprehensionScope)unit.Scope;
        }

        private ComprehensionScope MakeDictComprehensionScope(Comprehension node) {
            var unit = new DictionaryComprehensionAnalysisUnit(node, _entry.Tree, _curUnit, _scope);
            unit.Enqueue();
            return (ComprehensionScope)unit.Scope;
        }
#endif

        private void UpdateChildRanges(Node node) {
            var declScope = _curUnit.Scope;
            var prevScope = declScope.Children.LastOrDefault();
            StatementScope prevStmtScope;
#if FALSE
            IsInstanceScope prevInstanceScope;
#endif

            if ((prevStmtScope = prevScope as StatementScope) != null) {
                prevStmtScope.EndIndex = node.EndIndex;
#if FALSE
            } else if ((prevInstanceScope = prevScope as IsInstanceScope) != null) {
                prevInstanceScope.EndIndex = node.EndIndex;
#endif
            } else {
                declScope.Children.Add(new StatementScope(node.StartIndex, declScope));
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
#if FALSE
            AndExpression andExpr = node as AndExpression;
            OrExpression orExpr = node as OrExpression;
            if (andExpr != null) {
                GetIsInstanceNamesAndExpressions(ref names, andExpr.Left);
                GetIsInstanceNamesAndExpressions(ref names, andExpr.Right);
            } else if (orExpr != null) {
                GetIsInstanceNamesAndExpressions(ref names, orExpr.Left);
                GetIsInstanceNamesAndExpressions(ref names, orExpr.Right);
            }
#endif
        }

#if FALSE
        private void PushIsInstanceScope(Node node, KeyValuePair<Lookup, Expression>[] isInstanceNames, SuiteStatement effectiveSuite) {
            InterpreterScope scope;
            if (!_curUnit.Scope.TryGetNodeScope(node, out scope)) {
                if (_scope is IsInstanceScope) {
                    // Reuse the current scope
                    _curUnit.Scope.AddNodeScope(node, _scope);
                    return;
                }

                // find our parent scope, it may not be just the last entry in _scopes
                // because that can be a StatementScope and we would start a new range.
                var declScope = _scope.EnumerateTowardsGlobal.FirstOrDefault(s => !(s is StatementScope));

                scope = new IsInstanceScope(node.StartIndex, effectiveSuite, declScope);

                declScope.Children.Add(scope);
                declScope.AddNodeScope(node, scope);
                _scope = scope;
            }
        }
#endif

        public override bool Walk(VariableDeclaration node) {
            _scope.AddVariable(node.Name);
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
                    var nameExpr = node.Operand1 as Lookup;
                    _scope.AddVariable(nameExpr.Name, CreateVariableInDeclaredScope(nameExpr));
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
#if FALSE
        public override bool Walk(EmptyStatement node) {
            UpdateChildRanges(node);
            base.Visit(node);
        }

        public override bool Walk(ExpressionStatement node) {
            UpdateChildRanges(node);
            base.Visit(node);
        }

        public override bool Walk(ExecStatement node) {
            UpdateChildRanges(node);
            base.Visit(node);
        }
#endif
        public override bool Walk(ForNode node) {
            UpdateChildRanges(node);
            return base.Walk(node);
        }
#if FALSE
        public override bool Walk(FromImportStatement node) {
            UpdateChildRanges(node);
            
            var asNames = node.AsNames ?? node.Names;
            int len = Math.Min(node.Names.Count, asNames.Count);
            for (int i = 0; i < len; i++) {
                var nameNode = asNames[i] ?? node.Names[i];
                if (nameNode != null) {
                    if (nameNode.Name == "*") {
                        _scope.ContainsImportStar = true;
                    } else {
                        CreateVariableInDeclaredScope(nameNode);
                    }
                }
            }
            base.Visit(node);
        }
#endif
        public override bool Walk(IfNode node) {
            UpdateChildRanges(node);
#if FALSE
            if (node.Tests != null) {
                foreach (var test in node.Tests) {
                    var isInstanceNames = GetIsInstanceNamesAndExpressions(test.Test);
                    if (isInstanceNames != null) {
                        if (test.Test != null) {
                            test.Test.Walk(this);
                        }

                        if (test.Body != null && !(test.Body is ErrorStatement)) {
                            Debug.Assert(test.Body is SuiteStatement);

                            PushIsInstanceScope(test, isInstanceNames, (SuiteStatement)test.Body);

                            test.Body.Walk(this);
                        }
                    } else {
                        test.Walk(this);
                    }
                }
            }
            if (node.ElseStatement != null) {
                node.ElseStatement.Walk(this);
            }
            return false;
#endif
            return true;
        }

#if FALSE
        public override bool Walk(ImportStatement node) {
            for (int i = 0; i < node.Names.Count; i++) {
                Lookup name = null;
                if (i < node.AsNames.Count && node.AsNames[i] != null) {
                    name = node.AsNames[i];
                } else if (node.Names[i].Names.Count > 0) {
                    name = node.Names[i].Names[0];
                }

                if (name != null) {
                    CreateVariableInDeclaredScope(name);
                }
            }

            UpdateChildRanges(node);
            base.Visit(node);
        }

        public override bool Walk(PrintStatement node) {
            UpdateChildRanges(node);
            base.Visit(node);
        }
#endif
      
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
#if FALSE
            foreach (var item in node.Items) {
                var assignTo = item.Variable as Lookup;
                if (assignTo != null) {
                    _scope.AddVariable(assignTo.Name, CreateVariableInDeclaredScope(assignTo));
                }
            }
            base.Visit(node);
#endif
        }

        public override bool Walk(Block node) {
            var prevScope = _scope;
            var prevSuite = _curSuite;
            _curSuite = node;

            // recursively walk the statements in the suite
            foreach (var innerNode in node.Children) {
                innerNode.Walk(this);
            }
            

            _curSuite = prevSuite;
            _scope = prevScope;
#if FALSE
            // then check if we encountered an assert which added an isinstance scope.
            IsInstanceScope isInstanceScope = _scope as IsInstanceScope;
            if (isInstanceScope != null && isInstanceScope._effectiveSuite == node) {
                // pop the isinstance scope
                _scope = _scope.OuterScope;
                var declScope = _curUnit.Scope;
                // transform back into a line number and start the new statement scope on the line
                // after the suite statement.
                var lineNo = _entry.Tree.IndexToLocation(node.EndIndex).Line;

                int offset;
                if (_entry.Tree._lineLocations.Length == 0) {
                    // single line input
                    offset = 0;
                } else {
                    offset = lineNo < _entry.Tree._lineLocations.Length ? _entry.Tree._lineLocations[lineNo] : _entry.Tree._lineLocations[_entry.Tree._lineLocations.Length - 1];
                }
                var closingScope = new StatementScope(offset, declScope);
                _scope = closingScope;
                declScope.Children.Add(closingScope);
            }
#endif
            return false;
        }

        public override void PostWalk(Block node) {
            while (_scope is StatementScope) {
                _scope = _scope.OuterScope;
            }
            base.PostWalk(node);
        }
    }
}

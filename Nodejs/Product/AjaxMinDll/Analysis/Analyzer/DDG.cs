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
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    internal class DDG : AstVisitor {
        internal AnalysisUnit _unit;
        internal ExpressionEvaluator _eval;
        private Block _curSuite;

        public void Analyze(Deque<AnalysisUnit> queue, CancellationToken cancel, Action<int> reportQueueSize = null, int reportQueueInterval = 1) {
            if (cancel.IsCancellationRequested) {
                return;
            }
            try {
                // Including a marker at the end of the queue allows us to see in
                // the log how frequently the queue empties.
                var endOfQueueMarker = new AnalysisUnit(null, null);
                int queueCountAtStart = queue.Count;
                int reportInterval = reportQueueInterval - 1;

                if (queueCountAtStart > 0) {
                    queue.Append(endOfQueueMarker);
                }

                while (queue.Count > 0 && !cancel.IsCancellationRequested) {
                    _unit = queue.PopLeft();

                    if (_unit == endOfQueueMarker) {
                        AnalysisLog.EndOfQueue(queueCountAtStart, queue.Count);
                        if (reportInterval < 0 && reportQueueSize != null) {
                            reportQueueSize(queue.Count);
                        }

                        queueCountAtStart = queue.Count;
                        if (queueCountAtStart > 0) {
                            queue.Append(endOfQueueMarker);
                        }
                        continue;
                    }

                    AnalysisLog.Dequeue(queue, _unit);
                    if (reportInterval == 0 && reportQueueSize != null) {
                        reportQueueSize(queue.Count);
                        reportInterval = reportQueueInterval - 1;
                    } else if (reportInterval > 0) {
                        reportInterval -= 1;
                    }

                    _unit.IsInQueue = false;
                    SetCurrentUnit(_unit);
                    _unit.Analyze(this, cancel);
                }

                if (reportQueueSize != null) {
                    reportQueueSize(0);
                }

                if (cancel.IsCancellationRequested) {
                    AnalysisLog.Cancelled(queue);
                }
            } finally {
                AnalysisLog.Flush();
            }
        }

        public void SetCurrentUnit(AnalysisUnit unit) {
            _eval = new ExpressionEvaluator(unit);
            _unit = unit;
        }

        public EnvironmentRecord Scope {
            get {
                return _eval.Scope;
            }
            set {
                _eval.Scope = value;
            }
        }

        public ModuleInfo GlobalScope {
            get { return _unit.DeclaringModule; }
        }

        public JsAnalyzer ProjectState {
            get { return _unit.Analyzer; }
        }

        public override bool Walk(JsAst node) {
#if FALSE
            ModuleReference existingRef;
#endif
            //Debug.Assert(node == _unit.Ast);
#if FALSE
            if (!ProjectState.Modules.TryGetValue(_unit.DeclaringModule.Name, out existingRef)) {
                // publish our module ref now so that we don't collect dependencies as we'll be fully processed
                ProjectState.Modules[_unit.DeclaringModule.Name] = new ModuleReference(_unit.DeclaringModule);
            }
#endif

            return base.Walk(node);
        }

        /// <summary>
        /// Gets the function which we are processing code for currently or
        /// null if we are not inside of a function body.
        /// </summary>
        public FunctionScope CurrentFunction {
            get { return CurrentContainer<FunctionScope>(); }
        }

#if FALSE
        public ClassScope CurrentClass {
            get { return CurrentContainer<ClassScope>(); }
        }
#endif
        private T CurrentContainer<T>() where T : EnvironmentRecord {
            return Scope.EnumerateTowardsGlobal.OfType<T>().FirstOrDefault();
        }

        public override bool Walk(ExpressionStatement node) {
            _eval.Evaluate(node.Expression);
            return false;
        }

        public override bool Walk(VariableDeclaration node) {
            if (node.Initializer != null) {
                _eval.Scope.AssignVariable(
                    node.Name,
                    node,
                    _unit,
                    _eval.Evaluate(node.Initializer)
                );
            }
            return false;
        }

        public override bool Walk(Var node) {
            return base.Walk(node);
        }

        public override bool Walk(ForNode node) {
            if (node.Initializer != null) {
                node.Initializer.Walk(this);
            }
            if (node.Body != null) {
                node.Body.Walk(this);
            }
            if (node.Incrementer != null) {
                _eval.Evaluate(node.Incrementer);
            }
            if (node.Condition != null) {
                _eval.Evaluate(node.Condition);
            }
            return false;
        }
#if FALSE
        private bool TryImportModule(string modName, bool forceAbsolute, out ModuleReference moduleRef) {
            if (ProjectState.Limits.CrossModule != null &&
                ProjectState.ModulesByFilename.Count > ProjectState.Limits.CrossModule) {
                // too many modules loaded, disable cross module analysis by blocking
                // scripts from seeing other modules.
                moduleRef = null;
                return false;
            }

            foreach (var name in PythonAnalyzer.ResolvePotentialModuleNames(_unit.ProjectEntry, modName, forceAbsolute)) {
                if (ProjectState.Modules.TryGetValue(name, out moduleRef)) {
                    return true;
                }
            }

            _unit.DeclaringModule.AddUnresolvedModule(modName, forceAbsolute);

            moduleRef = null;
            return false;
        }

        internal List<AnalysisValue> LookupBaseMethods(string name, IEnumerable<IAnalysisSet> bases, Node node, AnalysisUnit unit) {
            var result = new List<AnalysisValue>();
            foreach (var b in bases) {
                foreach (var curType in b) {
                    BuiltinClassInfo klass = curType as BuiltinClassInfo;
                    if (klass != null) {
                        var value = klass.GetMember(node, unit, name);
                        if (value != null) {
                            result.AddRange(value);
                        }
                    }
                }
            }
            return result;
        }
#endif

        public override bool Walk(FunctionObject node) {
            EnvironmentRecord funcScope;
            if (_unit.Scope.TryGetNodeScope(node, out funcScope)) {
                var function = ((FunctionScope)funcScope).Function;
                var analysisUnit = (FunctionAnalysisUnit)((FunctionScope)funcScope).Function.AnalysisUnit;
#if FALSE
                var curClass = Scope as ClassScope;
                if (curClass != null) {
                    var bases = LookupBaseMethods(
                        analysisUnit.Ast.Name,
                        curClass.Class.Mro,
                        analysisUnit.Ast,
                        analysisUnit
                    );
                    foreach (var method in bases.OfType<BuiltinMethodInfo>()) {
                        foreach (var overload in method.Function.Overloads) {
                            function.UpdateDefaultParameters(_unit, overload.GetParameters());
                        }
                    }
                }
#endif
            }
            return false;
        }

        internal void WalkBody(Node node, AnalysisUnit unit) {
            var oldUnit = _unit;
            var eval = _eval;
            _unit = unit;
            _eval = new ExpressionEvaluator(unit);
            try {
                node.Walk(this);
            } finally {
                _unit = oldUnit;
                _eval = eval;
            }
        }

        public override bool Walk(IfNode node) {
            _eval.Evaluate(node.Condition);
            //TryPushIsInstanceScope(test, test.Test);
            node.TrueBlock.Walk(this);

            if (node.FalseBlock != null) {
                node.FalseBlock.Walk(this);
            }
            return true;
        }

#if FALSE
        public override bool Walk(ImportStatement node) {
            int len = Math.Min(node.Names.Count, node.AsNames.Count);
            for (int i = 0; i < len; i++) {
                var curName = node.Names[i];
                var asName = node.AsNames[i];

                string importing, saveName;
                Node nameNode;
                if (curName.Names.Count == 0) {
                    continue;
                } else if (curName.Names.Count > 1) {
                    // import fob.oar
                    if (asName != null) {
                        // import fob.oar as baz, baz becomes the value of the oar module
                        importing = curName.MakeString();
                        saveName = asName.Name;
                        nameNode = asName;
                    } else {
                        // plain import fob.oar, we bring in fob into the scope
                        saveName = importing = curName.Names[0].Name;
                        nameNode = curName.Names[0];
                    }
                } else {
                    // import fob
                    importing = curName.Names[0].Name;
                    if (asName != null) {
                        saveName = asName.Name;
                        nameNode = asName;
                    } else {
                        saveName = importing;
                        nameNode = curName.Names[0];
                    }
                }

                ModuleReference modRef;

                var def = Scope.CreateVariable(nameNode, _unit, saveName);
                if (TryImportModule(importing, node.ForceAbsolute, out modRef)) {
                    modRef.AddReference(_unit.DeclaringModule);

                    Debug.Assert(modRef.Module != null);
                    if (modRef.Module != null) {
                        modRef.Module.Imported(_unit);

                        if (modRef.AnalysisModule != null) {
                            def.AddTypes(_unit, modRef.AnalysisModule);
                        }
                        def.AddAssignment(nameNode, _unit);
                    }
                }
            }
            return true;
        }
#endif
        public override bool Walk(ReturnNode node) {
            var fnScope = CurrentFunction;
            if (node.Operand != null && fnScope != null) {
                var lookupRes = _eval.Evaluate(node.Operand);
                fnScope.AddReturnTypes(node, _unit, lookupRes);
            }
            return true;
        }

#if FALSE
        public override bool Walk(WithStatement node) {
            foreach (var item in node.Items) {
                var ctxMgr = _eval.Evaluate(item.ContextManager);
                if (item.Variable != null) {
                    _eval.AssignTo(node, item.Variable, ctxMgr);
                }
            }

            return true;
        }

        public override bool Walk(PrintStatement node) {
            foreach (var expr in node.Expressions) {
                _eval.Evaluate(expr);
            }
            return false;
        }

        public override bool Walk(AssertStatement node) {
            TryPushIsInstanceScope(node, node.Test);

            _eval.EvaluateMaybeNull(node.Test);
            _eval.EvaluateMaybeNull(node.Message);
            return false;
        }

        private void TryPushIsInstanceScope(Node node, Expression test) {
            InterpreterScope newScope;
            if (Scope.TryGetNodeScope(node, out newScope)) {
                var outerScope = Scope;
                var isInstanceScope = (IsInstanceScope)newScope;

                // magic assert isinstance statement alters the type information for a node
                var namesAndExpressions = OverviewWalker.GetIsInstanceNamesAndExpressions(test);
                foreach (var nameAndExpr in namesAndExpressions) {
                    var name = nameAndExpr.Key;
                    var type = nameAndExpr.Value;

                    var typeObj = _eval.EvaluateMaybeNull(type);
                    isInstanceScope.CreateTypedVariable(name, _unit, name.Name, typeObj);
                }

                // push the scope, it will be popped when we leave the current SuiteStatement.
                Scope = newScope;
            }
        }
#endif

        public override bool Walk(Block node) {
            var prevSuite = _curSuite;
            var prevScope = Scope;

            _curSuite = node;
            foreach (var statement in node.Children) {
                statement.Walk(this);
            }

            Scope = prevScope;
            _curSuite = prevSuite;
            return false;
        }
#if FALSE
        public override bool Walk(DelStatement node) {
            foreach (var expr in node.Expressions) {
                DeleteExpression(expr);
            }
            return false;
        }

        private void DeleteExpression(Expression expr) {
            Lookup name = expr as Lookup;
            if (name != null) {
                var var = Scope.CreateVariable(name, _unit, name.Name);

                return;
            }

            IndexExpression index = expr as IndexExpression;
            if (index != null) {
                var values = _eval.Evaluate(index.Target);
                var indexValues = _eval.Evaluate(index.Index);
                foreach (var value in values) {
                    value.DeleteIndex(index, _unit, indexValues);
                }
                return;
            }

            MemberExpression member = expr as MemberExpression;
            if (member != null) {
                var values = _eval.Evaluate(member.Target);
                foreach (var value in values) {
                    value.DeleteMember(member, _unit, member.Name);
                }
                return;
            }

            ParenthesisExpression paren = expr as ParenthesisExpression;
            if (paren != null) {
                DeleteExpression(paren.Expression);
                return;
            }

            SequenceExpression seq = expr as SequenceExpression;
            if (seq != null) {
                foreach (var item in seq.Items) {
                    DeleteExpression(item);
                }
                return;
            }
        }
#endif
        public override bool Walk(ThrowNode node) {
            _eval.EvaluateMaybeNull(node.Operand);
            return false;
        }

        public override bool Walk(WhileNode node) {
            _eval.Evaluate(node.Condition);

            node.Body.Walk(this);
            return false;
        }

        public override bool Walk(TryNode node) {
            node.TryBlock.Walk(this);
            if (node.CatchBlock != null) {
                node.CatchBlock.Walk(this);
            }
#if FALSE
            if (node.Handlers != null) {
                foreach (var handler in node.Handlers) {
                    var test = AnalysisSet.Empty;
                    if (handler.Test != null) {
                        var testTypes = _eval.Evaluate(handler.Test);

                        if (handler.Target != null) {
                            foreach (var type in testTypes) {
                                ClassInfo klass = type as ClassInfo;
                                if (klass != null) {
                                    test = test.Union(klass.Instance.SelfSet);
                                }

                                BuiltinClassInfo builtinClass = type as BuiltinClassInfo;
                                if (builtinClass != null) {
                                    test = test.Union(builtinClass.Instance.SelfSet);
                                }
                            }

                            _eval.AssignTo(handler, handler.Target, test);
                        }
                    }

                    handler.Body.Walk(this);
                }
            }
#endif

            if (node.FinallyBlock != null) {
                node.FinallyBlock.Walk(this);
            }
            return false;
        }
    }
}

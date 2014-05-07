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
    internal class ExpressionEvaluator {
        private readonly AnalysisUnit _unit;

        internal static readonly IAnalysisSet[] EmptySets = new IAnalysisSet[0];
        internal static readonly Lookup[] EmptyNames = new Lookup[0];

        /// <summary>
        /// Creates a new ExpressionEvaluator that will evaluate in the context of the top-level module.
        /// </summary>
        public ExpressionEvaluator(AnalysisUnit unit) {
            _unit = unit;
            Scope = unit.Environment;
        }

        public ExpressionEvaluator(AnalysisUnit unit, EnvironmentRecord scope) {
            _unit = unit;
            Scope = scope;
        }

        #region Public APIs

        /// <summary>
        /// Returns possible variable refs associated with the expr in the expression evaluators scope.
        /// </summary>
        public IAnalysisSet Evaluate(Expression node) {
            var res = EvaluateWorker(node);
            Debug.Assert(res != null);
            return res;
        }

        public IAnalysisSet EvaluateMaybeNull(Expression node) {
            if (node == null) {
                return null;
            }

            return Evaluate(node);
        }

        /// <summary>
        /// Returns a sequence of possible types associated with the name in the expression evaluators scope.
        /// </summary>
        public IAnalysisSet LookupAnalysisSetByName(Node node, string name, bool addRef = true) {
            foreach (var scope in Scope.EnumerateTowardsGlobal) {
                var refs = scope.GetVariable(node, _unit, name, addRef);
                if (refs != null) {
                    if (addRef) {
                        var linkedVars = scope.GetLinkedVariablesNoCreate(name);
                        if (linkedVars != null) {
                            foreach (var linkedVar in linkedVars) {
                                linkedVar.AddReference(node, _unit);
                            }
                        }
                    }
                    return refs.Types;
                }
            }

            return ProjectState._globalObject.GetMember(node, _unit, name);
        }

        #endregion

        #region Implementation Details

        private ModuleValue GlobalScope {
            get { return _unit.DeclaringModuleEnvironment.Module; }
        }

        private JsAnalyzer ProjectState {
            get { return _unit.Analyzer; }
        }

        /// <summary>
        /// The list of scopes which define the current context.
        /// </summary>
#if DEBUG
        public EnvironmentRecord Scope {
            get { return _currentScope; }
            set {
                // Scopes must be from a common stack.
                Debug.Assert(_currentScope == null ||
                    _currentScope.Parent == value.Parent ||
                    _currentScope.EnumerateTowardsGlobal.Contains(value) ||
                    value.EnumerateTowardsGlobal.Contains(_currentScope));
                _currentScope = value;
            }
        }

        private EnvironmentRecord _currentScope;
#else
        public EnvironmentRecord Scope;
#endif

        private IAnalysisSet EvaluateWorker(Node node) {
            EvalDelegate eval;
            if (_evaluators.TryGetValue(node.GetType(), out eval)) {
                return eval(this, node);
            }

            return AnalysisSet.Empty;
        }

        delegate IAnalysisSet EvalDelegate(ExpressionEvaluator ee, Node node);

        private static Dictionary<Type, EvalDelegate> _evaluators = new Dictionary<Type, EvalDelegate> {
            { typeof(BinaryOperator), ExpressionEvaluator.EvaluateBinary },
            { typeof(CallNode), ExpressionEvaluator.EvaluateCall },
            { typeof(Conditional), ExpressionEvaluator.EvaluateConditional},
            { typeof(ConstantWrapper), ExpressionEvaluator.EvaluateConstant },
            { typeof(DirectivePrologue), ExpressionEvaluator.EvaluateConstant },
            { typeof(ObjectLiteral), ExpressionEvaluator.EvaluateObjectLiteral },
            { typeof(Member), ExpressionEvaluator.EvaluateMember },
            { typeof(Lookup), ExpressionEvaluator.EvaluateLookup },
            { typeof(GroupingOperator), ExpressionEvaluator.EvaluateGroupingOperator },
            { typeof(UnaryOperator), ExpressionEvaluator.EvaluateUnary },
            { typeof(ArrayLiteral), ExpressionEvaluator.EvaluateArrayLiteral },
            { typeof(FunctionExpression), ExpressionEvaluator.EvaluateFunctionExpression },
            { typeof(ThisLiteral), ExpressionEvaluator.EvaluateThis }
#if FALSE
            { typeof(YieldExpression), ExpressionEvaluator.EvaluateYield },
            { typeof(YieldFromExpression), ExpressionEvaluator.EvaluateYieldFrom },
#endif
        };

        private static IAnalysisSet EvaluateThis(ExpressionEvaluator ee, Node node) {
            return ee.Scope.ThisValue;
        }

        private static IAnalysisSet EvaluateArrayLiteral(ExpressionEvaluator ee, Node node) {
            return ee.MakeArrayValue(ee, node);
        }

        private static IAnalysisSet EvaluateGroupingOperator(ExpressionEvaluator ee, Node node) {
            var n = (GroupingOperator)node;
            return ee.Evaluate(n.Operand);
        }

        private static IAnalysisSet EvaluateLookup(ExpressionEvaluator ee, Node node) {
            var n = (Lookup)node;
            var res = ee.LookupAnalysisSetByName(node, n.Name);
            foreach (var value in res) {
                value.AddReference(node, ee._unit);
            }
            return res;
        }

        private static IAnalysisSet EvaluateMember(ExpressionEvaluator ee, Node node) {
            var n = (Member)node;
            return ee.Evaluate(n.Root).GetMember(node, ee._unit, n.Name);
        }

        private static IAnalysisSet EvaluateIndex(ExpressionEvaluator ee, Node node) {
            var n = (CallNode)node;

            return ee.Evaluate(n.Function).GetIndex(n, ee._unit, ee.Evaluate(GetIndexArgument(n)));
        }

        private static Expression GetIndexArgument(CallNode n) {
            Debug.Assert(n.Arguments.Count == 1);
            var comma = n.Arguments[0] as CommaOperator;
            if (comma != null) {
                return comma.Expressions[comma.Expressions.Length - 1];
            }
            return n.Arguments[0];
        }

        private static IAnalysisSet EvaluateObjectLiteral(ExpressionEvaluator ee, Node node) {
            var n = (ObjectLiteral)node;
            IAnalysisSet result = ee.Scope.GlobalEnvironment.GetOrMakeNodeValue(node, _ => {
                var objectInfo = new ObjectLiteralValue(ee._unit.ProjectEntry, node);
                result = objectInfo.SelfSet;

                if (n.Properties.Count > 30) {
                    // probably some generated object literal, ignore it
                    // for the post part.
                    AssignProperty(ee, node, objectInfo, n.Properties.First());
                } else {
                    foreach (var x in n.Properties) {
                        AssignProperty(ee, node, objectInfo, x);
                    }
                }

                return result;
            });
            return result;
        }

        private static void AssignProperty(ExpressionEvaluator ee, Node node, ObjectLiteralValue objectInfo, ObjectLiteralProperty x) {
            if (x.Name.Value is string) {
                objectInfo.SetMember(
                    node,
                    ee._unit,
                    (string)x.Name.Value,
                    ee.EvaluateMaybeNull(x.Value) ?? AnalysisSet.Empty
                );
            } else {
                // {42:42}
                objectInfo.SetIndex(
                    node,
                    ee._unit,
                    ee.ProjectState.GetConstant(x.Name.Value) ?? AnalysisSet.Empty,
                    ee.EvaluateMaybeNull(x.Value) ?? AnalysisSet.Empty
                );
            }
        }

        private static IAnalysisSet EvaluateConstant(ExpressionEvaluator ee, Node node) {
            var n = (ConstantWrapper)node;

            return ee.ProjectState.GetConstant(n.Value);
        }

        private static IAnalysisSet EvaluateConditional(ExpressionEvaluator ee, Node node) {
            var n = (Conditional)node;
            ee.Evaluate(n.Condition);
            var result = ee.Evaluate(n.TrueExpression);
            return result.Union(ee.Evaluate(n.FalseExpression));
        }

        private static IAnalysisSet EvaluateCall(ExpressionEvaluator ee, Node node) {
            var n = (CallNode)node;
            if (n.InBrackets) {
                return EvaluateIndex(ee, node);
            }

            // Get the argument types that we're providing at this call site
            var argTypes = ee.Evaluate(n.Arguments);

            // Then lookup the possible methods we're calling


            var res = AnalysisSet.Empty;
            if (n.IsConstructor) {
                var targetRefs = ee.Evaluate(n.Function);
                foreach (var target in targetRefs) {
                    res = res.Union(target.Construct(node, ee._unit, argTypes));
                }
            } else {
                IAnalysisSet @this;
                IAnalysisSet targetRefs = ee.EvaluateReference(node, n, out @this);

                foreach (var target in targetRefs) {
                    res = res.Union(target.Call(node, ee._unit, @this, argTypes));
                }

            }
            return res;
        }

        private IAnalysisSet EvaluateReference(Node node, CallNode n, out IAnalysisSet baseValue) {
            Member member = n.Function as Member;
            IAnalysisSet targetRefs;
            if (member != null) {
                baseValue = Evaluate(member.Root);
                @targetRefs = baseValue.GetMember(node, _unit, member.Name);
            } else {
                CallNode call = n.Function as CallNode;
                if (call != null && call.InBrackets && call.Arguments.Count == 1) {
                    baseValue = Evaluate(call.Arguments[0]);
                    targetRefs = Evaluate(call.Function);
                } else {
                    baseValue = null;
                    targetRefs = Evaluate(n.Function);
                }
            }
            return targetRefs;
        }

        private IAnalysisSet[] Evaluate(AstNodeList<Expression> astNodeList) {
            var res = new IAnalysisSet[astNodeList.Count];
            for (int i = 0; i < res.Length; i++) {
                res[i] = Evaluate(astNodeList[i]);
            }
            return res;
        }

        private static IAnalysisSet EvaluateUnary(ExpressionEvaluator ee, Node node) {
            var n = (UnaryOperator)node;
            var operand = ee.Evaluate(n.Operand);
            switch (n.OperatorToken) {
                case JSToken.TypeOf:
                    IAnalysisSet res = AnalysisSet.Empty;
                    foreach (var expr in operand) {
                        string typeName;
                        switch (expr.TypeId) {
                            case BuiltinTypeId.Function: typeName = "function"; break;
                            case BuiltinTypeId.String: typeName = "string"; break;
                            case BuiltinTypeId.Null: typeName = "null"; break;
                            case BuiltinTypeId.Undefined: typeName = "undefined"; break;
                            case BuiltinTypeId.Number: typeName = "number"; break;
                            case BuiltinTypeId.Boolean: typeName = "boolean"; break;
                            default: typeName = "object"; break;
                        }
                        res = res.Union(ee.ProjectState.GetConstant(typeName));
                    }
                    return res;
                case JSToken.Void:
                    return ee._unit.Analyzer._undefined;
            }

            return operand.UnaryOperation(node, ee._unit, n.OperatorToken);
        }

        private static IAnalysisSet EvaluateBinary(ExpressionEvaluator ee, Node node) {
            var n = (BinaryOperator)node;
            switch (n.OperatorToken) {
                case JSToken.LogicalAnd:
                case JSToken.LogicalOr:
                    var result = ee.Evaluate(n.Operand1);
                    return result.Union(ee.Evaluate(n.Operand2));
                case JSToken.PlusAssign:                     // +=
                case JSToken.MinusAssign:                    // -=
                case JSToken.MultiplyAssign:                 // *=
                case JSToken.DivideAssign:                   // /=
                case JSToken.ModuloAssign:                   // %=
                case JSToken.BitwiseAndAssign:               // &=
                case JSToken.BitwiseOrAssign:                // |=
                case JSToken.BitwiseXorAssign:               // ^=
                case JSToken.LeftShiftAssign:                // <<=
                case JSToken.RightShiftAssign:               // >>=
                case JSToken.UnsignedRightShiftAssign:       // >>>=
                    var rightValue = ee.Evaluate(n.Operand2);
                    foreach (var x in ee.Evaluate(n.Operand1)) {
                        x.AugmentAssign(n, ee._unit, rightValue);
                    }
                    return rightValue;
                case JSToken.Assign:
                    var rhs = ee.Evaluate(n.Operand2);
                    ee.AssignTo(n, n.Operand1, rhs);
                    return rhs;
            }

            return ee.Evaluate(n.Operand1).Union(ee.Evaluate(n.Operand2));
        }

#if FALSE
        private static IAnalysisSet EvaluateYield(ExpressionEvaluator ee, Node node) {
            var yield = (YieldExpression)node;
            var scope = ee.Scope as FunctionScope;
            if (scope != null && scope.Generator != null) {
                var gen = scope.Generator;
                var res = ee.Evaluate(yield.Expression);

                gen.AddYield(node, ee._unit, res);

                gen.Sends.AddDependency(ee._unit);
                return gen.Sends.Types;
            }
            return AnalysisSet.Empty;
        }

        private static IAnalysisSet EvaluateYieldFrom(ExpressionEvaluator ee, Node node) {
            var yield = (YieldFromExpression)node;
            var scope = ee.Scope as FunctionScope;
            if (scope != null && scope.Generator != null) {
                var gen = scope.Generator;
                var res = ee.Evaluate(yield.Expression);

                gen.AddYieldFrom(node, ee._unit, res);

                gen.Returns.AddDependency(ee._unit);
                return gen.Returns.Types;
            }

            return AnalysisSet.Empty;
        }
#endif

        internal void AssignTo(Node assignStmt, Expression left, IAnalysisSet values) {
            if (left is Lookup) {
                var l = (Lookup)left;
                if (l.Name != null) {
                    Scope.AssignVariable(
                        l.Name,
                        l,
                        _unit,
                        values
                    );
                }
            } else if (left is Member) {
                var l = (Member)left;
                if (l.Name != null) {
                    foreach (var obj in Evaluate(l.Root)) {
                        obj.SetMember(l, _unit, l.Name, values);
                    }
                }
            } else if (left is CallNode) {
                var call = (CallNode)left;
                if (call.InBrackets) {
                    var indexObj = Evaluate(call.Function);
                    foreach (var obj in Evaluate(GetIndexArgument(call))) {
                        indexObj.SetIndex(assignStmt, _unit, obj, values);
                    }
                }
            }
        }

        private static IAnalysisSet EvaluateFunctionExpression(ExpressionEvaluator ee, Node node) {
            var func = (FunctionExpression)node;
            EnvironmentRecord funcRec;
            if (ee.Scope.GlobalEnvironment.TryGetNodeEnvironment(func.Function, out funcRec)) {
                return funcRec.AnalysisValue.SelfSet;
            }
            
            Debug.Fail("Failed to find function record");
            return AnalysisSet.Empty;
        }

        private IAnalysisSet MakeArrayValue(ExpressionEvaluator ee, Node node) {
            var sequence = (ArrayValue)ee.Scope.GlobalEnvironment.GetOrMakeNodeValue(node, x => {
                return new ArrayValue(
                    VariableDef.EmptyArray,
                    _unit.ProjectEntry
                ).SelfSet;
            });
            var seqItems = ((ArrayLiteral)node).Elements;
            var indexValues = new IAnalysisSet[seqItems.Count];

            for (int i = 0; i < seqItems.Count; i++) {
                indexValues[i] = Evaluate(seqItems[i]);
            }
            sequence.AddTypes(ee._unit, indexValues);
            return sequence.SelfSet;
        }

        #endregion
    }
}

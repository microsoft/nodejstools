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
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Provides specializations for user defined functions so
    /// we can provide deeper analysis of the code.
    /// </summary>
    internal partial class OverviewWalker {
        private static ExpectedLookup ObjectLookup = new ExpectedLookup("Object");
        private static Dictionary<string, FunctionSpecialization> _specializations = new Dictionary<string, FunctionSpecialization>() { 
            { "merge", MergeSpecialization() },
            { "copy", CopySpecialization() },
            { "create", CreateSpecialization() },
            { "keys", ObjectKeysSpecialization() },
            { "setProto", SetProtoSpecialization() },
        };

        /// <summary>
        /// function setProto(obj, proto) {
        ///   if (typeof Object.setPrototypeOf === "function")
        ///     return Object.setPrototypeOf(obj, proto)
        ///   else
        ///     obj.__proto__ = proto
        /// }
        /// 
        /// This specialization exists to avoid type merging when calling this function
        /// which results in an explosion of analysis.
        /// </summary>
        private static FunctionSpecialization SetProtoSpecialization() {
            var objParam = new ExpectedParameter(0);
            var protoParam = new ExpectedParameter(1);
            var objectSetPrototypeOf = new ExpectedMember(
                    new ExpectedLookup("Object"),
                    "setPrototypeOf"
                );


            return new FunctionSpecialization(
                SetProtoSpecializationImpl,
                false,
                new ExpectedNode(
                    typeof(IfNode),
                    new ExpectedBinary(
                        JSToken.StrictEqual,
                        new ExpectedUnary(
                            JSToken.TypeOf,
                            objectSetPrototypeOf
                        ),
                        new ExpectedConstant("function")
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ReturnNode),
                            new ExpectedCall(
                                objectSetPrototypeOf,
                                objParam,
                                protoParam
                            )
                        )
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ExpressionStatement),
                            new ExpectedBinary(
                                JSToken.Assign,
                                new ExpectedMember(
                                    objParam,
                                    "__proto__"
                                ),
                                protoParam
                            )
                        )
                    )
                )
            );
        }

        private static IAnalysisSet SetProtoSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 2) {
                args[0].SetMember(node, unit, "__proto__", args[1]);
            }
            return AnalysisSet.Empty;
        }

        /// <summary>
        /// function (o) {
        ///   var a = []
        ///   for (var i in o) if (o.hasOwnProperty(i)) a.push(i)
        ///   return a
        /// }        
        /// </summary>
        private static FunctionSpecialization ObjectKeysSpecialization() {
            var oParam = new ExpectedParameter(0);
            var aVar = new ExpectedVariableDeclaration(ExpectedArrayLiteral.Empty);

            var iVar = new ExpectedVariableDeclaration(null);


            return new FunctionSpecialization(
                ObjectKeysSpecializationImpl,
                false,
                aVar,                
                new ExpectedNode(
                    typeof(ForIn),
                    iVar,
                    oParam,
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(IfNode),
                            new ExpectedCall(
                                new ExpectedMember(
                                    oParam,
                                    "hasOwnProperty"
                                ),
                                iVar.Variable
                            ),
                            new ExpectedNode(
                                typeof(Block),
                                new ExpectedNode(
                                    typeof(ExpressionStatement),
                                    new ExpectedCall(
                                        new ExpectedMember(
                                            aVar.Variable,
                                            "push"
                                        ),
                                        iVar.Variable
                                    )
                                )
                            )
                        )
                    )
                ),
                new ExpectedNode(
                    typeof(ReturnNode),
                    aVar.Variable
                )
            );
        }

        private static IAnalysisSet ObjectKeysSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 1) {
                return unit.Analyzer._arrayFunction._instance;
            }
            return AnalysisSet.Empty;
        }


        /// <summary>
        /// function create(o) {
        ///    F.prototype = o;
        ///    return new F();
        ///}
        private static FunctionSpecialization CreateSpecialization() {
            var oParam = new ExpectedParameter(0);
            var fLookup = new ExpectedFlexibleLookup();

            return new FunctionSpecialization(
                CreateSpecializationImpl,
                false,
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedBinary(
                        JSToken.Assign,
                        new ExpectedMember(
                            fLookup,
                            "prototype"
                        ),
                        oParam
                    )
                ),
                new ExpectedNode(
                    typeof(ReturnNode),
                    new ExpectedNew(
                        fLookup.Confirmed
                    )
                )
            );
        }

        private static IAnalysisSet CreateSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 1) {
                // fake out copy by just returning the
                // original input object
                return args[0];
            }
            return AnalysisSet.Empty;
        }

        /// <summary>
        /// function copy (obj) {
        ///  var o = {}
        ///  Object.keys(obj).forEach(function (i) {
        ///    o[i] = obj[i]
        ///  })
        ///  return o
        ///}
        private static FunctionSpecialization CopySpecialization() {
            var objParam = new ExpectedParameter(0);
            var iParam = new ExpectedParameter(0);
            var oVar = new ExpectedVariableDeclaration(ExpectedObjectLiteral.Empty);

            return new FunctionSpecialization(
                CopySpecializationImpl,
                false,
                oVar,
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedCall(
                        new ExpectedMember(
                            new ExpectedCall(
                                new ExpectedMember(
                                    ObjectLookup,
                                    "keys"
                                ),
                                objParam
                            ),
                            "forEach"
                        ),
                        new ExpectedFunctionExpr(
                            new ExpectedNode(
                                typeof(Block),
                                new ExpectedNode(
                                    typeof(ExpressionStatement),
                                    new ExpectedBinary(
                                        JSToken.Assign,
                                        new ExpectedIndex(
                                            oVar.Variable,
                                            iParam
                                        ),
                                        new ExpectedIndex(
                                            new ExpectedParameter(0, 1),
                                            iParam
                                        )
                                    )
                                )
                            )
                        )
                    )
                ),
                new ExpectedNode(
                    typeof(ReturnNode),
                    oVar.Variable
                )
            );
        }

        private static IAnalysisSet CopySpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 1) {
                // fake out copy by just returning the
                // original input object
                return args[0];
            }
            return AnalysisSet.Empty;
        }


        /// <summary>
        /// Matches:
        /// 
        /// function merge(a, b) {
        ///     if(a && b) {
        ///         for(var key in b) {
        ///             a[key] = b[key]
        ///         }
        ///     }
        ///     return a;
        /// }
        /// </summary>
        private static FunctionSpecialization MergeSpecialization() {
            var targetParam = new ExpectedParameter(0);
            var sourceParam = new ExpectedParameter(1);
            var keyVar = new ExpectedVariableDeclaration();
            return new FunctionSpecialization(
                MergeSpecializationImpl,
                false,
                new ExpectedNode(
                    typeof(IfNode),
                    new ExpectedBinary(
                        JSToken.LogicalAnd,
                        targetParam,
                        sourceParam
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ForIn),
                // variable
                            keyVar,
                // collection
                            sourceParam,
                // body
                            new ExpectedNode(
                                typeof(Block),
                                new ExpectedNode(
                                    typeof(ExpressionStatement),
                                    new ExpectedBinary(
                                        JSToken.Assign,
                                        new ExpectedIndex(
                                            targetParam,
                                            keyVar.Variable
                                        ),
                                        new ExpectedIndex(
                                            sourceParam,
                                            keyVar.Variable
                                        )
                                    )
                                )
                            )
                        )
                    )
                ),
                new ExpectedNode(
                    typeof(ReturnNode),
                    targetParam
                )
            );
        }

        private static IAnalysisSet MergeSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 2) {
                foreach (var targetValue in args[0]) {
                    var target = targetValue as ExpandoValue;
                    if (target == null) {
                        continue;
                    }
                    foreach (var sourceValue in args[1]) {
                        var source = sourceValue as ExpandoValue;
                        if (source == null) {
                            continue;
                        }

                        target.AddLinkedValue(source);
                    }
                }

            }
            return AnalysisSet.Empty;
        }

        class MatchState {
            private Dictionary<object, object> _state;
            public readonly FunctionObject CurrentFunction;
            public readonly MatchState OuterState;

            public MatchState(FunctionObject curFunction) {
                CurrentFunction = curFunction;
            }

            public MatchState(FunctionObject curFunction, MatchState outerState) {
                OuterState = outerState;
                _state = outerState._state;
                CurrentFunction = curFunction;
            }

            public bool TryGetState(object key, out object value) {
                if (_state == null) {
                    value = null;
                    return false;
                }

                return _state.TryGetValue(key, out value);
            }

            public object this[object key] {
                get {
                    if (_state == null) {
                        throw new KeyNotFoundException();
                    }

                    return _state[key];
                }
                set {
                    if (_state == null) {
                        _state = new Dictionary<object, object>();
                    }
                    _state[key] = value;
                }
            }
        }

        class FunctionSpecialization {
            public readonly ExpectedChild Body;
            public readonly CallDelegate Specialization;
            public readonly bool CallBase;

            public FunctionSpecialization(CallDelegate specialization, params ExpectedChild[] children)
                : this(specialization, true, children) {
            }

            public FunctionSpecialization(CallDelegate specialization, bool callBase, params ExpectedChild[] children) {
                Specialization = specialization;
                CallBase = callBase;
                Body = new ExpectedNode(
                    typeof(Block),
                    children
                );
            }
        }

        abstract class ExpectedChild {
            public abstract bool IsMatch(MatchState state, Node node);

            protected static bool NoMatch {
                get {
                    // convenient spot for a breakpoint when debugging
                    // lack of matches...
                    return false;
                }
            }
        }

        class ExpectedVariableDeclaration : ExpectedChild {
            public readonly ExpectedVariable Variable;
            public readonly ExpectedChild Initializer;

            public ExpectedVariableDeclaration(ExpectedChild initializer = null) {
                Variable = new ExpectedVariable(this);
                Initializer = initializer;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Var)) {
                    return NoMatch;
                }

                Var var = (Var)node;
                if (var.Count != 1) {
                    return NoMatch;
                }

                if (var[0].Initializer != null) {
                    if (Initializer == null ||
                        !Initializer.IsMatch(state, var[0].Initializer)) {
                        return NoMatch;
                    }
                } else if (Initializer != null) {
                    return NoMatch;
                }

                state[this] = var[0].VariableField;

                return true;
            }
        }

        class ExpectedObjectLiteral : ExpectedChild {
            public static ExpectedObjectLiteral Empty = new ExpectedObjectLiteral();

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(ObjectLiteral)) {
                    return NoMatch;
                }

                var objLit = (ObjectLiteral)node;
                if (objLit.Properties.Count > 0) {
                    return NoMatch;
                }

                return true;
            }
        }

        class ExpectedArrayLiteral : ExpectedChild {
            public static ExpectedArrayLiteral Empty = new ExpectedArrayLiteral();

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(ArrayLiteral)) {
                    return NoMatch;
                }

                var arrLit = (ArrayLiteral)node;
                if (arrLit.Elements.Count > 0) {
                    return NoMatch;
                }

                return true;
            }
        }

        class ExpectedVariable : ExpectedChild {
            private readonly ExpectedVariableDeclaration _decl;
            public ExpectedVariable(ExpectedVariableDeclaration decl) {
                _decl = decl;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Lookup)) {
                    return NoMatch;
                }

                object field;
                if (state.TryGetState(_decl, out field)) {
                    var curField = ((Lookup)node).VariableField;
                    while (curField != null) {
                        if (curField == field) {
                            return true;
                        }
                        curField = curField.OuterField;
                    }
                }
                return NoMatch;
            }
        }

        class ExpectedParameter : ExpectedChild {
            private readonly int _position;
            private readonly int _functionDepth;

            public ExpectedParameter(int position, int functionDepth = 0) {
                _position = position;
                _functionDepth = functionDepth;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Lookup)) {
                    return NoMatch;
                }

                MatchState declState = state;
                for (int i = 0; i < _functionDepth && declState != null; i++) {
                    declState = declState.OuterState;
                }
                if (declState == null) {
                    return NoMatch;
                }

                var lookup = (Lookup)node;
                var curField = lookup.VariableField;
                while (curField != null) {
                    if (curField.Scope == declState.CurrentFunction &&
                        curField.FieldType == FieldType.Argument &&
                        curField.Position == _position) {
                        return true;
                    }
                    curField = curField.OuterField;
                }
                return NoMatch;
            }
        }

        class ExpectedNode : ExpectedChild {
            public readonly Type Type;
            public readonly ExpectedChild[] Expected;

            public ExpectedNode(Type type, params ExpectedChild[] children) {
                Type = type;
                Expected = children;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != Type) {
                    return NoMatch;
                }
                var children = node.Children.ToArray();
                if (children.Length != Expected.Length) {
                    return NoMatch;
                }
                for (int i = 0; i < Expected.Length; i++) {
                    if (!Expected[i].IsMatch(state, children[i])) {
                        return NoMatch;
                    }
                }
                return true;
            }
        }

        class ExpectedBinary : ExpectedChild {
            public readonly JSToken Token;
            public readonly ExpectedChild Left, Right;

            public ExpectedBinary(JSToken token, ExpectedChild left, ExpectedChild right) {
                Token = token;
                Left = left;
                Right = right;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(BinaryOperator)) {
                    return NoMatch;
                }

                BinaryOperator binOp = (BinaryOperator)node;
                if (binOp.OperatorToken != Token) {
                    return NoMatch;
                }

                return Left.IsMatch(state, binOp.Operand1) &&
                    Right.IsMatch(state, binOp.Operand2);
            }
        }

        class ExpectedUnary : ExpectedChild {
            public readonly JSToken Token;
            public readonly ExpectedChild Operand;

            public ExpectedUnary(JSToken token, ExpectedChild operand) {
                Token = token;
                Operand = operand;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(UnaryOperator)) {
                    return NoMatch;
                }

                var op = (UnaryOperator)node;
                if (op.OperatorToken != Token) {
                    return NoMatch;
                }

                return Operand.IsMatch(state, op.Operand);
            }
        }

        class ExpectedMember : ExpectedChild {
            public readonly ExpectedChild Root;
            public readonly string Name;

            public ExpectedMember(ExpectedChild root, string name) {
                Root = root;
                Name = name;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Member)) {
                    return NoMatch;
                }

                Member member = (Member)node;
                if (member.Name != Name) {
                    return NoMatch;
                }

                return Root.IsMatch(state, member.Root);
            }
        }

        class ExpectedLookup : ExpectedChild {
            public readonly string Name;

            public ExpectedLookup(string name) {
                Name = name;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Lookup)) {
                    return NoMatch;
                }

                Lookup member = (Lookup)node;
                if (member.Name != Name) {
                    return NoMatch;
                }

                return true;
            }
        }

        class ExpectedConstant : ExpectedChild {
            public readonly object Value;

            public ExpectedConstant(string value) {
                Value = value;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(ConstantWrapper)) {
                    return NoMatch;
                }

                var member = (ConstantWrapper)node;
                if (!member.Value.Equals(Value)) {
                    return NoMatch;
                }

                return true;
            }
        }

        class ExpectedFlexibleLookup : ExpectedChild {
            public readonly ExpectedConfirmedLookup Confirmed;

            public ExpectedFlexibleLookup() {
                Confirmed = new ExpectedConfirmedLookup(this);
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Lookup)) {
                    return NoMatch;
                }

                Lookup member = (Lookup)node;
                state[this] = member.Name;

                return true;
            }
        }

        class ExpectedConfirmedLookup : ExpectedChild {
            private readonly ExpectedFlexibleLookup _lookup;

            public ExpectedConfirmedLookup(ExpectedFlexibleLookup lookup) {
                _lookup = lookup;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Lookup)) {
                    return NoMatch;
                }

                Lookup member = (Lookup)node;
                object lookupName;
                if (!state.TryGetState(_lookup, out lookupName) ||
                    member.Name != (string)lookupName) {
                    return NoMatch;
                }

                return true;
            }
        }

        class ExpectedFunctionExpr : ExpectedChild {
            public readonly ExpectedChild Body;

            public ExpectedFunctionExpr(ExpectedChild body) {
                Body = body;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(FunctionExpression)) {
                    return NoMatch;
                }


                FunctionExpression func = (FunctionExpression)node;

                MatchState matchState = new MatchState(func.Function, state);
                return Body.IsMatch(matchState, func.Function.Body);
            }
        }

        class ExpectedIndex : ExpectedChild {
            public readonly ExpectedChild Value, Index;

            public ExpectedIndex(ExpectedChild value, ExpectedChild index) {
                Value = value;
                Index = index;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(CallNode)) {
                    return NoMatch;
                }

                CallNode call = (CallNode)node;
                if (!call.InBrackets || call.Arguments.Count != 1) {
                    return NoMatch;
                }

                return Value.IsMatch(state, call.Function) &&
                    Index.IsMatch(state, call.Arguments[0]);
            }
        }

        class ExpectedCall : ExpectedChild {
            public readonly ExpectedChild Value;
            public readonly ExpectedChild[] Args;

            public ExpectedCall(ExpectedChild value, params ExpectedChild[] args) {
                Value = value;
                Args = args;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(CallNode)) {
                    return NoMatch;
                }

                CallNode call = (CallNode)node;
                if (call.InBrackets || call.IsConstructor) {
                    return NoMatch;
                }

                if (Value.IsMatch(state, call.Function)) {
                    if (call.Arguments.Count != Args.Length) {
                        return NoMatch;
                    }
                    for (int i = 0; i < Args.Length; i++) {
                        if (!Args[i].IsMatch(state, call.Arguments[i])) {
                            return NoMatch;
                        }
                    }
                }
                return true;
            }
        }

        class ExpectedNew : ExpectedChild {
            public readonly ExpectedChild Value;
            public readonly ExpectedChild[] Args;

            public ExpectedNew(ExpectedChild value, params ExpectedChild[] args) {
                Value = value;
                Args = args;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(CallNode)) {
                    return NoMatch;
                }

                CallNode call = (CallNode)node;
                if (call.InBrackets || !call.IsConstructor) {
                    return NoMatch;
                }

                if (Value.IsMatch(state, call.Function)) {
                    if (call.Arguments.Count != Args.Length) {
                        return NoMatch;
                    }
                    for (int i = 0; i < Args.Length; i++) {
                        if (!Args[i].IsMatch(state, call.Arguments[i])) {
                            return NoMatch;
                        }
                    }
                }
                return true;
            }
        }
    }
}

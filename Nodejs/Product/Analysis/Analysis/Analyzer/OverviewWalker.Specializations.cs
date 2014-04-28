using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Provides specializations for user defined functions so
    /// we can provdie deeper analysis of the code.
    /// </summary>
    internal partial class OverviewWalker {

        static Dictionary<string, FunctionSpecialization> _specializations = new Dictionary<string, FunctionSpecialization>() { 
            { "merge", MergeSpecialization() }
        };

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
                MergeSpecialization,
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

        private static IAnalysisSet MergeSpecialization(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
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

            public FunctionSpecialization(CallDelegate specialization, params ExpectedChild[] children) {
                Specialization = specialization;
                Body = new ExpectedNode(
                    typeof(Block),
                    children
                );
            }
        }

        abstract class ExpectedChild {
            public abstract bool IsMatch(MatchState state, Node node);
        }

        class ExpectedVariableDeclaration : ExpectedChild {
            public readonly ExpectedVariable Variable;

            public ExpectedVariableDeclaration() {
                Variable = new ExpectedVariable(this);
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Var)) {
                    return false;
                }

                Var var = (Var)node;
                if (var.Count != 1) {
                    return false;
                }

                if (var[0].Initializer != null) {
                    return false;
                }
                state[this] = var[0].VariableField;

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
                    return false;
                }

                object field;
                if (state.TryGetState(_decl, out field)) {
                    return ((Lookup)node).VariableField == field;
                }
                return false;
            }
        }

        class ExpectedParameter : ExpectedChild {
            private readonly int _position;

            public ExpectedParameter(int position) {
                _position = position;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Lookup)) {
                    return false;
                }

                var lookup = (Lookup)node;
                if (lookup.VariableField != null &&
                    lookup.VariableField.FieldType == FieldType.Argument &&
                    lookup.VariableField.Position == _position) {
                    return true;

                }
                return false;
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
                    return false;
                }
                var children = node.Children.ToArray();
                if (children.Length != Expected.Length) {
                    return false;
                }
                for (int i = 0; i < Expected.Length; i++) {
                    if (!Expected[i].IsMatch(state, children[i])) {
                        return false;
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
                    return false;
                }

                BinaryOperator binOp = (BinaryOperator)node;
                if (binOp.OperatorToken != Token) {
                    return false;
                }

                return Left.IsMatch(state, binOp.Operand1) &&
                    Right.IsMatch(state, binOp.Operand2);
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
                    return false;
                }

                CallNode call = (CallNode)node;
                if (!call.InBrackets || call.Arguments.Count != 1) {
                    return false;
                }

                return Value.IsMatch(state, call.Function) &&
                    Index.IsMatch(state, call.Arguments[0]);
            }
        }
    }
}

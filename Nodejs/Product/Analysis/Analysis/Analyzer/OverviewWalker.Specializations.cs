//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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
        private static Dictionary<string, BaseSpecialization[]> _specializations = new Dictionary<string, BaseSpecialization[]>() { 
            { "merge", new[] { MergeSpecialization2(), MergeSpecialization() } },
            { "exports", new[] { MergeSpecialization(), MergeDescriptorsSpecialization()  } }, // utils-merge 1.0, merge-descriptors
            { "mergeClone", new[] { MergeCloneSpecialization()  } },
            { "copy", new[] { CopySpecialization() }},
            { "clone", new[] { new CloneSpecialization(CloneSpecializationImpl) }},
            { "create", new[] { CreateSpecialization() }},
            { "keys", new[] { ObjectKeysSpecialization() }},
            { "setProto", new[] { SetProtoSpecialization() } },
            { "extend", new[] { BackboneExtendSpecialization(), UnderscoreExtendSpecialization() } },
            { "wrapfunction", new[] { WrapFunctionSpecialization() } },
            { "assign", new[] { AssignSpecialization() } },
            { "toObject", new[] { ToObjectSpecialization() }},
        };

        private static IAnalysisSet CloneSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length > 0) {
                return args[0];
            }

            return AnalysisSet.Empty;            
        }

        /// <summary>
        /// var assign = function(object, source, guard) {
        ///      var index, iterable = object, result = iterable;
        ///      if (!iterable) return result;
        ///      var args = arguments,
        ///          argsIndex = 0,
        ///          argsLength = typeof guard == 'number' ? 2 : args.length;
        ///      if (argsLength > 3 && typeof args[argsLength - 2] == 'function') {
        ///        var callback = baseCreateCallback(args[--argsLength - 1], args[argsLength--], 2);
        ///      } else if (argsLength > 2 && typeof args[argsLength - 1] == 'function') {
        ///        callback = args[--argsLength];
        ///      }
        ///      while (++argsIndex < argsLength) {
        ///        iterable = args[argsIndex];
        ///        if (iterable && objectTypes[typeof iterable]) {
        ///          var ownIndex = -1,
        ///              ownProps = objectTypes[typeof iterable] && keys(iterable),
        ///              length = ownProps ? ownProps.length : 0;
        ///          
        ///          while (++ownIndex < length) {
        ///            index = ownProps[ownIndex];
        ///            result[index] = callback ? callback(result[index], iterable[index]) : iterable[index];
        ///          }
        ///        }
        ///      }
        ///      return result
        ///    };
        ///
        /// </summary>
        private static PatternSpecialization AssignSpecialization() {
            var object_ = new ExpectedParameter(0);
            var source = new ExpectedParameter(1);
            var guard = new ExpectedParameter(2);
            var index = new ExpectedVariableDeclaration();
            var iterable = new ExpectedVariableDeclaration(object_);
            var result = new ExpectedVariableDeclaration(iterable.Variable);
            var args = new ExpectedVariableDeclaration(new ExpectedLookup("arguments"));
            var argsIndex = new ExpectedVariableDeclaration(new ExpectedConstant(0.0));
            var argsLength = new ExpectedVariableDeclaration(AlwaysMatch.Instance);
            var callback = new ExpectedVariableDeclaration(AlwaysMatch.Instance);
            var objectTypes = new ExpectedLookup("objectTypes");
            var ownIndex = new ExpectedVariableDeclaration(new ExpectedUnary(JSToken.Minus, new ExpectedConstant(1.0)));
            var ownProps = new ExpectedVariableDeclaration(
                new ExpectedBinary(
                    JSToken.LogicalAnd,
                    new ExpectedIndex(
                        objectTypes,
                        new ExpectedUnary(JSToken.TypeOf, iterable.Variable)
                    ),
                    new ExpectedCall(
                        new ExpectedLookup("keys"),
                        iterable.Variable
                    )
                )
            );
            var length = new ExpectedVariableDeclaration(
                new ExpectedNode(
                    typeof(Conditional),
                    ownProps.Variable,
                    new ExpectedMember(ownProps.Variable, "length"),
                    new ExpectedConstant(0.0)
                )
            );

            return new PatternSpecialization(
                ExtendSpecializationImpl,
                new ExpectedVar(index, iterable, result),
                new ExpectedNode(
                    typeof(IfNode),
                    new ExpectedUnary(
                        JSToken.LogicalNot,
                        iterable.Variable
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ReturnNode),
                            result.Variable
                        )
                    )
                ),
                new ExpectedVar(args, argsIndex, argsLength),
                new ExpectedNode(
                    typeof(IfNode),
                    new ExpectedBinary(
                        JSToken.LogicalAnd,
                        new ExpectedBinary(
                            JSToken.GreaterThan,
                            argsLength.Variable,
                            new ExpectedConstant(3.0)
                        ),
                        new ExpectedBinary(
                            JSToken.Equal,
                            AlwaysMatch.Instance,
                            new ExpectedConstant("function")
                        )
                    ),
                    ExpectedBlock(callback),
                    ExpectedBlock(
                        new ExpectedNode(
                            typeof(IfNode),
                            new ExpectedBinary(
                                JSToken.LogicalAnd,
                                new ExpectedBinary(
                                    JSToken.GreaterThan,
                                    argsLength.Variable,
                                    new ExpectedConstant(2.0)
                                ),
                                new ExpectedBinary(
                                    JSToken.Equal,
                                    AlwaysMatch.Instance,
                                    new ExpectedConstant("function")
                                )
                            ),
                            ExpectedBlock(
                                new ExpectedNode(
                                    typeof(ExpressionStatement),
                                    new ExpectedBinary(
                                        JSToken.Assign,
                                        callback.Variable,
                                        AlwaysMatch.Instance
                                    )
                                )
                            )
                        )
                    )
                ),
                new ExpectedNode(
                    typeof(WhileNode),
                    new ExpectedBinary(
                        JSToken.LessThan,
                        new ExpectedUnary(
                            JSToken.Increment,
                            argsIndex.Variable
                        ),
                        argsLength.Variable
                    ),
                    ExpectedBlock(
                        ExpectedExprStmt(
                            ExpectedAssign(
                                iterable.Variable, 
                                new ExpectedIndex(args.Variable, argsIndex.Variable)
                            )
                        ),
                        new ExpectedNode(
                            typeof(IfNode),
                            new ExpectedBinary(
                                JSToken.LogicalAnd,
                                iterable.Variable,
                                new ExpectedIndex(
                                    objectTypes, 
                                    new ExpectedUnary(JSToken.TypeOf, iterable.Variable)
                                )
                            ),
                            ExpectedBlock(
                                new ExpectedVar(ownIndex, ownProps, length),
                                new ExpectedNode(
                                    typeof(WhileNode),
                                    new ExpectedBinary(
                                        JSToken.LessThan,
                                        new ExpectedUnary(
                                            JSToken.Increment,
                                            ownIndex.Variable
                                        ),
                                        length.Variable
                                    ),
                                    ExpectedBlock(
                                        ExpectedExprStmt(
                                            ExpectedAssign(
                                                index.Variable,
                                                new ExpectedIndex(
                                                    ownProps.Variable,
                                                    ownIndex.Variable
                                                )
                                            )
                                        ),
                                        ExpectedExprStmt(
                                            ExpectedAssign(
                                                new ExpectedIndex(result.Variable, index.Variable),
                                                new ExpectedNode(
                                                    typeof(Conditional),
                                                    callback.Variable,
                                                    AlwaysMatch.Instance,
                                                    new ExpectedIndex(iterable.Variable, index.Variable)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                ),
                new ExpectedNode(
                    typeof(ReturnNode),
                    result.Variable
                )
            );
        }

        /// <summary>
        /// Matches:
        /// 
        /// module.exports = function (dest, src) {
        ///     Object.getOwnPropertyNames(src).forEach(function (name) {
        ///         var descriptor = Object.getOwnPropertyDescriptor(src, name)
        ///         Object.defineProperty(dest, name, descriptor)
        ///     })
        ///     return dest
        /// }
        /// </summary>
        private static PatternSpecialization MergeDescriptorsSpecialization() {
            var destParam = new ExpectedParameter(0);
            var srcParam = new ExpectedParameter(1);
            var descriptorVar = new ExpectedVariableDeclaration(
                new ExpectedCall(
                    new ExpectedMember(new ExpectedLookup("Object"), "getOwnPropertyDescriptor"),
                    new ExpectedParameter(1, 1), // src in outer function
                    new ExpectedParameter(0)    // name in inner function
                )
            );

            return new PatternSpecialization(
                MergeSpecializationImpl,
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedCall(
                        new ExpectedMember(
                            new ExpectedCall(
                                new ExpectedMember(new ExpectedLookup("Object"), "getOwnPropertyNames"),
                                srcParam
                            ),
                            "forEach"
                        ),
                        new ExpectedFunctionExpr(
                            new ExpectedNode(
                                typeof(Block),
                                descriptorVar,
                                new ExpectedNode(
                                    typeof(ExpressionStatement),
                                    new ExpectedCall(
                                        new ExpectedMember(new ExpectedLookup("Object"), "defineProperty"),
                                        new ExpectedParameter(0, 1),    // dest in outer function
                                        new ExpectedParameter(0),   // name in inner function
                                        descriptorVar.Variable
                                    )
                                )
                            )
                        )
                    )
                ),
                new ExpectedNode(typeof(ReturnNode), destParam)
            );
        }

        private static IAnalysisSet AssignSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            return AnalysisSet.Empty;
        }

        private static ExpectedChild ExpectedAssign(ExpectedChild left, ExpectedChild right) {
            return new ExpectedBinary(JSToken.Assign, left, right);
        }


        private static ExpectedChild ExpectedExprStmt(ExpectedChild expression) {
            return new ExpectedNode(typeof(ExpressionStatement), expression);
        }

        private static ExpectedChild ExpectedBlock(params ExpectedChild[] expressions) {
            return new ExpectedNode(typeof(Block), expressions);
        }

        /// <summary>
        /// function wrapfunction(fn, message) {
        ///  if (typeof fn !== 'function') {
        ///    throw new TypeError('argument fn must be a function')
        ///  }
        ///
        ///  var args = createArgumentsString(fn.length)
        ///  var deprecate = this
        ///  var stack = getStack()
        ///  var site = callSiteLocation(stack[1])
        ///
        ///  site.name = fn.name
        ///
        ///  var deprecatedfn = eval('(function (' + args + ') {\n'
        ///    + '"use strict"\n'
        ///    + 'log.call(deprecate, message, site)\n'
        ///    + 'return fn.apply(this, arguments)\n'
        ///    + '})')
        ///
        ///  return deprecatedfn
        ///}
        /// </summary>
        /// <returns></returns>
        private static PatternSpecialization WrapFunctionSpecialization() {
            var fn = new ExpectedParameter(0);
            var message = new ExpectedParameter(1);
            var args = new ExpectedVariableDeclaration(
                new ExpectedCall(
                    new ExpectedLookup("createArgumentsString"),
                    new ExpectedMember(fn, "length")
                )
            );
            var deprecate = new ExpectedVariableDeclaration(
                new ExpectedNode(typeof(ThisLiteral))
            );
            var stack = new ExpectedVariableDeclaration(new ExpectedCall(new ExpectedLookup("getStack")));
            var site = new ExpectedVariableDeclaration(
                new ExpectedCall(
                    new ExpectedLookup("callSiteLocation"),
                    new ExpectedIndex(
                        stack.Variable,
                        new ExpectedConstant(1.0)
                    )
                )
            );
            var deprecatedfn = new ExpectedVariableDeclaration(
                new ExpectedCall(
                    new ExpectedLookup("eval"),
                    AlwaysMatch.Instance
                )
            );

            return new PatternSpecialization(
                WrapFunctionSpecializationImpl,
                new ExpectedNode(
                    typeof(IfNode),
                    new ExpectedBinary(
                        JSToken.StrictNotEqual,
                        new ExpectedUnary(
                            JSToken.TypeOf,
                            fn
                        ),
                        new ExpectedConstant("function")
                    ),
                    AlwaysMatch.Instance
                ),
                args,
                deprecate,
                stack,
                site,
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedBinary(
                        JSToken.Assign,
                        new ExpectedMember(site.Variable, "name"),
                        new ExpectedMember(fn, "name")
                    )
                ),
                deprecatedfn,
                new ExpectedNode(
                    typeof(ReturnNode),
                    deprecatedfn.Variable
                )
            );
        }

        private static IAnalysisSet WrapFunctionSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // just return the original unwrapped function for intellisense purposes...
            if (args.Length >= 1) {
                return args[0];
            }
            return AnalysisSet.Empty;
        }


        /// <summary>
        /// function extend(protoProps, staticProps) {
        ///   var parent = this;
        ///   var child;
        ///   
        ///   if (protoProps && _.has(protoProps, 'constructor')) {
        ///     child = protoProps.constructor;
        ///   } else {
        ///     child = function(){ return parent.apply(this, arguments); };
        ///   }
        ///
        ///   _.extend(child, parent, staticProps);
        ///   
        ///   var Surrogate = function(){ this.constructor = child; };
        ///   Surrogate.prototype = parent.prototype;
        ///   child.prototype = new Surrogate;
        ///
        ///   if (protoProps) _.extend(child.prototype, protoProps);
        ///
        ///   child.__super__ = parent.prototype;
        ///
        ///   return child;
        /// };
        /// </summary>
        /// <returns></returns>
        private static PatternSpecialization BackboneExtendSpecialization() {
            var protoProps = new ExpectedParameter(0);
            var staticProps = new ExpectedParameter(1);
            var parentVar = new ExpectedVariableDeclaration(new ExpectedNode(typeof(ThisLiteral)));
            var childVar = new ExpectedVariableDeclaration();
            var underscore = new ExpectedLookup("_");
            var thisNode = new ExpectedNode(typeof(ThisLiteral));
            var surrogate = new ExpectedVariableDeclaration(
                new ExpectedFunctionExpr(
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ExpressionStatement),
                            new ExpectedBinary(
                                JSToken.Assign,
                                new ExpectedMember(
                                    thisNode,
                                    "constructor"
                                ),
                                childVar.Variable
                            )
                        )
                    )
                )
            );

            return new PatternSpecialization(
                BackboneExtendSpecializationImpl,
                false,
                parentVar,
                childVar,
                new ExpectedNode(
                    typeof(IfNode),
                    new ExpectedBinary(
                        JSToken.LogicalAnd,
                        protoProps,
                        new ExpectedCall(
                            new ExpectedMember(underscore, "has"),
                            protoProps,
                            new ExpectedConstant("constructor")
                        )
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ExpressionStatement),
                            new ExpectedBinary(
                                JSToken.Assign,
                                childVar.Variable,
                                new ExpectedMember(
                                    protoProps,
                                    "constructor"
                                )
                            )
                        )
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ExpressionStatement),
                            new ExpectedBinary(
                                JSToken.Assign,
                                childVar.Variable,
                                new ExpectedFunctionExpr(
                                    new ExpectedNode(
                                        typeof(Block),
                                        new ExpectedNode(
                                            typeof(ReturnNode),
                                            new ExpectedCall(
                                                new ExpectedMember(parentVar.Variable, "apply"),
                                                thisNode,
                                                new ExpectedLookup("arguments")
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                ),
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedCall(
                        new ExpectedMember(underscore, "extend"),
                        childVar.Variable,
                        parentVar.Variable,
                        staticProps
                    )
                ),
                surrogate,
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedBinary(
                        JSToken.Assign,
                        new ExpectedMember(surrogate.Variable, "prototype"),
                        new ExpectedMember(parentVar.Variable, "prototype")
                    )
                ),
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedBinary(
                        JSToken.Assign,
                        new ExpectedMember(childVar.Variable, "prototype"),
                        new ExpectedNew(surrogate.Variable)
                    )
                ),
                new ExpectedNode(
                    typeof(IfNode),
                    protoProps,
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ExpressionStatement),
                            new ExpectedCall(
                                new ExpectedMember(underscore, "extend"),
                                new ExpectedMember(childVar.Variable, "prototype"),
                                protoProps
                            )
                        )
                    )
                ),
                new ExpectedNode(
                    typeof(ExpressionStatement),
                    new ExpectedBinary(
                        JSToken.Assign,
                        new ExpectedMember(childVar.Variable, "__super__"),
                        new ExpectedMember(parentVar.Variable, "prototype")
                    )
                ),
                new ExpectedNode(
                    typeof(ReturnNode),
                    childVar.Variable
                )
            );
        }

        internal class BackboneExtendFunctionValue : FunctionValue {
            internal readonly PrototypeValue _prototype;

            public BackboneExtendFunctionValue(ProjectEntry declaringEntry) :
                base(declaringEntry) {
                _prototype = (PrototypeValue)Descriptors["prototype"].Values.TypesNoCopy.First().Value;
            }
        }

        private static IAnalysisSet BackboneExtendSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet res = AnalysisSet.Empty;
            BackboneExtendFunctionValue value;
            if (!unit._env.GlobalEnvironment.TryGetNodeValue(NodeEnvironmentKind.ExtendCall, node, out res)) {
                value = new BackboneExtendFunctionValue(
                    unit.ProjectEntry
                );
                res = value.SelfSet;
                unit._env.GlobalEnvironment.AddNodeValue(
                    NodeEnvironmentKind.ExtendCall, 
                    node, 
                    value.SelfSet
                );
            } else {
                value = (BackboneExtendFunctionValue)res.First().Value;
            }

            if (@this != null) {
                value._instance.SetMember(
                    node,
                    unit,
                    "__proto__",
                    @this.Construct(node, unit, args)
                );
            }

            if (args.Length > 0) {
                if (args[0].Count < unit.Analyzer.Limits.MaxMergeTypes) {
                    foreach (var protoProps in args[0]) {
                        ExpandoValue expandoProto = protoProps.Value as ExpandoValue;
                        if (expandoProto != null) {
                            value._prototype.AddLinkedValue(unit, expandoProto);
                        }
                    }
                }
            }

            if (args.Length > 1) {
                if (args[1].Count < unit.Analyzer.Limits.MaxMergeTypes) {
                    foreach (var protoProps in args[1]) {
                        ExpandoValue expandoProto = protoProps.Value as ExpandoValue;
                        if (expandoProto != null) {
                            value.AddLinkedValue(unit, expandoProto);
                        }
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// function(obj) {
        ///    if (!_.isObject(obj)) return obj;
        ///    var source, prop;
        ///    for (var i = 1, length = arguments.length; i < length; i++) {
        ///      source = arguments[i];
        ///      for (prop in source) {
        ///        if (hasOwnProperty.call(source, prop)) {
        ///            obj[prop] = source[prop];
        ///        }
        ///      }
        ///    }
        ///    return obj;
        ///  };
        ///
        /// </summary>
        /// <returns></returns>
        private static PatternSpecialization UnderscoreExtendSpecialization() {
            var objParam = new ExpectedParameter(0);
            var sourceVar = new ExpectedVariableDeclaration();
            var propVar = new ExpectedVariableDeclaration();
            var iVar = new ExpectedVariableDeclaration(new ExpectedConstant(1.0));
            var arguments = new ExpectedLookup("arguments");
            var lengthVar = new ExpectedVariableDeclaration(new ExpectedMember(arguments, "length"));
            var retNode = new ExpectedNode(typeof(ReturnNode), objParam);

            return new PatternSpecialization(
                ExtendSpecializationImpl,
                false,
                new ExpectedNode(
                    typeof(IfNode),
                    new ExpectedUnary(
                        JSToken.LogicalNot,
                        new ExpectedCall(
                            new ExpectedMember(
                                new ExpectedLookup("_"),
                                "isObject"
                            ),
                            objParam
                        )
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        retNode
                    )
                ),
                new ExpectedVar(
                    sourceVar,
                    propVar
                ),
                new ExpectedNode(
                    typeof(ForNode),
                    new ExpectedVar(
                        iVar,
                        lengthVar
                    ),
                    new ExpectedBinary(
                        JSToken.LessThan,
                        iVar.Variable,
                        lengthVar.Variable
                    ),
                    new ExpectedUnary(
                        JSToken.Increment,
                        iVar.Variable
                    ),
                    new ExpectedNode(
                        typeof(Block),
                        new ExpectedNode(
                            typeof(ExpressionStatement),
                            new ExpectedBinary(
                                JSToken.Assign,
                                sourceVar.Variable,
                                new ExpectedIndex(
                                    arguments,
                                    iVar.Variable
                                )
                            )
                        ),
                        new ExpectedNode(
                            typeof(ForIn),
                            new ExpectedNode(
                                typeof(ExpressionStatement),
                                propVar.Variable
                            ),
                            sourceVar.Variable,
                            new ExpectedNode(
                                typeof(Block),
                                new ExpectedNode(
                                    typeof(IfNode),
                                    new ExpectedCall(
                                        new ExpectedMember(
                                            new ExpectedLookup("hasOwnProperty"),
                                            "call"
                                        ),
                                        sourceVar.Variable,
                                        propVar.Variable
                                    ),
                                    new ExpectedNode(
                                        typeof(Block),
                                        new ExpectedNode(
                                            typeof(ExpressionStatement),
                                            new ExpectedBinary(
                                                JSToken.Assign,
                                                new ExpectedIndex(
                                                    objParam,
                                                    propVar.Variable
                                                ),
                                                new ExpectedIndex(
                                                    sourceVar.Variable,
                                                    propVar.Variable
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                ),
                retNode
            );
        }

        private static IAnalysisSet ExtendSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 1) {
                var obj = args[0];
                if (args.Length >= 2) {
                    foreach (var targetValue in obj) {
                        var target = targetValue.Value as ExpandoValue;
                        if (target == null || target is BuiltinObjectPrototypeValue) {
                            continue;
                        }

                        for (int i = 1; i < args.Length; i++) {
                            var curArg = args[i];

                            if (curArg.Count < unit.Analyzer.Limits.MaxMergeTypes) {
                                foreach (var sourceValue in curArg) {
                                    var source = sourceValue.Value as ExpandoValue;
                                    if (source == null) {
                                        continue;
                                    }

                                    target.AddLinkedValue(unit, source);
                                }
                            }
                        }
                    }
                }
                return obj;
            }
            return AnalysisSet.Empty;
        }

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
        private static PatternSpecialization SetProtoSpecialization() {
            var objParam = new ExpectedParameter(0);
            var protoParam = new ExpectedParameter(1);
            var objectSetPrototypeOf = new ExpectedMember(
                    new ExpectedLookup("Object"),
                    "setPrototypeOf"
                );


            return new PatternSpecialization(
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
        private static PatternSpecialization ObjectKeysSpecialization() {
            var oParam = new ExpectedParameter(0);
            var aVar = new ExpectedVariableDeclaration(ExpectedArrayLiteral.Empty);

            var iVar = new ExpectedVariableDeclaration(null);


            return new PatternSpecialization(
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
                return unit.Analyzer._arrayFunction._instance.Proxy;
            }
            return AnalysisSet.Empty;
        }

        /// <summary>
        /// function toObject(value) {
        ///    return isObject(value) ? value : Object(value);
        /// }
        /// </summary>
        private static PatternSpecialization ToObjectSpecialization() {
            var value = new ExpectedParameter(0);

            return new PatternSpecialization(
                CreateSpecializationImpl,
                false,
                new ExpectedNode(
                    typeof(ReturnNode),
                    new ExpectedNode(
                        typeof(Conditional),
                        new ExpectedCall(
                            new ExpectedLookup("isObject"),
                            value
                        ),
                        value,
                        new ExpectedCall(
                            new ExpectedLookup("Object"),
                            value
                        )
                    )
                )
            );
        }

        /// <summary>
        /// function create(o) {
        ///    F.prototype = o;
        ///    return new F();
        ///}
        private static PatternSpecialization CreateSpecialization() {
            var oParam = new ExpectedParameter(0);
            var fLookup = new ExpectedFlexibleLookup();

            return new PatternSpecialization(
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
        private static PatternSpecialization CopySpecialization() {
            var objParam = new ExpectedParameter(0);
            var iParam = new ExpectedParameter(0);
            var oVar = new ExpectedVariableDeclaration(ExpectedObjectLiteral.Empty);

            return new PatternSpecialization(
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
        private static PatternSpecialization MergeSpecialization() {
            var targetParam = new ExpectedParameter(0);
            var sourceParam = new ExpectedParameter(1);
            var keyVar = new ExpectedVariableDeclaration();
            return new PatternSpecialization(
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

        /// <summary>
        /// Matches:
        /// 
        /// function merge (to, from) {
        ///  var keys = Object.keys(from)
        ///    , i = keys.length
        ///    , key
        ///
        ///  while (i--) {
        ///    key = keys[i];
        ///    if ('undefined' === typeof to[key]) {
        ///      to[key] = from[key];
        ///    } else {
        ///      if (exports.isObject(from[key])) {
        ///        merge(to[key], from[key]);
        ///      } else {
        ///        to[key] = from[key];
        ///      }
        ///    }
        ///  }
        ///}
        /// </summary>
        private static PatternSpecialization MergeSpecialization2() {
            var toParam = new ExpectedParameter(0);
            var fromParam = new ExpectedParameter(1);
            var keysVar = new ExpectedVariableDeclaration(
                new ExpectedCall(
                    new ExpectedMember(new ExpectedLookup("Object"), "keys"),
                    fromParam
                )
            );
            var iVar = new ExpectedVariableDeclaration(
                new ExpectedMember(keysVar.Variable, "length")
            );
            var keyVar = new ExpectedVariableDeclaration();

            var copyProp = ExpectedExprStmt(
                    ExpectedAssign(
                        new ExpectedIndex(
                            toParam,
                            keyVar.Variable
                        ),
                        new ExpectedIndex(
                            fromParam,
                            keyVar.Variable
                        )
                    )
                );

            return new PatternSpecialization(
                MergeSpecializationImpl,
                false,
                new ExpectedVar(keysVar, iVar, keyVar),
                new ExpectedNode(
                    typeof(WhileNode),
                    new ExpectedUnary(JSToken.Decrement, iVar.Variable),
                    ExpectedBlock(
                        ExpectedExprStmt(
                            ExpectedAssign(
                                keyVar.Variable,
                                new ExpectedIndex(keysVar.Variable, iVar.Variable)
                            )
                        ),

                        new ExpectedNode(
                            typeof(IfNode),
                            new ExpectedBinary(
                                JSToken.StrictEqual,
                                new ExpectedConstant("undefined"),
                                new ExpectedUnary(
                                    JSToken.TypeOf,
                                    new ExpectedIndex(toParam, keyVar.Variable)
                                )
                            ),
                            ExpectedBlock(copyProp),
                            ExpectedBlock(
                                new ExpectedNode(
                                    typeof(IfNode),
                                    new ExpectedCall(
                                        new ExpectedMember(
                                            AlwaysMatch.Instance,
                                            "isObject"
                                        ),
                                        new ExpectedIndex(
                                            fromParam,
                                            keyVar.Variable
                                        )
                                    ),
                                    ExpectedBlock(
                                        ExpectedExprStmt(
                                            new ExpectedCall(
                                                new ExpectedLookup("merge"),
                                                new ExpectedIndex(
                                                    toParam,
                                                    keyVar.Variable
                                                ),
                                                new ExpectedIndex(
                                                    fromParam,
                                                    keyVar.Variable
                                                )
                                            )                
                                        )
                                    ),
                                    ExpectedBlock(copyProp)
                                )
                            )
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Matches:
        /// 
        /// function merge (to, from) {
        ///  var keys = Object.keys(from)
        ///    , i = keys.length
        ///    , key
        ///
        ///  while (i--) {
        ///    key = keys[i];
        ///    if ('undefined' === typeof to[key]) {
        ///      to[key] = clone(from[key], {retainKeyOrder:1});
        ///    } else {
        ///      if (exports.isObject(from[key])) {
        ///        mergeClone(to[key], from[key]);
        ///      } else {
        ///        to[key] = clone(from[key], {retainKeyOrder: 1});
        ///      }
        ///    }
        ///  }
        ///}
        /// </summary>
        private static PatternSpecialization MergeCloneSpecialization() {
            var toParam = new ExpectedParameter(0);
            var fromParam = new ExpectedParameter(1);
            var keysVar = new ExpectedVariableDeclaration(
                new ExpectedCall(
                    new ExpectedMember(new ExpectedLookup("Object"), "keys"),
                    fromParam
                )
            );
            var iVar = new ExpectedVariableDeclaration(
                new ExpectedMember(keysVar.Variable, "length")
            );
            var keyVar = new ExpectedVariableDeclaration();

            var copyProp = ExpectedExprStmt(
                    ExpectedAssign(
                        new ExpectedIndex(
                            toParam,
                            keyVar.Variable
                        ),
                        new ExpectedCall(
                            new ExpectedLookup("clone"),
                            new ExpectedIndex(
                                fromParam,
                                keyVar.Variable
                            ),
                            AlwaysMatch.Instance
                        )
                    )
                );

            return new PatternSpecialization(
                MergeSpecializationImpl,
                false,
                new ExpectedVar(keysVar, iVar, keyVar),
                new ExpectedNode(
                    typeof(WhileNode),
                    new ExpectedUnary(JSToken.Decrement, iVar.Variable),
                    ExpectedBlock(
                        ExpectedExprStmt(
                            ExpectedAssign(
                                keyVar.Variable,
                                new ExpectedIndex(keysVar.Variable, iVar.Variable)
                            )
                        ),

                        new ExpectedNode(
                            typeof(IfNode),
                            new ExpectedBinary(
                                JSToken.StrictEqual,
                                new ExpectedConstant("undefined"),
                                new ExpectedUnary(
                                    JSToken.TypeOf,
                                    new ExpectedIndex(toParam, keyVar.Variable)
                                )
                            ),
                            ExpectedBlock(copyProp),
                            ExpectedBlock(
                                new ExpectedNode(
                                    typeof(IfNode),
                                    new ExpectedCall(
                                        new ExpectedMember(
                                            AlwaysMatch.Instance,
                                            "isObject"
                                        ),
                                        new ExpectedIndex(
                                            fromParam,
                                            keyVar.Variable
                                        )
                                    ),
                                    ExpectedBlock(
                                        ExpectedExprStmt(
                                            new ExpectedCall(
                                                new ExpectedLookup("mergeClone"),
                                                new ExpectedIndex(
                                                    toParam,
                                                    keyVar.Variable
                                                ),
                                                new ExpectedIndex(
                                                    fromParam,
                                                    keyVar.Variable
                                                )
                                            )
                                        )
                                    ),
                                    ExpectedBlock(copyProp)
                                )
                            )
                        )
                    )
                )
            );
        }

        private static IAnalysisSet MergeSpecializationImpl(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 2) {
                foreach (var targetValue in args[0]) {
                    var target = targetValue.Value as ExpandoValue;
                    if (target == null) {
                        continue;
                    }
                    if (args[1].Count < unit.Analyzer.Limits.MaxMergeTypes) {
                        foreach (var sourceValue in args[1]) {
                            var source = sourceValue.Value as ExpandoValue;
                            if (source == null) {
                                continue;
                            }

                            target.AddLinkedValue(unit, source);
                        }
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

        abstract class BaseSpecialization {
            public readonly CallDelegate Specialization;
            public readonly bool CallBase;

            public BaseSpecialization(CallDelegate specialization) {
                Specialization = specialization;
                CallBase = true;
            }

            public BaseSpecialization(CallDelegate specialization, bool callBase) {
                Specialization = specialization;
                CallBase = callBase;
            }

            public abstract bool IsMatch(FunctionObject node);
        }

        class PatternSpecialization : BaseSpecialization {
            public readonly ExpectedChild Body;

            public PatternSpecialization(CallDelegate specialization, params ExpectedChild[] children)
                : this(specialization, true, children) {
            }

            public PatternSpecialization(CallDelegate specialization, bool callBase, params ExpectedChild[] children)
                : base(specialization, callBase) {
                Body = new ExpectedNode(
                    typeof(Block),
                    children
                );
            }

            public override bool IsMatch(FunctionObject node) {
                MatchState state = new MatchState(node);

                return Body.IsMatch(state, node.Body);
            }
        }
        
        /// <summary>
        /// Identifies methods which are likely to clone their inputs.  These are
        /// methods named clone that return their first parameter.
        /// </summary>
        class CloneSpecialization : BaseSpecialization {
            public CloneSpecialization(CallDelegate specialization)
                : base(specialization, false) {
            }

            class CloneVisitor : AstVisitor {
                private JSVariableField _clonedVar;
                public bool ReturnsClonedVar;

                public CloneVisitor(JSVariableField variable) {
                    _clonedVar = variable;
                }

                public override bool Walk(ReturnNode node) {
                    if (node.Operand is Lookup &&
                        ((Lookup)node.Operand).VariableField == _clonedVar) {
                        ReturnsClonedVar = true;
                        return false;
                    }

                    return base.Walk(node);
                }
            }

            public override bool IsMatch(FunctionObject node) {
                if (node.ParameterDeclarations != null &&
                    node.ParameterDeclarations.Length >= 1) {
                    var visitor = new CloneVisitor(node.ParameterDeclarations[0].VariableField);
                    node.Walk(visitor);
                    return visitor.ReturnsClonedVar;
                }
                return false;
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

        class ExpectedVar : ExpectedChild {
            private readonly ExpectedVariableDeclaration[] _decls;

            public ExpectedVar(params ExpectedVariableDeclaration[] decls) {
                _decls = decls;
            }

            public override bool IsMatch(MatchState state, Node node) {
                if (node.GetType() != typeof(Var)) {
                    return NoMatch;
                }

                Var var = (Var)node;
                if (var.Count != _decls.Length) {
                    return NoMatch;
                }

                for (int i = 0; i < _decls.Length; i++) {
                    var decl = _decls[i];

                    if (var[i].Initializer != null) {
                        if (decl.Initializer == null ||
                            !decl.Initializer.IsMatch(state, var[i].Initializer)) {
                            return NoMatch;
                        }
                    } else if (decl.Initializer != null) {
                        return NoMatch;
                    }
                    state[decl] = var[i].VariableField;
                }

                return true;
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
                if (objLit.Properties.Length > 0) {
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
                if (arrLit.Elements.Length > 0) {
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

        class AlwaysMatch : ExpectedChild {
            public static AlwaysMatch Instance = new AlwaysMatch();

            public override bool IsMatch(MatchState state, Node node) {
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

            public ExpectedConstant(object value) {
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
                if (!call.InBrackets || call.Arguments.Length != 1) {
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
                    if (call.Arguments.Length != Args.Length) {
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
                    if (call.Arguments.Length != Args.Length) {
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

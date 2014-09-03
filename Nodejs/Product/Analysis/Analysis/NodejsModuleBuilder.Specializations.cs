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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis {
    partial class NodejsModuleBuilder {
        private static Dictionary<string, Dictionary<string, FunctionSpecializer>> _moduleSpecializations = new Dictionary<string, Dictionary<string, FunctionSpecializer>>() { 
            { 
                "util", 
                new Dictionary<string, FunctionSpecializer>() {
                    { "inherits", new CallableFunctionSpecializer(UtilInherits) }
                }
            },
            {
                "path",
                new Dictionary<string, FunctionSpecializer>() {
                    { "resolve", ReturnValueFunctionSpecializer.String },
                    { "normalize", ReturnValueFunctionSpecializer.String },
                    { "join", ReturnValueFunctionSpecializer.String },
                    { "relative", ReturnValueFunctionSpecializer.String },
                    { "dirname", ReturnValueFunctionSpecializer.String },
                    { "basename", ReturnValueFunctionSpecializer.String },
                    { "extname", ReturnValueFunctionSpecializer.String },
                }
            },
            {
                "fs",
                new Dictionary<string, FunctionSpecializer>() {
                    { "existsSync", ReturnValueFunctionSpecializer.Boolean }
                }
            }
        };

        private static Dictionary<string, Dictionary<string, FunctionSpecializer>> _classSpecializations = new Dictionary<string, Dictionary<string, FunctionSpecializer>>() { 
            { 
                "events.EventEmitter", 
                new Dictionary<string, FunctionSpecializer>() {
                    { "addListener", new CallableFunctionSpecializer(EventEmitterAddListener) },
                    { "on", new CallableFunctionSpecializer(EventEmitterAddListener) },
                    { "emit", new CallableFunctionSpecializer(EventEmitterEmit) }
                }
            }
        };

        abstract class FunctionSpecializer {
            public abstract FunctionValue Specialize(ProjectEntry projectEntry, string name, string doc, ParameterResult[] parameters);
        }

        class CallableFunctionSpecializer : FunctionSpecializer {
            private readonly CallDelegate _delegate;

            public CallableFunctionSpecializer(CallDelegate callDelegate) {
                _delegate = callDelegate;
            }

            public override FunctionValue Specialize(ProjectEntry projectEntry, string name, string doc, ParameterResult[] parameters) {
                return new SpecializedFunctionValue(
                    projectEntry,
                    name,
                    _delegate,
                    doc,
                    parameters
                );
            }
        }

        abstract class ReturnValueFunctionSpecializer : FunctionSpecializer {
            public static ReturnValueFunctionSpecializer String = new StringSpecializer();
            public static ReturnValueFunctionSpecializer Boolean = new BooleanSpecializer();

            public override FunctionValue Specialize(ProjectEntry projectEntry, string name, string doc, ParameterResult[] parameters) {
                return new ReturningFunctionValue(
                    projectEntry,
                    name,
                    GetReturnValue(projectEntry.Analyzer),
                    doc,
                    true,
                    parameters
                );
            }

            public abstract IAnalysisSet GetReturnValue(JsAnalyzer analyzer);

            class StringSpecializer : ReturnValueFunctionSpecializer {
                public override IAnalysisSet GetReturnValue(JsAnalyzer analyzer) {
                    return analyzer._emptyStringValue.SelfSet;
                }
            }

            class BooleanSpecializer : ReturnValueFunctionSpecializer {
                public override IAnalysisSet GetReturnValue(JsAnalyzer analyzer) {
                    return analyzer._trueInst.SelfSet;
                }
            }
        }

        private static IAnalysisSet UtilInherits(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // function inherits(ctor, superCtor)
            // sets ctor.super_ to superCtor
            // sets ctor.prototype = {copy of superCtor.prototype}
            if (args.Length >= 2) {
                args[0].SetMember(node, unit, "super_", args[1]);

                IAnalysisSet prototypeValue;
                InheritsPrototypeValue copied;
                var prototype = args[1].Get(node, unit, "prototype");
                if (!unit.DeclaringModuleEnvironment.TryGetNodeValue(NodeEnvironmentKind.InheritsPrototypeValue, node, out prototypeValue)) {
                    copied = new InheritsPrototypeValue(unit.ProjectEntry, prototype);
                    unit.DeclaringModuleEnvironment.AddNodeValue(NodeEnvironmentKind.InheritsPrototypeValue, node, copied.Proxy);
                } else {
                    copied = (InheritsPrototypeValue)prototypeValue.First().Value;
                    copied.AddPrototypes(prototype);
                }

                args[0].SetMember(node, unit, "prototype", copied.Proxy);
            }
            return AnalysisSet.Empty;
        }

        [Serializable]
        internal class EventListenerKey {
            public readonly string EventName;

            public EventListenerKey(string eventName) {
                EventName = eventName;
            }

            public override int GetHashCode() {
                return EventName.GetHashCode();
            }

            public override bool Equals(object obj) {
                EventListenerKey key = obj as EventListenerKey;
                if (key == null) {
                    return false;
                }

                return EventName == key.EventName;
            }
        }

        /// <summary>
        /// this.addListener(name, handler)
        /// 
        /// returns the event listener
        /// </summary>
        private static IAnalysisSet EventEmitterAddListener(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 2 && args[1].Count > 0) {
                foreach (var thisArg in @this) {
                    ExpandoValue expando = @thisArg.Value as ExpandoValue;
                    if (expando != null) {
                        foreach (var arg in args[0]) {
                            var strValue = arg.Value.GetStringValue();
                            if (strValue != null) {
                                var key = new EventListenerKey(strValue);
                                VariableDef events;
                                if (!expando.TryGetMetadata(key, out events)) {
                                    expando.SetMetadata(key, events = new VariableDef());
                                }

                                events.AddTypes(unit, args[1]);
                            }
                        }
                    }
                }
            }
            return @this;
        }

        /// <summary>
        /// eventListener.emit(eventName, args...)
        /// 
        /// Returns bool indicating if event was raised.
        /// </summary>
        private static IAnalysisSet EventEmitterEmit(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 1) {
                foreach (var thisArg in @this) {
                    ExpandoValue expando = @thisArg.Value as ExpandoValue;
                    if (expando != null) {
                        foreach (var arg in args[0]) {
                            var strValue = arg.Value.GetStringValue();
                            if (strValue != null) {
                                VariableDef events;
                                if (expando.TryGetMetadata<VariableDef>(new EventListenerKey(strValue), out events)) {
                                    foreach (var type in events.GetTypesNoCopy(unit)) {
                                        type.Call(node, unit, @this, args.Skip(1).ToArray());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return func.ProjectState._trueInst.Proxy;
        }
    }
}

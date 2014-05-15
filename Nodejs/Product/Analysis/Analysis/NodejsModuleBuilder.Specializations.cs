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

namespace Microsoft.NodejsTools.Analysis {
    partial class NodejsModuleBuilder {
        private static Dictionary<string, Dictionary<string, CallDelegate>> _moduleSpecializations = new Dictionary<string, Dictionary<string, CallDelegate>>() { 
            { 
                "util", 
                new Dictionary<string, CallDelegate>() {
                    { "inherits", UtilInherits }
                }
            }
        };

        private static Dictionary<string, Dictionary<string, CallDelegate>> _classSpecializations = new Dictionary<string, Dictionary<string, CallDelegate>>() { 
            { 
                "events.EventEmitter", 
                new Dictionary<string, CallDelegate>() {
                    { "addListener", EventEmitterAddListener },
                    { "on", EventEmitterAddListener },
                    { "emit", EventEmitterEmit }
                }
            }
        };

        private static IAnalysisSet UtilInherits(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // function inherits(ctor, superCtor)
            // sets ctor.super_ to superCtor
            // sets ctor.prototype = {copy of superCtor.prototype}
            // We skip the copy here which is cheating but lack of flow control
            // means even if we did copy we'd continue to need to copy, so it's fine.
            if (args.Length >= 2) {
                args[0].SetMember(node, unit, "super_", args[1]);

                args[0].SetMember(node, unit, "prototype", args[1].Get(node, unit, "prototype"));
            }
            return AnalysisSet.Empty;
        }

        class EventListenerKey {
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
                    ExpandoValue expando = @thisArg as ExpandoValue;
                    if (expando != null) {
                        foreach (var arg in args[0]) {
                            var strValue = arg.GetConstantValueAsString();
                            if (strValue != null) {
                                var md = expando.EnsureMetadata();
                                var key = new EventListenerKey(strValue);
                                object value;
                                VariableDef events;
                                if (!md.TryGetValue(key, out value)) {
                                    md[key] = value = events = new VariableDef();
                                } else {
                                    events = (VariableDef)value;
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
                    ExpandoValue expando = @thisArg as ExpandoValue;
                    if (expando != null) {
                        foreach (var arg in args[0]) {
                            var strValue = arg.GetConstantValueAsString();
                            if (strValue != null) {
                                var md = expando.Metadata;
                                if (md != null) {
                                    object value;
                                    if(md.TryGetValue(new EventListenerKey(strValue), out value)) {
                                        VariableDef events = (VariableDef)value;
                                        foreach (var type in events.TypesNoCopy) {
                                            type.Call(node, unit, @this, args.Skip(1).ToArray());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return func.ProjectState._trueInst;
        }
    }
}

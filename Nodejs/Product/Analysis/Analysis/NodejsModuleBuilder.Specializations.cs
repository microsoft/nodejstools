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
using System.IO;
using System.Diagnostics;

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
                    { "basename", new CallableFunctionSpecializer(FsBasename) },
                    { "extname", ReturnValueFunctionSpecializer.String },
            }
            },
            {
                "fs",
                new Dictionary<string, FunctionSpecializer>() {
                    { "existsSync", ReturnValueFunctionSpecializer.Boolean },
                    { "readdirSync", new CallableFunctionSpecializer(FsReadDirSync) },
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
                    null,
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


        internal class ReadDirSyncArrayValue : ArrayValue {
            private readonly HashSet<string> _readDirs = new HashSet<string>();
            private static char[] InvalidPathChars = Path.GetInvalidPathChars();

            public ReadDirSyncArrayValue(ProjectEntry projectEntry, Node node)
                : base(new[] { new TypedDef() }, projectEntry, node) {
            }

            public void AddDirectoryMembers(AnalysisUnit unit, string path) {
                
                if (!String.IsNullOrWhiteSpace(path) &&
                    path.IndexOfAny(InvalidPathChars) == -1 && 
                    _readDirs.Add(path) &&
                    Directory.Exists(path)) {
                        string trimmed = path.Trim();
                        if (trimmed != "." && trimmed != "/") {
                            var files = Directory.GetFiles(path);

                            foreach (var file in files) {
                                IndexTypes[0].AddTypes(
                                    unit,
                                    unit.Analyzer.GetConstant(Path.GetFileName(file)).SelfSet
                                );
                            }
                        }
                }
            }

            public override void ForEach(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
                // for this for each we want to process the un-merged values so that we
                // get the best results instead of merging all of the strings together.
                foreach (var value in IndexTypes[0].GetTypes(unit, ProjectEntry)) {
                    args[0].Call(
                        node,
                        unit,
                        null,
                        new IAnalysisSet[] { 
                            value, 
                            AnalysisSet.Empty, 
                            @this 
                        }
                    );
                }
            }
        }

        private static IAnalysisSet FsBasename(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            CallNode call = (CallNode)node;
            IAnalysisSet res = AnalysisSet.Empty;
            if (call.Arguments.Length == 2) {
                foreach (var extArg in args[1]) {
                    var strExt = extArg.Value.GetStringValue();
                    if (strExt != null) {
                        foreach (var nameArg in args[0]) {
                            string name = nameArg.Value.GetStringValue();
                            if (name != null) {
                                string oldName = name;
                                if (name.EndsWith(strExt, StringComparison.OrdinalIgnoreCase)) {
                                    name = name.Substring(0, name.Length - strExt.Length);
                                }
                                res = res.Union(unit.Analyzer.GetConstant(name).Proxy);
                            }
                        }
                    }
                }
            }
            if (res.Count == 0) {
                return unit.Analyzer._emptyStringValue.SelfSet;
            }
            return res;
        }

        private static IAnalysisSet FsReadDirSync(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            CallNode call = (CallNode)node;
            if (call.Arguments.Length == 1) {
                var ee = new ExpressionEvaluator(unit);
                IAnalysisSet arraySet;
                ReadDirSyncArrayValue array;
                if (!unit.GetDeclaringModuleEnvironment().TryGetNodeValue(NodeEnvironmentKind.ArrayValue, call, out arraySet)) {
                    array = new ReadDirSyncArrayValue(
                        unit.ProjectEntry,
                        node
                    );
                    arraySet = array.SelfSet;
                    unit.GetDeclaringModuleEnvironment().AddNodeValue(NodeEnvironmentKind.ArrayValue, call, arraySet);
                } else {
                    array = (ReadDirSyncArrayValue)arraySet.First().Value;
                }

                foreach (var path in ee.MergeStringLiterals(call.Arguments[0])) {
                    array.AddDirectoryMembers(unit, path);
                }

                return array.SelfSet;
            }
            return AnalysisSet.Empty;
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
                if (@this != null) {
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
            }
            return @this ?? AnalysisSet.Empty;
        }

        /// <summary>
        /// eventListener.emit(eventName, args...)
        /// 
        /// Returns bool indicating if event was raised.
        /// </summary>
        private static IAnalysisSet EventEmitterEmit(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 1) {
                if (@this != null) {
                    foreach (var thisArg in @this) {
                        ExpandoValue expando = @thisArg.Value as ExpandoValue;
                        if (expando != null) {
                            Debug.Assert(args[0].Count < 100);
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
            }

            return func.ProjectState._trueInst.Proxy;
        }
    }
}

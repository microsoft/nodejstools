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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;
using System.IO;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Analysis {
    partial class NodejsModuleBuilder {
        private static Dictionary<string, Dictionary<string, FunctionSpecializer>> _moduleSpecializations = new Dictionary<string, Dictionary<string, FunctionSpecializer>>() { 
            {
                "http",
                new Dictionary<string, FunctionSpecializer>() {
                    { "createServer", 
                       new CallbackFunctionSpecializer(
                           0,
                           new CallbackArgInfo("http", "IncomingMessage"),
                           new CallbackArgInfo("http", "ServerResponse")
                        ) 
                    },
                    { "request", 
                       new CallbackFunctionSpecializer(
                           1,
                           new CallbackArgInfo("http", "IncomingMessage")
                        ) 
                    },
                    { "get", 
                       new CallbackFunctionSpecializer(
                           1,
                           new CallbackArgInfo("http", "IncomingMessage")
                        ) 
                    }
                }
            },
            {
                "https",
                new Dictionary<string, FunctionSpecializer>() {
                    { "createServer", 
                       new CallbackFunctionSpecializer(
                           0,
                           new CallbackArgInfo("http", "ClientRequest"),
                           new CallbackArgInfo("http", "ServerResponse")
                        ) 
                    },
                    { "request", 
                       new CallbackFunctionSpecializer(
                           1,
                           new CallbackArgInfo("http", "IncomingMessage")
                        ) 
                    },
                    { "get", 
                       new CallbackFunctionSpecializer(
                           1,
                           new CallbackArgInfo("http", "IncomingMessage")
                        ) 
                    }
                }
            },
            {
                "net",
                new Dictionary<string, FunctionSpecializer>() {
                    {  "createServer", 
                        new CallbackFunctionSpecializer(
                           0,
                           new CallbackArgInfo("net", "Socket")
                        ) 
                    },
                    { "connect", 
                       new CallbackFunctionSpecializer(
                           1,
                           new CallbackArgInfo("net", "Socket")
                        ) 
                    },
                    { "createConnection", 
                       new CallbackFunctionSpecializer(
                           1,
                           new CallbackArgInfo("net", "Socket")
                        ) 
                    }
                }
            },
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

        private static Dictionary<string, Dictionary<string, PropertySpecializer>> _propertySpecializations = new Dictionary<string, Dictionary<string, PropertySpecializer>>() { 
            { 
                "process", 
                new Dictionary<string, PropertySpecializer>() {
                    { "platform", new ConstantSpecializer("win32") },
                    { "pid", ConstantSpecializer.Number },
                    { "maxTickDepth", ConstantSpecializer.Number },
                    { "title", ConstantSpecializer.String }
                }
            },
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

        abstract class PropertySpecializer {
            public abstract AnalysisValue Specialize(ProjectEntry projectEntry, string name);
        }

        class ConstantSpecializer : PropertySpecializer {
            private readonly object _value;
            public static ConstantSpecializer Number = new ConstantSpecializer(0.0);
            public static ConstantSpecializer String = new ConstantSpecializer("");

            public ConstantSpecializer(object value) {
                _value = value;
            }

            public static ConstantSpecializer Instance = new ConstantSpecializer("");

            public override AnalysisValue Specialize(ProjectEntry projectEntry, string name) {
                return projectEntry.Analyzer.GetConstant(_value);
            }
        }

        abstract class FunctionSpecializer {
            public abstract FunctionValue Specialize(ProjectEntry projectEntry, string name, string doc, AnalysisValue returnValue, ParameterResult[] parameters);
        }

        class CallableFunctionSpecializer : FunctionSpecializer {
            private readonly CallDelegate _delegate;

            public CallableFunctionSpecializer(CallDelegate callDelegate) {
                _delegate = callDelegate;
            }

            public override FunctionValue Specialize(ProjectEntry projectEntry, string name, string doc, AnalysisValue returnValue, ParameterResult[] parameters) {
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

        class CallbackFunctionSpecializer : FunctionSpecializer {
            private readonly int _index;
            private readonly CallbackArgInfo[] _args;
            
            public CallbackFunctionSpecializer(int argIndex, params CallbackArgInfo[] args) {
                _index = argIndex;
                _args = args;
            }

            public override FunctionValue Specialize(ProjectEntry projectEntry, string name, string doc, AnalysisValue returnValue, ParameterResult[] parameters) {
                return new CallbackReturningFunctionValue(
                    projectEntry,
                    name,
                    returnValue != null ? returnValue.SelfSet : AnalysisSet.Empty,
                    _index,
                    _args,
                    doc,
                    parameters
                );
            }
        }

        abstract class ReturnValueFunctionSpecializer : FunctionSpecializer {
            public static ReturnValueFunctionSpecializer String = new StringSpecializer();
            public static ReturnValueFunctionSpecializer Boolean = new BooleanSpecializer();

            public override FunctionValue Specialize(ProjectEntry projectEntry, string name, string doc, AnalysisValue returnValue, ParameterResult[] parameters) {
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
                    Directory.Exists(Path.Combine(Path.GetDirectoryName(unit.ProjectEntry.FilePath), path))) {
                    string trimmed = path.Trim();
                    if (trimmed != "." && trimmed != "/") {
                        string[] files;
                        try {
                            files = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(unit.ProjectEntry.FilePath), path));
                        } catch (IOException) {
                            return;
                        } catch (UnauthorizedAccessException) {
                            return;
                        }

                        if (IndexTypes.Length < files.Length) {
                            var types = IndexTypes;
                            Array.Resize(ref types, files.Length);
                            IndexTypes = types;
                                
                        }
                            
                        for (int i = 0; i < files.Length; i++) {
                            if (IndexTypes[i] == null) {
                                IndexTypes[i] = new TypedDef();
                            }
                            IndexTypes[i].AddTypes(
                                unit,
                                unit.Analyzer.GetConstant(Path.GetFileName(files[i])).SelfSet
                            );
                        }
                    }
                }
            }

            public override IAnalysisSet GetEnumerationValues(Node node, AnalysisUnit unit) {
                var res = (IAnalysisSet)unit.Analyzer._zeroIntValue.Proxy;
                for (int i = 1; i < _readDirs.Count; i++) {
                    res = res.Add(unit.Analyzer.GetConstant((double)i).Proxy);
                }
                return res;
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
            if (args.Length == 2) {
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
                        if (expando != null && args[0].Count < unit.Analyzer.Limits.MaxEvents) {
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

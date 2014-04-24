using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Builds the global object, builtin functions, etc...
    /// </summary>
    class GlobalBuilder {
        private readonly JsAnalyzer _analyzer;

        private GlobalBuilder(JsAnalyzer analyzer) {
            _analyzer = analyzer;
        }

        public static Globals MakeGlobal(JsAnalyzer analyzer) {
            return new GlobalBuilder(analyzer).MakeGlobal();
        }

        private Globals MakeGlobal() {
            JsAnalyzer analyzer = _analyzer;
            var builtinEntry = analyzer._builtinEntry;

            var stringValue = _analyzer.GetConstant("");
            var boolValue = _analyzer.GetConstant(true);
            var doubleValue = _analyzer.GetConstant(0.0);
            AnalysisValue numberPrototype, stringPrototype, booleanPrototype, functionPrototype;

            var globalObject = new ObjectValue(builtinEntry) {
                ArrayFunction(),
                BooleanFunction(out booleanPrototype),
                DateFunction(),
                ErrorFunction(),
                ErrorFunction("EvalError"),
                FunctionFunction(out functionPrototype),
                Member("Infinity", analyzer.GetConstant(double.PositiveInfinity)),
                Member("JSON", MakeJSONObject()),
                Member("Math", MakeMathObject()),
                Member("Infinity", analyzer.GetConstant(double.NaN)),
                NumberFunction(out numberPrototype),
                ObjectFunction(),
                ErrorFunction("RangeError"),
                ErrorFunction("ReferenceError"),
                RegExpFunction(),
                StringFunction(out stringPrototype),
                ErrorFunction("SyntaxError"),
                ErrorFunction("TypeError"),
                ErrorFunction("URIError"),
                ReturningFunction("decodeURI", stringValue),
                ReturningFunction("decodeURIComponent", stringValue),
                ReturningFunction("encodeURI", stringValue),
                ReturningFunction("encodeURIComponent", stringValue),
                ReturningFunction("escape", stringValue),
                BuiltinFunction("eval"),
                ReturningFunction("isFinite", boolValue),
                ReturningFunction("isNaN", boolValue),
                ReturningFunction("parseFloat", doubleValue),
                ReturningFunction("parseInt", doubleValue),
                ReturningFunction("unescape", stringValue),
                Member("undefined", analyzer._undefined),

                SpecializedFunction("require", Require)
            };

            // aliases for global object:
            globalObject.Add("GLOBAL", globalObject);
            globalObject.Add("global", globalObject);
            globalObject.Add("root", globalObject);

            // Node specific stuff:
            //'setImmediate',
            //'setInterval',
            //'setTimeout',
            //'url',
            //'module',
            //'clearImmediate',
            //'clearInterval',
            //'clearTimeout',
            //'ArrayBuffer',
            //'Buffer',
            //'Float32Array',
            //'Float64Array',
            //'Int16Array',
            //'Int32Array',
            //'Int8Array',
            //'Uint16Array',
            //'Uint32Array',
            //'Uint8Array',
            //'Uint8ClampedArray',
            //'COUNTER_HTTP_CLIENT_REQUEST',
            //'COUNTER_HTTP_CLIENT_RESPONSE',
            //'COUNTER_HTTP_SERVER_REQUEST',
            //'COUNTER_HTTP_SERVER_RESPONSE',
            //'COUNTER_NET_SERVER_CONNECTION',
            //'COUNTER_NET_SERVER_CONNECTION_CLOSE',
            //'DTRACE_HTTP_CLIENT_REQUEST',
            //'DTRACE_HTTP_CLIENT_RESPONSE',
            //'DTRACE_HTTP_SERVER_REQUEST',
            //'DTRACE_HTTP_SERVER_RESPONSE',
            //'DTRACE_NET_SERVER_CONNECTION',
            //'DTRACE_NET_SOCKET_READ',
            //'DTRACE_NET_SOCKET_WRITE',
            //'DTRACE_NET_STREAM_END',
            //'DataView',

            // Node modules:
            //'buffer',
            //'child_process',
            //'string_decoder',
            //'querystring',
            //'console',
            //'cluster',
            //'assert',
            //'fs',
            //'punycode',
            //'events',
            //'dgram',
            //'dns',
            //'domain',
            //'path',
            //'process',
            //'http',
            //'https',
            //'net',
            //'os',
            //'crypto',
            //'readline',
            //'require',
            //'stream',
            //'tls',
            //'tty',
            //'util',
            //'vm',
            //'zlib' ]
            return new Globals(globalObject, numberPrototype, stringPrototype, booleanPrototype, functionPrototype);
        }

        private BuiltinFunctionValue ArrayFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(builtinEntry, "Array") { 
                Member("prototype", 
                    new ObjectValue(builtinEntry) {
                        BuiltinFunction("concat"),
                        BuiltinFunction("constructor"),
                        BuiltinFunction("every"),
                        BuiltinFunction("filter"),
                        BuiltinFunction("forEach"),
                        BuiltinFunction("indexOf"),
                        BuiltinFunction("join"),
                        BuiltinFunction("lastIndexOf"),
                        BuiltinFunction("length"),
                        BuiltinFunction("map"),
                        BuiltinFunction("pop"),
                        BuiltinFunction("push"),
                        BuiltinFunction("reduce"),
                        BuiltinFunction("reduceRight"),
                        BuiltinFunction("reverse"),
                        BuiltinFunction("shift"),
                        BuiltinFunction("slice"),
                        BuiltinFunction("some"),
                        BuiltinFunction("sort"),
                        BuiltinFunction("splice"),
                        BuiltinFunction("toLocaleString"),
                        BuiltinFunction("toString"),
                        BuiltinFunction("unshift"),
                    }
                ),
                new ReturningFunctionValue(builtinEntry, "isArray", _analyzer._falseInst)
            };
        }

        private BuiltinFunctionValue BooleanFunction(out AnalysisValue booleanPrototype) {
            var builtinEntry = _analyzer._builtinEntry;
            var prototype = Member("prototype",
                new ObjectValue(builtinEntry) {
                    BuiltinFunction("constructor"),
                    BuiltinFunction("toString"),
                    BuiltinFunction("valueOf"),
                }
            );
            booleanPrototype = prototype.Value;
            return new BuiltinFunctionValue(builtinEntry, "Boolean") { 
                prototype
            };
        }

        private BuiltinFunctionValue DateFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(builtinEntry, "Date") { 
                Member("prototype", 
                    new ObjectValue(builtinEntry) {
                        BuiltinFunction("constructor"),
                        BuiltinFunction("getDate"),
                        BuiltinFunction("getDay"),
                        BuiltinFunction("getFullYear"),
                        BuiltinFunction("getHours"),
                        BuiltinFunction("getMilliseconds"),
                        BuiltinFunction("getMinutes"),
                        BuiltinFunction("getMonth"),
                        BuiltinFunction("getSeconds"),
                        BuiltinFunction("getTime"),
                        BuiltinFunction("getTimezoneOffset"),
                        BuiltinFunction("getUTCDate"),
                        BuiltinFunction("getUTCDay"),
                        BuiltinFunction("getUTCFullYear"),
                        BuiltinFunction("getUTCHours"),
                        BuiltinFunction("getUTCMilliseconds"),
                        BuiltinFunction("getUTCMinutes"),
                        BuiltinFunction("getUTCMonth"),
                        BuiltinFunction("getUTCSeconds"),
                        BuiltinFunction("getYear"),
                        BuiltinFunction("setDate"),
                        BuiltinFunction("setFullYear"),
                        BuiltinFunction("setHours"),
                        BuiltinFunction("setMilliseconds"),
                        BuiltinFunction("setMinutes"),
                        BuiltinFunction("setMonth"),
                        BuiltinFunction("setSeconds"),
                        BuiltinFunction("setTime"),
                        BuiltinFunction("setUTCDate"),
                        BuiltinFunction("setUTCFullYear"),
                        BuiltinFunction("setUTCHours"),
                        BuiltinFunction("setUTCMilliseconds"),
                        BuiltinFunction("setUTCMinutes"),
                        BuiltinFunction("setUTCMonth"),
                        BuiltinFunction("setUTCSeconds"),
                        BuiltinFunction("setYear"),
                        BuiltinFunction("toDateString"),
                        BuiltinFunction("toGMTString"),
                        BuiltinFunction("toISOString"),
                        BuiltinFunction("toJSON"),
                        BuiltinFunction("toLocaleDateString"),
                        BuiltinFunction("toLocaleString"),
                        BuiltinFunction("toLocaleTimeString"),
                        BuiltinFunction("toString"),
                        BuiltinFunction("toTimeString"),
                        BuiltinFunction("toUTCString"),
                        BuiltinFunction("valueOf"),
                    }
                )
            };
        }

        private BuiltinFunctionValue ErrorFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(builtinEntry, "Error") { 
                Member("prototype", 
                    new ObjectValue(builtinEntry) {
                        BuiltinFunction("constructor"),
                        BuiltinFunction("message"),
                        BuiltinFunction("name"),
                        BuiltinFunction("toString"),
                    }
                ),
                new BuiltinFunctionValue(builtinEntry, "captureStackTrace"),
                Member("stackTraceLimit", _analyzer.GetConstant(10.0))
            };
        }

        private BuiltinFunctionValue ErrorFunction(string errorName) {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(builtinEntry, errorName) { 
                Member("prototype", 
                    new ObjectValue(builtinEntry) {
                        BuiltinFunction("arguments"),
                        BuiltinFunction("constructor"),
                        BuiltinFunction("name"),
                        BuiltinFunction("stack"),
                        BuiltinFunction("type"),
                    }
                )
            };
        }

        private BuiltinFunctionValue FunctionFunction(out AnalysisValue functionPrototype) {
            var builtinEntry = _analyzer._builtinEntry;
            var prototype = Member("prototype",
                new ReturningFunctionValue(builtinEntry, "Empty", _analyzer._undefined) {
                    BuiltinFunction("apply"),
                    BuiltinFunction("bind"),
                    BuiltinFunction("call"),
                    BuiltinFunction("constructor"),
                    BuiltinFunction("toString"),
                }
            );
            functionPrototype = prototype.Value;
            return new BuiltinFunctionValue(builtinEntry, "Function") { 
                prototype
            };
        }

        private ObjectValue MakeJSONObject() {
            var builtinEntry = _analyzer._builtinEntry;

            // TODO: Should we see if we have something that we should parse?
            // TODO: Should we have a per-node value for the result of parse?
            var parseResult = new ObjectValue(builtinEntry);
            return new ObjectValue(builtinEntry) { 
                ReturningFunction("parse", parseResult),
                ReturningFunction("stringify", _analyzer.GetConstant("")),
            };
        }

        private ObjectValue MakeMathObject() {
            var builtinEntry = _analyzer._builtinEntry;

            var doubleResult = _analyzer.GetConstant(0.0);
            return new ObjectValue(builtinEntry) { 
                Member("E", _analyzer.GetConstant(Math.E)),
                Member("LN10", doubleResult),
                Member("LN2", doubleResult),
                Member("LOG2E", doubleResult),
                Member("LOG10", doubleResult),
                Member("PI", _analyzer.GetConstant(Math.PI)),
                Member("SQRT1_2", _analyzer.GetConstant(Math.Sqrt(1.0/2.0))),
                Member("SQRT2", _analyzer.GetConstant(Math.Sqrt(2))),
                ReturningFunction("random", doubleResult),
                ReturningFunction("abs", doubleResult),
                ReturningFunction("acos", doubleResult),
                ReturningFunction("asin", doubleResult),
                ReturningFunction("atan", doubleResult),
                ReturningFunction("ceil", doubleResult),
                ReturningFunction("cos", doubleResult),
                ReturningFunction("exp", doubleResult),
                ReturningFunction("floor", doubleResult),
                ReturningFunction("log", doubleResult),
                ReturningFunction("round", doubleResult),
                ReturningFunction("sin", doubleResult),
                ReturningFunction("sqrt", doubleResult),
                ReturningFunction("tan", doubleResult),
                ReturningFunction("atan2", doubleResult),
                ReturningFunction("pow", doubleResult),
                ReturningFunction("max", doubleResult),
                ReturningFunction("min", doubleResult),
            };
        }

        private BuiltinFunctionValue NumberFunction(out AnalysisValue numberPrototype) {
            var builtinEntry = _analyzer._builtinEntry;

            var prototype = Member("prototype", 
                new ObjectValue(builtinEntry) {
                    BuiltinFunction("constructor"),
                    BuiltinFunction("toExponential"),
                    BuiltinFunction("toFixed"),
                    BuiltinFunction("toLocaleString"),
                    BuiltinFunction("toPrecision"),
                    BuiltinFunction("toString"),
                    BuiltinFunction("valueOf"),
                }
            );
            numberPrototype = prototype.Value;

            return new BuiltinFunctionValue(builtinEntry, "Number") { 
                prototype,
                Member("length", _analyzer.GetConstant(1.0)),
                Member("name", _analyzer.GetConstant("Number")),
                Member("arguments", _analyzer._nullInst),
                Member("caller", _analyzer._nullInst),
                Member("prototype", new ObjectValue(builtinEntry)),
                Member("MAX_VALUE", _analyzer.GetConstant(Double.MaxValue)),
                Member("MIN_VALUE", _analyzer.GetConstant(Double.MinValue)),
                Member("NaN", _analyzer.GetConstant(Double.NaN)),
                Member("NEGATIVE_INFINITY", _analyzer.GetConstant(Double.NegativeInfinity)),
                Member("POSITIVE_INFINITY", _analyzer.GetConstant(Double.PositiveInfinity)),
                ReturningFunction("isFinite", _analyzer._trueInst),
                ReturningFunction("isNaN", _analyzer._falseInst),
            };
        }

        private BuiltinFunctionValue ObjectFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(builtinEntry, "Object") { 
                Member("prototype", new ObjectValue(builtinEntry)),
                BuiltinFunction("getPrototypeOf"),
                BuiltinFunction("getOwnPropertyDescriptor"),
                BuiltinFunction("getOwnPropertyNames"),
                BuiltinFunction("create"),
                SpecializedFunction("defineProperty", DefineProperty),
                BuiltinFunction("defineProperties"),
                BuiltinFunction("seal"),
                BuiltinFunction("freeze"),
                BuiltinFunction("preventExtensions"),
                BuiltinFunction("isSealed"),
                BuiltinFunction("isFrozen"),
                BuiltinFunction("isExtensible"),
                BuiltinFunction("keys"),
                BuiltinFunction("is"),
            };
        }

        private static IAnalysisSet DefineProperty(SpecializedFunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // object, name, property desc
            if (args.Length >= 3) {
                foreach (var obj in args[0]) {
                    ExpandoValue expando = obj as ExpandoValue;
                    if (expando != null) {
                        foreach (var name in args[1]) {
                            string propName = name.GetConstantValueAsString();
                            if (propName != null) {
                                foreach (var desc in args[2]) {
                                    expando.AddProperty(node, unit, propName, desc);
                                }
                            }
                        }
                    }
                }
            }
            if (args.Length > 0) {
                return args[0];
            }
            return AnalysisSet.Empty;
        }

        private static IAnalysisSet Require(SpecializedFunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet res = AnalysisSet.Empty;
            if (args.Length > 0) {
                foreach (var arg in args[0]) {
                    var moduleName = arg.GetConstantValueAsString();
                    if (moduleName != null) {
                        res = res.Union(
                            unit.Analyzer.Modules.RequireModule(
                                node,
                                unit,
                                moduleName, 
                                unit.DeclaringModule.Name
                            )
                        );
                    }
                }
            }
            return res;
        }

        private BuiltinFunctionValue RegExpFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(builtinEntry, "RegExp") { 
                Member("prototype", 
                    new ObjectValue(builtinEntry) {
                        BuiltinFunction("compile"),   
                        BuiltinFunction("constructor"),   
                        BuiltinFunction("exec"),  
                        BuiltinFunction("global"),  
                        BuiltinFunction("ignoreCase"),  
                        BuiltinFunction("lastIndex"),  
                        BuiltinFunction("multiline"),  
                        BuiltinFunction("source"),  
                        BuiltinFunction("test"),  
                        BuiltinFunction("toString") 
                    }
                ),
// TODO:   input: [Getter/Setter],
//  lastMatch: [Getter/Setter],
//  lastParen: [Getter/Setter],
//  leftContext: [Getter/Setter],
//  rightContext: [Getter/Setter],
//  '$1': [Getter/Setter],
//  '$2': [Getter/Setter],
//  '$3': [Getter/Setter],
//  '$4': [Getter/Setter],
//  '$5': [Getter/Setter],
//  '$6': [Getter/Setter],
//  '$7': [Getter/Setter],
//  '$8': [Getter/Setter],
//  '$9': [Getter/Setter] }
//[ '$&',
//  '$\'',
//  '$*',
//  '$+',
//  '$_',
//  '$`',
//  '$input',
                BuiltinProperty("multiline", _analyzer._falseInst),
                BuiltinFunction("arguments"),
                BuiltinFunction("caller"),
                BuiltinFunction("input"),
                BuiltinFunction("lastMatch"),
                BuiltinFunction("lastParen"),
                BuiltinFunction("leftContext"),
                BuiltinFunction("length"),
                BuiltinFunction("multiline"),
                BuiltinFunction("name"),
                BuiltinFunction("rightContext") 
            };
        }

        private BuiltinFunctionValue StringFunction(out AnalysisValue stringPrototype) {
            var builtinEntry = _analyzer._builtinEntry;
            var prototype = Member("prototype", 
                new ObjectValue(builtinEntry) {
                    BuiltinFunction("anchor"),
                    BuiltinFunction("big"),
                    BuiltinFunction("blink"),
                    BuiltinFunction("bold"),
                    BuiltinFunction("charAt"),
                    BuiltinFunction("charCodeAt"),
                    BuiltinFunction("concat"),
                    BuiltinFunction("constructor"),
                    BuiltinFunction("fixed"),
                    BuiltinFunction("fontcolor"),
                    BuiltinFunction("fontsize"),
                    BuiltinFunction("indexOf"),
                    BuiltinFunction("italics"),
                    BuiltinFunction("lastIndexOf"),
                    BuiltinFunction("length"),
                    BuiltinFunction("link"),
                    BuiltinFunction("localeCompare"),
                    BuiltinFunction("match"),
                    BuiltinFunction("replace"),
                    BuiltinFunction("search"),
                    BuiltinFunction("slice"),
                    BuiltinFunction("small"),
                    BuiltinFunction("split"),
                    BuiltinFunction("strike"),
                    BuiltinFunction("sub"),
                    BuiltinFunction("substr"),
                    BuiltinFunction("substring"),
                    BuiltinFunction("sup"),
                    BuiltinFunction("toLocaleLowerCase"),
                    BuiltinFunction("toLocaleUpperCase"),
                    BuiltinFunction("toLowerCase"),
                    BuiltinFunction("toString"),
                    BuiltinFunction("toUpperCase"),
                    BuiltinFunction("trim"),
                    BuiltinFunction("trimLeft"),
                    BuiltinFunction("trimRight"),
                    BuiltinFunction("valueOf"),
                }
            );
            stringPrototype = prototype.Value;

            return new BuiltinFunctionValue(builtinEntry, "String") { 
                prototype,
                ReturningFunction("fromCharCode", _analyzer.GetConstant("")),
            };
        }

        #region Building Helpers

        private static MemberAddInfo Member(string name, AnalysisValue value) {
            return new MemberAddInfo(name, value);
        }

        private BuiltinFunctionValue BuiltinFunction(string name) {
            return new BuiltinFunctionValue(_analyzer._builtinEntry, name);
        }

        private BuiltinFunctionValue ReturningFunction(string name, AnalysisValue value) {
            return new ReturningFunctionValue(_analyzer._builtinEntry, name, value);
        }

        private BuiltinFunctionValue SpecializedFunction(string name, Func<SpecializedFunctionValue, Node, AnalysisUnit, IAnalysisSet, IAnalysisSet[], IAnalysisSet> value) {
            return new SpecializedFunctionValue(_analyzer._builtinEntry, name, value);
        }

        private MemberAddInfo BuiltinProperty(string name, AnalysisValue propertyType) {
            return new MemberAddInfo(name, propertyType, isProperty: true);
        }

        #endregion
    }

    class Globals {
        public readonly ObjectValue GlobalObject;
        public readonly AnalysisValue NumberPrototype,
            StringPrototype,
            BooleanPrototype,
            FunctionPrototype;

        public Globals(ObjectValue globalObject, AnalysisValue numberPrototype, AnalysisValue stringPrototype, AnalysisValue booleanPrototype, AnalysisValue functionPrototype) {
            GlobalObject = globalObject;
            NumberPrototype = numberPrototype;
            StringPrototype = stringPrototype;
            BooleanPrototype = booleanPrototype;
            FunctionPrototype = functionPrototype;
        }
    }
}

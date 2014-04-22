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

        public static AnalysisValue MakeGlobal(JsAnalyzer analyzer) {
            return new GlobalBuilder(analyzer).MakeGlobal();
        }

        private AnalysisValue MakeGlobal() {
            JsAnalyzer analyzer = _analyzer;
            var builtinEntry = analyzer._builtinEntry;

            var stringValue = _analyzer.GetConstant("");
            var boolValue = _analyzer.GetConstant(true);
            var doubleValue = _analyzer.GetConstant(0.0);
            var res = new ObjectInfo(builtinEntry) {
                ArrayFunction(),
                BooleanFunction(),
                DateFunction(),
                ErrorFunction(),
                ErrorFunction("EvalError"),
                FunctionFunction(),
                Member("Infinity", analyzer.GetConstant(double.PositiveInfinity)),
                Member("JSON", MakeJSONObject()),
                Member("Math", MakeMathObject()),
                Member("Infinity", analyzer.GetConstant(double.NaN)),
                NumberFunction(),
                ObjectFunction(),
                ErrorFunction("RangeError"),
                ErrorFunction("ReferenceError"),
                RegExpFunction(),
                StringFunction(),
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
            res.Add("GLOBAL", res);
            res.Add("global", res);
            res.Add("root", res);

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
            return res;
        }

        private BuiltinFunctionInfo ArrayFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "Array") { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
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
                new ReturningFunctionInfo(builtinEntry, "isArray", _analyzer._falseInst)
            };
        }

        private BuiltinFunctionInfo BooleanFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "Boolean") { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
                        BuiltinFunction("constructor"),
                        BuiltinFunction("toString"),
                        BuiltinFunction("valueOf"),
                    }
                )
            };
        }

        private BuiltinFunctionInfo DateFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "Date") { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
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

        private BuiltinFunctionInfo ErrorFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "Error") { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
                        BuiltinFunction("constructor"),
                        BuiltinFunction("message"),
                        BuiltinFunction("name"),
                        BuiltinFunction("toString"),
                    }
                ),
                new BuiltinFunctionInfo(builtinEntry, "captureStackTrace"),
                Member("stackTraceLimit", _analyzer.GetConstant(10.0))
            };
        }

        private BuiltinFunctionInfo ErrorFunction(string errorName) {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, errorName) { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
                        BuiltinFunction("arguments"),
                        BuiltinFunction("constructor"),
                        BuiltinFunction("name"),
                        BuiltinFunction("stack"),
                        BuiltinFunction("type"),
                    }
                )
            };
        }

        private BuiltinFunctionInfo FunctionFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "Function") { 
                Member("prototype", 
                    new ReturningFunctionInfo(builtinEntry, "Empty", _analyzer._undefined) {
                        BuiltinFunction("apply"),
                        BuiltinFunction("bind"),
                        BuiltinFunction("call"),
                        BuiltinFunction("constructor"),
                        BuiltinFunction("toString"),
                    }
                )
            };
        }

        private ObjectInfo MakeJSONObject() {
            var builtinEntry = _analyzer._builtinEntry;

            // TODO: Should we see if we have something that we should parse?
            // TODO: Should we have a per-node value for the result of parse?
            var parseResult = new ObjectInfo(builtinEntry);
            return new ObjectInfo(builtinEntry) { 
                ReturningFunction("parse", parseResult),
                ReturningFunction("stringify", _analyzer.GetConstant("")),
            };
        }

        private ObjectInfo MakeMathObject() {
            var builtinEntry = _analyzer._builtinEntry;

            var doubleResult = _analyzer.GetConstant(0.0);
            return new ObjectInfo(builtinEntry) { 
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

        private BuiltinFunctionInfo NumberFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "Number") { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
                        BuiltinFunction("constructor"),
                        BuiltinFunction("toExponential"),
                        BuiltinFunction("toFixed"),
                        BuiltinFunction("toLocaleString"),
                        BuiltinFunction("toPrecision"),
                        BuiltinFunction("toString"),
                        BuiltinFunction("valueOf"),
                    }
                ),
                Member("length", _analyzer.GetConstant(1.0)),
                Member("name", _analyzer.GetConstant("Number")),
                Member("arguments", _analyzer._nullInst),
                Member("caller", _analyzer._nullInst),
                Member("prototype", new ObjectInfo(builtinEntry)),
                Member("MAX_VALUE", _analyzer.GetConstant(Double.MaxValue)),
                Member("MIN_VALUE", _analyzer.GetConstant(Double.MinValue)),
                Member("NaN", _analyzer.GetConstant(Double.NaN)),
                Member("NEGATIVE_INFINITY", _analyzer.GetConstant(Double.NegativeInfinity)),
                Member("POSITIVE_INFINITY", _analyzer.GetConstant(Double.PositiveInfinity)),
                ReturningFunction("isFinite", _analyzer._trueInst),
                ReturningFunction("isNaN", _analyzer._falseInst),
            };
        }

        private BuiltinFunctionInfo ObjectFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "Object") { 
                Member("prototype", new ObjectInfo(builtinEntry)),
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

        private static IAnalysisSet DefineProperty(SpecializedFunctionInfo func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
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

        private static IAnalysisSet Require(SpecializedFunctionInfo func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
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

        private BuiltinFunctionInfo RegExpFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "RegExp") { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
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

        private BuiltinFunctionInfo StringFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionInfo(builtinEntry, "String") { 
                Member("prototype", 
                    new ObjectInfo(builtinEntry) {
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
                ),
                ReturningFunction("fromCharCode", _analyzer.GetConstant("")),
            };
        }

        #region Building Helpers

        private static MemberAddInfo Member(string name, AnalysisValue value) {
            return new MemberAddInfo(name, value);
        }

        private BuiltinFunctionInfo BuiltinFunction(string name) {
            return new BuiltinFunctionInfo(_analyzer._builtinEntry, name);
        }

        private BuiltinFunctionInfo ReturningFunction(string name, AnalysisValue value) {
            return new ReturningFunctionInfo(_analyzer._builtinEntry, name, value);
        }

        private BuiltinFunctionInfo SpecializedFunction(string name, Func<SpecializedFunctionInfo, Node, AnalysisUnit, IAnalysisSet, IAnalysisSet[], IAnalysisSet> value) {
            return new SpecializedFunctionInfo(_analyzer._builtinEntry, name, value);
        }

        private MemberAddInfo BuiltinProperty(string name, AnalysisValue propertyType) {
            return new MemberAddInfo(name, propertyType, isProperty: true);
        }

        #endregion
    }
}

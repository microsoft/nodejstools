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
using System.Globalization;

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

            var stringValue = _analyzer.GetConstant(String.Empty);
            var boolValue = _analyzer.GetConstant(true);
            var doubleValue = _analyzer.GetConstant(0.0);
            AnalysisValue numberPrototype, stringPrototype, booleanPrototype, functionPrototype;
            ExpandoValue arrayPrototype;
            FunctionValue arrayFunction;
            ObjectValue objectPrototype;
            BuiltinFunctionValue requireFunc;
            BuiltinFunctionValue getOwnPropertyDescriptor;

            var globalObject = new GlobalValue(builtinEntry) {
                (arrayFunction = ArrayFunction(out arrayPrototype)),
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
                ObjectFunction(out objectPrototype, out getOwnPropertyDescriptor),
                ErrorFunction("RangeError"),
                ErrorFunction("ReferenceError"),
                RegExpFunction(),
                StringFunction(out stringPrototype),
                ErrorFunction("SyntaxError"),
                ErrorFunction("TypeError"),
                ErrorFunction("URIError"),
                ReturningFunction(
                    "decodeURI", 
                    stringValue,
                    "Gets the unencoded version of an encoded Uniform Resource Identifier (URI).",
                    Parameter("encodedURI", "A value representing an encoded URI.")
                ),
                ReturningFunction(
                    "decodeURIComponent", 
                    stringValue,
                    "Gets the unencoded version of an encoded component of a Uniform Resource Identifier (URI).",
                    Parameter("encodedURIComponent", "A value representing an encoded URI component.")
                ),
                ReturningFunction(
                    "encodeURI", 
                    stringValue,
                    "Encodes a text string as a valid Uniform Resource Identifier (URI)",
                    Parameter("uri", "A value representing an encoded URI.")
                ),
                ReturningFunction(
                    "encodeURIComponent", 
                    stringValue,
                    "Encodes a text string as a valid component of a Uniform Resource Identifier (URI).",
                    Parameter("uriComponent", "A value representing an encoded URI component.")
                ),
                ReturningFunction("escape", stringValue),
                BuiltinFunction(
                    "eval",
                    "Evaluates JavaScript code and executes it.",
                    Parameter("x", "A String value that contains valid JavaScript code.")
                ),
                ReturningFunction(
                    "isFinite", 
                    boolValue,
                    "Determines whether a supplied number is finite.",
                    Parameter("number", "Any numeric value.")
                ),
                ReturningFunction(
                    "isNaN", 
                    boolValue,
                    "Returns a Boolean value that indicates whether a value is the reserved value NaN (not a number).",
                    Parameter("number", "A numeric value.")
                ),
                ReturningFunction(
                    "parseFloat", 
                    doubleValue,
                    "Converts a string to a floating-point number.",
                    Parameter("string", "A string that contains a floating-point number.")
                ),
                ReturningFunction(
                    "parseInt", 
                    doubleValue,
                    "Converts A string to an integer.",
                    Parameter("s", "A string to convert into a number."),
                    Parameter("radix", @"A value between 2 and 36 that specifies the base of the number in numString. 
If this argument is not supplied, strings with a prefix of '0x' are considered hexadecimal.
All other strings are considered decimal.", isOptional:true)
                ),
                ReturningFunction("unescape", stringValue),
                Member("undefined", analyzer._undefined),

                (requireFunc = SpecializedFunction("require", Require))
            };

            // aliases for global object:
            globalObject.Add("GLOBAL", globalObject.Proxy);
            globalObject.Add("global", globalObject.Proxy);
            globalObject.Add("root", globalObject.Proxy);

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
            return new Globals(
                globalObject, 
                numberPrototype, 
                stringPrototype, 
                booleanPrototype, 
                functionPrototype,
                arrayFunction,
                objectPrototype,
                requireFunc,
                arrayPrototype,
                getOwnPropertyDescriptor
            );
        }

        private BuiltinFunctionValue ArrayFunction(out ExpandoValue arrayPrototype) {
            var builtinEntry = _analyzer._builtinEntry;

            return new SpecializedFunctionValue(
                builtinEntry,
                "Array",
                NewArray,
                null,
                arrayPrototype = new BuiltinObjectValue(builtinEntry) {
                        SpecializedFunction(
                            "concat",
                            ArrayConcat,
                            "Combines two or more arrays.",
                            Parameter("item"),
                            Parameter("item...")
                        ),
                        BuiltinFunction("constructor"),
                        ReturningFunction(
                            "every",
                            _analyzer._trueInst,
                            "Determines whether all the members of an array satisfy the specified test.",
                            Parameter("callbackfn", "A function that accepts up to three arguments. The every method calls the callbackfn function for each element in array1 until the callbackfn returns false, or until the end of the array."),
                            Parameter("thisArg", "An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.", isOptional:true)
                        ),
                        BuiltinFunction(
                            "filter",
                            "Returns the elements of an array that meet the condition specified in a callback function.",
                            Parameter("callbackfn", "A function that accepts up to three arguments. The filter method calls the callbackfn function one time for each element in the array."),
                            Parameter("thisArg", "An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.", isOptional:true)
                        ),
                        SpecializedFunction(
                            "forEach", 
                            ArrayForEach,
                            "Performs the specified action for each element in an array.",
                            Parameter("callbackfn", "A function that accepts up to three arguments. forEach calls the callbackfn function one time for each element in the array."),
                            Parameter("thisArg", "An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.")
                        ),
                        ReturningFunction(
                            "indexOf",
                            _analyzer._zeroIntValue,
                            "Returns the index of the first occurrence of a value in an array.",
                            Parameter("searchElement", "The value to locate in the array."),
                            Parameter("fromIndex", "The array index at which to begin the search. If fromIndex is omitted, the search starts at index 0.", isOptional: true)
                        ),
                        ReturningFunction(
                            "join",
                            _analyzer._emptyStringValue,
                            "Adds all the elements of an array separated by the specified separator string.",
                            Parameter("separator", "A string used to separate one element of an array from the next in the resulting String. If omitted, the array elements are separated with a comma.", isOptional: true)
                        ),
                        ReturningFunction(
                            "lastIndexOf",
                            _analyzer._zeroIntValue,
                            "Returns the index of the last occurrence of a specified value in an array.",
                            Parameter("searchElement", "The value to locate in the array."),
                            Parameter("fromIndex", "The array index at which to begin the search. If fromIndex is omitted, the search starts at the last index in the array.", isOptional: true)
                        ),
                        BuiltinProperty(
                            "length", 
                            _analyzer._zeroIntValue,
                            "Gets or sets the length of the array. This is a number one higher than the highest element defined in an array."
                        ),
                        BuiltinFunction(
                            "map",
                            "Calls a defined callback function on each element of an array, and returns an array that contains the results.",
                            Parameter("callbackfn", "A function that accepts up to three arguments. The map method calls the callbackfn function one time for each element in the array."),
                            Parameter("thisArg", "An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.")
                        ),
                        SpecializedFunction(
                            "pop",
                            ArrayPopFunction,
                            "Removes the last element from an array and returns it"
                        ),                        
                        SpecializedFunction(
                            "push",
                            ArrayPushFunction,
                            "Appends new elements to an array, and returns the new length of the array.",
                            Parameter("item", "New element of the Array."),
                            Parameter("item...",  "New element of the Array.", isOptional: true)
                        ),
                        BuiltinFunction(
                            "reduce",
                            "Calls the specified callback function for all the elements in an array. The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.",
                            Parameter("callbackfn", "A function that accepts up to four arguments. The reduce method calls the callbackfn function one time for each element in the array."),
                            Parameter("initialValue", "If initialValue is specified, it is used as the initial value to start the accumulation. The first call to the callbackfn function provides this value as an argument instead of an array value.", isOptional: true)
                        ),
                        BuiltinFunction(
                            "reduceRight",
                            "Calls the specified callback function for all the elements in an array, in descending order. The return value of the callback function is the accumulated result, and is provided as an argument in the next call to the callback function.",
                            Parameter("callbackfn", "A function that accepts up to four arguments. The reduceRight method calls the callbackfn function one time for each element in the array."),
                            Parameter("initialValue", "If initialValue is specified, it is used as the initial value to start the accumulation. The first call to the callbackfn function provides this value as an argument instead of an array value.", isOptional: true)
                        ),
                        BuiltinFunction(
                            "reverse", 
                            "Reverses the elements in an Array."
                        ),
                        BuiltinFunction(
                            "shift", 
                            "Removes the first element from an array and returns it."
                        ),
                        SpecializedFunction(
                            "slice",
                            ArraySliceFunction,
                            "Returns a section of an array.",
                            Parameter("start", "The beginning of the specified portion of the array.", isOptional: true),
                            Parameter("end", "end The end of the specified portion of the array.", isOptional: true)
                        ),
                        ReturningFunction(
                            "some",
                            _analyzer._trueInst,
                            "Determines whether the specified callback function returns true for any element of an array.",
                            Parameter("callbackfn", "A function that accepts up to three arguments. The some method calls the callbackfn function for each element in array1 until the callbackfn returns true, or until the end of the array."),
                            Parameter("thisArg", "An object to which the this keyword can refer in the callbackfn function. If thisArg is omitted, undefined is used as the this value.", isOptional:true)
                        ),
                        BuiltinFunction(
                            "sort",
                            "Sorts an array.",
                            Parameter("compareFn", "The function used to determine the order of the elements. If omitted, the elements are sorted in ascending, ASCII character order.")
                        ),
                        BuiltinFunction(
                            "splice", 
                            "Removes elements from an array and, if necessary, inserts new elements in their place, returning the deleted elements.",
                            Parameter("start", "The zero-based location in the array from which to start removing elements.")
                        ),
                        BuiltinFunction("toLocaleString"),
                        ReturningFunction(
                            "toString", 
                            _analyzer._emptyStringValue,
                            "Returns a string representation of an array."
                        ),
                        BuiltinFunction(
                            "unshift",
                            "Inserts new elements at the start of an array.",
                            Parameter("item...", "Element to insert at the start of the Array.")
                        ),
                }) { 
                new ReturningFunctionValue(builtinEntry, "isArray", _analyzer._falseInst.Proxy)
            };
        }

        private static IAnalysisSet NewArray(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet value;
            if (!unit._env.GlobalEnvironment.TryGetNodeValue(NodeEnvironmentKind.ArrayCall, node, out value)) {
                var arrValue = new ArrayValue(
                    new[] { new TypedDef() },
                    unit.ProjectEntry,
                    node
                );
                value = arrValue.SelfSet;
                unit._env.GlobalEnvironment.AddNodeValue(NodeEnvironmentKind.ArrayCall, node, arrValue.SelfSet);
            }

            return value;
        }

        private static IAnalysisSet NewObject(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet value;
            if (!unit._env.GlobalEnvironment.TryGetNodeValue(NodeEnvironmentKind.ObjectCall, node, out value)) {
                var objValue = new ObjectLiteralValue(
                    unit.ProjectEntry,
                    node
                );
                value = objValue.SelfSet;
                unit._env.GlobalEnvironment.AddNodeValue(NodeEnvironmentKind.ObjectCall, node, objValue.SelfSet);
            }

            return value;
        }


        private static IAnalysisSet ArrayPushFunction(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (args.Length >= 1 && @this != null) {                
                foreach (var thisValue in @this) {
                    ArrayValue arr = thisValue.Value as ArrayValue;
                    if (arr != null) {
                        arr.PushValue(node, unit, args[0]);
                    }
                }
            }
            return unit.Analyzer._zeroIntValue.SelfSet;
        }

        private static IAnalysisSet ArrayPopFunction(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet res = AnalysisSet.Empty;
            if (@this != null) {
                foreach (var thisValue in @this) {
                    ArrayValue arr = thisValue.Value as ArrayValue;
                    if (arr != null) {
                        res = res.Union(arr.PopValue(node, unit));
                    }
                }
            }
            return res;
        }

        private static IAnalysisSet ArraySliceFunction(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet res = AnalysisSet.Empty;
            if (args.Length >= 1 && @this != null) {
                foreach (var thisValue in @this) {
                    ArrayValue arr = thisValue.Value as ArrayValue;
                    if (arr != null) {
                        res = res.Add(thisValue);
                    }
                }
            }
            return res;
        }

        private static IAnalysisSet ArrayConcat(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // supporting actual concat would be nice, but could lead to infinite
            // analysis.  So we just return the existing array for now.
            return @this;
        }

        [ThreadStatic]
        private static bool _inForEach;

        private static IAnalysisSet ArrayForEach(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // prevent Array.forEach from calling Array.forEach recursively
            if (!_inForEach) {
                try {
                    _inForEach = true;
                    if (args.Length >= 1 && @this != null) {
                        foreach (var value in @this) {
                            ArrayValue arr = value.Value as ArrayValue;
                            if (arr != null) {
                                arr.ForEach(node, unit, @this, args);
                            }
                        }
                    }
                } finally {
                    _inForEach = false;
                }
            }
            return unit.Analyzer._undefined.Proxy;
        }

        private BuiltinFunctionValue BooleanFunction(out AnalysisValue booleanPrototype) {
            var builtinEntry = _analyzer._builtinEntry;
            var prototype = new BuiltinObjectValue(builtinEntry) {
                    BuiltinFunction("constructor"),
                    ReturningFunction("toString", _analyzer._emptyStringValue),
                    BuiltinFunction("valueOf"),
            };
            booleanPrototype = prototype;
            return new BuiltinFunctionValue(builtinEntry, "Boolean", null, prototype);
        }

        private BuiltinFunctionValue DateFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(
                builtinEntry, 
                "Date",
                new[] { 
                    new SimpleOverloadResult(
                        "Date", 
                        "Creates a new date from milliseconds specified in UTC starting at January 1, 1970", 
                        "milliseconds"
                    ),
                    new SimpleOverloadResult(
                        "Date", 
                        "Creates a new date from the specified string", 
                        "dateString"
                    ),
                    new SimpleOverloadResult(
                        "Date", 
                        "Creates a new date", 
                        "year",
                        "month",
                        "day",
                        "hours",
                        "minutes",
                        "seconds",
                        "milliseconds"
                    )
                },
                null,
                    new BuiltinObjectValue(builtinEntry) {
                        BuiltinFunction("constructor"),
                        ReturningFunction(
                            "getDate",
                            _analyzer._zeroIntValue,
                            "Gets the day-of-the-month, using local time."
                        ),
                        ReturningFunction(
                            "getDay",
                            _analyzer._zeroIntValue,
                            "Gets the day of the week, using local time."
                        ),
                        ReturningFunction(
                            "getFullYear",
                            _analyzer._zeroIntValue,
                            "Gets the year, using local time."
                        ),
                        ReturningFunction(
                            "getHours",
                            _analyzer._zeroIntValue,
                            "Gets the hours in a date, using local time."
                        ),
                        ReturningFunction(
                            "getMilliseconds",
                            _analyzer._zeroIntValue,
                            "Gets the milliseconds of a Date, using local time."
                        ),
                        ReturningFunction(
                            "getMinutes",
                            _analyzer._zeroIntValue,
                            "Gets the minutes of a Date object, using local time."
                        ),
                        ReturningFunction(
                            "getMonth",
                            _analyzer._zeroIntValue,
                            "Gets the month, using local time."
                        ),
                        ReturningFunction(
                            "getSeconds",
                            _analyzer._zeroIntValue,
                            "Gets the seconds of a Date object, using local time."
                        ),
                        ReturningFunction(
                            "getTime",
                            _analyzer._zeroIntValue,
                            "Gets the time value in milliseconds."
                        ),
                        ReturningFunction(
                            "getTimezoneOffset",
                            _analyzer._zeroIntValue,
                            "Gets the difference in minutes between the time on the local computer and Universal Coordinated Time (UTC)."
                        ),

                        
                        ReturningFunction(
                            "getUTCDate",
                            _analyzer._zeroIntValue,
                            "Gets the day-of-the-month, using Universal Coordinated Time (UTC)."
                        ),
                        ReturningFunction(
                            "getUTCDay",
                            _analyzer._zeroIntValue,
                            "Gets the day of the week using Universal Coordinated Time (UTC)."
                        ),
                        BuiltinFunction("getFullYear"),
                        ReturningFunction(
                            "getUTCHours",
                            _analyzer._zeroIntValue,
                            "Gets the hours value in a Date object using Universal Coordinated Time (UTC)."
                        ),
                        ReturningFunction(
                            "getUTCMilliseconds",
                            _analyzer._zeroIntValue,
                            "Gets the milliseconds of a Date object using Universal Coordinated Time (UTC)."
                        ),
                        ReturningFunction(
                            "getUTCMinutes",
                            _analyzer._zeroIntValue,
                            "Gets the minutes of a Date object using Universal Coordinated Time (UTC)."
                        ),
                        ReturningFunction(
                            "getUTCMonth",
                            _analyzer._zeroIntValue,
                            "Gets the month of a Date object using Universal Coordinated Time (UTC)."
                        ),
                        ReturningFunction(
                            "getUTCSeconds",
                            _analyzer._zeroIntValue,
                            "Gets the seconds of a Date object using Universal Coordinated Time (UTC)."
                        ),
                        ReturningFunction(
                            "getYear",
                            _analyzer._zeroIntValue,
                            "Gets the year minus 2000, using local time."
                        ),
                        ReturningFunction(
                            "setDate",
                            _analyzer._zeroIntValue,
                            "Sets the numeric day-of-the-month value of the Date object using local time. ",
                            Parameter("date", "A numeric value equal to the day of the month.")
                        ),
                        BuiltinFunction(
                            "setFullYear",
                            "Sets the year of the Date object using local time.",
                            Parameter("year", "A numeric value for the year."),
                            Parameter("month", "A zero-based numeric value for the month (0 for January, 11 for December). Must be specified if numDate is specified.", isOptional:true),
                            Parameter("date", "A numeric value equal for the day of the month.", isOptional:true)
                        ),
                        BuiltinFunction(
                            "setHours",
                            "Sets the hour value in the Date object using local time.",
                            Parameter("hours", "A numeric value equal to the hours value."),
                            Parameter("min", "A numeric value equal to the minutes value.", isOptional: true),
                            Parameter("sec", "A numeric value equal to the seconds value.", isOptional: true),
                            Parameter("ms", "A numeric value equal to the milliseconds value.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setMilliseconds",
                            _analyzer._zeroIntValue,
                            "Sets the milliseconds value in the Date object using local time.",
                            Parameter("ms", "A numeric value equal to the millisecond value.")
                        ),
                        ReturningFunction(
                            "setMinutes",
                            _analyzer._zeroIntValue,
                            "Sets the minutes value in the Date object using local time.",
                            Parameter("min", "A numeric value equal to the minutes value."),
                            Parameter("sec", "A numeric value equal to the seconds value.", isOptional: true),
                            Parameter("ms", "A numeric value equal to the milliseconds value.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setMonth",
                            _analyzer._zeroIntValue,
                            "Sets the month value in the Date object using local time.",
                            Parameter("month", "A numeric value equal to the month. The value for January is 0, and other month values follow consecutively."),
                            Parameter("date", "A numeric value representing the day of the month. If this value is not supplied, the value from a call to the getDate method is used.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setSeconds",
                            _analyzer._zeroIntValue,
                            "Sets the seconds value in the Date object using local time.",
                            Parameter("sec", "A numeric value equal to the seconds value."),
                            Parameter("ms", "A numeric value equal to the milliseconds value.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setTime",
                            _analyzer._zeroIntValue,
                            "Sets the date and time value in the Date object.",
                            Parameter("time", "A numeric value representing the number of elapsed milliseconds since midnight, January 1, 1970 GMT.")
                        ),
                        ReturningFunction(
                            "setUTCDate",
                            _analyzer._zeroIntValue,
                            "Sets the numeric day of the month in the Date object using Universal Coordinated Time (UTC).",
                            Parameter("date", "A numeric value equal to the day of the month.")
                        ),
                        ReturningFunction(
                            "setUTCFullYear",
                            _analyzer._zeroIntValue,
                            "Sets the year value in the Date object using Universal Coordinated Time (UTC).",
                            Parameter("year", "A numeric value equal to the year."),
                            Parameter("month", "A numeric value equal to the month. The value for January is 0, and other month values follow consecutively. Must be supplied if numDate is supplied.", isOptional: true),
                            Parameter("date", "A numeric value equal to the day of the month.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setUTCHours",
                            _analyzer._zeroIntValue,
                            "Sets the hours value in the Date object using Universal Coordinated Time (UTC).",
                            Parameter("hours", "A numeric value equal to the hours value."),
                            Parameter("min", "A numeric value equal to the minutes value.", isOptional: true),
                            Parameter("sec", "A numeric value equal to the seconds value.", isOptional: true),
                            Parameter("ms", "A numeric value equal to the milliseconds value.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setUTCMilliseconds",
                            _analyzer._zeroIntValue,
                            "Sets the milliseconds value in the Date object using Universal Coordinated Time (UTC).",
                            Parameter("ms", "A numeric value equal to the millisecond value.")
                        ),
                        ReturningFunction(
                            "setUTCMinutes",
                            _analyzer._zeroIntValue,
                            "Sets the minutes value in the Date object using Universal Coordinated Time (UTC).",
                            Parameter("min", "A numeric value equal to the minutes value."),
                            Parameter("sec", "A numeric value equal to the seconds value. ", isOptional: true),
                            Parameter("ms", "A numeric value equal to the milliseconds value.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setUTCMonth",
                            _analyzer._zeroIntValue,
                            "Sets the month value in the Date object using Universal Coordinated Time (UTC).",
                            Parameter("month", "A numeric value equal to the month. The value for January is 0, and other month values follow consecutively."),
                            Parameter("date", "A numeric value representing the day of the month. If it is not supplied, the value from a call to the getUTCDate method is used.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setUTCSeconds",
                            _analyzer._zeroIntValue,
                            "Sets the seconds value in the Date object using Universal Coordinated Time (UTC).",
                            Parameter("sec", "A numeric value equal to the seconds value."),
                            Parameter("ms", "A numeric value equal to the milliseconds value.", isOptional: true)
                        ),
                        ReturningFunction(
                            "setYear",
                            _analyzer._zeroIntValue
                        ),
                        ReturningFunction(
                            "toDateString",
                            _analyzer._emptyStringValue,
                            "Returns a date as a string value."
                        ),
                        BuiltinFunction("toGMTString"),
                        ReturningFunction(
                            "toISOString",
                            _analyzer._emptyStringValue,
                            "Returns a date as a string value in ISO format."
                        ),
                        ReturningFunction(
                            "toJSON",
                            _analyzer._emptyStringValue,
                            "Used by the JSON.stringify method to enable the transformation of an object's data for JavaScript Object Notation (JSON) serialization."
                        ),
                        ReturningFunction(
                            "toLocaleDateString",
                            _analyzer._emptyStringValue,
                            "Returns a date as a string value appropriate to the host environment's current locale."
                        ),
                        ReturningFunction(
                            "toLocaleString",
                            _analyzer._emptyStringValue,
                            "Returns a value as a string value appropriate to the host environment's current locale."
                        ),
                        ReturningFunction(
                            "toLocaleTimeString",
                            _analyzer._emptyStringValue,
                            "Returns a time as a string value appropriate to the host environment's current locale."
                        ),
                        ReturningFunction(
                            "toString",
                            _analyzer._emptyStringValue,
                            "Returns a string representation of a date. The format of the string depends on the locale."
                        ),
                        ReturningFunction(
                            "toTimeString",
                            _analyzer._emptyStringValue,
                            "Returns a time as a string value."
                        ),

                        BuiltinFunction("toUTCString"),
                        BuiltinFunction("valueOf"),
                }) { 
                ReturningFunction(
                    "UTC",
                    _analyzer._zeroIntValue,
                    "Returns the number of milliseconds between midnight, January 1, 1970 Universal Coordinated Time (UTC) (or GMT) and the specified date."
                ),
                ReturningFunction(
                    "parse",
                    _analyzer._zeroIntValue,
                    "Parses a string containing a date, and returns the number of milliseconds between that date and midnight, January 1, 1970.",
                    new ParameterResult("dateVal", "A string containing a date")
                ),
                ReturningFunction(
                    "now",
                    _analyzer._zeroIntValue,
                    "Returns the number of milliseconds between midnight, January 1, 1970, and the current date and time."
                )
            };
        }

        private BuiltinFunctionValue ErrorFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(
                builtinEntry, 
                "Error", 
                null, 
                    new BuiltinObjectValue(builtinEntry) {
                        BuiltinFunction("constructor"),
                        BuiltinProperty("message", _analyzer._emptyStringValue),
                        BuiltinProperty("name", _analyzer._emptyStringValue),
                        ReturningFunction("toString", _analyzer._emptyStringValue),
                }) { 
                new BuiltinFunctionValue(builtinEntry, "captureStackTrace"),
                Member("stackTraceLimit", _analyzer.GetConstant(10.0))
            };
        }

        private BuiltinFunctionValue ErrorFunction(string errorName) {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(
                builtinEntry, 
                errorName, 
                null, 
                    new BuiltinObjectValue(builtinEntry) {
                        BuiltinFunction("arguments"),
                        BuiltinFunction("constructor"),
                        BuiltinProperty("name", _analyzer._emptyStringValue),
                        BuiltinFunction("stack"),
                        BuiltinFunction("type"),
                    }
            );
        }

        private BuiltinFunctionValue FunctionFunction(out AnalysisValue functionPrototype) {
            var builtinEntry = _analyzer._builtinEntry;
            var prototype = new ReturningConstructingFunctionValue(builtinEntry, "Empty", _analyzer._undefined.Proxy, null) {
                    SpecializedFunction(
                        "apply",
                        ApplyFunction,
                        "Calls the function, substituting the specified object for the this value of the function, and the specified array for the arguments of the function.",
                        Parameter("thisArg", "The object to be used as the this object."),
                        Parameter("argArray", "A set of arguments to be passed to the function.")
                    ),
                    BuiltinFunction(
                        "bind",
                        @"For a given function, creates a bound function that has the same body as the original function. 
The this object of the bound function is associated with the specified object, and has the specified initial parameters.",
                        Parameter("thisArg", "An object to which the this keyword can refer inside the new function."),
                        Parameter("argArray", "A list of arguments to be passed to the new function.")

                    ),
                    BuiltinFunction(
                        "call",
                        "Calls a method of an object, substituting another object for the current object.",
                        Parameter("thisArg", "The object to be used as the current object."),
                        Parameter("argArray", "A list of arguments to be passed to the method.")
                    ),
                    BuiltinFunction("constructor"),
                    ReturningFunction("toString", _analyzer._emptyStringValue),
            };
            functionPrototype = prototype;
            return new BuiltinFunctionValue(builtinEntry, "Function", null, prototype);
        }

        private static IAnalysisSet ApplyFunction(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            var res = AnalysisSet.Empty;
            if (@this != null && args.Length > 0) {
                foreach (var value in @this) {
                    if (args.Length > 1) {
                        foreach (var arg in args[1]) {
                            res = res.Union(value.Call(node, unit, args[0], arg.Value.GetIndices(node, unit)));
                        }
                    } else {
                        res = res.Union(value.Call(node, unit, args[0], ExpressionEvaluator.EmptySets));
                    }
                }
            }
            return res;
        }

        private ObjectValue MakeJSONObject() {
            var builtinEntry = _analyzer._builtinEntry;

            // TODO: Should we see if we have something that we should parse?
            // TODO: Should we have a per-node value for the result of parse?
            var parseResult = new BuiltinObjectValue(builtinEntry);
            return new BuiltinObjectValue(builtinEntry) { 
                ReturningFunction(
                    "parse", 
                    parseResult,
                    "Converts a JavaScript Object Notation (JSON) string into an object.",
                    Parameter("text", "A valid JSON string."),
                    Parameter("reviver", @"A function that transforms the results. This function is called for each member of the object. 
If a member contains nested objects, the nested objects are transformed before the parent object is.", isOptional:true)
                ),
                ReturningFunction(
                    "stringify", 
                    _analyzer._emptyStringValue,
                    "Converts a JavaScript value to a JavaScript Object Notation (JSON) string.",
                    Parameter("value", "A JavaScript value, usually an object or array, to be converted.")
                ),
            };
        }

        private ObjectValue MakeMathObject() {
            var builtinEntry = _analyzer._builtinEntry;

            var doubleResult = _analyzer.GetConstant(0.0);
            return new BuiltinObjectValue(builtinEntry) { 
                Member("E", _analyzer.GetConstant(Math.E)),
                Member("LN10", doubleResult),
                Member("LN2", doubleResult),
                Member("LOG2E", doubleResult),
                Member("LOG10", doubleResult),
                Member("PI", _analyzer.GetConstant(Math.PI)),
                Member("SQRT1_2", _analyzer.GetConstant(Math.Sqrt(1.0/2.0))),
                Member("SQRT2", _analyzer.GetConstant(Math.Sqrt(2))),
                ReturningFunction(
                    "random", 
                    doubleResult, 
                    "Returns a pseudorandom number between 0 and 1."
                ),
                ReturningFunction(
                    "abs", 
                    doubleResult,
                    @"Returns the absolute value of a number (the value without regard to whether it is positive or negative). 
For example, the absolute value of -5 is the same as the absolute value of 5.",
                    Parameter("x", "A numeric expression for which the absolute value is needed.")
                ),
                ReturningFunction(
                    "acos", 
                    doubleResult,
                    "Returns the arc cosine (or inverse cosine) of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "asin", 
                    doubleResult,
                    "Returns the arcsine of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "atan", 
                    doubleResult,
                    "Returns the arctangent of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "ceil", 
                    doubleResult,
                    "Returns the smallest number greater than or equal to its numeric argument.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "cos", 
                    doubleResult,
                    "Returns the cosine of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "exp", 
                    doubleResult,
                    "Returns e (the base of natural logarithms) raised to a power.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "floor", 
                    doubleResult,
                    "Returns the greatest number less than or equal to its numeric argument.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "log", 
                    doubleResult,
                    "Returns the natural logarithm (base e) of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "round", 
                    doubleResult,
                    "Returns a supplied numeric expression rounded to the nearest number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "sin", 
                    doubleResult,
                    "Returns the sine of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "sqrt", 
                    doubleResult,
                    "Returns the square root of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "tan", 
                    doubleResult,
                    "Returns the tangent of a number.",
                    Parameter("x", "A numeric expression.")
                ),
                ReturningFunction(
                    "atan2", 
                    doubleResult,
                    "Returns the smallest number greater than or equal to its numeric argument.",
                    Parameter("y", "A numeric expression representing the cartesian y-coordinate."),
                    Parameter("x", "A numeric expression representing the cartesian x-coordinate.")
                ),
                ReturningFunction(
                    "pow", 
                    doubleResult,
                    "Returns the value of a base expression taken to a specified power.",
                    Parameter("x", "The base value of the expression."),
                    Parameter("y", "The exponent value of the expression.")
                ),
                ReturningFunction(
                    "max", 
                    doubleResult,
                    "Returns the larger of a set of supplied numeric expressions.",
                    Parameter("x", "Numeric expressions to be evaluated."),
                    Parameter("y...", "Numeric expressions to be evaluated.")
                ),
                ReturningFunction(
                    "min", 
                    doubleResult,
                    "Returns the smaller of a set of supplied numeric expressions. ",
                    Parameter("x", "Numeric expressions to be evaluated."),
                    Parameter("y...", "Numeric expressions to be evaluated.")
                ),
            };
        }

        private BuiltinFunctionValue NumberFunction(out AnalysisValue numberPrototype) {
            var builtinEntry = _analyzer._builtinEntry;

            var prototype = new BuiltinObjectValue(builtinEntry) {
                    BuiltinFunction("constructor"),
                    ReturningFunction(
                        "toExponential",
                        _analyzer._emptyStringValue,
                        "Returns a string containing a number represented in exponential notation."
                    ),
                    ReturningFunction(
                        "toFixed",
                        _analyzer._emptyStringValue,
                        "Returns a string representing a number in fixed-point notation."
                    ),
                    BuiltinFunction("toLocaleString"),
                    BuiltinFunction(
                        "toPrecision",
                        "Returns a string containing a number represented either in exponential or fixed-point notation with a specified number of digits.",
                        Parameter("precision", "Number of significant digits. Must be in the range 1 - 21, inclusive.", isOptional: true)
                    ),
                    ReturningFunction(
                        "toString",
                        _analyzer._emptyStringValue,
                        "Returns a string representation of an object.",
                        Parameter("radix", "Specifies a radix for converting numeric values to strings. This value is only used for numbers.", isOptional: true)
                    ),
                    BuiltinFunction("valueOf"),
            };
            numberPrototype = prototype;

            return new BuiltinFunctionValue(builtinEntry, "Number", null, prototype) { 
                Member("length", _analyzer.GetConstant(1.0)),
                Member("name", _analyzer.GetConstant("Number")),
                Member("arguments", _analyzer._nullInst),
                Member("caller", _analyzer._nullInst),
                Member("MAX_VALUE", _analyzer.GetConstant(Double.MaxValue)),
                Member("MIN_VALUE", _analyzer.GetConstant(Double.MinValue)),
                Member("NaN", _analyzer.GetConstant(Double.NaN)),
                Member("NEGATIVE_INFINITY", _analyzer.GetConstant(Double.NegativeInfinity)),
                Member("POSITIVE_INFINITY", _analyzer.GetConstant(Double.PositiveInfinity)),
                ReturningFunction(
                    "isFinite", 
                    _analyzer._trueInst,
                    "Determines whether a supplied number is finite."
                ),
                ReturningFunction(
                    "isNaN", 
                    _analyzer._falseInst,
                    "Returns a Boolean value that indicates whether a value is the reserved value NaN (not a number)."
                ),
            };
        }

        private BuiltinFunctionValue ObjectFunction(out ObjectValue objectPrototype, out BuiltinFunctionValue getOwnPropertyDescriptor) {
            var builtinEntry = _analyzer._builtinEntry;

            objectPrototype = new BuiltinObjectPrototypeValue(builtinEntry) {
                SpecializedFunction(
                    "__defineGetter__",
                    DefineGetter,
                    "Creates a getter method for the given property name",
                    Parameter("sprop", "The property name"),
                    Parameter("fun", "The function to be invoked")
                ),   
                BuiltinFunction(
                    "__lookupGetter__",
                    "Gets the getter function for the given property name",
                    Parameter("sprop", "The property name")
                ),   
                SpecializedFunction(
                    "__defineSetter__",
                    DefineSetter,
                    "Creates a setter method for the given property name",
                    Parameter("sprop", "The property name"),
                    Parameter("fun", "The function to be invoked")
                ),   
                BuiltinFunction(
                    "__lookupSetter__",
                    "Gets the setter function for the given property name",
                    Parameter("sprop", "The property name")
                ),   

                BuiltinFunction("constructor"),   
                ReturningFunction("toString", _analyzer._emptyStringValue),   
                ReturningFunction("toLocaleString", _analyzer._emptyStringValue),   
                BuiltinFunction("valueOf"),   
                ReturningFunction("hasOwnProperty", _analyzer._trueInst),   
                ReturningFunction(
                    "isPrototypeOf",
                    _analyzer._trueInst,
                    "Determines whether an object exists in another object's prototype chain.",
                    Parameter("v", "Another object whose prototype chain is to be checked.")
                ),   
                ReturningFunction(
                    "propertyIsEnumerable",
                    _analyzer._trueInst,
                    "Determines whether a specified property is enumerable.",
                    Parameter("v", "A property name.")
                ),   
            };

            getOwnPropertyDescriptor = SpecializedFunction(
                "getOwnPropertyDescriptor",
                GetOwnPropertyDescriptor,
                @"Gets the own property descriptor of the specified object. 
An own property descriptor is one that is defined directly on the object and is not inherited from the object's prototype. ",
                Parameter("o", "Object that contains the property."),
                Parameter("p", "Name of the property.")
            );

            return new SpecializedFunctionValue(
                builtinEntry, 
                "Object", 
                NewObject,
                null,
                objectPrototype) { 
                BuiltinFunction(
                    "getPrototypeOf",
                    "Returns the prototype of an object."
                ),
                getOwnPropertyDescriptor,
                BuiltinFunction(
                    "getOwnPropertyNames",
                    @"Returns the names of the own properties of an object. The own properties of an object are those that are defined directly 
on that object, and are not inherited from the object's prototype. The properties of an object include both fields (objects) and functions.",
                    Parameter("o", "Object that contains the own properties.")

                ),
                BuiltinFunction(
                    "create",
                    "Creates an object that has the specified prototype, and that optionally contains specified properties.",
                    Parameter("o", "Object to use as a prototype. May be null"),
                    Parameter("properties", "JavaScript object that contains one or more property descriptors.")
                ),
                SpecializedFunction(
                    "defineProperty",
                    DefineProperty,
                    "Adds a property to an object, or modifies attributes of an existing property.",
                    Parameter("o", "Object on which to add or modify the property. This can be a native JavaScript object (that is, a user-defined object or a built in object) or a DOM object."),
                    Parameter("p", "The property name."),
                    Parameter("attributes", "Descriptor for the property. It can be for a data property or an accessor property.")
                ),
                SpecializedFunction(
                    "defineProperties", 
                    DefineProperties,
                    "Adds one or more properties to an object, and/or modifies attributes of existing properties.",
                    Parameter("o", "Object on which to add or modify the properties. This can be a native JavaScript object or a DOM object."),
                    Parameter("properties", "JavaScript object that contains one or more descriptor objects. Each descriptor object describes a data property or an accessor property.")
                ),
                BuiltinFunction(
                    "seal",
                    "Prevents the modification of attributes of existing properties, and prevents the addition of new properties.",
                    Parameter("o", "Object on which to lock the attributes.")
                ),
                BuiltinFunction(
                    "freeze",
                    "Prevents the modification of existing property attributes and values, and prevents the addition of new properties.",
                    Parameter("o", "Object on which to lock the attributes.")
                ),
                BuiltinFunction(
                    "preventExtensions",
                    "Prevents the addition of new properties to an object.",
                    Parameter("o", "Object to make non-extensible.")
                ),
                ReturningFunction(
                    "isSealed",
                    _analyzer._trueInst,
                    "Returns true if existing property attributes cannot be modified in an object and new properties cannot be added to the object.",
                    Parameter("o", "Object to test. ")
                ),
                ReturningFunction(
                    "isFrozen",
                    _analyzer._trueInst,
                    "Returns true if existing property attributes and values cannot be modified in an object, and new properties cannot be added to the object.",
                    Parameter("o", "Object to test.")
                ),
                ReturningFunction(
                    "isExtensible",
                    _analyzer._trueInst,
                    "Returns a value that indicates whether new properties can be added to an object.",
                    Parameter("o", "Object to test.")
                ),
                SpecializedFunction(
                    "keys",
                    ObjectKeys,
                    "Returns the names of the enumerable properties and methods of an object.",
                    Parameter("o", "Object that contains the properties and methods. This can be an object that you created or an existing Document Object Model (DOM) object.")
                ),
                ReturningFunction("is", _analyzer._trueInst),
            };
        }

        private static IAnalysisSet ObjectKeys(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            ArrayValue arrValue;
            IAnalysisSet value;
            if (!unit._env.GlobalEnvironment.TryGetNodeValue(NodeEnvironmentKind.ObjectKeysArray, node, out value)) {
                arrValue = new ArrayValue(
                    new[] { new TypedDef() },
                    unit.ProjectEntry,
                    node
                );
                unit._env.GlobalEnvironment.AddNodeValue(NodeEnvironmentKind.ObjectKeysArray, node, arrValue.SelfSet);
            } else {
                arrValue = (ArrayValue)value.First().Value;
            }

            IAnalysisSet res = AnalysisSet.Empty;
            if (args.Length >= 1) {
                if (args[0].Count < unit.Analyzer.Limits.MaxObjectKeysTypes) {
                    arrValue.IndexTypes[0].AddTypes(
                        unit,
                        args[0].GetEnumerationValues(node, unit)
                    );
                }
            }
            return arrValue.SelfSet;
        }

        private static IAnalysisSet DefineSetter(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (@this != null && args.Length >= 2) {
                foreach (var thisVal in @this) {
                    ExpandoValue expando = thisVal.Value as ExpandoValue;
                    if (expando != null) {
                        foreach (var name in args[0]) {
                            var nameStr = name.Value.GetStringValue();
                            if (nameStr != null) {
                                expando.DefineSetter(unit, nameStr, args[1]);
                            }
                        }
                    }
                }
            }
            return unit.Analyzer._undefined.Proxy;
        }

        private static IAnalysisSet DefineGetter(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            if (@this != null && args.Length >= 2) {
                foreach (var thisVal in @this) {
                    ExpandoValue expando = thisVal.Value as ExpandoValue;
                    if (expando != null) {
                        foreach (var name in args[0]) {
                            var nameStr = name.Value.GetStringValue();
                            if (nameStr != null) {
                                expando.DefineGetter(unit, nameStr, args[1]);
                                // call the function w/ our this arg...
                                args[1].Call(node, unit, thisVal, Analyzer.ExpressionEvaluator.EmptySets);
                            }
                        }
                    }
                }
            }
            return unit.Analyzer._undefined.Proxy;
        }

        private static IAnalysisSet GetOwnPropertyDescriptor(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // Object.getOwnPropertyDescriptor(object, name)
            // returns an object like:
            //  {
            //      get:<function>,
            //      set:<function>,
            //      value:<object>,
            //      writable: boolean,
            //      enumerable: boolean,
            //      configurable: boolean
            // }
            if (args.Length >= 2) {
                IAnalysisSet res = AnalysisSet.Empty;
                foreach (var nameValue in args[1]) {
                    string nameStr = nameValue.Value.GetStringValue();
                    if (nameStr != null) {
                        foreach (var value in args[0]) {
                            var propDesc = value.Value.GetProperty(node, unit, nameStr) as PropertyDescriptorValue;
                            if (propDesc != null) {
                                res = res.Union(propDesc.SelfSet);
                            }
                        }
                    }
                }
                return res;
            }
            return AnalysisSet.Empty;
        }

        private static IAnalysisSet DefineProperty(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // object, name, property desc

            if (args.Length >= 3) {
                foreach (var obj in args[0]) {
                    ExpandoValue expando = obj.Value as ExpandoValue;
                    if (expando != null) {
                        foreach (var name in args[1]) {
                            string propName = name.Value.GetStringValue();
                            if (propName != null) {
                                foreach (var desc in args[2]) {
                                    expando.AddProperty(node, unit, propName, desc.Value);
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

        private static IAnalysisSet DefineProperties(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // object, {propName: {desc}, ...}
            if (args.Length >= 2 && args[0].Count < unit.Analyzer.Limits.MaxMergeTypes) {
                foreach (var obj in args[0]) {
                    ExpandoValue target = obj.Value as ExpandoValue;
                    if (target != null) {
                        foreach (var properties in args[1]) {
                            ExpandoValue propsObj = properties.Value as ExpandoValue;
                            if (propsObj != null) {
                                propsObj.AddDescriptorDependency(unit);

                                foreach (var propName in propsObj.Descriptors.Keys) {
                                    var definingProperty = propsObj.Get(node, unit, propName);
                                    foreach (var propValue in definingProperty) {
                                        target.AddProperty(
                                            node,
                                            unit,
                                            propName,
                                            propValue.Value
                                        );
                                    }
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

        private static IAnalysisSet Require(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet res = AnalysisSet.Empty;

            if (node.GetType() != typeof(CallNode)) {
                return res;
            }

            var call = (CallNode)node;

            // we care a lot about require analysis and people do some pretty
            // crazy dynamic things for require calls.  If we let our normal
            // analysis and specialized function handle it we won't get things
            // like handling './' + somePath.  So we'll go ahead and handle
            // some special cases here...
            if (IsSpecialRequire(unit, call, ref res)) {
                return res;
            }

            if (args.Length > 0) {
                foreach (var arg in args[0]) {
                    var moduleName = arg.Value.GetStringValue();
                    if (moduleName != null) {
                        res = res.Union(
                            unit.Analyzer.Modules.RequireModule(
                                node,
                                unit,
                                moduleName, 
                                unit.DeclaringModuleEnvironment.Name
                            )
                        );
                    }
                }
            }
            return res;
        }

        private static bool IsSpecialRequire(AnalysisUnit unit, CallNode n, ref IAnalysisSet res) {
            bool hitLiteral = false;
            if (n.Arguments.Length == 1) {
                var ee = new ExpressionEvaluator(unit);

                foreach (var name in ee.MergeStringLiterals(n.Arguments[0])) {
                    hitLiteral = true;
                    res = res.Union(
                        unit.Analyzer.Modules.RequireModule(
                            n,
                            unit,
                            name,
                            unit.DeclaringModuleEnvironment.Name
                        )
                    );
                }
            }

            return hitLiteral;
        }
       
        private BuiltinFunctionValue RegExpFunction() {
            var builtinEntry = _analyzer._builtinEntry;

            return new BuiltinFunctionValue(
                builtinEntry, 
                "RegExp", 
                null,
                    new BuiltinObjectValue(builtinEntry) {
                        BuiltinFunction("compile"),   
                        BuiltinFunction("constructor"),   
                        BuiltinFunction(
                            "exec",
                            "Executes a search on a string using a regular expression pattern, and returns an array containing the results of that search.",
                            Parameter("string", "The String object or string literal on which to perform the search.")
                        ),  
                        BuiltinProperty("global", _analyzer._trueInst),  
                        BuiltinProperty("ignoreCase", _analyzer._trueInst),  
                        BuiltinProperty("lastIndex", _analyzer._zeroIntValue),  
                        BuiltinProperty("multiline", _analyzer._trueInst),  
                        BuiltinProperty("source", _analyzer._emptyStringValue),  
                        ReturningFunction(
                            "test",
                            _analyzer._trueInst,
                            "Returns a Boolean value that indicates whether or not a pattern exists in a searched string.",
                            Parameter("string", "String on which to perform the search.")
                        ),  
                        ReturningFunction("toString", _analyzer._emptyStringValue) 
                }) { 
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
            var prototype = new BuiltinObjectValue(builtinEntry) {
                    ReturningFunction(
                        "anchor",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with an <a name=...> tag.",
                        Parameter("name", "the name attribute for the anchor")
                    ),
                    ReturningFunction(
                        "big",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <big> tag."
                    ),
                    ReturningFunction(
                        "blink",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <blink> tag."
                    ),
                    ReturningFunction(
                        "bold",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <bold> tag."
                    ),
                    ReturningFunction(
                        "charAt", 
                        _analyzer._emptyStringValue,
                        "Returns the character at the specified index.", 
                        Parameter("pos", "The zero-based index of the desired character.")
                    ),
                    ReturningFunction(
                        "charCodeAt", 
                        _analyzer._zeroIntValue,
                        "Returns the Unicode value of the character at the specified location.", 
                        Parameter("index", "The zero-based index of the desired character. If there is no character at the specified index, NaN is returned.")
                    ),
                    BuiltinFunction("concat", 
                        "Returns a string that contains the concatenation of two or more strings.", 
                        Parameter("...")
                    ),
                    BuiltinFunction("constructor"),
                    ReturningFunction(
                        "fixed",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <tt> tag."
                    ),
                    ReturningFunction(
                        "fontcolor",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <font color=...> tag.",
                        Parameter("color", "the color attribute for the font tag")
                    ),
                    ReturningFunction(
                        "fontsize",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <font size=...> tag.",
                        Parameter("size", "the size attribute for the font tag")
                    ),
                    BuiltinFunction("indexOf", 
                        "Returns the position of the first occurrence of a substring.", 
                        Parameter("searchString", "The substring to search for in the string"), 
                        Parameter("position", "The index at which to begin searching the String object. If omitted, search starts at the beginning of the string.", true)
                    ),
                    ReturningFunction(
                        "italics",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with an <i> tag."
                    ),
                    BuiltinFunction(
                        "lastIndexOf", 
                        "Returns the last occurrence of a substring in the string.", 
                        Parameter("searchString", "The substring to search for."), 
                        Parameter("position", "The index at which to begin searching. If omitted, the search begins at the end of the string.", true)
                    ),
                    BuiltinProperty("length", _analyzer._zeroIntValue),
                    ReturningFunction(
                        "link",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with an <a href=...> tag.",
                        Parameter("href", "the href attribute for the tag")
                    ),
                    BuiltinFunction(
                        "localeCompare", 
                        "Determines whether two strings are equivalent in the current locale.",
                        Parameter("that", "String to compare to target string")
                    ),
                    BuiltinFunction(
                        "match",
                        "Matches a string with a regular expression, and returns an array containing the results of that search.",
                        Parameter("regexp", "A string containing the regular expression pattern and flags or a RegExp.")

                    ),
                    BuiltinFunction(
                        "replace",
                        "Replaces text in a string, using a regular expression or search string.",
                        Parameter("searchValue", "A string that represents the regular expression or a RegExp"),
                        Parameter("replaceValue", "A string containing the text replacement text or a function which returns it.")
                    ),
                    BuiltinFunction(
                        "search",
                        "Finds the first substring match in a regular expression search.",
                        Parameter("regexp", "The regular expression pattern and applicable flags.")
                    ),
                    BuiltinFunction(
                        "slice",
                        "Returns a section of a string.",
                        Parameter("start", "The index to the beginning of the specified portion of stringObj."),
                        Parameter("end", "The index to the end of the specified portion of stringObj. The substring includes the characters up to, but not including, the character indicated by end.  If this value is not specified, the substring continues to the end of stringObj.")
                    ),
                    ReturningFunction(
                        "small",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <small> tag."
                    ),
                    BuiltinFunction(
                        "split",
                        "Split a string into substrings using the specified separator and return them as an array.",
                        Parameter("separator", "A string that identifies character or characters to use in separating the string. If omitted, a single-element array containing the entire string is returned. "),
                        Parameter("limit", "A value used to limit the number of elements returned in the array.", isOptional: true)
                    ),
                    ReturningFunction(
                        "strike",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <strike> tag."
                    ),
                    ReturningFunction(
                        "sub",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <sub> tag."
                    ),
                    BuiltinFunction("substr"),
                    BuiltinFunction(
                        "substring",
                        "Returns the substring at the specified location within a String object. ",
                        Parameter("start", "The zero-based index number indicating the beginning of the substring."),
                        Parameter("end", "Zero-based index number indicating the end of the substring. The substring includes the characters up to, but not including, the character indicated by end.  If end is omitted, the characters from start through the end of the original string are returned.")
                    ),
                    ReturningFunction(
                        "sup",
                        _analyzer._emptyStringValue,
                        "Surrounds the provided string with a <sup> tag."
                    ),
                    BuiltinFunction(
                        "toLocaleLowerCase",
                        "Converts all alphabetic characters to lowercase, taking into account the host environment's current locale."
                    ),
                    BuiltinFunction(
                        "toLocaleUpperCase",
                        "Returns a string where all alphabetic characters have been converted to uppercase, taking into account the host environment's current locale."
                    ),
                    SpecializedFunction(
                        "toLowerCase",
                        StringToLowerCase,
                        "Converts all the alphabetic characters in a string to lowercase."
                    ),
                    BuiltinFunction(
                        "toString",
                        "Returns a string representation of a string."
                    ),
                    SpecializedFunction(
                        "toUpperCase",
                        StringToUpperCase,
                        "Converts all the alphabetic characters in a string to uppercase."
                    ),
                    BuiltinFunction(
                        "trim",
                        "Removes the leading and trailing white space and line terminator characters from a string."
                    ),
                    BuiltinFunction(
                        "trimLeft",
                        "Removes the leading white space and line terminator characters from a string."
                    ),
                    BuiltinFunction(
                        "trimRight",
                        "Removes the trailing white space and line terminator characters from a string."
                    ),
                    BuiltinFunction("valueOf"),
            };
            stringPrototype = prototype;

            return new BuiltinFunctionValue(builtinEntry, "String", null, prototype) { 
                ReturningFunction("fromCharCode", _analyzer.GetConstant(String.Empty)),
            };
        }

        private static IAnalysisSet StringToUpperCase(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet res = AnalysisSet.Empty;
            if (@this != null) {
                foreach (var arg in @this) {
                    StringValue str = arg.Value as StringValue;
                    if (str != null) {
                        res = res.Union(unit.Analyzer.GetConstant(str._value.ToUpper(CultureInfo.CurrentCulture)).SelfSet);
                    }
                }
            }
            if (res.Count == 0) {
                return unit.Analyzer._emptyStringValue.SelfSet;
            }
            return res;
        }

        private static IAnalysisSet StringToLowerCase(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            IAnalysisSet res = AnalysisSet.Empty;
            if (@this != null) {
                foreach (var arg in @this) {
                    StringValue str = arg.Value as StringValue;
                    if (str != null) {
                        res = res.Union(unit.Analyzer.GetConstant(str._value.ToLower(CultureInfo.CurrentCulture)).SelfSet);
                    }
                }
            }
            if (res.Count == 0) {
                return unit.Analyzer._emptyStringValue.SelfSet;
            }
            return res;
        }

        #region Building Helpers

        private static MemberAddInfo Member(string name, AnalysisValue value) {
            return new MemberAddInfo(name, value);
        }

        private ParameterResult Parameter(string name, string doc = null, bool isOptional = false) {
            return new ParameterResult(name, doc, null, isOptional);
        }

        private BuiltinFunctionValue BuiltinFunction(string name, string documentation = null, params ParameterResult[] signature) {
            return new BuiltinFunctionValue(_analyzer._builtinEntry, name, documentation, null, signature);
        }

        private BuiltinFunctionValue ReturningFunction(string name, AnalysisValue value, string documentation = null, params ParameterResult[] parameters) {
            return new ReturningFunctionValue(_analyzer._builtinEntry, name, value.Proxy, documentation, parameters);
        }

        private BuiltinFunctionValue SpecializedFunction(string name, CallDelegate value, string documentation = null, params ParameterResult[] parameters) {
            return new SpecializedFunctionValue(_analyzer._builtinEntry, name, value, documentation, null, parameters);
        }

        private MemberAddInfo BuiltinProperty(string name, AnalysisValue propertyType, string documentation = null) {
            return new MemberAddInfo(name, propertyType, documentation, isProperty: true);
        }

        #endregion
    }

    class Globals {
        public readonly ObjectValue GlobalObject, ObjectPrototype;
        public readonly AnalysisValue NumberPrototype,
            StringPrototype,
            BooleanPrototype,
            FunctionPrototype;
        public readonly FunctionValue ArrayFunction;
        public readonly BuiltinFunctionValue RequireFunction;
        public readonly ExpandoValue ArrayPrototype;
        public readonly BuiltinFunctionValue ObjectGetOwnPropertyDescriptor;

        public Globals(ObjectValue globalObject, AnalysisValue numberPrototype, AnalysisValue stringPrototype, AnalysisValue booleanPrototype, AnalysisValue functionPrototype, FunctionValue arrayFunction, ObjectValue objectPrototype, BuiltinFunctionValue requireFunction, ExpandoValue arrayPrototype, BuiltinFunctionValue objectGetOwnPropertyDescriptor) {
            GlobalObject = globalObject;
            NumberPrototype = numberPrototype;
            StringPrototype = stringPrototype;
            BooleanPrototype = booleanPrototype;
            FunctionPrototype = functionPrototype;
            ArrayFunction = arrayFunction;
            ObjectPrototype = objectPrototype;
            RequireFunction = requireFunction;
            ArrayPrototype = arrayPrototype;
            ObjectGetOwnPropertyDescriptor = objectGetOwnPropertyDescriptor;
        }
    }
}

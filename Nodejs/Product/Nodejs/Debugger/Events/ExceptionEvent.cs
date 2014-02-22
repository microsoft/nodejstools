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

using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events {
    sealed class ExceptionEvent : IDebuggerEvent {
        /// <summary>
        /// V8 type transformations.
        /// </summary>
        private readonly Dictionary<string, string> _typeNameMappings = new Dictionary<string, string> {
            { "undefined", NodeVariableType.Undefined },
            { "null", NodeVariableType.Null },
            { "number", NodeVariableType.Number },
            { "boolean", NodeVariableType.Boolean },
            { "regexp", NodeVariableType.Regexp },
            { "function", NodeVariableType.Function },
            { "string", NodeVariableType.String },
            { "object", NodeVariableType.Object },
            { "error", NodeVariableType.Error },
        };

        public ExceptionEvent(JObject message) {
            Running = false;

            JToken body = message["body"];
            Line = (int?)body["sourceLine"];
            Column = (int?)body["sourceColumn"];
            Uncaught = (bool)body["uncaught"];
            ExceptionId = (int)body["exception"]["handle"];
            Description = (string)body["exception"]["text"];
            TypeName = GetExceptionType(body);
            ExceptionName = GetExceptionName(message);
            ErrorNumber = GetExceptionCodeRef(body);

            JToken script = body["script"];
            if (script != null) {
                var scriptId = (int)script["id"];
                var fileName = (string)script["name"];
                Module = new NodeModule(scriptId, fileName, fileName);
            }
        }

        public string ExceptionName { get; private set; }
        public int? ErrorNumber { get; private set; }
        public int? Line { get; private set; }
        public int? Column { get; private set; }
        public string Description { get; private set; }
        public int ExceptionId { get; private set; }
        public string TypeName { get; private set; }
        public bool Uncaught { get; private set; }
        public NodeModule Module { get; private set; }
        public bool Running { get; private set; }

        private int? GetExceptionCodeRef(JToken body) {
            JToken exception = body["exception"];
            var properties = (JArray)exception["properties"];
            if (properties != null) {
                foreach (JToken property in properties) {
                    if (((string)property["name"]) == "code") {
                        return (int)property["ref"];
                    }
                }
            }
            return null;
        }

        private string GetExceptionName(JObject json) {
            JToken body = json["body"];
            JToken exception = body["exception"];
            var name = (string)exception["type"];
            if (name == "error" || name == "object") {
                JToken constructorFunction = exception["constructorFunction"];
                var constructorFunctionHandle = (int)constructorFunction["ref"];
                var refs = (JArray)json["refs"];
                JToken refRecord = GetRefRecord(refs, constructorFunctionHandle);
                if (refRecord != null) {
                    name = (string)refRecord["name"];
                }
            }
            return _typeNameMappings.ContainsKey(name) ? _typeNameMappings[name] : name;
        }

        private JToken GetRefRecord(JArray refs, int handle) {
            foreach (JToken refRecordObj in refs) {
                JToken refRecord = refRecordObj;
                var refRecordHandle = (int)refRecord["handle"];
                if (refRecordHandle == handle) {
                    return refRecord;
                }
            }
            return null;
        }

        private string GetExceptionType(JToken body) {
            string name = (string)body["exception"]["className"]
                          ?? (string)body["exception"]["type"];
            return _typeNameMappings.ContainsKey(name) ? _typeNameMappings[name] : name;
        }
    }
}
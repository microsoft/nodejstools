// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events
{
    internal sealed class ExceptionEvent : IDebuggerEvent
    {
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

        public ExceptionEvent(JObject message)
        {
            this.Running = false;

            var body = message["body"];
            this.Line = (int?)body["sourceLine"];
            this.Column = (int?)body["sourceColumn"];
            this.Uncaught = (bool)body["uncaught"];
            this.ExceptionId = (int)body["exception"]["handle"];
            this.Description = (string)body["exception"]["text"];
            this.TypeName = GetExceptionType(body);
            this.ExceptionName = GetExceptionName(message);
            this.ErrorNumber = GetExceptionCodeRef(body);

            var script = body["script"];
            if (script != null)
            {
                var scriptId = (int)script["id"];
                var fileName = (string)script["name"];
                this.Module = new NodeModule(scriptId, fileName);
            }
        }

        public string ExceptionName { get; }
        public int? ErrorNumber { get; }
        public int? Line { get; }
        public int? Column { get; }
        public string Description { get; }
        public int ExceptionId { get; }
        public string TypeName { get; }
        public bool Uncaught { get; }
        public NodeModule Module { get; }
        public bool Running { get; }

        private int? GetExceptionCodeRef(JToken body)
        {
            var exception = body["exception"];
            var properties = (JArray)exception["properties"];
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    if (((string)property["name"]) == "code")
                    {
                        return (int)property["ref"];
                    }
                }
            }
            return null;
        }

        private string GetExceptionName(JObject json)
        {
            var body = json["body"];
            var exception = body["exception"];
            var name = (string)exception["type"];
            if (name == "error" || name == "object")
            {
                var constructorFunction = exception["constructorFunction"];
                var constructorFunctionHandle = (int)constructorFunction["ref"];
                var refs = (JArray)json["refs"];
                var refRecord = GetRefRecord(refs, constructorFunctionHandle);
                if (refRecord != null)
                {
                    name = (string)refRecord["name"];
                }
            }
            return this._typeNameMappings.ContainsKey(name) ? this._typeNameMappings[name] : name;
        }

        private JToken GetRefRecord(JArray refs, int handle)
        {
            foreach (var refRecordObj in refs)
            {
                var refRecord = refRecordObj;
                var refRecordHandle = (int)refRecord["handle"];
                if (refRecordHandle == handle)
                {
                    return refRecord;
                }
            }
            return null;
        }

        private string GetExceptionType(JToken body)
        {
            var name = (string)body["exception"]["className"]
                          ?? (string)body["exception"]["type"];
            return this._typeNameMappings.ContainsKey(name) ? this._typeNameMappings[name] : name;
        }
    }
}

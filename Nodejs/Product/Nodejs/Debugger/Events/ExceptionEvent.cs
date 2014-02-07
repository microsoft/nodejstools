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

using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events {
    sealed class ExceptionEvent : IDebuggerEvent {
        public ExceptionEvent(JObject message) {
            Running = false;

            Line = (int)message["body"]["sourceLine"];
            Column = (int)message["body"]["sourceColumn"];
            Uncaught = (bool)message["body"]["uncaught"];
            ExceptionId = (int)message["body"]["exception"]["handle"];
            Description = (string)message["body"]["exception"]["text"];
            string typeName = (string)message["body"]["exception"]["className"]
                              ?? (string)message["body"]["exception"]["type"];
            typeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(typeName);
            TypeName = string.Format("{0} exception", typeName);

            var scriptId = (int)message["body"]["script"]["id"];
            var filename = (string)message["body"]["script"]["name"];

            ExceptionName = GetExceptionName(message);
            ErrorNumber = GetExceptionCodeRef(message);

            Module = new NodeModule(null, scriptId, filename);
        }

        public string ExceptionName { get; private set; }
        public int? ErrorNumber { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Description { get; private set; }
        public int ExceptionId { get; private set; }
        public string TypeName { get; private set; }
        public bool Uncaught { get; private set; }
        public NodeModule Module { get; private set; }
        public bool Running { get; private set; }

        private int? GetExceptionCodeRef(JObject json) {
            JToken body = json["body"];
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
            return name;
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
    }
}
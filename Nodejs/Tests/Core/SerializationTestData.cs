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
using System.Web.Script.Serialization;
using Microsoft.NodejsTools.Debugger.Serialization;

namespace NodejsTests {
    static class SerializationTestData {
        /// <summary>
        /// Gets a json object produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static object GetDeserializedJsonObject() {
            string json = Resources.NodeVariable;
            var serializer = new JavaScriptSerializer();
            return serializer.DeserializeObject(json);
        }

        /// <summary>
        /// Gets a json array produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON array.</returns>
        public static object GetDeserializedPrimitiveJsonArray() {
            string json = Resources.NodeSimpleArray;
            var serializer = new JavaScriptSerializer();
            return serializer.DeserializeObject(json);
        }

        /// <summary>
        /// Gets a json array produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON array.</returns>
        public static object GetDeserializedComplexJsonArray() {
            string json = Resources.NodeComplexArray;
            var serializer = new JavaScriptSerializer();
            return serializer.DeserializeObject(json);
        }

        /// <summary>
        /// Gets a json backtrace response produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetBacktraceResponse() {
            string json = Resources.NodeBacktraceResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json backtrace object produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetBacktraceJsonObject() {
            string json = Resources.NodeBacktraceVariableObject;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json backtrace object without name produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetBacktraceJsonObjectWithNullName() {
            string json = Resources.NodeBacktraceVariableObjectWithoutName;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json evaluation response produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetEvaluateResponse() {
            string json = Resources.NodeEvaluationResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json evaluation object produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetEvaluationJsonObject() {
            string json = Resources.NodeEvaluationVariableObject;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json lookup response produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetLookupResponse() {
            string json = Resources.NodeLookupResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json lookup object produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetLookupJsonProperty() {
            string json = Resources.NodeLookupVariableObject;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json lookup references produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static Dictionary<int, JsonValue> GetLookupJsonReferences() {
            string json = Resources.NodeLookupReferencesArray;
            JsonArray refs = DeserializeJsonArray(json);
            var references = new Dictionary<int, JsonValue>(refs.Count);
            for (int i = 0; i < refs.Count; i++) {
                JsonValue reference = refs[i];
                var id = reference.GetValue<int>("handle");
                references.Add(id, reference);
            }

            return references;
        }

        /// <summary>
        /// Gets a json lookup object prototype produced by <see cref="JavaScriptSerializer" />.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JsonValue GetLookupJsonPrototype() {
            string json = Resources.NodeLookupPrototypeObject;
            return DeserializeJsonValue(json);
        }

        private static JsonValue DeserializeJsonValue(string json) {
            var serializer = new JavaScriptSerializer();
            var value = (Dictionary<string, object>)serializer.DeserializeObject(json);
            return new JsonValue(value);
        }

        private static JsonArray DeserializeJsonArray(string json) {
            var serializer = new JavaScriptSerializer();
            var array = (object[])serializer.DeserializeObject(json);
            return new JsonArray(array);
        }
    }
}
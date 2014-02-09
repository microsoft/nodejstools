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
using Newtonsoft.Json.Linq;

namespace NodejsTests {
    static class SerializationTestData {
        /// <summary>
        /// Gets a json object.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static object GetDeserializedJsonObject() {
            string json = Resources.NodeVariable;
            var serializer = new JavaScriptSerializer();
            return serializer.DeserializeObject(json);
        }

        /// <summary>
        /// Gets a json array.
        /// </summary>
        /// <returns>JSON array.</returns>
        public static object GetDeserializedPrimitiveJsonArray() {
            string json = Resources.NodeSimpleArray;
            var serializer = new JavaScriptSerializer();
            return serializer.DeserializeObject(json);
        }

        /// <summary>
        /// Gets a json array.
        /// </summary>
        /// <returns>JSON array.</returns>
        public static object GetDeserializedComplexJsonArray() {
            string json = Resources.NodeComplexArray;
            var serializer = new JavaScriptSerializer();
            return serializer.DeserializeObject(json);
        }

        /// <summary>
        /// Gets a json backtrace response.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetBacktraceResponse() {
            string json = Resources.NodeBacktraceResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json backtrace object.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetBacktraceJsonObject() {
            string json = Resources.NodeBacktraceVariableObject;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json backtrace object without name.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetBacktraceJsonObjectWithNullName() {
            string json = Resources.NodeBacktraceVariableObjectWithoutName;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json evaluation response.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetEvaluateResponse() {
            string json = Resources.NodeEvaluationResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json evaluation object.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetEvaluationJsonObject() {
            string json = Resources.NodeEvaluationVariableObject;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json lookup response.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetLookupResponse() {
            string json = Resources.NodeLookupResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json lookup object.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetLookupJsonProperty() {
            string json = Resources.NodeLookupVariableObject;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json scripts response.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetScriptsResponse() {
            string json = Resources.NodeScriptsResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json lookup references.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static Dictionary<int, JToken> GetLookupJsonReferences() {
            string json = Resources.NodeLookupReferencesArray;
            JArray refs = DeserializeJsonArray(json);
            var references = new Dictionary<int, JToken>(refs.Count);
            for (int i = 0; i < refs.Count; i++) {
                JToken reference = refs[i];
                var id = (int)reference["handle"];
                references.Add(id, reference);
            }

            return references;
        }

        /// <summary>
        /// Gets a json lookup object prototype.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetLookupJsonPrototype() {
            string json = Resources.NodeLookupPrototypeObject;
            return DeserializeJsonValue(json);
        }

        private static JObject DeserializeJsonValue(string json) {
            return JObject.Parse(json);
        }

        private static JArray DeserializeJsonArray(string json) {
            return JArray.Parse(json);
        }
    }
}
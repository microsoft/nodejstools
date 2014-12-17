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
        /// Gets a json evaluation response with reference error.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetEvaluateResponseWithReferenceError() {
            string json = Resources.NodeEvaluationResponseWithReferenceError;
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
        /// Gets a json lookup response with primitive object.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetLookupResponseWithPrimitiveObject() {
            string json = Resources.NodeLookupResponseWithPrimitiveObject;
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
        /// Gets a json set variable value response.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetSetVariableValueResponse() {
            string json = Resources.NodeSetVariableValueResponse;
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

        /// <summary>
        /// Gets a json change live response.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetChangeLiveResponse() {
            string json = Resources.NodeChangeLiveResponse;
            return DeserializeJsonValue(json);
        }

        /// <summary>
        /// Gets a json for set breakpoint response.
        /// </summary>
        /// <returns>JSON object.</returns>
        public static JObject GetSetBreakpointResponse() {
            string json = Resources.NodeSetBreakpointResponse;
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
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

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger.Serialization {
    /// <summary>
    /// Wrapper around json object data.
    /// </summary>
    class JsonValue {
        private readonly Dictionary<string, object> _json;

        /// <summary>
        /// Costructs a new instance based on json data.
        /// </summary>
        /// <param name="json">JSON data.</param>
        public JsonValue(Dictionary<string, object> json) {
            if (json == null) {
                throw new ArgumentNullException("json");
            }
            _json = json;
        }

        /// <summary>
        /// Gets an object property value by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Value.</returns>
        public JsonValue this[string name] {
            get {
                if (!_json.ContainsKey(name)) {
                    return null;
                }

                var value = _json[name] as Dictionary<string, object>;
                if (value == null) {
                    return null;
                }

                return new JsonValue(value);
            }
        }

        /// <summary>
        /// Gets a converted object value by name.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="name">Name.</param>
        /// <returns>Value</returns>
        public T GetValue<T>(string name) {
            if (!_json.ContainsKey(name)) {
                return default(T);
            }

            object value = _json[name];
            if (value == null) {
                return default(T);
            }

            return (T) Convert.ChangeType(value, typeof (T));
        }

        /// <summary>
        /// Gets an array by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Array.</returns>
        public JsonArray GetArray(string name) {
            if (!_json.ContainsKey(name)) {
                return null;
            }

            var values = (object[]) _json[name];
            if (values == null) {
                return null;
            }

            return new JsonArray(values);
        }
    }
}
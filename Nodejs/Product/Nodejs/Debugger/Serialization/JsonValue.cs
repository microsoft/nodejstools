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
using System.Linq;

namespace Microsoft.NodejsTools.Debugger.Serialization {
    class JsonValue {
        private readonly Dictionary<string, object> _json;

        public JsonValue(Dictionary<string, object> json) {
            _json = json;
        }

        public JsonValue this[string name] {
            get {
                if (!_json.ContainsKey(name)) {
                    return null;
                }

                object value = _json[name];
                if (value == null) {
                    return null;
                }

                return new JsonValue((Dictionary<string, object>) value);
            }
        }

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

        public IList<JsonValue> GetArray(string name) {
            if (!_json.ContainsKey(name)) {
                return new List<JsonValue>();
            }

            object values = _json[name];
            if (values == null) {
                return new List<JsonValue>();
            }

            var objects = (object[]) values;
            return objects.Select(p => new JsonValue((Dictionary<string, object>) p)).ToList();
        }
    }
}
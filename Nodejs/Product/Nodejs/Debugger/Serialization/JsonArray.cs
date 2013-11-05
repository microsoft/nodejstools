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
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger.Serialization {
    /// <summary>
    /// Wrapper around json array data.
    /// </summary>
    class JsonArray {
        private readonly object[] _data;

        /// <summary>
        /// Costructs a new instance based on json data.
        /// </summary>
        /// <param name="data">JSON data.</param>
        public JsonArray(object[] data) {
            Utilities.ArgumentNotNull("data", data);

            _data = data;
        }

        /// <summary>
        /// Gets an element by index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>Element.</returns>
        public JsonValue this[int index] {
            get {
                if (index < 0 || index >= _data.Length) {
                    string message = string.Format("Index should be in range [{0}..{1}]", 0, _data.Length - 1);
                    throw new IndexOutOfRangeException(message);
                }

                var value = _data[index] as Dictionary<string, object>;
                if (value == null) {
                    return null;
                }

                return new JsonValue(value);
            }
        }

        /// <summary>
        /// Gets a count of elements in the array.
        /// </summary>
        public int Count {
            get { return _data.Length; }
        }

        /// <summary>
        /// Gets a converted array element value by index.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="index">Index.</param>
        /// <returns>Value.</returns>
        public T GetValue<T>(int index) {
            if (index < 0 || index >= _data.Length) {
                string message = string.Format("Index should be in range [{0}..{1}]", 0, _data.Length - 1);
                throw new IndexOutOfRangeException(message);
            }

            object value = _data[index];
            if (value == null) {
                return default(T);
            }

            return (T)Convert.ChangeType(value, typeof (T));
        }

        /// <summary>
        /// Gets an embedded array by index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>Array.</returns>
        public JsonArray GetArray(int index) {
            if (index < 0 || index >= _data.Length) {
                string message = string.Format("Index should be in range [{0}..{1}]", 0, _data.Length - 1);
                throw new IndexOutOfRangeException(message);
            }

            var values = _data[index] as object[];
            if (values == null) {
                return null;
            }

            return new JsonArray(values);
        }
    }
}
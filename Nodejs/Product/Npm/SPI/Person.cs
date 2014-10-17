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
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Person : IPerson {

        [JsonConstructor]
        private Person() {
            // Enables Json deserialization
        }

        public Person(string source) {
            InitFromString(source);
        }

        private void InitFromString(string source) {
            try {
                var jObject = JObject.Parse(source);
                Name = (string)jObject["name"];
                Email = (string)jObject["email"];
                Url = (string)jObject["url"];
            } catch (Exception) {
                Name = source;
            }
        }

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string Email { get; private set; }

        [JsonProperty]
        public string Url { get; private set; }

        public override string ToString() {
            var buff = new StringBuilder();
            if (!string.IsNullOrEmpty(Name)) {
                buff.Append(Name);
            }

            if (!string.IsNullOrEmpty(Email)) {
                if (buff.Length > 0) {
                    buff.Append(' ');
                }
                buff.Append('<');
                buff.Append(Email);
                buff.Append('>');
            }

            if (!string.IsNullOrEmpty(Url)) {
                if (buff.Length > 0) {
                    buff.Append(' ');
                }
                buff.Append('(');
                buff.Append(Url);
                buff.Append(')');
            }
            return buff.ToString();
        }
    }
}
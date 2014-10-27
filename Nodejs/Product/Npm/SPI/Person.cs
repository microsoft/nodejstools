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
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

#if DEBUG
using Newtonsoft.Json.Linq;
#endif

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Person : IPerson {

    private static readonly Regex RegexPerson = new Regex(
        "\"name\":\\s*\"(?<name>[^<]+?)\"" +
        "[\\s,]*" +
        "(\"email\":\\s*\"(?<email>[^<]+?)\")?" +
        "[\\s,]*" +
        "(\"url\":\\s*\"(?<url>[^<]+?)\")?",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);
    
        [JsonConstructor]
        private Person() {
            // Enables Json deserialization
        }

        public Person(string source) {
            InitFromString(source);
        }

        private void InitFromString(string source) {
            if (source == null) {
                Name = string.Empty;
                return;
            }

            // We parse using a regex because JObject.Parse throws exceptions for malformatted json,
            // and simply handling them causes performance issues.
            var matches = RegexPerson.Matches(source);
            if (matches.Count == 1) {
                var match = matches[0];
                var group = match.Groups["name"];
                Name = group.Value;

                group = match.Groups["email"];
                Email = group.Value;

                group = match.Groups["url"];
                Url = group.Value;
            } else {
                Name = source;
            }

#if DEBUG
            // Verify we are parsing correctly
            try {
                var jObject = JObject.Parse(source);
                Debug.Assert(((string)jObject["name"] ?? string.Empty) == Name, string.Format("Failed to parse name from {0}", source));
                Debug.Assert(((string)jObject["email"] ?? string.Empty) == Email, string.Format("Failed to parse email from {0}", source));
                Debug.Assert(((string)jObject["url"] ?? string.Empty) == Url, string.Format("Failed to parse url from {0}", source));
            } catch (Exception) {
                Debug.Assert(source == Name);
            }
#endif
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
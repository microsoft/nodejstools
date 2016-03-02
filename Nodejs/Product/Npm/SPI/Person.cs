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

        // We cannot rely on the ordering of any of these fields,
        // so we should match them separately.
        private static readonly Regex ObjectPersonRegex = new Regex(
            "\"name\":\\s*\"(?<name>[^<]+?)\"" +
            "|" +
            "\"email\":\\s*\"(?<email>[^<]+?)\"" +
            "|" +
            "\"url\":\\s*\"(?<url>[^<]+?)\"",
            RegexOptions.Singleline);

        private static readonly Regex StringPersonRegex = new Regex(
            @"^""(?<name>[^""]+)""$",
            RegexOptions.Singleline);

        [JsonConstructor]
        private Person() {
            // Enables Json deserialization
        }

        private Person(string name, string email = null, string url = null) {
            Name = name;
            Email = email;
            Url = url;
        }

        public static Person CreateFromJsonSource(string source) {
            if (source == null)
                return new Person(string.Empty);

            var objectPerson = TryCreatePersonFromObject(source);
            if (objectPerson != null)
                return objectPerson;

            var stringPerson = TryCreatePersonFromString(source);
            if (stringPerson != null)
                return stringPerson;

            return new Person(source);
        }

        /// <summary>
        /// Try to create a person object from a json object.
        /// 
        /// This can either be a json object or a string: https://docs.npmjs.com/files/package.json#people-fields-author-contributors
        /// </summary>
        /// <param name="source">Json source</param>
        private static Person TryCreatePersonFromObject(string source) {
            string name = null;
            string email = null;
            string url = null;

            // We parse using a regex because JObject.Parse throws exceptions for malformatted json,
            // and simply handling them causes performance issues.
            var matches = ObjectPersonRegex.Matches(source);
            if (matches.Count >= 1) {
                foreach (Match match in matches) {
                    var group = match.Groups["name"];
                    if (group.Success) {
                        name = group.Value;
                        continue;
                    }

                    group = match.Groups["email"];
                    if (group.Success) {
                        email = group.Value;
                        continue;
                    }

                    group = match.Groups["url"];
                    if (group.Success) {
                        url = group.Value;
                        continue;
                    }
                }
            } else {
                return null;
            }
            return new Person(name, email, url);
        }

        /// <summary>
        /// Try to create a person object from a json string.
        /// 
        /// TODO: currently does not try to parse the string to extract the email or url.
        /// </summary>
        /// <param name="source">Json source</param>
        private static Person TryCreatePersonFromString(string source) {
            var matches = StringPersonRegex.Matches(source);
            if (matches.Count == 1) {
                var match = matches[0];
                var group = match.Groups["name"];
                if (group.Success)
                    return new Person(group.Value);
            }
            return null;
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
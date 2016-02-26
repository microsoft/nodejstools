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
        private static readonly Regex RegexPerson = new Regex(
        "\"name\":\\s*\"(?<name>[^<]+?)\"" +
        "|" +
        "\"email\":\\s*\"(?<email>[^<]+?)\"" +
        "|" +
        "\"url\":\\s*\"(?<url>[^<]+?)\"",
        RegexOptions.Singleline);
    
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
            if (matches.Count >= 1) {
                foreach (Match match in matches) {
                    var group = match.Groups["name"];
                    if (group.Success) {
                        Name = group.Value;
                        continue;
                    }

                    group = match.Groups["email"];
                    if (group.Success) {
                        Email = group.Value;
                        continue;
                    }

                    group = match.Groups["url"];
                    if (group.Success) {
                        Url = group.Value;
                        continue;
                    }
                }
            } else {
                Name = source;
            }

#if DEBUG
            // Verify we are parsing correctly
            try {
                var jObject = JObject.Parse(source);
                var name = (string)jObject["name"];
                Debug.Assert(name != null ? name == Name : Name == source, string.Format("Failed to parse name from {0}", source));
                Debug.Assert((string)jObject["email"] == Email, string.Format("Failed to parse email from {0}", source));
                Debug.Assert((string)jObject["url"] == Url, string.Format("Failed to parse url from {0}", source));
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
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

using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Bugs : IBugs {
        private readonly dynamic _json;

        public Bugs(dynamic package) {
            _json = package.bugs;
        }

        public string Url {
            get {
                string url = null;
                if (null != _json) {
                    var token = _json as JToken;
                    if (token.Type == JTokenType.Object) {
                        var temp = _json.url ?? _json.web;
                        if (null != temp) {
                            url = temp.ToString();
                        }
                    } else {
                        url = token.Value<string>();
                    }
                }
                return url;
            }
        }

        public string Email {
            get {
                string email = null;
                if (null != _json) {
                    var token = _json as JToken;
                    if (token.Type == JTokenType.Object) {
                        var temp = _json.email ?? _json.mail;
                        if (null != temp) {
                            email = temp.ToString();
                        }
                    }
                }
                return email;
            }
        }
    }
}
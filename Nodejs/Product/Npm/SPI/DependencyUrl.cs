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

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class DependencyUrl : IDependencyUrl {
        public DependencyUrl(string address) {
            Address = address;
        }

        public string Address { get; private set; }

        public DependencyUrlType Type {
            get {
                var index = Address.IndexOf("://");
                if (index < 0) {
                    return DependencyUrlType.GitHub;
                } else {
                    var prefix = Address.Substring(0, index);
                    switch (prefix) {
                        case "http":
                            return DependencyUrlType.Http;

                        case "git":
                            return DependencyUrlType.Git;

                        case "git+ssh":
                            return DependencyUrlType.GitSsh;

                        case "git+http":
                            return DependencyUrlType.GitHttp;

                        case "git+https":
                            return DependencyUrlType.GitHttps;

                        default:
                            return DependencyUrlType.UnsupportedProtocol;
                    }
                }
            }
        }
    }
}
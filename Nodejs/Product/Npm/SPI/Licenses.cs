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
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Licenses : ILicenses {
        private dynamic _json;

        public Licenses(dynamic package) {
            _json = package.license;
        }

        public int Count {
            get {
                if (_json != null) {
                    return 1;
                }

                if (null == _json) {
                    return 0;
                }

                return (_json as JArray).Count;
            }
        }

        public ILicense this[int index] {
            get {
                if (index < 0) {
                    throw new IndexOutOfRangeException(Resources.CannotRetrieveLicenseInvalidIndex);
                }

                if (index == 0 && _json != null) {
                    return new License(_json.ToString());
                }

                if (null == _json) {
                    throw new IndexOutOfRangeException(Resources.CannotRetrieveLicenseEmptyCollection);
                }

                var lic = _json[index];
                return new License(lic.type.ToString(), lic.url.ToString());
            }
        }
    }
}
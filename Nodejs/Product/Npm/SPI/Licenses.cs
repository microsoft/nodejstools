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
        private dynamic _package;

        public Licenses(dynamic package) {
            _package = package;
        }

        public int Count {
            get {
                if (_package.license != null) {
                    return 1;
                }

                var json = _package.licenses;
                if (null == json) {
                    return 0;
                }

                JArray array = json;
                return array.Count;
            }
        }

        public ILicense this[int index] {
            get {
                if (index < 0) {
                    throw new IndexOutOfRangeException(Resources.CannotRetrieveLicenseInvalidIndex);
                }

                if (index == 0 && _package.license != null) {
                    return new License(_package.license.ToString());
                }

                var json = _package.licenses;
                if (null == json) {
                    throw new IndexOutOfRangeException(Resources.CannotRetrieveLicenseEmptyCollection);
                }

                var lic = json[index];
                return new License(lic.type.ToString(), lic.url.ToString());
            }
        }
    }
}
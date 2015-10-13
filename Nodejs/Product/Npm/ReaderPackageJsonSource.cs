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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm {
    public class ReaderPackageJsonSource : IPackageJsonSource {
        public ReaderPackageJsonSource(TextReader reader) {
            try {
                var text = reader.ReadToEnd();
                try {
                    // JsonConvert and JObject.Parse exhibit slightly different behavior,
                    // so fall back to JObject.Parse if JsonConvert does not properly deserialize
                    // the object.
                    Package = JsonConvert.DeserializeObject(text);
                } catch (ArgumentException) {
                    Package = JObject.Parse(text);
                }
            } catch (JsonReaderException jre) {
                WrapExceptionAndRethrow(jre);
            } catch (JsonSerializationException jse) {
                WrapExceptionAndRethrow(jse);
            } catch (FormatException fe) {
                WrapExceptionAndRethrow(fe);
            } catch (ArgumentException ae) {
                throw new PackageJsonException(
                    string.Format(@"Error reading package.json. The file may be parseable JSON but may contain objects with duplicate properties.

The following error occurred:

{0}", ae.Message),
                    ae);
            }
        }

        private void WrapExceptionAndRethrow(
            Exception ex) {
            throw new PackageJsonException(
                string.Format(@"Unable to read package.json. Please ensure the file is valid JSON.

Reading failed because the following error occurred:

{0}", ex.Message),
                ex);
        }

        public dynamic Package { get; private set; }
    }
}
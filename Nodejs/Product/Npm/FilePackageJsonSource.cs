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
using System.Threading;

namespace Microsoft.NodejsTools.Npm {
    public class FilePackageJsonSource : IPackageJsonSource {

        private readonly ReaderPackageJsonSource _source;

        public FilePackageJsonSource(string fullPathToFile) {
            if (File.Exists(fullPathToFile)) {
                int retryInterval = 500;
                int attempts = 5;

                // populate _source with retries for recoverable errors.
                while (--attempts >= 0) {
                    try {
                        using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new StreamReader(fin)) {
                            _source = new ReaderPackageJsonSource(reader);
                            break;
                        }
                    } catch (PackageJsonException pje) {
                        WrapExceptionAndRethrow(fullPathToFile, pje);
                    } catch (IOException) {
                        if (attempts <= 0) { throw; }
                    } catch (UnauthorizedAccessException) {
                        if (attempts <= 0) { throw; }
                    }

                    Thread.Sleep(retryInterval);
                    retryInterval *= 2; // exponential backoff
                }
            }
        }

        private void WrapExceptionAndRethrow(
            string fullPathToFile,
            Exception ex) {
            throw new PackageJsonException(
                        string.Format(@"Error reading package.json at '{0}': {1}", fullPathToFile, ex.Message),
                        ex);
        }

        public dynamic Package { get { return null == _source ? null : _source.Package; } }
    }
}
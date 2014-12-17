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

using System.IO;

namespace Microsoft.NodejsTools.Npm {
    public class DirectoryPackageJsonSource : IPackageJsonSource {
        private readonly FilePackageJsonSource _source;

        public DirectoryPackageJsonSource(string fullDirectoryPath) {
            _source = new FilePackageJsonSource(Path.Combine(fullDirectoryPath, "package.json"));
        }

        public dynamic Package {
            get { return _source.Package; }
        }
    }
}
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

using System.Collections.Generic;
using System.Text;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmUpdateCommand : NpmCommand {
        public NpmUpdateCommand(string fullPathToRootPackageDirectory, bool global, string pathToNpm = null)
            : this(fullPathToRootPackageDirectory, new List<IPackage>(), global, pathToNpm) { }

        public NpmUpdateCommand(
            string fullPathToRootPackageDirectory,
            IEnumerable<IPackage> packages,
            bool global,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm) {
            var buff = new StringBuilder("update");
            if (global) {
                buff.Append(" -g");
            }

            foreach (var package in packages) {
                buff.Append(' ');
                buff.Append(package.Name);
            }

            if (!global) {
                buff.Append(" --save");
            }
            Arguments = buff.ToString();
        }
    }
}
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
using System.Text;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmLsCommand : NpmCommand {
        private string _listBaseDirectory;

        public NpmLsCommand(
            string fullPathToRootPackageDirectory,
            bool global,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm) {

            // We will have issues parsing the output if there are warnings.
            // npm warns on every command, even if that command has nothing to do with the warning.
            // For npm ls, we should use the silent flag because the warnings will never matter. 
            var buff = new StringBuilder("ls --silent");
            if (global) {
                buff.Append(" -g");
            }
            Arguments = buff.ToString();
        }

        public string ListBaseDirectory {
            get {
                if (null == _listBaseDirectory) {
                    var temp = StandardOutput;
                    if (null != temp) {
                        temp.Trim();
                        if (temp.Length > 0) {
                            // The standard output contains an informational 
                            // message added by the base command class through 
                            // the redirector.  We must trim it to get the output 
                            // of the ls command.
                            if (temp.StartsWith("====")) {
                                int index = temp.IndexOf("\n");
                                if (index >= 0) {
                                    temp = temp.Substring(index).Trim();
                                }
                            }

                            var splits = temp.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Length > 0) {
                                _listBaseDirectory = splits[0].Trim();
                            }
                        }
                    }
                }
                return _listBaseDirectory;
            }
        }
    }
}
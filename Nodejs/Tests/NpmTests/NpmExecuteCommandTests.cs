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

using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Npm.SPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    [TestClass]
    public class NpmExecuteCommandTests {
        // https://nodejstools.codeplex.com/workitem/1575
        [Ignore]
        [TestMethod, Priority(0), Timeout(180000)]
        public async Task TestNpmCommandProcessExitSucceeds() {
            var npmPath = NpmHelpers.GetPathToNpm();
            var redirector = new NpmCommand.NpmCommandRedirector(new NpmBinCommand(null, false));

            for (int j = 0; j < 200; j++) {
                await NpmHelpers.ExecuteNpmCommandAsync(
                    redirector,
                    npmPath,
                    null,
                    new[] {"config", "get", "registry"},
                    null);
            }
        }
    }
}

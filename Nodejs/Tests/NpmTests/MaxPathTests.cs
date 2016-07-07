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

using System.Diagnostics;
using System.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {

    [TestClass]
    public class MaxPathTests : AbstractPackageJsonTests {

        [TestMethod, Priority(0)]
        public void AngularFullstackScaffoldedProject() {
            using (var manager = new TemporaryFileManager()) {
                var rootDir = FilesystemPackageJsonTestHelpers.CreateRootPackageDir(manager);
                var controller = NpmControllerFactory.Create(rootDir, string.Empty);
                controller.OutputLogged += controller_OutputLogged;
                controller.ErrorLogged += controller_OutputLogged;
                controller.Refresh();

                using (var commander = controller.CreateNpmCommander()) {
                    var task = commander.InstallGlobalPackageByVersionAsync("yo", "*");
                    task.Wait();
                }

                var info = new ProcessStartInfo();

                //  TODO!
            }
        }

        void controller_OutputLogged(object sender, NpmLogEventArgs e) {
            Debug.WriteLine(e.LogText);
        }
    }
}

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
    public class MaxPathTests : AbstractFilesystemPackageJsonTests {

        [TestMethod, Priority(0)]
        public void TestAngularFullstackScaffoldedProject(){
            var rootDir = CreateRootPackageDir();
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

        [TestMethod, Priority(0)]
        public void TestInstallUninstallMaxPathGlobalModule() {
            var controller = NpmControllerFactory.Create(string.Empty, string.Empty);
            
            using (var commander = controller.CreateNpmCommander()) {
                commander.InstallGlobalPackageByVersionAsync("yo", "^1.2.0").Wait();
            }

            Assert.IsNotNull(controller.GlobalPackages, "Cannot retrieve global packages after install");
            Assert.IsTrue(controller.GlobalPackages.Modules.Contains("yo"), "Global package failed to install");

            using (var commander = controller.CreateNpmCommander()) {
                commander.UninstallGlobalPackageAsync("yo").Wait();
            }

            // Command has completed, but need to wait for all files/folders to be deleted.
            Thread.Sleep(5000);

            Assert.IsNotNull(controller.GlobalPackages, "Cannot retrieve global packages after uninstall");
            Assert.IsFalse(controller.GlobalPackages.Modules.Contains("yo"), "Global package failed to uninstall");
        }

        void controller_OutputLogged(object sender, NpmLogEventArgs e) {
            Debug.WriteLine(e.LogText);
        }
    }
}

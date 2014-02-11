using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {

    [TestClass]
    public class MaxPathTests : AbstractFilesystemPackageJsonTests {

        [TestMethod, Priority(0)]
        public void TestAngularFullstackScaffoldedProject(){
            var rootDir = CreateRootPackageDir();
            var controller = NpmControllerFactory.Create(rootDir);
            controller.OutputLogged += controller_OutputLogged;
            controller.ErrorLogged += controller_OutputLogged;
            controller.Refresh();

            using (var commander = controller.CreateNpmCommander()) {
                var task = commander.InstallGlobalPackageByVersionAsync("yo", "*");
                task.Wait();
            }

            var info = new ProcessStartInfo();
        }

        void controller_OutputLogged(object sender, NpmLogEventArgs e) {
            Debug.WriteLine(e.LogText);
        }
    }
}

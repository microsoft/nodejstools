/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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

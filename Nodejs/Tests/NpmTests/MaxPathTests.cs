// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    [TestClass]
    public class MaxPathTests : AbstractPackageJsonTests
    {
        [TestMethod, Priority(0), TestCategory("AppVeyorIgnore")]
        public void InstallUninstallMaxPathGlobalModule()
        {
            using (var manager = new TemporaryFileManager())
            {
                var rootDir = FilesystemPackageJsonTestHelpers.CreateRootPackage(manager, PkgSimple);
                var controller = NpmControllerFactory.Create(rootDir, string.Empty);

                using (var commander = controller.CreateNpmCommander())
                {
                    commander.InstallPackageByVersionAsync("yo", "^1.2.0", DependencyType.Standard, false).Wait();
                }

                Assert.IsNotNull(controller.RootPackage, "Cannot retrieve packages after install");
                Assert.IsTrue(controller.RootPackage.Modules.Contains("yo"), "Package failed to install");

                using (var commander = controller.CreateNpmCommander())
                {
                    commander.UninstallPackageAsync("yo").Wait();
                }

                // Command has completed, but need to wait for all files/folders to be deleted.
                Thread.Sleep(5000);

                Assert.IsNotNull(controller.RootPackage, "Cannot retrieve packages after uninstall");
                Assert.IsFalse(controller.RootPackage.Modules.Contains("yo"), "Package failed to uninstall");
            }
        }
    }
}


// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    [TestClass]
    public class FileSystemPackageJsonTests : AbstractPackageJsonTests
    {
        [TestMethod, Priority(0)]
        public void ReadFromFile()
        {
            using (var manager = new TemporaryFileManager())
            {
                var dir = manager.GetNewTempDirectory();
                var path = Path.Combine(dir.FullName, "package.json");
                FilesystemPackageJsonTestHelpers.CreatePackageJson(path, PkgSimple);
                CheckPackage(PackageJsonFactory.Create(new FilePackageJsonSource(path)));
            }
        }

        [TestMethod, Priority(0)]
        public void ReadFromDirectory()
        {
            using (var manager = new TemporaryFileManager())
            {
                var dir = manager.GetNewTempDirectory();
                FilesystemPackageJsonTestHelpers.CreatePackageJson(Path.Combine(dir.FullName, "package.json"), PkgSimple);
                CheckPackage(PackageJsonFactory.Create(new DirectoryPackageJsonSource(dir.FullName)));
            }
        }

        private static void CheckPackage(IPackageJson pkg)
        {
            Assert.IsNotNull(pkg, "Package should not be null.");
            Assert.AreEqual("TestPkg", pkg.Name, "Package name mismatch.");
            Assert.AreEqual(SemverVersion.Parse("0.1.0"), pkg.Version, "Package version mismatch.");
        }
    }
}


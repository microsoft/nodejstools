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

using System.IO;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    [TestClass]
    public class FileSystemPackageJsonTests : AbstractFilesystemPackageJsonTests {
        private static void CheckPackage(IPackageJson pkg) {
            Assert.IsNotNull(pkg, "Package should not be null.");
            Assert.AreEqual("TestPkg", pkg.Name, "Package name mismatch.");
            Assert.AreEqual(SemverVersion.Parse("0.1.0"), pkg.Version, "Package version mismatch.");
        }

        [TestMethod]
        public void TestReadFromFile() {
            var dir = TempFileManager.GetNewTempDirectory();
            var path = Path.Combine(dir.FullName, "package.json");
            CreatePackageJson(path, PkgSimple);
            CheckPackage(PackageJsonFactory.Create(new FilePackageJsonSource(path)));
        }

        [TestMethod]
        public void TestReadFromDirectory() {
            var dir = TempFileManager.GetNewTempDirectory();
            CreatePackageJson(Path.Combine(dir.FullName, "package.json"), PkgSimple);
            CheckPackage(PackageJsonFactory.Create(new DirectoryPackageJsonSource(dir.FullName)));
        }
    }
}
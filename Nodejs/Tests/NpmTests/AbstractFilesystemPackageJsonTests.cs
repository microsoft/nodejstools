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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    public abstract class AbstractFilesystemPackageJsonTests : AbstractPackageJsonTests {
        protected TemporaryFileManager TempFileManager { get; private set; }

        [TestInitialize]
        public void Init() {
            TempFileManager = new TemporaryFileManager();
        }

        [TestCleanup]
        public void Cleanup() {
            TempFileManager.Dispose();
        }

        protected void CreatePackageJson(string filename, string json) {
            using (var fout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)) {
                using (var writer = new StreamWriter(fout)) {
                    writer.Write(json);
                }
            }
        }

        protected string CreateRootPackage(string json) {
            var dir = TempFileManager.GetNewTempDirectory();
            var path = Path.Combine(dir.FullName, "package.json");
            CreatePackageJson(path, json);
            return dir.FullName;
        }
    }
}
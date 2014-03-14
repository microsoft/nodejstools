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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace NodejsTests {
    [TestClass]
    public class TemplateTests {
        [TestMethod, Priority(0)]
        public void TestPackageJsonTemplateEncoding() {
            var projectTemplates = Path.Combine(TestData.BinarySourceLocation, "ProjectTemplates", "JavaScript", "Node.js", "1033");
            Assert.IsTrue(Directory.Exists(projectTemplates), "Project templates are not available");

            // Special cases don't have to include package.json files
            var specialCases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "FromExistingCode.zip" };

            foreach (var file in Directory.EnumerateFiles(projectTemplates, "*.zip")) {
                bool anyChecked = specialCases.Contains(Path.GetFileName(file));
                Console.WriteLine("Checking template {0}", file);

                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true)) {
                    foreach (var package in zip.Entries.Where(e => e.Name == "package.json")) {
                        anyChecked = true;

                        byte[] bytes;
                        using (var packageStream = package.Open())
                        using (var memStream = new MemoryStream()) {
                            bytes = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = packageStream.Read(bytes, 0, bytes.Length)) != 0) {
                                memStream.Write(bytes, 0, bytesRead);
                            }
                            bytes = memStream.GetBuffer();
                        }

                        Assert.IsFalse(
                            bytes.Zip(bytes.Skip(1), (cr, lf) => cr == '\r' && lf == '\n').Any(b => b),
                            string.Format("package.json in {0} should not contains \\r\\n - only \\n", Path.GetFileName(file))
                        );
                        Assert.IsFalse(
                            bytes.Take(3).SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }),
                            string.Format("package.json in {0} should not contain UTF-8 BOM", Path.GetFileName(file))
                        );
                    }
                }

                Assert.IsTrue(anyChecked, "Failed to find package.json in " + file);
            }
        }

    }
}

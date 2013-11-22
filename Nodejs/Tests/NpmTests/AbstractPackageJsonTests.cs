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

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests{
    public abstract class AbstractPackageJsonTests{
        protected const string PkgEmpty = "{}";

        protected const string PkgSimple = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0""
}";

        protected IPackageJson LoadFrom(string json){
            return PackageJsonFactory.Create(new MockPackageJsonSource(json));
        }

        protected IPackageJson LoadFromResource(string manifestResourceName){
            using (
                var reader =
                    new StreamReader(
                        typeof (AbstractPackageJsonTests).Assembly.GetManifestResourceStream(
                            manifestResourceName))){
                return LoadFrom(reader.ReadToEnd());
            }
        }

        protected IPackageJson LoadFromFile(string fullPathToFile){
            return PackageJsonFactory.Create(new FilePackageJsonSource(fullPathToFile));
        }

        protected IPackageJson LoadFrom(TextReader reader){
            return PackageJsonFactory.Create(new ReaderPackageJsonSource(reader));
        }

        private static void CheckContains(ISet<string> retrieved, IEnumerable<string> expected){
            foreach (var value in expected){
                Assert.IsTrue(retrieved.Contains(value), string.Format("Expected to find value '{0}'.", value));
            }
        }

        protected static void CheckStringArrayContents(
            IPkgStringArray array,
            int expectedCount,
            IEnumerable<string> expectedValues){
            Assert.IsNotNull(array, "Array should not be null.");
            Assert.AreEqual(expectedCount, array.Count, "Value count mismatch.");

            var retrieved = new HashSet<string>();
            foreach (string file in array){
                retrieved.Add(file);
            }
            CheckContains(retrieved, expectedValues);

            retrieved = new HashSet<string>();
            for (int index = 0, size = array.Count; index < size; ++index){
                retrieved.Add(array[index]);
            }
            CheckContains(retrieved, expectedValues);
        }

        protected static void CheckEmptyArray(IPkgStringArray array){
            CheckStringArrayContents(array, 0, new string[0]);
        }
    }
}
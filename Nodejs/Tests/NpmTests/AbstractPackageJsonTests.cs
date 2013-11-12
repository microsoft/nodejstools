using System.Collections.Generic;
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
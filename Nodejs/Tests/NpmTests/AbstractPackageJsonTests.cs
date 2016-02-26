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

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    public abstract class AbstractPackageJsonTests {
        protected const string PkgEmpty = "{}";

        protected const string PkgSimple = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0""
}";

        protected IPackageJson LoadFrom(string json) {
            return PackageJsonFactory.Create(new MockPackageJsonSource(json));
        }

        protected string LoadStringFromResource(string manifestResourceName) {
            using (var reader = new StreamReader(typeof(AbstractPackageJsonTests).Assembly.GetManifestResourceStream(manifestResourceName))) {
                return reader.ReadToEnd();
            }
        }

        protected IPackageJson LoadFromResource(string manifestResourceName) {
            using (var reader = new StreamReader(typeof(AbstractPackageJsonTests).Assembly.GetManifestResourceStream(manifestResourceName))) {
                return LoadFrom(reader);
            }
        }

        protected IPackageJson LoadFromFile(string fullPathToFile) {
            return PackageJsonFactory.Create(new FilePackageJsonSource(fullPathToFile));
        }

        protected IPackageJson LoadFrom(TextReader reader) {
            return PackageJsonFactory.Create(new ReaderPackageJsonSource(reader));
        }

        private static void CheckContains(ISet<string> retrieved, IEnumerable<string> expected) {
            foreach (var value in expected) {
                Assert.IsTrue(retrieved.Contains(value), string.Format("Expected to find value '{0}'.", value));
            }
        }

        protected static void CheckStringArrayContents(
            IPkgStringArray array,
            int expectedCount,
            IEnumerable<string> expectedValues) {
            Assert.IsNotNull(array, "Array should not be null.");
            Assert.AreEqual(expectedCount, array.Count, "Value count mismatch.");

            var retrieved = new HashSet<string>();
            foreach (string file in array) {
                retrieved.Add(file);
            }
            CheckContains(retrieved, expectedValues);

            retrieved = new HashSet<string>();
            for (int index = 0, size = array.Count; index < size; ++index) {
                retrieved.Add(array[index]);
            }
            CheckContains(retrieved, expectedValues);
        }

        protected static void CheckEmptyArray(IPkgStringArray array) {
            CheckStringArrayContents(array, 0, new string[0]);
        }
    }
}
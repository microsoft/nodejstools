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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    [TestClass]
    [DeploymentItem(@"TestData\NpmPackageJsonData\", "NpmPackageJsonData")]
    public class ProblematicPackageJsonTests : AbstractPackageJsonTests {

        [TestMethod, Priority(0)]
        public void TestFreshPackageJsonParseFromResource() {
            var pkg = LoadFromResource("NpmTests.TestData.fresh_package.json");
            Assert.IsNotNull(pkg, "Fresh package should not be null.");
        }

        private void TestParseFromFile(string filename) {
            string file = string.Format(@"NpmPackageJsonData\{0}", filename);
            var pkg = LoadFromFile(file);
            Assert.IsNotNull(
                pkg,
                string.Format("Package from file '{0}' should not be null.", file));
        }

        [TestMethod, Priority(0)]
        public void TestFreshPackageParseFromFile() {
            TestParseFromFile("fresh_package.json");
        }

        private void TestFreshPackage(string suffix) {
            TestParseFromFile(string.Format("fresh_package_{0}.json", suffix));
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseDoubleColon() {
            TestFreshPackage("doublecolon");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseDoubleComma() {
            TestFreshPackage("doublecomma");
        }

        [Ignore]
        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseDuplicateProperty() {
            TestFreshPackage("duplicateproperty");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseLeadingBrace() {
            TestFreshPackage("leadingbrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseLeadingLetter() {
            TestFreshPackage("leadingletter");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseLeadingSquareBrace() {
            TestFreshPackage("leadingsquarebrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingColon() {
            TestFreshPackage("missingcolon");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingComma() {
            TestFreshPackage("missingcomma");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingLeadingBrace() {
            TestFreshPackage("missingleadingbrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingLeadingListBrace() {
            TestFreshPackage("missingleadinglistbrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingLeadingPropNameQuote() {
            TestFreshPackage("missingleadingpropnamequote");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingOnePropNameQuote() {
            TestFreshPackage("missingonepropnamequote");
        }

        [TestMethod, Priority(0)]
        public void TestParseMissingPropNameQuotes() {
            TestFreshPackage("missingpropnamequotes");
        }

        [Ignore]
        [TestMethod, Priority(0)]
        public void TestParseMissingTrailingBrace() {
            TestFreshPackage("missingtrailingbrace");
        }

        [Ignore]
        [TestMethod, Priority(0)]
        public void TestParseMissingTrailingListBrace() {
            TestFreshPackage("missingtrailinglistbrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseUnescapedQuote() {
            TestFreshPackage("unescapedquote");
        }

        [TestMethod, Priority(0)]
        public void TestParseCppStyleComment_WorkItem563() {
            var buff = new StringBuilder(@"{
  ""name"": ""angular-app-server"",
  ""description"": ""Back end server to support our angular app"",
  ""version"": ""0.0.1"",
  ""private"": true,
  ""dependencies"": {
    ""express"": ""~3.0"",
    ""passport"": ""~0.1.12"",
    //""passport-local"": ""~0.1.6"",
    ""express-namespace"": ""~0.1.1"",
    ""open"": ""0.0.3"",
    ""request"": ""~2.16.6""
  },
  ""devDependencies"": {
    ""rewire"": ""~1.0.3"",
    ""supervisor"": ""~0.4.1"",
    ""grunt"": ""~0.4"",
    ""grunt-contrib-jshint"": ""~0.2.0"",
    ""grunt-contrib-nodeunit"": ""~0.1.2""
  }
}
");
            ParseFromBuff(buff);
        }

        private void ParseFromBuff(StringBuilder buff) {
            try {
                using (var reader = new StringReader(buff.ToString())) {
                    LoadFrom(reader);
                }
            } catch (PackageJsonException) {
                //  This is fine -> do nothing
            }
        }

        [TestMethod, Priority(0)]
        public void TestParseFromEveryCharValue() {
            var buff = new StringBuilder();
            var ch = (char)0;
            do {
                buff.Append(ch);

                ParseFromBuff(buff);

                buff.Length = 0;
                ++ch;
            } while (ch != 0);
        }

        [TestMethod, Priority(0)]
        public void VeryEvilRandom1CharCorruptionTest() {
            var original = LoadStringFromResource("NpmTests.TestData.fresh_package.json");
            var buff = new StringBuilder();
            var generator = new Random();

            var ch = (char)0;
            var index = 0;

            try {
                do {
                    buff.Append(original);
                    index = generator.Next(buff.Length);
                    buff[index] = ch;

                    ParseFromBuff(buff);

                    buff.Length = 0;
                    ++ch;
                } while (ch != 0);
            } catch (Exception ex) {
                throw new PackageJsonException(
                    string.Format(@"Corruption/replacement test failed replacing with character code {0:x} at index {1} in content:

{2}

Exception message: {3}", (int)ch, index, buff, ex.Message),
                       ex);
            }
        }

        [TestMethod, Priority(0)]
        public void VeryEvilRandom1CharInsertionTest() {
            var original = LoadStringFromResource("NpmTests.TestData.fresh_package.json");
            var buff = new StringBuilder();
            var generator = new Random();

            var ch = (char)0;
            var index = 0;

            try {
                do {
                    buff.Append(original);
                    index = generator.Next(buff.Length);
                    buff.Insert(index, ch);

                    ParseFromBuff(buff);

                    buff.Length = 0;
                    ++ch;
                } while (ch != 0);
            } catch (Exception ex) {
                throw new PackageJsonException(
                    string.Format(@"Insertion test failed inserting character code {0:x} at index {1} in content:

{2}

Exception message: {3}", (int)ch, index, buff, ex.Message),
                       ex);
            }
        }
    }
}

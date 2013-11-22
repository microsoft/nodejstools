using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NpmTests
{
    [TestClass]
    public class ProblematicPackageJsonTests : AbstractPackageJsonTests{

        [ClassInitialize]
        public static void Init(TestContext context){
            NodejsTestData.Deploy();
        }

        [TestMethod]
        public void TestFreshPackageJsonParseFromResource(){
            var pkg = LoadFromResource("NpmTests.Resources.fresh_package.json");
            Assert.IsNotNull(pkg, "Fresh package should not be null.");
        }

        private void TestParseFromFile(string filename){
            string file = TestData.GetPath(@"TestData\NpmPackageJsonData\" + filename);
            var pkg = LoadFromFile(file);
            Assert.IsNotNull(
                pkg,
                string.Format("Package from file '{0}' should not be null.", file));
        }

        [TestMethod]
        public void TestFreshPackageParseFromFile(){
            TestParseFromFile("fresh_package.json");
        }

        private void TestFreshPackage(string suffix){
            TestParseFromFile(string.Format("fresh_package_{0}.json", suffix));
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseDoubleColon(){
            TestFreshPackage("doublecolon");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseDoubleComma(){
            TestFreshPackage("doublecomma");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseDuplicateProperty(){
            TestFreshPackage("duplicateproperty");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseLeadingBrace(){
            TestFreshPackage("leadingbrace");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseLeadingLetter(){
            TestFreshPackage("leadingletter");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseLeadingSquareBrace(){
            TestFreshPackage("leadingsquarebrace");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingColon(){
            TestFreshPackage("missingcolon");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingComma(){
            TestFreshPackage("missingcomma");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingLeadingBrace(){
            TestFreshPackage("missingleadingbrace");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingLeadingListBrace(){
            TestFreshPackage("missingleadinglistbrace");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingLeadingPropNameQuote(){
            TestFreshPackage("missingleadingpropnamequote");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseMissingOnePropNameQuote(){
            TestFreshPackage("missingonepropnamequote");
        }

        [TestMethod]
        public void TestParseMissingPropNameQuotes(){
            TestFreshPackage("missingpropnamequotes");
        }

        [TestMethod]
        public void TestParseMissingTrailingBrace(){
            TestFreshPackage("missingtrailingbrace");
        }

        [TestMethod]
        public void TestParseMissingTrailingListBrace(){
            TestFreshPackage("missingtrailinglistbrace");
        }

        [TestMethod]
        [ExpectedException(typeof(PackageJsonException))]
        public void TestParseUnescapedQuote(){
            TestFreshPackage("unescapedquote");
        }

        [TestMethod]
        public void TestParseFromEveryCharValue(){
            var buff = new StringBuilder();
            var ch = (char) 0;
            do{
                buff.Append(ch);

                try{
                    using (var reader = new StringReader(buff.ToString())){
                        LoadFrom(reader);
                    }
                } catch (PackageJsonException){
                    //  This is fine -> do nothing
                }

                buff.Length = 0;
                ++ch;
            } while (ch != 0);
        }
    }
}

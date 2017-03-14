// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    [TestClass]
    [DeploymentItem(@"TestData\NpmPackageJsonData\", "NpmPackageJsonData")]
    public class ProblematicPackageJsonTests : AbstractPackageJsonTests
    {
        [TestMethod, Priority(0)]
        public void FreshPackageJsonParseFromResource()
        {
            var pkg = LoadFromResource("NpmTests.TestData.fresh_package.json");
            Assert.IsNotNull(pkg, "Fresh package should not be null.");
        }

        private void TestParseFromFile(string filename)
        {
            string file = string.Format(@"NpmPackageJsonData\{0}", filename);
            var pkg = LoadFromFile(file);
            Assert.IsNotNull(
                pkg,
                string.Format("Package from file '{0}' should not be null.", file));
        }

        [TestMethod, Priority(0)]
        public void FreshPackageParseFromFile()
        {
            TestParseFromFile("fresh_package.json");
        }

        private void TestFreshPackage(string suffix)
        {
            TestParseFromFile(string.Format("fresh_package_{0}.json", suffix));
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseDoubleColon()
        {
            TestFreshPackage("doublecolon");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseDoubleComma()
        {
            TestFreshPackage("doublecomma");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseDuplicateProperty()
        {
            TestFreshPackage("duplicateproperty");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseLeadingBrace()
        {
            TestFreshPackage("leadingbrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseLeadingLetter()
        {
            TestFreshPackage("leadingletter");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseLeadingSquareBrace()
        {
            TestFreshPackage("leadingsquarebrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseMissingColon()
        {
            TestFreshPackage("missingcolon");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseMissingComma()
        {
            TestFreshPackage("missingcomma");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseMissingLeadingBrace()
        {
            TestFreshPackage("missingleadingbrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseMissingLeadingListBrace()
        {
            TestFreshPackage("missingleadinglistbrace");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseMissingLeadingPropNameQuote()
        {
            TestFreshPackage("missingleadingpropnamequote");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseMissingOnePropNameQuote()
        {
            TestFreshPackage("missingonepropnamequote");
        }

        [TestMethod, Priority(0)]
        public void ParseMissingPropNameQuotes()
        {
            TestFreshPackage("missingpropnamequotes");
        }

        [TestMethod, Priority(0)]
        [ExpectedException(typeof(PackageJsonException))]
        public void ParseUnescapedQuote()
        {
            TestFreshPackage("unescapedquote");
        }

        [TestMethod, Priority(0)]
        public void ParseCppStyleComment_WorkItem563()
        {
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

        private void ParseFromBuff(StringBuilder buff)
        {
            try
            {
                using (var reader = new StringReader(buff.ToString()))
                {
                    LoadFrom(reader);
                }
            }
            catch (PackageJsonException)
            {
                //  This is fine -> do nothing
            }
        }

        [TestMethod, Priority(0)]
        public void ParseFromEveryCharValue()
        {
            var buff = new StringBuilder();
            var ch = (char)0;
            do
            {
                buff.Append(ch);

                ParseFromBuff(buff);

                buff.Length = 0;
                ++ch;
            } while (ch != 0);
        }

        [TestMethod, Priority(0)]
        public void VeryEvilRandom1CharCorruptionTest()
        {
            var original = LoadStringFromResource("NpmTests.TestData.fresh_package.json");
            var buff = new StringBuilder();
            var generator = new Random();

            var ch = (char)0;
            var index = 0;

            try
            {
                do
                {
                    buff.Append(original);
                    index = generator.Next(buff.Length);
                    buff[index] = ch;

                    ParseFromBuff(buff);

                    buff.Length = 0;
                    ++ch;
                } while (ch != 0);
            }
            catch (Exception ex)
            {
                throw new PackageJsonException(
                    string.Format(@"Corruption/replacement test failed replacing with character code {0:x} at index {1} in content:

{2}

Exception message: {3}", (int)ch, index, buff, ex.Message),
                       ex);
            }
        }

        [TestMethod, Priority(0)]
        public void VeryEvilRandom1CharInsertionTest()
        {
            var original = LoadStringFromResource("NpmTests.TestData.fresh_package.json");
            var buff = new StringBuilder();
            var generator = new Random();

            var ch = (char)0;
            var index = 0;

            try
            {
                do
                {
                    buff.Append(original);
                    index = generator.Next(buff.Length);
                    buff.Insert(index, ch);

                    ParseFromBuff(buff);

                    buff.Length = 0;
                    ++ch;
                } while (ch != 0);
            }
            catch (Exception ex)
            {
                throw new PackageJsonException(
                    string.Format(@"Insertion test failed inserting character code {0:x} at index {1} in content:

{2}

Exception message: {3}", (int)ch, index, buff, ex.Message),
                       ex);
            }
        }
    }
}


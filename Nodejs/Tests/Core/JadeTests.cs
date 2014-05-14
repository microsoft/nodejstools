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
using System.Reflection;
using System.Text;
using Microsoft.NodejsTools.Jade;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace NodejsTests {
    [TestClass]
    public class JadeTokenizerTest {
        static bool _regenerateBaselineFiles = false;

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File01() {
            Tokenize("001.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File02() {
            Tokenize("002.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File03() {
            Tokenize("003.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File04() {
            Tokenize("004.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File05() {
            Tokenize("005.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File06() {
            Tokenize("006.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File07() {
            Tokenize("007.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File08() {
            Tokenize("008.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File09() {
            Tokenize("009.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File10() {
            Tokenize("010.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File11() {
            Tokenize("011.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File12() {
            Tokenize("012.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File13() {
            Tokenize("013.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File14() {
            Tokenize("014.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File15() {
            Tokenize("015.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File16() {
            Tokenize("016.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File17() {
            Tokenize("017.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File18() {
            Tokenize("018.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File19() {
            Tokenize("019.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File20() {
            Tokenize("020.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File21() {
            Tokenize("021.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File22() {
            Tokenize("022.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File23() {
            Tokenize("023.jade");
        }

        private void Tokenize(string fileName) {
            string path = TestData.GetPath(Path.Combine("TestData", "Jade", fileName));
            TokenizeFile<JadeToken, JadeTokenType>(path, new JadeTokenizer(null), _regenerateBaselineFiles);
        }

        static public string LoadFileAsString(string name) {
            StreamReader sr = new StreamReader(name);
            string s = sr.ReadToEnd();
            sr.Close();
            return s;
        }

        static public string LoadFile(string language, string subfolder, string fileName) {
            var filePath = GetTestFilesPath(language, subfolder);

            return LoadFileAsString(filePath + "\\" + fileName);
        }

        static public string GetTestFilesPath(string language, string subfolder = null) {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string assemblyLoc = Path.GetDirectoryName(thisAssembly);

            string path = Path.Combine(assemblyLoc, @"Files\", language);

            if (subfolder != null)
                path = Path.Combine(path, subfolder);

            return path;
        }

        public static IList<string> GetFiles(string language, string folder, string extension) {
            string path = GetTestFilesPath(language, folder);
            var files = new List<string>();

            IEnumerable<string> filesInFolder = Directory.EnumerateFiles(path);
            foreach (string name in filesInFolder) {
                if (name.EndsWith(extension))
                    files.Add(name);
            }

            return files;
        }

        static void TokenizeFile<TokenClass, TokenType>(string fileName, ITokenizer<TokenClass> tokenizer, bool regenerateBaselineFiles) where TokenClass : IToken<TokenType> {
            string baselineFile = fileName + ".tokens";
            string text = LoadFileAsString(fileName);

            var tokens = tokenizer.Tokenize(new TextStream(text), 0, text.Length);
            var actual = WriteTokens<TokenClass, TokenType>(tokens);

            BaselineCompare.CompareFiles(baselineFile, actual, regenerateBaselineFiles);
        }

        static string WriteTokens<TokenClass, TokenType>(ReadOnlyTextRangeCollection<TokenClass> tokens) where TokenClass : IToken<TokenType> {
            var sb = new StringBuilder();
            foreach (var token in tokens) {
                var tt = token.TokenType.ToString();
                tt = tt.Substring(tt.LastIndexOf('.') + 1);

                var formattedToken = String.Format("{0} [{1}...{2}]\r\n", tt, token.Start, token.End);

                sb.Append(formattedToken);
            }

            return sb.ToString();
        }
    }
}

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

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File01() {
            Tokenize("001.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File02() {
            Tokenize("002.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File03() {
            Tokenize("003.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File04() {
            Tokenize("004.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File05() {
            Tokenize("005.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File06() {
            Tokenize("006.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File07() {
            Tokenize("007.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File08() {
            Tokenize("008.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File09() {
            Tokenize("009.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File10() {
            Tokenize("010.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File11() {
            Tokenize("011.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File12() {
            Tokenize("012.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File13() {
            Tokenize("013.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File14() {
            Tokenize("014.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File15() {
            Tokenize("015.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File16() {
            Tokenize("016.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File17() {
            Tokenize("017.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File18() {
            Tokenize("018.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File19() {
            Tokenize("019.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File20() {
            Tokenize("020.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File21() {
            Tokenize("021.jade");
        }

        [TestMethod, Priority(0)]
        public void JadeTokenizerTest_File22() {
            Tokenize("022.jade");
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void JadeTokenizerTest_File23() {
            Tokenize("023.jade");
        }

        private void Tokenize(string fileName) {
            string path = TestData.GetPath(Path.Combine("TestData", "Jade", fileName));
            TokenizeFile<JadeToken, JadeTokenType>(path, new JadeTokenizer(null), _regenerateBaselineFiles);
        }

        static public string LoadFile(string language, string subfolder, string fileName) {
            var filePath = GetTestFilesPath(language, subfolder);

            return File.ReadAllText(filePath + "\\" + fileName);
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
            string text = File.ReadAllText(fileName);

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

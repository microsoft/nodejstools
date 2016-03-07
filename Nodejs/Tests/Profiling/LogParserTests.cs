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

using Microsoft.NodejsTools.LogParsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace ProfilerTests {
    [TestClass]
    public class LogParserTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void SplitRecord() {
            AssertExpectedRecords(@"shared-library,""C:\Program Files\nodejs\node.exe"",0x05c60000,0x06226000",
                "shared-library",
                @"""C:\Program Files\nodejs\node.exe""",
                "0x05c60000",
                "0x06226000");

            AssertExpectedRecords(@"code-creation,LazyCompile,0xf1617840,756,""STRING_ADD_LEFT native runtime.js:183"",0x90c07ec8,~",
                "code-creation",
                "LazyCompile",
                "0xf1617840",
                "756",
                @"""STRING_ADD_LEFT native runtime.js:183""",
                "0x90c07ec8",
                "~");

            AssertExpectedRecords(@"code-creation,LazyCompile,0xf1617840,756,""STRING_ADD_LEFT native,runtime.js:183"",0x90c07ec8,~",
                "code-creation",
                "LazyCompile",
                "0xf1617840",
                "756",
                @"""STRING_ADD_LEFT native,runtime.js:183""",
                "0x90c07ec8",
                "~");

            AssertExpectedRecords(@"code-creation,LoadIC,0xf16e4120,284,""""",
               "code-creation",
               "LoadIC",
               "0xf16e4120",
               "284",
               @"""""");

            AssertExpectedRecords(@"code-creation,LoadIC,0xf16e4120,284,""foo""""bar""",
               "code-creation",
               "LoadIC",
               "0xf16e4120",
               "284",
               @"""foo""bar""");
        }

        public void AssertExpectedRecords(string input, params string[] expected) {
            var records = LogConverter.SplitRecord(input);
            Assert.AreEqual(expected.Length, records.Length);
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i], records[i]);
            }
        }

        [TestMethod, Priority(0), TestCategory("Ignore")]
        public void TestFilenameParsing() {
            AssertExpectedFileInfo(
                " net.js:931",
                "net",
                "anonymous method",
                "net.js",
                931
            );

            AssertExpectedFileInfo(
                "f C:\\Source\\NodeApp2\\NodeApp2\\server.js:5",
                "server",
                "f",
                "C:\\Source\\NodeApp2\\NodeApp2\\server.js",
                5
            );

            AssertExpectedFileInfo(
                " C:\\Source\\NodeApp2\\NodeApp2\\server.js:16",
                "server",
                "anonymous method",
                "C:\\Source\\NodeApp2\\NodeApp2\\server.js",
                16
            );

            // https://nodejstools.codeplex.com/workitem/125
            AssertExpectedFileInfo(
                "native array.js",
                "<top-level module code>",
                "native array",
                "native array.js",
                1,
                "Script"
            );

            AssertExpectedFileInfo(
                @" C:\Users\dinov\visual studio 2012\Projects\NodeApp17\NodeApp17\server.js:1",
                "<top-level module code>",
                "server",
                @"C:\Users\dinov\visual studio 2012\Projects\NodeApp17\NodeApp17\server.js",
                1,
                "Function"
            );
            AssertExpectedFileInfo(
                @"C:\Users\dinov\visual studio 2012\Projects\NodeApp17\NodeApp17\server.js",
                "<top-level module code>",
                "server",
                @"C:\Users\dinov\visual studio 2012\Projects\NodeApp17\NodeApp17\server.js",
                1,
                "Script"
            );
        }

        public void AssertExpectedFileInfo(string input, string ns, string function, string filename, int? lineNo, string type="LazyCompile") {
            foreach (var curInput in new[] { input, "\"" + input + "\"" }) {
                var res = LogConverter.ExtractNamespaceAndMethodName(curInput, false, type);
                Assert.AreEqual(ns, res.Namespace);
                Assert.AreEqual(function, res.Function);
                Assert.AreEqual(filename, res.Filename);
                Assert.AreEqual(lineNo, res.LineNumber);
            }
        }
    }

    
}

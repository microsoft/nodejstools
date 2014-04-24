using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;
using TestUtilities.Nodejs;

namespace AnalysisTests {
    [TestClass]
    public class RequireTests {
        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            NodejsTestData.Deploy();
        }

        [TestMethod]
        public void TestRequireAssignedExports() {
            var entries = Analysis.Analyze(
                new AnalysisFile("mod.js", @"var x = require('mymod').value;"),
                new AnalysisFile("node_modules\\mymod\\index.js", @"module.exports = require('./realindex.js');"),
                new AnalysisFile("node_modules\\mymod\\realindex.js", @"exports.value = 42;")
            );

            AssertUtil.ContainsExactly(
                entries["mod.js"].Analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }

        // TODO: Negative tests
        // module = {}
        // module.exports = 42;
        
        //  and
        // exports = 42
        // Both should result in the original exports being seen, not the 
        // assigned exports.

        [TestMethod]
        public void TestRequireSimple() {
            var mod1 = @"exports.foo = 42;";
            var mod2 = @"x = require('./one.js').foo";

            var sourceUnit1 = AnalysisTests.GetSourceUnit(mod1);
            var sourceUnit2 = AnalysisTests.GetSourceUnit(mod2);
            var state = new JsAnalyzer();
            var entry1 = state.AddModule("one.js", null);
            var entry2 = state.AddModule("two.js", null);
            AnalysisTests.Prepare(entry1, sourceUnit1);
            AnalysisTests.Prepare(entry2, sourceUnit2);

            entry1.Analyze(CancellationToken.None);
            entry2.Analyze(CancellationToken.None);

            AssertUtil.ContainsExactly(
                entry2.Analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestBadRequire() {
            // foo.js
            //      require('./rec1')
            // rec1\
            //      package.json
            //          { "main": "../rec2" }
            // rec2\
            //      package.json
            //          { "main": "../rec1" }

            var analyzer = new JsAnalyzer();
            var mod = @"var x = require('./rec1')";
            analyzer.AddPackageJson("rec1\\package.json", "../rec2");
            analyzer.AddPackageJson("rec2\\package.json", "../rec1");

            var sourceUnit = AnalysisTests.GetSourceUnit(mod);
            var entry = analyzer.AddModule("one.js", null);
            AnalysisTests.Prepare(entry, sourceUnit);

            entry.Analyze(CancellationToken.None);

            Assert.AreEqual(
                0,
                entry.Analysis.GetTypeIdsByIndex("x", 0).Count()
            );
        }

        [TestMethod]
        public void TestRequireNodeModules() {
            var mod1 = @"exports.foo = 42;";
            var mod2 = @"x = require('one.js').foo";

            var sourceUnit1 = AnalysisTests.GetSourceUnit(mod1);
            var sourceUnit2 = AnalysisTests.GetSourceUnit(mod2);
            var state = new JsAnalyzer();
            var entry1 = state.AddModule("node_modules\\one.js", null);
            var entry2 = state.AddModule("two.js", null);
            AnalysisTests.Prepare(entry1, sourceUnit1);
            AnalysisTests.Prepare(entry2, sourceUnit2);

            entry1.Analyze(CancellationToken.None);
            entry2.Analyze(CancellationToken.None);

            AssertUtil.ContainsExactly(
                entry2.Analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }

        [TestMethod]
        public void TestRequireBuiltin() {
            string code = @"
var http = require('http');
";
            var analysis = AnalysisTests.ProcessText(code);
            AssertUtil.ContainsAtLeast(
                analysis.GetMembersByIndex("http", 0).Select(x => x.Name),
                "Server", "ServerResponse", "Agent", "ClientRequest",
                "createServer", "createClient", "request", "get"
            );
        }

        [TestMethod]
        public void TestRequire() {
            var testCases = new[] {
                new { File="server.js", Line = 4, Type = "mymod.", Expected = "mymod_export" },
                new { File="server.js", Line = 8, Type = "mymod2.", Expected = "mymod_export" },
                new { File="server.js", Line = 12, Type = "mymod3.", Expected = (string)null },
                new { File="server.js", Line = 19, Type = "foo.", Expected = "foo_export" },
                new { File="server.js", Line = 22, Type = "bar.", Expected = "bar_entry" },
                new { File="server.js", Line = 25, Type = "bar2.", Expected = "bar2_entry" },
                new { File="server.js", Line = 28, Type = "dup.", Expected = "node_modules_dup" },
                new { File="server.js", Line = 31, Type = "dup1.", Expected = "top_level" },
                new { File="server.js", Line = 34, Type = "dup2.", Expected = "top_level" },
                new { File="server.js", Line = 37, Type = "baz_dup.", Expected = "baz_dup" },
                new { File="server.js", Line = 40, Type = "baz_dup2.", Expected = "baz_dup" },
                new { File="server.js", Line = 42, Type = "recursive.", Expected = "recursive1" },
                new { File="server.js", Line = 42, Type = "recursive.", Expected = "recursive2" },
                new { File="server.js", Line = 48, Type = "nested.", Expected = (string)null },
                new { File="server.js", Line = 54, Type = "indexfolder.", Expected = "indexfolder" },
                new { File="server.js", Line = 56, Type = "indexfolder2.", Expected = "indexfolder" },
                // TODO: Requires require('path').resolve('./indexfolder') to work
                //new { File="server.js", Line = 60, Type = "resolve_path.", Expected = "indexfolder" },

                new { File="node_modules\\mymod.js", Line = 5, Type = "dup.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 8, Type = "dup0.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 11, Type = "dup1.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 14, Type = "dup2.", Expected = "node_modules_dup" },
                new { File="node_modules\\mymod.js", Line = 17, Type = "dup3.", Expected = "dup" },

                new { File="node_modules\\foo\\index.js", Line = 5, Type = "dup.", Expected = "foo_node_modules" },
                new { File="node_modules\\foo\\index.js", Line = 8, Type = "dup1.", Expected = "dup" },
                new { File="node_modules\\foo\\index.js", Line = 11, Type = "dup2.", Expected = "dup" },
                new { File="node_modules\\foo\\index.js", Line = 14, Type = "other.", Expected = "other" },
                new { File="node_modules\\foo\\index.js", Line = 17, Type = "other2.", Expected = "other" },
                new { File="node_modules\\foo\\index.js", Line = 20, Type = "other3.", Expected = (string)null },
                new { File="node_modules\\foo\\index.js", Line = 27, Type = "other4.", Expected = (string)null },

                new { File="baz\\dup.js", Line = 3, Type = "parent_dup.", Expected = "top_level" },
                new { File="baz\\dup.js", Line = 6, Type = "bar.", Expected = "bar_entry" },
                new { File="baz\\dup.js", Line = 9, Type = "parent_dup2.", Expected = "top_level" },
            };

            var analyzer = new JsAnalyzer();

            Dictionary<string, IJsProjectEntry> entries = new Dictionary<string, IJsProjectEntry>();
            var basePath = TestData.GetPath("TestData\\RequireTestApp\\RequireTestApp");
            foreach (var file in Directory.GetFiles(
                basePath,
                "*.js",
                SearchOption.AllDirectories
            )) {
                var entry = analyzer.AddModule(file, null);

                entries[file.Substring(basePath.Length + 1)] = entry;

                AnalysisTests.Prepare(entry, new StreamReader(file));
            }
            var serializer = new JavaScriptSerializer();
            foreach (var file in Directory.GetFiles(
                basePath,
                "package.json",
                SearchOption.AllDirectories
            )) {
                var packageJson = serializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file));
                string mainFile;
                if (packageJson.TryGetValue("main", out mainFile)) {
                    analyzer.AddPackageJson(file, mainFile);
                }
            }

            foreach (var entry in entries) {
                entry.Value.Analyze(CancellationToken.None);
            }
            foreach (var testCase in testCases) {
                Console.WriteLine(testCase);
                var analysis = entries[testCase.File].Analysis;
                var allText = File.ReadAllText(entries[testCase.File].FilePath);
                int offset = 0;
                for (int i = 1; i < testCase.Line; i++) {
                    offset = allText.IndexOf("\r\n", offset);
                    if (offset == -1) {
                        System.Diagnostics.Debug.Fail("failed to find line");
                    }
                    offset += 2;
                }
                var members = analysis.GetMembersByIndex(
                    testCase.Type.Substring(0, testCase.Type.Length - 1),
                    offset
                ).Select(x => x.Name).ToSet();

                if (testCase.Expected == null) {
                    Assert.AreEqual(0, members.Count);
                } else {
                    Assert.IsTrue(members.Contains(testCase.Expected));
                }
            }
        }
    }
}

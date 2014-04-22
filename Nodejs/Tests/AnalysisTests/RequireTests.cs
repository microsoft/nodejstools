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

            Dictionary<string, IPythonProjectEntry> entries = new Dictionary<string, IPythonProjectEntry>();
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

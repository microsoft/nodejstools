using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using TestUtilities;
using TestUtilities.UI;

namespace Microsoft.Nodejs.Tests.UI {
    [TestClass]
    public class ClassificationTests : NodejsProjectTest {
        [TestMethod, Priority(0), TestCategory("Core")]
        [HostType("TC Dynamic"), DynamicHostType(typeof(VsIdeHostAdapter))]
        public void ClassificationTest() {
            var code = @"""use-strict"";

var abc = /quox/;
console.log('Hello world');
console.log(""Hello world"");
function f() {
}

var x = 42;
var x = 42.0;

/*
multiline comment
*/

// comment

var x = 'abc';
var y = 2 + 3;

function x() {
    return 100;
}

abc = x[100];
x = $abc;";

            VerifyClassifications(
                code,
                new Classification("string", 0, 12, "\"use-strict\""),
                new Classification("Node.js operator", 12, 13, ";"),
                new Classification("keyword", 17, 20, "var"),
                new Classification("identifier", 21, 24, "abc"),
                new Classification("Node.js operator", 25, 26, "="),
                new Classification("literal", 27, 33, "/quox/"),
                new Classification("Node.js operator", 33, 34, ";"),
                new Classification("identifier", 36, 43, "console"),
                new Classification("Node.js dot", 43, 44, "."),
                new Classification("identifier", 44, 47, "log"),
                new Classification("Node.js grouping", 47, 48, "("),
                new Classification("string", 48, 61, "'Hello world'"),
                new Classification("Node.js grouping", 61, 62, ")"),
                new Classification("Node.js operator", 62, 63, ";"),
                new Classification("identifier", 65, 72, "console"),
                new Classification("Node.js dot", 72, 73, "."),
                new Classification("identifier", 73, 76, "log"),
                new Classification("Node.js grouping", 76, 77, "("),
                new Classification("string", 77, 90, "\"Hello world\""),
                new Classification("Node.js grouping", 90, 91, ")"),
                new Classification("Node.js operator", 91, 92, ";"),
                new Classification("keyword", 94, 102, "function"),
                new Classification("identifier", 103, 104, "f"),
                new Classification("Node.js grouping", 104, 106, "()"),
                new Classification("Node.js grouping", 107, 108, "{"),
                new Classification("Node.js grouping", 110, 111, "}"),
                new Classification("keyword", 115, 118, "var"),
                new Classification("identifier", 119, 120, "x"),
                new Classification("Node.js operator", 121, 122, "="),
                new Classification("number", 123, 125, "42"),
                new Classification("Node.js operator", 125, 126, ";"),
                new Classification("keyword", 128, 131, "var"),
                new Classification("identifier", 132, 133, "x"),
                new Classification("Node.js operator", 134, 135, "="),
                new Classification("number", 136, 140, "42.0"),
                new Classification("Node.js operator", 140, 141, ";"),
                new Classification("comment", 145, 170, "/*\r\nmultiline comment\r\n*/"),
                new Classification("comment", 174, 184, "// comment"),
                new Classification("keyword", 188, 191, "var"),
                new Classification("identifier", 192, 193, "x"),
                new Classification("Node.js operator", 194, 195, "="),
                new Classification("string", 196, 201, "'abc'"),
                new Classification("Node.js operator", 201, 202, ";"),
                new Classification("keyword", 204, 207, "var"),
                new Classification("identifier", 208, 209, "y"),
                new Classification("Node.js operator", 210, 211, "="),
                new Classification("number", 212, 213, "2"),
                new Classification("Node.js operator", 214, 215, "+"),
                new Classification("number", 216, 217, "3"),
                new Classification("Node.js operator", 217, 218, ";"),
                new Classification("keyword", 222, 230, "function"),
                new Classification("identifier", 231, 232, "x"),
                new Classification("Node.js grouping", 232, 234, "()"),
                new Classification("Node.js grouping", 235, 236, "{"),
                new Classification("keyword", 242, 248, "return"),
                new Classification("number", 249, 252, "100"),
                new Classification("Node.js operator", 252, 253, ";"),
                new Classification("Node.js grouping", 255, 256, "}"),
                new Classification("identifier", 260, 263, "abc"),
                new Classification("Node.js operator", 264, 265, "="),
                new Classification("identifier", 266, 267, "x"),
                new Classification("Node.js grouping", 267, 268, "["),
                new Classification("number", 268, 271, "100"),
                new Classification("Node.js grouping", 271, 272, "]"),
                new Classification("Node.js operator", 272, 273, ";"),
                new Classification("identifier", 275, 276, "x"),
                new Classification("Node.js operator", 277, 278, "="),
                new Classification("identifier", 279, 283, "$abc"),
                new Classification("Node.js operator", 283, 284, ";")
            );
        }

        private static void VerifyClassifications(string code, params Classification[] expected) {
            using (var solution = Project(
                "Classifications", Compile("server", code)).Generate().ToVs()) {
                var item = solution.Project.ProjectItems.Item("server.js");

                var window = item.Open();
                window.Activate();

                var doc = solution.App.GetDocument(item.Document.FullName);

                var snapshot = doc.TextView.TextBuffer.CurrentSnapshot;
                var classifier = doc.Classifier;
                var spans = classifier.GetClassificationSpans(new SnapshotSpan(snapshot, 0, snapshot.Length));

                Classification.Verify(spans, expected);
            }
        }

    }
}

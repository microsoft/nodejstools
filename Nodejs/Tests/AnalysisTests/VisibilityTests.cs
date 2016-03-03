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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace AnalysisTests {
    [TestClass]
    public class VisibilityTests {
        [TestMethod, Priority(0)]
        public void DirectedTests() {
            // make sure the order of adding modules doesn't matter...
            var analysis2 = Analysis.Analyze(
                new AnalysisFile("node_modules\\bar\\node_modules\\quox\\adding_nested.js"),
                new AnalysisFile("node_modules\\bar\\node_modules\\quox\\index.js"),
                new AnalysisFile("node_modules\\bar\\adding.js"),
                new AnalysisFile("node_modules\\bar\\index.js"),
                new AnalysisFile("node_modules\\baz\\mymod.js"),
                new AnalysisFile("node_modules\\baz\\index.js"),
                new AnalysisFile("myapp\\index.js"),
                new AnalysisFile("appmod.js"),
                new AnalysisFile("app.js")
            );

            var analysis1 = Analysis.Analyze(
                new AnalysisFile("app.js"),
                new AnalysisFile("appmod.js"),
                new AnalysisFile("myapp\\index.js"),
                new AnalysisFile("node_modules\\baz\\index.js"),
                new AnalysisFile("node_modules\\baz\\mymod.js"),
                new AnalysisFile("node_modules\\bar\\index.js"),
                new AnalysisFile("node_modules\\bar\\adding.js"),
                new AnalysisFile("node_modules\\bar\\node_modules\\quox\\index.js"),
                new AnalysisFile("node_modules\\bar\\node_modules\\quox\\adding_nested.js")
            );            

            foreach (var analysis in new[] { analysis1, analysis2 }) {
                // My peers can see my assignments/I can see my peers assignments 
                AssertIsVisible(analysis, "app.js", "appmod.js");
                AssertIsVisible(analysis, "app.js", "myapp\\index.js");
                AssertIsVisible(analysis, "appmod.js", "app.js");
                AssertIsVisible(analysis, "myapp\\index.js", "appmod.js");

                // My parent and its peers can see my assignments
                AssertIsVisible(analysis, "app.js", "node_modules\\baz\\index.js");
                AssertIsVisible(analysis, "app.js", "node_modules\\baz\\mymod.js");
                AssertIsVisible(analysis, "app.js", "node_modules\\bar\\index.js");
                AssertIsVisible(analysis, "app.js", "node_modules\\bar\\adding.js");

                AssertIsNotVisible(analysis, "app.js", "node_modules\\bar\\node_modules\\quox\\index.js");
                AssertIsNotVisible(analysis, "app.js", "node_modules\\bar\\node_modules\\quox\\adding_nested.js");
            }
        }

        [TestMethod, Priority(0)]
        public void TestVisible() {
            // make sure the order of adding modules doesn't matter...
            var analysis = Analysis.Analyze(
                new AnalysisFile("app.js", "Object.app = 100\r\nabc = Object.quox"),
                new AnalysisFile("node_modules\\foo\\app.js", "Object.foo = 42;"),
                new AnalysisFile("node_modules\\foo\\app2.js", ""),
                new AnalysisFile("node_modules\\foo\\node_modules\\quox\\app.js", "Object.quox = 100")
            );

            AssertUtil.ContainsAtLeast(
                analysis["app.js"].Analysis.GetMembersByIndex("Object", 0).Select(x => x.Name),
                "foo"
            );
            AssertUtil.ContainsAtLeast(
                analysis["node_modules\\foo\\app2.js"].Analysis.GetMembersByIndex("Object", 0).Select(x => x.Name),
                "foo"
            );
            AssertUtil.ContainsAtLeast(
                analysis["node_modules\\foo\\app.js"].Analysis.GetMembersByIndex("Object", 0).Select(x => x.Name),
                "quox"
            );
            AssertUtil.ContainsAtLeast(
                analysis["node_modules\\foo\\app2.js"].Analysis.GetMembersByIndex("Object", 0).Select(x => x.Name),
                "quox"
            );

            AssertUtil.ContainsExactly(
                analysis["app.js"].Analysis.GetTypeIdsByIndex("abc", 0)
            );

            AssertUtil.DoesntContain(
                analysis["app.js"].Analysis.GetMembersByIndex("Object", 0).Select(x => x.Name),
                "quox"
            );
        }


        private static void AssertIsVisible(JsAnalyzer analyzer, string from, string to) {
            Assert.IsTrue(
                IsVisible(analyzer, from, to),
                String.Format("{0} cannot see {1}", from, to)
            );
        }

        private static void AssertIsNotVisible(JsAnalyzer analyzer, string from, string to) {
            Assert.IsFalse(
                IsVisible(analyzer, from, to),
                String.Format("{0} can see {1}", from, to)
            );
        }

        private static bool IsVisible(JsAnalyzer analyzer, string from, string to) {
            return ((ProjectEntry)analyzer[from])._visibleEntries.Contains(
                analyzer[to]
            );
        }
    }
}

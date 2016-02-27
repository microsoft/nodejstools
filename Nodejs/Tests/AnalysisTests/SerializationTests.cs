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

using System.IO;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtilities;

namespace AnalysisTests {
    [TestClass]
    public class SerializationTests {
        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void BasicTest() {
            var analyzer = new JsAnalyzer();
            RoundTrip(analyzer);
        }

        public static T RoundTrip<T>(T value) {
            MemoryStream ms = new MemoryStream();
            new AnalysisSerializer().Serialize(ms, value);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)new AnalysisSerializer().Deserialize(ms);
        }

        [TestMethod, Priority(0), TestCategory("UnitTest")]
        public void RequireTest() {
            var entries = RoundTrip(
                Analysis.Analyze(
                    new AnalysisFile("mod.js", @"var x = require('mymod').value;"),
                    AnalysisFile.PackageJson("node_modules\\mymod\\package.json", "./lib/mymod"),
                    new AnalysisFile("node_modules\\mymod\\lib\\mymod.js", @"exports.value = 42;"),
                    new AnalysisFile("node_modules\\mymod\\lib\\mymod\\foo.js", @"exports.value = 'abc';")
                )
            );

            AssertUtil.ContainsExactly(
                entries["mod.js"].Analysis.GetTypeIdsByIndex("x", 0),
                BuiltinTypeId.Number
            );
        }
    }
}

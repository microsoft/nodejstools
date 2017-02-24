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

using System.Linq;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger
{
    [TestClass]
    public class InfrastructureTests
    {
        [TestMethod, Priority(0)]
        public void NaturalSortComparerTest()
        {
            // Arrange
            var comparer = new NaturalSortComparer();
            var source = new[] { "2name", "1name", "20name", "3name", "11name" };

            // Act
            string[] destination = source.OrderBy(p => p, comparer).ToArray();

            // Assert
            Assert.AreEqual(source.Length, destination.Length);
            Assert.AreEqual("1name", destination[0]);
            Assert.AreEqual("2name", destination[1]);
            Assert.AreEqual("3name", destination[2]);
            Assert.AreEqual("11name", destination[3]);
            Assert.AreEqual("20name", destination[4]);
        }
    }
}
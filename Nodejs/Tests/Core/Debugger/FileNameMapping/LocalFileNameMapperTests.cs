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

using Microsoft.NodejsTools.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.FileNameMapping {
    [TestClass]
    public class LocalFileNameMapperTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetLocalFileNameTests() {
            // Arrange
            const string remoteFileName = "remoteFileName";
            var fileNameMapper = new LocalFileNameMapper();

            // Act
            string fileName = fileNameMapper.GetLocalFileName(remoteFileName);

            // Assert
            Assert.AreEqual(remoteFileName, fileName);
        }
    }
}
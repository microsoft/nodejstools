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
using Microsoft.NodejsTools.Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NodejsTests.Debugger.FileNameMapping {
    [TestClass]
    public class FuzzyLogicFileNameMapperTests {
        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetLocalFileNameForBuiltInModuleTests() {
            // Arrange
            const string remoteFileName = "node.js";
            var localFiles = new List<string> { @"c:\path\to\project\app.js" };
            var fileNameMapper = new FuzzyLogicFileNameMapper(localFiles);

            // Act
            string fileName = fileNameMapper.GetLocalFileName(remoteFileName);

            // Assert
            Assert.AreEqual(remoteFileName, fileName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetLocalFileNameForRemoteModuleTests() {
            // Arrange
            const string remoteFileName = "/root/other/project/path/app.js";
            const string localFileName = @"c:\path\to\project\app.js";
            var localFiles = new List<string> { localFileName };
            var fileNameMapper = new FuzzyLogicFileNameMapper(localFiles);

            // Act
            string fileName = fileNameMapper.GetLocalFileName(remoteFileName);

            // Assert
            Assert.AreEqual(localFileName, fileName);
        }

        [TestMethod, Priority(0), TestCategory("Debugging")]
        public void GetLocalFileNameIfProjectContainsDuplicatesTests() {
            // Arrange
            const string remoteFileName1 = "/root/other/project/path/app.js";
            const string remoteFileName2 = "/root/other/project/path/sub/app.js";
            const string localFileName1 = @"c:\path\To\project\app.js";
            const string localFileName2 = @"c:\Path\to\project\sub\app.js";
            var localFiles = new List<string> { localFileName1, localFileName2 };
            var fileNameMapper = new FuzzyLogicFileNameMapper(localFiles);

            // Act
            string fileName1 = fileNameMapper.GetLocalFileName(remoteFileName1);
            string fileName2 = fileNameMapper.GetLocalFileName(remoteFileName2);

            // Assert
            Assert.IsTrue(string.Equals(localFileName1, fileName1, StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(localFileName2, fileName2, StringComparison.OrdinalIgnoreCase));
        }
    }
}
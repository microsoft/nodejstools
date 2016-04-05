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
using Microsoft.NodejsTools;

namespace NodejsTests.Mocks {
    class MockTextBuffer : TestUtilities.Mocks.MockTextBuffer {
        public MockTextBuffer(string content) :
            base(content: content, contentType: NodejsConstants.Nodejs) { }

        public MockTextBuffer(string content, string contentType, string filename = null) :
            base(content: content,contentType: contentType, filename: GetRandomFileNameIfNull(filename)) { }

        private static string GetRandomFileNameIfNull(string filename) {
            return filename ?? Path.Combine(TestUtilities.TestData.GetTempPath(), Path.GetRandomFileName(), "file.js");
        }
    }
}

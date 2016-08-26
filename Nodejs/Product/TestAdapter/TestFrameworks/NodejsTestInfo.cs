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

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class NodejsTestInfo {
        public NodejsTestInfo(string fullyQualifiedName) {
            string[] parts = fullyQualifiedName.Split(new string[] { "::" }, StringSplitOptions.None);
            if (parts.Length != 3) {
                throw new ArgumentException("Invalid fully qualified test name");
            }
            ModulePath = parts[0];
            TestName = parts[1];
            TestFramework = parts[2];
        }

        public NodejsTestInfo(string modulePath, string testName, string testFramework, int line, int column)
        {
            ModulePath = modulePath;
            TestName = testName;
            TestFramework = testFramework;
            SourceLine = line;
            SourceColumn = column;
        }

        public string FullyQualifiedName {
            get {
                return ModulePath + "::" + TestName + "::" + TestFramework;
            }
        }
        public string ModulePath { get; private set; }

        public string TestName { get; private set; }

        public string TestFramework { get; private set; }

        public int SourceLine { get; private set; }

        public int SourceColumn { get; private set; }
    }
}

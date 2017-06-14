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
using System.IO;
using System.Security.Cryptography;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class NodejsTestInfo {
        public NodejsTestInfo(string fullyQualifiedName, string modulePath) {
            string[] parts = fullyQualifiedName.Split(new string[] { "::" }, StringSplitOptions.None);
            if (parts.Length != 3) {
                throw new ArgumentException("Invalid fully qualified test name");
            }
            ModulePath = modulePath;
            ModuleName = parts[0];
            TestName = parts[1];
            TestFramework = parts[2];
        }

        public NodejsTestInfo(string modulePath, string testName, string testFramework, int line, int column)
        {
            ModulePath = modulePath;
            SetModuleName(ModulePath);
            TestName = testName;
            TestFramework = testFramework;
            SourceLine = line;
            SourceColumn = column;
        }

        private void SetModuleName(string modulePath)
        {
            ModuleName = String.Format("{0}[{1}]",
                (string)Path.GetFileName(modulePath).Split('.').GetValue(0),
                GetHash(modulePath));
        }

        private string GetHash(string filePath)
        {
            try
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    SHA1Managed sha = new SHA1Managed();
                    byte[] hash = sha.ComputeHash(stream);

                    // chop hash in half since we just need a unique ID
                    Int32 startIndex = hash.Length / 2;
                    sha.Dispose();

                    return BitConverter.ToString(hash, startIndex).Replace("-", String.Empty);
                }
            } catch (FileNotFoundException)
            {
                // Just return some default value and let node handle it later
                return "FILE_NOT_FOUND";
            }
        }

        public string FullyQualifiedName {
            get {
                return ModuleName + "::" + TestName + "::" + TestFramework;
            }
        }
        public string ModulePath { get; private set; }

        public string ModuleName { get; private set; }

        public string TestName { get; private set; }

        public string TestFramework { get; private set; }

        public int SourceLine { get; private set; }

        public int SourceColumn { get; private set; }
    }
}

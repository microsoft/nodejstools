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
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TestAdapterTests {
    class MockTestExecutionRecorder : IFrameworkHandle {
        public readonly List<TestResult> Results = new List<TestResult>();

        public bool EnableShutdownAfterTestRun {
            get {
                return false;
            }
            set {
            }
        }

        public int LaunchProcessWithDebuggerAttached(string filePath, string workingDirectory, string arguments, IDictionary<string, string> environmentVariables) {
            return 0;
        }

        public void RecordResult(TestResult result) {
            this.Results.Add(result);
        }

        public void RecordAttachments(IList<AttachmentSet> attachmentSets) {
        }

        public void RecordEnd(TestCase testCase, TestOutcome outcome) {
        }

        public void RecordStart(TestCase testCase) {
        }

        public void SendMessage(TestMessageLevel testMessageLevel, string message) {
        }
    }
}

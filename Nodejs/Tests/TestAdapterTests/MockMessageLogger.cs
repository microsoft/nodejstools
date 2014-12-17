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
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TestAdapterTests {
    class MockMessageLogger : IMessageLogger {
        public readonly List<Tuple<TestMessageLevel, string>> Messages = new List<Tuple<TestMessageLevel, string>>();

        public void SendMessage(TestMessageLevel testMessageLevel, string message) {
            this.Messages.Add(new Tuple<TestMessageLevel, string>(testMessageLevel, message));
        }
    }
}

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
using Microsoft.NodejsTools.TestFrameworks;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks
{
    internal class FrameworkDiscover
    {
        private readonly Dictionary<String, TestFramework> _frameworks;
        public FrameworkDiscover() : this(null)
        {
        }

        public FrameworkDiscover(IEnumerable<string> testFrameworkDirectories)
        {
            if (testFrameworkDirectories == null)
            {
                TestFrameworkDirectories directoryLoader = new TestFrameworkDirectories();
                testFrameworkDirectories = directoryLoader.GetFrameworkDirectories();
            }
            _frameworks = new Dictionary<string, TestFramework>(StringComparer.OrdinalIgnoreCase);
            foreach (string directory in testFrameworkDirectories)
            {
                TestFramework fx = new TestFramework(directory);
                _frameworks.Add(fx.Name, fx);
            }
        }

        public TestFramework Get(string frameworkName)
        {
            TestFramework testFX = null;
            _frameworks.TryGetValue(frameworkName, out testFX);
            return testFX;
        }
    }
}

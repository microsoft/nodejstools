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

using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudioTools.Project;

namespace NodejsTests
{
    internal class TestNodejsReplSite : INodejsReplSite
    {
        private readonly string _filename, _projectDir;
        public static TestNodejsReplSite Instance = new TestNodejsReplSite(null, null);

        public TestNodejsReplSite(string filename, string projectDir)
        {
            _filename = filename;
            _projectDir = projectDir;
        }

        #region INodejsReplSite Members

        public CommonProjectNode GetStartupProject()
        {
            return null;
        }

        public bool TryGetStartupFileAndDirectory(out string fileName, out string directory)
        {
            if (_projectDir != null)
            {
                fileName = _filename;
                directory = _projectDir;
                return true;
            }
            fileName = null;
            directory = null;
            return false;
        }

        #endregion
    }
}

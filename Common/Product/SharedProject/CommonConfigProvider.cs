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

using Microsoft.VisualStudio;

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Enables the Any CPU Platform form name for Dynamic Projects.
    /// Hooks language specific project config.
    /// </summary>
    internal class CommonConfigProvider : ConfigProvider {
        private CommonProjectNode _project;

        public CommonConfigProvider(CommonProjectNode project)
            : base(project) {
            _project = project;
        }

        #region overridden methods

        protected override ProjectConfig CreateProjectConfiguration(string configName) {
            return _project.MakeConfiguration(configName);
        }

        public override int GetPlatformNames(uint celt, string[] names, uint[] actual) {
            if (names != null) {
                names[0] = "Any CPU";
            }

            if (actual != null) {
                actual[0] = 1;
            }

            return VSConstants.S_OK;
        }

        public override int GetSupportedPlatformNames(uint celt, string[] names, uint[] actual) {
            if (names != null) {
                names[0] = "Any CPU";
            }

            if (actual != null) {
                actual[0] = 1;
            }

            return VSConstants.S_OK;
        }
        #endregion
    }
}

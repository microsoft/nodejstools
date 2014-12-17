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


using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project {

    [ComVisible(true)]
    internal class CommonProjectConfig : ProjectConfig {
        private readonly CommonProjectNode/*!*/ _project;

        public CommonProjectConfig(CommonProjectNode/*!*/ project, string configuration)
            : base(project, configuration) {
            _project = project;
        }

        public override int DebugLaunch(uint flags) {
            IProjectLauncher starter = _project.GetLauncher();

            __VSDBGLAUNCHFLAGS launchFlags = (__VSDBGLAUNCHFLAGS)flags;
            if ((launchFlags & __VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug) == __VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug) {
                //Start project with no debugger
                return starter.LaunchProject(false);
            } else {
                //Start project with debugger 
                return starter.LaunchProject(true);
            }
        }
    }
}

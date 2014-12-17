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
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Profiling {
    [Guid("1310992E-71FC-4197-B9D7-45003C95CD75")]
    public interface INodeProfiling {
        INodeProfileSession GetSession(object item);

        /// <summary>
        /// Launches profiling for the provided project using the projects current settings.
        /// </summary>
        INodeProfileSession LaunchProject(EnvDTE.Project projectToProfile, bool openReport = true);

        /// <summary>
        /// Launches profiling for the provided process.  
        /// </summary>
        /// <param name="interpreter">
        /// A full path to an interpreter..
        /// </param>
        /// <param name="script">
        /// Path to the script to profile.
        /// </param>
        /// <param name="workingDir">Working directory to run script from.</param>
        /// <param name="arguments">Any additional arguments which should be provided to the process.</param>
        INodeProfileSession LaunchProcess(string interpreter, string script, string workingDir, string arguments, bool openReport = true);

        void RemoveSession(INodeProfileSession session, bool deleteFromDisk);

        bool IsProfiling {
            get;
        }
    }
}

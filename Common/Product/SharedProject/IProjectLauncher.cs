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

namespace Microsoft.VisualStudioTools.Project {
    /// <summary>
    /// Defines an interface for launching a project or a file with or without debugging.
    /// </summary>
    public interface IProjectLauncher {
        /// <summary>
        /// Starts a project with or without debugging.
        /// 
        /// Returns an HRESULT indicating success or failure.
        /// </summary>
        int LaunchProject(bool debug);

        /// <summary>
        /// Starts a file in a project with or without debugging.
        /// 
        /// Returns an HRESULT indicating success or failure.
        /// </summary>
        int LaunchFile(string file, bool debug);
    }
}

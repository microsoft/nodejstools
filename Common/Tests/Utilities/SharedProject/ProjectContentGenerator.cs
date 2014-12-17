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

using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject {
    /// <summary>
    /// Base class for all generated project items.  Override Generate to create
    /// the item on disk (relative to the MSBuild.Project) and optionally add the
    /// generated item to the project.  
    /// </summary>
    public abstract class ProjectContentGenerator {
        /// <summary>
        /// Generates the specified item.  The item can use the project type to 
        /// customize the item.  The item can write it's self out to disk if 
        /// necessary and update the project file appropriately.
        /// </summary>
        public abstract void Generate(ProjectType projectType, MSBuild.Project project);
    }
}

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
    /// Groups a set of ProjectContentGenerator together.
    /// 
    /// This class exists solely to allow a hierarchy to be written in
    /// source code when describing the test projects.
    /// 
    /// It takes a list of ProjectContentGenerator, and when asked to
    /// generate will generate the list in order.
    /// </summary>
    public class ProjectContentGroup : ProjectContentGenerator {
        private readonly ProjectContentGenerator[] _content;

        public ProjectContentGroup(ProjectContentGenerator[] content) {
            _content = content;
        }

        public override void Generate(ProjectType projectType, MSBuild.Project project) {
            foreach (var content in _content) {
                content.Generate(projectType, project);
            }
        }
    }
}

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
using Microsoft.Build.Construction;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject {
    public class TargetDefinition : ProjectContentGenerator {
        public readonly string Name;
        public readonly Action<ProjectTargetElement>[] Creators;
        
        public TargetDefinition(string name, params Action<ProjectTargetElement>[] creators) {
            Name = name;
            Creators = creators;
        }

        public string DependsOnTargets { get; set; }

        public override void Generate(ProjectType projectType, MSBuild.Project project) {
            var target = project.Xml.AddTarget(Name);
            if (!string.IsNullOrEmpty(DependsOnTargets)) {
                target.DependsOnTargets = DependsOnTargets;
            }
            foreach (var creator in Creators) {
                creator(target);
            }
        }
    }
}

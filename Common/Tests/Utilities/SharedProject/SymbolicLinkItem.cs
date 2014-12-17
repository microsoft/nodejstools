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
using System.IO;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject {
    /// <summary>
    /// Generates a folder and if not excluded adds it to the generated project.
    /// </summary>
    public sealed class SymbolicLinkItem : ProjectContentGenerator {
        public readonly string Name, ReferencePath;
        public readonly bool IsExcluded, IsMissing;

        /// <summary>
        /// Creates a new folder with the specified name.  If the folder
        /// is excluded then it will be created on disk but not added to the
        /// project.
        /// </summary>
        public SymbolicLinkItem(string name, string referencePath, bool isExcluded = false, bool isMissing = false) {
            Name = name;
            ReferencePath = referencePath;
            IsExcluded = isExcluded;
            IsMissing = isMissing;
        }

        public override void Generate(ProjectType projectType, MSBuild.Project project) {
            if (!IsMissing) {
                var absName = Path.IsPathRooted(Name) ? Name : Path.Combine(project.DirectoryPath, Name);
                var absReferencePath = Path.IsPathRooted(ReferencePath) ? ReferencePath : Path.Combine(project.DirectoryPath, ReferencePath);
                
                NativeMethods.CreateSymbolicLink(absName, absReferencePath);
            }

            if (!IsExcluded) {
                project.AddItem("Folder", Name);
            }
        }
    }
}

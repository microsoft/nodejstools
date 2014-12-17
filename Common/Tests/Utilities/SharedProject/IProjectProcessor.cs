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
    /// Updates the generated file before and/or after the project file is generated.
    /// 
    /// This can insert extra data into the project which is required for proper functioning
    /// of the project system.
    /// 
    /// Classes implementing this interface should be exported with a ProjectExtensionAttribute
    /// specifying which project type the processor applies to.
    /// </summary>
    public interface IProjectProcessor {
        /// <summary>
        /// Runs before any test case defined content is added to the project.
        /// 
        /// This should be used to setup must haves for your project system.  Individual
        /// test cases may override your defaults here as appropriate.
        /// </summary>
        void PreProcess(MSBuild.Project project);

        /// <summary>
        /// Runs after all test case defined content is added to the project.
        /// 
        /// This allows any post generation fixups which might be necessary for the project
        /// system.
        /// </summary>
        void PostProcess(MSBuild.Project project);
    }
}

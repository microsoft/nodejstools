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

using System.ComponentModel;

namespace TestUtilities.SharedProject {
    /// <summary>
    /// Metadata interface for getting information about declared project kinds.
    /// MEF requires that this be public.
    /// </summary>
    public interface IProjectTypeDefinitionMetadata {
        string ProjectExtension { get; }
        string ProjectTypeGuid { get; }
        string CodeExtension { get; }

        [DefaultValue("")]
        string SampleCode { get; }
    }
}

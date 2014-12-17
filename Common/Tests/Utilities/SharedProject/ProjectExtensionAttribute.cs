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
using System.ComponentModel.Composition;

namespace TestUtilities.SharedProject {
    /// <summary>
    /// Registers the extension used for a project.  See ProjectTypeDefinition
    /// for how this is used.  This attribute is required.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProjectExtensionAttribute : Attribute {
        public readonly string _projectExtension;

        public ProjectExtensionAttribute(string extension) {
            _projectExtension = extension;
        }

        public string ProjectExtension {
            get {
                return _projectExtension;
            }
        }
    }
}

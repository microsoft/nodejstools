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

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Encapsulates information about a project reference.
    /// 
    /// Project references consist of a name and a kind.  Based upon the kind
    /// you can decode the information in the name which is typically a filename.
    /// </summary>
    public class ProjectReference : IEquatable<ProjectReference> {
        private readonly string _referenceName;
        private readonly ProjectReferenceKind _kind;

        public ProjectReference(string referenceName, ProjectReferenceKind kind) {
            _referenceName = referenceName;
            _kind = kind;
        }

        public string Name {
            get {
                return _referenceName;
            }
        }

        public ProjectReferenceKind Kind {
            get {
                return _kind;
            }
        }

        public override int GetHashCode() {
            return _kind.GetHashCode() ^ _referenceName.GetHashCode();
        }

        public override bool Equals(object obj) {
            ProjectReference other = obj as ProjectReference;
            if (other != null) {
                return this.Equals(other);
            }
            return false;
        }

        #region IEquatable<ProjectReference> Members

        public virtual bool Equals(ProjectReference other) {
            if (other.Kind != Kind) {
                return false;
            }

            switch (Kind) {
                case ProjectReferenceKind.Assembly:
                case ProjectReferenceKind.ExtensionModule:
                    return String.Equals(other.Name, Name, StringComparison.OrdinalIgnoreCase);
                default:
                    return String.Equals(other.Name, Name, StringComparison.Ordinal);
            }
        }

        #endregion
    }
#endif
}

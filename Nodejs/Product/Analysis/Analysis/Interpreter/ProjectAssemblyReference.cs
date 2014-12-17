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
using System.Reflection;

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    public sealed class ProjectAssemblyReference : ProjectReference, IEquatable<ProjectAssemblyReference> {
        private readonly AssemblyName _asmName;

        public ProjectAssemblyReference(AssemblyName assemblyName, string filename)
            : base(filename, ProjectReferenceKind.Assembly) {
                _asmName = assemblyName;
        }

        public AssemblyName AssemblyName {
            get {
                return _asmName;
            }
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ _asmName.GetHashCode();
        }

        public override bool Equals(object obj) {
            ProjectAssemblyReference asmRef = obj as ProjectAssemblyReference;
            if (asmRef != null) {
                return Equals(asmRef);
            }
            return false;
        }

        public override bool Equals(ProjectReference other) {
            ProjectAssemblyReference asmRef = other as ProjectAssemblyReference;
            if (asmRef != null) {
                return Equals(asmRef);
            }
            return false;
        }

        #region IEquatable<ProjectAssemblyReference> Members

        public bool Equals(ProjectAssemblyReference other) {
            if (base.Equals(other)) {
                return other._asmName == this._asmName;
            }
            return false;
        }

        #endregion
    }
#endif
}

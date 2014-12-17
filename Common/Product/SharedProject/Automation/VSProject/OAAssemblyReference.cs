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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using VSLangProj;

namespace Microsoft.VisualStudioTools.Project.Automation {
    [ComVisible(true)]
    public class OAAssemblyReference : OAReferenceBase {
        internal OAAssemblyReference(AssemblyReferenceNode assemblyReference) :
            base(assemblyReference) {
        }

        internal new AssemblyReferenceNode BaseReferenceNode {
            get {
                return (AssemblyReferenceNode)base.BaseReferenceNode;
            }
        }

        #region Reference override
        public override int BuildNumber {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version)) {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Build;
            }
        }
        public override string Culture {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.CultureInfo)) {
                    return string.Empty;
                }
                return BaseReferenceNode.ResolvedAssembly.CultureInfo.Name;
            }
        }
        public override string Identity {
            get {
                // Note that in this function we use the assembly name instead of the resolved one
                // because the identity of this reference is the assembly name needed by the project,
                // not the specific instance found in this machine / environment.
                if (null == BaseReferenceNode.AssemblyName) {
                    return null;
                }
                // changed from MPFProj, http://mpfproj10.codeplex.com/workitem/11274
                return BaseReferenceNode.AssemblyName.Name;
            }
        }
        public override int MajorVersion {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version)) {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Major;
            }
        }
        public override int MinorVersion {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version)) {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Minor;
            }
        }

        public override string PublicKeyToken {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                (null == BaseReferenceNode.ResolvedAssembly.GetPublicKeyToken())) {
                    return null;
                }
                StringBuilder builder = new StringBuilder();
                byte[] publicKeyToken = BaseReferenceNode.ResolvedAssembly.GetPublicKeyToken();
                for (int i = 0; i < publicKeyToken.Length; i++) {
                    // changed from MPFProj:
                    // http://mpfproj10.codeplex.com/WorkItem/View.aspx?WorkItemId=8257
                    builder.AppendFormat("{0:x2}", publicKeyToken[i]);
                }
                return builder.ToString();
            }
        }

        public override string Name {
            get {
                if (null != BaseReferenceNode.ResolvedAssembly) {
                    return BaseReferenceNode.ResolvedAssembly.Name;
                }
                if (null != BaseReferenceNode.AssemblyName) {
                    return BaseReferenceNode.AssemblyName.Name;
                }
                return null;
            }
        }
        public override int RevisionNumber {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version)) {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Revision;
            }
        }
        public override bool StrongName {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (0 == (BaseReferenceNode.ResolvedAssembly.Flags & AssemblyNameFlags.PublicKey))) {
                    return false;
                }
                return true;
            }
        }
        public override prjReferenceType Type {
            get {
                return prjReferenceType.prjReferenceTypeAssembly;
            }
        }
        public override string Version {
            get {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version)) {
                    return string.Empty;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.ToString();
            }
        }
        #endregion
    }
}

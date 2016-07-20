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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("3C3BD073-2AB3-4E66-BBE9-C8B2D8A774D1")]
    public class NpmNodeProperties : NodeProperties {
        internal NpmNodeProperties(AbstractNpmNode node) : base(node) {}

        private AbstractNpmNode NpmNode {
            get { return Node as AbstractNpmNode; }
        }

        public override string GetClassName() {
            return SR.GetString(SR.PropertiesClassNpm);
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmNodePackageInstallation)]
        [SRDescriptionAttribute(SR.NpmNodePackageInstallationDescription)]
        public string PackageInstallation {
            get {
                return SR.GetString(SR.PackageInstallationLocal);
            }
        }

        [SRCategoryAttribute(SR.General)]
        [SRDisplayName(SR.NpmNodePath)]
        [SRDescriptionAttribute(SR.NpmNodePathDescription)]
        public string Path {
            get {
                var node = NpmNode;
                if (null != node) {
                    var local = node as NodeModulesNode;
                    if (null != local) {
                        var root = local.RootPackage;
                        if (null != root) {
                            return root.Path;
                        }
                    }
                }
                return null;
            }
        }
    }
}

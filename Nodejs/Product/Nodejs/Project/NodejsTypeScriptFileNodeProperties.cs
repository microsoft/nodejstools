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
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.NodejsTools.Project
{
    [ComVisible(true)]
    public class NodejsTypeScriptFileNodeProperties : IncludedFileNodeProperties
    {
        internal NodejsTypeScriptFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }

        [SRCategory(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [ResourcesDescription(nameof(Resources.TestFrameworkDescription))]
        public string TestFramework
        {
            get
            {
                var framework = this.HierarchyNode.ItemNode.GetMetadata(SR.TestFramework);
                if (String.IsNullOrWhiteSpace(framework))
                {
                    return String.Empty;
                }
                return Convert.ToString(framework);
            }
            set
            {
                this.HierarchyNode.ItemNode.SetMetadata(SR.TestFramework, value.ToString());
            }
        }
    }

    [ComVisible(true)]
    public class NodejsTypeScriptLinkFileNodeProperties : LinkFileNodeProperties
    {
        internal NodejsTypeScriptLinkFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }

        [SRCategory(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [ResourcesDescription(nameof(Resources.TestFrameworkDescription))]
        public string TestFramework
        {
            get
            {
                var framework = this.HierarchyNode.ItemNode.GetMetadata(SR.TestFramework);
                if (String.IsNullOrEmpty(framework))
                {
                    return String.Empty;
                }
                return Convert.ToString(framework);
            }
            set
            {
                this.HierarchyNode.ItemNode.SetMetadata(SR.TestFramework, value.ToString());
            }
        }
    }
}

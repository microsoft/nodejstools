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
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    [Guid("62E8E091-6914-498E-A47B-6F198DC1873D")]
    class NodejsGeneralPropertyPage : CommonPropertyPage {
        private readonly NodejsGeneralPropertyPageControl _control;

        public NodejsGeneralPropertyPage() {
            _control = new NodejsGeneralPropertyPageControl(this);
        }

        public override System.Windows.Forms.Control Control {
            get { return _control; }
        }

        internal override CommonProjectNode Project {
            get {
                return base.Project;
            }
            set {
                if (value == null && base.Project != null) {
                    base.Project.PropertyPage = null;
                }
                base.Project = value;
                if (value != null) {
                    ((NodejsProjectNode)value).PropertyPage = this;
                }
            }
        }
        public override void Apply() {
            Project.SetProjectProperty(NodeProjectProperty.NodeExePath, _control.NodeExePath);
            Project.SetProjectProperty(NodeProjectProperty.NodeExeArguments, _control.NodeExeArguments);
            Project.SetProjectProperty(CommonConstants.StartupFile, _control.ScriptFile);
            Project.SetProjectProperty(NodeProjectProperty.ScriptArguments, _control.ScriptArguments);
            Project.SetProjectProperty(NodeProjectProperty.NodejsPort, _control.NodejsPort);
            Project.SetProjectProperty(NodeProjectProperty.StartWebBrowser, _control.StartWebBrowser.ToString());
            Project.SetProjectProperty(CommonConstants.WorkingDirectory, _control.WorkingDirectory);
            Project.SetProjectProperty(NodeProjectProperty.LaunchUrl, _control.LaunchUrl);
            Project.SetProjectProperty(NodeProjectProperty.DebuggerPort, _control.DebuggerPort);
            Project.SetProjectProperty(NodeProjectProperty.Environment, _control.Environment);
            IsDirty = false;
        }

        public override void LoadSettings() {
            Loading = true;
            try {
                _control.NodeExeArguments = Project.GetUnevaluatedProperty(NodeProjectProperty.NodeExeArguments);
                _control.NodeExePath = Project.GetUnevaluatedProperty(NodeProjectProperty.NodeExePath);
                _control.ScriptFile = Project.GetUnevaluatedProperty(CommonConstants.StartupFile);
                _control.ScriptArguments = Project.GetUnevaluatedProperty(NodeProjectProperty.ScriptArguments);
                _control.WorkingDirectory = Project.GetUnevaluatedProperty(CommonConstants.WorkingDirectory);
                _control.LaunchUrl = Project.GetUnevaluatedProperty(NodeProjectProperty.LaunchUrl);
                _control.NodejsPort = Project.GetUnevaluatedProperty(NodeProjectProperty.NodejsPort);
                _control.DebuggerPort = Project.GetUnevaluatedProperty(NodeProjectProperty.DebuggerPort);
                _control.Environment = Project.GetUnevaluatedProperty(NodeProjectProperty.Environment);
                
                // Attempt to parse the boolean.  If we fail, assume it is true.
                bool startWebBrowser;
                if (!Boolean.TryParse(Project.GetUnevaluatedProperty(NodeProjectProperty.StartWebBrowser), out startWebBrowser)) {
                    startWebBrowser = true;
                }
                _control.StartWebBrowser = startWebBrowser;
            } finally {
                Loading = false;
            }
        }

        public override string Name {
            get { return "General"; }
        }
    }
}

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
using System.ComponentModel.Design;
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
            Project.SetProjectProperty(NodejsConstants.NodeExePath, _control.NodeExePath);
            Project.SetProjectProperty(NodejsConstants.NodeExeArguments, _control.NodeExeArguments);
            Project.SetProjectProperty(CommonConstants.StartupFile, _control.ScriptFile);
            Project.SetProjectProperty(NodejsConstants.ScriptArguments, _control.ScriptArguments);
            Project.SetProjectProperty(NodejsConstants.NodejsPort, _control.NodejsPort);
            Project.SetProjectProperty(NodejsConstants.StartWebBrowser, _control.StartWebBrowser.ToString());
            Project.SetProjectProperty(CommonConstants.WorkingDirectory, _control.WorkingDirectory);
            Project.SetProjectProperty(NodejsConstants.LaunchUrl, _control.LaunchUrl);
            Project.SetProjectProperty(NodejsConstants.DebuggerPort, _control.DebuggerPort);
            Project.SetProjectProperty(NodejsConstants.Environment, _control.Environment);
			Project.SetProjectProperty(NodejsConstants.OverrideDefaultBrowser, _control.OverrideDefaultBrowser.ToString());
			Project.SetProjectProperty(NodejsConstants.BrowserExecutable, _control.BrowserExecutable);
			Project.SetProjectProperty(NodejsConstants.BrowserArguments, _control.BrowserArguments);
			IsDirty = false;
        }

        public override void LoadSettings() {
            Loading = true;
            try {
                _control.NodeExeArguments = Project.GetUnevaluatedProperty(NodejsConstants.NodeExeArguments);
                _control.NodeExePath = Project.GetUnevaluatedProperty(NodejsConstants.NodeExePath);
                _control.ScriptFile = Project.GetUnevaluatedProperty(CommonConstants.StartupFile);
                _control.ScriptArguments = Project.GetUnevaluatedProperty(NodejsConstants.ScriptArguments);
                _control.WorkingDirectory = Project.GetUnevaluatedProperty(CommonConstants.WorkingDirectory);
                _control.LaunchUrl = Project.GetUnevaluatedProperty(NodejsConstants.LaunchUrl);
                _control.NodejsPort = Project.GetUnevaluatedProperty(NodejsConstants.NodejsPort);
                _control.DebuggerPort = Project.GetUnevaluatedProperty(NodejsConstants.DebuggerPort);
                _control.Environment = Project.GetUnevaluatedProperty(NodejsConstants.Environment);
				_control.BrowserExecutable = Project.GetUnevaluatedProperty(NodejsConstants.BrowserExecutable);
				_control.BrowserArguments = Project.GetUnevaluatedProperty(NodejsConstants.BrowserArguments);
                
                // Attempt to parse the boolean.  If we fail, assume it is true.
                bool startWebBrowser;
                if (!Boolean.TryParse(Project.GetUnevaluatedProperty(NodejsConstants.StartWebBrowser), out startWebBrowser)) {
                    startWebBrowser = true;
                }
                _control.StartWebBrowser = startWebBrowser;

				bool overrideDefaultBrowser;
				if (!Boolean.TryParse(Project.GetUnevaluatedProperty(NodejsConstants.OverrideDefaultBrowser), out overrideDefaultBrowser)) {
					overrideDefaultBrowser = false;
                }
				_control.OverrideDefaultBrowser = overrideDefaultBrowser;
            } finally {
                Loading = false;
            }
        }

        public override string Name {
            get { return "General"; }
        }
    }
}

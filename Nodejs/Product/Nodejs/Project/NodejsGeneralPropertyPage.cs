/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

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
            Project.SetProjectProperty(NodejsConstants.ScriptArguments, _control.ScriptArguments);
            Project.SetProjectProperty(NodejsConstants.NodejsPort, _control.NodejsPort);
            Project.SetProjectProperty(NodejsConstants.StartWebBrowser, _control.StartWebBrowser.ToString());
            Project.SetProjectProperty(CommonConstants.WorkingDirectory, _control.WorkingDirectory);
            Project.SetProjectProperty(NodejsConstants.LaunchUrl, _control.LaunchUrl);
            Project.SetProjectProperty(NodejsConstants.DebuggerPort, _control.DebuggerPort);
            Project.SetProjectProperty(NodejsConstants.Environment, _control.Environment);
            IsDirty = false;
        }

        public override void LoadSettings() {
            Loading = true;
            try {
                _control.NodeExeArguments = Project.GetUnevaluatedProperty(NodejsConstants.NodeExeArguments);
                _control.NodeExePath = Project.GetUnevaluatedProperty(NodejsConstants.NodeExePath);
                _control.ScriptArguments = Project.GetUnevaluatedProperty(NodejsConstants.ScriptArguments);
                _control.WorkingDirectory = Project.GetUnevaluatedProperty(CommonConstants.WorkingDirectory);
                _control.LaunchUrl = Project.GetUnevaluatedProperty(NodejsConstants.LaunchUrl);
                _control.NodejsPort = Project.GetUnevaluatedProperty(NodejsConstants.NodejsPort);
                _control.DebuggerPort = Project.GetUnevaluatedProperty(NodejsConstants.DebuggerPort);
                _control.Environment = Project.GetUnevaluatedProperty(NodejsConstants.Environment);
                
                // Attempt to parse the boolean.  If we fail, assume it is true.
                bool startWebBrowser;
                if (!Boolean.TryParse(Project.GetUnevaluatedProperty(NodejsConstants.StartWebBrowser), out startWebBrowser)) {
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

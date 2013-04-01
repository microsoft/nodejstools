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

        public override void Apply() {
            Project.SetProjectProperty(NodeConstants.NodeExeLocation, _control.NodeExeLocation);
            Project.SetProjectProperty(NodeConstants.NodeExeArguments, _control.NodeExeArguments);
            Project.SetProjectProperty(NodeConstants.ScriptArguments, _control.ScriptArguments);
            Project.SetProjectProperty(NodeConstants.NodejsPort, _control.NodejsPort);
            Project.SetProjectProperty(NodeConstants.StartWebBrowser, _control.StartWebBrowser.ToString());
            Project.SetProjectProperty(CommonConstants.WorkingDirectory, _control.WorkingDirectory);
            Project.SetProjectProperty(NodeConstants.LaunchUrl, _control.LaunchUrl);
            IsDirty = false;
        }

        public override void LoadSettings() {
            Loading = true;
            try {
                _control.NodeExeArguments = Project.GetProjectProperty(NodeConstants.NodeExeArguments);
                _control.NodeExeLocation = Project.GetProjectProperty(NodeConstants.NodeExeLocation);
                _control.ScriptArguments = Project.GetProjectProperty(NodeConstants.ScriptArguments);
                _control.WorkingDirectory = Project.GetProjectProperty(CommonConstants.WorkingDirectory);
                _control.LaunchUrl = Project.GetProjectProperty(NodeConstants.LaunchUrl);
                _control.NodejsPort = Project.GetProjectProperty(NodeConstants.NodejsPort);
                bool startWebBrowser;
                if (!Boolean.TryParse(Project.GetProjectProperty(NodeConstants.StartWebBrowser), out startWebBrowser)) {
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

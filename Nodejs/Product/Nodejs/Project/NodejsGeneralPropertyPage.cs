// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    [Guid("62E8E091-6914-498E-A47B-6F198DC1873D")]
    internal class NodejsGeneralPropertyPage : CommonPropertyPage
    {
        private readonly NodejsGeneralPropertyPageControl control;

        public NodejsGeneralPropertyPage()
        {
            this.control = new NodejsGeneralPropertyPageControl(this);
        }

        protected override Control CreateControl()
        {
            return this.control;
        }

        public override Control Control => this.control;

        protected override Type ControlType => typeof(NodejsGeneralPropertyPageControl);

        internal override CommonProjectNode Project
        {
            get
            {
                return base.Project;
            }
            set
            {
                if (value == null && base.Project != null)
                {
                    base.Project.PropertyPage = null;
                }
                base.Project = value;
                if (value != null)
                {
                    ((NodejsProjectNode)value).PropertyPage = this;
                }
            }
        }

        protected override void Apply()
        {
            this.Project.SetProjectProperty(NodeProjectProperty.NodeExePath, this.control.NodeExePath);
            this.Project.SetProjectProperty(NodeProjectProperty.NodeExeArguments, this.control.NodeExeArguments);
            this.Project.SetProjectProperty(CommonConstants.StartupFile, this.control.ScriptFile);
            this.Project.SetProjectProperty(NodeProjectProperty.ScriptArguments, this.control.ScriptArguments);
            this.Project.SetProjectProperty(NodeProjectProperty.NodejsPort, this.control.NodejsPort);
            this.Project.SetProjectProperty(NodeProjectProperty.StartWebBrowser, this.control.StartWebBrowser.ToString());
            this.Project.SetProjectProperty(CommonConstants.WorkingDirectory, this.control.WorkingDirectory);
            this.Project.SetProjectProperty(NodeProjectProperty.LaunchUrl, this.control.LaunchUrl);
            this.Project.SetProjectProperty(NodeProjectProperty.DebuggerPort, this.control.DebuggerPort);
            this.Project.SetProjectProperty(NodeProjectProperty.Environment, this.control.Environment);
            this.control.IsDirty = false;
        }

        public override void LoadSettings()
        {
            this.control.NodeExeArguments = this.Project.GetUnevaluatedProperty(NodeProjectProperty.NodeExeArguments);
            this.control.NodeExePath = this.Project.GetUnevaluatedProperty(NodeProjectProperty.NodeExePath);
            this.control.ScriptFile = this.Project.GetUnevaluatedProperty(CommonConstants.StartupFile);
            this.control.ScriptArguments = this.Project.GetUnevaluatedProperty(NodeProjectProperty.ScriptArguments);
            this.control.WorkingDirectory = this.Project.GetUnevaluatedProperty(CommonConstants.WorkingDirectory);
            this.control.LaunchUrl = this.Project.GetUnevaluatedProperty(NodeProjectProperty.LaunchUrl);
            this.control.NodejsPort = this.Project.GetUnevaluatedProperty(NodeProjectProperty.NodejsPort);
            this.control.DebuggerPort = this.Project.GetUnevaluatedProperty(NodeProjectProperty.DebuggerPort);
            this.control.Environment = this.Project.GetUnevaluatedProperty(NodeProjectProperty.Environment);

            // Attempt to parse the boolean.  If we fail, assume it is true.
            if (!bool.TryParse(this.Project.GetUnevaluatedProperty(NodeProjectProperty.StartWebBrowser), out var startWebBrowser))
            {
                startWebBrowser = true;
            }
            this.control.StartWebBrowser = startWebBrowser;
        }

        protected override string Title => "General";
    }
}

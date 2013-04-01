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
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Project {
    partial class NodejsGeneralPropertyPageControl : UserControl {
        private readonly NodejsGeneralPropertyPage _propPage;

        public NodejsGeneralPropertyPageControl() {
            InitializeComponent();
        }

        public NodejsGeneralPropertyPageControl(NodejsGeneralPropertyPage page) {
            _propPage = page;
            InitializeComponent();
        }

        public string NodeExeLocation {
            get {
                return _nodeExeLocation.Text;
            }
            set {
                _nodeExeLocation.Text = value;
            }
        }

        public string NodeExeArguments {
            get{
                return _nodeExeArguments.Text;
            }
            set{
                _nodeExeArguments.Text = value;
            }
        }

        public string ScriptArguments {
            get {
                return _scriptArguments.Text;
            }
            set {
                _scriptArguments.Text = value;
            }
        }

        public string NodejsPort {
            get {
                return _nodejsPort.Text;
            }
            set {
                _nodejsPort.Text = value;
            }
        }

        public bool StartWebBrowser {
            get {
                return _startBrowser.Checked;
            }
            set {
                _startBrowser.Checked = value;
            }
        }


        public string WorkingDirectory {
            get {
                return _workingDir.Text;
            }
            set {
                _workingDir.Text = value;
            }
        }

        public string LaunchUrl {
            get {
                return _launchUrl.Text;
            }
            set {
                _launchUrl.Text = value;
            }
        }

        private void Changed(object sender, EventArgs e) {
            _propPage.IsDirty = true;
        }
    }
}

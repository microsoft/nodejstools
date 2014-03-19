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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    class NodejsTypeScriptFileNode : CommonFileNode {
        internal int _fileId;

        public NodejsTypeScriptFileNode(NodejsProjectNode root, ProjectElement e)
            : base(root, e) {
            _fileId = root._currentFileCounter++;
        }

        protected override NodeProperties CreatePropertiesObject() {
            if (IsLinkFile) {
                return new NodejsTypeScriptLinkFileNodeProperties(this);
            } else if (IsNonMemberItem) {
                return new ExcludedFileNodeProperties(this);
            }

            return new NodejsTypeScriptFileNodeProperties(this);
        }

        internal string MangledModuleFunctionName {
            get {
                return NodejsConstants.NodejsHiddenUserModule + _fileId;
            }
        }

        public new NodejsProjectNode ProjectMgr {
            get {
                return (NodejsProjectNode)base.ProjectMgr;
            }
        }
    }
}

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
using System.Windows.Forms;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands {
    internal sealed class OpenRemoteDebugProxyFolderCommand : Command {
        public override void DoCommand(object sender, EventArgs args) {
            // Open explorer to folder
            if (!File.Exists(NodejsPackage.RemoteDebugProxyFolder)) {
                MessageBox.Show(String.Format("Remote Debug Proxy \"{0}\" does not exist.", NodejsPackage.RemoteDebugProxyFolder), "Node.js Tools for Visual Studio");
            } else {
                Process.Start("explorer", string.Format("/e,/select,{0}", NodejsPackage.RemoteDebugProxyFolder));
            }
        }

        public override int CommandId {
            get { return (int)PkgCmdId.cmdidOpenRemoteDebugProxyFolder; }
        }
    }
}

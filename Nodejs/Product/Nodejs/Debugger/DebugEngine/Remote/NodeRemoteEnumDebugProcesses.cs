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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.Debugger.Remote {
    internal class NodeRemoteEnumDebugProcesses : NodeRemoteEnumDebug<IDebugProcess2>, IEnumDebugProcesses2 {
        public NodeRemoteEnumDebugProcesses(NodeRemoteDebugPort port)
            : base(Connect(port)) {
        }

        public NodeRemoteEnumDebugProcesses(NodeRemoteEnumDebugProcesses processes)
            : base(processes.Element) {
        }

        public int Clone(out IEnumDebugProcesses2 ppEnum) {
            ppEnum = new NodeRemoteEnumDebugProcesses(this);
            return VSConstants.S_OK;
        }

        // Connect to the remote debugging server. If any errors occur, display an error dialog, and keep
        // trying for as long as user clicks "Retry".
        private static NodeRemoteDebugProcess Connect(NodeRemoteDebugPort port) {
            NodeRemoteDebugProcess process = null;
            while (true) {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                    try {
                        socket.NoDelay = true;
                        socket.Connect(new DnsEndPoint(port.HostName, port.PortNumber));
                        // https://nodejstools.codeplex.com/workitem/578
                        // Read "welcome" headers from node debug socket before disconnecting to workaround issue
                        // where connect and immediate disconnect leaves node.js (V8) in a bad state which blocks attach.
                        try {
                            byte[] socketBuffer = new byte[1024];
                            IAsyncResult asyncResult =
                                socket.BeginReceive(
                                    socketBuffer,
                                    0,
                                    1024,
                                    SocketFlags.None,
                                    null,
                                    null
                            );
                            if (asyncResult.AsyncWaitHandle.WaitOne(1000)) {
                                var bytesReceived = socket.EndReceive(asyncResult);
                                var str = Encoding.UTF8.GetString(socketBuffer.Substring(0, bytesReceived)).Trim();
                                int pid = 1;
                                string exe = "node.exe";
                                string username = string.Empty;
                                string version = string.Empty;
                                process = new NodeRemoteDebugProcess(port, pid, exe, username, version);
                                break;
                            }

                        } finally {
                            socket.Disconnect(false);
                        }
                    }
                    catch (IOException) {
                    }
                    catch (SocketException) {
                    }
                }
                string errText =
                    string.Format(
                        "Could not attach to Node.js process at '{0}:{1}'. " +
                        "Make sure that the process is running behind the remote debug proxy (RemoteDebug.js), " +
                        "and that the debuger port (default 5858) has been opened on the target host.",
                        port.HostName,
                        port.PortNumber
                    );
                DialogResult dlgRes = MessageBox.Show(errText, null, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dlgRes != DialogResult.Retry) {
                    break;
                }
            }

            return process;
        }
    }
}

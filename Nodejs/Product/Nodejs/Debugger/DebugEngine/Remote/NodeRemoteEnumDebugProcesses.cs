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
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Windows.Forms;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.Remote {
    internal class NodeRemoteEnumDebugProcesses : NodeRemoteEnumDebug<IDebugProcess2>, IEnumDebugProcesses2 {
        private class DebuggerAlreadyAttachedException : Exception {
            public DebuggerAlreadyAttachedException()
                : base("A debugger is already attached to this node.js process.") {
            }
        }

        public NodeRemoteEnumDebugProcesses(NodeRemoteDebugPort port, INetworkClientFactory networkClientFactory)
            : base(Connect(port, networkClientFactory)) {
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
        private static NodeRemoteDebugProcess Connect(NodeRemoteDebugPort port, INetworkClientFactory networkClientFactory) {
            NodeRemoteDebugProcess process = null;
            while (true) {
                Exception exception = null;
                try {
                    Debug.WriteLine("NodeRemoteEnumDebugProcesses pinging remote host ...");
                    using (var client = networkClientFactory.CreateNetworkClient(port.Uri))
                    using (var stream = client.GetStream()) {
                        // https://nodejstools.codeplex.com/workitem/578
                        // Read "welcome" headers from node debug socket before disconnecting to workaround issue
                        // where connect and immediate disconnect leaves node.js (V8) in a bad state which blocks attach.
                        var buffer = new byte[1024];
                        var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                        if (readTask.Wait(5000)) {
                            string response = Encoding.UTF8.GetString(buffer, 0, readTask.Result);
                            Debug.WriteLine("NodeRemoteEnumDebugProcesses debugger response: " + response);

                            if (response == "Remote debugging session already active\r\n") {
                                throw new DebuggerAlreadyAttachedException();
                            }

                            process = new NodeRemoteDebugProcess(port, "node.exe", "", "");
                            Debug.WriteLine("NodeRemoteEnumDebugProcesses ping successful.");
                            break;
                        } else {
                            Debug.WriteLine("NodeRemoteEnumDebugProcesses ping timed out.");
                        }
                    }
                } catch (DebuggerAlreadyAttachedException ex) {
                    exception = ex;
                } catch (AggregateException ex) {
                    exception = ex;
                } catch (IOException ex) {
                    exception = ex;
                } catch (SocketException ex) {
                    exception = ex;
                } catch (WebSocketException ex) {
                    exception = ex;
                } catch (PlatformNotSupportedException) {
                    MessageBox.Show(
                        "Remote debugging of node.js Windows Azure applications is only supported on Windows 8 and above.",
                        null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                if (exception != null) {
                    while (exception.InnerException != null) {
                        exception = exception.InnerException;
                    }
                }

                string errText = string.Format(
                    "Could not attach to Node.js process at {0}{1}\r\n\r\n",
                    port.Uri,
                    exception != null ? ":\r\n\r\n" + exception.Message : ".");
                if (!(exception is DebuggerAlreadyAttachedException)) {
                    if (port.Uri.Scheme == "ws" || port.Uri.Scheme == "wss") {
                        errText +=
                            "Make sure that the Azure web site is deployed in the Debug configuration, and web sockets " +
                            "are enabled for it in the Azure management portal.";
                    } else {
                        errText += string.Format(
                            "Make sure that the process is running behind the remote debug proxy (RemoteDebug.js), " +
                            "and the debugger port (default {0}) is open on the target host.",
                            NodejsConstants.DefaultDebuggerPort);
                    }
                }

                DialogResult dlgRes = MessageBox.Show(errText, null, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dlgRes != DialogResult.Retry) {
                    break;
                }
            }

            return process;
        }
    }
}

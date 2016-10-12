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
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.NodejsTools.Logging;
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
            if (port.Uri.Fragment == "#ping=0") {
                return new NodeRemoteDebugProcess(port, "node.exe", String.Empty, String.Empty);
            }

            NodeRemoteDebugProcess process = null;
            while (true) {
                Exception exception = null;
                try {
                    LiveLogger.WriteLine("NodeRemoteEnumDebugProcesses pinging remote host ...");
                    using (var client = networkClientFactory.CreateNetworkClient(port.Uri))
                    using (var stream = client.GetStream()) {
                        // https://nodejstools.codeplex.com/workitem/578

                        // Node.js (V8) debugger is fragile during attach, and it's easy to put it into a bad state where it refuses
                        // future connections altogether, or accepts them but send responses that it queued up for another client.
                        // To avoid this, our ping needs to look like a proper debug session to the debuggee. For this, we need to
                        // do the following steps in order:
                        //
                        // 1. Receive the debugger's greeting message.
                        // 2. Send the "disconnect" request.
                        // 3. Receive the "disconnect" response.
                        //
                        // Only then can the socket be closed safely without disrupting V8.

                        // Receive greeting.
                        var buffer = new byte[1024];
                        int len = stream.ReadAsync(buffer, 0, buffer.Length, new CancellationTokenSource(5000).Token).GetAwaiter().GetResult();
                        string response = Encoding.UTF8.GetString(buffer, 0, len);
                        LiveLogger.WriteLine("NodeRemoteEnumDebugProcesses debugger greeting: " + response);

                        // There's no error code, so we have to do the string comparison. Luckily, it is hardcoded into V8 and is not localized.
                        if (response == "Remote debugging session already active\r\n") {
                            throw new DebuggerAlreadyAttachedException();
                        }

                        // Send "disconnect" request.
                        string request = @"{""command"":""disconnect"",""seq"":1,""type"":""request"",""arguments"":null}";
                        request = string.Format(CultureInfo.InvariantCulture, "Content-Length: {0}\r\n\r\n{1}", request.Length, request);
                        buffer = Encoding.UTF8.GetBytes(request);
                        stream.WriteAsync(buffer, 0, buffer.Length, new CancellationTokenSource(5000).Token).GetAwaiter().GetResult();

                        // Receive "disconnect" response.
                        buffer = new byte[1024];
                        len = stream.ReadAsync(buffer, 0, buffer.Length, new CancellationTokenSource(5000).Token).GetAwaiter().GetResult();
                        response = Encoding.UTF8.GetString(buffer, 0, len);
                        LiveLogger.WriteLine("NodeRemoteEnumDebugProcesses debugger response: " + response);

                        // If we got to this point, the debuggee is behaving as expected, and we can report it as a valid Node.js process.
                        process = new NodeRemoteDebugProcess(port, "node.exe", String.Empty, String.Empty);
                        LiveLogger.WriteLine("NodeRemoteEnumDebugProcesses ping successful.");
                        break;
                    }
                } catch (OperationCanceledException) {
                    LiveLogger.WriteLine("NodeRemoteEnumDebugProcesses ping timed out.");
                } catch (DebuggerAlreadyAttachedException ex) {
                    LiveLogger.WriteLine("DebuggerAlreadyAttachedException connecting to remote debugger");
                    exception = ex;
                } catch (AggregateException ex) {
                    LiveLogger.WriteLine("AggregateException connecting to remote debugger");
                    exception = ex;
                } catch (IOException ex) {
                    LiveLogger.WriteLine("IOException connecting to remote debugger");
                    exception = ex;
                } catch (InvalidOperationException ex) {
                    LiveLogger.WriteLine("InvalidOperationException connecting to remote debugger");
                    exception = ex;
                } catch (SocketException ex) {
                    LiveLogger.WriteLine("SocketException connecting to remote debugger");
                    exception = ex;
                } catch (WebSocketException ex) {
                    LiveLogger.WriteLine("WebSocketException connecting to remote debugger");
                    exception = ex;
                } catch (PlatformNotSupportedException) {
                    LiveLogger.WriteLine("PlatformNotSupportedException connecting to remote debugger");
                    MessageBox.Show(
                        "Remote debugging of node.js Microsoft Azure applications is only supported on Windows 8 and above.",
                        null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                if (exception != null) {
                    while (exception.InnerException != null) {
                        exception = exception.InnerException;
                    }
                }

                string errText = string.Format(CultureInfo.CurrentCulture,
                    "Could not attach to Node.js process at {0}{1}\r\n\r\n",
                    port.Uri,
                    exception != null ? ":\r\n\r\n" + exception.Message : ".");
                if (!(exception is DebuggerAlreadyAttachedException)) {
                    if (port.Uri.Scheme == "ws" || port.Uri.Scheme == "wss") {
                        errText +=
                            "Make sure that the Azure web site is deployed in the Debug configuration, and web sockets " +
                            "are enabled for it in the Azure management portal.";
                    } else {
                        errText += string.Format(CultureInfo.CurrentCulture,
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

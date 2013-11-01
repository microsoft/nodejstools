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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Ajax.Utilities;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Repl {
    [ReplRole("Reset"), ReplRole("Execution")]
    sealed class NodejsReplEvaluator : IReplEvaluator {
        private ListenerThread _listener;
        private IReplWindow _window;
        private readonly INodejsReplSite _site;

        public NodejsReplEvaluator()
            : this(VsNodejsReplSite.Site) {
        }

        public NodejsReplEvaluator(INodejsReplSite site) {
            _site = site;
        }

        #region IReplEvaluator Members

        public Task<ExecutionResult> Initialize(IReplWindow window) {
            _window = window;
            _window.SetOptionValue(ReplOptions.CommandPrefix, ".");
            _window.SetOptionValue(ReplOptions.PrimaryPrompt, "> ");
            _window.SetOptionValue(ReplOptions.SecondaryPrompt, ". ");
            _window.SetOptionValue(ReplOptions.DisplayPromptInMargin, false);
            _window.SetOptionValue(ReplOptions.SupportAnsiColors, true);
            _window.SetOptionValue(ReplOptions.UseSmartUpDown, true);
            return ExecutionResult.Succeeded;
        }

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer) {
        }

        public Task<ExecutionResult> Reset() {
            Connect();
            return ExecutionResult.Succeeded;
        }

        public bool CanExecuteText(string text) {
            var parser = new JSParser(text);
            var errorSink = new ErrorSink(text);
            parser.CompilerError += errorSink.CompilerError;
            parser.Parse(new CodeSettings());

            return !errorSink.Unterminated;
        }

        class ErrorSink {
            public bool Unterminated;
            public readonly string Text;

            public ErrorSink(string text) {
                Text = text;
            }

            public void CompilerError(object sender, JScriptExceptionEventArgs e) {
                
                switch(e.Exception.ErrorCode) {
                    case JSError.NoCatch:
                    case JSError.UnclosedFunction:
                    case JSError.NoCommentEnd:
                    case JSError.NoEndDebugDirective:
                    case JSError.NoEndIfDirective:
                    case JSError.NoLabel:
                    case JSError.NoLeftCurly:
                    case JSError.NoMemberIdentifier:
                    case JSError.NoRightBracket:
                    case JSError.NoRightParenthesis:
                    case JSError.NoRightParenthesisOrComma:
                    case JSError.NoRightCurly:
                    case JSError.NoEqual:
                    case JSError.NoCommaOrTypeDefinitionError:
                    case JSError.NoComma:
                    case JSError.ErrorEndOfFile:
                        Unterminated = true;
                        break;
                    default:
                        if (e.Exception.Context.StartPosition == Text.Length) {
                            // EOF error
                            Unterminated = true;
                        }
                        break;
                }
            }

        }

        public Task<ExecutionResult> ExecuteText(string text) {
            EnsureConnected();
            if (_listener == null) {
                return ExecutionResult.Failed;
            }

            return _listener.ExecuteText(text);
        }

        public void ExecuteFile(string filename) {
            throw new NotImplementedException();
        }

        public string FormatClipboard() {
            return Clipboard.GetText();
        }

        public void AbortCommand() {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            if (_listener != null) {
                _listener.Dispose();
            }
        }

        #endregion

        private void EnsureConnected() {
            if (_listener == null) {
                Connect();
            }
        }

        private void Connect() {
            if (_listener != null) {
                _listener.Disconnect();
                _listener.Dispose();
                _listener = null;
            }

            string nodeExePath = GetNodeExePath();
            if (String.IsNullOrWhiteSpace(nodeExePath)) {
                _window.WriteError(Resources.NodejsNotInstalled);
                _window.WriteError(Environment.NewLine);
                return;
            } else if (!File.Exists(nodeExePath)) {
                _window.WriteError(String.Format(Resources.NodeExeDoesntExist, nodeExePath));
                _window.WriteError(Environment.NewLine);
                return;
            }

            Socket socket;
            int port;
            CreateConnection(out socket, out port);

            var scriptPath = "\"" +
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "visualstudio_nodejs_repl.js"
                    ) + "\"";

            var psi = new ProcessStartInfo(nodeExePath, scriptPath + " " + port);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
                

            string fileName, directory = null;

            if (_site.TryGetStartupFileAndDirectory(out fileName, out directory)) {
                psi.WorkingDirectory = directory;
                psi.EnvironmentVariables["NODE_PATH"] = directory;
            }

            var process = new Process();
            process.StartInfo = psi;
            try {
                process.Start();
            } catch (Exception e) {
                _window.WriteError(String.Format("Failed to start interactive process: {0}{1}{2}", Environment.NewLine, e.ToString(), Environment.NewLine));
                return;
            }

            _listener = new ListenerThread(this, process, socket);
        }

        private string GetNodeExePath() {
            var startupProject = _site.GetStartupProject();
            string nodeExePath;
            if (startupProject != null) {
                nodeExePath = startupProject.GetProjectProperty(NodejsConstants.NodeExePath);
                if (String.IsNullOrWhiteSpace(nodeExePath)) {
                    nodeExePath = Nodejs.NodeExePath;
                }
            } else {
                nodeExePath = Nodejs.NodeExePath;
            }
            return nodeExePath;
        }


        private static void CreateConnection(out Socket conn, out int portNum) {
            conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            conn.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            conn.Listen(0);
            portNum = ((IPEndPoint)conn.LocalEndPoint).Port;
        }

        class ListenerThread : JsonListener, IDisposable {
            private readonly NodejsReplEvaluator _eval;
            private readonly Process _process;
            private readonly object _socketLock = new object();
            private Socket _acceptSocket;
            internal bool _connected;
            private TaskCompletionSource<ExecutionResult> _completion;
            private string _executionText;
            private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
            private bool _disposed;
#if DEBUG
            private Thread _socketLockedThread;
#endif
            static string _noReplProcess = "Current interactive window is disconnected - please reset the process." + Environment.NewLine;

            public ListenerThread(NodejsReplEvaluator eval, Process process, Socket socket) {
                _eval = eval;
                _process = process;
                _acceptSocket = socket;

                _acceptSocket.BeginAccept(SocketConnectionAccepted, null);

                _process.OutputDataReceived += new DataReceivedEventHandler(StdOutReceived);
                _process.ErrorDataReceived += new DataReceivedEventHandler(StdErrReceived);
                _process.EnableRaisingEvents = true;
                _process.Exited += ProcessExited;

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            private void StdOutReceived(object sender, DataReceivedEventArgs args) {
                if (args.Data != null) {
                    _eval._window.WriteOutput(args.Data + Environment.NewLine);
                }
            }

            private void StdErrReceived(object sender, DataReceivedEventArgs args) {
                if (args.Data != null) {
                    _eval._window.WriteError(args.Data + Environment.NewLine);
                }
            }

            private void ProcessExited(object sender, EventArgs args) {
                ProcessExitedWorker();
            }

            private void ProcessExitedWorker() {
                _eval._window.WriteError("The process has exited");
                using (new SocketLock(this)) {
                    if (_completion != null) {
                        _completion.SetResult(ExecutionResult.Failure);
                    }
                    _completion = null;
                }            
            }

            private void SocketConnectionAccepted(IAsyncResult result) {
                Socket = _acceptSocket.EndAccept(result);
                _acceptSocket.Close();

                using (new SocketLock(this)) {
                    _connected = true;
                }

                using (new SocketLock(this)) {
                    if (_executionText != null) {
                        Debug.WriteLine("Executing delayed text: " + _executionText);
                        SendExecuteText(_executionText);
                        _executionText = null;
                    }
                }

                StartListenerThread();
            }

            public Task<ExecutionResult> ExecuteText(string text) {
                TaskCompletionSource<ExecutionResult> completion;
                Debug.WriteLine("Executing text: " + text);
                using (new SocketLock(this)) {
                    if (!_connected) {
                        // delay executing the text until we're connected
                        Debug.WriteLine("Delayed executing text");
                        _completion = completion = new TaskCompletionSource<ExecutionResult>();
                        _executionText = text;
                        return completion.Task;
                    }

                    try {
                        if (!Socket.Connected) {
                            _eval._window.WriteError(_noReplProcess);
                            return ExecutionResult.Failed;
                        }

                        _completion = completion = new TaskCompletionSource<ExecutionResult>();
                        
                        SendExecuteText(text);
                    } catch (SocketException) {
                        _eval._window.WriteError(_noReplProcess);
                        return ExecutionResult.Failed;
                    }

                    return completion.Task;
                }
            }

            [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
            static extern bool AllowSetForegroundWindow(int dwProcessId);

            private void SendExecuteText(string text) {
                AllowSetForegroundWindow(_process.Id);
                var request = new Dictionary<string, object>() {
                    { "type", "execute" },
                    { "code", text },
                };

                SendRequest(request);
            }

            internal void SendRequest(Dictionary<string, object> request) {
                string json = _serializer.Serialize(request);

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                var length = "Content-length: " + bytes.Length + "\r\n\r\n";
                var lengthBytes = System.Text.Encoding.UTF8.GetBytes(length);
                Socket.Send(lengthBytes);
                Socket.Send(bytes);
            }

            protected override void OnSocketDisconnected() {
            }

            protected override void ProcessPacket(JsonResponse response) {
                var cmd = _serializer.Deserialize<Dictionary<string, object>>(response.Body);

                object type;
                if (cmd.TryGetValue("type", out type) && type is string) {
                    switch ((string)type) {
                        case "execute":
                            object result;
                            if (cmd.TryGetValue("result", out result)) {
                                _eval._window.WriteLine(result.ToString());
                                _completion.SetResult(ExecutionResult.Success);
                            } else if (cmd.TryGetValue("error", out result)) {
                                _eval._window.WriteError(result.ToString());
                                _completion.SetResult(ExecutionResult.Failure);
                            }
                            _completion = null;
                            break;
                        case "output":
                            if (cmd.TryGetValue("output", out result)) {
                                _eval._window.WriteOutput(FixOutput(result));
                            }
                            break;
                        case "output_error":
                            if (cmd.TryGetValue("output", out result)) {
                                _eval._window.WriteError(FixOutput(result));
                            }
                            break;
#if DEBUG
                        default:
                            Debug.WriteLine(String.Format("Unknown command: {0}", response.Body));
                            break;
#endif
                    }
                }
            }

            private static string FixOutput(object result) {
                var res = result.ToString();
                if (res.IndexOf('\n') != -1) {
                    StringBuilder fixedStr = new StringBuilder();
                    for (int i = 0; i < res.Length; i++) {
                        if (res[i] == '\r') {
                            if (i + 1 < res.Length && res[i + 1] == '\n') {
                                i++;
                                fixedStr.Append("\r\n");
                            } else {
                                fixedStr.Append("\r\n");
                            }
                        } else if (res[i] == '\n') {
                            fixedStr.Append("\r\n");
                        } else {
                            fixedStr.Append(res[i]);
                        }
                    }
                    res = fixedStr.ToString();
                }
                return res;
            }


            internal void Disconnect() {
                if (_completion != null) {
                    _completion.SetResult(ExecutionResult.Failure);
                    _completion = null;
                }
            }

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);                
            }

            protected virtual void Dispose(bool disposing) {
                if (!_disposed) {
                    if (_process != null && !_process.HasExited) {
                        try {
                            //Disconnect our event since we are forceably killing the process off
                            //  We'll synchronously send the message to the user
                            _process.Exited -= ProcessExited;
                            _process.Kill();
                        } catch (InvalidOperationException) {
                        } catch (NotSupportedException) {
                        } catch (System.ComponentModel.Win32Exception) {
                        }
                        ProcessExitedWorker();
                    }
                    
                    if(_process != null) {
                        _process.Dispose();                    
                    }                    
                    _disposed = true;
                }
            }


            /// <summary>
            /// Helper struct for locking and tracking the current holding thread.  This allows
            /// us to assert that our socket is always accessed while the lock is held.  The lock
            /// needs to be held so that requests from the UI (switching scopes, getting module lists,
            /// executing text, etc...) won't become interleaved with interactions from the repl process 
            /// (output, execution completing, etc...).
            /// </summary>
            #region SocketLock

            struct SocketLock : IDisposable {
                private readonly ListenerThread _evaluator;

                public SocketLock(ListenerThread evaluator) {
                    Monitor.Enter(evaluator._socketLock);
#if DEBUG
                    Debug.Assert(evaluator._socketLockedThread == null);
                    evaluator._socketLockedThread = Thread.CurrentThread;
#endif
                    _evaluator = evaluator;
                }

                public void Dispose() {
#if DEBUG
                    _evaluator._socketLockedThread = null;
#endif
                    Monitor.Exit(_evaluator._socketLock);
                }
            }
            #endregion
        }

        internal void Clear() {
            _listener.SendRequest(new Dictionary<string,object>() { { "type", "clear" }});
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Repl
{
    [ReplRole("Reset"), ReplRole("Execution")]
    internal sealed class NodejsReplEvaluator : IReplEvaluator
    {
        private ListenerThread _listener;
        private IReplWindow _window;
        private readonly INodejsReplSite _site;
        internal static readonly object InputBeforeReset = new object();    // used to mark buffers which are no longer valid because we've done a reset

        private static bool LoggedReplUse = false;

        public NodejsReplEvaluator()
            : this(VsNodejsReplSite.Site)
        {
        }

        public NodejsReplEvaluator(INodejsReplSite site)
        {
            this._site = site;
        }

        #region IReplEvaluator Members

        public Task<ExecutionResult> Initialize(IReplWindow window)
        {
            this._window = window;
            this._window.SetOptionValue(ReplOptions.CommandPrefix, ".");
            this._window.SetOptionValue(ReplOptions.PrimaryPrompt, "> ");
            this._window.SetOptionValue(ReplOptions.SecondaryPrompt, ". ");
            this._window.SetOptionValue(ReplOptions.DisplayPromptInMargin, false);
            this._window.SetOptionValue(ReplOptions.SupportAnsiColors, true);
            this._window.SetOptionValue(ReplOptions.UseSmartUpDown, true);

            this._window.WriteLine(Resources.ReplInitializationMessage);

            return ExecutionResult.Succeeded;
        }

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        {
        }

        public Task<ExecutionResult> Reset()
        {
            var buffersBeforeReset = this._window.TextView.BufferGraph.GetTextBuffers(TruePredicate);
            for (var i = 0; i < buffersBeforeReset.Count - 1; i++)
            {
                var buffer = buffersBeforeReset[i];

                if (!buffer.Properties.ContainsProperty(InputBeforeReset))
                {
                    buffer.Properties.AddProperty(InputBeforeReset, InputBeforeReset);
                }
            }

            Connect();
            return ExecutionResult.Succeeded;
        }

        private static bool TruePredicate(ITextBuffer buffer)
        {
            return true;
        }

        public bool CanExecuteText(string text)
        {
            return true;
        }

        public Task<ExecutionResult> ExecuteText(string text)
        {
            EnsureConnected();
            if (this._listener == null)
            {
                return ExecutionResult.Failed;
            }

            if (!LoggedReplUse)
            {
                // we only want to log the first time each session, 
                // and not flood the telemetry with every command.
                TelemetryHelper.LogReplUse();
                LoggedReplUse = true;
            }

            return this._listener.ExecuteText(text);
        }

        public void ExecuteFile(string filename)
        {
            throw new NotImplementedException();
        }

        public string FormatClipboard()
        {
            return Clipboard.GetText();
        }

        public void AbortCommand()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this._listener != null)
            {
                this._listener.Dispose();
            }
        }

        #endregion

        private void EnsureConnected()
        {
            if (this._listener == null)
            {
                Connect();
            }
        }

        private void Connect()
        {
            if (this._listener != null)
            {
                this._listener.Disconnect();
                this._listener.Dispose();
                this._listener = null;
            }

            var nodeExePath = GetNodeExePath();
            if (string.IsNullOrWhiteSpace(nodeExePath))
            {
                this._window.WriteError(Resources.NodejsNotInstalled);
                this._window.WriteError(Environment.NewLine);
                return;
            }
            else if (!File.Exists(nodeExePath))
            {
                this._window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.NodeExeDoesntExist, nodeExePath));
                this._window.WriteError(Environment.NewLine);
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

            if (this._site.TryGetStartupFileAndDirectory(out fileName, out directory))
            {
                psi.WorkingDirectory = directory;
                psi.EnvironmentVariables["NODE_PATH"] = directory;
            }

            var process = new Process();
            process.StartInfo = psi;
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                this._window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.InteractiveWindowFailedToStartProcessErrorMessage, Environment.NewLine, e.ToString(), Environment.NewLine));
                return;
            }

            this._listener = new ListenerThread(this, process, socket);
        }

        private string GetNodeExePath()
        {
            var startupProject = this._site.GetStartupProject();
            string nodeExePath;
            if (startupProject != null)
            {
                nodeExePath = Nodejs.GetAbsoluteNodeExePath(
                    startupProject.ProjectHome,
                    startupProject.GetProjectProperty(NodeProjectProperty.NodeExePath)
                );
            }
            else
            {
                nodeExePath = Nodejs.NodeExePath;
            }
            return nodeExePath;
        }

        private static void CreateConnection(out Socket conn, out int portNum)
        {
            conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            conn.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            conn.Listen(0);
            portNum = ((IPEndPoint)conn.LocalEndPoint).Port;
        }

        internal void Clear()
        {
            this._listener.SendRequest(new Dictionary<string, object>() { { "type", "clear" } });
        }

        internal class ListenerThread : JsonListener, IDisposable
        {
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
            private static string _noReplProcess = Resources.InteractiveWindowNoProcessErrorMessage + Environment.NewLine;

            public ListenerThread(NodejsReplEvaluator eval, Process process, Socket socket)
            {
                this._eval = eval;
                this._process = process;
                this._acceptSocket = socket;

                this._acceptSocket.BeginAccept(this.SocketConnectionAccepted, null);

                this._process.OutputDataReceived += new DataReceivedEventHandler(this.StdOutReceived);
                this._process.ErrorDataReceived += new DataReceivedEventHandler(this.StdErrReceived);
                this._process.EnableRaisingEvents = true;
                this._process.Exited += this.ProcessExited;

                this._process.BeginOutputReadLine();
                this._process.BeginErrorReadLine();
            }

            private void StdOutReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    this._eval._window.WriteOutput(args.Data + Environment.NewLine);
                }
            }

            private void StdErrReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    this._eval._window.WriteError(args.Data + Environment.NewLine);
                }
            }

            private void ProcessExited(object sender, EventArgs args)
            {
                ProcessExitedWorker();
            }

            private void ProcessExitedWorker()
            {
                this._eval._window.WriteError(Resources.InteractiveWindowProcessExitedMessage + Environment.NewLine);
                using (new SocketLock(this))
                {
                    if (this._completion != null)
                    {
                        this._completion.SetResult(ExecutionResult.Failure);
                    }
                    this._completion = null;
                }
            }

            private void SocketConnectionAccepted(IAsyncResult result)
            {
                this.Socket = this._acceptSocket.EndAccept(result);
                this._acceptSocket.Close();

                using (new SocketLock(this))
                {
                    this._connected = true;
                }

                using (new SocketLock(this))
                {
                    if (this._executionText != null)
                    {
#if DEBUG
                        Debug.WriteLine("Executing delayed text: " + this._executionText);
#endif
                        SendExecuteText(this._executionText);
                        this._executionText = null;
                    }
                }

                StartListenerThread();
            }

            public Task<ExecutionResult> ExecuteText(string text)
            {
                TaskCompletionSource<ExecutionResult> completion;
#if DEBUG
                Debug.WriteLine("Executing text: " + text);
#endif
                using (new SocketLock(this))
                {
                    if (!this._connected)
                    {
                        // delay executing the text until we're connected
#if DEBUG
                        Debug.WriteLine("Delayed executing text");
#endif
                        this._completion = completion = new TaskCompletionSource<ExecutionResult>();
                        this._executionText = text;
                        return completion.Task;
                    }

                    try
                    {
                        if (!this.Socket.Connected)
                        {
                            this._eval._window.WriteError(_noReplProcess);
                            return ExecutionResult.Failed;
                        }

                        this._completion = completion = new TaskCompletionSource<ExecutionResult>();

                        SendExecuteText(text);
                    }
                    catch (SocketException)
                    {
                        this._eval._window.WriteError(_noReplProcess);
                        return ExecutionResult.Failed;
                    }

                    return completion.Task;
                }
            }

            [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
            private static extern bool AllowSetForegroundWindow(int dwProcessId);

            private void SendExecuteText(string text)
            {
                AllowSetForegroundWindow(this._process.Id);
                var request = new Dictionary<string, object>() {
                    { "type", "execute" },
                    { "code", text },
                };

                SendRequest(request);
            }

            internal void SendRequest(Dictionary<string, object> request)
            {
                var json = this._serializer.Serialize(request);

                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                var length = "Content-length: " + bytes.Length + "\r\n\r\n";
                var lengthBytes = System.Text.Encoding.UTF8.GetBytes(length);
                this.Socket.Send(lengthBytes);
                this.Socket.Send(bytes);
            }

            protected override void OnSocketDisconnected()
            {
            }

            protected override void ProcessPacket(JsonResponse response)
            {
                var cmd = this._serializer.Deserialize<Dictionary<string, object>>(response.Body);

                object type;
                if (cmd.TryGetValue("type", out type) && type is string)
                {
                    switch ((string)type)
                    {
                        case "execute":
                            object result;
                            if (cmd.TryGetValue("result", out result))
                            {
                                this._eval._window.WriteLine(result.ToString());
                                this._completion.SetResult(ExecutionResult.Success);
                            }
                            else if (cmd.TryGetValue("error", out result))
                            {
                                this._eval._window.WriteError(result.ToString());
                                this._completion.SetResult(ExecutionResult.Failure);
                            }
                            this._completion = null;
                            break;
                        case "output":
                            if (cmd.TryGetValue("output", out result))
                            {
                                this._eval._window.WriteOutput(FixOutput(result));
                            }
                            break;
                        case "output_error":
                            if (cmd.TryGetValue("output", out result))
                            {
                                this._eval._window.WriteError(FixOutput(result));
                            }
                            break;
#if DEBUG
                        default:
                            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Unknown command: {0}", response.Body));
                            break;
#endif
                    }
                }
            }

            private static string FixOutput(object result)
            {
                var res = result.ToString();
                if (res.IndexOf('\n') != -1)
                {
                    var fixedStr = new StringBuilder();
                    for (var i = 0; i < res.Length; i++)
                    {
                        if (res[i] == '\r')
                        {
                            if (i + 1 < res.Length && res[i + 1] == '\n')
                            {
                                i++;
                                fixedStr.Append("\r\n");
                            }
                            else
                            {
                                fixedStr.Append("\r\n");
                            }
                        }
                        else if (res[i] == '\n')
                        {
                            fixedStr.Append("\r\n");
                        }
                        else
                        {
                            fixedStr.Append(res[i]);
                        }
                    }
                    res = fixedStr.ToString();
                }
                return res;
            }

            internal void Disconnect()
            {
                if (this._completion != null)
                {
                    this._completion.SetResult(ExecutionResult.Failure);
                    this._completion = null;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this._disposed)
                {
                    if (this._process != null && !this._process.HasExited)
                    {
                        try
                        {
                            //Disconnect our event since we are forceably killing the process off
                            //  We'll synchronously send the message to the user
                            this._process.Exited -= this.ProcessExited;
                            this._process.Kill();
                        }
                        catch (InvalidOperationException)
                        {
                        }
                        catch (NotSupportedException)
                        {
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                        }
                        ProcessExitedWorker();
                    }

                    if (this._process != null)
                    {
                        this._process.Dispose();
                    }
                    this._disposed = true;
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

            private struct SocketLock : IDisposable
            {
                private readonly ListenerThread _evaluator;

                public SocketLock(ListenerThread evaluator)
                {
                    Monitor.Enter(evaluator._socketLock);
#if DEBUG
                    Debug.Assert(evaluator._socketLockedThread == null);
                    evaluator._socketLockedThread = Thread.CurrentThread;
#endif
                    this._evaluator = evaluator;
                }

                public void Dispose()
                {
#if DEBUG
                    this._evaluator._socketLockedThread = null;
#endif
                    Monitor.Exit(this._evaluator._socketLock);
                }
            }
            #endregion
        }
    }
}


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
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Repl
{
    [ReplRole("Reset"), ReplRole("Execution")]
    internal sealed class NodejsReplEvaluator : IReplEvaluator
    {
        private ListenerThread listener;
        private IReplWindow window;
        private readonly VsNodejsReplSite site;
        internal static readonly object InputBeforeReset = new object();    // used to mark buffers which are no longer valid because we've done a reset

        private static bool LoggedReplUse = false;

        public NodejsReplEvaluator()
        {
            this.site = VsNodejsReplSite.Site;
        }

        private string nodeExePath;

        public string NodeExePath
        {
            get
            {
                if (string.IsNullOrEmpty(this.nodeExePath))
                {
                    this.nodeExePath = this.GetNodeExePath();
                }
                return this.nodeExePath;
            }
        }

        #region IReplEvaluator Members

        public Task<ExecutionResult> Initialize(IReplWindow window)
        {
            this.window = window;
            this.window.SetOptionValue(ReplOptions.CommandPrefix, ".");
            this.window.SetOptionValue(ReplOptions.PrimaryPrompt, "> ");
            this.window.SetOptionValue(ReplOptions.SecondaryPrompt, ". ");
            this.window.SetOptionValue(ReplOptions.DisplayPromptInMargin, false);
            this.window.SetOptionValue(ReplOptions.SupportAnsiColors, true);
            this.window.SetOptionValue(ReplOptions.UseSmartUpDown, true);

            this.window.WriteLine(Resources.ReplInitializationMessage);

            return ExecutionResult.Succeeded;
        }

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        {
        }

        public Task<ExecutionResult> Reset()
        {
            var buffersBeforeReset = this.window.TextView.BufferGraph.GetTextBuffers(_ => true);
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

        public bool CanExecuteText(string text)
        {
            return true;
        }

        public Task<ExecutionResult> ExecuteText(string text)
        {
            EnsureConnected();
            if (this.listener == null)
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

            return this.listener.ExecuteText(text);
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
            if (this.listener != null)
            {
                this.listener.Dispose();
            }
        }

        #endregion

        private void EnsureConnected()
        {
            if (this.listener == null)
            {
                Connect();
            }
        }

        /// <summary>
        /// Checks if Node.js Exe is installed correctly.
        /// Writes an error message if it's not.
        /// </summary>
        /// <returns></returns>
        public bool EnsureNodeInstalled()
        {
            if (string.IsNullOrWhiteSpace(this.NodeExePath))
            {
                this.window.WriteError(Resources.NodejsNotInstalled);
                this.window.WriteError(Environment.NewLine);
                return false;
            }

            if (!File.Exists(this.NodeExePath))
            {
                this.window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.NodeExeDoesntExist, this.NodeExePath));
                this.window.WriteError(Environment.NewLine);
                return false;
            }

            return true;
        }

        private void Connect()
        {
            if (this.listener != null)
            {
                this.listener.Disconnect();
                this.listener.Dispose();
                this.listener = null;
            }

            if(!this.EnsureNodeInstalled())
            {
                return;
            }
            CreateConnection(out var socket, out var port);

            var scriptPath = "\"" +
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "visualstudio_nodejs_repl.js"
                    ) + "\"";

            var psi = new ProcessStartInfo(this.NodeExePath, scriptPath + " " + port)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            if (this.site.TryGetStartupFileAndDirectory(out var _, out var directory))
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
                this.window.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.InteractiveWindowFailedToStartProcessErrorMessage, Environment.NewLine, e.ToString(), Environment.NewLine));
                return;
            }

            this.listener = new ListenerThread(this, process, socket);
        }

        private string GetNodeExePath()
        {
            var startupProject = this.site.GetStartupProject();
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
            this.listener.SendRequest(new Dictionary<string, object>() { { "type", "clear" } });
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
            private bool disposed;
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
                    this._eval.window.WriteOutput(args.Data + Environment.NewLine);
                }
            }

            private void StdErrReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    this._eval.window.WriteError(args.Data + Environment.NewLine);
                }
            }

            private void ProcessExited(object sender, EventArgs args)
            {
                ProcessExitedWorker();
            }

            private void ProcessExitedWorker()
            {
                this._eval.window.WriteError(Resources.InteractiveWindowProcessExitedMessage + Environment.NewLine);
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
                            this._eval.window.WriteError(_noReplProcess);
                            return ExecutionResult.Failed;
                        }

                        this._completion = completion = new TaskCompletionSource<ExecutionResult>();

                        SendExecuteText(text);
                    }
                    catch (SocketException)
                    {
                        this._eval.window.WriteError(_noReplProcess);
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

                if (cmd.TryGetValue("type", out var type) && type is string)
                {
                    switch ((string)type)
                    {
                        case "execute":
                            object result;
                            if (cmd.TryGetValue("result", out result))
                            {
                                this._eval.window.WriteLine(result.ToString());
                                this._completion.SetResult(ExecutionResult.Success);
                            }
                            else if (cmd.TryGetValue("error", out result))
                            {
                                this._eval.window.WriteError(result.ToString());
                                this._completion.SetResult(ExecutionResult.Failure);
                            }
                            this._completion = null;
                            break;
                        case "output":
                            if (cmd.TryGetValue("output", out result))
                            {
                                this._eval.window.WriteOutput(FixOutput(result));
                            }
                            break;
                        case "output_error":
                            if (cmd.TryGetValue("output", out result))
                            {
                                this._eval.window.WriteError(FixOutput(result));
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
                if (!this.disposed)
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
                    this.disposed = true;
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
                private readonly ListenerThread evaluator;

                public SocketLock(ListenerThread evaluator)
                {
                    Monitor.Enter(evaluator._socketLock);
#if DEBUG
                    Debug.Assert(evaluator._socketLockedThread == null);
                    evaluator._socketLockedThread = Thread.CurrentThread;
#endif
                    this.evaluator = evaluator;
                }

                public void Dispose()
                {
#if DEBUG
                    this.evaluator._socketLockedThread = null;
#endif
                    Monitor.Exit(this.evaluator._socketLock);
                }
            }
            #endregion
        }
    }
}

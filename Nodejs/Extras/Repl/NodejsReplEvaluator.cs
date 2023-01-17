// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Forms;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.InteractiveWindow.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json;
using EnvDTE;
using Microsoft.NodejsTools.Extras;


namespace Microsoft.NodejsTools.Repl
{
    public sealed class NodejsReplEvaluator : IInteractiveEvaluator, IDisposable
    {
        private ListenerThread listener;
        private readonly IServiceProvider serviceProvider;
        private readonly IContentType contentType;

        public static readonly object InputBeforeReset = new object();    // used to mark buffers which are no longer valid because we've done a reset

        private IInteractiveWindowCommands commands;

        private static bool LoggedReplUse = false;

        public NodejsReplEvaluator(IServiceProvider serviceProvider, IContentType contentType)
        {
            this.serviceProvider = serviceProvider;
            this.contentType = contentType;
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

        public IInteractiveWindow CurrentWindow { get; set; }

        public Task<ExecutionResult> InitializeAsync()
        {
            this.CurrentWindow.WriteLine(Resources.ReplInitializationMessage);

            this.CurrentWindow.TextView.Options.SetOptionValue(InteractiveWindowOptions.SmartUpDown, true);
            this.commands = GetInteractiveCommands();

            return ExecutionResult.Succeeded;
        }

        private IInteractiveWindowCommands GetInteractiveCommands()
        {
            var model = this.serviceProvider.GetComponentModel();
            var cmdFactory = model.GetService<IInteractiveWindowCommandsFactory>();
            var cmds = model.GetExtensions<IInteractiveWindowCommand>();

            return cmdFactory.CreateInteractiveCommands(this.CurrentWindow, ".", cmds.Where(IsApplicable));

            bool IsApplicable(IInteractiveWindowCommand command)
            {
                var commandContentTypes = command.GetType()
                       .GetCustomAttributes(typeof(ContentTypeAttribute), true)
                       .Select(a => ((ContentTypeAttribute)a).ContentTypes)
                       .ToArray();

                // Commands with no content type are always applicable
                // If a commands specifies content types and none apply, exclude it
                if (commandContentTypes.Any() && !commandContentTypes.Any(cct => this.contentType.IsOfType(cct)))
                {
                    return false;
                }
                return true;
            }
        }

        public void ActiveLanguageBufferChanged(ITextBuffer currentBuffer, ITextBuffer previousBuffer)
        {
        }

        public Task<ExecutionResult> ResetAsync(bool initialize = true)
        {
            var buffersBeforeReset = this.CurrentWindow.TextView.BufferGraph.GetTextBuffers(_ => true);
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

        public bool CanExecuteCode(string text)
        {
            return true;
        }

        public string GetPrompt()
        {
            if ((this.CurrentWindow?.CurrentLanguageBuffer.CurrentSnapshot.LineCount ?? 1) > 1)
            {
                return ". ";
            }
            else
            {
                return "> ";
            }
        }

        public async Task<ExecutionResult> ExecuteCodeAsync(string text)
        {
            EnsureConnected();
            if (this.listener == null)
            {
                return ExecutionResult.Failure;
            }

            var cmds = this.commands;

            var cmdRes = cmds.TryExecuteCommand();
            if (cmdRes != null)
            {
                return await cmdRes;
            }

            if (!LoggedReplUse)
            {
                // we only want to log the first time each session, 
                // and not flood the telemetry with every command.
                TelemetryHelper.LogReplUse();
                LoggedReplUse = true;
            }

            return await this.listener.ExecuteTextAsync(text);
        }

        public void ExecuteFile(string filename)
        {
            throw new NotImplementedException();
        }

        public string FormatClipboard()
        {
            return string.Empty;
        }

        public void AbortExecution()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (this.listener != null)
            {
                this.listener.Dispose();
                this.listener = null;
            }
            this.CurrentWindow = null;
        }

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
                this.CurrentWindow.WriteErrorLine(Resources.NodejsNotInstalled);
                return false;
            }

            if (!File.Exists(this.NodeExePath))
            {
                this.CurrentWindow.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, Resources.NodeExeDoesntExist, this.NodeExePath));
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

            if (!this.EnsureNodeInstalled())
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
            if (TryGetStartupDirectory(out var directory))
            {
                psi.WorkingDirectory = directory;
                psi.EnvironmentVariables["NODE_PATH"] = directory;
            }

            var process = new System.Diagnostics.Process();
            process.StartInfo = psi;
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                this.CurrentWindow.WriteError(string.Format(CultureInfo.CurrentCulture, Resources.InteractiveWindowFailedToStartProcessErrorMessage, Environment.NewLine, e.ToString(), Environment.NewLine));
                return;
            }

            this.listener = new ListenerThread(this, process, socket);
        }

        private bool TryGetStartupDirectory(out string directory)
        {
            directory = string.Empty;
            var dte = serviceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            if (dte != null)
            {
                var dteProject = (EnvDTE.Project)((Array)dte.ActiveSolutionProjects).GetValue(0);
                if (dteProject != null)
                {
                    directory= Path.GetDirectoryName(dteProject.FullName);
                    return directory != null;
                }
            }
            return false;
        }

        private string GetNodeExePath()
        {
            return Nodejs.NodeExePath;
        }

        private static void CreateConnection(out Socket conn, out int portNum)
        {
            conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            conn.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            conn.Listen(0);
            portNum = ((IPEndPoint)conn.LocalEndPoint).Port;
        }

        public void Clear()
        {
            this.listener.SendRequest(new Dictionary<string, object>() { { "type", "clear" } });
        }

        public void WriteLine(string text)
        {
            AppendTextWithEscapes(this.CurrentWindow, text, this.CurrentWindow.Write, addNewLine: true);
        }

        public void Write(string text)
        {
            AppendTextWithEscapes(this.CurrentWindow, text, this.CurrentWindow.Write, addNewLine: false);
        }

        public void WriteError(string text)
        {
            AppendTextWithEscapes(this.CurrentWindow, text, this.CurrentWindow.WriteError, addNewLine: false);
        }

        public void WriteErrorLine(string text)
        {
            AppendTextWithEscapes(this.CurrentWindow, text, this.CurrentWindow.WriteError, addNewLine: true);
        }

        private static void AppendTextWithEscapes(
                 IInteractiveWindow window,
            string text,
            Func<string, Span> writer,
            bool addNewLine)
        {
            var start = 0;
            var escape = text.IndexOf("\x1b[");
            var colors = window.OutputBuffer.Properties.GetOrCreateSingletonProperty(
                ReplOutputClassifier.ColorKey,
                () => new List<ColoredSpan>()
            );
            InteractiveWindowColor? color = null;

            Span span;

            while (escape >= 0)
            {
                span = writer(text.Substring(start, escape - start));
                if (span.Length > 0)
                {
                    colors.Add(new ColoredSpan(span, color));
                }

                start = escape + 2;
                color = GetColorFromEscape(text, ref start);
                escape = text.IndexOf("\x1b[", start);
            }

            var rest = text.Substring(start);
            if (addNewLine)
            {
                rest += Environment.NewLine;
            }

            span = writer(rest);
            if (span.Length > 0)
            {
                colors.Add(new ColoredSpan(span, color));
            }
        }

        private static InteractiveWindowColor Change(InteractiveWindowColor? from, InteractiveWindowColor to)
        {
            return ((from ?? InteractiveWindowColor.Foreground) & InteractiveWindowColor.DarkGray) | to;
        }

        private static InteractiveWindowColor? GetColorFromEscape(string text, ref int start)
        {
            // http://en.wikipedia.org/wiki/ANSI_escape_code
            // process any ansi color sequences...
            InteractiveWindowColor? color = null;
            var codes = new List<int>();
            int? value = 0;

            while (start < text.Length)
            {
                if (text[start] >= '0' && text[start] <= '9')
                {
                    // continue parsing the integer...
                    if (value == null)
                    {
                        value = 0;
                    }
                    value = 10 * value.Value + (text[start] - '0');
                }
                else if (text[start] == ';')
                {
                    if (value != null)
                    {
                        codes.Add(value.Value);
                        value = null;
                    }
                    else
                    {
                        // CSI ; - invalid or CSI ### ;;, both invalid
                        break;
                    }
                }
                else if (text[start] == 'm')
                {
                    start += 1;
                    if (value != null)
                    {
                        codes.Add(value.Value);
                    }

                    // parsed a valid code
                    if (codes.Count == 0)
                    {
                        // reset
                        color = null;
                    }
                    else
                    {
                        for (var j = 0; j < codes.Count; j++)
                        {
                            switch (codes[j])
                            {
                                case 0: color = InteractiveWindowColor.White; break;
                                case 1: // bright/bold
                                    color |= InteractiveWindowColor.DarkGray;
                                    break;
                                case 2: // faint

                                case 3: // italic
                                case 4: // single underline
                                    break;
                                case 5: // blink slow
                                case 6: // blink fast
                                    break;
                                case 7: // negative
                                case 8: // conceal
                                case 9: // crossed out
                                case 10: // primary font
                                case 11: // 11-19, n-th alternate font
                                    break;
                                case 21: // bright/bold off 
                                    color &= ~InteractiveWindowColor.DarkGray;
                                    break;
                                case 22: // normal intensity
                                case 24: // underline off
                                    break;
                                case 25: // blink off
                                    break;
                                case 27: // image - postive
                                case 28: // reveal
                                case 29: // not crossed out
                                case 30: color = Change(color, InteractiveWindowColor.Black); break;
                                case 31: color = Change(color, InteractiveWindowColor.DarkRed); break;
                                case 32: color = Change(color, InteractiveWindowColor.DarkGreen); break;
                                case 33: color = Change(color, InteractiveWindowColor.DarkYellow); break;
                                case 34: color = Change(color, InteractiveWindowColor.DarkBlue); break;
                                case 35: color = Change(color, InteractiveWindowColor.DarkMagenta); break;
                                case 36: color = Change(color, InteractiveWindowColor.DarkCyan); break;
                                case 37: color = Change(color, InteractiveWindowColor.Gray); break;
                                case 38: // xterm 286 background color
                                case 39: // default text color
                                    color = null;
                                    break;
                                case 40: // background colors
                                case 41:
                                case 42:
                                case 43:
                                case 44:
                                case 45:
                                case 46:
                                case 47: break;
                                case 90: color = InteractiveWindowColor.DarkGray; break;
                                case 91: color = InteractiveWindowColor.Red; break;
                                case 92: color = InteractiveWindowColor.Green; break;
                                case 93: color = InteractiveWindowColor.Yellow; break;
                                case 94: color = InteractiveWindowColor.Blue; break;
                                case 95: color = InteractiveWindowColor.Magenta; break;
                                case 96: color = InteractiveWindowColor.Cyan; break;
                                case 97: color = InteractiveWindowColor.White; break;
                            }
                        }
                    }
                    break;
                }
                else
                {
                    // unknown char, invalid escape
                    break;
                }
                start += 1;
            }
            return color;
        }

        private class ListenerThread : JsonListener, IDisposable
        {
            private readonly NodejsReplEvaluator eval;
            private readonly System.Diagnostics.Process process;
            private readonly object socketLock = new object();
            private Socket acceptSocket;
            public bool connected;
            private TaskCompletionSource<ExecutionResult> completion;
            private string executionText;
            private bool disposed;
#if DEBUG
            private System.Threading.Thread socketLockedThread;
#endif

            public ListenerThread(NodejsReplEvaluator eval, System.Diagnostics.Process process, Socket socket)
            {
                this.eval = eval;
                this.process = process;
                this.acceptSocket = socket;

                this.acceptSocket.BeginAccept(this.SocketConnectionAccepted, null);

                this.process.OutputDataReceived += new DataReceivedEventHandler(this.StdOutReceived);
                this.process.ErrorDataReceived += new DataReceivedEventHandler(this.StdErrReceived);
                this.process.EnableRaisingEvents = true;
                this.process.Exited += this.ProcessExited;

                this.process.BeginOutputReadLine();
                this.process.BeginErrorReadLine();
            }

            private void StdOutReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    this.eval.WriteLine(args.Data);
                }
            }

            private void StdErrReceived(object sender, DataReceivedEventArgs args)
            {
                if (args.Data != null)
                {
                    this.eval.WriteErrorLine(args.Data);
                }
            }

            private void ProcessExited(object sender, EventArgs args)
            {
                ProcessExitedWorker();
            }

            private void ProcessExitedWorker()
            {
                this.eval.WriteErrorLine(Resources.InteractiveWindowProcessExitedMessage);
                using (new SocketLock(this))
                {
                    if (this.completion != null)
                    {
                        this.completion.SetResult(ExecutionResult.Failure);
                    }
                    this.completion = null;
                }
            }

            private void SocketConnectionAccepted(IAsyncResult result)
            {
                this.Socket = this.acceptSocket.EndAccept(result);
                this.acceptSocket.Close();

                using (new SocketLock(this))
                {
                    this.connected = true;
                }

                using (new SocketLock(this))
                {
                    if (this.executionText != null)
                    {
#if DEBUG
                        Debug.WriteLine("Executing delayed text: " + this.executionText);
#endif
                        SendExecuteText(this.executionText);
                        this.executionText = null;
                    }
                }

                StartListenerThread();
            }

            public Task<ExecutionResult> ExecuteTextAsync(string text)
            {
                TaskCompletionSource<ExecutionResult> completion;
#if DEBUG
                Debug.WriteLine("Executing text: " + text);
#endif
                using (new SocketLock(this))
                {
                    if (!this.connected)
                    {
                        // delay executing the text until we're connected
#if DEBUG
                        Debug.WriteLine("Delayed executing text");
#endif
                        this.completion = completion = new TaskCompletionSource<ExecutionResult>();
                        this.executionText = text;
                        return completion.Task;
                    }

                    try
                    {
                        if (!this.Socket.Connected)
                        {
                            this.eval.WriteErrorLine(Resources.InteractiveWindowNoProcessErrorMessage);
                            return ExecutionResult.Failed;
                        }

                        this.completion = completion = new TaskCompletionSource<ExecutionResult>();

                        SendExecuteText(text);
                    }
                    catch (SocketException)
                    {
                        this.eval.WriteErrorLine(Resources.InteractiveWindowNoProcessErrorMessage);
                        return ExecutionResult.Failed;
                    }

                    return completion.Task;
                }
            }

            [DllImport("user32", CallingConvention = CallingConvention.Winapi)]
            private static extern bool AllowSetForegroundWindow(int dwProcessId);

            private void SendExecuteText(string text)
            {
                AllowSetForegroundWindow(this.process.Id);
                var request = new Dictionary<string, object>() {
                    { "type", "execute" },
                    { "code", text },
                };

                SendRequest(request);
            }

            public void SendRequest(Dictionary<string, object> request)
            {
                var json = JsonConvert.SerializeObject(request);

                var bytes = Encoding.UTF8.GetBytes(json);
                var length = "Content-length: " + bytes.Length + "\r\n\r\n";
                var lengthBytes = Encoding.UTF8.GetBytes(length);
                this.Socket.Send(lengthBytes);
                this.Socket.Send(bytes);
            }

            protected override void OnSocketDisconnected()
            {
            }

            protected override void ProcessPacket(JsonResponse response)
            {
                var cmd = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Body);

                if (cmd.TryGetValue("type", out var type) && type is string)
                {
                    switch ((string)type)
                    {
                        case "execute":
                            object result;
                            if (cmd.TryGetValue("result", out result))
                            {
                                this.eval.WriteLine(result.ToString());
                                this.completion.SetResult(ExecutionResult.Success);
                            }
                            else if (cmd.TryGetValue("error", out result))
                            {
                                this.eval.WriteError(result.ToString());
                                this.completion.SetResult(ExecutionResult.Failure);
                            }
                            this.completion = null;
                            break;
                        case "output":
                            if (cmd.TryGetValue("output", out result))
                            {
                                this.eval.Write(FixOutput(result));
                            }
                            break;
                        case "outputerror":
                            if (cmd.TryGetValue("output", out result))
                            {
                                this.eval.WriteError(FixOutput(result));
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

            public void Disconnect()
            {
                if (this.completion != null)
                {
                    this.completion.SetResult(ExecutionResult.Failure);
                    this.completion = null;
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
                    if (this.process != null && !this.process.HasExited)
                    {
                        try
                        {
                            //Disconnect our event since we are forceably killing the process off
                            //  We'll synchronously send the message to the user
                            this.process.Exited -= this.ProcessExited;
                            this.process.Kill();
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

                    if (this.process != null)
                    {
                        this.process.Dispose();
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
                    Monitor.Enter(evaluator.socketLock);
#if DEBUG
                    Debug.Assert(evaluator.socketLockedThread == null);
                    evaluator.socketLockedThread = System.Threading.Thread.CurrentThread;
#endif
                    this.evaluator = evaluator;
                }

                public void Dispose()
                {
#if DEBUG
                    this.evaluator.socketLockedThread = null;
#endif
                    Monitor.Exit(this.evaluator.socketLock);
                }
            }
            #endregion
        }
    }
}

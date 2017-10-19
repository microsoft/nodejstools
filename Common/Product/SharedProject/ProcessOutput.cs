// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Base class that can receive output from <see cref="ProcessOutput"/>.
    /// 
    /// If this class implements <see cref="IDisposable"/>, it will be disposed
    /// when the <see cref="ProcessOutput"/> object is disposed.
    /// </summary>
    internal abstract class Redirector
    {
        /// <summary>
        /// Called when a line is written to standard output.
        /// </summary>
        /// <param name="line">The line of text, not including the newline. This
        /// is never null.</param>
        public abstract void WriteLine(string line);
        /// <summary>
        /// Called when a line is written to standard error.
        /// </summary>
        /// <param name="line">The line of text, not including the newline. This
        /// is never null.</param>
        public abstract void WriteErrorLine(string line);

        /// <summary>
        /// Called when output is written that should be brought to the user's
        /// attention. The default implementation does nothing.
        /// </summary>
        public virtual void Show()
        {
        }

        /// <summary>
        /// Called when output is written that should be brought to the user's
        /// immediate attention. The default implementation does nothing.
        /// </summary>
        public virtual void ShowAndActivate()
        {
        }

        /// <summary>
        /// Called to determine if stdin should be closed for a redirected process.
        /// The default is true.
        /// </summary>
        public virtual bool CloseStandardInput()
        {
            return true;
        }
    }

    internal sealed class TeeRedirector : Redirector, IDisposable
    {
        private readonly Redirector[] _redirectors;

        public TeeRedirector(params Redirector[] redirectors)
        {
            this._redirectors = redirectors;
        }

        public void Dispose()
        {
            foreach (var redir in this._redirectors.OfType<IDisposable>())
            {
                redir.Dispose();
            }
        }

        public override void WriteLine(string line)
        {
            foreach (var redir in this._redirectors)
            {
                redir.WriteLine(line);
            }
        }

        public override void WriteErrorLine(string line)
        {
            foreach (var redir in this._redirectors)
            {
                redir.WriteErrorLine(line);
            }
        }

        public override void Show()
        {
            foreach (var redir in this._redirectors)
            {
                redir.Show();
            }
        }

        public override void ShowAndActivate()
        {
            foreach (var redir in this._redirectors)
            {
                redir.ShowAndActivate();
            }
        }
    }

    /// <summary>
    /// Represents a process and its captured output.
    /// </summary>
    internal sealed class ProcessOutput : IDisposable
    {
        private readonly Process _process;
        private readonly string _arguments;
        private readonly List<string> _output, _error;
        private ManualResetEvent _waitHandleEvent;
        private readonly Redirector _redirector;
        private bool _isDisposed;
        private readonly object _seenNullLock = new object();
        private bool _seenNullInOutput, _seenNullInError;
        private bool _haveRaisedExitedEvent;
        private Task<int> _awaiter;

        private static readonly char[] EolChars = new[] { '\r', '\n' };
        private static readonly char[] _needToBeQuoted = new[] { ' ', '"' };

        /// <summary>
        /// Runs the provided executable file and allows the program to display
        /// output to the user.
        /// </summary>
        /// <param name="filename">Executable file to run.</param>
        /// <param name="arguments">Arguments to pass.</param>
        /// <returns>A <see cref="ProcessOutput"/> object.</returns>
        public static ProcessOutput RunVisible(string filename, params string[] arguments)
        {
            return Run(filename, arguments, null, null, true, null);
        }

        /// <summary>
        /// Runs the provided executable file hidden and captures any output
        /// messages.
        /// </summary>
        /// <param name="filename">Executable file to run.</param>
        /// <param name="arguments">Arguments to pass.</param>
        /// <returns>A <see cref="ProcessOutput"/> object.</returns>
        public static ProcessOutput RunHiddenAndCapture(string filename, params string[] arguments)
        {
            return Run(filename, arguments, null, null, false, null);
        }

        /// <summary>
        /// Runs the file with the provided settings.
        /// </summary>
        /// <param name="filename">Executable file to run.</param>
        /// <param name="arguments">Arguments to pass.</param>
        /// <param name="workingDirectory">Starting directory.</param>
        /// <param name="env">Environment variables to set.</param>
        /// <param name="visible">
        /// False to hide the window and redirect output to
        /// <see cref="StandardOutputLines"/> and
        /// <see cref="StandardErrorLines"/>.
        /// </param>
        /// <param name="redirector">
        /// An object to receive redirected output.
        /// </param>
        /// <param name="quoteArgs">
        /// True to ensure each argument is correctly quoted.
        /// </param>
        /// <param name="elevate">
        /// True to run the process as an administrator. See
        /// <see cref="RunElevated"/>.
        /// </param>
        /// <returns>A <see cref="ProcessOutput"/> object.</returns>
        public static ProcessOutput Run(
            string filename,
            IEnumerable<string> arguments,
            string workingDirectory,
            IEnumerable<KeyValuePair<string, string>> env,
            bool visible,
            Redirector redirector,
            bool quoteArgs = true,
            bool elevate = false,
            Encoding outputEncoding = null,
            Encoding errorEncoding = null
        )
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("Filename required", nameof(filename));
            }
            if (elevate)
            {
                return RunElevated(
                    filename,
                    arguments,
                    workingDirectory,
                    redirector,
                    quoteArgs,
                    outputEncoding,
                    errorEncoding);
            }

            var psi = new ProcessStartInfo("cmd.exe")
            {
                Arguments = string.Format(@"/S /C pushd {0} & {1} {2}",
                    QuoteSingleArgument(workingDirectory),
                    QuoteSingleArgument(filename),
                    GetArguments(arguments, quoteArgs)),
                CreateNoWindow = !visible,
                UseShellExecute = false,
                RedirectStandardError = !visible || (redirector != null),
                RedirectStandardOutput = !visible || (redirector != null),
                RedirectStandardInput = !visible
            };
            psi.StandardOutputEncoding = outputEncoding ?? psi.StandardOutputEncoding;
            psi.StandardErrorEncoding = errorEncoding ?? outputEncoding ?? psi.StandardErrorEncoding;
            if (env != null)
            {
                foreach (var kv in env)
                {
                    psi.EnvironmentVariables[kv.Key] = kv.Value;
                }
            }

            var process = new Process { StartInfo = psi };
            return new ProcessOutput(process, redirector);
        }

        /// <summary>
        /// Runs the file with the provided settings as a user with
        /// administrative permissions. The window is always hidden and output
        /// is provided to the redirector when the process terminates.
        /// </summary>
        /// <param name="filename">Executable file to run.</param>
        /// <param name="arguments">Arguments to pass.</param>
        /// <param name="workingDirectory">Starting directory.</param>
        /// <param name="redirector">
        /// An object to receive redirected output.
        /// </param>
        /// <param name="quoteArgs"></param>
        /// <returns>A <see cref="ProcessOutput"/> object.</returns>
        public static ProcessOutput RunElevated(
            string filename,
            IEnumerable<string> arguments,
            string workingDirectory,
            Redirector redirector,
            bool quoteArgs = true,
            Encoding outputEncoding = null,
            Encoding errorEncoding = null
        )
        {
            var outFile = Path.GetTempFileName();
            var errFile = Path.GetTempFileName();
            var psi = new ProcessStartInfo("cmd.exe")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true,
                Arguments = string.Format(@"/S /C pushd {0} & ""{1} {2} >>{3} 2>>{4}""",
                    QuoteSingleArgument(workingDirectory),
                    QuoteSingleArgument(filename),
                    GetArguments(arguments, quoteArgs),
                    QuoteSingleArgument(outFile),
                    QuoteSingleArgument(errFile))
            };

            var process = new Process();
            process.StartInfo = psi;
            var result = new ProcessOutput(process, redirector);
            if (redirector != null)
            {
                result.Exited += (s, e) =>
                {
                    try
                    {
                        try
                        {
                            var lines = File.ReadAllLines(outFile, outputEncoding ?? Encoding.Default);
                            foreach (var line in lines)
                            {
                                redirector.WriteLine(line);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (IsCriticalException(ex))
                            {
                                throw;
                            }
                            redirector.WriteErrorLine("Failed to obtain standard output from elevated process.");
#if DEBUG
                            foreach (var line in SplitLines(ex.ToString()))
                            {
                                redirector.WriteErrorLine(line);
                            }
#else
                            Trace.TraceError("Failed to obtain standard output from elevated process.");
                            Trace.TraceError(ex.ToString());
#endif
                        }
                        try
                        {
                            var lines = File.ReadAllLines(errFile, errorEncoding ?? outputEncoding ?? Encoding.Default);
                            foreach (var line in lines)
                            {
                                redirector.WriteErrorLine(line);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (IsCriticalException(ex))
                            {
                                throw;
                            }
                            redirector.WriteErrorLine("Failed to obtain standard error from elevated process.");
#if DEBUG
                            foreach (var line in SplitLines(ex.ToString()))
                            {
                                redirector.WriteErrorLine(line);
                            }
#else
                            Trace.TraceError("Failed to obtain standard error from elevated process.");
                            Trace.TraceError(ex.ToString());
#endif
                        }
                    }
                    finally
                    {
                        try
                        {
                            File.Delete(outFile);
                        }
                        catch { }
                        try
                        {
                            File.Delete(errFile);
                        }
                        catch { }
                    }
                };
            }
            return result;
        }

        public static string GetArguments(IEnumerable<string> arguments, bool quoteArgs)
        {
            if (quoteArgs)
            {
                return string.Join(" ", arguments.Where(a => a != null).Select(QuoteSingleArgument));
            }
            else
            {
                return string.Join(" ", arguments.Where(a => a != null));
            }
        }

        internal static IEnumerable<string> SplitLines(string source)
        {
            var start = 0;
            var end = source.IndexOfAny(EolChars);
            while (end >= start)
            {
                yield return source.Substring(start, end - start);
                start = end + 1;
                if (source[start - 1] == '\r' && start < source.Length && source[start] == '\n')
                {
                    start += 1;
                }

                if (start < source.Length)
                {
                    end = source.IndexOfAny(EolChars, start);
                }
                else
                {
                    end = -1;
                }
            }
            if (start <= 0)
            {
                yield return source;
            }
            else if (start < source.Length)
            {
                yield return source.Substring(start);
            }
        }

        public static string QuoteSingleArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return "\"\"";
            }
            if (arg.IndexOfAny(_needToBeQuoted) < 0)
            {
                return arg;
            }

            if (arg.StartsWith("\"") && arg.EndsWith("\""))
            {
                var inQuote = false;
                var consecutiveBackslashes = 0;
                foreach (var c in arg)
                {
                    if (c == '"')
                    {
                        if (consecutiveBackslashes % 2 == 0)
                        {
                            inQuote = !inQuote;
                        }
                    }

                    if (c == '\\')
                    {
                        consecutiveBackslashes += 1;
                    }
                    else
                    {
                        consecutiveBackslashes = 0;
                    }
                }
                if (!inQuote)
                {
                    return arg;
                }
            }

            var newArg = arg.Replace("\"", "\\\"");
            if (newArg.EndsWith("\\"))
            {
                newArg += "\\";
            }
            return "\"" + newArg + "\"";
        }

        private ProcessOutput(Process process, Redirector redirector)
        {
            this._arguments = QuoteSingleArgument(process.StartInfo.FileName) + " " + process.StartInfo.Arguments;
            this._redirector = redirector;
            if (this._redirector == null)
            {
                this._output = new List<string>();
                this._error = new List<string>();
            }

            this._process = process;
            if (this._process.StartInfo.RedirectStandardOutput)
            {
                this._process.OutputDataReceived += this.OnOutputDataReceived;
            }
            if (this._process.StartInfo.RedirectStandardError)
            {
                this._process.ErrorDataReceived += this.OnErrorDataReceived;
            }

            if (!this._process.StartInfo.RedirectStandardOutput && !this._process.StartInfo.RedirectStandardError)
            {
                // If we are receiving output events, we signal that the process
                // has exited when one of them receives null. Otherwise, we have
                // to listen for the Exited event.
                // If we just listen for the Exited event, we may receive it
                // before all the output has arrived.
                this._process.Exited += this.OnExited;
            }
            this._process.EnableRaisingEvents = true;

            try
            {
                this._process.Start();
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                {
                    throw;
                }
                if (this._redirector != null)
                {
                    foreach (var line in SplitLines(ex.ToString()))
                    {
                        this._redirector.WriteErrorLine(line);
                    }
                }
                else if (this._error != null)
                {
                    this._error.AddRange(SplitLines(ex.ToString()));
                }
                this._process = null;
            }

            if (this._process != null)
            {
                if (this._process.StartInfo.RedirectStandardOutput)
                {
                    this._process.BeginOutputReadLine();
                }
                if (this._process.StartInfo.RedirectStandardError)
                {
                    this._process.BeginErrorReadLine();
                }

                if (this._process.StartInfo.RedirectStandardInput)
                {
                    // Close standard input so that we don't get stuck trying to read input from the user.
                    if (_redirector == null || (_redirector != null && _redirector.CloseStandardInput()))
                    {
                        try
                        {
                            this._process.StandardInput.Close();
                        }
                        catch (InvalidOperationException)
                        {
                            // StandardInput not available
                        }
                    }
                }
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (this._isDisposed)
            {
                return;
            }

            if (e.Data == null)
            {
                bool shouldExit;
                lock (this._seenNullLock)
                {
                    this._seenNullInOutput = true;
                    shouldExit = this._seenNullInError || !this._process.StartInfo.RedirectStandardError;
                }
                if (shouldExit)
                {
                    OnExited(this._process, EventArgs.Empty);
                }
            }
            else if (!string.IsNullOrEmpty(e.Data))
            {
                foreach (var line in SplitLines(e.Data))
                {
                    if (this._output != null)
                    {
                        this._output.Add(line);
                    }
                    if (this._redirector != null)
                    {
                        this._redirector.WriteLine(line);
                    }
                }
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (this._isDisposed)
            {
                return;
            }

            if (e.Data == null)
            {
                bool shouldExit;
                lock (this._seenNullLock)
                {
                    this._seenNullInError = true;
                    shouldExit = this._seenNullInOutput || !this._process.StartInfo.RedirectStandardOutput;
                }
                if (shouldExit)
                {
                    OnExited(this._process, EventArgs.Empty);
                }
            }
            else if (!string.IsNullOrEmpty(e.Data))
            {
                foreach (var line in SplitLines(e.Data))
                {
                    if (this._error != null)
                    {
                        this._error.Add(line);
                    }
                    if (this._redirector != null)
                    {
                        this._redirector.WriteLine(line);
                    }
                }
            }
        }

        public int? ProcessId => this._process != null ? this._process.Id : (int?)null;

        /// <summary>
        /// The arguments that were originally passed, including the filename.
        /// </summary>
        public string Arguments => this._arguments;

        /// <summary>
        /// True if the process started. False if an error occurred.
        /// </summary>
        public bool IsStarted => this._process != null;

        /// <summary>
        /// The exit code or null if the process never started or has not
        /// exited.
        /// </summary>
        public int? ExitCode
        {
            get
            {
                if (this._process == null || !this._process.HasExited)
                {
                    return null;
                }
                return this._process.ExitCode;
            }
        }

        /// <summary>
        /// Gets or sets the priority class of the process.
        /// </summary>
        public ProcessPriorityClass PriorityClass
        {
            get
            {
                if (this._process != null && !this._process.HasExited)
                {
                    try
                    {
                        return this._process.PriorityClass;
                    }
                    catch (Win32Exception)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                        // Return Normal if we've raced with the process
                        // exiting.
                    }
                }
                return ProcessPriorityClass.Normal;
            }
            set
            {
                if (this._process != null && !this._process.HasExited)
                {
                    try
                    {
                        this._process.PriorityClass = value;
                    }
                    catch (Win32Exception)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                        // Silently fail if we've raced with the process
                        // exiting.
                    }
                }
            }
        }

        /// <summary>
        /// The redirector that was originally passed.
        /// </summary>
        public Redirector Redirector => this._redirector;
        /// <summary>
        /// Writes a line to stdin. A redirector must have been provided that indicates not
        /// to close the StandardInput stream.
        /// </summary>
        /// <param name="line"></param>
        public void WriteInputLine(string line)
        {
            if (IsStarted && _redirector != null && !_redirector.CloseStandardInput())
            {
                _process.StandardInput.WriteLine(line);
                _process.StandardInput.Flush();
            }
        }

        private void FlushAndCloseOutput()
        {
            if (this._process == null)
            {
                return;
            }

            if (this._process.StartInfo.RedirectStandardOutput)
            {
                try
                {
                    this._process.CancelOutputRead();
                }
                catch (InvalidOperationException)
                {
                    // Reader has already been cancelled
                }
            }
            if (this._process.StartInfo.RedirectStandardError)
            {
                try
                {
                    this._process.CancelErrorRead();
                }
                catch (InvalidOperationException)
                {
                    // Reader has already been cancelled
                }
            }

            if (this._waitHandleEvent != null)
            {
                try
                {
                    this._waitHandleEvent.Set();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        /// <summary>
        /// The lines of text sent to standard output. These do not include
        /// newline characters.
        /// </summary>
        public IEnumerable<string> StandardOutputLines => this._output;

        /// <summary>
        /// The lines of text sent to standard error. These do not include
        /// newline characters.
        /// </summary>
        public IEnumerable<string> StandardErrorLines => this._error;

        /// <summary>
        /// A handle that can be waited on. It triggers when the process exits.
        /// </summary>
        public WaitHandle WaitHandle
        {
            get
            {
                if (this._process == null)
                {
                    return null;
                }
                if (this._waitHandleEvent == null)
                {
                    this._waitHandleEvent = new ManualResetEvent(this._haveRaisedExitedEvent);
                }
                return this._waitHandleEvent;
            }
        }

        /// <summary>
        /// Waits until the process exits.
        /// </summary>
        public void Wait()
        {
            if (this._process != null)
            {
                this._process.WaitForExit();
                // Should have already been called, in which case this is a no-op
                OnExited(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Waits until the process exits or the timeout expires.
        /// </summary>
        /// <param name="timeout">The maximum time to wait.</param>
        /// <returns>
        /// True if the process exited before the timeout expired.
        /// </returns>
        public bool Wait(TimeSpan timeout)
        {
            if (this._process != null)
            {
                var exited = this._process.WaitForExit((int)timeout.TotalMilliseconds);
                if (exited)
                {
                    // Should have already been called, in which case this is a no-op
                    OnExited(this, EventArgs.Empty);
                }
                return exited;
            }
            return true;
        }

        /// <summary>
        /// Enables using 'await' on this object.
        /// </summary>
        public TaskAwaiter<int> GetAwaiter()
        {
            if (this._awaiter == null)
            {
                if (this._process == null)
                {
                    var tcs = new TaskCompletionSource<int>();
                    tcs.SetCanceled();
                    this._awaiter = tcs.Task;
                }
                else if (this._process.HasExited)
                {
                    // Should have already been called, in which case this is a no-op
                    OnExited(this, EventArgs.Empty);
                    var tcs = new TaskCompletionSource<int>();
                    tcs.SetResult(this._process.ExitCode);
                    this._awaiter = tcs.Task;
                }
                else
                {
                    this._awaiter = Task.Run(() =>
                    {
                        try
                        {
                            Wait();
                        }
                        catch (Win32Exception)
                        {
                            throw new OperationCanceledException();
                        }
                        return this._process.ExitCode;
                    });
                }
            }

            return this._awaiter.GetAwaiter();
        }

        /// <summary>
        /// Immediately stops the process.
        /// </summary>
        public void Kill()
        {
            if (this._process != null && !this._process.HasExited)
            {
                this._process.Kill();
                // Should have already been called, in which case this is a no-op
                OnExited(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raised when the process exits.
        /// </summary>
        public event EventHandler Exited;

        private void OnExited(object sender, EventArgs e)
        {
            if (this._isDisposed || this._haveRaisedExitedEvent)
            {
                return;
            }
            this._haveRaisedExitedEvent = true;
            FlushAndCloseOutput();
            var evt = Exited;
            if (evt != null)
            {
                evt(this, e);
            }
        }

        /// <summary>
        /// Called to dispose of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this._isDisposed)
            {
                this._isDisposed = true;
                if (this._process != null)
                {
                    if (this._process.StartInfo.RedirectStandardOutput)
                    {
                        this._process.OutputDataReceived -= this.OnOutputDataReceived;
                    }
                    if (this._process.StartInfo.RedirectStandardError)
                    {
                        this._process.ErrorDataReceived -= this.OnErrorDataReceived;
                    }
                    this._process.Dispose();
                }
                var disp = this._redirector as IDisposable;
                if (disp != null)
                {
                    disp.Dispose();
                }
                if (this._waitHandleEvent != null)
                {
                    this._waitHandleEvent.Set();
                    this._waitHandleEvent.Dispose();
                }
            }
        }

        private static bool IsCriticalException(Exception ex)
        {
            return ex is StackOverflowException ||
                ex is OutOfMemoryException ||
                ex is ThreadAbortException ||
                ex is AccessViolationException;
        }
    }
}

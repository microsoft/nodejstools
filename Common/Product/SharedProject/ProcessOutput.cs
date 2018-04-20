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

    /// <summary>
    /// Represents a process and its captured output.
    /// </summary>
    internal sealed class ProcessOutput : IDisposable
    {
        private readonly string arguments;
        private readonly List<string> output, error;
        private ManualResetEvent waitHandleEvent;
        private readonly Redirector redirector;
        private bool isDisposed;
        private readonly object seenNullLock = new object();
        private bool seenNullInOutput, seenNullInError;
        private bool haveRaisedExitedEvent;
        private Task<int> awaiter;

        private static readonly char[] EolChars = new[] { '\r', '\n' };
        private static readonly char[] NeedToBeQuoted = new[] { ' ', '"' };

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
                UseShellExecute = false
            };

            if (!visible || (redirector != null))
            {
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                // only set the encoding when we're redirecting the output
                psi.StandardOutputEncoding = outputEncoding ?? psi.StandardOutputEncoding;
                psi.StandardErrorEncoding = errorEncoding ?? outputEncoding ?? psi.StandardErrorEncoding;
            }

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

            var process = new Process
            {
                StartInfo = psi
            };

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
            if (arg.IndexOfAny(NeedToBeQuoted) < 0)
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
            this.arguments = QuoteSingleArgument(process.StartInfo.FileName) + " " + process.StartInfo.Arguments;
            this.redirector = redirector;
            if (this.redirector == null)
            {
                this.output = new List<string>();
                this.error = new List<string>();
            }

            this.Process = process;
            if (this.Process.StartInfo.RedirectStandardOutput)
            {
                this.Process.OutputDataReceived += this.OnOutputDataReceived;
            }
            if (this.Process.StartInfo.RedirectStandardError)
            {
                this.Process.ErrorDataReceived += this.OnErrorDataReceived;
            }

            if (!this.Process.StartInfo.RedirectStandardOutput && !this.Process.StartInfo.RedirectStandardError)
            {
                // If we are receiving output events, we signal that the process
                // has exited when one of them receives null. Otherwise, we have
                // to listen for the Exited event.
                // If we just listen for the Exited event, we may receive it
                // before all the output has arrived.
                this.Process.Exited += this.OnExited;
            }
            this.Process.EnableRaisingEvents = true;

            try
            {
                this.Process.Start();
            }
            catch (Exception ex)
            {
                if (IsCriticalException(ex))
                {
                    throw;
                }
                if (this.redirector != null)
                {
                    foreach (var line in SplitLines(ex.ToString()))
                    {
                        this.redirector.WriteErrorLine(line);
                    }
                }
                else if (this.error != null)
                {
                    this.error.AddRange(SplitLines(ex.ToString()));
                }
                this.Process = null;
            }

            if (this.Process != null)
            {
                if (this.Process.StartInfo.RedirectStandardOutput)
                {
                    this.Process.BeginOutputReadLine();
                }
                if (this.Process.StartInfo.RedirectStandardError)
                {
                    this.Process.BeginErrorReadLine();
                }

                if (this.Process.StartInfo.RedirectStandardInput)
                {
                    // Close standard input so that we don't get stuck trying to read input from the user.
                    if (this.redirector == null || (this.redirector != null && this.redirector.CloseStandardInput()))
                    {
                        try
                        {
                            this.Process.StandardInput.Close();
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
            if (this.isDisposed)
            {
                return;
            }

            if (e.Data == null)
            {
                bool shouldExit;
                lock (this.seenNullLock)
                {
                    this.seenNullInOutput = true;
                    shouldExit = this.seenNullInError || !this.Process.StartInfo.RedirectStandardError;
                }
                if (shouldExit)
                {
                    OnExited(this.Process, EventArgs.Empty);
                }
            }
            else if (!string.IsNullOrEmpty(e.Data))
            {
                foreach (var line in SplitLines(e.Data))
                {
                    if (this.output != null)
                    {
                        this.output.Add(line);
                    }
                    if (this.redirector != null)
                    {
                        this.redirector.WriteLine(line);
                    }
                }
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (e.Data == null)
            {
                bool shouldExit;
                lock (this.seenNullLock)
                {
                    this.seenNullInError = true;
                    shouldExit = this.seenNullInOutput || !this.Process.StartInfo.RedirectStandardOutput;
                }
                if (shouldExit)
                {
                    OnExited(this.Process, EventArgs.Empty);
                }
            }
            else if (!string.IsNullOrEmpty(e.Data))
            {
                foreach (var line in SplitLines(e.Data))
                {
                    if (this.error != null)
                    {
                        this.error.Add(line);
                    }
                    if (this.redirector != null)
                    {
                        this.redirector.WriteLine(line);
                    }
                }
            }
        }

        public int? ProcessId => this.IsStarted ? this.Process.Id : (int?)null;

        public Process Process { get; }

        /// <summary>
        /// The arguments that were originally passed, including the filename.
        /// </summary>
        public string Arguments => this.arguments;

        /// <summary>
        /// True if the process started. False if an error occurred.
        /// </summary>
        public bool IsStarted => this.Process != null;

        /// <summary>
        /// The exit code or null if the process never started or has not
        /// exited.
        /// </summary>
        public int? ExitCode
        {
            get
            {
                if (this.Process == null || !this.Process.HasExited)
                {
                    return null;
                }
                return this.Process.ExitCode;
            }
        }

        /// <summary>
        /// Gets or sets the priority class of the process.
        /// </summary>
        public ProcessPriorityClass PriorityClass
        {
            get
            {
                if (this.Process != null && !this.Process.HasExited)
                {
                    try
                    {
                        return this.Process.PriorityClass;
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
                if (this.Process != null && !this.Process.HasExited)
                {
                    try
                    {
                        this.Process.PriorityClass = value;
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
        public Redirector Redirector => this.redirector;
        /// <summary>
        /// Writes a line to stdin. A redirector must have been provided that indicates not
        /// to close the StandardInput stream.
        /// </summary>
        /// <param name="line"></param>
        public void WriteInputLine(string line)
        {
            if (IsStarted && redirector != null && !redirector.CloseStandardInput())
            {
                Process.StandardInput.WriteLine(line);
                Process.StandardInput.Flush();
            }
        }

        private void FlushAndCloseOutput()
        {
            if (this.Process == null)
            {
                return;
            }

            if (this.Process.StartInfo.RedirectStandardOutput)
            {
                try
                {
                    this.Process.CancelOutputRead();
                }
                catch (InvalidOperationException)
                {
                    // Reader has already been cancelled
                }
            }
            if (this.Process.StartInfo.RedirectStandardError)
            {
                try
                {
                    this.Process.CancelErrorRead();
                }
                catch (InvalidOperationException)
                {
                    // Reader has already been cancelled
                }
            }

            if (this.waitHandleEvent != null)
            {
                try
                {
                    this.waitHandleEvent.Set();
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
        public IEnumerable<string> StandardOutputLines => this.output;

        /// <summary>
        /// The lines of text sent to standard error. These do not include
        /// newline characters.
        /// </summary>
        public IEnumerable<string> StandardErrorLines => this.error;

        /// <summary>
        /// A handle that can be waited on. It triggers when the process exits.
        /// </summary>
        public WaitHandle WaitHandle
        {
            get
            {
                if (this.Process == null)
                {
                    return null;
                }
                if (this.waitHandleEvent == null)
                {
                    this.waitHandleEvent = new ManualResetEvent(this.haveRaisedExitedEvent);
                }
                return this.waitHandleEvent;
            }
        }

        public bool IsDisposed { get => this.isDisposed; set => this.isDisposed = value; }

        /// <summary>
        /// Waits until the process exits.
        /// </summary>
        public void Wait()
        {
            if (this.Process != null)
            {
                this.Process.WaitForExit();
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
            if (this.Process != null)
            {
                var exited = this.Process.WaitForExit((int)timeout.TotalMilliseconds);
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
            if (this.awaiter == null)
            {
                if (this.Process == null)
                {
                    var tcs = new TaskCompletionSource<int>();
                    tcs.SetCanceled();
                    this.awaiter = tcs.Task;
                }
                else if (this.Process.HasExited)
                {
                    // Should have already been called, in which case this is a no-op
                    OnExited(this, EventArgs.Empty);
                    var tcs = new TaskCompletionSource<int>();
                    tcs.SetResult(this.Process.ExitCode);
                    this.awaiter = tcs.Task;
                }
                else
                {
                    this.awaiter = Task.Run(() =>
                    {
                        try
                        {
                            Wait();
                        }
                        catch (Win32Exception)
                        {
                            throw new OperationCanceledException();
                        }
                        return this.Process.ExitCode;
                    });
                }
            }

            return this.awaiter.GetAwaiter();
        }

        /// <summary>
        /// Immediately stops the process.
        /// </summary>
        public void Kill()
        {
            if (this.Process != null && !this.Process.HasExited)
            {
                this.Process.Kill();
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
            if (this.isDisposed || this.haveRaisedExitedEvent)
            {
                return;
            }
            this.haveRaisedExitedEvent = true;
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
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                if (this.Process != null)
                {
                    if (this.Process.StartInfo.RedirectStandardOutput)
                    {
                        this.Process.OutputDataReceived -= this.OnOutputDataReceived;
                    }
                    if (this.Process.StartInfo.RedirectStandardError)
                    {
                        this.Process.ErrorDataReceived -= this.OnErrorDataReceived;
                    }
                    this.Process.Dispose();
                }
                if (this.redirector is IDisposable disp)
                {
                    disp.Dispose();
                }
                if (this.waitHandleEvent != null)
                {
                    this.waitHandleEvent.Set();
                    this.waitHandleEvent.Dispose();
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

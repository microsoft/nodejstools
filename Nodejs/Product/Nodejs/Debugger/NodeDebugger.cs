// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.NodejsTools.Logging;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Handles all interactions with a Node process which is being debugged.
    /// </summary>
    internal sealed class NodeDebugger : IDisposable
    {
        public const int MainThreadId = 1;
        private readonly Dictionary<int, NodeBreakpointBinding> breakpointBindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly IDebuggerClient client;
        private readonly IDebuggerConnection connection;
        private readonly Uri debuggerEndpointUri;
        private readonly Dictionary<int, string> errorCodes = new Dictionary<int, string>();
        private readonly ExceptionHandler exceptionHandler;
        private readonly Dictionary<string, NodeModule> modules = new Dictionary<string, NodeModule>(StringComparer.OrdinalIgnoreCase);
        private readonly EvaluationResultFactory resultFactory;
        private readonly SourceMapper sourceMapper;
        private readonly Dictionary<int, NodeThread> threads = new Dictionary<int, NodeThread>();
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(5);
        private bool attached;
        private bool breakOnAllExceptions;
        private bool breakOnUncaughtExceptions;
        private int commandId;
        private IFileNameMapper fileNameMapper;
        private bool handleEntryPointHit;
        private int? id;
        private bool loadCompleteHandled;
        private NodeProcess process;
        private int steppingCallstackDepth;
        private SteppingKind steppingMode;

        private NodeDebugger()
        {
            this.connection = new DebuggerConnection(new NetworkClientFactory());
            this.connection.ConnectionClosed += this.OnConnectionClosed;

            this.client = new DebuggerClient(this.connection);
            this.client.BreakpointEvent += this.OnBreakpointEvent;
            this.client.CompileScriptEvent += this.OnCompileScriptEvent;
            this.client.ExceptionEvent += this.OnExceptionEvent;

            this.resultFactory = new EvaluationResultFactory();
            this.exceptionHandler = new ExceptionHandler();
            this.sourceMapper = new SourceMapper();
            this.fileNameMapper = new LocalFileNameMapper();
        }

        public NodeDebugger(Uri debuggerEndpointUri, int id)
            : this()
        {
            this.debuggerEndpointUri = debuggerEndpointUri;
            this.id = id;
            this.attached = true;
        }

        public NodeDebugger(
            string exe,
            string script,
            string dir,
            string env,
            string interpreterOptions,
            NodeDebugOptions debugOptions,
            ushort? debuggerPort = null,
            bool createNodeWindow = true)
            : this()
        {
            // Select debugger port for a local connection
            var debuggerPortOrDefault = debuggerPort ?? GetDebuggerPort();
            this.debuggerEndpointUri = new UriBuilder { Scheme = "tcp", Host = "localhost", Port = debuggerPortOrDefault }.Uri;

            this.process = StartNodeProcessWithDebug(exe, script, dir, env, interpreterOptions, debugOptions, debuggerPortOrDefault, createNodeWindow);
        }

        private static ushort GetDebuggerPort()
        {
            var debuggerPortOrDefault = NodejsConstants.DefaultDebuggerPort;

            var activeConnections = (from listener in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
                                     select listener.Port).ToList();

            if (activeConnections.Contains(debuggerPortOrDefault))
            {
                debuggerPortOrDefault = (ushort)Enumerable.Range(new Random().Next(5859, 6000), 60000).Except(activeConnections).First();
            }

            return debuggerPortOrDefault;
        }

        public static NodeProcess StartNodeProcessWithDebug(
            string exe,
            string script,
            string dir,
            string env,
            string interpreterOptions,
            NodeDebugOptions debugOptions,
            ushort? debuggerPort = null,
            bool createNodeWindow = true)
        {
            // Select debugger port for a local connection
            var debuggerPortOrDefault = debuggerPort ?? GetDebuggerPort();

            // Node usage: node [options] [ -e script | script.js ] [arguments]
            var allArgs = $"--debug-brk={debuggerPortOrDefault} --nolazy {interpreterOptions} {script}";  // script includes the arguments for the script, so we can't quote it here

            return StartNodeProcess(exe, dir, env, debugOptions, debuggerPortOrDefault, allArgs, createNodeWindow);
        }

        public static NodeProcess StartNodeProcessWithInspect(
            string exe,
            string script,
            string dir,
            string env,
            string interpreterOptions,
            NodeDebugOptions debugOptions,
            ushort? debuggerPort = null,
            bool createNodeWindow = true)
        {
            // Select debugger port for a local connection
            var debuggerPortOrDefault = debuggerPort ?? GetDebuggerPort();

            // Node usage: node [options] [ -e script | script.js ] [arguments]
            var allArgs = $"--inspect-brk={debuggerPortOrDefault} --nolazy {interpreterOptions} {script}"; // script includes the arguments for the script, so we can't quote it here

            return StartNodeProcess(exe, dir, env, debugOptions, debuggerPortOrDefault, allArgs, createNodeWindow);
        }

        // starts the nodeprocess in debug mode without hooking up our debugger, this way we can attach the WebKit debugger as a next step.
        private static NodeProcess StartNodeProcess(
            string exe,
            string dir,
            string env,
            NodeDebugOptions
            debugOptions,
            ushort debuggerPortOrDefault,
            string allArgs,
            bool createNodeWindow)
        {
            var psi = new ProcessStartInfo(exe, allArgs)
            {
                CreateNoWindow = !createNodeWindow,
                WorkingDirectory = dir,
                UseShellExecute = false
            };

            if (env != null)
            {
                var envValues = env.Split('\0');
                foreach (var curValue in envValues)
                {
                    var nameValue = curValue.Split(new[] { '=' }, 2);
                    if (nameValue.Length == 2 && !string.IsNullOrWhiteSpace(nameValue[0]))
                    {
                        psi.EnvironmentVariables[nameValue[0]] = nameValue[1];
                    }
                }
            }

            return new NodeProcess(
                psi,
                waitOnAbnormal: debugOptions.HasFlag(NodeDebugOptions.WaitOnAbnormalExit),
                waitOnNormal: debugOptions.HasFlag(NodeDebugOptions.WaitOnNormalExit),
                enableRaisingEvents: true,
                debuggerPort: debuggerPortOrDefault);
        }

        #region Public Process API

        public int Id => this.id != null ? this.id.Value : this.process.Id;

        private NodeThread MainThread => this.threads[MainThreadId];

        public bool HasExited => !this.connection.Connected;

        /// <summary>
        /// Gets or sets a value indicating whether executed remote debugging process.
        /// </summary>
        public bool IsRemote { get; set; }

        public void Start(bool startListening = true)
        {
            this.process.Start();
            if (startListening)
            {
                StartListening();
            }
        }

        public void WaitForExit()
        {
            if (this.process == null)
            {
                return;
            }
            this.process.WaitForExit();
        }

        public bool WaitForExit(int milliseconds)
        {
            if (this.process == null)
            {
                return true;
            }
            return this.process.WaitForExit(milliseconds);
        }

        /// <summary>
        /// Terminates Node.js process.
        /// </summary>
        public void Terminate(bool killProcess = true)
        {
            lock (this)
            {
                // Disconnect
                this.connection.Close();

                // Fall back to using -1 for exit code if we cannot obtain one from the process
                // This is the normal case for attach where there is no process to interrogate
                var exitCode = -1;

                if (this.process != null)
                {
                    // Cleanup process
                    Debug.Assert(!this.attached);
                    try
                    {
                        if (killProcess && !this.process.HasExited)
                        {
                            this.process.Kill();
                        }
                        else
                        {
                            exitCode = this.process.ExitCode;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (Win32Exception)
                    {
                    }

                    this.process.Dispose();
                    this.process = null;
                }
                else
                {
                    // Avoid multiple events fired if multiple calls to Terminate()
                    if (!this.attached)
                    {
                        return;
                    }
                    this.attached = false;
                }

                // Fire event
                EventHandler<ProcessExitedEventArgs> exited = ProcessExited;
                if (exited != null)
                {
                    exited(this, new ProcessExitedEventArgs(exitCode));
                }
            }
        }

        /// <summary>
        /// Breaks into the process.
        /// </summary>
        public async Task BreakAllAsync()
        {
            DebugWriteCommand("BreakAll");

            var tokenSource = new CancellationTokenSource(this.timeout);
            var suspendCommand = new SuspendCommand(this.CommandId);
            await TrySendRequestAsync(suspendCommand, tokenSource.Token).ConfigureAwait(false);

            // Handle success
            // We need to get the backtrace before we break, so we request the backtrace
            // and follow up with firing the appropriate event for the break
            tokenSource = new CancellationTokenSource(this.timeout);
            var running = await PerformBacktraceAsync(tokenSource.Token).ConfigureAwait(false);
            Debug.Assert(!running);

            // Fallback to firing step complete event
            EventHandler<ThreadEventArgs> asyncBreakComplete = AsyncBreakComplete;
            if (asyncBreakComplete != null)
            {
                asyncBreakComplete(this, new ThreadEventArgs(this.MainThread));
            }
        }

        internal bool IsRunning()
        {
            var backtraceCommand = new BacktraceCommand(this.CommandId, this.resultFactory, fromFrame: 0, toFrame: 1);
            var tokenSource = new CancellationTokenSource(this.timeout);
            var task = TrySendRequestAsync(backtraceCommand, tokenSource.Token);
            if (task.Wait(this.timeout) && task.Result)
            {
                return backtraceCommand.Running;
            }
            return false;
        }

        private void DebugWriteCommand(string commandName)
        {
            LiveLogger.WriteLine("NodeDebugger Called " + commandName);
        }

        /// <summary>
        /// Resumes the process.
        /// </summary>
        public void Resume()
        {
            DebugWriteCommand("Resume");
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var tokenSource = new CancellationTokenSource(this.timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.None, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
        }

        private Task ContinueAndSaveSteppingAsync(SteppingKind steppingKind, bool resetSteppingMode = true, int stepCount = 1, CancellationToken cancellationToken = new CancellationToken())
        {
            if (resetSteppingMode)
            {
                this.steppingMode = steppingKind;
                this.steppingCallstackDepth = this.MainThread.CallstackDepth;
            }

            return ContinueAsync(steppingKind, stepCount, cancellationToken);
        }

        private async Task ContinueAsync(SteppingKind stepping = SteppingKind.None, int stepCount = 1, CancellationToken cancellationToken = new CancellationToken())
        {
            // Ensure load complete and entrypoint breakpoint/tracepoint handling disabled after first real continue
            this.loadCompleteHandled = true;
            this.handleEntryPointHit = false;

            var continueCommand = new ContinueCommand(this.CommandId, stepping, stepCount);
            await TrySendRequestAsync(continueCommand, cancellationToken).ConfigureAwait(false);
        }

        private Task AutoResumeAsync(bool haveCallstack, CancellationToken cancellationToken = new CancellationToken())
        {
            // Simply continue, if not stepping
            if (this.steppingMode != SteppingKind.None)
            {
                return AutoResumeSteppingAsync(haveCallstack, cancellationToken);
            }

            return ContinueAsync(cancellationToken: cancellationToken);
        }

        private async Task AutoResumeSteppingAsync(bool haveCallstack, CancellationToken cancellationToken = new CancellationToken())
        {
            int callstackDepth;
            if (haveCallstack)
            {
                // Have callstack, so get callstack depth from it
                callstackDepth = this.MainThread.CallstackDepth;
            }
            else
            {
                // Don't have callstack, so get callstack depth from server
                // Doing this avoids doing a full backtrace for all auto resumes
                callstackDepth = await GetCallstackDepthAsync(cancellationToken).ConfigureAwait(false);
            }

            await AutoResumeSteppingAsync(callstackDepth, haveCallstack, cancellationToken).ConfigureAwait(false);
        }

        private async Task AutoResumeSteppingAsync(int callstackDepth, bool haveCallstack, CancellationToken cancellationToken = new CancellationToken())
        {
            switch (this.steppingMode)
            {
                case SteppingKind.Over:
                    var stepCount = callstackDepth - this.steppingCallstackDepth;
                    if (stepCount > 0)
                    {
                        // Stepping over autoresumed break (in nested frame)
                        await ContinueAndSaveSteppingAsync(SteppingKind.Out, false, stepCount, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    break;
                case SteppingKind.Out:
                    stepCount = callstackDepth - this.steppingCallstackDepth + 1;
                    if (stepCount > 0)
                    {
                        // Stepping out across autoresumed break (in nested frame)
                        await ContinueAndSaveSteppingAsync(SteppingKind.Out, false, stepCount, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    break;
                case SteppingKind.Into:
                    // Stepping into or to autoresumed break
                    break;
                default:
                    LiveLogger.WriteLine("Unexpected SteppingMode: {0}", this.steppingMode);
                    break;
            }

            await CompleteSteppingAsync(haveCallstack, cancellationToken).ConfigureAwait(false);
        }

        private async Task CompleteSteppingAsync(bool haveCallstack, CancellationToken cancellationToken = new CancellationToken())
        {
            // Ensure we have callstack
            if (!haveCallstack)
            {
                var running = await PerformBacktraceAsync(cancellationToken).ConfigureAwait(false);
                Debug.Assert(!running);
            }

            EventHandler<ThreadEventArgs> stepComplete = StepComplete;
            if (stepComplete != null)
            {
                stepComplete(this, new ThreadEventArgs(this.MainThread));
            }
        }

        /// <summary>
        /// Adds a breakpoint in the specified file.
        /// </summary>
        public NodeBreakpoint AddBreakpoint(string fileName, int line, int column, bool enabled = true, BreakOn breakOn = new BreakOn(), string condition = null)
        {
            var target = new FilePosition(fileName, line, column);

            return new NodeBreakpoint(this, target, enabled, breakOn, condition);
        }

        public void SetExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
        )
        {
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var updated = false;

                if (defaultExceptionTreatment.HasValue)
                {
                    updated |= this.exceptionHandler.SetDefaultExceptionHitTreatment(defaultExceptionTreatment.Value);
                }

                if (exceptionTreatments != null)
                {
                    updated |= this.exceptionHandler.SetExceptionTreatments(exceptionTreatments);
                }

                if (updated)
                {
                    var tokenSource = new CancellationTokenSource(this.timeout);
                    await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
                }
            });
        }

        public void ClearExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
        )
        {
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var updated = false;

                if (defaultExceptionTreatment.HasValue)
                {
                    updated |= this.exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakNever);
                }

                updated |= this.exceptionHandler.ClearExceptionTreatments(exceptionTreatments);

                if (updated)
                {
                    var tokenSource = new CancellationTokenSource(this.timeout);
                    await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
                }
            });
        }

        public void ClearExceptionTreatment()
        {
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var updated = this.exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakNever);
                updated |= this.exceptionHandler.ResetExceptionTreatments();

                if (updated)
                {
                    var tokenSource = new CancellationTokenSource(this.timeout);
                    await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
                }
            });
        }

        #endregion

        #region Debuggee Communcation

        /// <summary>
        /// Gets a next command identifier.
        /// </summary>
        private int CommandId => Interlocked.Increment(ref this.commandId);

        /// <summary>
        /// Gets a source mapper.
        /// </summary>
        public SourceMapper SourceMapper => this.sourceMapper;

        /// <summary>
        /// Gets or sets a file name mapper.
        /// </summary>
        public IFileNameMapper FileNameMapper
        {
            get
            {
                return this.fileNameMapper;
            }
            set
            {
                if (value != null)
                {
                    this.fileNameMapper = value;
                }
            }
        }

        internal void Unregister()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Updates a module content while debugging.
        /// </summary>
        /// <param name="module">Node module.</param>
        /// <returns>Operation result.</returns>
        internal async Task<bool> UpdateModuleSourceAsync(NodeModule module)
        {
            module.Source = File.ReadAllText(module.JavaScriptFileName);

            var changeLiveCommand = new ChangeLiveCommand(this.CommandId, module);

            // Check whether update was successfull
            if (!await TrySendRequestAsync(changeLiveCommand).ConfigureAwait(false) ||
                !changeLiveCommand.Updated)
            {
                return false;
            }

            // Make step into and update stacktrace if required
            if (changeLiveCommand.StackModified)
            {
                var continueCommand = new ContinueCommand(this.CommandId, SteppingKind.Into);
                await TrySendRequestAsync(continueCommand).ConfigureAwait(false);
                await CompleteSteppingAsync(false).ConfigureAwait(false);
            }

            return true;
        }

        /// <summary>
        /// Starts listening for debugger communication.  Can be called after Start
        /// to give time to attach to debugger events.
        /// </summary>
        public void StartListening()
        {
            LiveLogger.WriteLine("NodeDebugger start listening");

            this.connection.Connect(this.debuggerEndpointUri);

            var mainThread = new NodeThread(this, MainThreadId, false);
            this.threads[mainThread.Id] = mainThread;

            if (!GetScriptsAsync().Wait((int)this.timeout.TotalMilliseconds))
            {
                LiveLogger.WriteLine("NodeDebugger GetScripts timeout");
                throw new TimeoutException("Timed out while retrieving scripts from debuggee.");
            }

            if (!SetExceptionBreakAsync().Wait((int)this.timeout.TotalMilliseconds))
            {
                LiveLogger.WriteLine("NodeDebugger SetException timeout");
                throw new TimeoutException("Timed out while setting up exception handling in debuggee.");
            }

            var backTraceTask = PerformBacktraceAsync();
            if (!backTraceTask.Wait((int)this.timeout.TotalMilliseconds))
            {
                LiveLogger.WriteLine("NodeDebugger backtrace timeout");
                throw new TimeoutException("Timed out while performing initial backtrace.");
            }

            // At this point we can fire events
            ThreadCreated?.Invoke(this, new ThreadEventArgs(mainThread));
            ProcessLoaded?.Invoke(this, new ThreadEventArgs(this.MainThread));
        }

        private void OnConnectionClosed(object sender, EventArgs args)
        {
            ThreadExited?.Invoke(this, new ThreadEventArgs(this.MainThread));
            Terminate(false);
        }

        private async Task GetScriptsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var scriptsCommand = new ScriptsCommand(this.CommandId);
            if (await TrySendRequestAsync(scriptsCommand, cancellationToken).ConfigureAwait(false))
            {
                AddModules(scriptsCommand.Modules);
            }
        }

        private void AddModules(IEnumerable<NodeModule> modules)
        {
            EventHandler<ModuleLoadedEventArgs> moduleLoaded = ModuleLoaded;
            if (moduleLoaded == null)
            {
                return;
            }

            foreach (var module in modules)
            {
                NodeModule newModule;
                if (GetOrAddModule(module, out newModule))
                {
                    foreach (var breakpoint in this.breakpointBindings)
                    {
                        var target = breakpoint.Value.Breakpoint.Target;
                        if (target.FileName.Equals(newModule.FileName, StringComparison.OrdinalIgnoreCase))
                        {
                            // attempt to rebind the breakpoint
                            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
                            {
                                await breakpoint.Value.Breakpoint.BindAsync().WaitAsync(TimeSpan.FromSeconds(2));
                            });
                        }
                    }

                    moduleLoaded(this, new ModuleLoadedEventArgs(newModule));
                }
            }
        }

        private async Task SetExceptionBreakAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break on all
            //var breakOnAllExceptions = defaultExceptionTreatment == ExceptionHitTreatment.BreakAlways || exceptionTreatments.Values.Any(value => value == ExceptionHitTreatment.BreakAlways);
            //var breakOnUncaughtExceptions = !all && (defaultExceptionTreatment != ExceptionHitTreatment.BreakNever || exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakNever));
            var breakOnAllExceptions = this.exceptionHandler.BreakOnAllExceptions;
            const bool breakOnUncaughtExceptions = false;

            if (this.HasExited)
            {
                return;
            }

            if (this.breakOnAllExceptions != breakOnAllExceptions)
            {
                var setExceptionBreakCommand = new SetExceptionBreakCommand(this.CommandId, false, breakOnAllExceptions);
                await TrySendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);

                this.breakOnAllExceptions = breakOnAllExceptions;
            }

            if (this.breakOnUncaughtExceptions != breakOnUncaughtExceptions)
            {
                var setExceptionBreakCommand = new SetExceptionBreakCommand(this.CommandId, true, breakOnUncaughtExceptions);
                await TrySendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);

                this.breakOnUncaughtExceptions = breakOnUncaughtExceptions;
            }
        }

        private void OnCompileScriptEvent(object sender, CompileScriptEventArgs args)
        {
            AddModules(new[] { args.CompileScriptEvent.Module });
        }

        private void OnBreakpointEvent(object sender, BreakpointEventArgs args)
        {
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var breakpointEvent = args.BreakpointEvent;

                // Process breakpoint bindings, ensuring we have callstack
                var running = await PerformBacktraceAsync().ConfigureAwait(false);
                Debug.Assert(!running);

                // Complete stepping, if no breakpoint bindings
                if (breakpointEvent.Breakpoints.Count == 0)
                {
                    await CompleteSteppingAsync(true).ConfigureAwait(false);
                    return;
                }

                //  Derive breakpoint bindings, if any
                var breakpointBindings = new List<NodeBreakpointBinding>();
                foreach (var breakpoint in args.BreakpointEvent.Breakpoints)
                {
                    NodeBreakpointBinding nodeBreakpointBinding;
                    if (this.breakpointBindings.TryGetValue(breakpoint, out nodeBreakpointBinding))
                    {
                        breakpointBindings.Add(nodeBreakpointBinding);
                    }
                }

                // Retrieve a local module
                NodeModule module;
                GetOrAddModule(breakpointEvent.Module, out module);
                module = module ?? breakpointEvent.Module;

                // Process break for breakpoint bindings, if any
                if (!await ProcessBreakpointBreakAsync(module, breakpointBindings, false).ConfigureAwait(false))
                {
                    // If we haven't reported LoadComplete yet, and don't have any matching bindings, this is the
                    // virtual breakpoint corresponding to the entry point (new since Node v0.12). We want to ignore
                    // this for the time being and not do anything - when we report LoadComplete, VS will calls us
                    // back telling us to continue, and at that point we will unfreeze the process.
                    // Otherwise, this is just some breakpoint that we don't know of, so tell it to resume running.
                    if (this.loadCompleteHandled)
                    {
                        await AutoResumeAsync(false).ConfigureAwait(false);
                    }
                }
            });
        }

        private async Task<bool> ProcessBreakpointBreakAsync(
            NodeModule brokeIn,
            IEnumerable<NodeBreakpointBinding> breakpointBindings,
            bool testFullyBound,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // Process breakpoint binding
            var hitBindings = new List<NodeBreakpointBinding>();

            // Iterate over breakpoint bindings, processing them as fully bound or not
            var currentLine = this.MainThread.TopStackFrame.Line;
            foreach (var breakpointBinding in breakpointBindings)
            {
                // Handle normal (fully bound) breakpoint binding
                if (breakpointBinding.FullyBound)
                {
                    if (!testFullyBound || await breakpointBinding.TestAndProcessHitAsync().ConfigureAwait(false))
                    {
                        hitBindings.Add(breakpointBinding);
                    }
                }
                else
                {
                    // Handle fixed-up breakpoint binding
                    // Rebind breakpoint
                    await RemoveBreakpointAsync(breakpointBinding, cancellationToken).ConfigureAwait(false);

                    var breakpoint = breakpointBinding.Breakpoint;

                    // If this breakpoint has been deleted, then do not try to rebind it after removing it from the list,
                    // and do not treat this binding as hit.
                    if (breakpoint.Deleted)
                    {
                        continue;
                    }

                    var result = await SetBreakpointAsync(breakpoint, cancellationToken: cancellationToken).ConfigureAwait(false);

                    // Treat rebound breakpoint binding as fully bound
                    var reboundbreakpointBinding = CreateBreakpointBinding(breakpoint, result.BreakpointId, result.ScriptId, breakpoint.GetPosition(this.SourceMapper).FileName, result.Line, result.Column, true);
                    HandleBindBreakpointSuccess(reboundbreakpointBinding, breakpoint);

                    // Handle invalid-line fixup (second bind matches current line)
                    if (reboundbreakpointBinding.Target.Line == currentLine && await reboundbreakpointBinding.TestAndProcessHitAsync().ConfigureAwait(false))
                    {
                        hitBindings.Add(reboundbreakpointBinding);
                    }
                }
            }

            // Handle last processed breakpoint binding by breaking with breakpoint hit events
            var matchedBindings = ProcessBindings(brokeIn.JavaScriptFileName, hitBindings).ToList();

            // Fire breakpoint hit event(s)
            EventHandler<BreakpointHitEventArgs> breakpointHit = BreakpointHit;
            foreach (var binding in matchedBindings)
            {
                await binding.ProcessBreakpointHitAsync(cancellationToken).ConfigureAwait(false);
                if (breakpointHit != null)
                {
                    breakpointHit(this, new BreakpointHitEventArgs(binding, this.MainThread));
                }
            }

            return matchedBindings.Count != 0;
        }

        /// <summary>
        /// Checks list of selected bindings.
        /// </summary>
        /// <param name="fileName">Module file name.</param>
        /// <param name="hitBindings">Collection of selected bindings.</param>
        /// <returns>Matched bindings.</returns>
        private IEnumerable<NodeBreakpointBinding> ProcessBindings(string fileName, IEnumerable<NodeBreakpointBinding> hitBindings)
        {
            foreach (var hitBinding in hitBindings)
            {
                var localFileName = this.fileNameMapper.GetLocalFileName(fileName);
                if (StringComparer.OrdinalIgnoreCase.Equals(localFileName, hitBinding.Position.FileName))
                {
                    yield return hitBinding;
                }
                else
                {
                    hitBinding.FixupHitCount();
                }
            }
        }

        private void OnExceptionEvent(object sender, ExceptionEventArgs args)
        {
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var exception = args.ExceptionEvent;

                if (exception.ErrorNumber == null)
                {
                    ReportException(exception);
                    return;
                }

                var errorNumber = exception.ErrorNumber.Value;
                string errorCodeFromMap;
                if (this.errorCodes.TryGetValue(errorNumber, out errorCodeFromMap))
                {
                    ReportException(exception, errorCodeFromMap);
                    return;
                }

                var lookupCommand = new LookupCommand(this.CommandId, this.resultFactory, new[] { exception.ErrorNumber.Value });
                string errorCodeFromLookup = null;

                if (await TrySendRequestAsync(lookupCommand).ConfigureAwait(false))
                {
                    errorCodeFromLookup = lookupCommand.Results[errorNumber][0].StringValue;
                    this.errorCodes[errorNumber] = errorCodeFromLookup;
                }

                ReportException(exception, errorCodeFromLookup);
            });
        }

        private void ReportException(ExceptionEvent exceptionEvent, string errorCode = null)
        {
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var exceptionName = exceptionEvent.ExceptionName;
                if (!string.IsNullOrEmpty(errorCode))
                {
                    exceptionName = string.Format(CultureInfo.InvariantCulture, "{0}({1})", exceptionName, errorCode);
                }

                // UNDONE Handle break on unhandled, once just my code is supported
                // Node has a catch all, so there are no uncaught exceptions
                // For now just break always or never
                //if (exceptionTreatment == ExceptionHitTreatment.BreakNever ||
                //    (exceptionTreatment == ExceptionHitTreatment.BreakOnUnhandled && !uncaught)) {
                var exceptionTreatment = this.exceptionHandler.GetExceptionHitTreatment(exceptionName);
                if (exceptionTreatment == ExceptionHitTreatment.BreakNever)
                {
                    await AutoResumeAsync(false).ConfigureAwait(false);
                    return;
                }

                // We need to get the backtrace before we break, so we request the backtrace
                // and follow up with firing the appropriate event for the break
                var running = await PerformBacktraceAsync().ConfigureAwait(false);
                Debug.Assert(!running);

                // Handle followup
                EventHandler<ExceptionRaisedEventArgs> exceptionRaised = ExceptionRaised;
                if (exceptionRaised == null)
                {
                    return;
                }

                var description = exceptionEvent.Description;
                if (description.StartsWith("#<", StringComparison.Ordinal) && description.EndsWith(">", StringComparison.Ordinal))
                {
                    // Serialize exception object to get a proper description
                    var tokenSource = new CancellationTokenSource(this.timeout);
                    var evaluateCommand = new EvaluateCommand(this.CommandId, this.resultFactory, exceptionEvent.ExceptionId);
                    if (await TrySendRequestAsync(evaluateCommand, tokenSource.Token).ConfigureAwait(false))
                    {
                        description = evaluateCommand.Result.StringValue;
                    }
                }

                var exception = new NodeException(exceptionName, description);
                exceptionRaised(this, new ExceptionRaisedEventArgs(this.MainThread, exception, exceptionEvent.Uncaught));
            });
        }

        private async Task<int> GetCallstackDepthAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var backtraceCommand = new BacktraceCommand(this.CommandId, this.resultFactory, 0, 1, true);
            await TrySendRequestAsync(backtraceCommand, cancellationToken).ConfigureAwait(false);
            return backtraceCommand.CallstackDepth;
        }

        private IEnumerable<NodeStackFrame> GetLocalFrames(IEnumerable<NodeStackFrame> stackFrames)
        {
            foreach (var stackFrame in stackFrames)
            {
                // Retrieve a local module
                NodeModule module;
                GetOrAddModule(stackFrame.Module, out module, stackFrame);
                module = module ?? stackFrame.Module;

                var line = stackFrame.Line;
                var column = stackFrame.Column;
                var functionName = stackFrame.FunctionName;

                // Map file position to original, if required
                if (module.JavaScriptFileName != module.FileName)
                {
                    var mapping = this.SourceMapper.MapToOriginal(module.JavaScriptFileName, line, column);
                    if (mapping != null)
                    {
                        line = mapping.Line;
                        column = mapping.Column;
                        functionName = string.IsNullOrEmpty(mapping.Name) ? functionName : mapping.Name;
                    }
                }

                stackFrame.Process = this;
                stackFrame.Module = module;
                stackFrame.Line = line;
                stackFrame.Column = column;
                stackFrame.FunctionName = functionName;

                yield return stackFrame;
            }
        }

        /// <summary>
        /// Retrieves a backtrace for current execution point.
        /// </summary>
        /// <returns>Whether program execution in progress.</returns>
        private async Task<bool> PerformBacktraceAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            // CONSIDER:  Lazy population of callstacks
            // Given the VS Debugger UI always asks for full callstacks, we always ask Node.js for full backtraces.
            // Given the nature or Node.js code, deep callstacks are expected to be rare.
            // Although according to the V8 docs (http://code.google.com/p/v8/wiki/DebuggerProtocol) the 'backtrace'
            // request takes a 'bottom' parameter, empirically, Node.js fails requests with it set.  Here we
            // approximate 'bottom' for 'toFrame' using int.MaxValue.  Node.js silently handles toFrame depths
            // greater than the current callstack.
            var backtraceCommand = new BacktraceCommand(this.CommandId, this.resultFactory, 0, int.MaxValue);
            if (!await TrySendRequestAsync(backtraceCommand, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }

            // Add extracted modules
            AddModules(backtraceCommand.Modules.Values);

            // Add stack frames
            var stackFrames = GetLocalFrames(backtraceCommand.StackFrames).ToList();

            // Collects results of number type which have null values and perform a lookup for actual values
            var numbersWithNullValue = new List<NodeEvaluationResult>();
            foreach (var stackFrame in stackFrames)
            {
                numbersWithNullValue.AddRange(stackFrame.Locals.Concat(stackFrame.Parameters)
                    .Where(p => p.TypeName == NodeVariableType.Number && p.StringValue == null));
            }

            if (numbersWithNullValue.Count > 0)
            {
                var lookupCommand = new LookupCommand(this.CommandId, this.resultFactory, numbersWithNullValue);
                if (await TrySendRequestAsync(lookupCommand, cancellationToken).ConfigureAwait(false))
                {
                    foreach (var targetResult in numbersWithNullValue)
                    {
                        var lookupResult = lookupCommand.Results[targetResult.Handle][0];
                        targetResult.StringValue = targetResult.HexValue = lookupResult.StringValue;
                    }
                }
            }

            this.MainThread.Frames = stackFrames;

            return backtraceCommand.Running;
        }

        internal IList<NodeThread> GetThreads()
        {
            return this.threads.Values.ToList();
        }

        internal void SendStepOver(int identity)
        {
            DebugWriteCommand("StepOver");
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var tokenSource = new CancellationTokenSource(this.timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.Over, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
        }

        internal void SendStepInto(int identity)
        {
            DebugWriteCommand("StepInto");
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var tokenSource = new CancellationTokenSource(this.timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.Into, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
        }

        internal void SendStepOut(int identity)
        {
            DebugWriteCommand("StepOut");
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                var tokenSource = new CancellationTokenSource(this.timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.Out, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
        }

        internal void SendResumeThread(int threadId)
        {
            DebugWriteCommand("ResumeThread");
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                // Handle load complete resume
                if (!this.loadCompleteHandled)
                {
                    this.loadCompleteHandled = true;
                    this.handleEntryPointHit = true;

                    // Handle breakpoint binding at entrypoint
                    // Attempt to fire breakpoint hit event without actually resuming
                    var topFrame = this.MainThread.TopStackFrame;
                    var currentLine = topFrame.Line;
                    var breakFileName = topFrame.Module.FileName;
                    var breakModule = GetModuleForFilePath(breakFileName);

                    var breakpointBindings = new List<NodeBreakpointBinding>();
                    foreach (var breakpointBinding in this.breakpointBindings.Values)
                    {
                        if (breakpointBinding.Enabled && breakpointBinding.Position.Line == currentLine &&
                            GetModuleForFilePath(breakpointBinding.Target.FileName) == breakModule)
                        {
                            breakpointBindings.Add(breakpointBinding);
                        }
                    }

                    if (breakpointBindings.Count > 0)
                    {
                        // Delegate to ProcessBreak() which knows how to correctly
                        // fire breakpoint hit events for given breakpoint bindings and current backtrace
                        if (!await ProcessBreakpointBreakAsync(breakModule, breakpointBindings, true).ConfigureAwait(false))
                        {
                            HandleEntryPointHit();
                        }
                        return;
                    }

                    // Handle no breakpoint at entrypoint
                    // Fire entrypoint hit event without actually resuming
                    // SDM will auto-resume on entrypoint hit for F5 launch, but not for F10/F11 launch
                    HandleEntryPointHit();
                    return;
                }

                // Handle tracepoint (auto-resumed "when hit" breakpoint) at entrypoint resume, by firing entrypoint hit event without actually resuming
                // If the SDM auto-resumes a tracepoint hit at the entrypoint, we need to give the SDM a chance to handle the entrypoint.
                // By first firing breakpoint hit for a breakpoint/tracepoint at the entrypoint, and then falling back to firing entrypoint hit
                // when the breakpoint is a tracepoint (auto-resumed), the breakpoint's/tracepoint's side effects will be seen, including when effectively
                // breaking at the entrypoint for F10/F11 launch.            
                // SDM will auto-resume on entrypoint hit for F5 launch, but not for F10/F11 launch
                if (HandleEntryPointHit())
                {
                    return;
                }

                // Handle tracepoint (auto-resumed "when hit" breakpoint) resume during stepping
                await AutoResumeAsync(true).ConfigureAwait(false);
            });
        }

        private bool HandleEntryPointHit()
        {
            if (this.handleEntryPointHit)
            {
                this.handleEntryPointHit = false;
                EventHandler<ThreadEventArgs> entryPointHit = EntryPointHit;
                if (entryPointHit != null)
                {
                    entryPointHit(this, new ThreadEventArgs(this.MainThread));
                    return true;
                }
            }
            return false;
        }

        public void SendClearStepping(int threadId)
        {
            DebugWriteCommand("ClearStepping");
            //throw new NotImplementedException();
        }

        public void Detach()
        {
            DebugWriteCommand("Detach");
            DebuggerClient.RunWithRequestExceptionsHandled(async () =>
            {
                // Disconnect request has no response
                var tokenSource = new CancellationTokenSource(this.timeout);
                var disconnectCommand = new DisconnectCommand(this.CommandId);
                await TrySendRequestAsync(disconnectCommand, tokenSource.Token).ConfigureAwait(false);
                this.connection.Close();
            });
        }

        public async Task<NodeBreakpointBinding> BindBreakpointAsync(NodeBreakpoint breakpoint, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await SetBreakpointAsync(breakpoint, cancellationToken: cancellationToken).ConfigureAwait(false);

            var position = breakpoint.GetPosition(this.SourceMapper);
            var fullyBound = (result.ScriptId.HasValue && result.Line == position.Line);
            var breakpointBinding = CreateBreakpointBinding(breakpoint, result.BreakpointId, result.ScriptId, position.FileName, result.Line, result.Column, fullyBound);

            // Fully bound (normal case)
            // Treat as success
            if (fullyBound)
            {
                HandleBindBreakpointSuccess(breakpointBinding, breakpoint);
                return breakpointBinding;
            }

            // Not fully bound, with predicate
            // Rebind without predicate
            if (breakpoint.HasPredicate)
            {
                await RemoveBreakpointAsync(breakpointBinding, cancellationToken).ConfigureAwait(false);
                result = await SetBreakpointAsync(breakpoint, true, cancellationToken).ConfigureAwait(false);

                Debug.Assert(!(result.ScriptId.HasValue && result.Line == position.Line));
                CreateBreakpointBinding(breakpoint, result.BreakpointId, result.ScriptId, position.FileName, result.Line, result.Column, false);
            }

            // Not fully bound, without predicate
            // Treat as failure (for now)
            HandleBindBreakpointFailure(breakpoint);
            return null;
        }

        private async Task<SetBreakpointCommand> SetBreakpointAsync(
            NodeBreakpoint breakpoint,
            bool withoutPredicate = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            DebugWriteCommand("Set Breakpoint");

            // Try to find module
            var module = GetModuleForFilePath(breakpoint.Target.FileName);

            var setBreakpointCommand = new SetBreakpointCommand(this.CommandId, module, breakpoint, withoutPredicate, this.IsRemote, this.SourceMapper);
            await TrySendRequestAsync(setBreakpointCommand, cancellationToken).ConfigureAwait(false);

            return setBreakpointCommand;
        }

        private NodeBreakpointBinding CreateBreakpointBinding(NodeBreakpoint breakpoint, int breakpointId, int? scriptId, string filename, int line, int column, bool fullyBound)
        {
            var position = new FilePosition(filename, line, column);
            var target = position;

            var mapping = this.SourceMapper.MapToOriginal(filename, line, column);
            if (mapping != null)
            {
                target = new FilePosition(breakpoint.Target.FileName, mapping.Line, mapping.Column);
            }

            var breakpointBinding = breakpoint.CreateBinding(target, position, breakpointId, scriptId, fullyBound);
            this.breakpointBindings[breakpointId] = breakpointBinding;
            return breakpointBinding;
        }

        private void HandleBindBreakpointSuccess(NodeBreakpointBinding breakpointBinding, NodeBreakpoint breakpoint)
        {
            EventHandler<BreakpointBindingEventArgs> breakpointBound = BreakpointBound;
            if (breakpointBound != null)
            {
                breakpointBound(this, new BreakpointBindingEventArgs(breakpoint, breakpointBinding));
            }
        }

        private void HandleBindBreakpointFailure(NodeBreakpoint breakpoint)
        {
            EventHandler<BreakpointBindingEventArgs> breakpointBindFailure = BreakpointBindFailure;
            if (breakpointBindFailure != null)
            {
                breakpointBindFailure(this, new BreakpointBindingEventArgs(breakpoint, null));
            }
        }

        internal async Task UpdateBreakpointBindingAsync(
            int breakpointId,
            bool? enabled = null,
            string condition = null,
            int? ignoreCount = null,
            bool validateSuccess = false,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // DEVNOTE: Calling UpdateBreakpointBinding() on the debug thread with validateSuccess == true will deadlock
            // and timout, causing both the followup handler to be called before confirmation of success (or failure), and
            // a return of false (failure).
            DebugWriteCommand("Update Breakpoint binding");

            var changeBreakPointCommand = new ChangeBreakpointCommand(this.CommandId, breakpointId, enabled, condition, ignoreCount);
            await TrySendRequestAsync(changeBreakPointCommand, cancellationToken).ConfigureAwait(false);
        }

        internal async Task<int?> GetBreakpointHitCountAsync(int breakpointId, CancellationToken cancellationToken = new CancellationToken())
        {
            var listBreakpointsCommand = new ListBreakpointsCommand(this.CommandId);

            int hitCount;
            if (await TrySendRequestAsync(listBreakpointsCommand, cancellationToken).ConfigureAwait(false) &&
                listBreakpointsCommand.Breakpoints.TryGetValue(breakpointId, out hitCount))
            {
                return hitCount;
            }

            return null;
        }

        internal async Task<NodeEvaluationResult> ExecuteTextAsync(
            NodeStackFrame stackFrame,
            string text,
            CancellationToken cancellationToken = new CancellationToken())
        {
            DebugWriteCommand("Execute Text Async");

            var evaluateCommand = new EvaluateCommand(this.CommandId, this.resultFactory, text, stackFrame);
            await this.client.SendRequestAsync(evaluateCommand, cancellationToken).ConfigureAwait(false);
            return evaluateCommand.Result;
        }

        internal async Task<NodeEvaluationResult> SetVariableValueAsync(
            NodeStackFrame stackFrame,
            string name,
            string value,
            CancellationToken cancellationToken = new CancellationToken())
        {
            DebugWriteCommand("Set Variable Value");

            // Create a new value
            var evaluateValueCommand = new EvaluateCommand(this.CommandId, this.resultFactory, value, stackFrame);
            await this.client.SendRequestAsync(evaluateValueCommand, cancellationToken).ConfigureAwait(false);
            var handle = evaluateValueCommand.Result.Handle;

            // Set variable value
            var setVariableValueCommand = new SetVariableValueCommand(this.CommandId, this.resultFactory, stackFrame, name, handle);
            await this.client.SendRequestAsync(setVariableValueCommand, cancellationToken).ConfigureAwait(false);
            return setVariableValueCommand.Result;
        }

        internal async Task<List<NodeEvaluationResult>> EnumChildrenAsync(NodeEvaluationResult nodeEvaluationResult, CancellationToken cancellationToken = new CancellationToken())
        {
            DebugWriteCommand("Enum Children");

            var lookupCommand = new LookupCommand(this.CommandId, this.resultFactory, new List<NodeEvaluationResult> { nodeEvaluationResult });
            if (!await TrySendRequestAsync(lookupCommand, cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            return lookupCommand.Results[nodeEvaluationResult.Handle];
        }

        internal async Task RemoveBreakpointAsync(NodeBreakpointBinding breakpointBinding, CancellationToken cancellationToken = new CancellationToken())
        {
            DebugWriteCommand("Remove Breakpoint");

            // Perform remove idempotently, as remove may be called in response to BreakpointUnound event
            if (breakpointBinding.Unbound)
            {
                return;
            }

            var breakpointId = breakpointBinding.BreakpointId;
            if (this.connection.Connected)
            {
                var clearBreakpointsCommand = new ClearBreakpointCommand(this.CommandId, breakpointId);
                await TrySendRequestAsync(clearBreakpointsCommand, cancellationToken).ConfigureAwait(false);
            }

            var breakpoint = breakpointBinding.Breakpoint;
            this.breakpointBindings.Remove(breakpointId);
            breakpoint.RemoveBinding(breakpointBinding);
            breakpointBinding.Unbound = true;

            EventHandler<BreakpointBindingEventArgs> breakpointUnbound = BreakpointUnbound;
            if (breakpointUnbound != null)
            {
                breakpointUnbound(this, new BreakpointBindingEventArgs(breakpoint, breakpointBinding));
            }
        }

        internal async Task<string> GetScriptTextAsync(int moduleId, CancellationToken cancellationToken = new CancellationToken())
        {
            DebugWriteCommand("GetScriptText: " + moduleId);

            var scriptsCommand = new ScriptsCommand(this.CommandId, true, moduleId);
            if (!await TrySendRequestAsync(scriptsCommand, cancellationToken).ConfigureAwait(false) ||
                scriptsCommand.Modules.Count == 0)
            {
                return null;
            }

            return scriptsCommand.Modules[0].Source;
        }

        internal async Task<bool> TestPredicateAsync(string expression, CancellationToken cancellationToken = new CancellationToken())
        {
            DebugWriteCommand("TestPredicate: " + expression);

            var predicateExpression = string.Format(CultureInfo.InvariantCulture, "Boolean({0})", expression);
            var evaluateCommand = new EvaluateCommand(this.CommandId, this.resultFactory, predicateExpression);

            return await TrySendRequestAsync(evaluateCommand, cancellationToken).ConfigureAwait(false) &&
                   evaluateCommand.Result != null &&
                   evaluateCommand.Result.Type == NodeExpressionType.Boolean &&
                   evaluateCommand.Result.StringValue == "true";
        }

        private async Task<bool> TrySendRequestAsync(DebuggerCommand command, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                await this.client.SendRequestAsync(command, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (DebuggerCommandException ex)
            {
                var evt = DebuggerOutput;
                if (evt != null)
                {
                    evt(this, new OutputEventArgs(null, ex.Message + Environment.NewLine));
                }
                return false;
            }
        }

        #endregion

        #region Debugging Events

        /// <summary>
        /// Fired when the process has started and is broken into the debugger, but before any user code is run.
        /// </summary>
        public event EventHandler<ThreadEventArgs> ProcessLoaded;

        public event EventHandler<ThreadEventArgs> ThreadCreated;
        public event EventHandler<ThreadEventArgs> ThreadExited;
        public event EventHandler<ThreadEventArgs> EntryPointHit;
        public event EventHandler<ThreadEventArgs> StepComplete;
        public event EventHandler<ThreadEventArgs> AsyncBreakComplete;
        public event EventHandler<ProcessExitedEventArgs> ProcessExited;
        public event EventHandler<ModuleLoadedEventArgs> ModuleLoaded;
        public event EventHandler<ExceptionRaisedEventArgs> ExceptionRaised;
        public event EventHandler<BreakpointBindingEventArgs> BreakpointBound;
        public event EventHandler<BreakpointBindingEventArgs> BreakpointUnbound;
        public event EventHandler<BreakpointBindingEventArgs> BreakpointBindFailure;
        public event EventHandler<BreakpointHitEventArgs> BreakpointHit;

        public event EventHandler<OutputEventArgs> DebuggerOutput;

        #endregion

        #region Modules Management

        /// <summary>
        /// Gets or adds a new module.
        /// </summary>
        /// <param name="module">New module.</param>
        /// <param name="value">Existing module.</param>
        /// <param name="stackFrame">The stack frame linked to the module.</param>
        /// <returns>True if module was added otherwise false.</returns>
        private bool GetOrAddModule(NodeModule module, out NodeModule value, NodeStackFrame stackFrame = null)
        {
            value = null;
            var javaScriptFileName = module.JavaScriptFileName;
            int? line = null, column = null;

            if (string.IsNullOrEmpty(javaScriptFileName) ||
                javaScriptFileName == NodeVariableType.UnknownModule ||
                javaScriptFileName.StartsWith("binding:", StringComparison.Ordinal))
            {
                return false;
            }

            // Get local JS file name
            javaScriptFileName = this.FileNameMapper.GetLocalFileName(javaScriptFileName);

            // Try to get mapping for JS file
            if (stackFrame != null)
            {
                line = stackFrame.Line;
                column = stackFrame.Column;
            }
            var originalFileName = this.SourceMapper.GetOriginalFileName(javaScriptFileName, line, column);

            if (originalFileName == null)
            {
                module = new NodeModule(module.Id, javaScriptFileName);
            }
            else
            {
                var directoryName = Path.GetDirectoryName(javaScriptFileName) ?? string.Empty;
                var fileName = CommonUtils.GetAbsoluteFilePath(directoryName, originalFileName.Replace('/', '\\'));

                module = new NodeModule(module.Id, fileName, javaScriptFileName);
            }

            // Check whether module already exits
            if (this.modules.TryGetValue(module.FileName, out value))
            {
                return false;
            }

            value = module;

            // Add module
            this.modules[module.FileName] = module;

            return true;
        }

        /// <summary>
        /// Gets a module for file path.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Module.</returns>
        public NodeModule GetModuleForFilePath(string filePath)
        {
            NodeModule module;
            this.modules.TryGetValue(filePath, out module);
            return module;
        }

        #endregion

        internal void Close()
        {
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NodeDebugger()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Clean up managed resources
                Terminate();
            }
        }

        #endregion
    }
}


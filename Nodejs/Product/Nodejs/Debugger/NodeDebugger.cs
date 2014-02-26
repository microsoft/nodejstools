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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Handles all interactions with a Node process which is being debugged.
    /// </summary>
    class NodeDebugger : IDisposable {
        public readonly int MainThreadId = 1;
        private readonly Dictionary<int, NodeBreakpointBinding> _breakpointBindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly IDebuggerClient _client;
        private readonly IDebuggerConnection _connection;
        private readonly Dictionary<int, string> _errorCodes = new Dictionary<int, string>();
        private readonly ExceptionHandler _exceptionHandler;
        private readonly string _hostName;
        private readonly Dictionary<int, NodeModule> _mapIdToScript = new Dictionary<int, NodeModule>();
        private readonly Dictionary<string, NodeModule> _mapNameToScript = new Dictionary<string, NodeModule>(StringComparer.OrdinalIgnoreCase);
        private readonly Version _nodeSetVariableValueVersion = new Version(0, 10, 12);
        private readonly ushort _portNumber;
        private readonly EvaluationResultFactory _resultFactory;
        private readonly Dictionary<int, NodeThread> _threads = new Dictionary<int, NodeThread>();
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(2);
        private bool _attached;
        private bool _breakOnAllExceptions;
        private bool _breakOnUncaughtExceptions;
        private bool _handleEntryPointHit;
        private int? _id;
        private bool _loadCompleteHandled;
        private int _number;
        private Process _process;
        private int _steppingCallstackDepth;
        private SteppingKind _steppingMode;

        private NodeDebugger() {
            _connection = new DebuggerConnection(new TcpClientFactory());
            _connection.ConnectionClosed += OnConnectionClosed;

            _client = new DebuggerClient(_connection);
            _client.BreakpointEvent += OnBreakpointEvent;
            _client.CompileScriptEvent += OnCompileScriptEvent;
            _client.ExceptionEvent += OnExceptionEvent;

            _resultFactory = new EvaluationResultFactory();
            _exceptionHandler = new ExceptionHandler();
            SourceMapper = new SourceMapper();
        }

        public NodeDebugger(
            string exe,
            string script,
            string dir,
            string env,
            string interpreterOptions,
            NodeDebugOptions debugOptions,
            List<string[]> dirMapping,
            ushort? debuggerPort = null,
            bool createNodeWindow = true) : this() {
            // Select debugger port for a local connection
            ushort debuggerPortOrDefault = NodejsConstants.DefaultDebuggerPort;
            if (debuggerPort != null) {
                debuggerPortOrDefault = debuggerPort.Value;
            } else {
                List<int> activeConnections =
                    (from listener in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners()
                        select listener.Port).ToList();
                if (activeConnections.Contains(debuggerPortOrDefault)) {
                    debuggerPortOrDefault = (ushort)Enumerable.Range(new Random().Next(5859, 6000), 60000).Except(activeConnections).First();
                }
            }

            _hostName = "localhost";
            _portNumber = debuggerPortOrDefault;

            var allArgs = String.Format("--debug-brk={0} {1}", debuggerPortOrDefault, script);
            if (!string.IsNullOrEmpty(interpreterOptions)) {
                allArgs += " " + interpreterOptions;
            }

            var psi = new ProcessStartInfo(exe, allArgs) {
                CreateNoWindow = !createNodeWindow,
                WorkingDirectory = dir,
                UseShellExecute = false
            };

            if (env != null) {
                string[] envValues = env.Split('\0');
                foreach (string curValue in envValues) {
                    string[] nameValue = curValue.Split(new[] { '=' }, 2);
                    if (nameValue.Length == 2 && !String.IsNullOrWhiteSpace(nameValue[0])) {
                        psi.EnvironmentVariables[nameValue[0]] = nameValue[1];
                    }
                }
            }

            FixupForRunAndWait(
                debugOptions.HasFlag(NodeDebugOptions.WaitOnAbnormalExit),
                debugOptions.HasFlag(NodeDebugOptions.WaitOnNormalExit),
                psi
            );

            _process = new Process {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
        }

        public NodeDebugger(string hostName, ushort portNumber, int id) : this() {
            _hostName = hostName;
            _portNumber = portNumber;
            _id = id;
            _attached = true;
        }

        #region Public Process API

        public int Id {
            get { return _id != null ? _id.Value : _process.Id; }
        }

        private NodeThread MainThread {
            get { return _threads[MainThreadId]; }
        }

        public bool HasExited {
            get { return !_connection.Connected; }
        }

        public void Start(bool startListening = true) {
            _process.Start();
            if (startListening) {
                StartListening();
            }
        }

        public void WaitForExit() {
            if (_process == null) {
                return;
            }
            _process.WaitForExit();
        }

        public bool WaitForExit(int milliseconds) {
            if (_process == null) {
                return true;
            }
            return _process.WaitForExit(milliseconds);
        }

        /// <summary>
        /// Terminates node.js process.
        /// </summary>
        public void Terminate(bool killProcess = true) {
            lock (this) {
                // Disconnect
                _connection.Close();

                // Fall back to using -1 for exit code if we cannot obtain one from the process
                // This is the normal case for attach where there is no process to interrogate
                int exitCode = -1;

                if (_process != null) {
                    // Cleanup process
                    Debug.Assert(!_attached);
                    try {
                        if (killProcess && !_process.HasExited) {
                            _process.Kill();
                        } else {
                            exitCode = _process.ExitCode;
                        }
                    } catch (InvalidOperationException) {
                    } catch (Win32Exception) {
                    }

                    _process.Dispose();
                    _process = null;
                } else {
                    // Avoid multiple events fired if multiple calls to Terminate()
                    if (!_attached) {
                        return;
                    }
                    _attached = false;
                }

                // Fire event
                EventHandler<ProcessExitedEventArgs> exited = ProcessExited;
                if (exited != null) {
                    exited(this, new ProcessExitedEventArgs(exitCode));
                }
            }
        }

        /// <summary>
        /// Breaks into the process.
        /// </summary>
        public async Task BreakAllAsync() {
            DebugWriteCommand("BreakAll");

            var tokenSource = new CancellationTokenSource(_timeout);
            var suspendCommand = new SuspendCommand(CommandId);
            await _client.SendRequestAsync(suspendCommand, tokenSource.Token).ConfigureAwait(false);

            // Handle success
            // We need to get the backtrace before we break, so we request the backtrace
            // and follow up with firing the appropriate event for the break
            tokenSource = new CancellationTokenSource(_timeout);
            bool running = await PerformBacktraceAsync(tokenSource.Token).ConfigureAwait(false);
            Debug.Assert(!running);

            // Fallback to firing step complete event
            EventHandler<ThreadEventArgs> asyncBreakComplete = AsyncBreakComplete;
            if (asyncBreakComplete != null) {
                asyncBreakComplete(this, new ThreadEventArgs(MainThread));
            }
        }

        [Conditional("DEBUG")]
        private void DebugWriteCommand(string commandName) {
            DebugWriteLine("NodeDebugger Called " + commandName);
        }

        [Conditional("DEBUG")]
        private void DebugWriteLine(string message) {
            Debug.WriteLine("[{0}] {1}", DateTime.UtcNow.TimeOfDay, message);
        }

        /// <summary>
        /// Resumes the process.
        /// </summary>
        public async void Resume() {
            DebugWriteCommand("Resume");
            var tokenSource = new CancellationTokenSource(_timeout);
            await ContinueAndSaveSteppingAsync(SteppingKind.None, cancellationToken: tokenSource.Token).ConfigureAwait(false);
        }

        private Task ContinueAndSaveSteppingAsync(SteppingKind steppingKind, bool resetSteppingMode = true, int stepCount = 1, CancellationToken cancellationToken = new CancellationToken()) {
            if (resetSteppingMode) {
                _steppingMode = steppingKind;
                _steppingCallstackDepth = MainThread.CallstackDepth;
            }

            return ContinueAsync(steppingKind, stepCount, cancellationToken);
        }

        private async Task ContinueAsync(SteppingKind stepping = SteppingKind.None, int stepCount = 1, CancellationToken cancellationToken = new CancellationToken()) {
            // Ensure load complete and entrypoint breakpoint/tracepoint handling disabled after first real continue
            _loadCompleteHandled = true;
            _handleEntryPointHit = false;

            var continueCommand = new ContinueCommand(CommandId, stepping, stepCount);
            await _client.SendRequestAsync(continueCommand, cancellationToken).ConfigureAwait(false);
        }

        private Task AutoResumeAsync(bool haveCallstack, CancellationToken cancellationToken = new CancellationToken()) {
            // Simply continue, if not stepping
            if (_steppingMode != SteppingKind.None) {
                return AutoResumeSteppingAsync(haveCallstack, cancellationToken);
            }

            return ContinueAsync(cancellationToken: cancellationToken);
        }

        private async Task AutoResumeSteppingAsync(bool haveCallstack, CancellationToken cancellationToken = new CancellationToken()) {
            int callstackDepth;
            if (haveCallstack) {
                // Have callstack, so get callstack depth from it
                callstackDepth = MainThread.CallstackDepth;
            } else {
                // Don't have callstack, so get callstack depth from server
                // Doing this avoids doing a full backtrace for all auto resumes
                callstackDepth = await GetCallstackDepthAsync(cancellationToken).ConfigureAwait(false);
            }

            await AutoResumeSteppingAsync(callstackDepth, haveCallstack, cancellationToken).ConfigureAwait(false);
        }

        private async Task AutoResumeSteppingAsync(int callstackDepth, bool haveCallstack, CancellationToken cancellationToken = new CancellationToken()) {
            switch (_steppingMode) {
                case SteppingKind.Over:
                    int stepCount = callstackDepth - _steppingCallstackDepth;
                    if (stepCount > 0) {
                        // Stepping over autoresumed break (in nested frame)
                        await ContinueAndSaveSteppingAsync(SteppingKind.Out, false, stepCount, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    break;
                case SteppingKind.Out:
                    stepCount = callstackDepth - _steppingCallstackDepth + 1;
                    if (stepCount > 0) {
                        // Stepping out across autoresumed break (in nested frame)
                        await ContinueAndSaveSteppingAsync(SteppingKind.Out, false, stepCount, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    break;
                case SteppingKind.Into:
                    // Stepping into or to autoresumed break
                    break;
                default:
                    Debug.WriteLine("Unexpected SteppingMode: {0}", _steppingMode);
                    break;
            }

            await CompleteSteppingAsync(haveCallstack, cancellationToken).ConfigureAwait(false);
        }

        private async Task CompleteSteppingAsync(bool haveCallstack, CancellationToken cancellationToken = new CancellationToken()) {
            // Ensure we have callstack
            if (!haveCallstack) {
                bool running = await PerformBacktraceAsync(cancellationToken).ConfigureAwait(false);
                Debug.Assert(!running);
            }
            CompleteStepping();
        }

        private void CompleteStepping() {
            EventHandler<ThreadEventArgs> stepComplete = StepComplete;
            if (stepComplete != null) {
                stepComplete(this, new ThreadEventArgs(MainThread));
            }
        }

        /// <summary>
        /// Adds a breakpoint in the specified file.
        /// Line number is 1 based
        /// </summary>
        public NodeBreakpoint AddBreakPoint(
            string requestedFileName,
            int requestedLineNo,
            bool enabled = true,
            BreakOn breakOn = new BreakOn(),
            string condition = null) {
            string fileName;
            int lineNo;

            SourceMapper.MapToJavaScript(requestedFileName, requestedLineNo, out fileName, out lineNo);

            return new NodeBreakpoint(this, fileName, requestedFileName, lineNo, requestedLineNo, enabled, breakOn, condition);
        }

        public async void SetExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments) {
            bool updated = false;

            if (defaultExceptionTreatment.HasValue) {
                updated |= _exceptionHandler.SetDefaultExceptionHitTreatment(defaultExceptionTreatment.Value);
            }

            if (exceptionTreatments != null) {
                updated |= _exceptionHandler.SetExceptionTreatments(exceptionTreatments);
            }

            if (updated) {
                var tokenSource = new CancellationTokenSource(_timeout);
                await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
            }
        }

        public async void ClearExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments) {
            bool updated = false;

            if (defaultExceptionTreatment.HasValue) {
                updated |= _exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakNever);
            }

            updated |= _exceptionHandler.ClearExceptionTreatments(exceptionTreatments);

            if (updated) {
                var tokenSource = new CancellationTokenSource(_timeout);
                await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
            }
        }

        public async void ClearExceptionTreatment() {
            bool updated = _exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakAlways);
            updated |= _exceptionHandler.ResetExceptionTreatments();

            if (updated) {
                var tokenSource = new CancellationTokenSource(_timeout);
                await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
            }
        }

        public string GetModuleFileName(string javaScriptFileName) {
            SourceMapping mapping = SourceMapper.MapToOriginal(javaScriptFileName, 0);
            if (mapping == null) {
                return javaScriptFileName;
            }

            string directoryName = Path.GetDirectoryName(javaScriptFileName) ?? string.Empty;
            string fileName = Path.GetFileName(mapping.FileName) ?? string.Empty;
            return Path.Combine(directoryName, fileName);
        }

        #endregion

        #region Debuggee Communcation

        // Gets a next command identifier
        private int CommandId {
            get { return Interlocked.Increment(ref _number); }
        }

        /// <summary>
        /// Gets or sets a source mapper.
        /// </summary>
        public SourceMapper SourceMapper { get; private set; }

        internal void Unregister() {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts listening for debugger communication.  Can be called after Start
        /// to give time to attach to debugger events.
        /// </summary>
        public async void StartListening() {
            _connection.Connect(_hostName, _portNumber);

            var mainThread = new NodeThread(this, MainThreadId, false);
            _threads[mainThread.Id] = mainThread;

            await GetScriptsAsync().ConfigureAwait(false);
            await SetExceptionBreakAsync().ConfigureAwait(false);
            bool running = await PerformBacktraceAsync().ConfigureAwait(false);

            // At this point we can fire events
            EventHandler<ThreadEventArgs> newThread = ThreadCreated;
            if (newThread != null) {
                newThread(this, new ThreadEventArgs(mainThread));
            }

            EventHandler<ProcessLoadedEventArgs> procLoaded = ProcessLoaded;
            if (procLoaded != null) {
                procLoaded(this, new ProcessLoadedEventArgs(mainThread, running));
            }
        }

        private void OnConnectionClosed(object sender, EventArgs args) {
            Terminate(false);

            EventHandler<ThreadEventArgs> threadExited = ThreadExited;
            if (threadExited != null) {
                threadExited(this, new ThreadEventArgs(MainThread));
            }
        }

        private async Task GetScriptsAsync(CancellationToken cancellationToken = new CancellationToken()) {
            var scriptsCommand = new ScriptsCommand(CommandId, this);
            await _client.SendRequestAsync(scriptsCommand, cancellationToken).ConfigureAwait(false);

            foreach (NodeModule module in scriptsCommand.Modules) {
                AddScript(module);
            }
        }

        private void AddScript(NodeModule newModule) {
            string name = newModule.JavaScriptFileName;
            if (!string.IsNullOrEmpty(name)) {
                NodeModule module;
                if (!_mapNameToScript.TryGetValue(name, out module)) {
                    _mapNameToScript[name] = newModule;
                    _mapIdToScript[newModule.ModuleId] = newModule;

                    EventHandler<ModuleLoadedEventArgs> modLoad = ModuleLoaded;
                    if (modLoad != null) {
                        modLoad(this, new ModuleLoadedEventArgs(newModule));
                    }
                }
            }
        }

        private async Task SetExceptionBreakAsync(CancellationToken cancellationToken = new CancellationToken()) {
            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break on all
            //var breakOnAllExceptions = _defaultExceptionTreatment == ExceptionHitTreatment.BreakAlways || _exceptionTreatments.Values.Any(value => value == ExceptionHitTreatment.BreakAlways);
            //var breakOnUncaughtExceptions = !all && (_defaultExceptionTreatment != ExceptionHitTreatment.BreakNever || _exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakNever));
            bool breakOnAllExceptions = _exceptionHandler.BreakOnAllExceptions;
            const bool breakOnUncaughtExceptions = false;

            if (HasExited) {
                return;
            }

            if (_breakOnAllExceptions != breakOnAllExceptions) {
                var setExceptionBreakCommand = new SetExceptionBreakCommand(CommandId, false, breakOnAllExceptions);
                await _client.SendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);

                _breakOnAllExceptions = breakOnAllExceptions;
            }

            if (_breakOnUncaughtExceptions != breakOnUncaughtExceptions) {
                var setExceptionBreakCommand = new SetExceptionBreakCommand(CommandId, true, breakOnUncaughtExceptions);
                await _client.SendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);

                _breakOnUncaughtExceptions = breakOnUncaughtExceptions;
            }
        }

        private void OnCompileScriptEvent(object sender, CompileScriptEventArgs args) {
            AddScript(args.CompileScriptEvent.Module);
        }

        private async void OnBreakpointEvent(object sender, BreakpointEventArgs args) {
            BreakpointEvent breakpointEvent = args.BreakpointEvent;

            // Complete stepping, if no breakpoint bindings
            if (breakpointEvent.Breakpoints.Count == 0) {
                await CompleteSteppingAsync(false).ConfigureAwait(false);
                return;
            }

            //  Derive breakpoint bindings, if any
            var breakpointBindings = new List<NodeBreakpointBinding>();
            foreach (int breakpoint in args.BreakpointEvent.Breakpoints) {
                NodeBreakpointBinding nodeBreakpointBinding;
                if (_breakpointBindings.TryGetValue(breakpoint, out nodeBreakpointBinding)) {
                    breakpointBindings.Add(nodeBreakpointBinding);
                }
            }

            // Process break for breakpoint bindings, if any
            if (!await ProcessBreakpointBreakAsync(breakpointBindings, false, false).ConfigureAwait(false)) {
                await AutoResumeAsync(false).ConfigureAwait(false);
            }
        }

        private async Task<bool> ProcessBreakpointBreakAsync(
            List<NodeBreakpointBinding> breakpointBindings,
            bool haveCallstack,
            bool testFullyBound,
            CancellationToken cancellationToken = new CancellationToken()) {
            bool result = true;
            // Handle breakpoint(s) but no matching binding(s)
            // Indicated by non-null but empty breakpoint bindings collection
            int bindingsToProcess = breakpointBindings.Count;
            if (bindingsToProcess == 0) {
                result = false;
            }

            // Process breakpoint binding
            var hitBindings = new List<NodeBreakpointBinding>();
            Action<NodeBreakpointBinding> processBinding = async binding => {
                // Collect hit breakpoint bindings
                if (binding != null) {
                    hitBindings.Add(binding);
                }

                // Handle last processed breakpoint binding by either breaking with breakpoint hit events or calling noBreakpointsHitHandler
                if (--bindingsToProcess == 0) {
                    if (hitBindings.Count > 0) {
                        // Fire breakpoint hit event(s)
                        EventHandler<BreakpointHitEventArgs> breakpointHit = BreakpointHit;
                        if (breakpointHit != null) {
                            foreach (NodeBreakpointBinding hitBinding in hitBindings) {
                                NodeBreakpointBinding breakpointBinding = hitBinding;
                                await hitBinding.ProcessBreakpointHitAsync(cancellationToken).ConfigureAwait(false);
                                breakpointHit(this, new BreakpointHitEventArgs(breakpointBinding, MainThread));
                            }
                        }
                    } else {
                        // No breakpoints hit
                        result = false;
                    }
                }
            };

            // Process breakpoint bindings, ensuring we have callstack
            if (!haveCallstack) {
                bool running = await PerformBacktraceAsync(cancellationToken).ConfigureAwait(false);
                Debug.Assert(!running);
            }

            await ProcessBreakpointBindingsAsync(breakpointBindings, processBinding, testFullyBound, cancellationToken).ConfigureAwait(false);

            return result;
        }

        private async Task ProcessBreakpointBindingsAsync(
            IEnumerable<NodeBreakpointBinding> breakpointBindings,
            Action<NodeBreakpointBinding> processBinding,
            bool testFullyBound,
            CancellationToken cancellationToken = new CancellationToken()) {
            // Iterate over breakpoint bindings, processing them as fully bound or not
            int currentLineNo = MainThread.TopStackFrame.LineNo;
            foreach (NodeBreakpointBinding breakpointBinding in breakpointBindings) {
                // Handle normal (fully bound) breakpoint binding
                if (breakpointBinding.FullyBound) {
                    if (testFullyBound) {
                        // Process based on whether hit (based on hit count and/or condition predicates)
                        if (await breakpointBinding.TestAndProcessHitAsync()) {
                            processBinding(breakpointBinding);
                        } else {
                            processBinding(null);
                        }
                        continue;
                    }

                    processBinding(breakpointBinding);
                    continue;
                }

                // Handle fixed-up breakpoint binding
                // Rebind breakpoint
                await RemoveBreakPointAsync(breakpointBinding, cancellationToken).ConfigureAwait(false);

                NodeBreakpoint breakpoint = breakpointBinding.Breakpoint;
                Tuple<int, int?, int> result = await SetBreakpointAsync(breakpoint, cancellationToken: cancellationToken).ConfigureAwait(false);

                // Treat rebound breakpoint binding as fully bound
                NodeBreakpointBinding reboundbreakpointBinding = CreateBreakpointBinding(breakpoint, result.Item1, result.Item2, result.Item3, true);
                HandleBindBreakpointSuccess(reboundbreakpointBinding, breakpoint);

                // Handle invalid-line fixup (second bind matches current line)
                if (reboundbreakpointBinding.LineNo == currentLineNo) {
                    // Process based on whether hit (based on hit count and/or condition predicates)
                    if (await reboundbreakpointBinding.TestAndProcessHitAsync()) {
                        processBinding(reboundbreakpointBinding);
                    } else {
                        processBinding(null);
                    }
                    return;
                }

                // Handle lambda-eval fixup (second bind does not match current line)
                // Process as not hit
                processBinding(null);
            }
        }

        private async void OnExceptionEvent(object sender, ExceptionEventArgs args) {
            ExceptionEvent exception = args.ExceptionEvent;

            if (exception.ErrorNumber == null) {
                ReportException(exception);
                return;
            }

            int errorNumber = exception.ErrorNumber.Value;
            string errorCodeFromMap;
            if (_errorCodes.TryGetValue(errorNumber, out errorCodeFromMap)) {
                ReportException(exception, errorCodeFromMap);
                return;
            }

            var tokenSource = new CancellationTokenSource(_timeout);
            var lookupCommand = new LookupCommand(CommandId, _resultFactory, new[] { exception.ErrorNumber.Value });
            await _client.SendRequestAsync(lookupCommand, tokenSource.Token).ConfigureAwait(false);

            string errorCodeFromLookup = lookupCommand.Results[errorNumber][0].StringValue;
            _errorCodes[errorNumber] = errorCodeFromLookup;

            ReportException(exception, errorCodeFromLookup);
        }

        private async void ReportException(ExceptionEvent exceptionEvent, string errorCode = null) {
            string exceptionName = exceptionEvent.ExceptionName;
            if (!string.IsNullOrEmpty(errorCode)) {
                exceptionName = string.Format("{0}({1})", exceptionName, errorCode);
            }

            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break always or never
            //if (exceptionTreatment == ExceptionHitTreatment.BreakNever ||
            //    (exceptionTreatment == ExceptionHitTreatment.BreakOnUnhandled && !uncaught)) {
            ExceptionHitTreatment exceptionTreatment = _exceptionHandler.GetExceptionHitTreatment(exceptionName);
            if (exceptionTreatment == ExceptionHitTreatment.BreakNever) {
                await AutoResumeAsync(false).ConfigureAwait(false);
                return;
            }

            // We need to get the backtrace before we break, so we request the backtrace
            // and follow up with firing the appropriate event for the break
            bool running = await PerformBacktraceAsync().ConfigureAwait(false);
            Debug.Assert(!running);

            // Handle followup
            EventHandler<ExceptionRaisedEventArgs> exceptionRaised = ExceptionRaised;
            if (exceptionRaised == null) {
                return;
            }

            string description = exceptionEvent.Description;
            if (description.StartsWith("#<") && description.EndsWith(">")) {
                // Serialize exception object to get a proper description
                var tokenSource = new CancellationTokenSource(_timeout);
                var evaluateCommand = new EvaluateCommand(CommandId, _resultFactory, exceptionEvent.ExceptionId);
                await _client.SendRequestAsync(evaluateCommand, tokenSource.Token).ConfigureAwait(false);

                description = evaluateCommand.Result.StringValue;
            }

            var exception = new NodeException(exceptionName, description);
            exceptionRaised(this, new ExceptionRaisedEventArgs(MainThread, exception, exceptionEvent.Uncaught));
        }

        private async Task<int> GetCallstackDepthAsync(CancellationToken cancellationToken = new CancellationToken()) {
            var backtraceCommand = new BacktraceCommand(CommandId, _resultFactory, 0, 1);
            await _client.SendRequestAsync(backtraceCommand, cancellationToken).ConfigureAwait(false);

            return backtraceCommand.CallstackDepth;
        }

        /// <summary>
        /// Retrieves a backtrace for current execution point.
        /// </summary>
        /// <returns>Whether program execution in progress.</returns>
        private async Task<bool> PerformBacktraceAsync(CancellationToken cancellationToken = new CancellationToken()) {
            // CONSIDER:  Lazy population of callstacks
            // Given the VS Debugger UI always asks for full callstacks, we always ask Node.js for full backtraces.
            // Given the nature or Node.js code, deep callstacks are expected to be rare.
            // Although according to the V8 docs (http://code.google.com/p/v8/wiki/DebuggerProtocol) the 'backtrace'
            // request takes a 'bottom' parameter, empirically, Node.js fails requests with it set.  Here we
            // approximate 'bottom' for 'toFrame' using int.MaxValue.  Node.js silently handles toFrame depths
            // greater than the current callstack.
            var backtraceCommand = new BacktraceCommand(CommandId, _resultFactory, 0, int.MaxValue, this);
            await _client.SendRequestAsync(backtraceCommand, cancellationToken).ConfigureAwait(false);

            List<NodeStackFrame> stackFrames = backtraceCommand.StackFrames;

            // Collects results of number type which have null values and perform a lookup for actual values
            var numbersWithNullValue = new List<NodeEvaluationResult>();
            foreach (NodeStackFrame stackFrame in stackFrames) {
                numbersWithNullValue.AddRange(stackFrame.Locals.Concat(stackFrame.Parameters)
                    .Where(p => p.TypeName == NodeVariableType.Number && p.StringValue == null));
            }

            if (numbersWithNullValue.Count > 0) {
                var lookupCommand = new LookupCommand(CommandId, _resultFactory, numbersWithNullValue);
                await _client.SendRequestAsync(lookupCommand, cancellationToken).ConfigureAwait(false);

                foreach (NodeEvaluationResult targetResult in numbersWithNullValue) {
                    NodeEvaluationResult lookupResult = lookupCommand.Results[targetResult.Handle][0];
                    targetResult.StringValue = targetResult.HexValue = lookupResult.StringValue;
                }
            }

            MainThread.Frames = stackFrames;
            foreach (NodeModule module in backtraceCommand.Modules.Values) {
                AddScript(module);
            }

            return backtraceCommand.Running;
        }

        internal IList<NodeThread> GetThreads() {
            return _threads.Values.ToList();
        }

        internal async void SendStepOver(int identity) {
            DebugWriteCommand("StepOver");
            var tokenSource = new CancellationTokenSource(_timeout);
            await ContinueAndSaveSteppingAsync(SteppingKind.Over, cancellationToken: tokenSource.Token).ConfigureAwait(false);
        }

        internal async void SendStepInto(int identity) {
            DebugWriteCommand("StepInto");
            var tokenSource = new CancellationTokenSource(_timeout);
            await ContinueAndSaveSteppingAsync(SteppingKind.Into, cancellationToken: tokenSource.Token).ConfigureAwait(false);
        }

        internal async void SendStepOut(int identity) {
            DebugWriteCommand("StepOut");
            var tokenSource = new CancellationTokenSource(_timeout);
            await ContinueAndSaveSteppingAsync(SteppingKind.Out, cancellationToken: tokenSource.Token).ConfigureAwait(false);
        }

        internal async void SendResumeThread(int threadId) {
            DebugWriteCommand("ResumeThread");

            // Handle load complete resume
            if (!_loadCompleteHandled) {
                _loadCompleteHandled = true;
                _handleEntryPointHit = true;

                // Handle breakpoint binding at entrypoint
                // Attempt to fire breakpoint hit event without actually resuming
                NodeStackFrame topFrame = MainThread.TopStackFrame;
                int breakLineNo = topFrame.LineNo;
                string breakFileName = topFrame.FileName.ToLower();
                NodeModule breakModule = GetModuleForFilePath(breakFileName);

                var breakpointBindings = new List<NodeBreakpointBinding>();
                foreach (NodeBreakpointBinding breakpointBinding in _breakpointBindings.Values) {
                    if (breakpointBinding.Enabled && breakpointBinding.LineNo == breakLineNo &&
                        GetModuleForFilePath(breakpointBinding.FileName) == breakModule) {
                        breakpointBindings.Add(breakpointBinding);
                    }
                }

                if (breakpointBindings.Count > 0) {
                    // Delegate to ProcessBreak() which knows how to correctly
                    // fire breakpoint hit events for given breakpoint bindings and current backtrace
                    if (!await ProcessBreakpointBreakAsync(breakpointBindings, true, true).ConfigureAwait(false)) {
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
            if (HandleEntryPointHit()) {
                return;
            }

            // Handle tracepoint (auto-resumed "when hit" breakpoint) resume during stepping
            await AutoResumeAsync(true).ConfigureAwait(false);
        }

        private bool HandleEntryPointHit() {
            if (_handleEntryPointHit) {
                _handleEntryPointHit = false;
                EventHandler<ThreadEventArgs> entryPointHit = EntryPointHit;
                if (entryPointHit != null) {
                    entryPointHit(this, new ThreadEventArgs(MainThread));
                    return true;
                }
            }
            return false;
        }

        public void SendClearStepping(int threadId) {
            DebugWriteCommand("ClearStepping");
            //throw new NotImplementedException();
        }

        public async void Detach() {
            DebugWriteCommand("Detach");

            // Disconnect request has no response
            var tokenSource = new CancellationTokenSource(_timeout);
            var disconnectCommand = new DisconnectCommand(CommandId);
            await _client.SendRequestAsync(disconnectCommand, tokenSource.Token).ConfigureAwait(false);

            _connection.Close();
        }

        public async Task<NodeBreakpointBinding> BindBreakpointAsync(NodeBreakpoint breakpoint, CancellationToken cancellationToken = new CancellationToken()) {
            Tuple<int, int?, int> result = await SetBreakpointAsync(breakpoint, cancellationToken: cancellationToken).ConfigureAwait(false);

            bool fullyBound = (result.Item2.HasValue && result.Item3 == breakpoint.LineNo);
            NodeBreakpointBinding breakpointBinding = CreateBreakpointBinding(breakpoint, result.Item1, result.Item2, result.Item3, fullyBound);

            // Fully bound (normal case)
            // Treat as success
            if (fullyBound) {
                HandleBindBreakpointSuccess(breakpointBinding, breakpoint);
                return breakpointBinding;
            }

            // Not fully bound, with predicate
            // Rebind without predicate
            if (breakpoint.HasPredicate) {
                await RemoveBreakPointAsync(breakpointBinding, cancellationToken).ConfigureAwait(false);
                result = await SetBreakpointAsync(breakpoint, true, cancellationToken).ConfigureAwait(false);

                Debug.Assert(!(result.Item2.HasValue && result.Item3 == breakpoint.LineNo));
                CreateBreakpointBinding(breakpoint, result.Item1, result.Item2, result.Item3, false);
            }

            // Not fully bound, without predicate
            // Treat as failure (for now)
            HandleBindBreakpointFailure(breakpoint);
            return null;
        }

        internal async Task<Tuple<int, int?, int>> SetBreakpointAsync(
            NodeBreakpoint breakpoint,
            bool withoutPredicate = false,
            CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand(String.Format("Set Breakpoint"));

            // Try to find module
            NodeModule module = GetModuleForFilePath(breakpoint.FileName);

            var setBreakpointCommand = new SetBreakpointCommand(CommandId, module, breakpoint, withoutPredicate);
            await _client.SendRequestAsync(setBreakpointCommand, cancellationToken).ConfigureAwait(false);

            return new Tuple<int, int?, int>(setBreakpointCommand.BreakpointId, setBreakpointCommand.ScriptId, setBreakpointCommand.LineNo);
        }

        private NodeBreakpointBinding CreateBreakpointBinding(NodeBreakpoint breakpoint, int breakpointId, int? scriptId, int lineNo, bool fullyBound) {
            NodeBreakpointBinding breakpointBinding = breakpoint.CreateBinding(lineNo, breakpointId, scriptId, fullyBound);
            _breakpointBindings[breakpointId] = breakpointBinding;
            return breakpointBinding;
        }

        private void HandleBindBreakpointSuccess(NodeBreakpointBinding breakpointBinding, NodeBreakpoint breakpoint) {
            EventHandler<BreakpointBindingEventArgs> breakpointBound = BreakpointBound;
            if (breakpointBound != null) {
                breakpointBound(this, new BreakpointBindingEventArgs(breakpoint, breakpointBinding));
            }
        }

        private void HandleBindBreakpointFailure(NodeBreakpoint breakpoint) {
            EventHandler<BreakpointBindingEventArgs> breakpointBindFailure = BreakpointBindFailure;
            if (breakpointBindFailure != null) {
                breakpointBindFailure(this, new BreakpointBindingEventArgs(breakpoint, null));
            }
        }

        internal NodeModule GetModuleForFilePath(string filePath) {
            NodeModule module;
            _mapNameToScript.TryGetValue(filePath, out module);
            return module;
        }

        internal async Task UpdateBreakpointBindingAsync(
            int breakpointId,
            bool? enabled = null,
            string condition = null,
            int? ignoreCount = null,
            bool validateSuccess = false,
            CancellationToken cancellationToken = new CancellationToken()) {
            // DEVNOTE: Calling UpdateBreakpointBinding() on the debug thread with validateSuccess == true will deadlock
            // and timout, causing both the followup handler to be called before confirmation of success (or failure), and
            // a return of false (failure).
            DebugWriteCommand(String.Format("Update Breakpoint binding"));

            var changeBreakPointCommand = new ChangeBreakpointCommand(CommandId, breakpointId, enabled, condition, ignoreCount);
            await _client.SendRequestAsync(changeBreakPointCommand, cancellationToken).ConfigureAwait(false);
        }

        internal async Task<int?> GetBreakpointHitCountAsync(int breakpointId, CancellationToken cancellationToken = new CancellationToken()) {
            int? hitCount = null;

            var listBreakpointsCommand = new ListBreakpointsCommand(CommandId);
            await _client.SendRequestAsync(listBreakpointsCommand, cancellationToken).ConfigureAwait(false);

            foreach (var breakpoint in listBreakpointsCommand.Breakpoints) {
                if (breakpoint.Key == breakpointId) {
                    hitCount = breakpoint.Value;
                    break;
                }
            }

            return hitCount;
        }

        internal async Task<NodeEvaluationResult> ExecuteTextAsync(
            string text,
            NodeStackFrame nodeStackFrame,
            CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Execute Text Async");

            var evaluateCommand = new EvaluateCommand(CommandId, _resultFactory, text, nodeStackFrame);
            await _client.SendRequestAsync(evaluateCommand, cancellationToken).ConfigureAwait(false);

            return evaluateCommand.Result;
        }

        internal async Task<NodeEvaluationResult> SetVariableValueAsync(
            NodeStackFrame stackFrame,
            string name,
            string value,
            CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Set Variable Value");

            if (_connection.NodeVersion >= _nodeSetVariableValueVersion) {
                var evaluateValueCommand = new EvaluateCommand(CommandId, _resultFactory, value, stackFrame);
                await _client.SendRequestAsync(evaluateValueCommand, cancellationToken).ConfigureAwait(false);

                int handle = evaluateValueCommand.Result.Handle;
                var setVariableValuecommand = new SetVariableValueCommand(CommandId, _resultFactory, stackFrame, name, handle);
                await _client.SendRequestAsync(setVariableValuecommand, cancellationToken).ConfigureAwait(false);
                return setVariableValuecommand.Result;
            }

            string expression = string.Format("{0} = {1}", name, value);
            var evaluateCommand = new EvaluateCommand(CommandId, _resultFactory, expression, stackFrame);
            await _client.SendRequestAsync(evaluateCommand, cancellationToken).ConfigureAwait(false);

            return evaluateCommand.Result;
        }

        internal async Task<List<NodeEvaluationResult>> EnumChildrenAsync(NodeEvaluationResult nodeEvaluationResult, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Enum Children");

            var lookupCommand = new LookupCommand(CommandId, _resultFactory, new List<NodeEvaluationResult> { nodeEvaluationResult });
            await _client.SendRequestAsync(lookupCommand, cancellationToken).ConfigureAwait(false);

            return lookupCommand.Results[nodeEvaluationResult.Handle];
        }

        internal async Task RemoveBreakPointAsync(NodeBreakpointBinding breakpointBinding, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Remove Breakpoint");

            // Perform remove idempotently, as remove may be called in response to BreakpointUnound event
            if (breakpointBinding.Unbound) {
                return;
            }

            int breakpointId = breakpointBinding.BreakpointId;
            if (_connection.Connected) {
                var clearBreakpointsCommand = new ClearBreakpointCommand(CommandId, breakpointId);
                await _client.SendRequestAsync(clearBreakpointsCommand, cancellationToken).ConfigureAwait(false);
            }

            NodeBreakpoint breakpoint = breakpointBinding.Breakpoint;
            _breakpointBindings.Remove(breakpointId);
            breakpoint.RemoveBinding(breakpointBinding);
            breakpointBinding.Unbound = true;

            EventHandler<BreakpointBindingEventArgs> breakpointUnbound = BreakpointUnbound;
            if (breakpointUnbound != null) {
                breakpointUnbound(this, new BreakpointBindingEventArgs(breakpoint, breakpointBinding));
            }
        }

        internal bool SetLineNumber(NodeStackFrame nodeStackFrame, int lineNo) {
            DebugWriteCommand("Set Line Number");
            throw new NotImplementedException();
        }

        internal async Task<string> GetScriptTextAsync(int moduleId, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("GetScriptText: " + moduleId);

            var scriptsCommand = new ScriptsCommand(CommandId, this, true, moduleId);
            await _client.SendRequestAsync(scriptsCommand, cancellationToken).ConfigureAwait(false);

            if (scriptsCommand.Modules.Count == 0) {
                return null;
            }

            return scriptsCommand.Modules[0].Source;
        }

        internal async Task<bool> TestPredicateAsync(string expression, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("TestPredicate: " + expression);

            string predicateExpression = string.Format("Boolean({0})", expression);
            var evaluateCommand = new EvaluateCommand(CommandId, _resultFactory, predicateExpression);
            await _client.SendRequestAsync(evaluateCommand, cancellationToken).ConfigureAwait(false);

            return evaluateCommand.Result != null &&
                   evaluateCommand.Result.Type == NodeExpressionType.Boolean &&
                   evaluateCommand.Result.StringValue == "true";
        }

        #endregion

        #region Debugging Events

        /// <summary>
        /// Fired when the process has started and is broken into the debugger, but before any user code is run.
        /// </summary>
        public event EventHandler<ProcessLoadedEventArgs> ProcessLoaded;

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

        public event EventHandler<OutputEventArgs> DebuggerOutput {
            add { }
            remove { }
        }

        #endregion

        internal void Close() {
        }

        internal static void FixupForRunAndWait(bool waitOnAbnormal, bool waitOnNormal, ProcessStartInfo psi) {
            if (waitOnAbnormal || waitOnNormal) {
                string args = "/c \"\"" + psi.FileName + "\" " + psi.Arguments;

                if (waitOnAbnormal && waitOnNormal) {
                    args += " & pause";
                } else if (waitOnAbnormal) {
                    args += " & if errorlevel 1 pause";
                } else {
                    args += " & if not errorlevel 1 pause";
                }
                args += "\"";
                var binaryType = NativeMethods.GetBinaryType(psi.FileName);
                if (binaryType == System.Reflection.ProcessorArchitecture.Amd64) {
                    // VS wants the binary we launch to match in bitness to the Node.exe
                    // we're requesting it to launch.
                    psi.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative", "cmd.exe");
                } else {
                    psi.FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe");
                }
                psi.Arguments = args;
            }
        }

        #region IDisposable

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                //Clean up managed resources
                Terminate();
            }
        }

        #endregion
    }
}
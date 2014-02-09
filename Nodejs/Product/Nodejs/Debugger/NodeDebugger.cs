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
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.NodejsTools.Debugger.Serialization;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Handles all interactions with a Node process which is being debugged.
    /// </summary>
    class NodeDebugger : IDisposable, ISourceMapper {
        public readonly int MainThreadId = 1;
        private readonly Dictionary<int, NodeBreakpointBinding> _breakpointBindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly Dictionary<int, string> _errorCodes = new Dictionary<int, string>();
        private readonly Dictionary<string, SourceMap> _generatedFileToSourceMap = new Dictionary<string, SourceMap>(StringComparer.OrdinalIgnoreCase);
        private readonly string _hostName;
        private readonly Dictionary<int, NodeModule> _mapIdToScript = new Dictionary<int, NodeModule>();
        private readonly Dictionary<string, NodeModule> _mapNameToScript = new Dictionary<string, NodeModule>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, JavaScriptSourceMapInfo> _originalFileToSourceMap = new Dictionary<string, JavaScriptSourceMapInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly ushort _portNumber;
        private readonly Dictionary<int, NodeThread> _threads = new Dictionary<int, NodeThread>();
        private bool _attached;
        private bool _breakOnAllExceptions;
        private bool _breakOnUncaughtExceptions;
        private IDebuggerClient _client;
        private IDebuggerConnection _connection;
        private bool _handleEntryPointHit;
        private int? _id;
        private bool _loadCompleteHandled;
        private Process _process;
        private int _steppingCallstackDepth;
        private SteppingKind _steppingMode;

        private NodeDebugger() {
            Connection = Connection ?? new DebuggerConnection(new TcpClientFactory());
            Client = Client ?? new DebuggerClient(Connection);
            ExceptionHandler = ExceptionHandler ?? new ExceptionHandler();
            CommandFactory = CommandFactory ?? new CommandFactory(
                new SequentialNumberGenerator(),
                new EvaluationResultFactory());

            if (Timeout == TimeSpan.Zero) {
                Timeout = TimeSpan.FromSeconds(2);
            }
        }

        public NodeDebugger(
            string exe,
            string script,
            string dir,
            string env,
            string interpreterOptions,
            NodeDebugOptions debugOptions,
            List<string[]> dirMapping,
            bool createNodeWindow = true) : this() {
            // Select debugger port for a local connection
            List<int> activeConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port).ToList();
            ushort debugPort = 5858;
            if (activeConnections.Contains(debugPort)) {
                debugPort = (ushort)Enumerable.Range(new Random().Next(5859, 6000), 60000).Except(activeConnections).First();
            }

            _hostName = "localhost";
            _portNumber = debugPort;

            string allArgs = String.Format("--debug-brk={0} {1}", debugPort, script);
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
            get { return !Connection.Connected; }
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

        public void Terminate() {
            lock (this) {
                // Disconnect
                Connection.Close();

                // Fall back to using -1 for exit code if we cannot obtain one from the process
                // This is the normal case for attach where there is no process to interrogate
                int exitCode = -1;

                if (_process != null) {
                    // Cleanup process
                    Debug.Assert(!_attached);
                    try {
                        if (!_process.HasExited) {
                            _process.Kill();
                        } else {
                            exitCode = _process.ExitCode;
                        }
                    } catch {
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
        public async void BreakAll() {
            DebugWriteCommand("BreakAll");

            SuspendCommand suspendCommand = CommandFactory.CreateSuspendCommand();
            await Client.SendRequestAsync(suspendCommand).ConfigureAwait(false);

            // Handle success
            // We need to get the backtrace before we break, so we request the backtrace
            // and follow up with firing the appropriate event for the break
            bool running = await PerformBacktraceAsync().ConfigureAwait(false);
            Debug.Assert(!running);

            // Fallback to firing step complete event
            EventHandler<ThreadEventArgs> asyncBreakComplete = AsyncBreakComplete;
            if (asyncBreakComplete != null) {
                asyncBreakComplete(this, new ThreadEventArgs(MainThread));
            }
        }

        [Conditional("DEBUG")]
        private void DebugWriteCommand(string commandName) {
            DebugWriteLine("Node Debugger Sending Command " + commandName);
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
            await ContinueAsync().ConfigureAwait(false);
        }

        private Task ContinueAsync(SteppingKind steppingKind, bool resetSteppingMode = true, int stepCount = 1) {
            if (resetSteppingMode) {
                _steppingMode = steppingKind;
                _steppingCallstackDepth = MainThread.CallstackDepth;
            }

            return ContinueAsync(steppingKind, stepCount);
        }

        private async Task ContinueAsync(SteppingKind stepping = SteppingKind.None, int stepCount = 1) {
            // Ensure load complete and entrypoint breakpoint/tracepoint handling disabled after first real continue
            _loadCompleteHandled = true;
            _handleEntryPointHit = false;

            ContinueCommand continueCommand = CommandFactory.CreateContinueCommand(stepping, stepCount);
            await Client.SendRequestAsync(continueCommand).ConfigureAwait(false);
        }

        private Task AutoResumeAsync(bool haveCallstack) {
            // Simply continue, if not stepping
            if (_steppingMode != SteppingKind.None) {
                return AutoResumeSteppingAsync(haveCallstack);
            }

            return ContinueAsync();
        }

        private async Task AutoResumeSteppingAsync(bool haveCallstack) {
            int callstackDepth;
            if (haveCallstack) {
                // Have callstack, so get callstack depth from it
                callstackDepth = MainThread.CallstackDepth;
            } else {
                // Don't have callstack, so get callstack depth from server
                // Doing this avoids doing a full backtrace for all auto resumes
                callstackDepth = await GetCallstackDepthAsync().ConfigureAwait(false);
            }

            await AutoResumeSteppingAsync(callstackDepth, haveCallstack).ConfigureAwait(false);
        }

        private async Task AutoResumeSteppingAsync(int callstackDepth, bool haveCallstack) {
            switch (_steppingMode) {
                case SteppingKind.Over:
                    int stepCount = callstackDepth - _steppingCallstackDepth;
                    if (stepCount > 0) {
                        // Stepping over autoresumed break (in nested frame)
                        await ContinueAsync(SteppingKind.Out, false, stepCount).ConfigureAwait(false);
                        return;
                    }
                    break;
                case SteppingKind.Out:
                    stepCount = callstackDepth - _steppingCallstackDepth + 1;
                    if (stepCount > 0) {
                        // Stepping out across autoresumed break (in nested frame)
                        await ContinueAsync(SteppingKind.Out, false, stepCount).ConfigureAwait(false);
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

            await CompleteSteppingAsync(haveCallstack).ConfigureAwait(false);
        }

        private async Task CompleteSteppingAsync(bool haveCallstack) {
            // Ensure we have callstack
            if (!haveCallstack) {
                bool running = await PerformBacktraceAsync().ConfigureAwait(false);
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
            string condition = null
            ) {
            string fileName;
            int lineNo;

            MapToJavaScript(requestedFileName, requestedLineNo - 1, out fileName, out lineNo);
            lineNo++;
            var res =
                new NodeBreakpoint(
                    this,
                    fileName,
                    requestedFileName,
                    lineNo,
                    requestedLineNo,
                    enabled,
                    breakOn,
                    condition
                    );
            return res;
        }

        public async void SetExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
            ) {
            bool updated = false;

            if (defaultExceptionTreatment.HasValue) {
                updated |= ExceptionHandler.SetDefaultExceptionHitTreatment(defaultExceptionTreatment.Value);
            }

            if (exceptionTreatments != null) {
                updated |= ExceptionHandler.SetExceptionTreatments(exceptionTreatments);
            }

            if (updated) {
                var cts = new CancellationTokenSource(Timeout);
                await SetExceptionBreakAsync(cts.Token).ConfigureAwait(false);
            }
        }

        public async void ClearExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
            ) {
            bool updated = false;

            if (defaultExceptionTreatment.HasValue) {
                updated |= ExceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakNever);
            }

            updated |= ExceptionHandler.ClearExceptionThreatments(exceptionTreatments);

            if (updated) {
                var cts = new CancellationTokenSource(Timeout);
                await SetExceptionBreakAsync(cts.Token).ConfigureAwait(false);
            }
        }

        public async void ClearExceptionTreatment() {
            bool updated = ExceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakAlways);
            updated |= ExceptionHandler.ResetExceptionTreatments();

            if (updated) {
                var cts = new CancellationTokenSource(Timeout);
                await SetExceptionBreakAsync(cts.Token).ConfigureAwait(false);
            }
        }

        #endregion

        #region Debuggee Communcation

        /// <summary>
        /// Gets or sets a node command execution timeout.
        /// </summary>
        private TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets a connection to node debugger.
        /// </summary>
        public IDebuggerConnection Connection {
            get { return _connection; }
            set {
                if (_connection == value) {
                    return;
                }

                OnNodeConnectionChanged(_connection, value);
                _connection = value;
            }
        }

        /// <summary>
        /// Gets or sets a node connection client.
        /// </summary>
        public IDebuggerClient Client {
            get { return _client; }
            set {
                if (_client == value) {
                    return;
                }

                OnNodeClientChanged(_client, value);
                _client = value;
            }
        }

        /// <summary>
        /// Gets or sets a command factory.
        /// </summary>
        public ICommandFactory CommandFactory { get; set; }

        /// <summary>
        /// Gets or sets exceptions handler.
        /// </summary>
        public IExceptionHandler ExceptionHandler { get; set; }

        internal void Unregister() {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts listening for debugger communication.  Can be called after Start
        /// to give time to attach to debugger events.
        /// </summary>
        public void StartListening() {
            Connection.Connect(_hostName, _portNumber);
            ProcessConnect();
        }

        /// <summary>
        /// Handles client changes.
        /// </summary>
        /// <param name="oldClient">Old client.</param>
        /// <param name="newClient">New client.</param>
        private void OnNodeClientChanged(IDebuggerClient oldClient, IDebuggerClient newClient) {
            if (oldClient != null) {
                oldClient.BreakpointEvent -= OnBreakpointEvent;
                oldClient.CompileScriptEvent -= OnCompileScriptEvent;
                oldClient.ExceptionEvent -= OnExceptionEvent;
            }
            if (newClient != null) {
                newClient.BreakpointEvent += OnBreakpointEvent;
                newClient.CompileScriptEvent += OnCompileScriptEvent;
                newClient.ExceptionEvent += OnExceptionEvent;
            }
        }

        /// <summary>
        /// Handles connection chnages.
        /// </summary>
        /// <param name="oldConnection">Old connection.</param>
        /// <param name="newConnection">New connection.</param>
        private void OnNodeConnectionChanged(IDebuggerConnection oldConnection, IDebuggerConnection newConnection) {
            if (oldConnection != null) {
                oldConnection.ConnectionClosed -= OnConnectionClosed;
            }
            if (newConnection != null) {
                newConnection.ConnectionClosed += OnConnectionClosed;
            }
        }

        private void OnConnectionClosed(object sender, EventArgs args) {
            Terminate();
        }

        private async void ProcessConnect() {
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

        private async Task GetScriptsAsync() {
            ScriptsCommand scriptsCommand = CommandFactory.CreateScriptsCommand();
            await Client.SendRequestAsync(scriptsCommand).ConfigureAwait(false);

            foreach (NodeModule module in scriptsCommand.Modules) {
                AddScript(module);
            }
        }

        private void AddScript(NodeModule newModule) {
            string name = newModule.FileName;
            if (!string.IsNullOrEmpty(name)) {
                NodeModule module;
                if (!_mapNameToScript.TryGetValue(name, out module)) {
                    int id = newModule.ModuleId;
                    module = new NodeModule(this, id, name);
                    _mapNameToScript[name] = module;
                    _mapIdToScript[id] = module;
                    EventHandler<ModuleLoadedEventArgs> modLoad = ModuleLoaded;
                    if (modLoad != null) {
                        modLoad(this, new ModuleLoadedEventArgs(module));
                    }
                }
            }
        }

        private async Task<bool> SetExceptionBreakAsync(CancellationToken cancellationToken = default (CancellationToken)) {
            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break on all
            //var breakOnAllExceptions = _defaultExceptionTreatment == ExceptionHitTreatment.BreakAlways || _exceptionTreatments.Values.Any(value => value == ExceptionHitTreatment.BreakAlways);
            //var breakOnUncaughtExceptions = !all && (_defaultExceptionTreatment != ExceptionHitTreatment.BreakNever || _exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakNever));
            bool breakOnAllExceptions = ExceptionHandler.BreakOnAllExceptions;
            const bool breakOnUncaughtExceptions = false;

            if (HasExited) {
                return false;
            }

            if (_breakOnAllExceptions != breakOnAllExceptions) {
                SetExceptionBreakCommand setExceptionBreakCommand = CommandFactory.CreateSetExceptionBreakCommand(false, breakOnAllExceptions);
                try {
                    await Client.SendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);
                } catch (Exception) {
                    return false;
                }
                _breakOnAllExceptions = breakOnAllExceptions;
            }

            if (_breakOnUncaughtExceptions != breakOnUncaughtExceptions) {
                SetExceptionBreakCommand setExceptionBreakCommand = CommandFactory.CreateSetExceptionBreakCommand(true, breakOnUncaughtExceptions);
                try {
                    await Client.SendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);
                } catch (Exception) {
                    return false;
                }
                _breakOnUncaughtExceptions = breakOnUncaughtExceptions;
            }

            return true;
        }

        private void OnCompileScriptEvent(object sender, CompileScriptEventArgs args) {
            // Add script
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
            await ProcessBreakpointBreakAsync(breakpointBindings, () => AutoResumeAsync(false), false, false).ConfigureAwait(false);
        }

        private async Task ProcessBreakpointBreakAsync(List<NodeBreakpointBinding> breakpointBindings, Action noBreakpointsHitHandler, bool haveCallstack, bool testFullyBound) {
            // Handle breakpoint(s) but no matching binding(s)
            // Indicated by non-null but empty breakpoint bindings collection
            int bindingsToProcess = breakpointBindings.Count;
            if (bindingsToProcess == 0) {
                noBreakpointsHitHandler();
            }

            // Process breakpoint binding
            var hitBindings = new List<NodeBreakpointBinding>();
            Action<NodeBreakpointBinding> processBinding =
                binding => {
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
                                    hitBinding.ProcessBreakpointHit(
                                        () => breakpointHit(this, new BreakpointHitEventArgs(breakpointBinding, MainThread)));
                                }
                            }
                        } else {
                            // No breakpoints hit
                            noBreakpointsHitHandler();
                        }
                    }
                };

            // Process breakpoint bindings, ensuring we have callstack
            if (!haveCallstack) {
                bool running = await PerformBacktraceAsync().ConfigureAwait(false);
                Debug.Assert(!running);
            }

            await ProcessBreakpointBindingsAsync(breakpointBindings, processBinding, testFullyBound).ConfigureAwait(false);
        }

        private async Task ProcessBreakpointBindingsAsync(IEnumerable<NodeBreakpointBinding> breakpointBindings, Action<NodeBreakpointBinding> processBinding, bool testFullyBound) {
            // Iterate over breakpoint bindings, processing them as fully bound or not
            int currentLineNo = MainThread.TopStackFrame.LineNo;
            foreach (NodeBreakpointBinding breakpointBinding in breakpointBindings) {
                // Handle normal (fully bound) breakpoint binding
                if (breakpointBinding.FullyBound) {
                    if (testFullyBound) {
                        // Process based on whether hit (based on hit count and/or condition predicates)
                        breakpointBinding.TestAndProcessHit(processBinding);
                        continue;
                    }

                    processBinding(breakpointBinding);
                    continue;
                }

                // Handle fixed-up breakpoint binding
                // Rebind breakpoint
                try {
                    await RemoveBreakPointAsync(breakpointBinding).ConfigureAwait(false);
                } catch (Exception) {
                    processBinding(breakpointBinding);
                }

                NodeBreakpoint breakpoint = breakpointBinding.Breakpoint;
                Tuple<int, int?, int> result;
                try {
                    result = await SetBreakpointAsync(breakpoint).ConfigureAwait(false);
                } catch (Exception) {
                    processBinding(null);
                    return;
                }

                // Treat rebound breakpoint binding as fully bound
                NodeBreakpointBinding reboundbreakpointBinding = CreateBreakpointBinding(breakpoint, result.Item1, result.Item2, result.Item3, true);
                HandleBindBreakpointSuccess(reboundbreakpointBinding, breakpoint);

                // Handle invalid-line fixup (second bind matches current line)
                if (reboundbreakpointBinding.LineNo == currentLineNo) {
                    // Process based on whether hit (based on hit count and/or condition predicates)
                    reboundbreakpointBinding.TestAndProcessHit(processBinding);
                    return;
                }

                // Handle lambda-eval fixup (second bind does not match current line)
                // Process as not hit
                processBinding(null);
            }
        }

        private async void OnExceptionEvent(object sender, ExceptionEventArgs args) {
            ExceptionEvent exception = args.ExceptionEvent;

            if (exception.ErrorNumber != null) {
                int errorNumber = exception.ErrorNumber.Value;
                string errorCodeFromMap;
                if (_errorCodes.TryGetValue(errorNumber, out errorCodeFromMap)) {
                    ReportException(exception, errorCodeFromMap);
                } else {
                    LookupCommand lookupCommand = CommandFactory.CreateLookupCommand(new[] { exception.ErrorNumber.Value });
                    try {
                        await Client.SendRequestAsync(lookupCommand).ConfigureAwait(false);

                        string errorCodeFromLookup = lookupCommand.Results[errorNumber][0].StringValue;
                        _errorCodes[errorNumber] = errorCodeFromLookup;

                        ReportException(exception, errorCodeFromLookup);
                    } catch (Exception) {
                        ReportException(exception);
                    }
                }
            } else {
                ReportException(exception);
            }
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
            ExceptionHitTreatment exceptionTreatment = ExceptionHandler.GetExceptionHitTreatment(exceptionName);
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
            if (exceptionRaised != null) {
                var exception = new NodeException(exceptionName, exceptionEvent.Description);
                exceptionRaised(this, new ExceptionRaisedEventArgs(MainThread, exception, exceptionEvent.Uncaught));
            }
        }

        private async Task<int> GetCallstackDepthAsync() {
            BacktraceCommand backtraceCommand = CommandFactory.CreateBacktraceCommand(0, 1);
            await Client.SendRequestAsync(backtraceCommand).ConfigureAwait(false);
            return backtraceCommand.CallstackDepth;
        }

        /// <summary>
        /// Retrieves a backtrace for current execution point.
        /// </summary>
        /// <returns>Whether program execution in progress.</returns>
        private async Task<bool> PerformBacktraceAsync() {
            // CONSIDER:  Lazy population of callstacks
            // Given the VS Debugger UI always asks for full callstacks, we always ask Node.js for full backtraces.
            // Given the nature or Node.js code, deep callstacks are expected to be rare.
            // Although according to the V8 docs (http://code.google.com/p/v8/wiki/DebuggerProtocol) the 'backtrace'
            // request takes a 'bottom' parameter, empirically, Node.js fails requests with it set.  Here we
            // approximate 'bottom' for 'toFrame' using int.MaxValue.  Node.js silently handles toFrame depths
            // greater than the current callstack.
            BacktraceCommand backtraceCommand = CommandFactory.CreateBacktraceCommand(0, int.MaxValue, MainThread, _mapIdToScript);
            try {
                await Client.SendRequestAsync(backtraceCommand).ConfigureAwait(false);
            } catch (Exception) {
                return false;
            }

            MainThread.Frames = backtraceCommand.StackFrames.ToArray();
            return backtraceCommand.Running;
        }

        internal IList<NodeThread> GetThreads() {
            return _threads.Values.ToList();
        }

        internal async void SendStepOver(int identity) {
            DebugWriteCommand("StepOver");
            await ContinueAsync(SteppingKind.Over, 1).ConfigureAwait(false);
        }

        internal async void SendStepInto(int identity) {
            DebugWriteCommand("StepInto");
            await ContinueAsync(SteppingKind.Into, 1).ConfigureAwait(false);
        }

        internal async void SendStepOut(int identity) {
            DebugWriteCommand("StepOut");
            await ContinueAsync(SteppingKind.Out, 1).ConfigureAwait(false);
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
                    if (breakpointBinding.Enabled && breakpointBinding.LineNo == breakLineNo && GetModuleForFilePath(breakpointBinding.FileName) == breakModule) {
                        breakpointBindings.Add(breakpointBinding);
                    }
                }
                if (breakpointBindings.Count > 0) {
                    // Delegate to ProcessBreak() which knows how to correctly
                    // fire breakpoint hit events for given breakpoint bindings and current backtrace
                    await ProcessBreakpointBreakAsync(breakpointBindings, () => HandleEntryPointHit(), true, true).ConfigureAwait(false);
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
            DisconnectCommand disconnectCommand = CommandFactory.CreateDisconnectCommand();
            await Client.SendRequestAsync(disconnectCommand).ConfigureAwait(false);

            Connection.Close();
        }

        public async void BindBreakpoint(NodeBreakpoint breakpoint, Action<NodeBreakpointBinding> successHandler = null, Action failureHandler = null) {
            // Wrap failure handler
            Action wrappedFailureHandler = () => {
                HandleBindBreakpointFailure(breakpoint);

                if (failureHandler != null) {
                    failureHandler();
                }
            };

            Tuple<int, int?, int> result;
            try {
                result = await SetBreakpointAsync(breakpoint).ConfigureAwait(false);
            } catch (Exception) {
                wrappedFailureHandler();
                return;
            }

            bool fullyBound = (result.Item2.HasValue && result.Item3 == breakpoint.LineNo);
            NodeBreakpointBinding breakpointBinding = CreateBreakpointBinding(breakpoint, result.Item1, result.Item2, result.Item3, fullyBound);

            // Fully bound (normal case)
            // Treat as success
            if (fullyBound) {
                HandleBindBreakpointSuccess(breakpointBinding, breakpoint);
                if (successHandler != null) {
                    successHandler(breakpointBinding);
                }
                return;
            }

            // Not fully bound, with predicate
            // Rebind without predicate
            if (breakpoint.HasPredicate) {
                try {
                    await RemoveBreakPointAsync(breakpointBinding).ConfigureAwait(false);
                } catch (Exception) {
                    wrappedFailureHandler();
                }

                try {
                    result = await SetBreakpointAsync(breakpoint, true).ConfigureAwait(false);
                } catch (Exception) {
                    wrappedFailureHandler();
                    return;
                }

                Debug.Assert(!(result.Item2.HasValue && result.Item3 == breakpoint.LineNo));
                CreateBreakpointBinding(breakpoint, result.Item1, result.Item2, result.Item3, false);

                // Treat as failure (for now)
                wrappedFailureHandler();
            }

            // Not fully bound, without predicate
            // Treat as failure (for now)
            wrappedFailureHandler();
        }

        internal async Task<Tuple<int, int?, int>> SetBreakpointAsync(NodeBreakpoint breakpoint, bool withoutPredicate = false) {
            DebugWriteCommand(String.Format("Set Breakpoint"));

            // Try to find module
            NodeModule module = GetModuleForFilePath(breakpoint.FileName);

            SetBreakpointCommand setBreakpointCommand = CommandFactory.CreateSetBreakpointCommand(module, breakpoint, withoutPredicate);
            try {
                await Client.SendRequestAsync(setBreakpointCommand).ConfigureAwait(false);
            } catch (Exception) {
                return null;
            }

            return new Tuple<int, int?, int>(setBreakpointCommand.BreakpointId, setBreakpointCommand.ScriptId, setBreakpointCommand.LineNo);
        }

        private NodeBreakpointBinding CreateBreakpointBinding(NodeBreakpoint breakpoint, int breakpointID, int? scriptID, int lineNo, bool fullyBound) {
            NodeBreakpointBinding breakpointBinding = breakpoint.CreateBinding(lineNo, breakpointID, scriptID, fullyBound);
            _breakpointBindings[breakpointID] = breakpointBinding;
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

        internal async Task<bool> UpdateBreakpointBindingAsync(
            int breakpointId,
            bool? enabled = null,
            string condition = null,
            int? ignoreCount = null,
            Action followupHandler = null,
            bool validateSuccess = false
            ) {
            // DEVNOTE: Calling UpdateBreakpointBinding() on the debug thread with validateSuccess == true will deadlock
            // and timout, causing both the followup handler to be called before confirmation of success (or failure), and
            // a return of false (failure).

            DebugWriteCommand(String.Format("Update Breakpoint binding"));

            ChangeBreakpointCommand changeBreakPointCommand = CommandFactory.CreateChangeBreakpointCommand(breakpointId, enabled, condition, ignoreCount);
            try {
                await Client.SendRequestAsync(changeBreakPointCommand).ConfigureAwait(false);
                if (followupHandler != null) {
                    followupHandler();
                }
                return true;
            } catch (Exception) {
                // Handle failure
                if (followupHandler != null) {
                    followupHandler();
                }
                return !validateSuccess;
            }
        }

        internal async Task<int?> GetBreakpointHitCountAsync(int breakpointId) {
            int? hitCount = null;
            ListBreakpointsCommand listBreakpointsCommand = CommandFactory.CreateListBreakpointsCommand();
            try {
                await Client.SendRequestAsync(listBreakpointsCommand).ConfigureAwait(false);
            } catch (Exception) {
                return null;
            }

            foreach (var breakpoint in listBreakpointsCommand.Breakpoints) {
                if (breakpoint.Key == breakpointId) {
                    hitCount = breakpoint.Value;
                    break;
                }
            }

            return hitCount;
        }

        internal async Task<NodeEvaluationResult> ExecuteTextAsync(string text, NodeStackFrame nodeStackFrame) {
            DebugWriteCommand("ExecuteText to thread " + nodeStackFrame.Thread.Id + " " /*+ executeId*/);

            EvaluateCommand evaluateCommand = CommandFactory.CreateEvaluateCommand(text, nodeStackFrame);
            try {
                await Client.SendRequestAsync(evaluateCommand).ConfigureAwait(false);
            } catch (Exception e) {
                return new NodeEvaluationResult(e.Message, text, nodeStackFrame);
            }

            return evaluateCommand.Result;
        }

        internal async Task<NodeEvaluationResult[]> EnumChildrenAsync(NodeEvaluationResult nodeEvaluationResult) {
            DebugWriteCommand("Enum Children");

            LookupCommand lookupCommand = CommandFactory.CreateLookupCommand(new List<NodeEvaluationResult> { nodeEvaluationResult });
            try {
                await Client.SendRequestAsync(lookupCommand).ConfigureAwait(false);
            } catch (Exception) {
                return new NodeEvaluationResult[] { };
            }

            return lookupCommand.Results[nodeEvaluationResult.Handle].ToArray();
        }

        internal async Task<bool> RemoveBreakPointAsync(NodeBreakpointBinding breakpointBinding) {
            DebugWriteCommand("Remove Breakpoint");

            // Perform remove idempotently, as remove may be called in response to BreakpointUnound event
            if (breakpointBinding.Unbound) {
                return true;
            }

            int breakpointId = breakpointBinding.BreakpointID;
            ClearBreakpointCommand clearBreakpointsCommand = CommandFactory.CreateClearBreakpointsCommand(breakpointId);

            try {
                await Client.SendRequestAsync(clearBreakpointsCommand).ConfigureAwait(false);
            } catch (Exception) {
                return false;
            }

            NodeBreakpoint breakpoint = breakpointBinding.Breakpoint;
            _breakpointBindings.Remove(breakpointId);
            breakpoint.RemoveBinding(breakpointBinding);
            breakpointBinding.Unbound = true;

            EventHandler<BreakpointBindingEventArgs> breakpointUnbound = BreakpointUnbound;
            if (breakpointUnbound != null) {
                breakpointUnbound(this, new BreakpointBindingEventArgs(breakpoint, breakpointBinding));
            }

            return true;
        }

        internal bool SetLineNumber(NodeStackFrame nodeStackFrame, int lineNo) {
            DebugWriteCommand("Set Line Number");
            throw new NotImplementedException();
        }

        internal async Task<string> GetScriptTextAsync(int moduleId) {
            DebugWriteCommand("GetScriptText: " + moduleId);

            ScriptsCommand scriptsCommand = CommandFactory.CreateScriptsCommand(true, moduleId);
            var cts = new CancellationTokenSource(Timeout);
            try {
                await Client.SendRequestAsync(scriptsCommand, cts.Token).ConfigureAwait(false);
            } catch (Exception) {
                return null;
            }

            if (scriptsCommand.Modules.Count == 0) {
                return null;
            }

            return scriptsCommand.Modules[0].Source;
        }

        internal async Task<bool> TestPredicateAsync(string expression) {
            DebugWriteCommand("TestPredicate: " + expression);

            string predicateExpression = string.Format("Boolean({0})", expression);
            EvaluateCommand evaluateCommand = CommandFactory.CreateEvaluateCommand(predicateExpression);

            try {
                await Client.SendRequestAsync(evaluateCommand).ConfigureAwait(false);
            } catch (Exception) {
                return false;
            }

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

        public event EventHandler<ThreadEventArgs> ThreadExited {
            add { }
            remove { }
        }

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

        #region Source Map Support

        /// <summary>
        /// Gets a source mapping for the given filename.  Line numbers are zero based.
        /// </summary>
        public SourceMapping MapToOriginal(string filename, int line) {
            JavaScriptSourceMapInfo mapInfo;
            if (!_originalFileToSourceMap.TryGetValue(filename, out mapInfo)) {
                if (File.Exists(filename)) {
                    string[] contents = File.ReadAllLines(filename);
                    const string marker = "# sourceMappingURL=";
                    int markerStart;
                    string markerLine = contents.Reverse().FirstOrDefault(x => x.IndexOf(marker, StringComparison.Ordinal) != -1);
                    if (markerLine != null && (markerStart = markerLine.IndexOf(marker, StringComparison.Ordinal)) != -1) {
                        string sourceMapFilename = markerLine.Substring(markerStart + marker.Length).Trim();
                        if (!File.Exists(sourceMapFilename)) {
                            sourceMapFilename = Path.Combine(Path.GetDirectoryName(filename) ?? string.Empty, Path.GetFileName(sourceMapFilename));
                        }

                        if (File.Exists(sourceMapFilename)) {
                            try {
                                _originalFileToSourceMap[filename] = mapInfo = new JavaScriptSourceMapInfo(new SourceMap(new StreamReader(sourceMapFilename)), contents);
                            } catch (InvalidOperationException) {
                                _originalFileToSourceMap[filename] = null;
                            } catch (NotSupportedException) {
                                _originalFileToSourceMap[filename] = null;
                            }
                        }
                    }
                }
            }
            if (mapInfo != null) {
                SourceMapping mapping;
                int column = 0;
                if (line < mapInfo.Lines.Length) {
                    string lineText = mapInfo.Lines[line];
                    // map to the 1st non-whitespace character on the line
                    // This ensures we get the correct line number, mapping to column 0
                    // can give us the previous line.
                    if (!String.IsNullOrWhiteSpace(lineText)) {
                        for (; column < lineText.Length; column++) {
                            if (!Char.IsWhiteSpace(lineText[column])) {
                                break;
                            }
                        }
                    }
                }
                if (mapInfo.Map.TryMapPoint(line, column, out mapping)) {
                    return mapping;
                }
            }
            return null;
        }

        /// <summary>
        /// Maps a line number from the original code to the generated JavaScript.
        /// Line numbers are zero based.
        /// </summary>
        internal void MapToJavaScript(string requestedFileName, int requestedLineNo, out string fileName, out int lineNo) {
            fileName = requestedFileName;
            lineNo = requestedLineNo;
            SourceMap sourceMap = GetSourceMap(requestedFileName);

            if (sourceMap != null) {
                SourceMapping result;
                if (sourceMap.TryMapPointBack(requestedLineNo, 0, out result)) {
                    lineNo = result.Line;
                    fileName = Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, result.FileName);
                    Debug.WriteLine("Mapped breakpoint from {0} {1} to {2} {3}", requestedFileName, requestedLineNo, fileName, lineNo);
                }
            }
        }

        private SourceMap GetSourceMap(string fileName) {
            SourceMap sourceMap;
            if (!_generatedFileToSourceMap.TryGetValue(fileName, out sourceMap)) {
                // see if we are using source maps for this file.
                if (!String.Equals(Path.GetExtension(fileName), NodejsConstants.FileExtension, StringComparison.OrdinalIgnoreCase)) {
                    string baseFile = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length);
                    if (File.Exists(baseFile + ".js") && File.Exists(baseFile + ".js.map")) {
                        // we're using source maps...
                        try {
                            _generatedFileToSourceMap[fileName] = sourceMap = new SourceMap(new StreamReader(baseFile + ".js.map"));
                        } catch (NotSupportedException) {
                            _generatedFileToSourceMap[fileName] = null;
                        } catch (InvalidOperationException) {
                            _generatedFileToSourceMap[fileName] = null;
                        }
                    } else {
                        _generatedFileToSourceMap[fileName] = null;
                    }
                }
            }
            return sourceMap;
        }

        private class JavaScriptSourceMapInfo {
            public readonly string[] Lines;
            public readonly SourceMap Map;

            public JavaScriptSourceMapInfo(SourceMap map, string[] lines) {
                Map = map;
                Lines = lines;
            }
        }

        #endregion

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
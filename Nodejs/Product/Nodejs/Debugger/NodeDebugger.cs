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
using Microsoft.NodejsTools.Logging;
using Microsoft.NodejsTools.SourceMapping;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Handles all interactions with a Node process which is being debugged.
    /// </summary>
    sealed class NodeDebugger : IDisposable {
        public readonly int MainThreadId = 1;
        private readonly Dictionary<int, NodeBreakpointBinding> _breakpointBindings = new Dictionary<int, NodeBreakpointBinding>();
        private readonly IDebuggerClient _client;
        private readonly IDebuggerConnection _connection;
        private readonly Uri _debuggerEndpointUri;
        private readonly Dictionary<int, string> _errorCodes = new Dictionary<int, string>();
        private readonly ExceptionHandler _exceptionHandler;
        private readonly Dictionary<string, NodeModule> _modules = new Dictionary<string, NodeModule>(StringComparer.OrdinalIgnoreCase);
        private readonly EvaluationResultFactory _resultFactory;
        private readonly SourceMapper _sourceMapper;
        private readonly Dictionary<int, NodeThread> _threads = new Dictionary<int, NodeThread>();
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private bool _attached;
        private bool _breakOnAllExceptions;
        private bool _breakOnUncaughtExceptions;
        private int _commandId;
        private IFileNameMapper _fileNameMapper;
        private bool _handleEntryPointHit;
        private int? _id;
        private bool _loadCompleteHandled;
        private NodeProcess _process;
        private int _steppingCallstackDepth;
        private SteppingKind _steppingMode;

        private NodeDebugger() {
            _connection = new DebuggerConnection(new NetworkClientFactory());
            _connection.ConnectionClosed += OnConnectionClosed;

            _client = new DebuggerClient(_connection);
            _client.BreakpointEvent += OnBreakpointEvent;
            _client.CompileScriptEvent += OnCompileScriptEvent;
            _client.ExceptionEvent += OnExceptionEvent;

            _resultFactory = new EvaluationResultFactory();
            _exceptionHandler = new ExceptionHandler();
            _sourceMapper = new SourceMapper();
            _fileNameMapper = new LocalFileNameMapper();
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
            : this() {
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

            _debuggerEndpointUri = new UriBuilder { Scheme = "tcp", Host = "localhost", Port = debuggerPortOrDefault }.Uri;

            // Node usage: node [options] [ -e script | script.js ] [arguments]
            string allArgs = String.Format(
                "--debug-brk={0} {1} {2}",
                debuggerPortOrDefault,
                interpreterOptions,
                script
            );

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

            _process = new NodeProcess(
                psi,
                debugOptions.HasFlag(NodeDebugOptions.WaitOnAbnormalExit),
                debugOptions.HasFlag(NodeDebugOptions.WaitOnNormalExit),
                true);
        }

        public NodeDebugger(Uri debuggerEndpointUri, int id)
            : this() {
            _debuggerEndpointUri = debuggerEndpointUri;
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

        /// <summary>
        /// Gets or sets a value indicating whether executed remote debugging process.
        /// </summary>
        public bool IsRemote { get; set; }

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
        /// Terminates Node.js process.
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
            await TrySendRequestAsync(suspendCommand, tokenSource.Token).ConfigureAwait(false);

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

        internal bool IsRunning() {
            var backtraceCommand = new BacktraceCommand(CommandId, _resultFactory, fromFrame: 0, toFrame: 1);
            var tokenSource = new CancellationTokenSource(_timeout);
            if (TrySendRequestAsync(backtraceCommand, tokenSource.Token).GetAwaiter().GetResult()) {
                return backtraceCommand.Running;
            }
            return false;
        }

        private void DebugWriteCommand(string commandName) {
            LiveLogger.WriteLine("NodeDebugger Called " + commandName);
        }

        /// <summary>
        /// Resumes the process.
        /// </summary>
        public void Resume() {
            DebugWriteCommand("Resume");
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                var tokenSource = new CancellationTokenSource(_timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.None, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
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
            await TrySendRequestAsync(continueCommand, cancellationToken).ConfigureAwait(false);
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
                    LiveLogger.WriteLine("Unexpected SteppingMode: {0}", _steppingMode);
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

            EventHandler<ThreadEventArgs> stepComplete = StepComplete;
            if (stepComplete != null) {
                stepComplete(this, new ThreadEventArgs(MainThread));
            }
        }

        /// <summary>
        /// Adds a breakpoint in the specified file.
        /// </summary>
        public NodeBreakpoint AddBreakpoint(string fileName, int line, int column, bool enabled = true, BreakOn breakOn = new BreakOn(), string condition = null) {
            var target = new FilePosition(fileName, line, column);

            return new NodeBreakpoint(this, target, enabled, breakOn, condition);
        }

        public void SetExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
        ) {
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
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
            });
        }

        public void ClearExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
        ) {
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                bool updated = false;

                if (defaultExceptionTreatment.HasValue) {
                    updated |= _exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakNever);
                }

                updated |= _exceptionHandler.ClearExceptionTreatments(exceptionTreatments);

                if (updated) {
                    var tokenSource = new CancellationTokenSource(_timeout);
                    await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
                }
            });
        }

        public void ClearExceptionTreatment() {
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                bool updated = _exceptionHandler.SetDefaultExceptionHitTreatment(ExceptionHitTreatment.BreakNever);
                updated |= _exceptionHandler.ResetExceptionTreatments();

                if (updated) {
                    var tokenSource = new CancellationTokenSource(_timeout);
                    await SetExceptionBreakAsync(tokenSource.Token).ConfigureAwait(false);
                }
            });
        }

        #endregion

        #region Debuggee Communcation

        /// <summary>
        /// Gets a next command identifier.
        /// </summary>
        private int CommandId {
            get { return Interlocked.Increment(ref _commandId); }
        }

        /// <summary>
        /// Gets a source mapper.
        /// </summary>
        public SourceMapper SourceMapper {
            get { return _sourceMapper; }
        }

        /// <summary>
        /// Gets or sets a file name mapper.
        /// </summary>
        public IFileNameMapper FileNameMapper {
            get { return _fileNameMapper; }
            set {
                if (value != null) {
                    _fileNameMapper = value;
                }
            }
        }

        internal void Unregister() {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Updates a module content while debugging.
        /// </summary>
        /// <param name="module">Node module.</param>
        /// <returns>Operation result.</returns>
        internal async Task<bool> UpdateModuleSourceAsync(NodeModule module) {
            module.Source = File.ReadAllText(module.JavaScriptFileName);

            var changeLiveCommand = new ChangeLiveCommand(CommandId, module);

            // Check whether update was successfull
            if (!await TrySendRequestAsync(changeLiveCommand).ConfigureAwait(false) ||
                !changeLiveCommand.Updated) {
                return false;
            }

            // Make step into and update stacktrace if required
            if (changeLiveCommand.StackModified) {
                var continueCommand = new ContinueCommand(CommandId, SteppingKind.Into);
                await TrySendRequestAsync(continueCommand).ConfigureAwait(false);
                await CompleteSteppingAsync(false).ConfigureAwait(false);
            }

            return true;
        }

        /// <summary>
        /// Starts listening for debugger communication.  Can be called after Start
        /// to give time to attach to debugger events.
        /// </summary>
        public void StartListening() {
            _connection.Connect(_debuggerEndpointUri);

            var mainThread = new NodeThread(this, MainThreadId, false);
            _threads[mainThread.Id] = mainThread;

            if (!GetScriptsAsync().Wait((int)_timeout.TotalMilliseconds)) {
                throw new TimeoutException("Timed out while retrieving scripts from debuggee.");
            }

            if (!SetExceptionBreakAsync().Wait((int)_timeout.TotalMilliseconds)) {
                throw new TimeoutException("Timed out while setting up exception handling in debuggee.");
            }

            var backTraceTask = PerformBacktraceAsync();
            if (!backTraceTask.Wait((int)_timeout.TotalMilliseconds)) {
                throw new TimeoutException("Timed out while performing initial backtrace.");
            }

            // At this point we can fire events
            EventHandler<ThreadEventArgs> newThread = ThreadCreated;
            if (newThread != null) {
                newThread(this, new ThreadEventArgs(mainThread));
            }
            EventHandler<ThreadEventArgs> procLoaded = ProcessLoaded;
            if (procLoaded != null) {
                procLoaded(this, new ThreadEventArgs(MainThread));
            }

        }

        private void OnConnectionClosed(object sender, EventArgs args) {
            EventHandler<ThreadEventArgs> threadExited = ThreadExited;
            if (threadExited != null) {
                threadExited(this, new ThreadEventArgs(MainThread));
            }

            Terminate(false);
        }

        private async Task GetScriptsAsync(CancellationToken cancellationToken = new CancellationToken()) {
            var scriptsCommand = new ScriptsCommand(CommandId);
            if (await TrySendRequestAsync(scriptsCommand, cancellationToken).ConfigureAwait(false)) {
                AddModules(scriptsCommand.Modules);
            }
        }

        private void AddModules(IEnumerable<NodeModule> modules) {
            EventHandler<ModuleLoadedEventArgs> moduleLoaded = ModuleLoaded;
            if (moduleLoaded == null) {
                return;
            }

            foreach (NodeModule module in modules) {
                NodeModule newModule;
                if (GetOrAddModule(module, out newModule)) {
                    if (newModule.FileName != newModule.JavaScriptFileName) {
                        foreach (var breakpoint in _breakpointBindings) {
                            var target = breakpoint.Value.Breakpoint.Target;
                            if (target.FileName == newModule.FileName) {
                                // attempt to rebind the breakpoint
                                DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                                    await breakpoint.Value.Breakpoint.BindAsync().WaitAsync(TimeSpan.FromSeconds(2));
                                });
                            }
                        }
                    }

                    moduleLoaded(this, new ModuleLoadedEventArgs(newModule));
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
                await TrySendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);

                _breakOnAllExceptions = breakOnAllExceptions;
            }

            if (_breakOnUncaughtExceptions != breakOnUncaughtExceptions) {
                var setExceptionBreakCommand = new SetExceptionBreakCommand(CommandId, true, breakOnUncaughtExceptions);
                await TrySendRequestAsync(setExceptionBreakCommand, cancellationToken).ConfigureAwait(false);

                _breakOnUncaughtExceptions = breakOnUncaughtExceptions;
            }
        }

        private void OnCompileScriptEvent(object sender, CompileScriptEventArgs args) {
            AddModules(new[] { args.CompileScriptEvent.Module });
        }

        private void OnBreakpointEvent(object sender, BreakpointEventArgs args) {
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                BreakpointEvent breakpointEvent = args.BreakpointEvent;

                // Process breakpoint bindings, ensuring we have callstack
                bool running = await PerformBacktraceAsync().ConfigureAwait(false);
                Debug.Assert(!running);

                // Complete stepping, if no breakpoint bindings
                if (breakpointEvent.Breakpoints.Count == 0) {
                    await CompleteSteppingAsync(true).ConfigureAwait(false);
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

                // Retrieve a local module
                NodeModule module;
                GetOrAddModule(breakpointEvent.Module, out module);
                module = module ?? breakpointEvent.Module;

                // Process break for breakpoint bindings, if any
                if (!await ProcessBreakpointBreakAsync(module, breakpointBindings, false).ConfigureAwait(false)) {
                    // If we haven't reported LoadComplete yet, and don't have any matching bindings, this is the
                    // virtual breakpoint corresponding to the entry point (new since Node v0.12). We want to ignore
                    // this for the time being and not do anything - when we report LoadComplete, VS will calls us
                    // back telling us to continue, and at that point we will unfreeze the process.
                    // Otherwise, this is just some breakpoint that we don't know of, so tell it to resume running.
                    if (_loadCompleteHandled) {
                        await AutoResumeAsync(false).ConfigureAwait(false);
                    }
                }
            });
        }

        private async Task<bool> ProcessBreakpointBreakAsync(
            NodeModule brokeIn,
            IEnumerable<NodeBreakpointBinding> breakpointBindings,
            bool testFullyBound,
            CancellationToken cancellationToken = new CancellationToken()) {
            // Process breakpoint binding
            var hitBindings = new List<NodeBreakpointBinding>();

            // Iterate over breakpoint bindings, processing them as fully bound or not
            int currentLine = MainThread.TopStackFrame.Line;
            foreach (NodeBreakpointBinding breakpointBinding in breakpointBindings) {
                // Handle normal (fully bound) breakpoint binding
                if (breakpointBinding.FullyBound) {
                    if (!testFullyBound || await breakpointBinding.TestAndProcessHitAsync().ConfigureAwait(false)) {
                        hitBindings.Add(breakpointBinding);
                    }
                } else {
                    // Handle fixed-up breakpoint binding
                    // Rebind breakpoint
                    await RemoveBreakpointAsync(breakpointBinding, cancellationToken).ConfigureAwait(false);

                    NodeBreakpoint breakpoint = breakpointBinding.Breakpoint;

                    // If this breakpoint has been deleted, then do not try to rebind it after removing it from the list,
                    // and do not treat this binding as hit.
                    if (breakpoint.Deleted) {
                        continue;
                    }

                    SetBreakpointCommand result = await SetBreakpointAsync(breakpoint, cancellationToken: cancellationToken).ConfigureAwait(false);
                    
                    // Treat rebound breakpoint binding as fully bound
                    NodeBreakpointBinding reboundbreakpointBinding = CreateBreakpointBinding(breakpoint, result.BreakpointId, result.ScriptId, breakpoint.GetPosition(SourceMapper).FileName, result.Line, result.Column, true);
                    HandleBindBreakpointSuccess(reboundbreakpointBinding, breakpoint);

                    // Handle invalid-line fixup (second bind matches current line)
                    if (reboundbreakpointBinding.Target.Line == currentLine && await reboundbreakpointBinding.TestAndProcessHitAsync().ConfigureAwait(false)) {
                        hitBindings.Add(reboundbreakpointBinding);
                    }
                }
            }

            // Handle last processed breakpoint binding by breaking with breakpoint hit events
            List<NodeBreakpointBinding> matchedBindings = ProcessBindings(brokeIn.JavaScriptFileName, hitBindings).ToList();

            // Fire breakpoint hit event(s)
            EventHandler<BreakpointHitEventArgs> breakpointHit = BreakpointHit;
            foreach (NodeBreakpointBinding binding in matchedBindings) {
                await binding.ProcessBreakpointHitAsync(cancellationToken).ConfigureAwait(false);
                if (breakpointHit != null) {
                    breakpointHit(this, new BreakpointHitEventArgs(binding, MainThread));
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
        private IEnumerable<NodeBreakpointBinding> ProcessBindings(string fileName, IEnumerable<NodeBreakpointBinding> hitBindings) {
            foreach (NodeBreakpointBinding hitBinding in hitBindings) {
                string localFileName = _fileNameMapper.GetLocalFileName(fileName);
                if (string.Equals(localFileName, hitBinding.Position.FileName, StringComparison.OrdinalIgnoreCase)) {
                    yield return hitBinding;
                } else {
                    hitBinding.FixupHitCount();
                }
            }
        }

        private void OnExceptionEvent(object sender, ExceptionEventArgs args) {
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
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

                var lookupCommand = new LookupCommand(CommandId, _resultFactory, new[] { exception.ErrorNumber.Value });
                string errorCodeFromLookup = null;
                
                if (await TrySendRequestAsync(lookupCommand).ConfigureAwait(false)) {
                    errorCodeFromLookup = lookupCommand.Results[errorNumber][0].StringValue;
                    _errorCodes[errorNumber] = errorCodeFromLookup;
                }

                ReportException(exception, errorCodeFromLookup);
            });
        }

        private void ReportException(ExceptionEvent exceptionEvent, string errorCode = null) {
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
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
                    if (await TrySendRequestAsync(evaluateCommand, tokenSource.Token).ConfigureAwait(false)) {
                        description = evaluateCommand.Result.StringValue;
                    }
                }

                var exception = new NodeException(exceptionName, description);
                exceptionRaised(this, new ExceptionRaisedEventArgs(MainThread, exception, exceptionEvent.Uncaught));
            });
        }

        private async Task<int> GetCallstackDepthAsync(CancellationToken cancellationToken = new CancellationToken()) {
            var backtraceCommand = new BacktraceCommand(CommandId, _resultFactory, 0, 1, true);
            await TrySendRequestAsync(backtraceCommand, cancellationToken).ConfigureAwait(false);
            return backtraceCommand.CallstackDepth;
        }

        private IEnumerable<NodeStackFrame> GetLocalFrames(IEnumerable<NodeStackFrame> stackFrames) {
            foreach (NodeStackFrame stackFrame in stackFrames) {
                // Retrieve a local module
                NodeModule module;
                GetOrAddModule(stackFrame.Module, out module, stackFrame);
                module = module ?? stackFrame.Module;

                int line = stackFrame.Line;
                int column = stackFrame.Column;
                string functionName = stackFrame.FunctionName;

                // Map file position to original, if required
                if (module.JavaScriptFileName != module.FileName) {
                    SourceMapInfo mapping = SourceMapper.MapToOriginal(module.JavaScriptFileName, line, column);
                    if (mapping != null) {
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
        private async Task<bool> PerformBacktraceAsync(CancellationToken cancellationToken = new CancellationToken()) {
            // CONSIDER:  Lazy population of callstacks
            // Given the VS Debugger UI always asks for full callstacks, we always ask Node.js for full backtraces.
            // Given the nature or Node.js code, deep callstacks are expected to be rare.
            // Although according to the V8 docs (http://code.google.com/p/v8/wiki/DebuggerProtocol) the 'backtrace'
            // request takes a 'bottom' parameter, empirically, Node.js fails requests with it set.  Here we
            // approximate 'bottom' for 'toFrame' using int.MaxValue.  Node.js silently handles toFrame depths
            // greater than the current callstack.
            var backtraceCommand = new BacktraceCommand(CommandId, _resultFactory, 0, int.MaxValue);
            if (!await TrySendRequestAsync(backtraceCommand, cancellationToken).ConfigureAwait(false)) {
                return false;
            }

            // Add extracted modules
            AddModules(backtraceCommand.Modules.Values);

            // Add stack frames
            List<NodeStackFrame> stackFrames = GetLocalFrames(backtraceCommand.StackFrames).ToList();

            // Collects results of number type which have null values and perform a lookup for actual values
            var numbersWithNullValue = new List<NodeEvaluationResult>();
            foreach (NodeStackFrame stackFrame in stackFrames) {
                numbersWithNullValue.AddRange(stackFrame.Locals.Concat(stackFrame.Parameters)
                    .Where(p => p.TypeName == NodeVariableType.Number && p.StringValue == null));
            }

            if (numbersWithNullValue.Count > 0) {
                var lookupCommand = new LookupCommand(CommandId, _resultFactory, numbersWithNullValue);
                if (await TrySendRequestAsync(lookupCommand, cancellationToken).ConfigureAwait(false)) {
                    foreach (NodeEvaluationResult targetResult in numbersWithNullValue) {
                        NodeEvaluationResult lookupResult = lookupCommand.Results[targetResult.Handle][0];
                        targetResult.StringValue = targetResult.HexValue = lookupResult.StringValue;
                    }
                }
            }

            MainThread.Frames = stackFrames;

            return backtraceCommand.Running;
        }

        internal IList<NodeThread> GetThreads() {
            return _threads.Values.ToList();
        }

        internal void SendStepOver(int identity) {
            DebugWriteCommand("StepOver");
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                var tokenSource = new CancellationTokenSource(_timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.Over, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
        }

        internal void SendStepInto(int identity) {
            DebugWriteCommand("StepInto");
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                var tokenSource = new CancellationTokenSource(_timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.Into, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
        }

        internal void SendStepOut(int identity) {
            DebugWriteCommand("StepOut");
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                var tokenSource = new CancellationTokenSource(_timeout);
                await ContinueAndSaveSteppingAsync(SteppingKind.Out, cancellationToken: tokenSource.Token).ConfigureAwait(false);
            });
        }

        internal void SendResumeThread(int threadId) {
            DebugWriteCommand("ResumeThread");
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                // Handle load complete resume
                if (!_loadCompleteHandled) {
                    _loadCompleteHandled = true;
                    _handleEntryPointHit = true;

                    // Handle breakpoint binding at entrypoint
                    // Attempt to fire breakpoint hit event without actually resuming
                    NodeStackFrame topFrame = MainThread.TopStackFrame;
                    int currentLine = topFrame.Line;
                    string breakFileName = topFrame.Module.FileName;
                    NodeModule breakModule = GetModuleForFilePath(breakFileName);

                    var breakpointBindings = new List<NodeBreakpointBinding>();
                    foreach (NodeBreakpointBinding breakpointBinding in _breakpointBindings.Values) {
                        if (breakpointBinding.Enabled && breakpointBinding.Position.Line == currentLine &&
                            GetModuleForFilePath(breakpointBinding.Target.FileName) == breakModule) {
                            breakpointBindings.Add(breakpointBinding);
                        }
                    }

                    if (breakpointBindings.Count > 0) {
                        // Delegate to ProcessBreak() which knows how to correctly
                        // fire breakpoint hit events for given breakpoint bindings and current backtrace
                        if (!await ProcessBreakpointBreakAsync(breakModule, breakpointBindings, true).ConfigureAwait(false)) {
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
            });
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

        public void Detach() {
            DebugWriteCommand("Detach");
            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                // Disconnect request has no response
                var tokenSource = new CancellationTokenSource(_timeout);
                var disconnectCommand = new DisconnectCommand(CommandId);
                await TrySendRequestAsync(disconnectCommand, tokenSource.Token).ConfigureAwait(false);
                _connection.Close();
            });
        }

        public async Task<NodeBreakpointBinding> BindBreakpointAsync(NodeBreakpoint breakpoint, CancellationToken cancellationToken = new CancellationToken()) {
            SetBreakpointCommand result = await SetBreakpointAsync(breakpoint, cancellationToken: cancellationToken).ConfigureAwait(false);

            var position = breakpoint.GetPosition(SourceMapper);
            bool fullyBound = (result.ScriptId.HasValue && result.Line == position.Line);
            NodeBreakpointBinding breakpointBinding = CreateBreakpointBinding(breakpoint, result.BreakpointId, result.ScriptId, position.FileName, result.Line, result.Column, fullyBound);

            // Fully bound (normal case)
            // Treat as success
            if (fullyBound) {
                HandleBindBreakpointSuccess(breakpointBinding, breakpoint);
                return breakpointBinding;
            }

            // Not fully bound, with predicate
            // Rebind without predicate
            if (breakpoint.HasPredicate) {
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
            CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Set Breakpoint");

            // Try to find module
            NodeModule module = GetModuleForFilePath(breakpoint.Target.FileName);

            var setBreakpointCommand = new SetBreakpointCommand(CommandId, module, breakpoint, withoutPredicate, IsRemote, SourceMapper);
            await TrySendRequestAsync(setBreakpointCommand, cancellationToken).ConfigureAwait(false);

            return setBreakpointCommand;
        }

        private NodeBreakpointBinding CreateBreakpointBinding(NodeBreakpoint breakpoint, int breakpointId, int? scriptId, string filename, int line, int column, bool fullyBound) {
            var position = new FilePosition(filename, line, column);
            FilePosition target = position;

            SourceMapInfo mapping = SourceMapper.MapToOriginal(filename, line, column);
            if (mapping != null) {
                target = new FilePosition(breakpoint.Target.FileName, mapping.Line, mapping.Column);
            }

            NodeBreakpointBinding breakpointBinding = breakpoint.CreateBinding(target, position, breakpointId, scriptId, fullyBound);
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
            DebugWriteCommand("Update Breakpoint binding");

            var changeBreakPointCommand = new ChangeBreakpointCommand(CommandId, breakpointId, enabled, condition, ignoreCount);
            await TrySendRequestAsync(changeBreakPointCommand, cancellationToken).ConfigureAwait(false);
        }

        internal async Task<int?> GetBreakpointHitCountAsync(int breakpointId, CancellationToken cancellationToken = new CancellationToken()) {
            var listBreakpointsCommand = new ListBreakpointsCommand(CommandId);
            
            int hitCount;
            if (await TrySendRequestAsync(listBreakpointsCommand, cancellationToken).ConfigureAwait(false) &&
                listBreakpointsCommand.Breakpoints.TryGetValue(breakpointId, out hitCount)) {
                return hitCount;
            }

            return null;
        }

        internal async Task<NodeEvaluationResult> ExecuteTextAsync(
            NodeStackFrame stackFrame,
            string text,
            CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Execute Text Async");

            var evaluateCommand = new EvaluateCommand(CommandId, _resultFactory, text, stackFrame);
            await _client.SendRequestAsync(evaluateCommand, cancellationToken).ConfigureAwait(false);
            return evaluateCommand.Result;
        }

        internal async Task<NodeEvaluationResult> SetVariableValueAsync(
            NodeStackFrame stackFrame,
            string name,
            string value,
            CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Set Variable Value");

            // Create a new value
            var evaluateValueCommand = new EvaluateCommand(CommandId, _resultFactory, value, stackFrame);
            await _client.SendRequestAsync(evaluateValueCommand, cancellationToken).ConfigureAwait(false);
            int handle = evaluateValueCommand.Result.Handle;

            // Set variable value
            var setVariableValueCommand = new SetVariableValueCommand(CommandId, _resultFactory, stackFrame, name, handle);
            await _client.SendRequestAsync(setVariableValueCommand, cancellationToken).ConfigureAwait(false);
            return setVariableValueCommand.Result;
        }

        internal async Task<List<NodeEvaluationResult>> EnumChildrenAsync(NodeEvaluationResult nodeEvaluationResult, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Enum Children");

            var lookupCommand = new LookupCommand(CommandId, _resultFactory, new List<NodeEvaluationResult> { nodeEvaluationResult });
            if (!await TrySendRequestAsync(lookupCommand, cancellationToken).ConfigureAwait(false)) {
                return null;
            }

            return lookupCommand.Results[nodeEvaluationResult.Handle];
        }

        internal async Task RemoveBreakpointAsync(NodeBreakpointBinding breakpointBinding, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("Remove Breakpoint");

            // Perform remove idempotently, as remove may be called in response to BreakpointUnound event
            if (breakpointBinding.Unbound) {
                return;
            }

            int breakpointId = breakpointBinding.BreakpointId;
            if (_connection.Connected) {
                var clearBreakpointsCommand = new ClearBreakpointCommand(CommandId, breakpointId);
                await TrySendRequestAsync(clearBreakpointsCommand, cancellationToken).ConfigureAwait(false);
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

        internal async Task<string> GetScriptTextAsync(int moduleId, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("GetScriptText: " + moduleId);

            var scriptsCommand = new ScriptsCommand(CommandId, true, moduleId);
            if (!await TrySendRequestAsync(scriptsCommand, cancellationToken).ConfigureAwait(false) ||
                scriptsCommand.Modules.Count == 0) {
                return null;
            }

            return scriptsCommand.Modules[0].Source;
        }

        internal async Task<bool> TestPredicateAsync(string expression, CancellationToken cancellationToken = new CancellationToken()) {
            DebugWriteCommand("TestPredicate: " + expression);

            string predicateExpression = string.Format("Boolean({0})", expression);
            var evaluateCommand = new EvaluateCommand(CommandId, _resultFactory, predicateExpression);

            return await TrySendRequestAsync(evaluateCommand, cancellationToken).ConfigureAwait(false) &&
                   evaluateCommand.Result != null &&
                   evaluateCommand.Result.Type == NodeExpressionType.Boolean &&
                   evaluateCommand.Result.StringValue == "true";
        }

        private async Task<bool> TrySendRequestAsync(DebuggerCommand command, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                await _client.SendRequestAsync(command, cancellationToken).ConfigureAwait(false);
                return true;
            } catch (DebuggerCommandException ex) {
                var evt = DebuggerOutput;
                if (evt != null) {
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
        private bool GetOrAddModule(NodeModule module, out NodeModule value, NodeStackFrame stackFrame = null) {
            value = null;
            string javaScriptFileName = module.JavaScriptFileName;
            int? line = null, column = null;

            if (string.IsNullOrEmpty(javaScriptFileName) ||
                javaScriptFileName == NodeVariableType.UnknownModule ||
                javaScriptFileName.StartsWith("binding:")) {
                return false;
            }

            // Get local JS file name
            javaScriptFileName = FileNameMapper.GetLocalFileName(javaScriptFileName);

            // Try to get mapping for JS file
            if(stackFrame != null) {
                line = stackFrame.Line;
                column = stackFrame.Column;
            }
            string originalFileName = SourceMapper.GetOriginalFileName(javaScriptFileName, line, column);

            if (originalFileName == null) {
                module = new NodeModule(module.Id, javaScriptFileName);
            } else {
                string directoryName = Path.GetDirectoryName(javaScriptFileName) ?? string.Empty;
                string fileName = CommonUtils.GetAbsoluteFilePath(directoryName, originalFileName.Replace('/', '\\'));

                module = new NodeModule(module.Id, fileName, javaScriptFileName);
            }

            // Check whether module already exits
            if (_modules.TryGetValue(module.FileName, out value)) {
                return false;
            }

            value = module;

            // Add module
            _modules[module.FileName] = module;

            return true;
        }

        /// <summary>
        /// Gets a module for file path.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Module.</returns>
        public NodeModule GetModuleForFilePath(string filePath) {
            NodeModule module;
            _modules.TryGetValue(filePath, out module);
            return module;
        }

        #endregion

        internal void Close() {
        }



        #region IDisposable

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NodeDebugger() {
            Dispose(false);
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                //Clean up managed resources
                Terminate();
            }
        }

        #endregion
    }
}
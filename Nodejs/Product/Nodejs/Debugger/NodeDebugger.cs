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
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.NodejsTools.Debugger.Serialization;

namespace Microsoft.NodejsTools.Debugger {
    enum SteppingKind {
        None = 0,
        Over,
        Into,
        Out
    }

    enum ExceptionHitTreatment {
        BreakNever = 0,
        BreakAlways,
        BreakOnUnhandled
    }

    enum BreakOnKind {
        Always = 0,
        Equal,
        GreaterThanOrEqual,
        Mod
    }

    struct BreakOn {
        public BreakOnKind kind;
        public uint count;
        public BreakOn(BreakOnKind kind, uint count) {
            if (kind != BreakOnKind.Always && count < 1) {
                throw new ArgumentException("Invalid BreakOn count");
            }
            this.kind = kind;
            this.count = count;
        }
    }

    /// <summary>
    /// Handles all interactions with a Node process which is being debugged.
    /// </summary>
    class NodeDebugger : IDisposable {
        private Process _process;
        private bool _attached;
        private int? _id;
        private readonly Dictionary<int, NodeBreakpointBinding> _breakpointBindings = new Dictionary<int, NodeBreakpointBinding>();
        private bool _loadCompleteHandled;
        private bool _handleEntryPointTracePoint;
        private SteppingKind _steppingMode;
        private int _steppingCallstackDepth;
        private bool _resumingStepping;
        private readonly Dictionary<int, NodeThread> _threads = new Dictionary<int, NodeThread>();
        public readonly int MainThreadId = 1;
        private readonly Dictionary<string, NodeModule> _scripts = new Dictionary<string, NodeModule>(StringComparer.OrdinalIgnoreCase);
        private ExceptionHitTreatment _defaultExceptionTreatment = ExceptionHitTreatment.BreakAlways;
        private Dictionary<string, ExceptionHitTreatment> _exceptionTreatments = GetDefaultExceptionTreatments();
        private Dictionary<int, string> _errorCodes = new Dictionary<int, string>();
        private bool _breakOnAllExceptions;
        private bool _breakOnUncaughtExceptions;
        private INodeConnection _connection;

        public INodeConnection Connection {
            get {
                return _connection;
            }
            set {
                if (_connection == value) {
                    return;
                }

                if (_connection != null) {
                    _connection.SocketDisconnected -= OnSocketDisconnected;
                    _connection.NodeEvent -= OnNodeEvent;
                }

                _connection = value;

                if (_connection != null) {
                    _connection.SocketDisconnected += OnSocketDisconnected;
                    _connection.NodeEvent += OnNodeEvent;
                }
            }
        }

        public INodeResponseHandler ResponseHandler { get; set; }

        private static Dictionary<string, ExceptionHitTreatment> GetDefaultExceptionTreatments() {
            // Keep exception types in sync with those declared in ProvideDebugExceptionAttribute's in NodePackage.cs
            string[] exceptionTypes = {
                "Error",
                "Error(EACCES)",
                "Error(EADDRINUSE)",
                "Error(EADDRNOTAVAIL)",
                "Error(EAFNOSUPPORT)",
                "Error(EAGAIN)",
                "Error(EWOULDBLOCK)",
                "Error(EALREADY)",
                "Error(EBADF)",
                "Error(EBADMSG)",
                "Error(EBUSY)",
                "Error(ECANCELED)",
                "Error(ECHILD)",
                "Error(ECONNABORTED)",
                "Error(ECONNREFUSED)",
                "Error(ECONNRESET)",
                "Error(EDEADLK)",
                "Error(EDESTADDRREQ)",
                "Error(EDOM)",
                "Error(EEXIST)",
                "Error(EFAULT)",
                "Error(EFBIG)",
                "Error(EHOSTUNREACH)",
                "Error(EIDRM)",
                "Error(EILSEQ)",
                "Error(EINPROGRESS)",
                "Error(EINTR)",
                "Error(EINVAL)",
                "Error(EIO)",
                "Error(EISCONN)",
                "Error(EISDIR)",
                "Error(ELOOP)",
                "Error(EMFILE)",
                "Error(EMLINK)",
                "Error(EMSGSIZE)",
                "Error(ENAMETOOLONG)",
                "Error(ENETDOWN)",
                "Error(ENETRESET)",
                "Error(ENETUNREACH)",
                "Error(ENFILE)",
                "Error(ENOBUFS)",
                "Error(ENODATA)",
                "Error(ENODEV)",
                "Error(ENOENT)",
                "Error(ENOEXEC)",
                "Error(ENOLINK)",
                "Error(ENOLCK)",
                "Error(ENOMEM)",
                "Error(ENOMSG)",
                "Error(ENOPROTOOPT)",
                "Error(ENOSPC)",
                "Error(ENOSR)",
                "Error(ENOSTR)",
                "Error(ENOSYS)",
                "Error(ENOTCONN)",
                "Error(ENOTDIR)",
                "Error(ENOTEMPTY)",
                "Error(ENOTSOCK)",
                "Error(ENOTSUP)",
                "Error(ENOTTY)",
                "Error(ENXIO)",
                "Error(EOVERFLOW)",
                "Error(EPERM)",
                "Error(EPIPE)",
                "Error(EPROTO)",
                "Error(EPROTONOSUPPORT)",
                "Error(EPROTOTYPE)",
                "Error(ERANGE)",
                "Error(EROFS)",
                "Error(ESPIPE)",
                "Error(ESRCH)",
                "Error(ETIME)",
                "Error(ETIMEDOUT)",
                "Error(ETXTBSY)",
                "Error(EXDEV)",
                "Error(SIGHUP)",
                "Error(SIGINT)",
                "Error(SIGILL)",
                "Error(SIGABRT)",
                "Error(SIGFPE)",
                "Error(SIGKILL)",
                "Error(SIGSEGV)",
                "Error(SIGTERM)",
                "Error(SIGBREAK)",
                "Error(SIGWINCH)",
                "EvalError",
                "RangeError",
                "ReferenceError",
                "SyntaxError",
                "TypeError",
                "URIError"
            };
            string[] breakNeverTypes = { // should probably be break on unhandled when we have just my code support
                "Error(ENOENT)",
            };
            var defaultExceptionTreatments = new Dictionary<string, ExceptionHitTreatment>();
            foreach (var exceptionType in exceptionTypes) {
                defaultExceptionTreatments[exceptionType] = ExceptionHitTreatment.BreakAlways;
            }
            foreach (var exceptionType in breakNeverTypes) {
                defaultExceptionTreatments[exceptionType] = ExceptionHitTreatment.BreakNever;
            }
            return defaultExceptionTreatments;
        }

        private NodeDebugger() {
            if (ResponseHandler == null) {
                var evaluationResultFactory = new NodeEvaluationResultFactory();
                ResponseHandler = new NodeResponseHandler(evaluationResultFactory);
            }

            if (Connection == null) {
                Connection = new NodeConnection();
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

            string allArgs = "--debug-brk " + script;
            if (!string.IsNullOrEmpty(interpreterOptions)) {
                allArgs += " " + interpreterOptions;
            }

            var psi = new ProcessStartInfo(exe, allArgs);
            psi.CreateNoWindow = !createNodeWindow;
            psi.WorkingDirectory = dir;
            psi.UseShellExecute = false;
            if (env != null) {
                string[] envValues = env.Split('\0');
                foreach (var curValue in envValues) {
                    string[] nameValue = curValue.Split(new[] { '=' }, 2);
                    if (nameValue.Length == 2 && !String.IsNullOrWhiteSpace(nameValue[0])) {
                        psi.EnvironmentVariables[nameValue[0]] = nameValue[1];
                    }
                }
            }

            _process = new Process();
            _process.StartInfo = psi;
            _process.EnableRaisingEvents = true;
        }

        public NodeDebugger(string hostName, ushort portNumber, int id) : this() {
            _id = id;
            _attached = true;

            Connection = new NodeConnection(hostName, portNumber);
        }

        #region Public Process API

        public int Id {
            get {
                return _id != null ? _id.Value : _process.Id;
            }
        }

        public void Start(bool startListening = true) {
            _process.Start();
            if (startListening) {
                StartListening();
            }
        }

        private NodeThread MainThread {
            get {
                return _threads[MainThreadId];
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
                Connection.Disconnect();

                // Fall back to using -1 for exit code if we cannot obtain one from the process
                // This is the normal case for attach where there is no process to interrogate
                int exitCode =  -1;

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
                var exited = ProcessExited;
                if (exited != null) {
                    exited(this, new ProcessExitedEventArgs(exitCode));
                }
            }
        }

        public bool HasExited {
            get { return !Connection.Connected; }
        }

        /// <summary>
        /// Breaks into the process.
        /// </summary>
        public void BreakAll() {
            DebugWriteCommand("BreakAll");

            Connection.SendRequest(
                "suspend",
                null,   // args
                json => {
                    // Handle success
                    // We need to get the backtrace before we break, so we request the backtrace
                    // and follow up with firing the appropriate event for the break
                    PerformBacktrace((running) => {
                        // Handle followup
                        // Fallback to firing step complete event
                        Debug.Assert(!running);
                        var asyncBreakComplete = AsyncBreakComplete;
                        if (asyncBreakComplete != null) {
                            asyncBreakComplete(this, new ThreadEventArgs(MainThread));
                        }
                    });
                });
        }

        [Conditional("DEBUG")]
        private void DebugWriteCommand(string commandName) {
            Debug.WriteLine("Node Debugger Sending Command " + commandName);
        }

        /// <summary>
        /// Resumes the process.
        /// </summary>
        public void Resume() {
            DebugWriteCommand("Resume");

            Continue(SteppingKind.None);
        }

        private void Continue(SteppingKind steppingKind, bool resetSteppingMode = true) {
            if (resetSteppingMode) {
                _steppingMode = steppingKind;
                _steppingCallstackDepth = MainThread.Frames.Count();
                _resumingStepping = false;
            }
            Dictionary<string, object> args = null;
            switch (steppingKind) {
                case SteppingKind.Over:
                    args = new Dictionary<string, object> { { "stepaction", "next" } };
                    break;
                case SteppingKind.Into:
                    args = new Dictionary<string, object> { { "stepaction", "in" } };
                    break;
                case SteppingKind.Out:
                    args = new Dictionary<string, object> { { "stepaction", "out" } };
                    break;
                default:
                    break;
            }

            Continue(args);
        }

        private void Continue(Dictionary<string, object> args = null) {
            // Ensure load complete and entrypoint breakpoint/tracepoint handling disabled after first real continue
            _loadCompleteHandled = true;
            _handleEntryPointTracePoint = false;

            Connection.SendRequest(
                "continue",
                args,
                json => {
                    // Handle success
                    // Nothing to do
                });
        }

        private void AutoResume(bool needBacktrace = false) {
            // Continue stepping, if stepping
            if (_steppingMode != SteppingKind.None) {
                if (needBacktrace) {
                    // Get backtrace
                    // Doing this here avoids doing a backtrace for all auto resumes
                    PerformBacktrace((running) => {
                        // Handle followup
                        _resumingStepping = true;
                        CompleteStepping();
                    });
                    return;
                }

                // Have backtrace
                _resumingStepping = true;
                CompleteStepping();
                return;
            }

            // Fall back to continue, without stepping
            Continue();
        }

        private void CompleteStepping() {
            if (_resumingStepping) {
                switch (_steppingMode) {
                    // Stepping over or to tracepoint
                    case SteppingKind.Over:
                        if (MainThread.Frames.Count() > _steppingCallstackDepth) {
                            // Stepping over traceport (in nested frame)
                            Continue(SteppingKind.Out, resetSteppingMode: false);
                            return;
                        }
                        break;
                    // Stepping into or to tracepoint
                    case SteppingKind.Into:
                        break;
                    // Stepping out accross or to tracepoint
                    case SteppingKind.Out:
                        if ((MainThread.Frames.Count() + 1) > _steppingCallstackDepth) {
                            // Stepping out accross tracepoint (in nested frame)
                            Continue(SteppingKind.Out, resetSteppingMode: false);
                            return;
                        }
                        break;
                    default:
                        Debug.WriteLine(String.Format("Unexpected SteppingMode: {0}", _steppingMode));
                        break;
                }
            }

            var stepComplete = StepComplete;
            if (stepComplete != null) {
                stepComplete(this, new ThreadEventArgs(MainThread));
            }
        }

        public bool StoppedForException {
            get {
                // TODO: Implement me
                return false;
            }
        }

        public NodeBreakpoint AddBreakPoint(
            string fileName,
            int lineNo,
            bool enabled = true,
            BreakOn breakOn = new BreakOn(),
            string condition = null
            ) {
            var res =
                new NodeBreakpoint(
                    this,
                    fileName,
                    lineNo,
                    enabled,
                    breakOn,
                    condition
                );
            return res;
        }

        public void SetExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
        ) {
            bool updated = false;

            if (defaultExceptionTreatment.HasValue && (_defaultExceptionTreatment != defaultExceptionTreatment.Value)) {
                _defaultExceptionTreatment = defaultExceptionTreatment.Value;
                updated = true;
            }

            if (exceptionTreatments != null) {
                foreach (var exceptionTreatment in exceptionTreatments) {
                    ExceptionHitTreatment treatmentValue;
                    if (!_exceptionTreatments.TryGetValue(exceptionTreatment.Key, out treatmentValue) ||
                        (exceptionTreatment.Value != treatmentValue)
                    ) {
                        _exceptionTreatments[exceptionTreatment.Key] = exceptionTreatment.Value;
                        updated = true;
                    }
                }
            }

            if (updated) {
                SetExceptionBreak(synchronous: true);
            }
        }

        public void ClearExceptionTreatment(
            ExceptionHitTreatment? defaultExceptionTreatment,
            ICollection<KeyValuePair<string, ExceptionHitTreatment>> exceptionTreatments
        ) {
            bool updated = false;

            if (defaultExceptionTreatment.HasValue && (_defaultExceptionTreatment != ExceptionHitTreatment.BreakNever)) {
                _defaultExceptionTreatment = ExceptionHitTreatment.BreakNever;
                updated = true;
            }

            foreach (var exceptionTreatment in exceptionTreatments) {
                ExceptionHitTreatment treatmentValue;
                if (_exceptionTreatments.TryGetValue(exceptionTreatment.Key, out treatmentValue)) {
                    _exceptionTreatments.Remove(exceptionTreatment.Key);
                    updated = true;
                }
            }

            if (updated) {
                SetExceptionBreak(synchronous: true);
            }
        }

        public void ClearExceptionTreatment() {
            bool updated = false;

            if (_defaultExceptionTreatment != ExceptionHitTreatment.BreakAlways) {
                _defaultExceptionTreatment = ExceptionHitTreatment.BreakAlways;
                updated = true;
            }

            if (_exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakAlways)) {
                _exceptionTreatments = GetDefaultExceptionTreatments();
                updated = true;
            }

            if (updated) {
                SetExceptionBreak(synchronous: true);
            }
        }

        #endregion

        #region Debuggee Communcation

        internal void Unregister() {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts listening for debugger communication.  Can be called after Start
        /// to give time to attach to debugger events.
        /// </summary>
        public void StartListening() {
            Connection.Connect();
            ProcessConnect();
        }

        private void OnSocketDisconnected(object sender, EventArgs args) {
            Terminate();
        }

        private void OnNodeEvent(object sender, NodeEventEventArgs e) {
            ProcessEvent(e.Data);
        }

        private void ProcessConnect() {
            var mainThread = new NodeThread(this, MainThreadId, false);
            _threads[mainThread.Id] = mainThread;

            GetScripts();

            SetExceptionBreak();

            PerformBacktrace((running) => {
                // At this point we can fire events
                var newThread = ThreadCreated;
                if (newThread != null) {
                    newThread(this, new ThreadEventArgs(mainThread));
                }
                var procLoaded = ProcessLoaded;
                if (procLoaded != null) {
                    procLoaded(this, new ProcessLoadedEventArgs(mainThread, running));
                }
            });
        }

        private void GetScripts() {
            Connection.SendRequest(
                "scripts",
                null,   // args
                json => {
                    // Handle success
                    foreach (Dictionary<string, object> script in (object[])json["body"]) {
                        AddScript(script);
                    }
                }
            );
        }

        private void AddScript(Dictionary<string, object> script) {
            object nameObj;
            if (script.TryGetValue("name", out nameObj) && !string.IsNullOrEmpty((string)nameObj)) {
                var name = (string)nameObj;
                NodeModule existingModule;
                if (!_scripts.TryGetValue(name, out existingModule)) {
                    int id = (int)script["id"];
                    var newModule = _scripts[name] = new NodeModule(id, name);
                    var modLoad = ModuleLoaded;
                    if (modLoad != null) {
                        modLoad(this, new ModuleLoadedEventArgs(newModule));
                    }
                }
            }
        }

        private bool SetExceptionBreak(bool synchronous = false) {
            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break on all
            //var breakOnAllExceptions = _defaultExceptionTreatment == ExceptionHitTreatment.BreakAlways || _exceptionTreatments.Values.Any(value => value == ExceptionHitTreatment.BreakAlways);
            //var breakOnUncaughtExceptions = !all && (_defaultExceptionTreatment != ExceptionHitTreatment.BreakNever || _exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakNever));
            var breakOnAllExceptions = _defaultExceptionTreatment != ExceptionHitTreatment.BreakNever || _exceptionTreatments.Values.Any(value => value != ExceptionHitTreatment.BreakNever);
            var breakOnUncaughtExceptions = false;

            int? timeout = null;
            Func<bool> shortCircuitPredicate = null;
            if (synchronous) {
                timeout = 2000;
                shortCircuitPredicate = () => HasExited;
            }

            if (_breakOnAllExceptions != breakOnAllExceptions) {
                if (!Connection.SendRequest(
                        "setexceptionbreak",
                        new Dictionary<string, object> {
                            { "type", "all" },
                            { "enabled", breakOnAllExceptions }
                        },
                        successHandler: JsonListener => {
                            _breakOnAllExceptions = breakOnAllExceptions;
                        },
                        timeout: timeout,
                        shortCircuitPredicate: shortCircuitPredicate) &&
                    synchronous
                ) {
                    return false;
                };
            }
            if (_breakOnUncaughtExceptions != breakOnUncaughtExceptions) {
                if (!Connection.SendRequest(
                        "setexceptionbreak",
                        new Dictionary<string, object> {
                            { "type", "uncaught" },
                            { "enabled", breakOnUncaughtExceptions }
                        },
                        successHandler: JsonListener => {
                            _breakOnUncaughtExceptions = breakOnUncaughtExceptions;
                        },
                        timeout: timeout,
                        shortCircuitPredicate: shortCircuitPredicate) &&
                    synchronous
                ) {
                    return false;
                };
            }

            return true;
        }

        private void ProcessEvent(Dictionary<string, object> json) {
            switch ((string)json["event"]) {
                case "afterCompile":
                    ProcessCompile(json);
                    break;
                case "break":
                    ProcessBreak(json);
                    break;
                case "exception":
                    ProcessException(json);
                    break;
                //case "scriptCollected":
                //    GetScripts();
                //    break;
                default:
                    Debug.WriteLine(String.Format("Unknown event: {0}", (string)json["event"]));
                    break;
            }
        }

        private void ProcessCompile(Dictionary<string, object> json) {
            // Add script
            var script = (Dictionary<string, object>)(((Dictionary<string, object>)json["body"])["script"]);
            AddScript(script);
        }

        private void ProcessBreak(Dictionary<string, object> json) {
            object breakpointsObj;
            ((Dictionary<string, object>)json["body"]).TryGetValue("breakpoints", out breakpointsObj);
            object[] breakpoints = breakpointsObj as object[];

            // Handle step break
            if (breakpoints == null || breakpoints.Length == 0) {
                // We need to get the backtrace before we break, so we request the backtrace
                // and follow up with firing the appropriate event for the break
                PerformBacktrace((running) => {
                    // Handle followup
                    Debug.Assert(!running);
                    CompleteStepping();
                });
                return;
            }

            // Collect rebind pending and normal breakpoints
            List<NodeBreakpointBinding> rebindPendingBreakpointBindings = new List<NodeBreakpointBinding>();
            List<NodeBreakpointBinding> normalBreakpointBindings = new List<NodeBreakpointBinding>();
            foreach (int breakpoint in breakpoints) {
                NodeBreakpointBinding nodeBreakpointBinding;
                if (_breakpointBindings.TryGetValue(breakpoint, out nodeBreakpointBinding)) {
                    // Rebind breakpoint binding flagged as not fully bound
                    if (!nodeBreakpointBinding.FullyBound) {
                        rebindPendingBreakpointBindings.Add(nodeBreakpointBinding);
                    } else {
                        normalBreakpointBindings.Add(nodeBreakpointBinding);
                    }
                }
            }

            // Process breakpoint bindings
            // Collect hit candidates
            // Handle last processed breakpoint by either breaking with breakpoint hit events or resuming
            var bindingsToProcess = rebindPendingBreakpointBindings.Count + normalBreakpointBindings.Count;
            List<NodeBreakpointBinding> candidateBindings = new List<NodeBreakpointBinding>();
            Action<NodeBreakpointBinding> processBinding = (binding) => {
                if (binding != null) {
                    // Collect hit breakpoints
                    candidateBindings.Add(binding);
                }
                if (--bindingsToProcess == 0) {
                    // Handle last processed binding
                    var candidatesToProcess = candidateBindings.Count;
                    if (candidatesToProcess > 0) {
                        // We need to get the backtrace before we break, so we request the backtrace
                        // and follow up with firing the appropriate events for the break
                        PerformBacktrace((running) => {
                            // Handle followup
                            // Process candidate bindings
                            // Only "hit" binding if it matches current frame location
                            // It is possible for a rebound breakpoint binding to not match
                            Debug.Assert(!running);
                            bool candidateMatch = false;
                            Action processCandidate = () => {
                                if (--candidatesToProcess == 0) {
                                    // Handle last processed candidate
                                    if (!candidateMatch) {
                                        // No matching candidate, so resume
                                        Resume();
                                    }
                                }
                            };
                            var matchingLineNo = MainThread.Frames[0].LineNo;
                            var breakpointHit = BreakpointHit;
                            foreach (var candidateBreakpointBinding in candidateBindings) {
                                if (candidateBreakpointBinding.LineNo == matchingLineNo) {
                                    candidateMatch = true;
                                    candidateBreakpointBinding.ProcessBreakpointHit(
                                        () => {
                                            // Fire breakpoint hit event
                                            if (breakpointHit != null) {
                                                breakpointHit(this, new BreakpointHitEventArgs(candidateBreakpointBinding, MainThread));
                                            }
                                            processCandidate();
                                        }
                                    );
                                } else {
                                    processCandidate();
                                }
                            }
                        });
                    } else {
                        // No candidates, so resume
                        Resume();
                    }
                }
            };

            // Process rebind pending breakpoints
            foreach (var rebindPendingBreakpointBinding in rebindPendingBreakpointBindings) {
                RemoveBreakPoint(
                    rebindPendingBreakpointBinding,
                    successHandler:
                        () => {
                            BindBreakpoint(
                                rebindPendingBreakpointBinding.Breakpoint,
                                successHandler:
                                    (breakpointBinding) => {
                                        // Consider rebound breakpoint binding fully bound if bound by script id and line fixup steady state
                                        if (breakpointBinding.ScriptID.HasValue && breakpointBinding.LineNo == rebindPendingBreakpointBinding.LineNo) {
                                            breakpointBinding.FullyBound = true;
                                        }
                                        processBinding(breakpointBinding);
                                    },
                                failureHandler:
                                    () => {
                                        processBinding(null);
                                    }
                            );
                        },
                    failureHandler:
                        () => { // Failure handler
                            processBinding(rebindPendingBreakpointBinding);
                        }
                );
            }

            // Process normal breakpoints
            foreach (var normalBreakpointBinding in normalBreakpointBindings) {
                processBinding(normalBreakpointBinding);
            }
        }

        private void ProcessException(Dictionary<string, object> json) {
            var body = (Dictionary<string, object>)json["body"];
            var uncaught = (bool)body["uncaught"];

            var exceptionName = GetExceptionName(json);
            var errNo = GetExceptionCodeRef(json);
            if (errNo != null) {
                string errorCodeFromMap;
                if (_errorCodes.TryGetValue(errNo.Value, out errorCodeFromMap)) {
                    ReportException(body, uncaught, exceptionName, errorCodeFromMap);
                } else {
                    Connection.SendRequest(
                        "lookup",
                        new Dictionary<string, object> {
                            { "handles", new object[] {errNo.Value} },
                            { "includeSource", false }
                        },
                        lookupSuccessJson => {
                            var errorCodeFromLookup = ((Dictionary<string, object>)((Dictionary<string, object>)lookupSuccessJson["body"])[errNo.ToString()])["value"].ToString();
                            _errorCodes[errNo.Value] = errorCodeFromLookup;
                            ReportException(body, uncaught, exceptionName, errorCodeFromLookup);
                        },
                        lookupFailureJson => {
                            ReportException(body, uncaught, exceptionName);
                        }
                    );
                }
            } else {
                ReportException(body, uncaught, exceptionName);
            }
        }

        private void ReportException(Dictionary<string, object> body, bool uncaught, string exceptionName, string errorCode) {
            ReportException(body, uncaught, exceptionName + "(" + errorCode + ")");
        }
        
        private void ReportException(Dictionary<string, object> body, bool uncaught, string exceptionName) {
            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break always or never
            //if (exceptionTreatment == ExceptionHitTreatment.BreakNever ||
            //    (exceptionTreatment == ExceptionHitTreatment.BreakOnUnhandled && !uncaught)) {
            ExceptionHitTreatment exceptionTreatment;
            if (!_exceptionTreatments.TryGetValue(exceptionName, out exceptionTreatment)) {
                exceptionTreatment = _defaultExceptionTreatment;
            }
            if (exceptionTreatment == ExceptionHitTreatment.BreakNever) {
                AutoResume(needBacktrace: true);
                return;
            }

            // We need to get the backtrace before we break, so we request the backtrace
            // and follow up with firing the appropriate event for the break
            PerformBacktrace((running) => {
                // Handle followup
                Debug.Assert(!running);
                var exceptionRaised = ExceptionRaised;
                if (exceptionRaised != null) {
                    var exception = (Dictionary<string, object>)body["exception"];
                    var text = (string)exception["text"];
                    exceptionRaised(this, new ExceptionRaisedEventArgs(MainThread, new NodeException(exceptionName, text), uncaught));
                }
            });
        }

        private int? GetExceptionCodeRef(Dictionary<string, object> json) {
            var body = (Dictionary<string, object>)json["body"];
            var exception = (Dictionary<string, object>)body["exception"];
            object propertiesObj = null;
            if (exception.TryGetValue("properties", out propertiesObj) && propertiesObj != null) {
                var properties = (object[])propertiesObj;
                foreach (Dictionary<string, object> property in properties) {
                    if (((string)property["name"]) == "code") {
                        return (int)property["ref"];
                    }
                }
            }

            return null;
        }

        private string GetExceptionName(Dictionary<string, object> json) {
            var body = (Dictionary<string, object>)json["body"];
            var exception = (Dictionary<string, object>)body["exception"];
            var name = (string)exception["type"];
            if (name == "error" || name == "object") {
                var constructorFunction = (Dictionary<string, object>)exception["constructorFunction"];
                var constructorFunctionHandle = (int)constructorFunction["ref"];
                var refs = (object[])json["refs"];
                var refRecord = GetRefRecord(refs, constructorFunctionHandle);
                if (refRecord != null) {
                    name = (string)refRecord["name"];
                }
            }
            return name;
        }

        private void PerformBacktrace(Action<bool> followupHandler) {
            Connection.SendRequest(
                "backtrace",
                new Dictionary<string, object> { { "inlineRefs", true } },
                successHandler:
                    json => {
                        var running = (bool) json["running"];
                        if (!running) {
                            var mainThread = MainThread;
                            var jsonValue = new JsonValue(json);
                            mainThread.Frames = ResponseHandler.ProcessBacktrace(mainThread, jsonValue);
                        }
                        if (followupHandler != null) {
                            followupHandler(running);
                        }
                    }
                );
        }

        internal IList<NodeThread> GetThreads() {
            List<NodeThread> threads = new List<NodeThread>();
            foreach (var thread in _threads.Values) {
                threads.Add(thread);
            }
            return threads;
        }


        internal void SendStepOver(int identity) {
            DebugWriteCommand("StepOver");
            Continue(SteppingKind.Over);
        }

        internal void SendStepInto(int identity) {
            DebugWriteCommand("StepInto");
            Continue(SteppingKind.Into);
        }

        internal void SendStepOut(int identity) {
            DebugWriteCommand("StepOut");
            Continue(SteppingKind.Out);
        }

        internal void SendResumeThread(int threadId) {
            DebugWriteCommand("ResumeThread");

            // Handle load complete resume
            if (!_loadCompleteHandled) {
                _loadCompleteHandled = true;

                // Handle case where breakpoint at entrypoint, by firing breakpoint hit event without actually resuming
                var topFrame = MainThread.Frames.First();
                var breakLineNo = topFrame.LineNo;
                var breakFileName = topFrame.FileName.ToLower();
                var breakModule = GetModuleForFilePath(breakFileName);
                foreach (var breakpointBinding in _breakpointBindings.Values) {
                    if (breakpointBinding.Enabled && breakpointBinding.LineNo == breakLineNo && GetModuleForFilePath(breakpointBinding.FileName) == breakModule) {
                        // Handle ignore count
                        if (!breakpointBinding.TryIgnore()) {
                            // Fire breakpoint/tracepoint hit and setup to handle entrypoint tracepoint resume
                            var breakpointHit = BreakpointHit;
                            if (breakpointHit != null) {
                                breakpointHit(this, new BreakpointHitEventArgs(breakpointBinding, MainThread));
                                _handleEntryPointTracePoint = true;
                            }
                        }
                    }
                }

                if (!_handleEntryPointTracePoint) {
                    // Fall back to firing entrypoint hit event without actually resuming
                    // SDM will auto-resume on entrypoint hit for F5 launch, but not for F10/F11 launch
                    var entryPointHit = EntryPointHit;
                    if (entryPointHit != null) {
                        entryPointHit(this, new ThreadEventArgs(MainThread));
                    }
                }

                return;
            }

            // Handle tracepoint (auto-resumed "when hit" breakpoint) at entrypoint resume, by firing entrypoint hit event without actually resuming
            // If the SDM auto-resumes a tracepoint hit at the entrypoint, we need to give the SDM a chance to handle the entrypoint.
            // By first firing breakpoint hit for a breakpoint/tracepoint at the entrypoint, and then falling back to firing entrypoint hit
            // when the breakpoint is a tracepoint (auto-resumed), the breakpoint's/tracepoint's side effects will be seen, including when effectively
            // breaking at the entrypoint for F10/F11 launch.            
            if (_handleEntryPointTracePoint) {
                _handleEntryPointTracePoint = false;

                // SDM will auto-resume on entrypoint hit for F5 launch, but not for F10/F11 launch
                var entryPointHit = EntryPointHit;
                if (entryPointHit != null) {
                    entryPointHit(this, new ThreadEventArgs(MainThread));
                    return;
                }
            }

            // Handle tracepoint (auto-resumed "when hit" breakpoint) resume during stepping
            AutoResume();
        }

        public void SendClearStepping(int threadId) {
            DebugWriteCommand("ClearStepping");
            //throw new NotImplementedException();
        }

        public void Detach() {
            DebugWriteCommand("Detach");

            // Disconnect request has no response
            Connection.SendRequest("disconnect");
            Connection.Disconnect();
        }

        private string GetCaseInsensitiveRegex(string filePath, bool leafNameOnly) {
            // NOTE: There is no way to pass a regex case insensitive modifier to the Node (V8) engine
            var fileName = filePath;
            var trailing = false;
            if (leafNameOnly) {
                fileName = Path.GetFileName(filePath);
                trailing = fileName != filePath;
            }

            fileName = Regex.Escape(fileName);

            var builder = new StringBuilder();
            if (trailing) {
                builder.Append("[" + Regex.Escape(Path.DirectorySeparatorChar.ToString() + Path.AltDirectorySeparatorChar.ToString()) + "]");
            } else {
                builder.Append('^');
            }

            foreach (var ch in fileName) {
                var upper = ch.ToString().ToUpper();
                var lower =  ch.ToString().ToLower();
                if (upper != lower) {
                    builder.Append('[');
                    builder.Append(upper);
                    builder.Append(lower);
                    builder.Append(']');
                } else {
                    builder.Append(upper);
                }
            }

            builder.Append("$");
            return builder.ToString();
        }

        internal void BindBreakpoint(NodeBreakpoint breakpoint, Action<NodeBreakpointBinding> successHandler = null, Action failureHandler = null) {
            DebugWriteCommand(String.Format("Bind Breakpoint"));

            // Zero based line numbers
            var line = breakpoint.LineNo - 1;

            // Zero based column numbers
            // Special case column to avoid (line 0, column 0) which
            // Node (V8) treats specially for script loaded via require
            var column = line == 0 ? 1 : 0;

            // Compose request arguments
            var args =
                new Dictionary<string, object> { 
                    { "line", line },
                    { "column", column }
                };
            var module = GetModuleForFilePath(breakpoint.FileName);
            if (module != null) {
                args["type"] = "scriptId";
                args["target"]= module.ModuleId;
            } else {
                args["type"] = "scriptRegExp";
                args["target"] = GetCaseInsensitiveRegex(breakpoint.FileName, _attached);
            }
            if (!NodeBreakpointBinding.GetEngineEnabled(breakpoint.Enabled, breakpoint.BreakOn, 0)) {
                args["enabled"] = false;
            }
            var ignoreCount = NodeBreakpointBinding.GetEngineIgnoreCount(breakpoint.BreakOn, 0);
            if (ignoreCount > 0) {
                args["ignoreCount"] = ignoreCount;
            }
            if (!string.IsNullOrEmpty(breakpoint.Condition)) {
                args["condition"] = breakpoint.Condition;
            }

            Action<Dictionary<string, object>> responseHandler = json => {
                var success = (bool)json["success"];
                if (success) {
                    var body = (Dictionary<string, object>)json["body"];
                    var breakpointID = (int)body["breakpoint"];
                    int? scriptID = null;
                    if (module != null) {
                        scriptID = module.ModuleId;
                    }

                    // Handle breakpoint actual location fixup
                    var lineNo = breakpoint.LineNo;
                    object actualLocationsObject;
                    if (body.TryGetValue("actual_locations", out actualLocationsObject) && actualLocationsObject != null) {
                        var actualLocations = (object[])actualLocationsObject;
                        if (actualLocations.Length > 0) {
                            Debug.Assert(actualLocations.Length == 1);
                            var actualLocation = (int)((Dictionary<string, object>)actualLocations[0])["line"] + 1;
                            if (actualLocation != breakpoint.LineNo) {
                                lineNo = actualLocation;
                            }
                        }
                    }

                    var breakpointBinding = breakpoint.CreateBinding(lineNo, breakpointID, scriptID);
                    _breakpointBindings[breakpointID] = breakpointBinding;

                    var breakpointBound = BreakpointBound;
                    if (breakpointBound != null) {
                        breakpointBound(this, new BreakpointBindingEventArgs(breakpoint, breakpointBinding));
                    }

                    if (successHandler != null) {
                        successHandler(breakpointBinding);
                    }
                } else { // Failure
                    var breakpointBindFailure = BreakpointBindFailure;
                    if (breakpointBindFailure != null) {
                        breakpointBindFailure(this, new BreakpointBindingEventArgs(breakpoint, null));
                    }

                    if (failureHandler != null) {
                        failureHandler();
                    }
                }
            };

            Connection.SendRequest(
                "setbreakpoint",
                args,
                responseHandler,
                responseHandler
            );
        }

        internal NodeModule GetModuleForFilePath(string filePath) {
            NodeModule module = null;
            _scripts.TryGetValue(filePath, out module);
            return module;
        }

        internal bool UpdateBreakpointBinding(
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

            // Compose request arguments
            if (enabled == null && condition == null && ignoreCount == null) {
                Debug.Fail("enabled and/or condition and/ or ignoreCount expected");
                return false;
            }
            var args = new Dictionary<string, object> { { "breakpoint", breakpointId }};
            if (enabled != null) {
                args["enabled"] = enabled.Value;
            }
            if (condition != null) {
                args["condition"] = condition;
            }
            if (ignoreCount != null) {
                args["ignoreCount"] = ignoreCount.Value;
            }

            // Process request
            bool success = false;
            Connection.SendRequest(
                "changebreakpoint",
                args,
                json => {
                    // Handle success
                    if (followupHandler != null) {
                        followupHandler();
                    }
                    success = true;
                },
                json => {
                    // Handle failure
                    if (followupHandler != null) {
                        followupHandler();
                    }
                },
                validateSuccess ? (int?)2000 : null,
                validateSuccess ? (Func<bool>)(() => HasExited) : null
            );

            return validateSuccess ? success : true;
        }

        internal int? GetBreakpointHitCount(
            int breakpointId
        ) {
            int? hitCount = null;
            Connection.SendRequest(
                "listbreakpoints",
                null,   // args
                json => {
                    // Handle success
                    var body = (Dictionary<string, object>)json["body"];
                    var breakpoints = (object [])body["breakpoints"];
                    foreach (var breakpointObj in breakpoints) {
                        var breakpoint = (Dictionary<string, object>)breakpointObj;
                        if ((int)breakpoint["number"] == breakpointId) {
                            hitCount = (int)breakpoint["hit_count"];
                            break;
                        }
                    }
                },
                timeout: 2000,
                shortCircuitPredicate: () => HasExited
            );

            return hitCount;
        }

        internal void ExecuteText(string text, NodeStackFrame nodeStackFrame, Action<NodeEvaluationResult> completion) {
            DebugWriteCommand("ExecuteText to thread " + nodeStackFrame.Thread.Id + " " /*+ executeId*/);

            Connection.SendRequest(
                "evaluate",
                new Dictionary<string, object> {
                            { "expression", text },
                            { "frame",  nodeStackFrame.FrameId },
                            { "global", false },
                            { "disable_break", true },
                            { "maxStringLength", -1 }
                },
                json => {
                    // Handle success
                    var jsonValue = new JsonValue(json);
                    var evaluationResult = ResponseHandler.ProcessEvaluate(nodeStackFrame, text, jsonValue);
                    completion(evaluationResult);
                },
                json => {
                    // Handle failure
                    completion(new NodeEvaluationResult((string)json["message"],text,nodeStackFrame));
                }
            );
        }

        internal void EnumChildren(NodeEvaluationResult nodeEvaluationResult, Action<NodeEvaluationResult[]> completion) {
            DebugWriteCommand("Enum Children");

            Connection.SendRequest(
                "lookup",
                new Dictionary<string, object> {
                            { "handles", new object[] {nodeEvaluationResult.Handle} },
                            { "includeSource", false }
                },
                json => {
                    // Handle success
                    var jsonValue = new JsonValue(json);
                    var evaluationResults = ResponseHandler.ProcessLookup(nodeEvaluationResult, jsonValue);
                    completion(evaluationResults.ToArray());
                }
            );
        }

        private Dictionary<string, object> GetRefRecord(object[] refs, int handle) {
            foreach (var refRecordObj in refs) {
                var refRecord = (Dictionary<string, object>)refRecordObj;
                var refRecordHandle = (int)refRecord["handle"];
                if (refRecordHandle == handle) {
                    return refRecord;
                }
            }

            return null;
        }

        internal void RemoveBreakPoint(NodeBreakpointBinding breakpointBinding, Action successHandler = null, Action failureHandler = null) {
            DebugWriteCommand("Remove Breakpoint");

            // Perform remove idempotently, as remove may be called in response to BreakpointUnound event
            if (breakpointBinding.Unbound) {
                if (successHandler != null) {
                    successHandler();
                }
                return;
            }

            Connection.SendRequest(
                "clearbreakpoint",
                new Dictionary<string, object> {
                    { "breakpoint", breakpointBinding.BreakpointID }
                },
                successHandler:
                    json => {
                        var breakpoint = breakpointBinding.Breakpoint;
                        _breakpointBindings.Remove(breakpointBinding.BreakpointID);
                        breakpoint.RemoveBinding(breakpointBinding);
                        breakpointBinding.Unbound = true;

                        var breakpointUnbound = BreakpointUnbound;
                        if (breakpointUnbound != null) {
                            breakpointUnbound(this, new BreakpointBindingEventArgs(breakpoint, breakpointBinding));
                        }

                        if (successHandler != null) {
                            successHandler();
                        }
                    },
                failureHandler:
                    json => {
                        if (failureHandler != null) {
                            failureHandler();
                        }
                    }
            );
        }

        internal bool SetLineNumber(NodeStackFrame nodeStackFrame, int lineNo) {
            DebugWriteCommand("Set Line Number");

            throw new NotImplementedException();
        }

        #endregion

        #region Debugging Events

        /// <summary>
        /// Fired when the process has started and is broken into the debugger, but before any user code is run.
        /// </summary>
        public event EventHandler<ProcessLoadedEventArgs> ProcessLoaded;
        public event EventHandler<ThreadEventArgs> ThreadCreated;
        public event EventHandler<ThreadEventArgs> ThreadExited { add { } remove { } }
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
        public event EventHandler<OutputEventArgs> DebuggerOutput { add { } remove { } }

        #endregion

        internal void Close() {
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

        internal string GetScriptText(int moduleId) {
            DebugWriteCommand("GetScriptText: " + moduleId);

            string scriptText = null;
            Connection.SendRequest(
                "scripts",
                new Dictionary<string, object> {
                        { "ids", new object[] {moduleId} },
                        { "includeSource",  true },
                },
                successHandler: json => {
                    var script = (Dictionary<string, object>)((object[])json["body"]).First();
                    scriptText = (string)script["source"];
                },
                timeout: 3000
            );
            return scriptText;
        }
    }
}

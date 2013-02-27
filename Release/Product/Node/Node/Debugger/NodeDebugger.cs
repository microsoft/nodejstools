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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Microsoft.NodeTools.Debugger {
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
    class NodeDebugger : JsonListener {
        private readonly Process _process;
        private string _hostName = "localhost";
        private ushort _portNumber = 5858;
        private int? _id;
        private readonly Dictionary<int, NodeBreakpoint> _breakpoints = new Dictionary<int, NodeBreakpoint>();
        private bool _loadCompleteHandled;
        private bool _handleEntryPointTracePoint;
        private SteppingKind _steppingMode;
        private int _steppingCallstackDepth;
        private bool _resumingStepping;
        private readonly Dictionary<int, NodeThread> _threads = new Dictionary<int, NodeThread>();
        public readonly int MainThreadId = 1;
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private int _currentRequestSequence = 1;
        private readonly byte[] _socketBuffer = new byte[4096];
        private bool _sentExited;
        private readonly Dictionary<string, NodeModule> _scripts = new Dictionary<string, NodeModule>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, object> _requestData = new Dictionary<int, object>();
        private ExceptionHitTreatment _defaultExceptionTreatment;
        private Dictionary<string, ExceptionHitTreatment> _exceptionTreatments = new Dictionary<string, ExceptionHitTreatment>();
        private bool _breakOnAllExceptions;
        private bool _breakOnUncaughtExceptions;
        private int _entryPointFrameCount;

        public NodeDebugger(string exe, string args, string dir, string env, string interpreterOptions, NodeDebugOptions debugOptions, List<string[]> dirMapping) {
            args = "--debug-brk " + args;
            if (!string.IsNullOrEmpty(interpreterOptions)) {
                args += " " + interpreterOptions;
            }
            var psi = new ProcessStartInfo(exe, args);
            psi.WorkingDirectory = dir;
            // TODO: Env
            _process = new Process();
            _process.StartInfo = psi;
            _process.EnableRaisingEvents = true;
            _process.Exited += new EventHandler(_process_Exited);
        }

        public NodeDebugger(string hostName, ushort portNumber, int id) {
            _hostName = hostName;
            _portNumber = portNumber;
            _id = id;
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

        private void _process_Exited(object sender, EventArgs e) {
            if (!_sentExited) {
                _sentExited = true;
                var exited = ProcessExited;
                if (exited != null) {
                    int exitCode;
                    try {
                        exitCode = _process.HasExited ? _process.ExitCode : -1;
                    } catch (InvalidOperationException) {
                        // debug attach, we didn't start the process...
                        exitCode = -1;
                    }
                    exited(this, new ProcessExitedEventArgs(exitCode));
                }
            }
        }

        public void WaitForExit() {
            _process.WaitForExit();
        }

        public bool WaitForExit(int milliseconds) {
            return _process.WaitForExit(milliseconds);
        }

        public void Terminate() {
            if (_process != null && !_process.HasExited) {
                Socket = null;
                _process.Kill();
            }
        }

        public bool HasExited {
            get {
                return _process != null ? _process.HasExited : Socket == null || !Socket.Connected;
            }
        }

        /// <summary>
        /// Breaks into the process.
        /// </summary>
        public void BreakAll() {
            DebugWriteCommand("BreakAll");

            SendRequest(
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

            SendRequest(
                "continue",
                args,
                json => {
                    // Handle success
                    // Nothing to do
                });
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

            if (_defaultExceptionTreatment != default(ExceptionHitTreatment)) {
                _defaultExceptionTreatment = default(ExceptionHitTreatment);
                updated = true;
            }

            if (_exceptionTreatments.Count() > 0) {
                _exceptionTreatments.Clear();
                updated = true;
            }

            if (updated) {
                SetExceptionBreak(synchronous: true);
            }
        }

        #endregion

        #region Debuggee Communcation

        class ResponseHandler {
            private Action<Dictionary<string, object>> _successHandler;
            private Action<Dictionary<string, object>> _failureHandler;
            private int? _timeout;
            private Func<bool> _shortCircuitPredicate;
            private AutoResetEvent _completedEvent;
            public ResponseHandler(
                Action<Dictionary<string, object>> successHandler = null,
                Action<Dictionary<string, object>> failureHandler = null,
                int? timeout = null,
                Func<bool> shortCircuitPredicate = null
            ) {
                Debug.Assert(
                    successHandler != null || failureHandler != null || timeout != null,
                    "At least success handler, failure handler or timeout should be non-null");
                _successHandler = successHandler;
                _failureHandler = failureHandler;
                _timeout = timeout;
                _shortCircuitPredicate = shortCircuitPredicate;
                if (timeout.HasValue) {
                    _completedEvent = new AutoResetEvent(false);
                }
            }

            public bool Wait() {
                // Handle asynchronous (no wait)
                if (_completedEvent == null) {
                    Debug.Assert((_timeout == null), "No completedEvent implies no timeout");
                    Debug.Assert((_shortCircuitPredicate == null), "No completedEvent implies no shortCircuitPredicate");
                    return true;
                }
                Debug.Assert((_timeout != null) && _timeout > 0, "completedEvent implies timeout");

                // Handle synchronous without short circuiting
                int timeout = _timeout.Value;
                if (_shortCircuitPredicate == null) {
                    return _completedEvent.WaitOne(timeout);
                }

                // Handle synchronous with short circuiting
                int interval = Math.Max(1, timeout / 10);
                while (!_shortCircuitPredicate()) {
                    if (_completedEvent.WaitOne(Math.Min(timeout, interval))) {
                        return true;
                    }

                    timeout -= interval;
                    if (timeout <= 0) {
                        break;
                    }
                }
                return false;
            }

            public void HandleResponse(Dictionary<string, object> json) {
                if ((bool)json["success"]) {
                    if (_successHandler != null) {
                        _successHandler(json);
                    }
                } else {
                    if (_failureHandler != null) {
                        _failureHandler(json);
                    }
                }

                if (_completedEvent != null) {
                    _completedEvent.Set();
                }
            }
        }

        private bool SendRequest(
            string command,
            Dictionary<string, object> args = null,
            Action<Dictionary<string, object>> successHandler = null,
            Action<Dictionary<string, object>> failureHandler = null,
            int? timeout = null,
            Func<bool> shortCircuitPredicate = null
        ) {
            if (shortCircuitPredicate != null && shortCircuitPredicate()) {
                if (failureHandler != null) {
                    failureHandler(null);
                }
                return false;
            }

            int reqId = DispenseRequestId();

            // Use response handler if followup (given success or failure handler) or synchronous (given timeout)
            ResponseHandler responseHandler = null;
            if ((successHandler != null) || (failureHandler != null) || (timeout != null)) {
                responseHandler = new ResponseHandler(successHandler, failureHandler, timeout, shortCircuitPredicate);
                _requestData[reqId] = responseHandler;
            }

            Socket.Send(CreateRequest(command, args, reqId));

            return (responseHandler != null) ? responseHandler.Wait() : true;
        }

        private int DispenseRequestId() {
            return _currentRequestSequence++;
        }

        private byte[] CreateRequest(string command, Dictionary<string, object> args, int reqId) {
            string json;

            if (args != null) {
                json = _serializer.Serialize(
                    new {
                        seq = reqId,
                        type = "request",
                        command = command,
                        arguments = args
                    }
                );
            } else {
                json = _serializer.Serialize(
                    new {
                        seq = reqId,
                        type = "request",
                        command = command
                    }
                );
            }

            var requestStr = string.Format("Content-Length: {0}\r\n\r\n{1}", Encoding.UTF8.GetByteCount(json), json);

            Debug.WriteLine(String.Format("Request: {0}", requestStr));

            return Encoding.UTF8.GetBytes(requestStr);
        }

        internal void Connected(Socket socket) {
            Debug.WriteLine("Process Connected: ");

            Socket = socket;
            /*
            if (!_delayUnregister) {
                Unregister();
            }*/
        }

        internal void Unregister() {
            //DebugConnectionListener.UnregisterProcess(_processGuid);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts listening for debugger communication.  Can be called after Start
        /// to give time to attach to debugger events.
        /// </summary>
        public void StartListening() {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.NoDelay = true;
            Socket.Connect(new DnsEndPoint(_hostName, _portNumber));

            StartListenerThread();
        }

        protected override void OnSocketDisconnected() {
            if (!_sentExited && _process == null) {
                _sentExited = true;
                var exited = ProcessExited;
                if (exited != null) {
                    // debug attach, we didn't start the process...
                    int exitCode = -1;
                    exited(this, new ProcessExitedEventArgs(exitCode));
                }
            }
        }

        protected override void ProcessPacket(JsonResponse response) {
            Debug.WriteLine("Headers:");

            foreach (var keyValue in response.Headers) {
                Debug.WriteLine(String.Format("{0}: {1}", keyValue.Key, keyValue.Value));
            }

            Debug.WriteLine(String.Format("Body: {0}", string.IsNullOrEmpty(response.Body) ? string.Empty : response.Body));

            if (response.Headers.ContainsKey("type")) {
                switch (response.Headers["type"]) {
                    case "connect":
                        ProcessConnect();
                        break;
                    default:
                        Debug.WriteLine(String.Format("Unknown header type: {0}", response.Headers["type"]));
                        break;
                }
                return;
            }

            var json = (Dictionary<string, object>)_serializer.DeserializeObject(response.Body);
            switch ((string)json["type"]) {
                case "response":
                    ProcessCommandResponse(json);
                    break;
                case "event":
                    ProcessEvent(json);
                    break;
                default:
                    Debug.WriteLine(String.Format("Unknown body type: {0}", json["type"]));
                    break;
            }
        }

        private void ProcessConnect() {
            var mainThread = new NodeThread(this, MainThreadId, false);
            _threads[mainThread.Id] = mainThread;

            GetScripts();

            SetExceptionBreak();

            PerformBacktrace((running) => {
                // Handle followup
                if (!running) {
                    _entryPointFrameCount = MainThread.Frames.Count();
                }

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
            SendRequest(
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
            string name = (string)script["name"];
            NodeModule existingModule;
            if (!_scripts.TryGetValue(name, out existingModule)) {
                int id = (int)script["id"];
                var newModule = _scripts[name] = new NodeModule(id, name);
                var modLoad = ModuleLoaded;
                if (modLoad != null) {
                    modLoad(this, new ModuleLoadedEventArgs(newModule));
                }

                // Fixup breakpoints bound by name
                // This is necessary to work around a bug in node which binds breakpoints by comparing case against that given to Require()
                foreach (var breakpoint in _breakpoints.Values.Where(b => b.BoundByName)) {
                    if (GetModuleForFilePath(breakpoint.FileName) == newModule && string.Compare(breakpoint.FileName, newModule.FileName) != 0) {
                        id = breakpoint.Id;
                        breakpoint.Id = 0;
                        breakpoint.Add();
                        RemoveBreakPoint(id);
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
                if (!SendRequest(
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
                if (!SendRequest(
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

        private void ProcessCommandResponse(Dictionary<string, object> json) {
            object reqIdObj;
            if (!json.TryGetValue("request_seq", out reqIdObj)) {
                return;
            }
            int reqId = (int)reqIdObj;

            object responseHandlerObj;
            if (!_requestData.TryGetValue(reqId, out responseHandlerObj)) {
                return;
            }
            ResponseHandler responseHandler = responseHandlerObj as ResponseHandler;
            if (responseHandler == null) {
                return;
            }
            _requestData.Remove(reqId);

            responseHandler.HandleResponse(json);
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

            // We need to get the backtrace before we break, so we request the backtrace
            // and follow up with firing the appropriate event for the break
            PerformBacktrace((running) => {
                // Handle followup
                Debug.Assert(!running);
                if (breakpoints != null) {
                    // Process breakpoint hit
                    foreach (int breakpoint in breakpoints) {
                        NodeBreakpoint nodeBreakpoint;
                        if (_breakpoints.TryGetValue(breakpoint, out nodeBreakpoint)) {
                            var breakpointHit = BreakpointHit;
                            nodeBreakpoint.ProcessBreakpointHit(
                                () => {
                                    // Fire breakpoint hit event
                                    if (breakpointHit != null) {
                                        breakpointHit(this, new BreakpointHitEventArgs(nodeBreakpoint, MainThread));
                                    }
                                }
                             );
                        }
                    }
                } else {
                    // Fallback to completing stepping
                    CompleteStepping();
                }
            });
        }

        private void ProcessException(Dictionary<string, object> json) {
            var body = (Dictionary<string, object>)json["body"];
            var uncaught = (bool)body["uncaught"];

            var exceptionName = GetExceptionName(json);
            ExceptionHitTreatment exceptionTreatment;
            if (!_exceptionTreatments.TryGetValue(exceptionName, out exceptionTreatment)) {
                exceptionTreatment = _defaultExceptionTreatment;
            }

            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break always or never
            //if (exceptionTreatment == ExceptionHitTreatment.BreakNever ||
            //    (exceptionTreatment == ExceptionHitTreatment.BreakOnUnhandled && !uncaught)) {
            if (exceptionTreatment == ExceptionHitTreatment.BreakNever) {
                Resume();
                return;
            }

            // We need to get the backtrace before we break, so we request the backtrace
            // and follow up with firing the appropriate event for the break
            PerformBacktrace((running) => {
                // Handle followup
                if (MainThread.Frames.Count() < _entryPointFrameCount) {
                    // Auto resume exceptions thrown up the stack from our entry point
                    // to avoid firing exception raised events for rethrow performed by Node
                    Resume();
                    return;
                }
                Debug.Assert(!running);
                var exceptionRaised = ExceptionRaised;
                if (exceptionRaised != null) {
                    var exception = (Dictionary<string, object>)body["exception"];
                    var text = (string)exception["text"];
                    exceptionRaised(this, new ExceptionRaisedEventArgs(MainThread, new NodeException(exceptionName, text), uncaught));
                }
            });
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
            SendRequest(
                "backtrace",
                new Dictionary<string, object> { { "inlineRefs", true } },
                json => {
                    // Handle success
                    var running = (bool)json["running"];
                    if (!running) {
                        var mainThread = MainThread;
                        var body = (Dictionary<string, object>)json["body"];
                        var frames = (object[])body["frames"];

                        NodeStackFrame[] nodeFrames = new NodeStackFrame[frames.Length];

                        for (int i = 0; i < frames.Length; i++) {
                            var frame = (Dictionary<string, object>)frames[i];

                            var func = (Dictionary<string, object>)frame["func"];
                            object scriptIdObj;
                            string scriptFilename = "<unknown>";
                            if (func.TryGetValue("scriptId", out scriptIdObj)) {
                                foreach (var value in _scripts.Values) {
                                    if (value.ModuleId == (int)scriptIdObj) {
                                        scriptFilename = value.FileName;
                                    }
                                }
                            }

                            var nodeFrame = nodeFrames[i] = new NodeStackFrame(
                                mainThread,
                                (string)func["name"],
                                scriptFilename,
                                (int)frame["line"] + 1,   // FIXME, should be function line start
                                (int)frame["line"] + 1,   // FIXME, should be function line end
                                (int)frame["line"] + 1,
                                0,  // Let GetFrameVariables() set argCount
                                i
                            );

                            GetFrameVariables(nodeFrame, frame);
                        }

                        mainThread.Frames = nodeFrames;
                    }
                    if (followupHandler != null) {
                        followupHandler(running);
                    }
                }
            );
        }

        private void GetFrameVariables(NodeStackFrame nodeFrame, Dictionary<string, object> frame) {
            var jsonArgObjs = ((object[])frame["arguments"]).Where(
                                jsonArgObj =>
                                    String.Compare(
                                        (string)(((Dictionary<string, object>)((Dictionary<string, object>)jsonArgObj)["value"])["type"]),
                                        "function") != 0);
            nodeFrame.SetArgCount(jsonArgObjs.Count());

            var jsonVarObjs = jsonArgObjs.Concat(
                                    ((object[])frame["locals"])).Where(
                                        jsonVarObj =>
                                            String.Compare(
                                                (string)(((Dictionary<string, object>)((Dictionary<string, object>)jsonVarObj)["value"])["type"]),
                                                "function") != 0);

            List<NodeEvaluationResult> childNodeEvaluationResults = new List<NodeEvaluationResult>();
            foreach (var jsonVarObj in jsonVarObjs) {
                var childNodeEvaluationResult = CreateFrameVariableNodeEvaluationResult(nodeFrame, (Dictionary<string, object>)jsonVarObj);
                if (childNodeEvaluationResult != null) {
                    childNodeEvaluationResults.Add(childNodeEvaluationResult);
                }
            }

            nodeFrame.SetVariables(childNodeEvaluationResults.ToArray());
        }

        private void ReadMoreData(int bytesRead, ref string text, ref int pos) {
            var newText = Encoding.UTF8.GetString(_socketBuffer, 0, bytesRead);
            text = text.Substring(pos) + newText;
            pos = 0;
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
                foreach (var nodeBreakpoint in _breakpoints.Values) {
                    if (nodeBreakpoint.Enabled && nodeBreakpoint.LineNo == breakLineNo && GetModuleForFilePath(nodeBreakpoint.FileName) == breakModule) {
                        // Handle ignore count
                        if (!nodeBreakpoint.TryIgnore()) {
                            // Fire breakpoint/tracepoint hit and setup to handle entrypoint tracepoint resume
                            var breakpointHit = BreakpointHit;
                            if (breakpointHit != null) {
                                breakpointHit(this, new BreakpointHitEventArgs(nodeBreakpoint, MainThread));
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
            if (_steppingMode != SteppingKind.None) {
                _resumingStepping = true;
                CompleteStepping();
                return;
            }

            Continue();
        }

        public void SendClearStepping(int threadId) {
            DebugWriteCommand("ClearStepping");
            //throw new NotImplementedException();
        }

        public void Detach() {
            DebugWriteCommand("Detach");
            throw new NotImplementedException();
        }

        internal void BindBreakpoint(NodeBreakpoint breakpoint, Action successHandler = null, Action failureHandler = null) {
            DebugWriteCommand(String.Format("Bind Breakpoint"));

            // Compose request arguments
            var args =
                new Dictionary<string, object> { 
                    { "line", breakpoint.LineNo - 1 },  // Zero based line numbers
                    { "column", 0 } // Zero based column numbers
                };
            bool boundByName = false;
            var module = GetModuleForFilePath(breakpoint.FileName);
            if (module != null) {
                args["type"] = "scriptId";
                args["target"]= module.ModuleId;
            } else {
                args["type"] = "script";
                args["target"] = breakpoint.FileName;
                boundByName = true;
            }
            if (!breakpoint.GetEngineEnabled()) {
                args["enabled"] = false;
            }
            var ignoreCount = breakpoint.GetEngineIgnoreCount();
            if (ignoreCount > 0) {
                args["ignoreCount"] = ignoreCount;
            }
            if (!string.IsNullOrEmpty(breakpoint.Condition)) {
                args["condition"] = breakpoint.Condition;
            }

            Action<Dictionary<string, object>> responseHandler = json => {
                var success = (bool)json["success"];
                if (success) {
                    breakpoint.Id = (int)((Dictionary<string, object>)json["body"])["breakpoint"];
                    _breakpoints[breakpoint.Id] = breakpoint;
                    breakpoint.BoundByName = boundByName;                    
                }
                Action bindResultHandler = success ? successHandler : failureHandler;
                if (bindResultHandler != null) {
                    bindResultHandler();
                }
            };

            SendRequest(
                "setbreakpoint",
                args,
                responseHandler,
                responseHandler
            );
        }

        internal NodeModule GetModuleForFilePath(string filePath) {
            NodeModule module;
            if (!_scripts.TryGetValue(filePath, out module) || module == null) {
                var fileNameToLower = Path.GetFileName(filePath).ToLower();
                foreach (var kvp in _scripts) {
                    if (Path.GetFileName(kvp.Key).ToLower() == fileNameToLower && kvp.Value != null) {
                        module = kvp.Value;
                        break;
                    }
                }
            }
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
            SendRequest(
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
            SendRequest(
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

            SendRequest(
                "evaluate",
                new Dictionary<string, object> {
                            { "expression", text },
                            { "frame",  nodeStackFrame.FrameId },
                            { "global", false },
                            { "disable_break", true },
                            //{ "additional_context",  new object[] {
                            //    new Dictionary<string, object> {
                            //        { "name", "<name1>" },
                            //        { "handle", "<handle1>" },
                            //    },
                            //    new Dictionary<string, object> {
                            //        { "name", "<name2>" },
                            //        { "handle", "<handle12" },
                            //    },
                            //} }
                },
                json => {
                    // Handle success
                    var record = (Dictionary<string, object>)json["body"];
                    completion(CreateNodeEvaluationResult(nodeStackFrame, text, record));
                },
                json => {
                    // Handle failure
                    completion(new NodeEvaluationResult(
                            this,
                            (string)json["message"],
                            text,
                            nodeStackFrame
                       ));
                }
            );
        }

        internal void EnumChildren(NodeEvaluationResult nodeEvaluationResult, Action<NodeEvaluationResult[]> completion) {
            DebugWriteCommand("Enum Children");

            SendRequest(
                "lookup",
                new Dictionary<string, object> {
                            { "handles", new object[] {nodeEvaluationResult.Handle} },
                            { "includeSource", false }
                },
                json => {
                    // Handle success
                    List<NodeEvaluationResult> childNodeEvaluationResults = new List<NodeEvaluationResult>();

                    var refs = (object[])json["refs"];
                    var body = (Dictionary<string, object>)json["body"];
                    var record = (Dictionary<string, object>)body[nodeEvaluationResult.Handle.ToString()];
                    var properties = (object[])record["properties"];

                    if (nodeEvaluationResult.IsArray) {
                        var countProperty = (Dictionary<string, object>)properties[0];
                        var countHandle = (int)countProperty["ref"];
                        var refRecord = GetRefRecord(refs, countHandle);
                        if (refRecord != null) {
                            var count = (int)refRecord["value"];
                            for (var i = 1; i <= count; ++i) {
                                var elementProperty = (Dictionary<string, object>)properties[i];
                                var elementHandle = (int)elementProperty["ref"];
                                refRecord = GetRefRecord(refs, elementHandle);
                                if (refRecord != null) {
                                    var elementName = string.Format("[{0}]", i - 1);
                                    var childNodeEvaluationResult = CreateNodeEvaluationResult(nodeEvaluationResult.Frame, elementName, refRecord);
                                    if (childNodeEvaluationResult != null) {
                                        childNodeEvaluationResults.Add(childNodeEvaluationResult);
                                    }
                                }
                            }
                        }
                    } else {
                        foreach (var propertyObj in properties) {
                            var property = (Dictionary<string, object>)propertyObj;
                            var propertyName = property["name"].ToString();
                            var propertyHandle = (int)property["ref"];
                            var refRecord = GetRefRecord(refs, propertyHandle);
                            if (refRecord != null) {
                                var childNodeEvaluationResult = CreateNodeEvaluationResult(nodeEvaluationResult.Frame, propertyName, refRecord);
                                if (childNodeEvaluationResult != null) {
                                    childNodeEvaluationResults.Add(childNodeEvaluationResult);
                                }
                            }
                        }
                    }

                    completion(childNodeEvaluationResults.ToArray());
                }
            );
        }

        private NodeEvaluationResult CreateFrameVariableNodeEvaluationResult(NodeStackFrame nodeFrame, Dictionary<string, object> varRecord) {
            object nameObj;
            var name = varRecord.TryGetValue("name", out nameObj) ? (string)nameObj : "<unknown>";
            var record = (Dictionary<string, object>)(varRecord["value"]);

            object valueObj;
            record.TryGetValue("value", out valueObj);
            string value = string.Empty;
            
            int? handle = null;
            var expandable = false;

            var type = (string)record["type"];
            switch (type) {
                case "object":
                    expandable = true;
                    object classNameObj;
                    if (record.TryGetValue("className", out classNameObj)) {
                        var className = (string)classNameObj;
                        if (!string.IsNullOrEmpty(className)) {
                            switch (className) {
                                case "Date":
                                    // UNDONE Evaluate frame var using followup request ('lookup', 'evaluate', ...),
                                    // to workaround fact that 'backrace' response json does not include date values
                                    type = "date";
                                    value = (string)valueObj;
                                    expandable = false;
                                    break;
                                default:
                                    value = className;
                                    break;
                            }
                        }
                    }
                    if (record.TryGetValue("ref", out valueObj)) {
                        handle = (int)valueObj;
                    }
                    break;
                case "string":
                    value = "\"" + (string)valueObj + "\"";
                    break;
                case "number":
                    value = valueObj != null ? valueObj.ToString() : "null";
                    break;
                case "boolean":
                    value = (bool)valueObj ? "true" : "false";
                    break;
                case "null":
                    value = "null";
                    break;
                case "undefined":
                    value = "undefined";
                    break;
                case "function":
                    return null;
                default:
                    Debug.WriteLine(String.Format("Unhandled value type: {0}", type));
                    break;
            }
            return new NodeEvaluationResult(
                            this,
                            handle,
                            value,
                            "",
                            type,
                            name,
                            "",
                            false,
                            false,
                            nodeFrame,
                            expandable
                       );
        }

        private NodeEvaluationResult CreateNodeEvaluationResult(NodeStackFrame nodeFrame, string name, Dictionary<string, object> valueContainer) {
            object valueObj;
            var value = valueContainer.TryGetValue("text", out valueObj) ? (string)valueObj : string.Empty;
            int? handle = null;
            var expandable = false;

            var type = (string)valueContainer["type"];
            switch (type) {
                case "object":
                    expandable = true;
                    if (valueContainer.TryGetValue("className", out valueObj)) {
                        var className = (string)valueObj;
                        if (!string.IsNullOrEmpty(className)) {
                            switch (className) {
                                case "Date":
                                    type = "date";
                                    expandable = false;
                                    break;
                                default:
                                    value = className;
                                    break;
                            }
                        }
                    }
                    if (valueContainer.TryGetValue("handle", out valueObj)) {
                        handle = (int)valueObj;
                    }
                    break;
                case "string":
                    value = "\"" + value + "\"";
                    break;
                case "number":
                case "boolean":
                    break;
                case "null":
                    value = "null";
                    break;
                case "undefined":
                    value = "undefined";
                    break;
                case "function":
                    return null;
                default:
                    Debug.WriteLine(String.Format("Unhandled value type: {0}", type));
                    break;
            }
            return new NodeEvaluationResult(
                            this,
                            handle,
                            value,
                            "",
                            type,
                            name,
                            "",
                            false,
                            false,
                            nodeFrame,
                            expandable
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

        internal void RemoveBreakPoint(int id) {
            DebugWriteCommand("Remove Breakpoint");

            SendRequest(
                "clearbreakpoint",
                new Dictionary<string, object> {
                    { "breakpoint", id }
                },
                json => {
                    // Handle success
                    _breakpoints.Remove(id);
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
        public event EventHandler<BreakpointHitEventArgs> BreakpointHit;
        public event EventHandler<OutputEventArgs> DebuggerOutput { add { } remove { } }

        #endregion

        internal void Close() {
        }
    }
}

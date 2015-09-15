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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using EnvDTE;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.NodejsTools.Debugger.Remote;
using Microsoft.NodejsTools.Logging;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using SR = Microsoft.NodejsTools.Project.SR;

namespace Microsoft.NodejsTools.Debugger.DebugEngine {
    // AD7Engine is the primary entrypoint object for the debugging engine. 
    //
    // It implements:
    //
    // IDebugEngine2: This interface represents a debug engine (DE). It is used to manage various aspects of a debugging session, 
    // from creating breakpoints to setting and clearing exceptions.
    //
    // IDebugEngineLaunch2: Used by a debug engine (DE) to launch and terminate programs.
    //
    // IDebugProgram3: This interface represents a program that is running in a process. Since this engine only debugs one process at a time and each 
    // process only contains one program, it is implemented on the engine.

    [ComVisible(true)]
    [Guid(Guids.DebugEngine)]
    public sealed class AD7Engine : IDebugEngine2, IDebugEngineLaunch2, IDebugProgram3, IDebugSymbolSettings100 {
        // used to send events to the debugger. Some examples of these events are thread create, exception thrown, module load.
        private IDebugEventCallback2 _events;

        // The core of the engine is implemented by NodeDebugger - we wrap and expose that to VS.
        private NodeDebugger _process;

        // mapping between NodeThread threads and AD7Threads
        private readonly Dictionary<NodeThread, AD7Thread> _threads = new Dictionary<NodeThread, AD7Thread>();
        private readonly Dictionary<NodeModule, AD7Module> _modules = new Dictionary<NodeModule, AD7Module>();
        private AD7Thread _mainThread;
        private bool _sdmAttached;
        private bool _processLoaded;
        private bool _loadComplete;
        private readonly object _syncLock = new object();
        private bool _attached;
        private readonly AutoResetEvent _threadExitedEvent = new AutoResetEvent(false), _processExitedEvent = new AutoResetEvent(false);
        private readonly BreakpointManager _breakpointManager;
        private Guid _ad7ProgramId;             // A unique identifier for the program being debugged.
        private static readonly HashSet<WeakReference> Engines = new HashSet<WeakReference>();
        private string _webBrowserUrl;

        public const string DebugEngineId = "{0A638DAC-429B-4973-ADA0-E8DCDFB29B61}";
        public static Guid DebugEngineGuid = new Guid(DebugEngineId);
        private bool _trackFileChanges;
        private DocumentEvents _documentEvents;

        /// <summary>
        /// Specifies whether the process should prompt for input before exiting on an abnormal exit.
        /// </summary>
        public const string WaitOnAbnormalExitSetting = "WAIT_ON_ABNORMAL_EXIT";

        /// <summary>
        /// Specifies whether the process should prompt for input before exiting on a normal exit.
        /// </summary>
        public const string WaitOnNormalExitSetting = "WAIT_ON_NORMAL_EXIT";

        /// <summary>
        /// Specifies if the output should be redirected to the visual studio output window.
        /// </summary>
        public const string RedirectOutputSetting = "REDIRECT_OUTPUT";

        /// <summary>
        /// Specifies options which should be passed to the Node runtime before the script.  If
        /// the interpreter options should include a semicolon then it should be escaped as a double
        /// semi-colon.
        /// </summary>
        public const string InterpreterOptions = "INTERPRETER_OPTIONS";

        /// <summary>
        /// Specifies URL to which to open web browser on node debug connect.
        /// </summary>
        public const string WebBrowserUrl = "WEB_BROWSER_URL";

        /// <summary>
        /// Specifies the port to be used for the debugger.
        /// </summary>
        public const string DebuggerPort = "DEBUGGER_PORT";

        /// <summary>
        /// Specifies a directory mapping in the form of:
        /// 
        /// OldDir|NewDir
        /// 
        /// for mapping between the files on the local machine and the files deployed on the
        /// running machine.
        /// </summary>
        public const string DirMappingSetting = "DIR_MAPPING";

        public AD7Engine() {
            LiveLogger.WriteLine("--------------------------------------------------------------------------------");
            LiveLogger.WriteLine("AD7Engine Created ({0})", GetHashCode());
            _breakpointManager = new BreakpointManager(this);
            Engines.Add(new WeakReference(this));
        }

        ~AD7Engine() {
            LiveLogger.WriteLine("AD7Engine Finalized ({0})", GetHashCode());
            if (!_attached && _process != null) {
                // detach the process exited event, we don't need to send the exited event
                // which could happen when we terminate the process and check if it's still
                // running.
                _process.ProcessExited -= OnProcessExited;

                // we launched the process, go ahead and kill it now that
                // VS has released us
                _process.Terminate();
            }

            foreach (var engine in Engines) {
                if (engine.Target == this) {
                    Engines.Remove(engine);
                    break;
                }
            }
        }

        internal static IList<AD7Engine> GetEngines() {
            var engines = new List<AD7Engine>();
            foreach (var engine in Engines) {
                var target = (AD7Engine)engine.Target;
                if (target != null) {
                    engines.Add(target);
                }
            }
            return engines;
        }

        internal NodeDebugger Process {
            get {
                return _process;
            }
        }

        internal AD7Thread MainThread {
            get {
                return _mainThread;
            }
        }

        internal BreakpointManager BreakpointManager {
            get {
                return _breakpointManager;
            }
        }

        #region IDebugEngine2 Members

        // Attach the debug engine to a program. 
        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 ad7Callback, enum_ATTACH_REASON dwReason) {
            DebugWriteCommand("Attach");

            AssertMainThread();
            Debug.Assert(_ad7ProgramId == Guid.Empty);

            if (celtPrograms != 1) {
                Debug.Fail("Node debugging only supports one program in a process");
                throw new ArgumentException();
            }

            int processId = EngineUtils.GetProcessId(rgpPrograms[0]);
            if (processId == 0) {
                // engine only supports system processes
                LiveLogger.WriteLine("AD7Engine failed to get process id during attach");
                return VSConstants.E_NOTIMPL;
            }

            EngineUtils.RequireOk(rgpPrograms[0].GetProgramId(out _ad7ProgramId));

            // Attach can either be called to attach to a new process, or to complete an attach
            // to a launched process
            if (_process == null) {
                _events = ad7Callback;

                var program = (NodeRemoteDebugProgram)rgpPrograms[0];
                var process = program.DebugProcess;
                var uri = process.DebugPort.Uri;

                _process = new NodeDebugger(uri, process.Id);

                // We only need to do fuzzy comparisons when debugging remotely
                if (!uri.IsLoopback) {
                    _process.IsRemote = true;
                    _process.FileNameMapper = new FuzzyLogicFileNameMapper(EnumerateSolutionFiles());
                }

                AttachEvents(_process);
                _attached = true;
            } else {
                if (processId != _process.Id) {
                    Debug.Fail("Asked to attach to a process while we are debugging");
                    return VSConstants.E_FAIL;
                }
            }

            lock (_syncLock) {
                _sdmAttached = true;
                HandleLoadComplete();
            }

            LiveLogger.WriteLine("AD7Engine Attach returning S_OK");
            return VSConstants.S_OK;
        }

        private void HandleLoadComplete() {

            // Handle load complete once both sdm attached and process loaded
            if (!_sdmAttached || !_processLoaded) {
                return;
            }

            LiveLogger.WriteLine("Sending load complete ({0})", GetHashCode());

            AD7EngineCreateEvent.Send(this);

            AD7ProgramCreateEvent.Send(this);

            foreach (var module in _modules.Values) {
                SendModuleLoad(module);
            }

            foreach (var thread in _threads.Values) {
                SendThreadCreate(thread);
            }

            lock (_syncLock) {
                if (_processLoaded && _process.IsRunning()) {
                    Send(new AD7LoadCompleteRunningEvent(), AD7LoadCompleteRunningEvent.IID, _mainThread);
                } else {
                    Send(new AD7LoadCompleteEvent(), AD7LoadCompleteEvent.IID, _mainThread);
                }
            }

            _loadComplete = true;

            if (!String.IsNullOrWhiteSpace(_webBrowserUrl)) {
                var uri = new Uri(_webBrowserUrl);
                lock (_syncLock) {
                    OnPortOpenedHandler.CreateHandler(
                        uri.Port,
                        shortCircuitPredicate: () => !_processLoaded,
                        action: LaunchBrowserDebugger
                    );
                }
            }
        }

        private void SendThreadCreate(AD7Thread ad7Thread) {
            Send(new AD7ThreadCreateEvent(), AD7ThreadCreateEvent.IID, ad7Thread);
        }

        private void SendModuleLoad(AD7Module ad7Module) {
            var eventObject = new AD7ModuleLoadEvent(ad7Module, true /* this is a module load */);

            // TODO: Bind breakpoints when the module loads

            Send(eventObject, AD7ModuleLoadEvent.IID, null);
        }

        // Requests that all programs being debugged by this DE stop execution the next time one of their threads attempts to run.
        // This is normally called in response to the user clicking on the pause button in the debugger.
        // When the break is complete, an AsyncBreakComplete event will be sent back to the debugger.
        int IDebugEngine2.CauseBreak() {
            DebugWriteCommand("CauseBreak");
            AssertMainThread();
            return CauseBreak();
        }

        [Conditional("DEBUG")]
        private static void AssertMainThread() {
            //Debug.Assert(Worker.MainThreadId == Worker.CurrentThreadId);
        }

        // Called by the SDM to indicate that a synchronous debug event, previously sent by the DE to the SDM,
        // was received and processed. The only event we send in this fashion is Program Destroy.
        // It responds to that event by shutting down the engine.
        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 eventObject) {
            DebugWriteCommand("ContinueFromSynchronousEvent");
            AssertMainThread();

            if (eventObject is AD7ProgramDestroyEvent) {
                var debuggedProcess = _process;

                _events = null;
                _process = null;
                _ad7ProgramId = Guid.Empty;
                _threads.Clear();
                _modules.Clear();

                if (_trackFileChanges) {
                    _documentEvents.DocumentSaved -= OnDocumentSaved;
                    _documentEvents = null;
                }

                debuggedProcess.Close();
            } else {
                Debug.Fail("Unknown synchronous event");
            }

            return VSConstants.S_OK;
        }

        // Creates a pending breakpoint in the engine. A pending breakpoint is contains all the information needed to bind a breakpoint to 
        // a location in the debuggee.
        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBpRequest, out IDebugPendingBreakpoint2 ppPendingBp) {
            DebugWriteCommand("CreatePendingBreakpoint");
            Debug.Assert(_breakpointManager != null);
            ppPendingBp = null;

            // Check whether breakpoint request for our language
            var requestInfo = new BP_REQUEST_INFO[1];
            EngineUtils.CheckOk(pBpRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_LANGUAGE | enum_BPREQI_FIELDS.BPREQI_BPLOCATION, requestInfo));
            if (requestInfo[0].guidLanguage != Guids.NodejsDebugLanguage &&
                requestInfo[0].guidLanguage != Guids.ScriptDebugLanguage &&
                requestInfo[0].guidLanguage != Guids.TypeScriptDebugLanguage) {
                // Check whether breakpoint request for our "downloaded" script
                // "Downloaded" script will have our IDebugDocument2
                IDebugDocument2 debugDocument;
                var debugDocumentPosition = Marshal.GetObjectForIUnknown(requestInfo[0].bpLocation.unionmember2) as IDebugDocumentPosition2;
                if (debugDocumentPosition == null || VSConstants.S_OK != debugDocumentPosition.GetDocument(out debugDocument) || null == debugDocument as AD7Document) {
                    // Not ours
                    return VSConstants.E_FAIL;
                }
            }

            _breakpointManager.CreatePendingBreakpoint(pBpRequest, out ppPendingBp);
            return VSConstants.S_OK;
        }

        // Informs a DE that the program specified has been atypically terminated and that the DE should 
        // clean up all references to the program and send a program destroy event.
        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram) {
            DebugWriteCommand("DestroyProgram");

            // Tell the SDM that the engine knows that the program is exiting, and that the
            // engine will send a program destroy. We do this because the Win32 debug api will always
            // tell us that the process exited, and otherwise we have a race condition.
            return (DebuggerConstants.E_PROGRAM_DESTROY_PENDING);
        }

        // Gets the GUID of the DE.
        int IDebugEngine2.GetEngineId(out Guid guidEngine) {
            DebugWriteCommand("GetEngineId");
            guidEngine = DebugEngineGuid;
            return VSConstants.S_OK;
        }

        private static ExceptionHitTreatment GetExceptionTreatment(enum_EXCEPTION_STATE exceptionState) {
            if ((exceptionState & enum_EXCEPTION_STATE.EXCEPTION_STOP_FIRST_CHANCE) != 0) {
                return ExceptionHitTreatment.BreakAlways;
            }

            // UNDONE Handle break on unhandled, once just my code is supported
            // Node has a catch all, so there are no uncaught exceptions
            // For now just break always or never
            //if ((exceptionState & enum_EXCEPTION_STATE.EXCEPTION_STOP_USER_UNCAUGHT) != 0)
            //{
            //    return ExceptionHitTreatment.BreakOnUnhandled;
            //}

            return ExceptionHitTreatment.BreakNever;
        }

        private static void UpdateExceptionTreatment(
            IEnumerable<EXCEPTION_INFO> exceptionInfos,
            Action<ExceptionHitTreatment?, ICollection<KeyValuePair<string, ExceptionHitTreatment>>> updateExceptionTreatment
        ) {
            ExceptionHitTreatment? defaultExceptionTreatment = null;
            var exceptionTreatments = new List<KeyValuePair<string, ExceptionHitTreatment>>();
            bool sendUpdate = false;
            foreach (var exceptionInfo in exceptionInfos) {
                if (exceptionInfo.guidType == DebugEngineGuid) {
                    sendUpdate = true;
                    if (exceptionInfo.bstrExceptionName == "Node.js Exceptions") {
                        defaultExceptionTreatment = GetExceptionTreatment(exceptionInfo.dwState);
                    } else {
                        exceptionTreatments.Add(new KeyValuePair<string, ExceptionHitTreatment>(exceptionInfo.bstrExceptionName, GetExceptionTreatment(exceptionInfo.dwState)));
                    }
                }
            }

            if (sendUpdate) {
                updateExceptionTreatment(defaultExceptionTreatment, exceptionTreatments);
            }
        }

        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType) {
            DebugWriteCommand("RemoveAllSetExceptions");
            if (guidType == DebugEngineGuid || guidType == Guid.Empty) {
                _process.ClearExceptionTreatment();
            }
            return VSConstants.S_OK;
        }

        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException) {
            DebugWriteCommand("RemoveSetException");
            UpdateExceptionTreatment(pException, _process.ClearExceptionTreatment);
            return VSConstants.S_OK;
        }

        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException) {
            DebugWriteCommand("SetException");
            UpdateExceptionTreatment(pException, _process.SetExceptionTreatment);
            return VSConstants.S_OK;
        }

        // Sets the locale of the DE.
        // This method is called by the session debug manager (SDM) to propagate the locale settings of the IDE so that
        // strings returned by the DE are properly localized. The engine is not localized so this is not implemented.
        int IDebugEngine2.SetLocale(ushort wLangId) {
            DebugWriteCommand("SetLocale");
            return VSConstants.S_OK;
        }

        // A metric is a registry value used to change a debug engine's behavior or to advertise supported functionality. 
        // This method can forward the call to the appropriate form of the Debugging SDK Helpers function, SetMetric.
        int IDebugEngine2.SetMetric(string pszMetric, object varValue) {
            DebugWriteCommand("SetMetric");
            return VSConstants.S_OK;
        }

        // Sets the registry root currently in use by the DE. Different installations of Visual Studio can change where their registry information is stored
        // This allows the debugger to tell the engine where that location is.
        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot) {
            DebugWriteCommand("SetRegistryRoot");
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugEngineLaunch2 Members

        // Determines if a process can be terminated.
        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 process) {
            DebugWriteCommand("CanTerminateProcess");
            AssertMainThread();

            Debug.Assert(_events != null);
            Debug.Assert(_process != null);

            int processId = EngineUtils.GetProcessId(process);
            if (processId == _process.Id) {
                return VSConstants.S_OK;
            }

            return VSConstants.S_FALSE;
        }

        // Launches a process by means of the debug engine.
        // Normally, Visual Studio launches a program using the IDebugPortEx2::LaunchSuspended method and then attaches the debugger 
        // to the suspended program. However, there are circumstances in which the debug engine may need to launch a program 
        // (for example, if the debug engine is part of an interpreter and the program being debugged is an interpreted language), 
        // in which case Visual Studio uses the IDebugEngineLaunch2::LaunchSuspended method
        // The IDebugEngineLaunch2::ResumeProcess method is called to start the process after the process has been successfully launched in a suspended state.
        int IDebugEngineLaunch2.LaunchSuspended(string pszServer, IDebugPort2 port, string exe, string args, string dir, string env, string options, enum_LAUNCH_FLAGS launchFlags, uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 ad7Callback, out IDebugProcess2 process) {
            LiveLogger.WriteLine("AD7Engine LaunchSuspended Called with flags '{0}' ({1})", launchFlags, GetHashCode());
            AssertMainThread();

            Debug.Assert(_events == null);
            Debug.Assert(_process == null);
            Debug.Assert(_ad7ProgramId == Guid.Empty);

            _events = ad7Callback;

            var debugOptions = NodeDebugOptions.None;
            List<string[]> dirMapping = null;
            string interpreterOptions = null;
            ushort? debugPort = null;
            if (options != null) {
                var splitOptions = SplitOptions(options);

                foreach (var optionSetting in splitOptions) {
                    var setting = optionSetting.Split(new[] { '=' }, 2);

                    if (setting.Length == 2) {
                        setting[1] = HttpUtility.UrlDecode(setting[1]);

                        switch (setting[0]) {
                            case WaitOnAbnormalExitSetting:
                                bool value;
                                if (Boolean.TryParse(setting[1], out value) && value) {
                                    debugOptions |= NodeDebugOptions.WaitOnAbnormalExit;
                                }
                                break;
                            case WaitOnNormalExitSetting:
                                if (Boolean.TryParse(setting[1], out value) && value) {
                                    debugOptions |= NodeDebugOptions.WaitOnNormalExit;
                                }
                                break;
                            case RedirectOutputSetting:
                                if (Boolean.TryParse(setting[1], out value) && value) {
                                    debugOptions |= NodeDebugOptions.RedirectOutput;
                                }
                                break;
                            case DirMappingSetting:
                                string[] dirs = setting[1].Split('|');
                                if (dirs.Length == 2) {
                                    if (dirMapping == null) {
                                        dirMapping = new List<string[]>();
                                    }
                                    LiveLogger.WriteLine(String.Format("Mapping dir {0} to {1}", dirs[0], dirs[1]));
                                    dirMapping.Add(dirs);
                                }
                                break;
                            case InterpreterOptions:
                                interpreterOptions = setting[1];
                                break;
                            case WebBrowserUrl:
                                _webBrowserUrl = setting[1];
                                break;
                            case DebuggerPort:
                                ushort dbgPortTmp;
                                if (ushort.TryParse(setting[1], out dbgPortTmp)) {
                                    debugPort = dbgPortTmp;
                                }
                                break;
                        }
                    }
                }
            }

            _process =
                new NodeDebugger(
                    exe,
                    args,
                    dir,
                    env,
                    interpreterOptions,
                    debugOptions,
                    debugPort
                );

            _process.Start(false);

            AttachEvents(_process);

            var adProcessId = new AD_PROCESS_ID();
            adProcessId.ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            adProcessId.dwProcessId = (uint)_process.Id;

            EngineUtils.RequireOk(port.GetProcess(adProcessId, out process));
            LiveLogger.WriteLine("AD7Engine LaunchSuspended returning S_OK");
            Debug.Assert(process != null);
            Debug.Assert(!_process.HasExited);

            return VSConstants.S_OK;
        }

        private static IEnumerable<string> SplitOptions(string options) {
            var res = new List<string>();
            int lastStart = 0;
            for (int i = 0; i < options.Length; i++) {
                if (options[i] == ';') {
                    if (i < options.Length - 1 && options[i + 1] != ';') {
                        // valid option boundary
                        res.Add(options.Substring(lastStart, i - lastStart));
                        lastStart = i + 1;
                    } else {
                        i++;
                    }
                }
            }
            if (options.Length - lastStart > 0) {
                res.Add(options.Substring(lastStart, options.Length - lastStart));
            }
            return res;
        }

        // Resume a process launched by IDebugEngineLaunch2.LaunchSuspended
        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 process) {
            DebugWriteCommand("ResumeProcess");
            AssertMainThread();

            if (_events == null) {
                // process failed to start
                LiveLogger.WriteLine("ResumeProcess fails, no events");
                return VSConstants.E_FAIL;
            }

            Debug.Assert(_events != null);
            Debug.Assert(_process != null);
            Debug.Assert(_process != null);
            Debug.Assert(_ad7ProgramId == Guid.Empty);

            int processId = EngineUtils.GetProcessId(process);

            if (processId != _process.Id) {
                LiveLogger.WriteLine("ResumeProcess fails, wrong process");
                return VSConstants.S_FALSE;
            }

            // Send a program node to the SDM. This will cause the SDM to turn around and call IDebugEngine2.Attach
            // which will complete the hookup with AD7
            IDebugPort2 port;
            EngineUtils.RequireOk(process.GetPort(out port));

            var defaultPort = (IDebugDefaultPort2)port;

            IDebugPortNotify2 portNotify;
            EngineUtils.RequireOk(defaultPort.GetPortNotify(out portNotify));

            EngineUtils.RequireOk(portNotify.AddProgramNode(new AD7ProgramNode(_process.Id)));

            if (_ad7ProgramId == Guid.Empty) {
                LiveLogger.WriteLine("ResumeProcess fails, empty program guid");
                Debug.Fail("Unexpected problem -- IDebugEngine2.Attach wasn't called");
                return VSConstants.E_FAIL;
            }

            LiveLogger.WriteLine("ResumeProcess return S_OK");
            return VSConstants.S_OK;
        }

        // This function is used to terminate a process that the engine launched
        // The debugger will call IDebugEngineLaunch2::CanTerminateProcess before calling this method.
        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 process) {
            DebugWriteCommand("TerminateProcess");
            AssertMainThread();

            Debug.Assert(_events != null);
            Debug.Assert(_process != null);

            int processId = EngineUtils.GetProcessId(process);
            if (processId != _process.Id) {
                return VSConstants.S_FALSE;
            }

            _process.Terminate();

            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugProgram2 Members

        // Determines if a debug engine (DE) can detach from the program.
        public int CanDetach() {
            DebugWriteCommand("CanDetach");
            return VSConstants.S_OK;
        }

        // The debugger calls CauseBreak when the user clicks on the pause button in VS. The debugger should respond by entering
        // breakmode. 
        public int CauseBreak() {
            DebugWriteCommand("CauseBreak");
            AssertMainThread();

            _process.BreakAllAsync().Wait();

            return VSConstants.S_OK;
        }

        // Continue is called from the SDM when it wants execution to continue in the debugee
        // but have stepping state remain. An example is when a tracepoint is executed, 
        // and the debugger does not want to actually enter break mode.
        public int Continue(IDebugThread2 pThread) {
            AssertMainThread();

            var thread = (AD7Thread)pThread;
            DebugWriteCommand("Continue");

            // TODO: How does this differ from ExecuteOnThread?
            thread.GetDebuggedThread().Resume();

            return VSConstants.S_OK;
        }

        // Detach is called when debugging is stopped and the process was attached to (as opposed to launched)
        // or when one of the Detach commands are executed in the UI.
        public int Detach() {
            DebugWriteCommand("Detach");
            AssertMainThread();

            _breakpointManager.ClearBreakpointBindingResults();

            _process.Detach();

            // Before unregistering event handlers, make sure that we have received thread exit and process exit events,
            // since we need to report these as AD7 events to VS to gracefully terminate the debugging session.
            _threadExitedEvent.WaitOne(3000);
            _processExitedEvent.WaitOne(3000);

            DetachEvents(_process);
            _ad7ProgramId = Guid.Empty;

            return VSConstants.S_OK;
        }

        // Enumerates the code contexts for a given position in a source file.
        public int EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum) {
            DebugWriteCommand("EnumCodeContexts");

            string filename;
            pDocPos.GetFileName(out filename);
            TEXT_POSITION[] beginning = new TEXT_POSITION[1], end = new TEXT_POSITION[1];

            pDocPos.GetRange(beginning, end);

            ppEnum = new AD7CodeContextEnum(new[] { new AD7MemoryAddress(this, filename, (int)beginning[0].dwLine, (int)beginning[0].dwColumn) });
            return VSConstants.S_OK;
        }

        // EnumCodePaths is used for the step-into specific feature -- right click on the current statment and decide which
        // function to step into. This is not something that we support.
        public int EnumCodePaths(string hint, IDebugCodeContext2 start, IDebugStackFrame2 frame, int fSource, out IEnumCodePaths2 pathEnum, out IDebugCodeContext2 safetyContext) {
            DebugWriteCommand("EnumCodePaths");

            pathEnum = null;
            safetyContext = null;
            return VSConstants.E_NOTIMPL;
        }

        // EnumModules is called by the debugger when it needs to enumerate the modules in the program.
        public int EnumModules(out IEnumDebugModules2 ppEnum) {
            DebugWriteCommand("EnumModules");
            AssertMainThread();

            var moduleObjects = new AD7Module[_modules.Count];
            int i = 0;
            foreach (var keyValue in _modules) {
                var adModule = keyValue.Value;
                moduleObjects[i++] = adModule;
            }

            ppEnum = new AD7ModuleEnum(moduleObjects);

            return VSConstants.S_OK;
        }

        // EnumThreads is called by the debugger when it needs to enumerate the threads in the program.
        public int EnumThreads(out IEnumDebugThreads2 ppEnum) {
            DebugWriteCommand("EnumThreads");
            AssertMainThread();

            var threadObjects = new AD7Thread[_threads.Count];
            int i = 0;
            foreach (var keyValue in _threads) {
                var adThread = keyValue.Value;

                Debug.Assert(adThread != null);
                threadObjects[i++] = adThread;
            }

            ppEnum = new AD7ThreadEnum(threadObjects);

            return VSConstants.S_OK;
        }

        // The properties returned by this method are specific to the program. If the program needs to return more than one property, 
        // then the IDebugProperty2 object returned by this method is a container of additional properties and calling the 
        // IDebugProperty2::EnumChildren method returns a list of all properties.
        // A program may expose any number and type of additional properties that can be described through the IDebugProperty2 interface. 
        // An IDE might display the additional program properties through a generic property browser user interface.
        public int GetDebugProperty(out IDebugProperty2 ppProperty) {
            DebugWriteCommand("GetDebugProperty");
            throw new Exception("The method or operation is not implemented.");
        }

        // The debugger calls this when it needs to obtain the IDebugDisassemblyStream2 for a particular code-context.
        public int GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 codeContext, out IDebugDisassemblyStream2 disassemblyStream) {
            DebugWriteCommand("GetDisassemblyStream");
            disassemblyStream = null;
            return VSConstants.E_NOTIMPL;
        }

        // This method gets the Edit and Continue (ENC) update for this program. A custom debug engine always returns E_NOTIMPL
        public int GetENCUpdate(out object update) {
            DebugWriteCommand("GetENCUpdate");
            update = null;
            return VSConstants.S_OK;
        }

        // Gets the name and identifier of the debug engine (DE) running this program.
        public int GetEngineInfo(out string engineName, out Guid engineGuid) {
            DebugWriteCommand("GetEngineInfo");
            engineName = "Node Engine";
            engineGuid = DebugEngineGuid;
            return VSConstants.S_OK;
        }

        // The memory bytes as represented by the IDebugMemoryBytes2 object is for the program's image in memory and not any memory 
        // that was allocated when the program was executed.
        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            DebugWriteCommand("GetMemoryBytes");
            throw new Exception("The method or operation is not implemented.");
        }

        // Gets the name of the program.
        // The name returned by this method is always a friendly, user-displayable name that describes the program.
        public int GetName(out string programName) {
            // The engine uses default transport and doesn't need to customize the name of the program,
            // so return NULL.
            programName = null;
            return VSConstants.S_OK;
        }

        // Gets a GUID for this program. A debug engine (DE) must return the program identifier originally passed to the IDebugProgramNodeAttach2::OnAttach
        // or IDebugEngine2::Attach methods. This allows identification of the program across debugger components.
        public int GetProgramId(out Guid guidProgramId) {
            DebugWriteCommand("GetProgramId");
            guidProgramId = _ad7ProgramId;
            return guidProgramId == Guid.Empty ? VSConstants.E_FAIL : VSConstants.S_OK;
        }

        // This method is deprecated. Use the IDebugProcess3::Step method instead.

        /// <summary>
        /// Performs a step. 
        /// 
        /// In case there is any thread synchronization or communication between threads, other threads in the program should run when a particular thread is stepping.
        /// </summary>
        public int Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT step) {
            DebugWriteCommand("Step");
            var thread = ((AD7Thread)pThread).GetDebuggedThread();
            switch (sk) {
                case enum_STEPKIND.STEP_INTO: thread.StepInto(); break;
                case enum_STEPKIND.STEP_OUT: thread.StepOut(); break;
                case enum_STEPKIND.STEP_OVER: thread.StepOver(); break;
            }
            return VSConstants.S_OK;
        }

        // Terminates the program.
        public int Terminate() {
            DebugWriteCommand("Terminate");

            // Because we implement IDebugEngineLaunch2 we will terminate
            // the process in IDebugEngineLaunch2.TerminateProcess
            return VSConstants.S_OK;
        }

        // Writes a dump to a file.
        public int WriteDump(enum_DUMPTYPE dumptype, string pszDumpUrl) {
            DebugWriteCommand("WriteDump");
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDebugProgram3 Members

        // ExecuteOnThread is called when the SDM wants execution to continue and have 
        // stepping state cleared.  See http://msdn.microsoft.com/en-us/library/bb145596.aspx for a
        // description of different ways we can resume.
        public int ExecuteOnThread(IDebugThread2 pThread) {
            DebugWriteCommand("ExecuteOnThread");
            AssertMainThread();

            // clear stepping state on the thread the user was currently on
            var thread = (AD7Thread)pThread;
            thread.GetDebuggedThread().ClearSteppingState();

            _process.Resume();

            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugSymbolSettings100 members

        public int SetSymbolLoadState(int bIsManual, int bLoadAdjacent, string strIncludeList, string strExcludeList) {
            DebugWriteCommand("SetSymbolLoadState");

            // The SDM will call this method on the debug engine when it is created, to notify it of the user's
            // symbol settings in Tools->Options->Debugging->Symbols.
            //
            // Params:
            // bIsManual: true if 'Automatically load symbols: Only for specified modules' is checked
            // bLoadAdjacent: true if 'Specify modules'->'Always load symbols next to the modules' is checked
            // strIncludeList: semicolon-delimited list of modules when automatically loading 'Only specified modules'
            // strExcludeList: semicolon-delimited list of modules when automatically loading 'All modules, unless excluded'

            return VSConstants.S_OK;
        }

        #endregion

        #region Deprecated interface methods
        // These methods are not called by the Visual Studio debugger, so they don't need to be implemented

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 programs) {
            Debug.Fail("This function is not called by the debugger");

            programs = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Attach(IDebugEventCallback2 pCallback) {
            Debug.Fail("This function is not called by the debugger");

            return VSConstants.E_NOTIMPL;
        }

        public int GetProcess(out IDebugProcess2 process) {
            Debug.Fail("This function is not called by the debugger");

            process = null;
            return VSConstants.E_NOTIMPL;
        }

        public int Execute() {
            Debug.Fail("This function is not called by the debugger.");
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region Events

        internal void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProgram2 program, IDebugThread2 thread) {
            LiveLogger.WriteLine("AD7Engine Event: {0} ({1})", eventObject.GetType(), iidEvent);

            // Check that events was not disposed
            var events = _events;
            if (events == null) {
                return;
            }

            uint attributes;
            var riidEvent = new Guid(iidEvent);

            EngineUtils.RequireOk(eventObject.GetAttributes(out attributes));

            if ((attributes & (uint)enum_EVENTATTRIBUTES.EVENT_STOPPING) != 0 && thread == null) {
                Debug.Fail("A thread must be provided for a stopping event");
                return;
            }

            try {
                EngineUtils.RequireOk(events.Event(this, null, program, thread, eventObject, ref riidEvent, attributes));
            } catch (InvalidCastException) {
                // COM object has gone away
            }
        }

        internal void Send(IDebugEvent2 eventObject, string iidEvent, IDebugThread2 thread) {
            Send(eventObject, iidEvent, this, thread);
        }

        private void AttachEvents(NodeDebugger process) {
            process.ProcessLoaded += OnProcessLoaded;
            process.ModuleLoaded += OnModuleLoaded;
            process.ThreadCreated += OnThreadCreated;

            process.BreakpointBound += OnBreakpointBound;
            process.BreakpointUnbound += OnBreakpointUnbound;
            process.BreakpointBindFailure += OnBreakpointBindFailure;

            process.BreakpointHit += OnBreakpointHit;
            process.AsyncBreakComplete += OnAsyncBreakComplete;
            process.ExceptionRaised += OnExceptionRaised;
            process.ProcessExited += OnProcessExited;
            process.EntryPointHit += OnEntryPointHit;
            process.StepComplete += OnStepComplete;
            process.ThreadExited += OnThreadExited;
            process.DebuggerOutput += OnDebuggerOutput;

            // Subscribe to document changes if Edit and Continue is enabled.
            var shell = (IVsShell)Package.GetGlobalService(typeof(SVsShell));
            if (shell != null) {
                // The debug engine is loaded by VS separately from the main NTVS package, so we
                // need to make sure that the package is also loaded before querying its options.
                var packageGuid = new Guid(Guids.NodejsPackageString);
                IVsPackage package;
                shell.LoadPackage(ref packageGuid, out package);

                var nodejsPackage = package as NodejsPackage;
                if (nodejsPackage != null) {
                    _trackFileChanges = nodejsPackage.GeneralOptionsPage.EditAndContinue;

                    if (_trackFileChanges) {
                        _documentEvents = nodejsPackage.DTE.Events.DocumentEvents;
                        _documentEvents.DocumentSaved += OnDocumentSaved;
                    }
                }
            }

            process.StartListening();
        }

        private void DetachEvents(NodeDebugger process) {
            process.ProcessLoaded -= OnProcessLoaded;
            process.ModuleLoaded -= OnModuleLoaded;
            process.ThreadCreated -= OnThreadCreated;

            process.BreakpointBound -= OnBreakpointBound;
            process.BreakpointUnbound -= OnBreakpointUnbound;
            process.BreakpointBindFailure -= OnBreakpointBindFailure;

            process.BreakpointHit -= OnBreakpointHit;
            process.AsyncBreakComplete -= OnAsyncBreakComplete;
            process.ExceptionRaised -= OnExceptionRaised;
            process.ProcessExited -= OnProcessExited;
            process.EntryPointHit -= OnEntryPointHit;
            process.StepComplete -= OnStepComplete;
            process.ThreadExited -= OnThreadExited;
            process.DebuggerOutput -= OnDebuggerOutput;

            if (_documentEvents != null) {
                _documentEvents.DocumentSaved -= OnDocumentSaved;
            }
        }

        private void OnThreadExited(object sender, ThreadEventArgs e) {
            // TODO: Thread exit code
            AD7Thread oldThread;
            _threads.TryGetValue(e.Thread, out oldThread);
            _threads.Remove(e.Thread);

            _threadExitedEvent.Set();

            if (oldThread != null) {
                Send(new AD7ThreadDestroyEvent(0), AD7ThreadDestroyEvent.IID, oldThread);
            }
        }

        private void OnThreadCreated(object sender, ThreadEventArgs e) {
            LiveLogger.WriteLine("Thread created: " + e.Thread.Id);

            lock (_syncLock) {
                var newThread = new AD7Thread(this, e.Thread);

                // Treat first thread created as main thread
                // Should only be one for Node
                Debug.Assert(_mainThread == null);
                if (_mainThread == null) {
                    _mainThread = newThread;
                }

                _threads.Add(e.Thread, newThread);
                if (_loadComplete) {
                    SendThreadCreate(newThread);
                }
            }
        }

        public static List<IVsDocumentPreviewer> GetDefaultBrowsers() {
            var browserList = new List<IVsDocumentPreviewer>();
            var doc3 = (IVsUIShellOpenDocument3)NodejsPackage.Instance.GetService(typeof(SVsUIShellOpenDocument));
            IVsEnumDocumentPreviewers previewersEnum = doc3.DocumentPreviewersEnum;

            var rgPreviewers = new IVsDocumentPreviewer[1];
            uint celtFetched;
            while (ErrorHandler.Succeeded(previewersEnum.Next(1, rgPreviewers, out celtFetched)) && celtFetched == 1) {
                if (rgPreviewers[0].IsDefault && !string.IsNullOrEmpty(rgPreviewers[0].Path)) {
                    browserList.Add(rgPreviewers[0]);
                }
            }
            return browserList;
        }


        private void OnEntryPointHit(object sender, ThreadEventArgs e) {
            Send(new AD7EntryPointEvent(), AD7EntryPointEvent.IID, _threads[e.Thread]);
        }

        private void LaunchBrowserDebugger() {
            LiveLogger.WriteLine("LaunchBrowserDebugger Started");

            var vsDebugger = (IVsDebugger2)ServiceProvider.GlobalProvider.GetService(typeof(SVsShellDebugger));

            var info = new VsDebugTargetInfo2();
            var infoSize = Marshal.SizeOf(info);
            info.cbSize = (uint)infoSize;
            info.bstrExe = _webBrowserUrl;
            info.dlo = (uint)_DEBUG_LAUNCH_OPERATION3.DLO_LaunchBrowser;
            var defaultBrowsers = GetDefaultBrowsers();
            if (defaultBrowsers.Count != 1 || defaultBrowsers[0].DisplayName != "Internet Explorer") {
                // if we use UseDefaultBrowser we lose the nice control & debugging of IE, so
                // instead launch w/ no debugging when the user has selected a browser other than IE.
                info.LaunchFlags |= (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd |
                                    (uint)__VSDBGLAUNCHFLAGS4.DBGLAUNCH_UseDefaultBrowser |
                                    (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug;
            }

            info.guidLaunchDebugEngine = DebugEngineGuid;
            IntPtr infoPtr = Marshal.AllocCoTaskMem(infoSize);
            Marshal.StructureToPtr(info, infoPtr, false);

            try {
                vsDebugger.LaunchDebugTargets2(1, infoPtr);
            } finally {
                if (infoPtr != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(infoPtr);
                }
            }

            LiveLogger.WriteLine("LaunchBrowserDebugger Completed");
        }

        private void OnStepComplete(object sender, ThreadEventArgs e) {
            Send(new AD7SteppingCompleteEvent(), AD7SteppingCompleteEvent.IID, _threads[e.Thread]);
        }

        private void OnProcessLoaded(object sender, ThreadEventArgs e) {
            lock (_syncLock) {
                _processLoaded = true;
                HandleLoadComplete();
            }
        }

        private void OnProcessExited(object sender, ProcessExitedEventArgs e) {
            try {
                _processExitedEvent.Set();
                lock (_syncLock) {
                    _processLoaded = false;
                    Send(new AD7ProgramDestroyEvent((uint)e.ExitCode), AD7ProgramDestroyEvent.IID, null);
                }
            } catch (InvalidOperationException) {
                // we can race at shutdown and deliver the event after the debugger is shutting down.
            }
        }

        private void OnModuleLoaded(object sender, ModuleLoadedEventArgs e) {
            lock (_syncLock) {
                var adModule = _modules[e.Module] = new AD7Module(e.Module);
                if (_loadComplete) {
                    SendModuleLoad(adModule);
                }
            }
        }

        private void OnExceptionRaised(object sender, ExceptionRaisedEventArgs e) {
            // Exception events are sent when an exception occurs in the debuggee that the debugger was not expecting.
            AD7Thread thread;
            if (_threads.TryGetValue(e.Thread, out thread)) {
                Send(
                    new AD7DebugExceptionEvent(e.Exception.TypeName, e.Exception.Description, e.IsUnhandled, this),
                    AD7DebugExceptionEvent.IID,
                    thread
                );
            }
        }

        private void OnBreakpointHit(object sender, BreakpointHitEventArgs e) {
            var boundBreakpoint = _breakpointManager.GetBoundBreakpoint(e.BreakpointBinding);
            Send(new AD7BreakpointEvent(new AD7BoundBreakpointsEnum(new[] { boundBreakpoint })), AD7BreakpointEvent.IID, _threads[e.Thread]);
        }

        private void OnBreakpointBound(object sender, BreakpointBindingEventArgs e) {
            var pendingBreakpoint = _breakpointManager.GetPendingBreakpoint(e.Breakpoint);
            var breakpointBinding = e.BreakpointBinding;
            var codeContext = new AD7MemoryAddress(this, pendingBreakpoint.DocumentName, breakpointBinding.Target.Line, breakpointBinding.Target.Column);
            var documentContext = new AD7DocumentContext(codeContext);
            var breakpointResolution = new AD7BreakpointResolution(this, breakpointBinding, documentContext);
            var boundBreakpoint = new AD7BoundBreakpoint(breakpointBinding, pendingBreakpoint, breakpointResolution, breakpointBinding.Enabled);
            _breakpointManager.AddBoundBreakpoint(breakpointBinding, boundBreakpoint);
            Send(
                new AD7BreakpointBoundEvent(pendingBreakpoint, boundBreakpoint),
                AD7BreakpointBoundEvent.IID,
                null
            );
        }

        private void OnBreakpointUnbound(object sender, BreakpointBindingEventArgs e) {
            var breakpointBinding = e.BreakpointBinding;
            var boundBreakpoint = _breakpointManager.GetBoundBreakpoint(breakpointBinding);
            if (boundBreakpoint != null) {
                _breakpointManager.RemoveBoundBreakpoint(breakpointBinding);
                Send(
                    new AD7BreakpointUnboundEvent(boundBreakpoint),
                    AD7BreakpointUnboundEvent.IID,
                    null
                );
            }
        }

        private void OnBreakpointBindFailure(object sender, BreakpointBindingEventArgs e) {
            var pendingBreakpoint = _breakpointManager.GetPendingBreakpoint(e.Breakpoint);
            var breakpointErrorEvent = new AD7BreakpointErrorEvent(pendingBreakpoint, this);
            pendingBreakpoint.AddBreakpointError(breakpointErrorEvent);
            Send(breakpointErrorEvent, AD7BreakpointErrorEvent.IID, null);
        }

        private void OnAsyncBreakComplete(object sender, ThreadEventArgs e) {
            AD7Thread thread;
            if (!_threads.TryGetValue(e.Thread, out thread)) {
                _threads[e.Thread] = thread = new AD7Thread(this, e.Thread);
            }
            Send(new AD7AsyncBreakCompleteEvent(), AD7AsyncBreakCompleteEvent.IID, thread);
        }

        private void OnDebuggerOutput(object sender, OutputEventArgs e) {
            AD7Thread thread = null;
            if (e.Thread != null && !_threads.TryGetValue(e.Thread, out thread)) {
                _threads[e.Thread] = thread = new AD7Thread(this, e.Thread);
            }

            // thread can be null for an output string event because it is not
            // a stopping event.
            Send(new AD7DebugOutputStringEvent2(e.Output), AD7DebugOutputStringEvent2.IID, thread);
        }

        private void OnDocumentSaved(Document document) {
            var module = Process.GetModuleForFilePath(document.FullName);
            if (module == null) {
                return;
            }

            // For .ts files, we need to build the project to regenerate .js code.
            if (String.Equals(Path.GetExtension(module.FileName), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                if (document.ProjectItem.ContainingProject.GetNodeProject().Build(null, null) != MSBuildResult.Successful) {
                    var statusBar = (IVsStatusbar)ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar));
                    statusBar.SetText(SR.GetString(SR.DebuggerModuleUpdateFailed));
                    return;
                }
            }

            DebuggerClient.RunWithRequestExceptionsHandled(async () => {
                if (!await Process.UpdateModuleSourceAsync(module).ConfigureAwait(false)) {
                    var statusBar = (IVsStatusbar)ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar));
                    statusBar.SetText(SR.GetString(SR.DebuggerModuleUpdateFailed));
                }
            });
        }

        #endregion

        internal static void MapLanguageInfo(string filename, out string pbstrLanguage, out Guid pguidLanguage) {
            if (String.Equals(Path.GetExtension(filename), NodejsConstants.TypeScriptExtension, StringComparison.OrdinalIgnoreCase)) {
                pbstrLanguage = NodejsConstants.TypeScript;
                pguidLanguage = Guids.TypeScriptDebugLanguage;
            } else {
                pbstrLanguage = NodejsConstants.JavaScript;
                pguidLanguage = Guids.NodejsDebugLanguage;
            }
        }

        /// <summary>
        /// Enumerates files in the solution projects.
        /// </summary>
        /// <returns>File names collection.</returns>
        private IEnumerable<string> EnumerateSolutionFiles() {
            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (solution != null) {
                foreach (IVsProject project in solution.EnumerateLoadedProjects(false)) {
                    foreach (uint itemid in project.EnumerateProjectItems()) {
                        string moniker;
                        if (ErrorHandler.Succeeded(project.GetMkDocument(itemid, out moniker)) && moniker != null) {
                            yield return moniker;
                        }
                    }
                }
            }
        }

        private void DebugWriteCommand(string commandName) {
            LiveLogger.WriteLine("AD7Engine Called " + commandName);
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;
using Process = System.Diagnostics.Process;

namespace Microsoft.VisualStudioTools
{
    internal class VisualStudioApp : IDisposable
    {
        private static readonly Dictionary<int, VisualStudioApp> KnownInstances = new Dictionary<int, VisualStudioApp>();
        private readonly int processId;

        public static VisualStudioApp FromProcessId(int processId)
        {
            VisualStudioApp inst;
            lock (KnownInstances)
            {
                if (!KnownInstances.TryGetValue(processId, out inst))
                {
                    KnownInstances[processId] = inst = new VisualStudioApp(processId);
                }
            }
            return inst;
        }

        public static VisualStudioApp FromEnvironmentVariable(string variable)
        {
            var pid = Environment.GetEnvironmentVariable(variable);
            if (pid == null)
            {
                return null;
            }

            if (!int.TryParse(pid, out var processId))
            {
                return null;
            }

            return FromProcessId(processId);
        }

        private VisualStudioApp(int processId)
        {
            this.processId = processId;
        }

        public void Dispose()
        {
            lock (KnownInstances)
            {
                KnownInstances.Remove(this.processId);
            }
        }

        // Source from
        //  http://blogs.msdn.com/b/kirillosenkov/archive/2011/08/10/how-to-get-dte-from-visual-studio-process-id.aspx
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public DTE GetDTE()
        {
            var dte = GetDTE(this.processId);
            if (dte == null)
            {
                throw new InvalidOperationException("Could not find VS DTE object for process " + this.processId);
            }
            return dte;
        }

        private static bool DTELoaded = false;

        private static DTE GetDTE(int processId)
        {
            // VS 2017 doesn't install some assemblies to the GAC that are needed to work with the
            // debugger, and as the tests don't execute in the devenv.exe process, those assemblies
            // fail to load - so load them manually from PublicAssemblies.

            // Use the executable name, as this is only needed for the out of proc test execution
            // that may interact with the debugger (vstest.executionengine.x86.exe).
            if (!DTELoaded)
            {
                var currentProc = Process.GetCurrentProcess().MainModule.FileName;
                if (StringComparer.OrdinalIgnoreCase.Equals(
                        Path.GetFileName(currentProc), "vstest.executionengine.x86.exe"))
                {
                    var baseDir = Path.GetDirectoryName(currentProc);
                    var publicAssemblies = Path.Combine(baseDir, "..\\..\\..\\PublicAssemblies");

                    Assembly.LoadFrom(Path.Combine(publicAssemblies, "Microsoft.VisualStudio.OLE.Interop.dll"));
                    Assembly.LoadFrom(Path.Combine(publicAssemblies, "envdte90.dll"));
                    Assembly.LoadFrom(Path.Combine(publicAssemblies, "envdte80.dll"));
                    Assembly.LoadFrom(Path.Combine(publicAssemblies, "envdte.dll"));
                }
                DTELoaded = true;
            }

            MessageFilter.Register();

            var prefix = Process.GetProcessById(processId).ProcessName;
            if ("devenv".Equals(prefix, StringComparison.OrdinalIgnoreCase))
            {
                prefix = "VisualStudio";
            }

            var progId = $"!{prefix}.DTE.15.0:{processId}";
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                var moniker = new IMoniker[1];
                uint numberFetched = 0;
                while (enumMonikers.Next(1, moniker, out numberFetched) == 0)
                {
                    var runningObjectMoniker = moniker[0];
                    string name = null;

                    try
                    {
                        if (runningObjectMoniker != null)
                        {
                            runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    if (StringComparer.Ordinal.Equals(name, progId))
                    {
                        rot.GetObject(runningObjectMoniker, out runningObject);
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            return (DTE)runningObject;
        }

        private static readonly Guid WebkitDebuggerGuid = Guid.Parse("4cc6df14-0ab5-4a91-8bb4-eb0bf233d0fe");
        private static readonly Guid WebkitPortSupplierGuid = Guid.Parse("4103f338-2255-40c0-acf5-7380e2bea13d");
        internal static readonly Guid WebKitDebuggerV2Guid = Guid.Parse("30d423cc-6d0b-4713-b92d-6b2a374c3d89");

        private static readonly Guid Node2AttachEngineGuid = Guid.Parse("3F14B534-C345-44B5-AF84-642246EEEB62");

        public bool AttachToProcessNode2DebugAdapter(int port)
        {
            var dte = (VisualStudio.OLE.Interop.IServiceProvider)GetDTE();

            var serviceProvider = new ServiceProvider(dte);

            // setup debug info and attach
            var pDebugEngine = Marshal.AllocCoTaskMem(Marshal.SizeOf<Guid>());
            var debugUri = $"http://127.0.0.1:{port}";
            try
            {
                Marshal.StructureToPtr(Node2AttachEngineGuid, pDebugEngine, false);
                var dbgInfo = new VsDebugTargetInfo4()
                {
                    dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_AlreadyRunning,
                    pDebugEngines = pDebugEngine,
                    dwDebugEngineCount = 1,
                    bstrExe = "dummy",
                    guidPortSupplier = WebkitPortSupplierGuid,
                    bstrPortName = debugUri,
                    dwProcessId = 1
                };

                var launchResults = new VsDebugTargetProcessInfo[1];
                var debugger = (IVsDebugger4)serviceProvider.GetService(typeof(SVsShellDebugger));
                debugger.LaunchDebugTargets4(1, new[] { dbgInfo }, launchResults);
            }
            finally
            {
                if (pDebugEngine != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pDebugEngine);
                }
            }

            return true;
        }

        public bool AttachToProcess(ProcessOutput processOutput, Guid portSupplier, string transportQualifierUri)
        {
            var debugger3 = (EnvDTE90.Debugger3)GetDTE().Debugger;
            var transports = debugger3.Transports;
            EnvDTE80.Transport transport = null;
            for (var i = 1; i <= transports.Count; ++i)
            {
                var t = transports.Item(i);
                if (Guid.Parse(t.ID) == portSupplier)
                {
                    transport = t;
                    break;
                }
            }
            if (transport == null)
            {
                return false;
            }

            var processes = debugger3.GetProcesses(transport, transportQualifierUri);
            if (processes.Count < 1)
            {
                return false;
            }

            var process = processes.Item(1);
            return AttachToProcess(processOutput, process);
        }

        public bool AttachToProcess(ProcessOutput processOutput, EnvDTE.Process process, Guid[] engines = null)
        {
            // Retry the attach itself 3 times before displaying a Retry/Cancel
            // dialog to the user.
            var dte = GetDTE();
            dte.SuppressUI = true;

            try
            {
                if (engines == null)
                {
                    process.Attach();
                }
                else
                {
                    var process3 = process as EnvDTE90.Process3;
                    if (process3 == null)
                    {
                        return false;
                    }
                    process3.Attach2(engines.Select(engine => engine.ToString("B")).ToArray());
                }
                return true;
            }
            catch (COMException)
            {
                if (processOutput.Wait(TimeSpan.FromMilliseconds(500)))
                {
                    // Process exited while we were trying
                    return false;
                }
            }
            finally
            {
                dte.SuppressUI = false;
            }

            // Another attempt, but display UI.
            process.Attach();
            return true;
        }
    }

    public class MessageFilter : IOleMessageFilter
    {
        // Start the filter.
        public static void Register()
        {
            var newFilter = new MessageFilter();
            CoRegisterMessageFilter(newFilter, out var oldFilter);
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            CoRegisterMessageFilter(null, out var oldFilter);
        }

        private const int SERVERCALL_ISHANDLED = 0;
        private const int SERVERCALL_RETRYLATER = 2;
        private const int PENDINGMSG_WAITDEFPROCESS = 2;

        private MessageFilter() { }

        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
                                                 IntPtr hTaskCaller,
                                                 int dwTickCount,
                                                 IntPtr lpInterfaceInfo)
        {
            return SERVERCALL_ISHANDLED;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == SERVERCALL_RETRYLATER && dwTickCount < 10000)
            {
                // Retry the thread call after 250ms
                return 250;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
        {
            return PENDINGMSG_WAITDEFPROCESS;
        }

        // Implement the IOleMessageFilter interface.
        [SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("Ole32.dll")]
        private static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);
    }

    [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleMessageFilter
    {
        [PreserveSig]
        int HandleInComingCall(int dwCallType,
                               IntPtr hTaskCaller,
                               int dwTickCount,
                               IntPtr lpInterfaceInfo);

        [PreserveSig]
        int RetryRejectedCall(IntPtr hTaskCallee,
                              int dwTickCount,
                              int dwRejectType);

        [PreserveSig]
        int MessagePending(IntPtr hTaskCallee,
                           int dwTickCount,
                           int dwPendingType);
    }
}

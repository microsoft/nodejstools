// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Wrapper around Process which handles our wait for exit logic
    /// </summary>
    internal sealed class NodeProcess : IDisposable
    {
        private readonly ProcessStartInfo _psi;
        private readonly bool _waitOnAbnormal, _waitOnNormal, _enableRaisingEvents;
        private Process _process;
        private Process _pressAnyKeyProcess;

        public ushort DebuggerPort { get; }

        public NodeProcess(ProcessStartInfo psi, bool waitOnAbnormal, bool waitOnNormal, bool enableRaisingEvents, ushort debuggerPort = 0) 
        {
            this._psi = psi;
            this._waitOnAbnormal = waitOnAbnormal;
            this._waitOnNormal = waitOnNormal;
            this._enableRaisingEvents = enableRaisingEvents;
            DebuggerPort = debuggerPort;
        }

        public static NodeProcess Start(ProcessStartInfo psi, bool waitOnAbnormal, bool waitOnNormal)
        {
            var res = new NodeProcess(psi, waitOnAbnormal, waitOnNormal, false);
            res.Start();
            return res;
        }

        public void ResponseToTerminateEvent(object sender, EventArgs e)
        {
            this.Kill();
        }

        public void Start()
        {
            string waitMode;
            if (this._waitOnNormal && this._waitOnAbnormal)
            {
                waitMode = "both";
            }
            else if (this._waitOnAbnormal)
            {
                waitMode = "abnormal";
            }
            else if (this._waitOnNormal)
            {
                waitMode = "normal";
            }
            else
            {
                waitMode = null;
            }

            if (waitMode != null)
            {
                var pidFile = Path.GetTempFileName();
                this._psi.Arguments = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}",
                    waitMode,
                    ProcessOutput.QuoteSingleArgument(pidFile),
                    ProcessOutput.QuoteSingleArgument(this._psi.FileName),
                    this._psi.Arguments
                );
                this._psi.FileName = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Microsoft.NodejsTools.PressAnyKey.exe"
                );
                var process = this._pressAnyKeyProcess = Process.Start(this._psi);
                int? pid = null;
                while (!process.HasExited)
                {
                    if (new FileInfo(pidFile).Length == 0)
                    {
                        System.Threading.Thread.Sleep(10);
                        continue;
                    }

                    try
                    {
                        var strPid = File.ReadAllText(pidFile);
                        if (int.TryParse(strPid, out var pidValue))
                        {
                            pid = pidValue;
                            break;
                        }
                    }
                    catch (IOException)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }

                if (pid == null)
                {
                    throw new Win32Exception("failed to start proess");
                }
                this._process = Process.GetProcessById(pid.Value);
            }
            else
            {
                this._process = Process.Start(this._psi);
            }
            this._process.EnableRaisingEvents = this._enableRaisingEvents;
        }

        public void WaitForExit()
        {
            if (this._process == null)
            {
                return;
            }
            this._process.WaitForExit();
        }

        public bool WaitForExit(int milliseconds)
        {
            if (this._process == null)
            {
                return true;
            }
            return this._process.WaitForExit(milliseconds);
        }

        public bool HasExited
        {
            get
            {
                if (this._process == null)
                {
                    return false;
                }
                return this._process.HasExited;
            }
        }

        public int Id
        {
            get
            {
                if (this._process == null)
                {
                    throw new InvalidOperationException();
                }
                return this._process.Id;
            }
        }

        internal void Kill()
        {
            if (this._pressAnyKeyProcess != null)
            {
                this._pressAnyKeyProcess.Kill();
            }

            if (this._process != null)
            {
                this._process.Kill();
            }
        }

        public int ExitCode
        {
            get
            {
                if (this._process == null)
                {
                    throw new InvalidOperationException();
                }
                return this._process.ExitCode;
            }
        }

        public void Dispose()
        {
            if (this._pressAnyKeyProcess != null)
            {
                this._pressAnyKeyProcess.Dispose();
            }
            if (this._process != null)
            {
                this._process.Dispose();
            }
        }
    }
}

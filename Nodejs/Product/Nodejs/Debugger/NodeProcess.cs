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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Wrapper around Process which handles our wait for exit logic
    /// </summary>
    sealed class NodeProcess : IDisposable {
        private readonly ProcessStartInfo _psi;
        private readonly bool _waitOnAbnormal, _waitOnNormal, _enableRaisingEvents;
        private Process _process;

        public NodeProcess(ProcessStartInfo psi, bool waitOnAbnormal, bool waitOnNormal, bool enableRaisingEvents) {
            _psi = psi;
            _waitOnAbnormal = waitOnAbnormal;
            _waitOnNormal = waitOnNormal;
            _enableRaisingEvents = enableRaisingEvents;
        }

        public static NodeProcess Start(ProcessStartInfo psi, bool waitOnAbnormal, bool waitOnNormal) {
            var res = new NodeProcess(psi, waitOnAbnormal, waitOnNormal, false);
            res.Start();
            return res;
        }

        public void Start() {
            string waitMode;
            if (_waitOnNormal && _waitOnAbnormal) {
                waitMode = "both";
            } else if (_waitOnAbnormal) {
                waitMode = "abnormal";
            } else if (_waitOnNormal) {
                waitMode = "normal";
            } else {
                waitMode = null;
            }

            if (waitMode != null) {
                var pidFile = Path.GetTempFileName();
                _psi.Arguments = String.Format("{0} {1} {2} {3}",
                    waitMode,
                    ProcessOutput.QuoteSingleArgument(pidFile),
                    ProcessOutput.QuoteSingleArgument(_psi.FileName),
                    _psi.Arguments
                );
                _psi.FileName = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Microsoft.NodejsTools.PressAnyKey.exe"
                );
                var process = Process.Start(_psi);
                int? pid = null;
                while (!process.HasExited) {
                    if (new FileInfo(pidFile).Length == 0) {
                        System.Threading.Thread.Sleep(10);
                        continue;
                    }

                    try {
                        string strPid = File.ReadAllText(pidFile);
                        int pidValue;
                        if (Int32.TryParse(strPid, out pidValue)) {
                            pid = pidValue;
                            break;
                        }
                    } catch (IOException) {
                        System.Threading.Thread.Sleep(10);
                    }
                }

                if (pid == null) {
                    throw new Win32Exception("failed to start proess");
                }
                _process = Process.GetProcessById(pid.Value);
            } else {
                _process = Process.Start(_psi);
            }
            _process.EnableRaisingEvents = _enableRaisingEvents;
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

        public bool HasExited {
            get {
                if (_process == null) {
                    return false;
                }
                return _process.HasExited;
            }
        }

        public int Id {
            get {
                if (_process == null) {
                    throw new InvalidOperationException();
                }
                return _process.Id;
            }
        }

        internal void Kill() {
            if (_process == null) {
                return;
            }
            _process.Kill();
        }

        public int ExitCode {
            get {
                if (_process == null) {
                    throw new InvalidOperationException();
                }
                return _process.ExitCode;
            }
        }

        public void Dispose() {
            if (_process != null) {
                _process.Dispose();
            }
        }
    }
}

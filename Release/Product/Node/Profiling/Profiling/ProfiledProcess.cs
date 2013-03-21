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
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.Profiling {
    class ProfiledProcess {
        private readonly string _exe, _args, _dir;
        private readonly ProcessorArchitecture _arch;
        private readonly Process _process;

        public ProfiledProcess(string exe, string args, string dir, Dictionary<string, string> envVars, ProcessorArchitecture arch) {
            if (arch != ProcessorArchitecture.X86 && arch != ProcessorArchitecture.Amd64) {
                throw new InvalidOperationException(String.Format("Unsupported architecture: {0}", arch));
            }
            if (dir.EndsWith("\\")) {
                dir = dir.Substring(0, dir.Length - 1);
            }
            if (String.IsNullOrEmpty(dir)) {
                dir = ".";
            }
            _exe = exe;
            _args = args;
            _dir = dir;
            _arch = arch;

            var processInfo = new ProcessStartInfo(_exe);
            processInfo.WorkingDirectory = dir;

            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = false;

            if (envVars != null) {
                foreach (var keyValue in envVars) {
                    processInfo.EnvironmentVariables[keyValue.Key] = keyValue.Value;
                }
            }

            processInfo.Arguments = "--prof " + args;
            _process = new Process();
            _process.StartInfo = processInfo;
        }

        public void StartProfiling(string filename) {
            _process.EnableRaisingEvents = true;
            _process.Exited += (sender, args) => {
                var executionTime = _process.ExitTime.Subtract(_process.StartTime);

                var psi = new ProcessStartInfo(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "Microsoft.NodeTools.NodeLogConverter.exe"
                        ),
                        "\"" + Path.Combine(_dir, "v8.log") + "\" " +
                        "\"" + filename + "\" " +
                        "\"" + _process.StartTime.ToString() + "\" " +
                        "\"" + executionTime.ToString() + "\""
                    );

                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                var convertProcess = Process.Start(psi);
                convertProcess.WaitForExit();

                var procExited = ProcessExited;
                if (procExited != null) {
                    procExited(this, EventArgs.Empty);
                }
            };

            _process.Start();
        }

        public event EventHandler ProcessExited;

        internal void StopProfiling() {
            _process.Kill();
        }
    }
}

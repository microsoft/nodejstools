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

        public ProfiledProcess(string exe, string script, string args, string dir, Dictionary<string, string> envVars, ProcessorArchitecture arch) {
            if (arch != ProcessorArchitecture.X86 && arch != ProcessorArchitecture.Amd64) {
                throw new InvalidOperationException(String.Format("Unsupported architecture: {0}", arch));
            }
            if (dir.EndsWith("\\")) {
                dir = dir.Substring(0, dir.Length - 1);
            }
            if (String.IsNullOrEmpty(dir)) {
                // run from where the script is by default (the UI enforces this)
                Debug.Assert(Path.IsPathRooted(script));
                dir = Path.GetDirectoryName(script);
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

            processInfo.Arguments = String.Format("--prof \"{0}\" {1}", script, args);
            _process = new Process();
            _process.StartInfo = processInfo;
        }

        public void StartProfiling(string filename) {
            _process.EnableRaisingEvents = true;
            _process.Exited += (sender, args) => {
                var executionTime = _process.ExitTime.Subtract(_process.StartTime);
                var v8log = Path.Combine(_dir, "v8.log");
                if (!File.Exists(v8log)) {
                    MessageBox.Show(String.Format("v8 log file was not successfully saved to:\r\n{0}\r\n\r\nNo profiling data is available.", v8log));
                    return;
                }   

                var psi = new ProcessStartInfo(
                        Path.Combine(
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "Microsoft.NodejsTools.NodeLogConverter.exe"
                        ),
                        "\"" + v8log + "\" " +
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
                    try {
                        File.Delete(Path.Combine(_dir, "v8.log"));
                    } catch {
                        // file in use, multiple node.exe's running, user trying
                        // to profile multiple times, etc...
                        MessageBox.Show("Unable to delete v8.log.\r\n\r\n" +
                                        "There is probably a second copy of node.exe running and the\r\n" +
                                        "results may be unavailable or incorrect.\r\n");
                    }
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

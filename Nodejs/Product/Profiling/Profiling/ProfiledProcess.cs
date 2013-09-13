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
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Profiling {
    class ProfiledProcess {
        private readonly string _exe, _args, _dir, _launchUrl;
        private readonly ProcessorArchitecture _arch;
        private readonly Process _process;
        private readonly int? _port;
        private readonly bool _startBrowser;

        public ProfiledProcess(string exe, string interpreterArgs, string script, string scriptArgs, string dir, Dictionary<string, string> envVars, ProcessorArchitecture arch, string launchUrl, int? port, bool startBrowser) {
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
            _args = interpreterArgs;
            _dir = dir;
            _arch = arch;
            _launchUrl = launchUrl;
            _port = port;
            _startBrowser = startBrowser;

            var processInfo = new ProcessStartInfo(_exe);
            processInfo.WorkingDirectory = dir;
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = false;

            if (_startBrowser && _port == null) {
                _port = GetFreePort();
            }
            if (_port != null) {
                if (envVars == null) {
                    envVars = new Dictionary<string, string>();
                }

                envVars["PORT"] = port.ToString();
            }

            if (envVars != null) {
                foreach (var keyValue in envVars) {
                    processInfo.EnvironmentVariables[keyValue.Key] = keyValue.Value;
                }
            }

            processInfo.Arguments = String.Format("{1} --prof \"{0}\" {2}", script, interpreterArgs, scriptArgs);
            _process = new Process();
            _process.StartInfo = processInfo;
        }

        public void StartInBrowser(string url) {
            VsShellUtilities.OpenBrowser(url, (uint)__VSOSPFLAGS.OSP_LaunchNewBrowser);
        }

        class BrowserStartInfo {
            public readonly int Port;
            public readonly string Url;

            public BrowserStartInfo(int port, string url) {
                Port = port;
                Url = url;
            }
        }

        private void StartBrowser(object browserStart) {
            var startInfo = (BrowserStartInfo)browserStart;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Blocking = true;
            for (int i = 0; i < 100 && !_process.HasExited; i++) {
                try {
                    socket.Connect(IPAddress.Loopback, startInfo.Port);
                    break;
                } catch {
                    System.Threading.Thread.Sleep(100);
                }
            }
            socket.Close();
            if (!_process.HasExited) {
                StartInBrowser(startInfo.Url);
            }
        }

        private static int GetFreePort() {
            return Enumerable.Range(new Random().Next(1200, 2000), 60000).Except(
                from connection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                select connection.LocalEndPoint.Port
            ).First();
        }

        public void StartProfiling(string filename) {
            _process.EnableRaisingEvents = true;
            _process.Exited += (sender, args) => {
                try {
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
                } finally {
                    var procExited = ProcessExited;
                    if (procExited != null) {
                        procExited(this, EventArgs.Empty);
                    }
                }

                try {
                    File.Delete(Path.Combine(_dir, "v8.log"));
                } catch {
                    // file in use, multiple node.exe's running, user trying
                    // to profile multiple times, etc...
                    MessageBox.Show("Unable to delete v8.log.\r\n\r\n" +
                                    "There is probably a second copy of node.exe running and the\r\n" +
                                    "results may be unavailable or incorrect.\r\n");
                }
            };

            _process.Start();

            if (_startBrowser) {
                Debug.Assert(_port != null);

                string webBrowserUrl = _launchUrl;
                if (String.IsNullOrWhiteSpace(webBrowserUrl)) {
                    webBrowserUrl = "http://localhost:" + _port;
                }

                ThreadPool.QueueUserWorkItem(StartBrowser, new BrowserStartInfo(_port.Value, webBrowserUrl));
            }
        }

        public event EventHandler ProcessExited;

        internal void StopProfiling() {
            _process.Kill();
        }
    }
}

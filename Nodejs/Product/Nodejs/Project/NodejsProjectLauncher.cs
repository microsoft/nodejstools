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
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using Microsoft.Win32;
using System.Web;

namespace Microsoft.NodejsTools.Project {
    class NodejsProjectLauncher : IProjectLauncher {
        private readonly NodejsProjectNode _project;

        public NodejsProjectLauncher(NodejsProjectNode project) {
            _project = project;
        }

        #region IProjectLauncher Members

        public int LaunchProject(bool debug) {
            return Start(ResolveStartupFile(), debug);
        }

        public int LaunchFile(string file, bool debug) {
            return Start(file, debug);
        }

        private int Start(string file, bool debug) {
            string nodePath = GetNodePath();
            if (nodePath == null) {
                Nodejs.ShowNodejsNotInstalled();
                return VSConstants.S_OK;
            }

            var startBrowserStr = _project.GetProjectProperty(NodejsConstants.StartWebBrowser);
            bool startBrowser;
            if (!Boolean.TryParse(startBrowserStr, out startBrowser)) {
                startBrowser = true;
            }
            int? port = null;
            string webBrowserUrl = null;
            if (startBrowser) {
                var portStr = _project.GetProjectProperty(NodejsConstants.NodejsPort);
                int tmpPort;
                if (String.IsNullOrWhiteSpace(portStr) || !Int32.TryParse(portStr, out tmpPort)) {
                    // make sure we know the port for when we start the browser
                    port = GetFreePort();
                } else {
                    port = tmpPort;
                }
                Debug.Assert(port != null);
                webBrowserUrl = _project.GetProjectProperty(NodejsConstants.LaunchUrl);
                if (String.IsNullOrWhiteSpace(webBrowserUrl)) {
                    webBrowserUrl = "http://localhost:" + port;
                }
            }

            if (debug) {
                StartWithDebugger(file, port, (webBrowserUrl != null) ? HttpUtility.UrlEncode(webBrowserUrl) : webBrowserUrl);
            } else {
                var psi = new ProcessStartInfo();
                psi.UseShellExecute = false;
                if (port != null) {
                    psi.EnvironmentVariables["PORT"] = port.ToString();
                }

                psi.FileName = nodePath;
                psi.Arguments = GetFullArguments(file);
                psi.WorkingDirectory = _project.GetWorkingDirectory();
                var process = Process.Start(psi);

                if (webBrowserUrl != null) {
                    Debug.Assert(port != null);
                    OnPortOpenedHandler.CreateHandler(
                        port.Value,
                        shortCircuitPredicate: () => process.HasExited,
                        action: () => {
                            VsShellUtilities.OpenBrowser(webBrowserUrl, (uint)__VSOSPFLAGS.OSP_LaunchNewBrowser);
                        }
                    );
                }
            }
            return VSConstants.S_OK;
        }

        private string GetFullArguments(string file, bool includeNodeArgs = true) {
            string res = "";
            if (includeNodeArgs) {
                var nodeArgs = _project.GetProjectProperty(NodejsConstants.NodeExeArguments);
                if (!String.IsNullOrWhiteSpace(nodeArgs)) {
                    res = nodeArgs + " ";
                }
            }
            res += "\"" + file + "\"";
            var scriptArgs = _project.GetProjectProperty(NodejsConstants.ScriptArguments);
            if (!String.IsNullOrWhiteSpace(scriptArgs)) {
                res += " " + scriptArgs;
            }
            return res;
        }

        private string GetNodePath() {
            var overridePath = _project.GetProjectProperty(NodejsConstants.NodeExePath);
            if (!String.IsNullOrWhiteSpace(overridePath)) {
                return overridePath;
            }
            return Nodejs.NodeExePath;
        }

        #endregion

        /// <summary>
        /// Default implementation of the "Start Debugging" command.
        /// </summary>
        private void StartWithDebugger(string startupFile, int? port, string webBrowserUrl) {
            VsDebugTargetInfo dbgInfo = new VsDebugTargetInfo();
            dbgInfo.cbSize = (uint)Marshal.SizeOf(dbgInfo);

            SetupDebugInfo(ref dbgInfo, startupFile, port, webBrowserUrl);

            LaunchDebugger(_project.Site, dbgInfo);
        }


        private static void LaunchDebugger(IServiceProvider provider, VsDebugTargetInfo dbgInfo) {
            if (!Directory.Exists(UnquotePath(dbgInfo.bstrCurDir))) {
                MessageBox.Show(String.Format("Working directory \"{0}\" does not exist.", dbgInfo.bstrCurDir), "Node.js Tools for Visual Studio");
            } else if (!File.Exists(UnquotePath(dbgInfo.bstrExe))) {
                MessageBox.Show(String.Format("Interpreter \"{0}\" does not exist.", dbgInfo.bstrExe), "Node.js Tools for Visual Studio");
            } else {
                VsShellUtilities.LaunchDebugger(provider, dbgInfo);
            }
        }

        private static string UnquotePath(string p) {
            if (p.StartsWith("\"") && p.EndsWith("\"")) {
                return p.Substring(1, p.Length - 2);
            }
            return p;
        }

        /// <summary>
        /// Sets up debugger information.
        /// </summary>
        private void SetupDebugInfo(ref VsDebugTargetInfo dbgInfo, string startupFile, int? port, string webBrowserUrl) {
            dbgInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            dbgInfo.bstrExe = GetNodePath();
            dbgInfo.bstrCurDir = _project.GetWorkingDirectory();
            dbgInfo.bstrArg = GetFullArguments(startupFile, includeNodeArgs: false);    // we need to supply node args via options
            dbgInfo.bstrRemoteMachine = null;
            var nodeArgs = _project.GetProjectProperty(NodejsConstants.NodeExeArguments);
            if(!String.IsNullOrWhiteSpace(nodeArgs)) {
                dbgInfo.bstrOptions = AD7Engine.InterpreterOptions + "=" + nodeArgs;
            }
            if (port.HasValue) {
                if (!String.IsNullOrWhiteSpace(dbgInfo.bstrOptions)) {
                    dbgInfo.bstrOptions += ";" + AD7Engine.WebBrowserPort + "=" + port.Value.ToString();
                } else {
                    dbgInfo.bstrOptions = AD7Engine.WebBrowserPort + "=" + port.Value.ToString();
                }
            }
            if (!String.IsNullOrWhiteSpace(webBrowserUrl)) {
                if (!String.IsNullOrWhiteSpace(dbgInfo.bstrOptions)) {
                    dbgInfo.bstrOptions += ";" + AD7Engine.WebBrowserUrl + "=" + webBrowserUrl;
                } else {
                    dbgInfo.bstrOptions = AD7Engine.WebBrowserUrl + "=" + webBrowserUrl;
                }
            }

            dbgInfo.fSendStdoutToOutputWindow = 0;

            StringDictionary env = new StringDictionary();
            SetupEnvironment(env, port);
            if (env.Count > 0) {
                // add any inherited env vars
                var variables = Environment.GetEnvironmentVariables();
                foreach (var key in variables.Keys) {
                    string strKey = (string)key;
                    if (!env.ContainsKey(strKey)) {
                        env.Add(strKey, (string)variables[key]);
                    }
                }

                //Environemnt variables should be passed as a
                //null-terminated block of null-terminated strings. 
                //Each string is in the following form:name=value\0
                StringBuilder buf = new StringBuilder();
                foreach (DictionaryEntry entry in env) {
                    buf.AppendFormat("{0}={1}\0", entry.Key, entry.Value);
                }
                buf.Append("\0");
                dbgInfo.bstrEnv = buf.ToString();
            }

            // Set the Node  debugger
            dbgInfo.clsidCustom = AD7Engine.DebugEngineGuid;
            dbgInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
        }

        private void SetupEnvironment(StringDictionary env, int? port) {
            if (port != null) {
                env["PORT"] = port.ToString();
            }
        }

        private static int GetFreePort() {
            return Enumerable.Range(new Random().Next(1200, 2000), 60000).Except(
                from connection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                select connection.LocalEndPoint.Port
            ).First();
        }

        private string ResolveStartupFile() {
            string startupFile = _project.GetStartupFile();
            if (string.IsNullOrEmpty(startupFile)) {
                throw new ApplicationException("No startup file is defined for the startup project.");
            }
            return startupFile;
        }
    }

    internal class OnPortOpenedHandler {

        class OnPortOpenedInfo {
            public readonly int Port;
            public readonly TimeSpan Timeout;
            public readonly int Sleep;
            public readonly Func<bool> ShortCircuitPredicate;
            public readonly Action Action;
            public readonly DateTime StartTime;

            public OnPortOpenedInfo(
                int port,
                int? timeout = null,
                int? sleep = null,
                Func<bool> shortCircuitPredicate = null,
                Action action = null
            ) {
                Port = port;
                Timeout = TimeSpan.FromMilliseconds(timeout ?? 60000);  // 1 min timeout
                Sleep = sleep ?? 500;                                   // 1/2 second sleep
                ShortCircuitPredicate = shortCircuitPredicate ?? (() => false);
                Action = action ?? (() => {});
                StartTime = System.DateTime.Now;
            }
        }

        internal static void CreateHandler(
            int port,
            int? timeout = null,
            int? sleep = null,
            Func<bool> shortCircuitPredicate = null,
            Action action = null
        ) {
            ThreadPool.QueueUserWorkItem(
                OnPortOpened,
                new OnPortOpenedInfo(
                    port,
                    timeout,
                    sleep,
                    shortCircuitPredicate,
                    action
                )
            );
        }

        private static void OnPortOpened(object infoObj) {
            var info = (OnPortOpenedInfo)infoObj;

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                socket.Blocking = true;
                try {
                    while (true) {
                        // Short circuit
                        if (info.ShortCircuitPredicate()) {
                            return;
                        }

                        // Try connect
                        try {
                            socket.Connect(IPAddress.Loopback, info.Port);
                            break;
                        } catch {
                            // Connect failure
                            // Fall through
                        }

                        // Timeout
                        if ((System.DateTime.Now - info.StartTime) >= info.Timeout) {
                            break;
                        }

                        // Sleep
                        System.Threading.Thread.Sleep(info.Sleep);
                    }
                } finally {
                    socket.Close();
                }
            }

            // Launch browser (if not short-circuited)
            if (!info.ShortCircuitPredicate()) {
                info.Action();                
            }
        }
    }
}

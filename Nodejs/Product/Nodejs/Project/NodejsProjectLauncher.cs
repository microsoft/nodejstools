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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Microsoft.NodejsTools.Debugger;
using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    class NodejsProjectLauncher : IProjectLauncher {
        private readonly NodejsProjectNode _project;
        private int? _testServerPort;

        public NodejsProjectLauncher(NodejsProjectNode project) {
            _project = project;

            var portNumber = _project.GetProjectProperty(NodejsConstants.NodejsPort);
            int portNum;
            if (Int32.TryParse(portNumber, out portNum)) {
                _testServerPort = portNum;
            }
        }

        #region IProjectLauncher Members

        public int LaunchProject(bool debug) {
            NodejsPackage.Instance.Logger.LogEvent(Logging.NodejsToolsLogEvent.Launch, debug ? 1 : 0);
            return Start(ResolveStartupFile(), debug);
        }

        public int LaunchFile(string file, bool debug) {
            NodejsPackage.Instance.Logger.LogEvent(Logging.NodejsToolsLogEvent.Launch, debug ? 1 : 0);
            return Start(file, debug);
        }

        private int Start(string file, bool debug) {
            string nodePath = GetNodePath();
            if (nodePath == null) {
                Nodejs.ShowNodejsNotInstalled();
                return VSConstants.S_OK;
            } else if (!Nodejs.CheckNodejsSupported(nodePath)) {
                return VSConstants.S_OK;
            }

            bool startBrowser = ShouldStartBrowser();

            if (debug) {
                StartWithDebugger(file);
            } else {
                var psi = new ProcessStartInfo();
                psi.UseShellExecute = false;

                psi.FileName = nodePath;
                psi.Arguments = GetFullArguments(file);
                psi.WorkingDirectory = _project.GetWorkingDirectory();

                string webBrowserUrl = GetFullUrl();
                Uri uri = null;
                if (!String.IsNullOrWhiteSpace(webBrowserUrl)) {
                    uri = new Uri(webBrowserUrl);

                    psi.EnvironmentVariables["PORT"] = uri.Port.ToString();
                }

                foreach (var nameValue in GetEnvironmentVariables()) {
                    psi.EnvironmentVariables[nameValue.Key] = nameValue.Value;
                }

                var process = NodeProcess.Start(
                    psi,
                    NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit,
                    NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit
                );

                if (startBrowser && uri != null) {
                    OnPortOpenedHandler.CreateHandler(
                        uri.Port,
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

        private string GetFullUrl() {
            var host = _project.GetProjectProperty(NodejsConstants.LaunchUrl);

            try {
                return GetFullUrl(host, TestServerPort);
            } catch (UriFormatException) {
                var output = OutputWindowRedirector.GetGeneral(NodejsPackage.Instance);
                output.WriteErrorLine(SR.GetString(SR.ErrorInvalidLaunchUrl, host));
                output.ShowAndActivate();
                return string.Empty;
            }
        }

        internal static string GetFullUrl(string host, int port) {
            UriBuilder builder;
            Uri uri;
            if (Uri.TryCreate(host, UriKind.Absolute, out uri)) {
                builder = new UriBuilder(uri);
            } else {
                builder = new UriBuilder();
                builder.Scheme = Uri.UriSchemeHttp;
                builder.Host = "localhost";
                builder.Path = host;
            }

            builder.Port = port;

            return builder.ToString();
        }

        private string TestServerPortString {
            get {
                if (!_testServerPort.HasValue) {
                    _testServerPort = GetFreePort();
                }
                return _testServerPort.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        private int TestServerPort {
            get {
                if (!_testServerPort.HasValue) {
                    _testServerPort = GetFreePort();
                }
                return _testServerPort.Value;
            }
        }

        /// <summary>
        /// Default implementation of the "Start Debugging" command.
        /// </summary>
        private void StartWithDebugger(string startupFile) {
            VsDebugTargetInfo dbgInfo = new VsDebugTargetInfo();
            dbgInfo.cbSize = (uint)Marshal.SizeOf(dbgInfo);

            if (SetupDebugInfo(ref dbgInfo, startupFile)) {
                LaunchDebugger(_project.Site, dbgInfo);
            }
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

        private void AppendOption(ref VsDebugTargetInfo dbgInfo, string option, string value) {
            if (!String.IsNullOrWhiteSpace(dbgInfo.bstrOptions)) {
                dbgInfo.bstrOptions += ";";
            }

            dbgInfo.bstrOptions += option + "=" + HttpUtility.UrlEncode(value);
        }

        /// <summary>
        /// Sets up debugger information.
        /// </summary>
        private bool SetupDebugInfo(ref VsDebugTargetInfo dbgInfo, string startupFile) {
            dbgInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            dbgInfo.bstrExe = GetNodePath();
            if (!Nodejs.CheckNodejsSupported(dbgInfo.bstrExe)) {
                return false;
            }
            dbgInfo.bstrCurDir = _project.GetWorkingDirectory();
            dbgInfo.bstrArg = GetFullArguments(startupFile, includeNodeArgs: false);    // we need to supply node args via options
            dbgInfo.bstrRemoteMachine = null;
            var nodeArgs = _project.GetProjectProperty(NodejsConstants.NodeExeArguments);
            if (!String.IsNullOrWhiteSpace(nodeArgs)) {
                AppendOption(ref dbgInfo, AD7Engine.InterpreterOptions, nodeArgs);
            }

            var url = GetFullUrl();
            if (ShouldStartBrowser() && !String.IsNullOrWhiteSpace(url)) {
                AppendOption(ref dbgInfo, AD7Engine.WebBrowserUrl, url);
            }

            var debuggerPort = _project.GetProjectProperty(NodejsConstants.DebuggerPort);
            if (!String.IsNullOrWhiteSpace(debuggerPort)) {
                AppendOption(ref dbgInfo, AD7Engine.DebuggerPort, debuggerPort);
            }

            if (NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit) {
                AppendOption(ref dbgInfo, AD7Engine.WaitOnAbnormalExitSetting, "true");
            }

            if (NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit) {
                AppendOption(ref dbgInfo, AD7Engine.WaitOnNormalExitSetting, "true");
            }

            dbgInfo.fSendStdoutToOutputWindow = 0;

            StringDictionary env = new StringDictionary();
            if (!String.IsNullOrWhiteSpace(url)) {
                Uri webUrl = new Uri(url);
                env["PORT"] = webUrl.Port.ToString();
            }

            foreach (var nameValue in GetEnvironmentVariables()) {
                env[nameValue.Key] = nameValue.Value;
            }

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
            return true;
        }

        private bool ShouldStartBrowser() {
            var startBrowser = _project.GetProjectProperty(NodejsConstants.StartWebBrowser);
            bool fStartBrowser;
            if (!String.IsNullOrEmpty(startBrowser) &&
                Boolean.TryParse(startBrowser, out fStartBrowser)) {
                return fStartBrowser;
            }

            return true;
        }

        private IEnumerable<KeyValuePair<string, string>> GetEnvironmentVariables() {
            var envVars = _project.GetProjectProperty(NodejsConstants.EnvironmentVariables);
            if (envVars != null) {
                foreach (var envVar in envVars.Split(';')) {
                    var nameValue = envVar.Split(new[] { '=' }, 2);
                    if (nameValue.Length == 2) {
                        yield return new KeyValuePair<string, string>(nameValue[0], nameValue[1]);
                    }
                }
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
            public readonly TimeSpan? Timeout;
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
                if (timeout.HasValue) {
                    Timeout = TimeSpan.FromMilliseconds(Convert.ToDouble(timeout));
                }
                Sleep = sleep ?? 500;                                   // 1/2 second sleep
                ShortCircuitPredicate = shortCircuitPredicate ?? (() => false);
                Action = action ?? (() => { });
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
                        if (info.Timeout.HasValue && (System.DateTime.Now - info.StartTime) >= info.Timeout) {
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

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
using Microsoft.NodejsTools.Npm.SPI;
using Microsoft.NodejsTools.Options;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.NodejsTools.TypeScript;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Project
{
    internal class NodejsProjectLauncher : IProjectLauncher
    {
        private readonly NodejsProjectNode _project;
        private int? _testServerPort;

        internal static readonly Guid WebKitDebuggerV2Guid = Guid.Parse("30d423cc-6d0b-4713-b92d-6b2a374c3d89");

        public NodejsProjectLauncher(NodejsProjectNode project)
        {
            this._project = project;

            var portNumber = this._project.GetProjectProperty(NodeProjectProperty.NodejsPort);
            if (int.TryParse(portNumber, out var portNum))
            {
                this._testServerPort = portNum;
            }
        }

        #region IProjectLauncher Members
        public int LaunchProject(bool debug)
        {
            return LaunchFile(ResolveStartupFile(), debug);
        }

        public int LaunchFile(string file, bool debug)
        {
            var nodePath = GetNodePath();

            if(this._project.IsInstallingMissingModules)
            {
                Nodejs.ShowNpmIsInstalling();
                this._project.NpmOutputPane?.Show();

                return VSConstants.S_OK;
            }

            if (nodePath == null)
            {
                Nodejs.ShowNodejsNotInstalled();
                return VSConstants.S_OK;
            }

            var nodeVersion = Nodejs.GetNodeVersion(nodePath);
            var startBrowser = ShouldStartBrowser();

            // The call to Version.ToString() is safe, since changes to the ToString method are very unlikely, as the current output is widely documented.
            if (debug)
            {
                if (nodeVersion >= new Version(8, 0))
                {
                    StartWithChromeV2Debugger(file, nodePath, startBrowser);
                    TelemetryHelper.LogDebuggingStarted("ChromeV2", nodeVersion.ToString());
                }
                else
                {
                    StartWithDebugger(file);
                    TelemetryHelper.LogDebuggingStarted("Node6", nodeVersion.ToString());
                }
            }
            else
            {
                StartNodeProcess(file, nodePath, startBrowser);
                TelemetryHelper.LogDebuggingStarted("None", nodeVersion.ToString());
            }

            return VSConstants.S_OK;
        }

        // todo: move usersettings to separate class, so we can use this from other places.

        internal static bool CheckEnableDiagnosticLoggingOption()
        {
            var optionString = NodejsDialogPage.LoadString(name: "DiagnosticLogging", cat: "Debugging");

            return !StringComparer.OrdinalIgnoreCase.Equals(optionString, "false");
        }

        private void StartNodeProcess(string file, string nodePath, bool startBrowser)
        {
            //TODO: looks like this duplicates a bunch of code in NodeDebugger
            var psi = new ProcessStartInfo()
            {
                UseShellExecute = false,

                FileName = nodePath,
                Arguments = GetFullArguments(file, includeNodeArgs: true),
                WorkingDirectory = _project.GetWorkingDirectory()
            };

            var webBrowserUrl = GetFullUrl();
            Uri uri = null;
            if (!String.IsNullOrWhiteSpace(webBrowserUrl))
            {
                uri = new Uri(webBrowserUrl);
                psi.EnvironmentVariables["PORT"] = uri.Port.ToString();
            }

            foreach (var nameValue in GetEnvironmentVariables())
            {
                psi.EnvironmentVariables[nameValue.Key] = nameValue.Value;
            }

            var process = NodeProcess.Start(
                psi,
                waitOnAbnormal: NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit,
                waitOnNormal: NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit);

            this._project.OnDispose += process.ResponseToTerminateEvent;

            if (startBrowser && uri != null)
            {
                OnPortOpenedHandler.CreateHandler(
                    uri.Port,
                    shortCircuitPredicate: () => process.HasExited,
                    action: () =>
                    {
                        VsShellUtilities.OpenBrowser(webBrowserUrl, (uint)__VSOSPFLAGS.OSP_LaunchNewBrowser);
                    }
                );
            }
        }

        private string GetFullArguments(string file, bool includeNodeArgs)
        {
            var res = string.Empty;
            if (includeNodeArgs)
            {
                var nodeArgs = this._project.GetProjectProperty(NodeProjectProperty.NodeExeArguments);
                if (!string.IsNullOrWhiteSpace(nodeArgs))
                {
                    res = nodeArgs + " ";
                }
            }

            res += "\"" + file + "\"";
            var scriptArgs = this._project.GetProjectProperty(NodeProjectProperty.ScriptArguments);
            if (!string.IsNullOrWhiteSpace(scriptArgs))
            {
                res += " " + scriptArgs;
            }
            return res;
        }

        private string GetNodePath()
        {
            var overridePath = this._project.GetProjectProperty(NodeProjectProperty.NodeExePath);
            return Nodejs.GetAbsoluteNodeExePath(this._project.ProjectHome, overridePath);
        }

        #endregion

        private string GetFullUrl()
        {
            var host = this._project.GetProjectProperty(NodeProjectProperty.LaunchUrl);

            try
            {
                return GetFullUrl(host, this.TestServerPort);
            }
            catch (UriFormatException)
            {
                var output = OutputWindowRedirector.GetGeneral(NodejsPackage.Instance);
                output.WriteErrorLine(string.Format(CultureInfo.CurrentCulture, Resources.ErrorInvalidLaunchUrl, host));
                output.ShowAndActivate();
                return string.Empty;
            }
        }

        internal static string GetFullUrl(string host, int port)
        {
            UriBuilder builder;
            if (Uri.TryCreate(host, UriKind.Absolute, out var uri))
            {
                builder = new UriBuilder(uri);
            }
            else
            {
                builder = new UriBuilder();
                builder.Scheme = Uri.UriSchemeHttp;
                builder.Host = "localhost";
                builder.Path = host;
            }

            builder.Port = port;

            return builder.ToString();
        }

        private int TestServerPort
        {
            get
            {
                if (!this._testServerPort.HasValue)
                {
                    this._testServerPort = GetFreePort();
                }
                return this._testServerPort.Value;
            }
        }

        /// <summary>
        /// Default implementation of the "Start Debugging" command.
        /// </summary>
        private void StartWithDebugger(string startupFile)
        {
            var dbgInfo = new VsDebugTargetInfo();
            dbgInfo.cbSize = (uint)Marshal.SizeOf(dbgInfo);
            if (SetupDebugInfo(ref dbgInfo, startupFile))
            {
                LaunchDebugger(this._project.Site, dbgInfo);
            }
        }

        private void StartWithChromeV2Debugger(string file, string nodePath, bool startBrowser)
        {
            var serviceProvider = _project.Site;

            // Here we need to massage the env variables into the format expected by node and vs code
            var webBrowserUrl = GetFullUrl();
            var envVars = GetEnvironmentVariables(webBrowserUrl);
            var debuggerPort = this._project.GetProjectProperty(NodeProjectProperty.DebuggerPort);
            if (string.IsNullOrWhiteSpace(debuggerPort))
            {
                debuggerPort = NodejsConstants.DefaultDebuggerPort.ToString();
            }

            var runtimeArguments = ConvertArguments(this._project.GetProjectProperty(NodeProjectProperty.NodeExeArguments));
            // If we supply the port argument we also need to manually add --inspect-brk=port to the runtime arguments
            runtimeArguments = runtimeArguments.Append($"--inspect-brk={debuggerPort}");
            var scriptArguments = ConvertArguments(this._project.GetProjectProperty(NodeProjectProperty.ScriptArguments));

            var cwd = _project.GetWorkingDirectory(); // Current working directory
            var configuration = new JObject(
                new JProperty("name", "Debug Node.js program from Visual Studio"),
                new JProperty("type", "node2"),
                new JProperty("request", "launch"),
                new JProperty("program", file),
                new JProperty("args", scriptArguments),
                new JProperty("runtimeExecutable", nodePath),
                new JProperty("runtimeArgs", runtimeArguments),
                new JProperty("port", debuggerPort),
                new JProperty("cwd", cwd),
                new JProperty("console", "externalTerminal"),
                new JProperty("env", JObject.FromObject(envVars)),
                new JProperty("trace", CheckEnableDiagnosticLoggingOption()),
                new JProperty("sourceMaps", true),
                new JProperty("stopOnEntry", true));

            var jsonContent = configuration.ToString();

            var debugTargets = new[] {
                new VsDebugTargetInfo4() {
                    dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess,
                    guidLaunchDebugEngine = WebKitDebuggerV2Guid,
                    bstrExe = file,
                    bstrOptions = jsonContent
                }
            };

            var processInfo = new VsDebugTargetProcessInfo[debugTargets.Length];

            var debugger = (IVsDebugger4)serviceProvider.GetService(typeof(SVsShellDebugger));
            debugger.LaunchDebugTargets4(1, debugTargets, processInfo);

            // Launch browser 
            if (startBrowser && !string.IsNullOrWhiteSpace(webBrowserUrl))
            {
                var uri = new Uri(webBrowserUrl);
                OnPortOpenedHandler.CreateHandler(
                    uri.Port,
                    timeout: 5_000, // 5 seconds
                    action: () =>
                    {
                        VsShellUtilities.OpenBrowser(webBrowserUrl, (uint)__VSOSPFLAGS.OSP_LaunchNewBrowser);
                    }
                );
            }
        }

        private static string[] ConvertArguments(string argumentString)
        {
            if (argumentString != null)
            {
                return argumentString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return Array.Empty<string>();
        }

        private void LaunchDebugger(IServiceProvider provider, VsDebugTargetInfo dbgInfo)
        {
            if (!Directory.Exists(dbgInfo.bstrCurDir))
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.DebugWorkingDirectoryDoesNotExistErrorMessage, dbgInfo.bstrCurDir), SR.ProductName);
            }
            else if (!File.Exists(dbgInfo.bstrExe))
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.DebugInterpreterDoesNotExistErrorMessage, dbgInfo.bstrExe), SR.ProductName);
            }
            else if (DoesProjectSupportDebugging())
            {
                VsShellUtilities.LaunchDebugger(provider, dbgInfo);
            }
        }

        private bool DoesProjectSupportDebugging()
        {
            var typeScriptOutFile = this._project.GetProjectProperty("TypeScriptOutFile");
            if (!string.IsNullOrEmpty(typeScriptOutFile))
            {
                return MessageBox.Show(
                    Resources.DebugTypeScriptCombineNotSupportedWarningMessage,
                    SR.ProductName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes;
            }

            return true;
        }

        private static void AppendOption(ref VsDebugTargetInfo dbgInfo, string option, string value)
        {
            if (!string.IsNullOrWhiteSpace(dbgInfo.bstrOptions))
            {
                dbgInfo.bstrOptions += ";";
            }

            dbgInfo.bstrOptions += option + "=" + HttpUtility.UrlEncode(value);
        }

        /// <summary>
        /// Sets up debugger information.
        /// </summary>
        private bool SetupDebugInfo(ref VsDebugTargetInfo dbgInfo, string startupFile)
        {
            dbgInfo.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            dbgInfo.bstrExe = GetNodePath();
            dbgInfo.bstrCurDir = this._project.GetWorkingDirectory();
            dbgInfo.bstrArg = GetFullArguments(startupFile, includeNodeArgs: false);    // we need to supply node args via options
            dbgInfo.bstrRemoteMachine = null;
            var nodeArgs = this._project.GetProjectProperty(NodeProjectProperty.NodeExeArguments);
            if (!string.IsNullOrWhiteSpace(nodeArgs))
            {
                AppendOption(ref dbgInfo, AD7Engine.InterpreterOptions, nodeArgs);
            }

            var url = GetFullUrl();
            if (ShouldStartBrowser() && !string.IsNullOrWhiteSpace(url))
            {
                AppendOption(ref dbgInfo, AD7Engine.WebBrowserUrl, url);
            }

            var debuggerPort = this._project.GetProjectProperty(NodeProjectProperty.DebuggerPort);
            if (!string.IsNullOrWhiteSpace(debuggerPort))
            {
                AppendOption(ref dbgInfo, AD7Engine.DebuggerPort, debuggerPort);
            }

            if (NodejsPackage.Instance.GeneralOptionsPage.WaitOnAbnormalExit)
            {
                AppendOption(ref dbgInfo, AD7Engine.WaitOnAbnormalExitSetting, "true");
            }

            if (NodejsPackage.Instance.GeneralOptionsPage.WaitOnNormalExit)
            {
                AppendOption(ref dbgInfo, AD7Engine.WaitOnNormalExitSetting, "true");
            }

            dbgInfo.fSendStdoutToOutputWindow = 0;
            dbgInfo.bstrEnv = GetEnvironmentVariablesString(url);

            // Set the Node  debugger
            dbgInfo.clsidCustom = AD7Engine.DebugEngineGuid;
            dbgInfo.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
            return true;
        }

        private string GetEnvironmentVariablesString(string url)
        {
            var env = GetEnvironmentVariables(url);
            if (env.Count > 0)
            {
                //Environment variables should be passed as a
                //null-terminated block of null-terminated strings. 
                //Each string is in the following form:name=value\0
                var buf = new StringBuilder();
                foreach (var entry in env)
                {
                    buf.AppendFormat("{0}={1}\0", entry.Key, entry.Value);
                }
                buf.Append("\0");
                return buf.ToString();
            }

            return null;
        }

        private Dictionary<string, string> GetEnvironmentVariables(string url)
        {
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(url))
            {
                var webUrl = new Uri(url);
                env["PORT"] = webUrl.Port.ToString();
            }

            foreach (var nameValue in GetEnvironmentVariables())
            {
                env[nameValue.Key] = nameValue.Value;
            }

            if (env.Count > 0)
            {
                // add any inherited env vars
                var variables = Environment.GetEnvironmentVariables();
                foreach (var key in variables.Keys)
                {
                    var strKey = (string)key;
                    if (!env.ContainsKey(strKey))
                    {
                        env.Add(strKey, (string)variables[key]);
                    }
                }
            }

            return env;
        }

        private bool ShouldStartBrowser()
        {
            var startBrowser = this._project.GetProjectProperty(NodeProjectProperty.StartWebBrowser);
            if (!string.IsNullOrEmpty(startBrowser) &&
                bool.TryParse(startBrowser, out var fStartBrowser))
            {
                return fStartBrowser;
            }

            return true;
        }

        private IEnumerable<KeyValuePair<string, string>> GetEnvironmentVariables()
        {
            var envVars = this._project.GetProjectProperty(NodeProjectProperty.Environment);
            if (envVars != null)
            {
                foreach (var envVar in envVars.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var nameValue = envVar.Split(new[] { '=' }, 2);
                    if (nameValue.Length == 2)
                    {
                        yield return new KeyValuePair<string, string>(nameValue[0], nameValue[1]);
                    }
                }
            }
        }

        private static int GetFreePort()
        {
            return Enumerable.Range(new Random().Next(1200, 2000), 60000).Except(
                from connection in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                select connection.LocalEndPoint.Port
            ).First();
        }

        private string ResolveStartupFile()
        {
            var startupFile = this._project.GetStartupFile();
            if (string.IsNullOrEmpty(startupFile))
            {
                throw new ApplicationException(Resources.DebugCouldNotResolveStartupFileErrorMessage);
            }

            if (TypeScriptHelpers.IsTypeScriptFile(startupFile))
            {
                startupFile = TypeScriptHelpers.GetTypeScriptBackedJavaScriptFile(this._project, startupFile);
            }
            return startupFile;
        }
    }

    internal class OnPortOpenedHandler
    {
        private class OnPortOpenedInfo
        {
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
            )
            {
                this.Port = port;
                if (timeout.HasValue)
                {
                    this.Timeout = TimeSpan.FromMilliseconds(Convert.ToDouble(timeout));
                }
                this.Sleep = sleep ?? 500;                                   // 1/2 second sleep
                this.ShortCircuitPredicate = shortCircuitPredicate ?? (() => false);
                this.Action = action ?? (() => { });
                this.StartTime = System.DateTime.Now;
            }
        }

        internal static void CreateHandler(
            int port,
            int? timeout = null,
            int? sleep = null,
            Func<bool> shortCircuitPredicate = null,
            Action action = null
        )
        {
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

        private static void OnPortOpened(object infoObj)
        {
            var info = (OnPortOpenedInfo)infoObj;

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Blocking = true;
                try
                {
                    while (true)
                    {
                        // Short circuit
                        if (info.ShortCircuitPredicate())
                        {
                            return;
                        }

                        // Try connect
                        try
                        {
                            socket.Connect(IPAddress.Loopback, info.Port);
                            break;
                        }
                        catch
                        {
                            // Connect failure
                            // Fall through
                        }

                        // Timeout
                        if (info.Timeout.HasValue && (System.DateTime.Now - info.StartTime) >= info.Timeout)
                        {
                            break;
                        }

                        // Sleep
                        System.Threading.Thread.Sleep(info.Sleep);
                    }
                }
                finally
                {
                    socket.Close();
                }
            }

            // Launch browser (if not short-circuited)
            if (!info.ShortCircuitPredicate())
            {
                info.Action();
            }
        }
    }
}

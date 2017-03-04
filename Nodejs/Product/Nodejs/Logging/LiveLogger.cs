// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Options;
using Microsoft.VisualStudioTools.Project;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.NodejsTools.Logging
{
    /// <summary>
    /// An efficient logger that logs diagnostic messages using Debug.WriteLine.
    /// Additionally logs messages to the NTVS Diagnostics task pane if option is enabled.
    /// </summary>
    internal sealed class LiveLogger
    {
        private static Guid LiveDiagnosticLogPaneGuid = new Guid("{66386208-2E7E-4B93-A852-D1A32EE00107}");
        private const string LiveDiagnosticLogPaneName = "Node.js Tools Live Diagnostics";

        private static volatile LiveLogger _instance;
        private static object _loggerLock = new object();

        private NodejsDiagnosticsOptionsPage _diagnosticsOptions;

        private LiveLogger()
        {
        }

        private NodejsDiagnosticsOptionsPage DiagnosticsOptions
        {
            get
            {
                if (this._diagnosticsOptions == null && NodejsPackage.Instance != null)
                {
                    this._diagnosticsOptions = NodejsPackage.Instance.DiagnosticsOptionsPage;
                }
                return this._diagnosticsOptions;
            }
        }

        private static LiveLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_loggerLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LiveLogger();
                        }
                    }
                }
                return _instance;
            }
        }

        public static void WriteLine(string message, Type category)
        {
            WriteLine("{0}: {1}", category.Name, message);
        }

        public static void WriteLine(string message)
        {
            var str = string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", DateTime.UtcNow.TimeOfDay, message);
            Instance.LogMessage(str);
        }

        public static void WriteLine(string format, params object[] args)
        {
            var str = string.Format(CultureInfo.InvariantCulture, format, args);
            WriteLine(str);
        }

        private void LogMessage(string message)
        {
            Debug.WriteLine(message);

            if (this.DiagnosticsOptions != null && this.DiagnosticsOptions.IsLiveDiagnosticsEnabled)
            {
                var pane = OutputWindowRedirector.Get(VisualStudio.Shell.ServiceProvider.GlobalProvider, LiveDiagnosticLogPaneGuid, LiveDiagnosticLogPaneName);
                if (pane != null)
                {
                    pane.WriteLine(message);
                }
            }
        }
    }
}


// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.NodejsTools.Options;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Logging
{
    /// <summary>
    /// An efficient logger that logs diagnostic messages using Debug.WriteLine.
    /// Additionally logs messages to the NTVS Diagnostics task pane if option is enabled.
    /// </summary>
    internal sealed class LiveLogger
    {
        private static readonly LiveLogger Instance = new LiveLogger();
        private static readonly object _loggerLock = new object();

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

        public static void WriteLine(string message, Type category)
        {
            WriteLine("{0}: {1}", category.Name, message);
        }

        public static void WriteLine(string message)
        {
            var str = string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", DateTime.UtcNow.TimeOfDay, message);
            Debug.WriteLine(message);
        }

        public static void WriteLine(string format, params object[] args)
        {
            var str = string.Format(CultureInfo.InvariantCulture, format, args);
            WriteLine(str);
        }
    }
}


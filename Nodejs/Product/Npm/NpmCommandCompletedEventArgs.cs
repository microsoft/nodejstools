// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.NodejsTools.Npm
{
    /// <summary>
    /// Fired when an attempt to execute an npm command is completed, whether
    /// successfully or not.
    /// </summary>
    public class NpmCommandCompletedEventArgs : EventArgs
    {
        public NpmCommandCompletedEventArgs(string arguments, bool withErrors, bool cancelled)
        {
            Arguments = arguments;
            WithErrors = withErrors;
            Cancelled = cancelled;
        }

        public string Arguments { get; private set; }

        public string CommandText
        {
            get { return string.IsNullOrEmpty(Arguments) ? "npm" : string.Format(CultureInfo.InvariantCulture, "npm {0}", Arguments); }
        }

        /// <summary>
        /// Indicates whether or not there were errors whilst executing npm.
        /// </summary>
        public bool WithErrors { get; private set; }

        /// <summary>
        /// Indicates whether or not the command was cancelled, with or without errors.
        /// </summary>
        public bool Cancelled { get; private set; }
    }
}


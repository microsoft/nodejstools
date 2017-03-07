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
            this.Arguments = arguments;
            this.WithErrors = withErrors;
            this.Cancelled = cancelled;
        }

        public string Arguments { get; }

        public string CommandText
        {
            get { return string.IsNullOrEmpty(this.Arguments) ? "npm" : string.Format(CultureInfo.InvariantCulture, "npm {0}", this.Arguments); }
        }

        /// <summary>
        /// Indicates whether or not there were errors whilst executing npm.
        /// </summary>
        public bool WithErrors { get; }

        /// <summary>
        /// Indicates whether or not the command was cancelled, with or without errors.
        /// </summary>
        public bool Cancelled { get; }
    }
}


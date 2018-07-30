// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Npm.SPI
{
    public abstract class AbstractNpmLogSource : INpmLogSource
    {
        public event EventHandler CommandStarted;

        protected void OnCommandStarted()
        {
            var handlers = CommandStarted;
            if (null != handlers)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        protected void FireNpmLogEvent(string logText, EventHandler<NpmLogEventArgs> handlers)
        {
            if (null != handlers && !string.IsNullOrEmpty(logText))
            {
                handlers(this, new NpmLogEventArgs(logText));
            }
        }

        public event EventHandler<NpmLogEventArgs> OutputLogged;

        protected void OnOutputLogged(string logText)
        {
            FireNpmLogEvent(logText, OutputLogged);
        }

        public event EventHandler<NpmLogEventArgs> ErrorLogged;

        protected void OnErrorLogged(string logText)
        {
            FireNpmLogEvent(logText, ErrorLogged);
        }

        public event EventHandler<NpmExceptionEventArgs> ExceptionLogged;

        protected void OnExceptionLogged(Exception e)
        {
            var handlers = ExceptionLogged;
            if (null != handlers)
            {
                handlers(this, new NpmExceptionEventArgs(e));
            }
        }

        public event EventHandler<NpmCommandCompletedEventArgs> CommandCompleted;

        protected void OnCommandCompleted(
            string arguments,
            bool withErrors,
            bool cancelled)
        {
            var handlers = CommandCompleted;
            if (null != handlers)
            {
                handlers(this, new NpmCommandCompletedEventArgs(arguments, withErrors, cancelled));
            }
        }
    }
}

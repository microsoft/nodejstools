// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudioTools
{
    internal static class VsTaskExtensions
    {
        private static readonly HashSet<string> _displayedMessages = new HashSet<string>();

        /// <summary>
        /// Waits for a task to complete and logs all exceptions except those
        /// that return true from <see cref="IsCriticalException"/>, which are
        /// rethrown.
        /// </summary>
        public static T WaitAndHandleAllExceptions<T>(
            this Task<T> task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        )
        {
            return task.HandleAllExceptions(productTitle, callerType, callerFile, callerLineNumber, callerName)
                .WaitAndUnwrapExceptions();
        }

        /// <summary>
        /// Logs all exceptions from a task except those that return true from
        /// <see cref="IsCriticalException"/>, which are rethrown.
        /// If an exception is thrown, <c>default(T)</c> is returned.
        /// </summary>
        public static async Task<T> HandleAllExceptions<T>(
            this Task<T> task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        )
        {
            var result = default(T);
            try
            {
                result = await task;
            }
            catch (Exception ex)
            {
                if (ex.IsCriticalException())
                {
                    throw;
                }

                var message = SR.GetUnhandledExceptionString(ex, callerType, callerFile, callerLineNumber, callerName);
                // Send the message to the trace listener in case there is
                // somebody out there listening.
                Trace.TraceError(message);

                string logFile;
                try
                {
                    logFile = ActivityLog.LogFilePath;
                }
                catch (InvalidOperationException)
                {
                    logFile = null;
                }

                lock (_displayedMessages)
                {
                    if (!string.IsNullOrEmpty(logFile) &&
                        _displayedMessages.Add(string.Format("{0}:{1}", callerFile, callerLineNumber)))
                    {
                        // First time we've seen this error, so let the user know
                        MessageBox.Show(SR.GetString(SR.SeeActivityLog, logFile), productTitle);
                    }
                }

                try
                {
                    ActivityLog.LogError(productTitle, message);
                }
                catch (InvalidOperationException)
                {
                    // Activity Log is unavailable.
                }

                // In debug builds let the user know immediately
                Debug.Fail(message);
            }
            return result;
        }

        /// <summary>
        /// Waits for a task to complete and logs all exceptions except those
        /// that return true from <see cref="IsCriticalException"/>, which are
        /// rethrown.
        /// </summary>
        public static void WaitAndHandleAllExceptions(
            this Task task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        )
        {
            task.HandleAllExceptions(productTitle, callerType, callerFile, callerLineNumber, callerName)
                .WaitAndUnwrapExceptions();
        }

        /// <summary>
        /// Logs all exceptions from a task except those that return true from
        /// <see cref="IsCriticalException"/>, which are rethrown.
        /// </summary>
        public static async Task HandleAllExceptions(
            this Task task,
            string productTitle,
            Type callerType = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = null
        )
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                if (ex.IsCriticalException())
                {
                    throw;
                }

                var message = SR.GetUnhandledExceptionString(ex, callerType, callerFile, callerLineNumber, callerName);
                // Send the message to the trace listener in case there is
                // somebody out there listening.
                Trace.TraceError(message);

                string logFile;
                try
                {
                    logFile = ActivityLog.LogFilePath;
                }
                catch (InvalidOperationException)
                {
                    logFile = null;
                }

                lock (_displayedMessages)
                {
                    if (!string.IsNullOrEmpty(logFile) &&
                        _displayedMessages.Add(string.Format("{0}:{1}", callerFile, callerLineNumber)))
                    {
                        // First time we've seen this error, so let the user know
                        MessageBox.Show(SR.GetString(SR.SeeActivityLog, logFile), productTitle);
                    }
                }

                try
                {
                    ActivityLog.LogError(productTitle, message);
                }
                catch (InvalidOperationException)
                {
                    // Activity Log is unavailable.
                }

                // In debug builds let the user know immediately
                Debug.Fail(message);
            }
        }
    }
}

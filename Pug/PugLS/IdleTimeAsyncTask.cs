// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TPL = System.Threading.Tasks;

namespace Microsoft.VisualStudio.Pug
{
    /// <summary>
    /// Asynchronous task that start on next idle slot
    /// </summary>
    internal sealed class IdleTimeAsyncTask : IDisposable
    {
        private Func<object> _taskAction;
        private Action<object> _callbackAction;
        private bool _taskRunning = false;
        private bool _connectedToIdle = false;
        private long _closed = 0;
        private int _delay = 0;
        private DateTime _idleConnectTime;

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="callbackAction">Callback to invoke when task completes</param>
        public IdleTimeAsyncTask(Func<object> taskAction, Action<object> callbackAction)
        {
            this._taskAction = taskAction;
            this._callbackAction = callbackAction;
        }
        
        /// <summary>
        /// Run task on next idle slot
        /// </summary>
        public void DoTaskOnIdle()
        {
            if (this._taskAction == null)
            {
                throw new InvalidOperationException("Task action is null");
            }

            if (Interlocked.Read(ref this._closed) == 0)
            {
                ConnectToIdle();
            }
        }

        /// <summary>
        /// Run task on next idle slot after certain amount of milliseconds
        /// </summary>
        /// <param name="msDelay"></param>
        public void DoTaskOnIdle(int msDelay)
        {
            if (this._taskAction == null)
            {
                throw new InvalidOperationException("Task action is null");
            }

            this._delay = msDelay;

            if (Interlocked.Read(ref this._closed) == 0)
            {
                ConnectToIdle();
            }
        }
        
        private void DoTaskInternal()
        {
            if (!this._taskRunning)
            {
                this._taskRunning = true;

                TPL.Task.Factory.StartNew(async () =>
                {
                    if (Interlocked.Read(ref this._closed) == 0)
                    {
                        object result = null;

                        try
                        {
                            result = this._taskAction();
                        }
                        catch (Exception ex)
                        {
                            Debug.Fail($"Background task exception {ex.Message}. Inner exception: {ex.InnerException?.Message ?? "(none)"}");
                            result = ex;
                        }
                        finally
                        {
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            UIThreadCompletedCallback(result);
                        }
                    }
                });
            }
        }

        private void UIThreadCompletedCallback(object result)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (this._callbackAction != null && Interlocked.Read(ref this._closed) == 0)
                {
                    this._callbackAction(result);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail($"Background task UI thread callback exception {ex.Message}. Inner exception: {ex.InnerException?.Message ?? "(none)"}");
            }

            this._taskRunning = false;
        }



        private void OnIdle(object sender, EventArgs e)
        {
            if (this._delay == 0 || TimeUtility.MillisecondsSince(this._idleConnectTime) > this._delay)
            {
                DoTaskInternal();
                DisconnectFromIdle();
            }
        }

        private void ConnectToIdle()
        {
            if (!this._connectedToIdle)
            {
                this._connectedToIdle = true;
                this._idleConnectTime = DateTime.Now;

                // make sure our package is loaded so we can use its
                // OnIdle event
                var nodePackage = new Guid(Guids.NodejsPackageString);
                IVsPackage package;
                var shell = (IVsShell)VSPackage.GetGlobalService(typeof(SVsShell));
                shell.LoadPackage(ref nodePackage, out package);

                VSPackage.Instance.OnIdle += this.OnIdle;
            }
        }

        private void DisconnectFromIdle()
        {
            if (this._connectedToIdle)
            {
                this._connectedToIdle = false;
                VSPackage.Instance.OnIdle -= this.OnIdle;
            }
        }

        public void Dispose()
        {
            DisconnectFromIdle();
            Interlocked.Exchange(ref this._closed, 1);
        }
    }
}

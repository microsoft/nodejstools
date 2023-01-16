// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Extras;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Asynchronous task that start on next idle slot
    /// </summary>
    internal sealed class IdleTimeAsyncTask : IDisposable
    {
        private Func<object> _taskAction;
        private Action<object> _callbackAction;
        private Action<object> _cancelAction;
        private bool _taskRunning = false;
        private bool _connectedToIdle = false;
        private long _closed = 0;
        private int _delay = 0;
        private DateTime _idleConnectTime;

        public object Tag { get; private set; }

        public IdleTimeAsyncTask()
        {
        }

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="callbackAction">Callback to invoke when task completes</param>
        /// <param name="cancelAction">Callback to invoke if task is canceled</param>
        public IdleTimeAsyncTask(Func<object> taskAction, Action<object> callbackAction, Action<object> cancelAction)
            : this()
        {
            Debug.Assert(taskAction != null);

            if (taskAction == null)
            {
                throw new ArgumentNullException(nameof(taskAction));
            }

            this._taskAction = taskAction;
            this._callbackAction = callbackAction;
            this._cancelAction = cancelAction;
        }

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="callbackAction">Callback to invoke when task completes</param>
        public IdleTimeAsyncTask(Func<object> taskAction, Action<object> callbackAction)
            : this(taskAction, callbackAction, null)
        {
        }

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        public IdleTimeAsyncTask(Func<object> taskAction)
            : this(taskAction, null)
        {
        }

        /// <summary>
        /// Asynchronous idle time task constructor
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="msDelay">Milliseconds to delay before performing task on idle</param>
        public IdleTimeAsyncTask(Func<object> taskAction, int msDelay)
            : this(taskAction, null)
        {
            this._delay = msDelay;
        }

        public void DoTaskNow()
        {
            if (Interlocked.Read(ref this._closed) == 0)
            {
                DoTaskInternal();
            }
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

        /// <summary>
        /// Runs specified task on next idle. Task must not be currently running.
        /// </summary>
        /// <param name="taskAction">Task to perform in a background thread</param>
        /// <param name="callbackAction">Callback to invoke when task completes</param>
        /// <param name="cancelAction">Callback to invoke if task is canceled</param>
        public void DoTaskOnIdle(Func<object> taskAction, Action<object> callbackAction, Action<object> cancelAction, object tag = null)
        {
            if (this.TaskRunning)
            {
                throw new InvalidOperationException("Task is running");
            }

            if (taskAction == null)
            {
                throw new ArgumentNullException(nameof(taskAction));
            }

            this.Tag = tag;

            this._taskAction = taskAction;
            this._callbackAction = callbackAction;
            this._cancelAction = cancelAction;

            DoTaskOnIdle();
        }

        public bool TaskRunning => this._connectedToIdle || this._taskRunning;
        public bool TaskScheduled => this.TaskRunning;
        private void DoTaskInternal()
        {
            if (!this._taskRunning)
            {
                this._taskRunning = true;

                Task.Factory.StartNew(() =>
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
                            Debug.Fail(
                                string.Format(CultureInfo.CurrentCulture, "Background task exception {0}. Inner exception: {1}",
                                                           ex.Message,
                                                           ex.InnerException != null ? ex.InnerException.Message : "(none)")
                            );
                            result = ex;
                        }
                        finally
                        {
                            NodeExtrasPackage.Instance.GetUIThread().InvokeAsync(() => UIThreadCompletedCallback(result)).FileAndForget("vs/nodejstools/extras/fault");
                        }
                    }
                    else if (Interlocked.Read(ref this._closed) > 0)
                    {
                        NodeExtrasPackage.Instance.GetUIThread().InvokeAsync((() => UIThreadCanceledCallback(null))).FileAndForget("vs/nodejstools/extras/fault");
                    }
                });
            }
        }

        private void UIThreadCompletedCallback(object result)
        {
            try
            {
                if (this._callbackAction != null && Interlocked.Read(ref this._closed) == 0)
                {
                    this._callbackAction(result);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(
                    string.Format(CultureInfo.CurrentCulture, "Background task UI thread callback exception {0}. Inner exception: {1}",
                                  ex.Message, ex.InnerException != null ? ex.InnerException.Message : "(none)"));
            }

            this._taskRunning = false;
        }

        private void UIThreadCanceledCallback(object result)
        {
            if (this._cancelAction != null && Interlocked.Read(ref this._closed) > 0)
            {
                this._cancelAction(result);
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
                var nodePackage = new Guid(Guids.NodeExtrasPackageString);
                var shell = (IVsShell)NodeExtrasPackage.GetGlobalService(typeof(SVsShell));
                shell.LoadPackage(ref nodePackage, out var package);

                NodeExtrasPackage.Instance.OnIdle += this.OnIdle;
            }
        }

        private void DisconnectFromIdle()
        {
            if (this._connectedToIdle)
            {
                this._connectedToIdle = false;
                NodeExtrasPackage.Instance.OnIdle -= this.OnIdle;
            }
        }

        #region IDisposable

        public void Dispose()
        {
            DisconnectFromIdle();
            Interlocked.Exchange(ref this._closed, 1);
        }

        #endregion
    }
}

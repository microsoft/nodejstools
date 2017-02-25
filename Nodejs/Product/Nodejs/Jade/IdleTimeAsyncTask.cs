//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using SR = Microsoft.NodejsTools.Project.SR;

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
                throw new ArgumentNullException("taskAction");

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
                DoTaskInternal();
        }

        /// <summary>
        /// Run task on next idle slot
        /// </summary>
        public void DoTaskOnIdle()
        {
            if (this._taskAction == null)
                throw new InvalidOperationException("Task action is null");

            if (Interlocked.Read(ref this._closed) == 0)
                ConnectToIdle();
        }

        /// <summary>
        /// Run task on next idle slot after certain amount of milliseconds
        /// </summary>
        /// <param name="msDelay"></param>
        public void DoTaskOnIdle(int msDelay)
        {
            if (this._taskAction == null)
                throw new InvalidOperationException("Task action is null");

            this._delay = msDelay;

            if (Interlocked.Read(ref this._closed) == 0)
                ConnectToIdle();
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
                throw new InvalidOperationException("Task is running");

            if (taskAction == null)
                throw new ArgumentNullException("taskAction");

            this.Tag = tag;

            this._taskAction = taskAction;
            this._callbackAction = callbackAction;
            this._cancelAction = cancelAction;

            DoTaskOnIdle();
        }

        public bool TaskRunning
        {
            get { return this._connectedToIdle || this._taskRunning; }
        }

        public bool TaskScheduled
        {
            get { return this.TaskRunning; }
        }

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
                            NodejsPackage.Instance.GetUIThread().InvokeAsync(() => UIThreadCompletedCallback(result))
                                .HandleAllExceptions(SR.ProductName)
                                .DoNotWait();
                        }
                    }
                    else if (Interlocked.Read(ref this._closed) > 0)
                    {
                        NodejsPackage.Instance.GetUIThread().InvokeAsync((() => UIThreadCanceledCallback(null)))
                            .HandleAllExceptions(SR.ProductName)
                            .DoNotWait();
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
                Guid nodePackage = new Guid(Guids.NodejsPackageString);
                IVsPackage package;
                var shell = (IVsShell)NodejsPackage.GetGlobalService(typeof(SVsShell));
                shell.LoadPackage(ref nodePackage, out package);

                NodejsPackage.Instance.OnIdle += this.OnIdle;
            }
        }

        private void DisconnectFromIdle()
        {
            if (this._connectedToIdle)
            {
                this._connectedToIdle = false;
                NodejsPackage.Instance.OnIdle -= this.OnIdle;
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

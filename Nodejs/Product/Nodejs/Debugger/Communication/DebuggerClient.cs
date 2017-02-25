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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger.Commands;
using Microsoft.NodejsTools.Debugger.Events;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class DebuggerClient : IDebuggerClient
    {
        private readonly IDebuggerConnection _connection;

        private ConcurrentDictionary<int, TaskCompletionSource<JObject>> _messages =
            new ConcurrentDictionary<int, TaskCompletionSource<JObject>>();

        private readonly static Newtonsoft.Json.JsonSerializerSettings jsonSettings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            DateParseHandling = Newtonsoft.Json.DateParseHandling.None
        };

        public DebuggerClient(IDebuggerConnection connection)
        {
            Utilities.ArgumentNotNull("connection", connection);

            this._connection = connection;
            this._connection.OutputMessage += this.OnOutputMessage;
            this._connection.ConnectionClosed += this.OnConnectionClosed;
        }

        /// <summary>
        /// Send a command to debugger.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task SendRequestAsync(DebuggerCommand command, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                TaskCompletionSource<JObject> promise = this._messages.GetOrAdd(command.Id, i => new TaskCompletionSource<JObject>());
                this._connection.SendMessage(command.ToString());
                cancellationToken.ThrowIfCancellationRequested();

                cancellationToken.Register(() => promise.TrySetCanceled(), false);

                JObject response = await promise.Task.ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                command.ProcessResponse(response);
            }
            finally
            {
                TaskCompletionSource<JObject> promise;
                this._messages.TryRemove(command.Id, out promise);
            }
        }

        /// <summary>
        /// Execudes the provided code, and catches any expected exceptions that may arise from direct or indirect use of <see cref="DebuggerClient.SendRequestAsync"/>.
        /// (in particular, when the connection is shut down, or is forcibly dropped from the other end).
        /// </summary>
        /// <remarks>
        /// This is intended to be used primarily with fire-and-forget async void methods that run on threadpool threads and cannot leak those exceptions
        /// without crashing the process.
        /// </remarks>
        public static async void RunWithRequestExceptionsHandled(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (IOException)
            {
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// Break point event handler.
        /// </summary>
        public event EventHandler<BreakpointEventArgs> BreakpointEvent;

        /// <summary>
        /// Compile script event handler.
        /// </summary>
        public event EventHandler<CompileScriptEventArgs> CompileScriptEvent;

        /// <summary>
        /// Exception event handler.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionEvent;

        /// <summary>
        /// Handles disconnect from debugger.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnConnectionClosed(object sender, EventArgs e)
        {
            ConcurrentDictionary<int, TaskCompletionSource<JObject>> messages = Interlocked.Exchange(ref this._messages, new ConcurrentDictionary<int, TaskCompletionSource<JObject>>());
            foreach (var kv in messages)
            {
                var exception = new IOException(Resources.DebuggerConnectionClosed);
                kv.Value.SetException(exception);
            }

            messages.Clear();
        }

        /// <summary>
        /// Process message from debugger connection.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Event arguments.</param>
        private void OnOutputMessage(object sender, MessageEventArgs args)
        {
            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(args.Message, jsonSettings);
            var messageType = (string)message["type"];

            switch (messageType)
            {
                case "event":
                    HandleEventMessage(message);
                    break;

                case "response":
                    HandleResponseMessage(message);
                    break;

                default:
                    Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Unrecognized type '{0}' in message: {1}", messageType, message));
                    break;
            }
        }

        /// <summary>
        /// Handles event message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void HandleEventMessage(JObject message)
        {
            var eventType = (string)message["event"];
            switch (eventType)
            {
                case "afterCompile":
                    EventHandler<CompileScriptEventArgs> compileScriptHandler = CompileScriptEvent;
                    if (compileScriptHandler != null)
                    {
                        var compileScriptEvent = new CompileScriptEvent(message);
                        compileScriptHandler(this, new CompileScriptEventArgs(compileScriptEvent));
                    }
                    break;

                case "break":
                    EventHandler<BreakpointEventArgs> breakpointHandler = BreakpointEvent;
                    if (breakpointHandler != null)
                    {
                        var breakpointEvent = new BreakpointEvent(message);
                        breakpointHandler(this, new BreakpointEventArgs(breakpointEvent));
                    }
                    break;

                case "exception":
                    EventHandler<ExceptionEventArgs> exceptionHandler = ExceptionEvent;
                    if (exceptionHandler != null)
                    {
                        var exceptionEvent = new ExceptionEvent(message);
                        exceptionHandler(this, new ExceptionEventArgs(exceptionEvent));
                    }
                    break;

                case "beforeCompile":
                case "breakForCommand":
                case "newFunction":
                case "scriptCollected":
                case "compileError":
                    break;

                default:
                    Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Unrecognized type '{0}' in event message: {1}", eventType, message));
                    break;
            }
        }

        /// <summary>
        /// Handles response message.
        /// </summary>
        /// <param name="message">Message.</param>
        private void HandleResponseMessage(JObject message)
        {
            TaskCompletionSource<JObject> promise;
            var messageId = (int)message["request_seq"];

            if (this._messages.TryGetValue(messageId, out promise))
            {
                promise.SetResult(message);
            }
            else
            {
                Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Invalid response identifier '{0}'", messageId));
            }
        }
    }
}
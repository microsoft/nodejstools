/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class DebuggerConnection : IDebuggerConnection {
        private readonly Regex _contentLength = new Regex(@"Content-Length: (\d+)", RegexOptions.Compiled);
        private readonly AsyncProducerConsumerCollection<string> _messages;
        private readonly INetworkClientFactory _networkClientFactory;
        private readonly Regex _nodeVersion = new Regex(@"Embedding-Host: node v([0-9.]+)", RegexOptions.Compiled);
        private INetworkClient _networkClient;
        private readonly object _networkClientLock = new object();

        public DebuggerConnection(INetworkClientFactory networkClientFactory) {
            Utilities.ArgumentNotNull("networkClientFactory", networkClientFactory);

            _networkClientFactory = networkClientFactory;
            _messages = new AsyncProducerConsumerCollection<string>();
            NodeVersion = new Version();
        }

        public void Dispose() {
            Close();
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        public void Close() {
            lock (_networkClientLock) {
                if (_networkClient != null) {
                    _networkClient.Dispose();
                    _networkClient = null;
                }
            }
        }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message.</param>
        public void SendMessage(string message) {
            Utilities.ArgumentNotNullOrEmpty("message", message);

            if (!Connected) {
                return;
            }

            DebugWriteLine("Request: " + message);

            int byteCount = Encoding.UTF8.GetByteCount(message);
            string request = string.Format("Content-Length: {0}{1}{1}{2}", byteCount, Environment.NewLine, message);

            _messages.Add(request);
        }

        /// <summary>
        /// Fired when received inbound message.
        /// </summary>
        public event EventHandler<MessageEventArgs> OutputMessage;

        /// <summary>
        /// Fired when connection was closed.
        /// </summary>
        public event EventHandler<EventArgs> ConnectionClosed;

        /// <summary>
        /// Gets a value indicating whether connection established.
        /// </summary>
        public bool Connected {
            get {
                lock (_networkClientLock) {
                    return _networkClient != null && _networkClient.Connected;
                }
            }
        }

        /// <summary>
        /// Gets a node.js version.
        /// </summary>
        public Version NodeVersion { get; private set; }

        /// <summary>
        /// Connect to specified debugger endpoint.
        /// </summary>
        /// <param name="uri">URI identifying the endpoint to connect to.</param>
        public void Connect(Uri uri) {
            Utilities.ArgumentNotNull("uri", uri);

            Close();
            lock (_networkClientLock) {
                _networkClient = _networkClientFactory.CreateNetworkClient(uri);
            }
            Task.Factory.StartNew(ReadStreamAsync);
            Task.Factory.StartNew(WriteStreamAsync);
        }

        /// <summary>
        /// Writes messages to debugger input stream.
        /// </summary>
        private async void WriteStreamAsync() {
            INetworkClient networkClient;
            lock (_networkClientLock) {
                networkClient = _networkClient;
            }
            if (networkClient == null) {
                return;
            }

            try {
                using (var streamWriter = new StreamWriter(networkClient.GetStream())) {
                    while (Connected) {
                        string message = await _messages.TakeAsync().ConfigureAwait(false);
                        await streamWriter.WriteAsync(message).ConfigureAwait(false);
                        await streamWriter.FlushAsync().ConfigureAwait(false);
                    }
                }
            } catch (SocketException) {
            } catch (ObjectDisposedException) {
            } catch (IOException) {
            } catch (Exception e) {
                DebugWriteLine(string.Format("DebuggerConnection: failed to write message {0}.", e));
                throw;
            }
        }

        /// <summary>
        /// Reads data from debugger output stream.
        /// </summary>
        private async void ReadStreamAsync() {
            DebugWriteLine("DebuggerConnection: established connection.");

            INetworkClient networkClient;
            lock (_networkClientLock) {
                networkClient = _networkClient;
            }
            if (networkClient == null) {
                return;
            }

            try {
                using (var streamReader = new StreamReader(networkClient.GetStream(), Encoding.Default)) {
                    while (Connected) {
                        // Read message header
                        string result = await streamReader.ReadLineAsync().ConfigureAwait(false);
                        if (result == null) {
                            continue;
                        }

                        // Check whether result is content length header
                        Match match = _contentLength.Match(result);
                        if (!match.Success) {
                            // Check whether result is node.js version string
                            match = _nodeVersion.Match(result);
                            if (match.Success) {
                                NodeVersion = new Version(match.Groups[1].Value);
                            } else {
                                DebugWriteLine(string.Format("Debugger info: {0}", result));
                            }

                            continue;
                        }

                        await streamReader.ReadLineAsync().ConfigureAwait(false);

                        // Retrieve content length
                        int length = int.Parse(match.Groups[1].Value);
                        if (length == 0) {
                            continue;
                        }

                        // Read content
                        string message = await streamReader.ReadLineBlockAsync(length).ConfigureAwait(false);

                        DebugWriteLine("Response: " + message);

                        // Notify subscribers
                        EventHandler<MessageEventArgs> outputMessage = OutputMessage;
                        if (outputMessage != null) {
                            outputMessage(this, new MessageEventArgs(message));
                        }
                    }
                }
            } catch (SocketException) {
            } catch (ObjectDisposedException) {
            } catch (IOException) {
            } catch (Exception e) {
                DebugWriteLine(string.Format("DebuggerConnection: message processing failed {0}.", e));
                throw;
            } finally {
                DebugWriteLine("DebuggerConnection: connection was closed.");

                EventHandler<EventArgs> connectionClosed = ConnectionClosed;
                if (connectionClosed != null) {
                    connectionClosed(this, EventArgs.Empty);
                }
            }
        }

        [Conditional("DEBUG")]
        private void DebugWriteLine(string message) {
            Debug.WriteLine("[{0}] {1}", DateTime.UtcNow.TimeOfDay, message);
        }
    }
}
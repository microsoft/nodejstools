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
        private readonly Regex _nodeVersion = new Regex(@"Embedding-Host: node v([0-9.]+)", RegexOptions.Compiled);
        private readonly ITcpClientFactory _tcpClientFactory;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private ITcpClient _tcpClient;

        public DebuggerConnection(ITcpClientFactory tcpClientFactory) {
            _tcpClientFactory = tcpClientFactory;
            NodeVersion = new Version();
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        public void Close() {
            if (_streamReader != null) {
                _streamReader.Close();
                _streamReader = null;
            }

            if (_streamWriter != null) {
                _streamWriter.Close();
                _streamWriter = null;
            }

            if (_tcpClient != null) {
                _tcpClient.Close();
                _tcpClient = null;
            }
        }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message.</param>
        public async Task SendMessageAsync(string message) {
            Utilities.ArgumentNotNullOrEmpty("message", message);
            Utilities.CheckNotNull(_streamWriter, "No connection with node.js debugger.");

            int byteCount = Encoding.UTF8.GetByteCount(message);
            string request = string.Format("Content-Length: {0}{1}{1}{2}", byteCount, Environment.NewLine, message);

            DebugWriteLine("Request: " + message);

            await _streamWriter.WriteAsync(request).ConfigureAwait(false);
            await _streamWriter.FlushAsync().ConfigureAwait(false);
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
            get { return _tcpClient != null && _tcpClient.Connected; }
        }

        /// <summary>
        /// Gets a node.js version.
        /// </summary>
        public Version NodeVersion { get; private set; }

        /// <summary>
        /// Connect to specified debugger endpoint.
        /// </summary>
        /// <param name="hostName">Host address.</param>
        /// <param name="portNumber">Port number.</param>
        public void Connect(string hostName, int portNumber) {
            Utilities.ArgumentNotNullOrEmpty("hostName", hostName);

            Close();

            _tcpClient = _tcpClientFactory.CreateTcpClient(hostName, portNumber);

            Stream stream = _tcpClient.GetStream();
            _streamReader = new StreamReader(stream, Encoding.Default);
            _streamWriter = new StreamWriter(stream);

            Task.Factory.StartNew(ReadStreamAsync);
        }

        /// <summary>
        /// Asynchronous read of the debugger output stream.
        /// </summary>
        private async void ReadStreamAsync() {
            DebugWriteLine("DebuggerConnection: established connection.");

            try {
                while (Connected) {
                    // Read message header
                    string result = await _streamReader.ReadLineAsync();
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

                    await _streamReader.ReadLineAsync();

                    // Retrieve content length
                    int length = int.Parse(match.Groups[1].Value);
                    if (length == 0) {
                        continue;
                    }

                    // Read content
                    string message = await _streamReader.ReadLineBlockAsync(length);

                    DebugWriteLine("Response: " + message);

                    // Notify subscribers
                    EventHandler<MessageEventArgs> outputMessage = OutputMessage;
                    if (outputMessage != null) {
                        outputMessage(this, new MessageEventArgs(message));
                    }
                }
            } catch (SocketException) {
            } catch (ObjectDisposedException) {
            } catch (IOException) {
            } catch (Exception e) {
                DebugWriteLine(string.Format("DebuggerConnection: message processing failed {0}.", e));
                throw;
            } finally {
                Close();

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
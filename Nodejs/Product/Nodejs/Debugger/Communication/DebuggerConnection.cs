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
using Utilities = Microsoft.VisualStudioTools.Project.Utilities;

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class DebuggerConnection : IDebuggerConnection {
        private string _hostName;
        private ushort _portNumber;
        private readonly Regex _contentLength = new Regex(@"Content-Length: (\d+)", RegexOptions.Compiled);
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private TcpClient _tcpClient;

        public DebuggerConnection(string hostName, ushort portNumber) {
            Utilities.ArgumentNotNullOrEmpty("hostName", hostName);

            _hostName = hostName;
            _portNumber = portNumber;
        }

        public DebuggerConnection(ushort portNumber) : this("localhost", portNumber) {
        }

        public void Close() {
            if (_tcpClient != null) {
                _tcpClient.Close();
                _tcpClient = null;
            }
            
            if (_streamReader != null) {
                _streamReader.Close();
                _streamReader = null;
            }

            if (_streamWriter != null) {
                _streamWriter.Close();
                _streamWriter = null;
            }
        }

        public async Task SendMessageAsync(string message) {
            Utilities.ArgumentNotNullOrEmpty("message", message);
            Utilities.CheckNotNull(_streamWriter, "No connection with node.js debugger.");

            string request = string.Format("Content-Length: {0}{1}{1}{2}", Encoding.UTF8.GetByteCount(message), Environment.NewLine, message);
            Debug.WriteLine(String.Format("Request: {0}", request));

            await _streamWriter.WriteAsync(request).ConfigureAwait(false);
            await _streamWriter.FlushAsync().ConfigureAwait(false);
        }

        public event EventHandler<MessageEventArgs> OutputMessage;
        public event EventHandler<EventArgs> ConnectionClosed;

        /// <summary>
        /// Gets a value indicating whether connection established.
        /// </summary>
        public bool Connected {
            get { return _tcpClient != null && _tcpClient.Connected; }
        }

        public void Connect() {
            Close();

            _tcpClient = new TcpClient(_hostName, _portNumber);
            _streamReader = new StreamReader(_tcpClient.GetStream());
            _streamWriter = new StreamWriter(_tcpClient.GetStream());

            Task.Factory.StartNew(ReadStreamAsync);
        }

        public void Connect(string hostName, ushort portNumber) {
            Utilities.ArgumentNotNullOrEmpty("hostName", hostName);

            _hostName = hostName;
            _portNumber = portNumber;

            Connect();
        }

        /// <summary>
        /// Asynchronous read of the debugger output stream.
        /// </summary>
        private async void ReadStreamAsync() {
            try {
                while (Connected) {
                    // Read message header
                    string result = await _streamReader.ReadLineAsync();
                    if (result == null) {
                        break;
                    }

                    // Check whether result is content length header
                    Match match = _contentLength.Match(result);
                    if (!match.Success) {
                        continue;
                    }

                    await _streamReader.ReadLineAsync();

                    // Retrieve body length
                    int length = int.Parse(match.Groups[1].Value);
                    if (length == 0) {
                        continue;
                    }

                    // Read message body
                    var buffer = new char[length];
                    int count = await _streamReader.ReadBlockAsync(buffer, 0, length);
                    if (count == 0) {
                        break;
                    }

                    // Notify subscribers
                    byte[] bytes = Encoding.UTF8.GetBytes(buffer, 0, count);
                    string message = Encoding.UTF8.GetString(bytes);

                    Debug.Print(message);

                    EventHandler<MessageEventArgs> outputMessage = OutputMessage;
                    if (outputMessage != null) {
                        outputMessage(this, new MessageEventArgs(message));
                    }
                }
            } catch (SocketException) {
            } finally {
                Close();

                Debug.Print("Debugger connection was closed.");

                EventHandler<EventArgs> connectionClosed = ConnectionClosed;
                if (connectionClosed != null) {
                    connectionClosed(this, EventArgs.Empty);
                }
            }
        }
    }
}
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

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class DebuggerConnection : IDebuggerConnection {
        private readonly Regex _contentLength = new Regex(@"Content-Length: (\d+)", RegexOptions.Compiled);
        private readonly Encoding _encoding = Encoding.GetEncoding("latin1");
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private TcpClient _tcpClient;

        public void Dispose() {
            if (_tcpClient != null) {
                _tcpClient.Close();
                _tcpClient = null;
            }
        }

        public void ConnectTo(string host, int port) {
            _tcpClient = new TcpClient(host, port);
            _streamReader = new StreamReader(_tcpClient.GetStream(), _encoding);
            _streamWriter = new StreamWriter(_tcpClient.GetStream(), _encoding);

            Task.Factory.StartNew(ReadStreamAsync);
        }

        public void Close() {
            Dispose();
        }

        public async Task SendMessageAsync(string message) {
            Debug.Print(message);

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            char[] chars = _encoding.GetChars(bytes);
            string messageText = string.Format("Content-Length: {0}{1}{1}", chars.Length, Environment.NewLine);

            await _streamWriter.WriteAsync(messageText).ConfigureAwait(false);
            await _streamWriter.WriteAsync(chars, 0, chars.Length).ConfigureAwait(false);
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

        private async Task<T> HandleExceptionsAsync<T>(Task<T> action) {
            try {
                return await action;
            } catch (Exception e) {
                Debug.Print("Connection failed: {0}", e);

                EventHandler<EventArgs> connectionClosed = ConnectionClosed;
                if (connectionClosed != null) {
                    connectionClosed(this, EventArgs.Empty);
                }

                return default(T);
            }
        }

        /// <summary>
        /// Asynchronous read of the debugger output stream.
        /// </summary>
        private async void ReadStreamAsync() {
            while (_tcpClient != null) {
                // Read message header
                string result = await HandleExceptionsAsync(_streamReader.ReadLineAsync());
                if (result == null) {
                    break;
                }

                // Check whether result is content length header
                Match match = _contentLength.Match(result);
                if (!match.Success) {
                    continue;
                }

                await HandleExceptionsAsync(_streamReader.ReadLineAsync());

                // Retrieve body length
                int length = int.Parse(match.Groups[1].Value);
                if (length == 0) {
                    continue;
                }

                // Read message body
                var buffer = new char[length];
                int count = await HandleExceptionsAsync(_streamReader.ReadBlockAsync(buffer, 0, length));
                if (count == 0) {
                    break;
                }

                // Notify subscribers
                byte[] bytes = _encoding.GetBytes(buffer, 0, count);
                string message = Encoding.UTF8.GetString(bytes);

                Debug.Print(message);

                EventHandler<MessageEventArgs> outputMessage = OutputMessage;
                if (outputMessage != null) {
                    outputMessage(this, new MessageEventArgs(message));
                }
            }
        }
    }
}
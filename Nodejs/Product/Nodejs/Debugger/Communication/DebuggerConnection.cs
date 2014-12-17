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
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class DebuggerConnection : IDebuggerConnection {
        private static readonly Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        private static readonly Regex _contentLengthFieldRegex = new Regex(@"Content-Length: (\d+)", RegexOptions.Compiled);
        private static readonly Regex _nodeVersionFieldRegex = new Regex(@"Embedding-Host: node v([0-9.]+)", RegexOptions.Compiled);

        private readonly AsyncProducerConsumerCollection<byte[]> _packetsToSend = new AsyncProducerConsumerCollection<byte[]>();
        private readonly INetworkClientFactory _networkClientFactory;
        private INetworkClient _networkClient;
        private readonly object _networkClientLock = new object();
        private volatile Version _nodeVersion;

        public DebuggerConnection(INetworkClientFactory networkClientFactory) {
            Utilities.ArgumentNotNull("networkClientFactory", networkClientFactory);

            _networkClientFactory = networkClientFactory;
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

            Debug.WriteLine("Request: " + message, typeof(DebuggerConnection).Name);

            var messageBody = _encoding.GetBytes(message);
            var messageHeader = _encoding.GetBytes(string.Format("Content-Length: {0}\r\n\r\n", messageBody.Length));
            _packetsToSend.Add(messageHeader);
            _packetsToSend.Add(messageBody);
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
        /// Gets a Node.js version, or <c>null</c> if it was not supplied by the debuggee.
        /// </summary>
        public Version NodeVersion {
            get { return _nodeVersion;  }
        }

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

            Task.Factory.StartNew(ReceiveAndDispatchMessagesWorker);
            Task.Factory.StartNew(SendPacketsWorker);
        }

        /// <summary>
        /// Sends packets queued by <see cref="SendMessage"/>.
        /// </summary>
        private async void SendPacketsWorker() {
            INetworkClient networkClient;
            lock (_networkClientLock) {
                networkClient = _networkClient;
            }
            if (networkClient == null) {
                return;
            }

            try {
                var stream = networkClient.GetStream();
                while (Connected) {
                    byte[] packet = await _packetsToSend.TakeAsync().ConfigureAwait(false);
                    await stream.WriteAsync(packet, 0, packet.Length).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);
                }
            } catch (SocketException) {
            } catch (ObjectDisposedException) {
            } catch (IOException) {
            } catch (Exception e) {
                Debug.WriteLine(string.Format("Failed to write message {0}.", e), typeof(DebuggerConnection).Name);
                throw;
            }
        }

        /// <summary>
        /// Receives messages from debugger, parses them to extract the body, and dispatches them to <see cref="OutputMessage"/> listeners.
        /// </summary>
        private async void ReceiveAndDispatchMessagesWorker() {
            Debug.WriteLine("Established connection.", typeof(DebuggerConnection).Name);

            INetworkClient networkClient;
            lock (_networkClientLock) {
                networkClient = _networkClient;
            }
            if (networkClient == null) {
                return;
            }

            try {
                var stream = networkClient.GetStream();

                // Use a single read buffer and a single StringBuilder (periodically cleared) across loop iterations,
                // to avoid costly repeated allocations.
                var buffer = new byte[0x1000];
                var sb = new StringBuilder();

                // Read and process incoming messages until disconnected.
                while (true) {
                    // Read the header of this message.
                    int contentLength = 0;
                    while (true) {
                        // Read a single header field.
                        string field;
                        sb.Clear();
                        while (true) {
                            int bytesRead = await stream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                            if (bytesRead < 1) {
                                // End of stream - we are disconnected from debuggee.
                                throw new EndOfStreamException();
                            }

                            // All fields that we care about are ASCII, and for all the other fields we only need to recognize
                            // the trailing \r\n, so there's no need to do proper decoding here.
                            sb.Append((char)buffer[0]);

                            // "\r\n" terminates the field.
                            if (sb.Length >= 2 && sb[sb.Length - 2] == '\r' && sb[sb.Length - 1] == '\n') {
                                field = sb.ToString(0, sb.Length - 2);
                                break;
                            }
                        }

                        // Blank line terminates the header.
                        if (string.IsNullOrEmpty(field)) {
                            break;
                        }

                        // Otherwise, it's an actual field. Parse it if it's something we care about.

                        // Content-Length
                        var match = _contentLengthFieldRegex.Match(field);
                        if (match.Success) {
                            int.TryParse(match.Groups[1].Value, out contentLength);
                            continue;
                        } 

                        // Embedding-Host, which contains the Node.js version number. Only try parsing that if we don't know the version yet -
                        // it normally comes in the very first packet, so this saves time trying to parse all the consequent ones.
                        if (NodeVersion == null) {
                            match = _nodeVersionFieldRegex.Match(field);
                            if (match.Success) {
                                Version nodeVersion;
                                Version.TryParse(match.Groups[1].Value, out nodeVersion);
                                _nodeVersion = nodeVersion;
                            }
                        }
                    }

                    if (contentLength == 0) {
                        continue;
                    }

                    // Read the body of this message.

                    // If our preallocated buffer is large enough, use it - this should be true for vast majority of messages.
                    // If not, allocate a buffer that is large enough and use that, then throw it away - don't replace the original
                    // buffer with it, so that we don't hold onto a huge chunk of memory for the rest of the debugging session just
                    // because of a single long message.
                    var bodyBuffer = buffer.Length >= contentLength ? buffer : new byte[contentLength];

                    for (int i = 0; i < contentLength; ) {
                        i += await stream.ReadAsync(bodyBuffer, i, contentLength - i).ConfigureAwait(false);
                    }

                    string message = _encoding.GetString(bodyBuffer, 0, contentLength);
                    Debug.WriteLine("Response: " + message, typeof(DebuggerConnection).Name);

                    // Notify subscribers.
                    var outputMessage = OutputMessage;
                    if (outputMessage != null) {
                        outputMessage(this, new MessageEventArgs(message));
                    }
                }
            } catch (SocketException) {
            } catch (IOException) {
            } catch (ObjectDisposedException) {
            } catch (DecoderFallbackException ex) {
                Debug.WriteLine(string.Format("Error decoding response body: {0}", ex), typeof(DebuggerConnection).Name);
            } catch (JsonReaderException ex) {
                Debug.WriteLine(string.Format("Error parsing JSON response: {0}", ex), typeof(DebuggerConnection).Name);
            } catch (Exception ex) {
                Debug.WriteLine(string.Format("Message processing failed: {0}", ex), typeof(DebuggerConnection).Name);
                throw;
            } finally {
                Debug.WriteLine("Connection was closed.", typeof(DebuggerConnection).Name);

                var connectionClosed = ConnectionClosed;
                if (connectionClosed != null) {
                    connectionClosed(this, EventArgs.Empty);
                }
            }
        }
    }
}
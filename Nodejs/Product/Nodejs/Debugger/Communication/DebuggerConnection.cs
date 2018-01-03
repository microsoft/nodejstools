// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Logging;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class DebuggerConnection : IDebuggerConnection
    {
        private static readonly Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        private static readonly Regex _contentLengthFieldRegex = new Regex(@"Content-Length: (\d+)", RegexOptions.Compiled);
        private static readonly Regex _nodeVersionFieldRegex = new Regex(@"Embedding-Host: node v([0-9.]+)", RegexOptions.Compiled);

        private readonly AsyncProducerConsumerCollection<byte[]> _packetsToSend = new AsyncProducerConsumerCollection<byte[]>();
        private readonly INetworkClientFactory _networkClientFactory;
        private INetworkClient _networkClient;
        private readonly object _networkClientLock = new object();
        private volatile Version _nodeVersion;
        private bool _isClosed = false;

        public DebuggerConnection(INetworkClientFactory networkClientFactory)
        {
            Utilities.ArgumentNotNull("networkClientFactory", networkClientFactory);

            this._networkClientFactory = networkClientFactory;
        }

        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Close connection.
        /// </summary>
        public void Close()
        {
            this._isClosed = true;

            lock (this._networkClientLock)
            {
                if (this._networkClient != null)
                {
                    this._networkClient.Dispose();
                    this._networkClient = null;
                }
            }
        }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message.</param>
        public void SendMessage(string message)
        {
            Utilities.ArgumentNotNullOrEmpty("message", message);

            if (!this.Connected)
            {
                return;
            }

            LiveLogger.WriteLine("Request: " + message, typeof(DebuggerConnection));

            var messageBody = _encoding.GetBytes(message);
            var messageHeader = _encoding.GetBytes(string.Format(CultureInfo.InvariantCulture, "Content-Length: {0}\r\n\r\n", messageBody.Length));
            this._packetsToSend.Add(messageHeader);
            this._packetsToSend.Add(messageBody);
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
        public bool Connected
        {
            get
            {
                lock (this._networkClientLock)
                {
                    return this._networkClient != null && this._networkClient.Connected;
                }
            }
        }

        /// <summary>
        /// Gets a Node.js version, or <c>null</c> if it was not supplied by the debuggee.
        /// </summary>
        public Version NodeVersion => this._nodeVersion;
        /// <summary>
        /// Connect to specified debugger endpoint.
        /// </summary>
        /// <param name="uri">URI identifying the endpoint to connect to.</param>
        public void Connect(Uri uri)
        {
            Utilities.ArgumentNotNull("uri", uri);
            LiveLogger.WriteLine("Debugger connecting to URI: {0}", uri);

            Close();
            lock (this._networkClientLock)
            {
                var connection_attempts = 0;
                const int MAX_ATTEMPTS = 5;
                while (true)
                {
                    connection_attempts++;
                    try
                    {
                        // TODO: This currently results in a call to the synchronous TcpClient
                        // constructor, which is a blocking call, and can take a couple of seconds
                        // to connect (with timeouts and retries). This code is running on the UI
                        // thread. Ideally this should be connecting async, or moved off the UI thread.
                        this._networkClient = this._networkClientFactory.CreateNetworkClient(uri);

                        // Unclear if the above can succeed and not be connected, but check for safety.
                        // The code needs to either break out the while loop, or hit the retry logic
                        // in the exception handler.
                        if (this._networkClient.Connected)
                        {
                            LiveLogger.WriteLine("Debugger connected successfully");
                            break;
                        }
                        else
                        {
                            throw new SocketException();
                        }
                    }
                    catch (Exception ex) when (!Microsoft.VisualStudioTools.ExceptionExtensions.IsCriticalException(ex))
                    {
                        LiveLogger.WriteLine("Connection attempt {0} failed with: {1}", connection_attempts, ex);
                        if (this._isClosed || connection_attempts >= MAX_ATTEMPTS)
                        {
                            throw;
                        }
                        else
                        {
                            // See above TODO. This should be moved off the UI thread or posted to retry
                            // without blocking in the meantime. For now, this seems the lesser of two
                            // evils. (The other being the debugger failing to attach on launch if the
                            // debuggee socket wasn't open quickly enough).
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                }
            }

            Task.Factory.StartNew(this.ReceiveAndDispatchMessagesWorker);
            Task.Factory.StartNew(this.SendPacketsWorker);
        }

        /// <summary>
        /// Sends packets queued by <see cref="SendMessage"/>.
        /// </summary>
        private async void SendPacketsWorker()
        {
            INetworkClient networkClient;
            lock (this._networkClientLock)
            {
                networkClient = this._networkClient;
            }
            if (networkClient == null)
            {
                return;
            }

            try
            {
                var stream = networkClient.GetStream();
                while (this.Connected)
                {
                    var packet = await this._packetsToSend.TakeAsync().ConfigureAwait(false);
                    await stream.WriteAsync(packet, 0, packet.Length).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);
                }
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (IOException)
            {
            }
            catch (Exception e)
            {
                LiveLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, "Failed to write message {0}.", e), typeof(DebuggerConnection));
                throw;
            }
        }

        /// <summary>
        /// Receives messages from debugger, parses them to extract the body, and dispatches them to <see cref="OutputMessage"/> listeners.
        /// </summary>
        private async void ReceiveAndDispatchMessagesWorker()
        {
            LiveLogger.WriteLine("Established connection.", typeof(DebuggerConnection));

            INetworkClient networkClient;
            lock (this._networkClientLock)
            {
                networkClient = this._networkClient;
            }
            if (networkClient == null)
            {
                return;
            }

            try
            {
                var stream = networkClient.GetStream();

                // Use a single read buffer and a single StringBuilder (periodically cleared) across loop iterations,
                // to avoid costly repeated allocations.
                var buffer = new byte[0x1000];
                var sb = new StringBuilder();

                // Read and process incoming messages until disconnected.
                while (true)
                {
                    // Read the header of this message.
                    var contentLength = 0;
                    while (true)
                    {
                        // Read a single header field.
                        string field;
                        sb.Clear();
                        while (true)
                        {
                            var bytesRead = await stream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                            if (bytesRead < 1)
                            {
                                // End of stream - we are disconnected from debuggee.
                                throw new EndOfStreamException();
                            }

                            // All fields that we care about are ASCII, and for all the other fields we only need to recognize
                            // the trailing \r\n, so there's no need to do proper decoding here.
                            sb.Append((char)buffer[0]);

                            // "\r\n" terminates the field.
                            if (sb.Length >= 2 && sb[sb.Length - 2] == '\r' && sb[sb.Length - 1] == '\n')
                            {
                                field = sb.ToString(0, sb.Length - 2);
                                break;
                            }
                        }

                        // Blank line terminates the header.
                        if (string.IsNullOrEmpty(field))
                        {
                            break;
                        }

                        // Otherwise, it's an actual field. Parse it if it's something we care about.

                        // Content-Length
                        var match = _contentLengthFieldRegex.Match(field);
                        if (match.Success)
                        {
                            int.TryParse(match.Groups[1].Value, out contentLength);
                            continue;
                        }

                        // Embedding-Host, which contains the Node.js version number. Only try parsing that if we don't know the version yet -
                        // it normally comes in the very first packet, so this saves time trying to parse all the consequent ones.
                        if (this.NodeVersion == null)
                        {
                            match = _nodeVersionFieldRegex.Match(field);
                            if (match.Success)
                            {
                                Version.TryParse(match.Groups[1].Value, out var nodeVersion);
                                this._nodeVersion = nodeVersion;
                            }
                        }
                    }

                    if (contentLength == 0)
                    {
                        continue;
                    }

                    // Read the body of this message.

                    // If our preallocated buffer is large enough, use it - this should be true for vast majority of messages.
                    // If not, allocate a buffer that is large enough and use that, then throw it away - don't replace the original
                    // buffer with it, so that we don't hold onto a huge chunk of memory for the rest of the debugging session just
                    // because of a single long message.
                    var bodyBuffer = buffer.Length >= contentLength ? buffer : new byte[contentLength];

                    for (var i = 0; i < contentLength;)
                    {
                        i += await stream.ReadAsync(bodyBuffer, i, contentLength - i).ConfigureAwait(false);
                    }

                    var message = _encoding.GetString(bodyBuffer, 0, contentLength);
                    LiveLogger.WriteLine("Response: " + message, typeof(DebuggerConnection));

                    // Notify subscribers.
                    var outputMessage = OutputMessage;
                    if (outputMessage != null)
                    {
                        outputMessage(this, new MessageEventArgs(message));
                    }
                }
            }
            catch (SocketException)
            {
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (DecoderFallbackException ex)
            {
                LiveLogger.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error decoding response body: {0}", ex), typeof(DebuggerConnection));
            }
            catch (JsonReaderException ex)
            {
                LiveLogger.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error parsing JSON response: {0}", ex), typeof(DebuggerConnection));
            }
            catch (Exception ex)
            {
                LiveLogger.WriteLine(string.Format(CultureInfo.InvariantCulture, "Message processing failed: {0}", ex), typeof(DebuggerConnection));
                throw;
            }
            finally
            {
                LiveLogger.WriteLine("Connection was closed.", typeof(DebuggerConnection));

                var connectionClosed = ConnectionClosed;
                if (connectionClosed != null)
                {
                    connectionClosed(this, EventArgs.Empty);
                }
            }
        }
    }
}

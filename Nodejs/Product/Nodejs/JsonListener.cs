// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Microsoft.NodejsTools
{
    /// <summary>
    /// Base class for listening to a socket where we're communicating by sending JSON over
    /// the wire.  Usage is to subclass, set the socket, and then override ProcessPacket.
    /// </summary>
    internal abstract class JsonListener
    {
        private readonly byte[] _socketBuffer = new byte[4096];
        private Socket _socket;

        protected void StartListenerThread()
        {
            var debuggerThread = new Thread(this.ListenerThread);
            debuggerThread.Name = GetType().Name + " Thread";
            debuggerThread.Start();
        }

        private void ListenerThread()
        {
            var pos = 0;
            var text = Array.Empty<byte>();

            // Use a local for Socket to keep nulling of _socket field (on non listener thread)
            // from causing spurious null dereferences
            var socket = this._socket;

            try
            {
                if (socket != null && socket.Connected)
                {
                    // _socket == null || !_socket.Connected effectively stops listening and associated packet processing
                    while (this._socket != null && socket.Connected)
                    {
                        if (pos >= text.Length)
                        {
                            ReadMoreData(socket.Receive(this._socketBuffer), ref text, ref pos);
                        }

                        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        while (this._socket != null && socket.Connected)
                        {
                            var newPos = text.FirstNewLine(pos);
                            if (newPos == pos)
                            {
                                // double \r\n, we're done with headers.
                                pos += 2;
                                break;
                            }
                            else if (newPos == -1)
                            {
                                // we need to get more data...
                                ReadMoreData(socket.Receive(this._socketBuffer), ref text, ref pos);
                            }
                            else
                            {
                                // continue onto next header
                                // save header, continue to the next one.
                                var nameEnd = text.IndexOf((byte)':', pos, newPos - pos);
                                if (nameEnd != -1)
                                {
                                    var headerName = text.Substring(pos, nameEnd - pos);
                                    var headerNameStr = Encoding.UTF8.GetString(headerName).Trim();

                                    var headerValue = text.Substring(nameEnd + 1, newPos - nameEnd - 1);
                                    var headerValueStr = Encoding.UTF8.GetString(headerValue).Trim();
                                    headers[headerNameStr] = headerValueStr;
                                }
                                pos = newPos + 2;
                            }
                        }

                        var body = string.Empty;
                        if (headers.TryGetValue("Content-Length", out var contentLen))
                        {
                            var lengthRemaining = int.Parse(contentLen, CultureInfo.InvariantCulture);
                            if (lengthRemaining != 0)
                            {
                                var bodyBuilder = new StringBuilder();

                                while (this._socket != null && socket.Connected)
                                {
                                    var len = Math.Min(text.Length - pos, lengthRemaining);
                                    bodyBuilder.Append(Encoding.UTF8.GetString(text.Substring(pos, len)));
                                    pos += len;

                                    lengthRemaining -= len;

                                    if (lengthRemaining == 0)
                                    {
                                        break;
                                    }

                                    ReadMoreData(socket.Receive(this._socketBuffer), ref text, ref pos);
                                }
                                body = bodyBuilder.ToString();
                            }
                        }

                        if (this._socket != null && socket.Connected)
                        {
                            try
                            {
                                ProcessPacket(new JsonResponse(headers, body));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: {0}", e);
                            }
                        }
                    }
                }
            }
            catch (SocketException)
            {
            }
            finally
            {
                Debug.Assert(this._socket == null || !this._socket.Connected);
                if (socket != null && socket.Connected)
                {
                    socket.Disconnect(false);
                }
                OnSocketDisconnected();
            }
        }

        protected abstract void OnSocketDisconnected();
        protected abstract void ProcessPacket(JsonResponse response);

        private void ReadMoreData(int bytesRead, ref byte[] text, ref int pos)
        {
            var combinedText = new byte[bytesRead + text.Length - pos];
            Buffer.BlockCopy(text, pos, combinedText, 0, text.Length - pos);
            Buffer.BlockCopy(this._socketBuffer, 0, combinedText, text.Length - pos, bytesRead);
            text = combinedText;
            pos = 0;
        }

        protected Socket Socket
        {
            get
            {
                return this._socket;
            }
            set
            {
                this._socket = value;
            }
        }
    }

    internal static class ByteExtensions
    {
        public static int IndexOf(this byte[] bytes, byte ch, int start, int count)
        {
            for (var i = start; i < start + count && i < bytes.Length; i++)
            {
                if (bytes[i] == ch)
                {
                    return i;
                }
            }
            return -1;
        }

        public static byte[] Substring(this byte[] bytes, int start, int length)
        {
            var res = new byte[length];
            for (var i = 0; i < length; i++)
            {
                res[i] = bytes[i + start];
            }
            return res;
        }

        public static int FirstNewLine(this byte[] bytes, int start)
        {
            for (var i = start; i < bytes.Length - 1; i++)
            {
                if (bytes[i] == '\r' && bytes[i + 1] == '\n')
                {
                    return i;
                }
            }
            return -1;
        }
    }

    internal class JsonResponse
    {
        public readonly Dictionary<string, string> Headers;
        public readonly string Body;

        public JsonResponse(Dictionary<string, string> headers, string body)
        {
            this.Headers = headers;
            this.Body = body;
        }
    }
}

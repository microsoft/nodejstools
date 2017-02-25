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
            int pos = 0;
            byte[] text = Array.Empty<byte>();

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

                        Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        while (this._socket != null && socket.Connected)
                        {
                            int newPos = text.FirstNewLine(pos);
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
                                int nameEnd = text.IndexOf((byte)':', pos, newPos - pos);
                                if (nameEnd != -1)
                                {
                                    var headerName = text.Substring(pos, nameEnd - pos);
                                    string headerNameStr = Encoding.UTF8.GetString(headerName).Trim();

                                    var headerValue = text.Substring(nameEnd + 1, newPos - nameEnd - 1);
                                    string headerValueStr = Encoding.UTF8.GetString(headerValue).Trim();
                                    headers[headerNameStr] = headerValueStr;
                                }
                                pos = newPos + 2;
                            }
                        }

                        string body = String.Empty;
                        string contentLen;
                        if (headers.TryGetValue("Content-Length", out contentLen))
                        {
                            int lengthRemaining = int.Parse(contentLen, CultureInfo.InvariantCulture);
                            if (lengthRemaining != 0)
                            {
                                StringBuilder bodyBuilder = new StringBuilder();

                                while (this._socket != null && socket.Connected)
                                {
                                    int len = Math.Min(text.Length - pos, lengthRemaining);
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
            byte[] combinedText = new byte[bytesRead + text.Length - pos];
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
            for (int i = start; i < start + count && i < bytes.Length; i++)
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
            byte[] res = new byte[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = bytes[i + start];
            }
            return res;
        }

        public static int FirstNewLine(this byte[] bytes, int start)
        {
            for (int i = start; i < bytes.Length - 1; i++)
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

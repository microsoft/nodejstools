// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class WebSocketNetworkClient : INetworkClient
    {
        private readonly ClientWebSocket _webSocket;
        private readonly WebSocketStream _stream;

        public WebSocketNetworkClient(Uri uri)
        {
            // iisnode starts node.exe processes lazily on the first incoming request, and will terminate them after a period
            // of inactivity, making it impossible to attach. So before trying to connect to the debugger, "ping" the website
            // via HTTP to ensure that we have something to connect to.
            try
            {
                var httpRequest = WebRequest.Create(new UriBuilder(uri) { Scheme = "http", Port = -1, Path = "/" }.Uri);
                httpRequest.Method = WebRequestMethods.Http.Head;
                httpRequest.Timeout = 5000;
                httpRequest.GetResponse().Dispose();
            }
            catch (WebException)
            {
                // If it fails or times out, just go ahead and try to connect anyway, and rely on normal error reporting path.
            }

            this._webSocket = new ClientWebSocket();
            this._webSocket.ConnectAsync(uri, CancellationToken.None).GetAwaiter().GetResult();
            this._stream = new WebSocketStream(this._webSocket);
        }

        public bool Connected => this._webSocket.State == WebSocketState.Open;
        public void Dispose()
        {
            this._stream.Dispose();
            try
            {
                this._webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).GetAwaiter().GetResult();
                this._webSocket.Dispose();
            }
            catch (WebSocketException)
            {
                // We don't care about any errors when cleaning up and closing connection.
            }
        }

        public Stream GetStream()
        {
            return this._stream;
        }
    }
}

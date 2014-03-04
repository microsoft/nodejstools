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
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class WebSocketNetworkClient : INetworkClient {
        private readonly ClientWebSocket _webSocket;
        private readonly WebSocketStream _stream;

        public WebSocketNetworkClient(Uri uri) {
            _webSocket = new ClientWebSocket();
            _webSocket.ConnectAsync(uri, CancellationToken.None).GetAwaiter().GetResult();
            _stream = new WebSocketStream(_webSocket);
        }

        public bool Connected {
            get { return _webSocket.State == WebSocketState.Open; }
        }

        public void Dispose() {
            _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).GetAwaiter().GetResult();
            _webSocket.Dispose();
        }

        public Stream GetStream() {
            return _stream;
        }
    }
}
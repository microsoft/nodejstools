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

        public bool Connected
        {
            get { return this._webSocket.State == WebSocketState.Open; }
        }

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
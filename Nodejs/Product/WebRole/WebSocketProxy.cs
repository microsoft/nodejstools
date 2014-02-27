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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace Microsoft.NodejsTools.Debugger {
    public class WebSocketProxy : IHttpHandler {

        private const int DefaultPort = 5858;

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {
            if (context.IsWebSocketRequest) {
                context.AcceptWebSocketRequest(WebSocketRequestHandler);
            } else {
                context.Response.ContentType = "text/html";
                context.Response.ContentEncoding = Encoding.UTF8;

                using (var stream = GetType().Assembly.GetManifestResourceStream("Microsoft.NodejsTools.WebRole.WebSocketProxy.html"))
                using (var reader = new StreamReader(stream)) {
                    string html = reader.ReadToEnd();
                    var wsUri = new UriBuilder(context.Request.Url) { Scheme = context.Request.IsSecureConnection ? "wss" : "ws" };
                    context.Response.Write(html.Replace("{{WS_URI}}", wsUri.ToString()));
                }

                context.Response.End();
            }
        }

        private async Task WebSocketRequestHandler(AspNetWebSocketContext context) {
            using (var tcpClient = new TcpClient("localhost", DefaultPort)) {
                var stream = tcpClient.GetStream();
                var webSocket = context.WebSocket;
                var cts = new CancellationTokenSource();

                // Start the workers that copy data from one socket to the other in both directions, and wait until either
                // completes. The workers are fully async, and so their loops are transparently interleaved when running.
                // Usually end of session is caused by VS dropping its connection on detach, and so it will be
                // CopyFromWebSocketToStream that returns first; but it can be the other one if node process crashes.
                var copyFromStreamToWebSocketTask = CopyFromStreamToWebSocketWorker(stream, webSocket, cts.Token);
                var copyFromWebSocketToStreamTask = CopyFromWebSocketToStreamWorker(webSocket, stream, cts.Token);
                try {
                    await Task.WhenAny(copyFromStreamToWebSocketTask, copyFromWebSocketToStreamTask);
                } catch (IOException) {
                } catch (WebSocketException) {
                }

                // Now that one worker is done, try to gracefully terminate the other one by issuing a cancellation request.
                // it is normally blocked on a read, and this will cancel it if possible, and throw OperationCanceledException.
                cts.Cancel();
                try {
                    await Task.WhenAny(Task.WhenAll(copyFromStreamToWebSocketTask, copyFromWebSocketToStreamTask), Task.Delay(1000));
                } catch (OperationCanceledException) {
                }
            }
        }

        private async Task CopyFromStreamToWebSocketWorker(Stream stream, WebSocket webSocket, CancellationToken ct) {
            var buffer = new byte[0x10000];
            while (true) {
                ct.ThrowIfCancellationRequested();
                int count = await stream.ReadAsync(buffer, 0, buffer.Length, ct);
                if (count == 0) {
                    break;
                }
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, count), WebSocketMessageType.Binary, true, ct);
            }
        }

        private async Task CopyFromWebSocketToStreamWorker(WebSocket webSocket, Stream stream, CancellationToken ct) {
            var buffer = new ArraySegment<byte>(new byte[0x10000]);
            while (webSocket.State == WebSocketState.Open) {
                ct.ThrowIfCancellationRequested();
                var recv = await webSocket.ReceiveAsync(buffer, ct);
                await stream.WriteAsync(buffer.Array, 0, recv.Count, ct);
            }
        }
    }
}


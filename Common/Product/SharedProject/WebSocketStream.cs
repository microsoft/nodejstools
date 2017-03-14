// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudioTools
{
    /// <summary>
    /// Wraps a <see cref="WebSocket"/> instance and exposes its interface as a generic <see cref="Stream"/>.
    /// </summary>
    internal class WebSocketStream : Stream
    {
        private readonly WebSocket _webSocket;
        private bool _ownsSocket;

        public WebSocketStream(WebSocket webSocket, bool ownsSocket = false)
        {
            this._webSocket = webSocket;
            this._ownsSocket = ownsSocket;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && this._ownsSocket)
            {
                this._webSocket.Dispose();
            }
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                return (await this._webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cancellationToken).ConfigureAwait(false)).Count;
            }
            catch (WebSocketException ex)
            {
                throw new IOException(ex.Message, ex);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                await this._webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
            }
            catch (WebSocketException ex)
            {
                throw new IOException(ex.Message, ex);
            }
        }

        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
    }
}


// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class NetworkClientFactory : INetworkClientFactory
    {
        public INetworkClient CreateNetworkClient(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsAbsoluteUri)
            {
                switch (uri.Scheme)
                {
                    case "tcp":
                        if (uri.Port < 0)
                        {
                            throw new ArgumentException("tcp:// URI must include port number", "uri");
                        }
                        return new TcpNetworkClient(uri.Host, uri.Port);
                    case "ws":
                    case "wss":
                        return new WebSocketNetworkClient(uri);
                }
            }

            throw new ArgumentException("tcp://, ws:// or wss:// URI required", "uri");
        }
    }
}

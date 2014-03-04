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

namespace Microsoft.NodejsTools.Debugger.Communication {
    sealed class NetworkClientFactory : INetworkClientFactory {
        public INetworkClient CreateNetworkClient(Uri uri) {
            if (uri == null) {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsAbsoluteUri) {
                switch (uri.Scheme) {
                    case "tcp":
                        if (uri.Port < 0) {
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
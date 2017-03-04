// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.Net.Sockets;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class TcpNetworkClient : INetworkClient
    {
        private readonly TcpClient _tcpClient;

        public TcpNetworkClient(string hostName, int portNumber)
        {
            this._tcpClient = new TcpClient(hostName, portNumber);
        }

        public bool Connected => this._tcpClient.Connected;
        public void Dispose()
        {
            this._tcpClient.Close();
        }

        public Stream GetStream()
        {
            return this._tcpClient.GetStream();
        }
    }
}


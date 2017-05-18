// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal interface IDebuggerConnection : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether connection established.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets a Node.js version.
        /// </summary>
        Version NodeVersion { get; }

        /// <summary>
        /// Connect to specified debugger endpoint.
        /// </summary>
        /// <param name="uri">URI identifying the endpoint to connect to.</param>
        void Connect(Uri uri);

        /// <summary>
        /// Close connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">Message.</param>
        void SendMessage(string message);

        /// <summary>
        /// Fired when received inbound message.
        /// </summary>
        event EventHandler<MessageEventArgs> OutputMessage;

        /// <summary>
        /// Fired when connection was closed.
        /// </summary>
        event EventHandler<EventArgs> ConnectionClosed;
    }
}


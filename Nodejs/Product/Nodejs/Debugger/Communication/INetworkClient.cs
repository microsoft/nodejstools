// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal interface INetworkClient : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether client is connected to a remote host.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Returns the <see cref="T:System.IO.Stream" /> used to send and receive data.
        /// </summary>
        /// <returns>The underlying <see cref="T:System.IO.Stream" /></returns>
        Stream GetStream();
    }
}

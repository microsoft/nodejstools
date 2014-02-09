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

using System.IO;

namespace Microsoft.NodejsTools.Debugger.Communication {
    interface ITcpClient {
        /// <summary>
        /// Gets a value indicating whether client is connected to a remote host.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Disposes instance and requests that the underlying tcp connection be closed.
        /// </summary>
        void Close();

        /// <summary>
        /// Returns the <see cref="T:System.IO.Stream" /> used to send and receive data.
        /// </summary>
        /// <returns>The underlying <see cref="T:System.IO.Stream" /></returns>
        Stream GetStream();
    }
}
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
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Defines an interface for a node debugger.
    /// </summary>
    interface INodeConnection {
        /// <summary>
        /// Gets a value indicating whether connection established.
        /// </summary>
        bool Connected { get; }

        event EventHandler<EventArgs> SocketDisconnected;
        event EventHandler<NodeEventEventArgs> NodeEvent;

        /// <summary>
        /// Connects to the node debugger.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects from the node debugger.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a command to the node debugger.
        /// </summary>
        /// <param name="command">Command name.</param>
        /// <param name="args">Command arguments.</param>
        /// <param name="successHandler">Successful handler.</param>
        /// <param name="failureHandler">Failure handler.</param>
        /// <param name="timeout">Timeout interval in ms.</param>
        /// <param name="shortCircuitPredicate"></param>
        /// <returns></returns>
        bool SendRequest(
            string command,
            Dictionary<string, object> args = null,
            Action<Dictionary<string, object>> successHandler = null,
            Action<Dictionary<string, object>> failureHandler = null,
            int? timeout = null,
            Func<bool> shortCircuitPredicate = null);
    }
}
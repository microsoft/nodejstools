// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Debugger.Commands;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal interface IDebuggerClient
    {
        /// <summary>
        /// Send a command to debugger.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendRequestAsync(DebuggerCommand command, CancellationToken cancellationToken = new CancellationToken());

        /// <summary>
        /// Break point event handler.
        /// </summary>
        event EventHandler<BreakpointEventArgs> BreakpointEvent;

        /// <summary>
        /// Compile script event handler.
        /// </summary>
        event EventHandler<CompileScriptEventArgs> CompileScriptEvent;

        /// <summary>
        /// Exception event handler.
        /// </summary>
        event EventHandler<ExceptionEventArgs> ExceptionEvent;
    }
}


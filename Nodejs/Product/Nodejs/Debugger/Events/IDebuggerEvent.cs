// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger.Events
{
    internal interface IDebuggerEvent
    {
        /// <summary>
        /// Gets a value indicating whether command execution in progress.
        /// </summary>
        bool Running { get; }
    }
}


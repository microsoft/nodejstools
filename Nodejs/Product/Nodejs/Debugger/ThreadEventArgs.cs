// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Event args for start/stop of threads.
    /// </summary>
    internal class ThreadEventArgs : EventArgs
    {
        public readonly NodeThread Thread;

        public ThreadEventArgs(NodeThread thread)
        {
            this.Thread = thread;
        }
    }
}


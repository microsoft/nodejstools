// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class OutputEventArgs : EventArgs
    {
        public readonly NodeThread Thread;
        public readonly string Output;

        public OutputEventArgs(NodeThread thread, string output)
        {
            this.Thread = thread;
            this.Output = output;
        }
    }
}

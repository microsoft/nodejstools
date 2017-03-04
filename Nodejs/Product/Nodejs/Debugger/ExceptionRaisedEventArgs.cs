// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    internal class ExceptionRaisedEventArgs : EventArgs
    {
        public readonly NodeThread Thread;
        public readonly NodeException Exception;
        public readonly bool IsUnhandled;

        public ExceptionRaisedEventArgs(NodeThread thread, NodeException exception, bool isUnhandled)
        {
            this.Thread = thread;
            this.Exception = exception;
            this.IsUnhandled = isUnhandled;
        }
    }
}


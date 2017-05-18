// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.NodejsTools.Debugger.Events;

namespace Microsoft.NodejsTools.Debugger.Communication
{
    internal sealed class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(ExceptionEvent exceptionEvent)
        {
            this.ExceptionEvent = exceptionEvent;
        }

        public ExceptionEvent ExceptionEvent { get; private set; }
    }
}

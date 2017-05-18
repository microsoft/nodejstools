// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class ProcessExitedEventArgs : EventArgs
    {
        public readonly int ExitCode;

        public ProcessExitedEventArgs(int exitCode)
        {
            this.ExitCode = exitCode;
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    [Serializable]
    internal class DebuggerCommandException : Exception
    {
        public DebuggerCommandException() { }
        public DebuggerCommandException(string message) : base(message) { }
        public DebuggerCommandException(string message, Exception innerException) : base(message, innerException) { }
        protected DebuggerCommandException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}


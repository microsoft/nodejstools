// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class DisconnectCommand : DebuggerCommand
    {
        public DisconnectCommand(int id) : base(id, "disconnect") { }
    }
}

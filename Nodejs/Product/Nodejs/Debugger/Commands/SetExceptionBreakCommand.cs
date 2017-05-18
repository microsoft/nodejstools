// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class SetExceptionBreakCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public SetExceptionBreakCommand(int id, bool uncaughtExceptions, bool enabled) : base(id, "setexceptionbreak")
        {
            this._arguments = new Dictionary<string, object> {
                { "type", uncaughtExceptions ? "uncaught" : "all" },
                { "enabled", enabled }
            };
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
    }
}

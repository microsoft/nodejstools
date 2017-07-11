// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class ContinueCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;

        public ContinueCommand(int id, SteppingKind stepping, int stepCount = 1) : base(id, "continue")
        {
            switch (stepping)
            {
                case SteppingKind.Into:
                    this._arguments = new Dictionary<string, object> {
                        { "stepaction", "in" },
                        { "stepcount", stepCount }
                    };
                    break;

                case SteppingKind.Out:
                    this._arguments = new Dictionary<string, object> {
                        { "stepaction", "out" },
                        { "stepcount", stepCount }
                    };
                    break;

                case SteppingKind.Over:
                    this._arguments = new Dictionary<string, object> {
                        { "stepaction", "next" },
                        { "stepcount", stepCount }
                    };
                    break;
            }
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
    }
}

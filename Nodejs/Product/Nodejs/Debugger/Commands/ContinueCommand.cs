/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class ContinueCommand : DebuggerCommand {
        private readonly Dictionary<string, object> _arguments;

        public ContinueCommand(int id, SteppingKind stepping, int stepCount = 1) : base(id, "continue") {
            switch (stepping) {
                case SteppingKind.Into:
                    _arguments = new Dictionary<string, object> {
                        { "stepaction", "in" },
                        { "stepcount", stepCount }
                    };
                    break;

                case SteppingKind.Out:
                    _arguments = new Dictionary<string, object> {
                        { "stepaction", "out" },
                        { "stepcount", stepCount }
                    };
                    break;

                case SteppingKind.Over:
                    _arguments = new Dictionary<string, object> {
                        { "stepaction", "next" },
                        { "stepcount", stepCount }
                    };
                    break;
            }
        }

        protected override IDictionary<string, object> Arguments {
            get { return _arguments; }
        }
    }
}
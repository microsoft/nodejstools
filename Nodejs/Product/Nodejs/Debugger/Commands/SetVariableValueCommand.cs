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
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class SetVariableValueCommand : DebuggerCommandBase {
        private readonly string _name;
        private readonly IEvaluationResultFactory _resultFactory;
        private readonly NodeStackFrame _stackFrame;

        public SetVariableValueCommand(int id, IEvaluationResultFactory resultFactory, NodeStackFrame stackFrame, string name, int handle) : base(id) {
            Utilities.ArgumentNotNull("resultFactory", resultFactory);
            Utilities.ArgumentNotNull("stackFrame", stackFrame);
            Utilities.ArgumentNotNullOrEmpty("name", name);

            _resultFactory = resultFactory;
            _stackFrame = stackFrame;
            _name = name;

            CommandName = "setVariableValue";
            Arguments = new Dictionary<string, object> {
                { "name", name },
                { "newValue", new { handle } },
                { "scope", new { frameNumber = stackFrame.FrameId, number = 0 } }
            };
        }

        public NodeEvaluationResult Result { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            var variableProvider = new NodeSetValueVariable(_stackFrame, _name, response);
            Result = _resultFactory.Create(variableProvider);
        }
    }
}
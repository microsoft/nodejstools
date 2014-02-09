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
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    sealed class EvaluateCommand : DebuggerCommandBase {
        private readonly string _expression;
        private readonly IEvaluationResultFactory _resultFactory;
        private readonly NodeStackFrame _stackFrame;

        public EvaluateCommand(int id, IEvaluationResultFactory resultFactory, string expression, NodeStackFrame stackFrame = null) : base(id) {
            _resultFactory = resultFactory;
            _expression = expression;
            _stackFrame = stackFrame;

            CommandName = "evaluate";
            Arguments = new Dictionary<string, object> {
                { "expression", _expression },
                { "frame", _stackFrame != null ? _stackFrame.FrameId : 0 },
                { "global", false },
                { "disable_break", true },
                { "maxStringLength", -1 }
            };
        }

        public NodeEvaluationResult Result { get; private set; }

        public override void ProcessResponse(JObject response) {
            base.ProcessResponse(response);

            var variableProvider = new NodeEvaluationVariable(_stackFrame, _expression, response["body"]);
            Result = _resultFactory.Create(variableProvider);
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.NodejsTools.Debugger.Serialization;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal sealed class SetVariableValueCommand : DebuggerCommand
    {
        private readonly Dictionary<string, object> _arguments;
        private readonly string _name;
        private readonly IEvaluationResultFactory _resultFactory;
        private readonly NodeStackFrame _stackFrame;

        public SetVariableValueCommand(int id, IEvaluationResultFactory resultFactory, NodeStackFrame stackFrame, string name, int handle)
            : base(id, "setVariableValue")
        {
            Utilities.ArgumentNotNull("resultFactory", resultFactory);
            Utilities.ArgumentNotNull("stackFrame", stackFrame);
            Utilities.ArgumentNotNullOrEmpty("name", name);

            this._resultFactory = resultFactory;
            this._stackFrame = stackFrame;
            this._name = name;

            this._arguments = new Dictionary<string, object> {
                { "name", name },
                { "newValue", new { handle } },
                { "scope", new { frameNumber = stackFrame.FrameId, number = 0 } }
            };
        }

        protected override IDictionary<string, object> Arguments => this._arguments;
        public NodeEvaluationResult Result { get; private set; }

        public override void ProcessResponse(JObject response)
        {
            base.ProcessResponse(response);

            var variableProvider = new NodeSetValueVariable(this._stackFrame, this._name, response);
            this.Result = this._resultFactory.Create(variableProvider);
        }
    }
}

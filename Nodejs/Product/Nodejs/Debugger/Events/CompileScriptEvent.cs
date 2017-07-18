// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Debugger.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Events
{
    internal sealed class CompileScriptEvent : IDebuggerEvent
    {
        public CompileScriptEvent(JObject message)
        {
            this.Running = (bool)message["running"];

            var scriptId = (int)message["body"]["script"]["id"];
            var fileName = (string)message["body"]["script"]["name"] ?? NodeVariableType.UnknownModule;

            this.Module = new NodeModule(scriptId, fileName);
        }

        public NodeModule Module { get; private set; }
        public bool Running { get; private set; }
    }
}

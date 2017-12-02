// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands
{
    internal abstract class DebuggerCommand
    {
        private readonly string _commandName;

        protected DebuggerCommand(int id, string commandName)
        {
            this.Id = id;
            this._commandName = commandName;
        }

        /// <summary>
        /// Gets a command arguments.
        /// </summary>
        protected virtual IDictionary<string, object> Arguments => null;
        /// <summary>
        /// Gets a command identifier.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets a value indicating whether command execution in progress.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Parses response message.
        /// </summary>
        /// <param name="response">Message.</param>
        /// <returns>Indicates whether command execution succeeded.</returns>
        public virtual void ProcessResponse(JObject response)
        {
            this.Running = (bool?)response["running"] ?? false;

            if (!(bool)response["success"])
            {
                var message = (string)response["message"];
                throw new DebuggerCommandException(message);
            }
        }

        /// <summary>
        /// Serializes a command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(
                new
                {
                    command = _commandName,
                    seq = Id,
                    type = "request",
                    arguments = Arguments
                });
        }
    }
}

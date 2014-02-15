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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Debugger.Commands {
    abstract class DebuggerCommand {
        private readonly string _commandName;

        protected DebuggerCommand(int id, string commandName) {
            Id = id;
            _commandName = commandName;
        }

        /// <summary>
        /// Gets a command arguments.
        /// </summary>
        protected virtual IDictionary<string, object> Arguments {
            get { return null; }
        }

        /// <summary>
        /// Gets a command identifier.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets a value indicating whether command execution in progress.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Parses response message.
        /// </summary>
        /// <param name="response">Message.</param>
        /// <returns>Indicates whether command execution succeeded.</returns>
        public virtual void ProcessResponse(JObject response) {
            Running = (bool)response["running"];

            if (!(bool)response["success"]) {
                var message = (string)response["message"];
                throw new DebuggerCommandException(message);
            }
        }

        /// <summary>
        /// Serializes a command.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return JsonConvert.SerializeObject(
                new {
                    command = _commandName,
                    seq = Id,
                    type = "request",
                    arguments = Arguments
                });
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Represents a command which can be run from a REPL window.
    /// 
    /// This interface is a MEF contract and can be implemented and exported to add commands to the REPL window.
    /// This is new in v1.5.
    /// </summary>
    public interface IReplCommand2 : IReplCommand
    {
        /// <summary>
        /// Gets a list of aliases for the command.
        /// </summary>
        IEnumerable<string> Aliases
        {
            get;
        }
    }
}


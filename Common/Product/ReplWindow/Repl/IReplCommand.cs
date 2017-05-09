// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Represents a command which can be run from a REPL window.
    /// 
    /// This interface is a MEF contract and can be implemented and exported to add commands to the REPL window.
    /// </summary>
    public interface IReplCommand
    {
        /// <summary>
        /// Asynchronously executes the command with specified arguments and calls back the given completion when finished.
        /// </summary>
        /// <param name="window">The interactive window.</param>
        /// <returns>The task that completes the execution.</returns>
        Task<ExecutionResult> Execute(IReplWindow window, string arguments);

        /// <summary>
        /// Gets a description of the REPL command which is displayed when the user asks for help.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Gets the text for the actual command.
        /// </summary>
        string Command
        {
            get;
        }

        /// <summary>
        /// Content to be placed in a toolbar button or null if should not be placed on a toolbar.
        /// </summary>
        object ButtonContent
        {
            get;
        }
    }
}

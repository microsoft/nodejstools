// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Creates a REPL window.  Implementations should check replId and ensure that it is a REPL window that they requested to be created.  The
    /// replId which will be provided is the same as the ID passed to IReplWindowProvider.CreateReplWindow.  You can receive an ID which has
    /// not been created during the current Visual Studio session if the user exited Visual Studio with the REPL window opened and docked.  Therefore
    /// the replId should contain enough information to re-create the appropriate REPL window.
    /// </summary>
    public interface IReplEvaluatorProvider
    {
        IReplEvaluator GetEvaluator(string replId);
    }
}

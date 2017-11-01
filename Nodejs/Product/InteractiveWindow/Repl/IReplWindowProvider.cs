// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Provides access to creating or finding existing REPL windows.   
    /// </summary>
    public interface IReplWindowProvider
    {
        /// <summary>
        /// Creates a REPL window and returns a ToolWindowPane which implements IReplWindow.  An IReplEvaluatorProvider must exist
        /// to respond and create the specified REPL ID.
        /// 
        /// The returned object is also a ToolWindowPane and can be cast for access to control the docking with VS.
        /// </summary>
        IReplWindow CreateReplWindow(IContentType contentType, string title, Guid languageServiceGuid, string replId);

        /// <summary>
        /// Finds the REPL w/ the specified ID or returns null if the window hasn't been created.  An IReplEvaluatorProvider must exist
        /// to respond and create the specified REPL ID.
        /// 
        /// The returned object is also a ToolWindowPane and can be cast for access to control the docking with VS.
        /// </summary>
        IReplWindow FindReplWindow(string replId);

        /// <summary>
        /// Returns this list of repl windows currently loaded.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IReplWindow> GetReplWindows();
    }
}

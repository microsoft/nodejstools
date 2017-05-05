// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Supports a REPL evaluator which enables the user to switch between
    /// multiple scopes of execution.
    /// </summary>
    public interface IMultipleScopeEvaluator : IReplEvaluator
    {
        /// <summary>
        /// Sets the current scope to the given name.
        /// </summary>
        void SetScope(string scopeName);

        /// <summary>
        /// Gets the list of scopes which can be changed to.
        /// </summary>
        IEnumerable<string> GetAvailableScopes();

        /// <summary>
        /// Gets the current scope name.
        /// </summary>
        string CurrentScopeName
        {
            get;
        }

        /// <summary>
        /// Event is fired when the list of available scopes changes.
        /// </summary>
        event EventHandler<EventArgs> AvailableScopesChanged;

        /// <summary>
        /// Event is fired when support of multiple scopes has changed.
        /// </summary>
        event EventHandler<EventArgs> MultipleScopeSupportChanged;

        /// <summary>
        /// Returns true if multiple scope support is currently enabled, false if not.
        /// </summary>
        bool EnableMultipleScopes
        {
            get;
        }
    }
}


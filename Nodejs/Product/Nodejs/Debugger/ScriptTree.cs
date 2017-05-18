// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Stores all of the scripts which are loaded in the debuggee in reverse order based
    /// upon their file components.  The first entry is the filename, then the parent directory,
    /// then parent of that directory, etc...
    /// This is used to do fuzzy filename matching when a breakpoint is hit.
    /// </summary>
    internal sealed class ScriptTree
    {
        public readonly HashSet<string> Children = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly string Filename;
        public readonly Dictionary<string, ScriptTree> Parents = new Dictionary<string, ScriptTree>(StringComparer.OrdinalIgnoreCase);

        public ScriptTree(string filename)
        {
            this.Filename = filename;
        }
    }
}

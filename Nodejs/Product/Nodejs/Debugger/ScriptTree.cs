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

using System;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Stores all of the scripts which are loaded in the debuggee in reverse order based
    /// upon their file components.  The first entry is the filename, then the parent directory,
    /// then parent of that directory, etc...
    /// This is used to do fuzzy filename matching when a breakpoint is hit.
    /// </summary>
    sealed class ScriptTree {
        public readonly List<string> Children = new List<string>();
        public readonly string Filename;
        public readonly Dictionary<string, ScriptTree> Parents = new Dictionary<string, ScriptTree>(StringComparer.OrdinalIgnoreCase);

        public ScriptTree(string filename) {
            Filename = filename;
        }
    }
}
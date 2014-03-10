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
using System.Linq;

namespace Microsoft.NodejsTools.Debugger {
    sealed class FuzzyLogicBreakpointHandler : BreakpointHandler {
        private readonly ScriptTree _scriptTree = new ScriptTree(null);

        public override void AddModuleName(string fileName) {
            ScriptTree curTree = _scriptTree;
            IEnumerable<string> pathComponents = GetPathComponents(fileName);
            foreach (string component in pathComponents.Reverse()) {
                ScriptTree nextTree;
                if (!curTree.Parents.TryGetValue(component, out nextTree)) {
                    curTree.Parents[component] = nextTree = new ScriptTree(component);
                }

                curTree.Children.Add(fileName);
                curTree = nextTree;
            }
        }

        private static IEnumerable<string> GetPathComponents(string path) {
            return path.Split('\\', '/', ':');
        }

        protected override bool MatchFileName(string remoteFileName, string localFileName) {
            // when we bind breakpoints we can end up setting breakpoints on index.js
            // and then hit the breakpoint and we need to make sure the paths are
            // actually the same.  We do this based upon all of the scripts which are
            // loaded into the process, walking up the path until we have a trailing
            // path that matches a single script.  If we can't get to just a single
            // match we'll break in all of the files.
            IEnumerable<string> pathComponents = GetPathComponents(localFileName);
            ScriptTree curTree = _scriptTree;
            foreach (string component in pathComponents.Reverse()) {
                ScriptTree nextTree;
                if (!curTree.Parents.TryGetValue(component, out nextTree)) {
                    // we know nothing about this script, it must not be loaded yet
                    return false;
                }

                if (nextTree.Children.Count == 1) {
                    // we map to a single script, see if it's where we broke.
                    return String.Equals(
                        nextTree.Children[0],
                        remoteFileName,
                        StringComparison.OrdinalIgnoreCase);
                }

                curTree = nextTree;
            }

            return true;
        }
    }
}
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
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Handles file name mapping while remote debugging.
    /// </summary>
    sealed class FuzzyLogicFileNameMapper : IFileNameMapper {
        private readonly ScriptTree _loadedScripts = new ScriptTree(null);
        private readonly ScriptTree _projectFiles = new ScriptTree(null);

        public FuzzyLogicFileNameMapper() {
            foreach (string fileName in EnumerateSolutionFiles()) {
                AddModuleToTree(_projectFiles, fileName);
            }
        }

        public void AddModuleName(string fileName) {
            AddModuleToTree(_loadedScripts, fileName);
        }

        public bool MatchFileName(string remoteFileName, string localFileName) {
            return FindModuleInTree(_loadedScripts, remoteFileName, localFileName);
        }

        public string GetLocalFileName(string remoteFileName) {
            return FindModuleMatchInTree(_projectFiles, remoteFileName);
        }

        private void AddModuleToTree(ScriptTree scriptTree, string fileName) {
            ScriptTree curTree = scriptTree;
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

        private bool FindModuleInTree(ScriptTree scriptTree, string remoteFileName, string localFileName) {
            // when we bind breakpoints we can end up setting breakpoints on index.js
            // and then hit the breakpoint and we need to make sure the paths are
            // actually the same.  We do this based upon all of the scripts which are
            // loaded into the process, walking up the path until we have a trailing
            // path that matches a single script.  If we can't get to just a single
            // match we'll break in all of the files.
            IEnumerable<string> pathComponents = GetPathComponents(remoteFileName);
            ScriptTree curTree = scriptTree;
            foreach (string component in pathComponents.Reverse()) {
                ScriptTree nextTree;
                if (!curTree.Parents.TryGetValue(component, out nextTree)) {
                    // we know nothing about this script, it must not be loaded yet
                    return false;
                }

                if (nextTree.Children.Count == 1) {
                    // we map to a single script, see if it's where we broke.
                    return nextTree.Children.Contains(localFileName);
                }

                curTree = nextTree;
            }

            return true;
        }

        private string FindModuleMatchInTree(ScriptTree scriptTree, string remoteFileName) {
            IEnumerable<string> pathComponents = GetPathComponents(remoteFileName);
            ScriptTree curTree = scriptTree;
            foreach (string component in pathComponents.Reverse()) {
                ScriptTree nextTree;
                if (!curTree.Parents.TryGetValue(component, out nextTree)) {
                    return remoteFileName;
                }

                if (nextTree.Children.Count == 1) {
                    return nextTree.Children.First();
                }

                curTree = nextTree;
            }

            return remoteFileName;
        }

        private IEnumerable<string> EnumerateSolutionFiles() {
            var solution = Package.GetGlobalService(typeof (SVsSolution)) as IVsSolution;
            if (solution != null) {
                foreach (IVsProject project in solution.EnumerateLoadedProjects(false)) {
                    foreach (uint itemid in project.EnumerateProjectItems()) {
                        string moniker;
                        if (ErrorHandler.Succeeded(project.GetMkDocument(itemid, out moniker)) && moniker != null) {
                            yield return moniker;
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> GetPathComponents(string path) {
            return path.Split('\\', '/', ':');
        }
    }
}
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Maintains the list of modules loaded into the JsAnalyzer.
    /// 
    /// This keeps track of the builtin modules as well as the user defined modules.  It's wraps
    /// up various elements we need to keep track of such as thread safety and lazy loading of built-in
    /// modules.
    /// </summary>
    [Serializable]
    class ModuleTable {
        private readonly Dictionary<string, ModuleTree> _modulesByFilename = new Dictionary<string, ModuleTree>(StringComparer.OrdinalIgnoreCase);
        private readonly ModuleTree _modules = new ModuleTree(null, String.Empty);
        private List<ProjectEntry> _builtins;
        private readonly object _lock = new object();
        [ThreadStatic]
        private static HashSet<ModuleRecursion> _recursionCheck;

        public bool TryGetValue(string name, out ModuleTree moduleTree) {
            lock (_lock) {
                return _modulesByFilename.TryGetValue(name, out moduleTree);
            }
        }

        public IEnumerable<ProjectEntry> Modules {
            get {
                List<ProjectEntry> res = new List<ProjectEntry>();
                lock (_lock) {
                    EnumerateChildren(res, _modules);
                }
                return res;
            }
        }

        public IEnumerable<ProjectEntry> BuiltinModules {
            get {
                return _builtins;
            }
        }

        private static void EnumerateChildren(List<ProjectEntry> res, ModuleTree cur) {
            if (cur.ProjectEntry != null) {
                res.Add(cur.ProjectEntry);
            }
            foreach (var child in cur.Children.Values) {
                EnumerateChildren(res, child);
            }
        }

        public ModuleTree GetModuleTree(string name) {
            lock (_lock) {
                var curTree = _modules;
                foreach (var comp in GetPathComponents(name)) {
                    ModuleTree nextTree;
                    if (!curTree.Children.TryGetValue(comp, out nextTree)) {
                        curTree.Children[comp] = nextTree = new ModuleTree(curTree, comp);
                    }

                    curTree = nextTree;
                }

                _modulesByFilename[name] = curTree;
                return curTree;
            }
        }

        internal ModuleTree Remove(string filename) {
            lock (_lock) {
                var curTree = _modules;
                _modulesByFilename.Remove(filename);
                foreach (var comp in GetPathComponents(filename)) {
                    ModuleTree nextTree;
                    if (!curTree.Children.TryGetValue(comp, out nextTree)) {
                        return null;
                    }

                    curTree = nextTree;
                }
                if (curTree.Parent != null) {
                    curTree.Parent.Children.Remove(Path.GetFileName(filename));
                    return curTree.Parent;
                }
            }
            return null;
        }

        public void AddModule(string name, ProjectEntry value) {
            lock (_lock) {
                var tree = GetModuleTree(name);
                tree.ProjectEntry = value;

                // Update visibility..
                AddVisibility(tree, value, true);

                value._enqueueModuleDependencies = true;

                if (value.IsBuiltin) {
                    //We found a new builtin, replace our current tree;
                    if(_builtins == null){
                        _builtins = new List<ProjectEntry>();
                    }
                    _builtins.Add(value);
                }
            }
        }

        // Visibility rules:
        // My peers can see my assignments/I can see my peers assignments 
        //      Everything up to the next node_modules and terminating at child node_modules
        //      folders is a set of peers.  They can easily require each other using relative
        //      paths.  We make all of the assignments made by these modules available to
        //      see by all the other modules.
        //
        // My parent and its peers can see my assignments
        //      A folder which contains node_modules is presumably using those modules.  ANy
        //      peers within that folder structure can see all of the changes by the children
        //      in node_modules.
        //
        // We share hashsets of visible nodes.  They're stored in the ModuleTree and when a
        // new module is added we assign it's _visibleEntries field to the one shared by all
        // of it's peers.  We then update the relevant entries with the new values.
        private void AddVisibility(ModuleTree tree, ProjectEntry newModule, bool recurse) {
            // My peers can see my assignments/I can see my peers assignments.  Update
            // ourselves and our peers so we can see each others writes.
            var curTree = tree;
            while (curTree.Parent != null && curTree.Parent.Name != AnalysisConstants.NodeModulesFolder) {
                curTree = curTree.Parent;
            }

            if (curTree.VisibleEntries == null) {
                curTree.VisibleEntries = new HashSet<ProjectEntry>();
                curTree.VisibleEntries.Add(newModule.Analyzer._builtinEntry);
            }
            curTree.VisibleEntries.Add(newModule);
            newModule._visibleEntries = curTree.VisibleEntries;

            // My parent and its peers can see my assignments.  Update existing parents
            // so they can see the newly added modules writes.
            if (curTree.Parent != null) {
                Debug.Assert(curTree.Parent.Name == AnalysisConstants.NodeModulesFolder);

                var grandParent = curTree.Parent.Parent;
                if (grandParent != null) {
                    while (grandParent.Parent != null && !String.Equals(grandParent.Parent.Name, AnalysisConstants.NodeModulesFolder, StringComparison.OrdinalIgnoreCase)) {
                        grandParent = grandParent.Parent;
                    }
                    if (grandParent.VisibleEntries == null) {
                        grandParent.VisibleEntries = new HashSet<ProjectEntry>();
                    }
                    grandParent.VisibleEntries.Add(newModule);
                }
            }
        }

        /// <summary>
        /// Attempts to resolve the required module when required from the declaring module.
        /// 
        /// If declModule is null then only global imports will be resolved.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="unit"></param>
        /// <param name="moduleName">filename of the file user has required</param>
        /// <param name="declModule">String to the fullpath of the filename we are currently analyzing</param>
        /// <returns></returns>
        public IAnalysisSet RequireModule(Node node, AnalysisUnit unit, string moduleName, string declModule = null) {
            lock (_lock) {
                ModuleTree moduleTree;

                if (TryGetValue(moduleName.Replace('/', '\\'), out moduleTree)) {
                    // exact filename match or built-in module
                    return GetExports(node, unit, moduleTree);
                }

                if (declModule != null && TryGetValue(declModule, out moduleTree)) {
                    return RequireModule(node, unit, moduleName, moduleTree.Parent);
                }

                return AnalysisSet.Empty;
            }
        }

        private IAnalysisSet RequireModule(Node node, AnalysisUnit unit, string moduleName, ModuleTree relativeTo) {
            if (moduleName.StartsWith("./") || moduleName.StartsWith("../")) {
                // search relative to our declaring module.
                return GetExports(
                    node,
                    unit,
                    ResolveModule(relativeTo, moduleName, unit)
                );
            } else {
                // must be in node_modules, search in the current directory
                // and up through our parents                
                do {
                    var nodeModules = relativeTo.GetChild(AnalysisConstants.NodeModulesFolder, unit);
                    var curTree = ResolveModule(nodeModules, moduleName, unit);

                    if (curTree != null) {
                        return GetExports(node, unit, curTree);
                    }

                    relativeTo = relativeTo.Parent;
                } while (relativeTo != null);
            }
            return AnalysisSet.Empty;
        }

        public static ModuleTree ResolveModule(ModuleTree parentTree, string relativeName) {
            return ResolveModule(parentTree, relativeName, null);
        }

        private static ModuleTree ResolveModule(ModuleTree parentTree, string relativeName, AnalysisUnit unit) {
            ModuleTree curTree = parentTree;
            var components = ModuleTable.GetPathComponents(relativeName);
            for (int i = 0; i < components.Length; i++) {
                var comp = components[i];

                if (curTree == null) {
                    return null;
                }

                if (comp == "." || comp == "") {
                    continue;
                } else if (comp == "..") {
                    curTree = curTree.Parent;
                    continue;
                }

                ModuleTree nextTree;
                if (i == components.Length - 1) {

                    nextTree = curTree.GetChild(comp + ".js", unit);

                    if (nextTree.ProjectEntry != null) {
                        return nextTree;
                    }
                }

                nextTree = curTree.GetChild(comp, unit);

                if (nextTree.Children.Count > 0 || nextTree.ProjectEntry != null) {
                    curTree = nextTree;
                    continue;
                }

                return null;
            }

            return curTree;
        }

        /// <summary>
        /// Gets the exports object from the module, or if we currently point to 
        /// a folder resolves to the default package.json.
        /// </summary>
        private IAnalysisSet GetExports(Node node, AnalysisUnit unit, ModuleTree curTree) {
            if (curTree != null) {
                if (curTree.ProjectEntry != null) {
                    var module = curTree.ProjectEntry.GetModule(unit);
                    if (module != null) {
                        return module.Get(
                            node,
                            unit,
                            "exports"
                        );
                    }
                } else if (curTree.Parent != null) {
                    // No ModuleReference, this is a folder, check and see
                    // if we have the default package file (either index.js
                    // or the file specified in package.json)

                    // we need to check for infinite recursion
                    // if someone setup two package.json's which
                    // point the main file at each other.
                    if (_recursionCheck == null) {
                        _recursionCheck = new HashSet<ModuleRecursion>();
                    }

                    var recCheck = new ModuleRecursion(curTree.DefaultPackage, curTree);
                    if (_recursionCheck.Add(recCheck)) {
                        try {
                            return RequireModule(
                                node,
                                unit,
                                curTree.DefaultPackage,
                                curTree
                            );
                        } finally {
                            _recursionCheck.Remove(recCheck);
                        }
                    }
                }
            }
            return AnalysisSet.Empty;
        }

        class ModuleRecursion : IEquatable<ModuleRecursion> {
            public readonly string Name;
            public readonly ModuleTree Module;

            public ModuleRecursion(string name, ModuleTree module) {
                Name = name;
                Module = module;
            }

            public override int GetHashCode() {
                return Name.GetHashCode() ^ Module.GetHashCode();
            }

            public override bool Equals(object obj) {
                var other = obj as ModuleRecursion;
                if (other == null) {
                    return false;
                }

                return Equals(other);
            }

            public bool Equals(ModuleRecursion other) {
                return other.Module == Module &&
                    other.Name == Name;
            }
        }

        internal static string[] GetPathComponents(string path) {
            return path.Split(PathSplitter);
        }

        private static char[] PathSplitter = new[] { '\\', '/', ':' };
    }

    [Serializable]
    sealed class ModuleTree {
        public readonly ModuleTree Parent;
        public readonly string Name;
        public readonly Dictionary<string, ModuleTree> Children = new Dictionary<string, ModuleTree>(StringComparer.OrdinalIgnoreCase);
        private ProjectEntry _projectEntry;
        public HashSet<ProjectEntry> VisibleEntries;
        private string _defaultPackage = "./index.js";
        DependentData _dependencies = new DependentData();

        public string DefaultPackage {
            get {
                return _defaultPackage;
            }
            set {
                _defaultPackage = value;
            }
        }

        /// <summary>
        /// Enqueues dependents which have required this tree.  This method must be called
        /// on the analysis thread which means any mutations to the tree need to propagate
        /// out an IAnalyzable or capture their updates in an existing analysis.
        /// </summary>
        public void EnqueueDependents() {
            var curTree = this;
            while (curTree != null) {
                curTree._dependencies.EnqueueDependents();
                curTree = curTree.Parent;
            }
        }

        public ProjectEntry ProjectEntry {
            get {
                return _projectEntry;
            }
            set {
                _projectEntry = value;
            }
        }

        public ModuleTree(ModuleTree parent, string name) {
            Parent = parent;
            Name = name;
        }

        public ModuleTree GetChild(string name, AnalysisUnit unit) {
            ModuleTree tree;
            if (!Children.TryGetValue(name, out tree)) {
                Children[name] = tree = new ModuleTree(this, name);
            }
            if (unit != null) {
                tree.AddDependency(unit);
            }
            return tree;
        }

#if DEBUG
        public string Path {
            get {
                StringBuilder res = new StringBuilder();
                AppendPath(res, this);
                return res.ToString();
            }
        }

        private static void AppendPath(StringBuilder res, ModuleTree moduleTree) {
            if (moduleTree.Parent != null) {
                AppendPath(res, moduleTree.Parent);
            }
            if (!String.IsNullOrEmpty(moduleTree.Name)) {
                res.Append(moduleTree.Name);
                res.Append('\\');
            }
        }
#endif

        internal void AddDependency(AnalysisUnit unit) {
            _dependencies.AddDependency(unit);
        }
    }
}

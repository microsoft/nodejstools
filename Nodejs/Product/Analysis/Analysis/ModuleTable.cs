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
        private readonly JsAnalyzer _analyzer;
        private readonly Dictionary<string, ModuleTree> _modulesByFilename = new Dictionary<string, ModuleTree>(StringComparer.OrdinalIgnoreCase);
        private readonly ModuleTree _modules = new ModuleTree(null, "");
        private readonly object _lock = new object();
        [ThreadStatic]
        private static HashSet<ModuleRecursion> _recursionCheck;

        public ModuleTable(JsAnalyzer analyzer) {
            _analyzer = analyzer;
        }

        public bool TryGetValue(string name, out ModuleTree moduleTree) {
            lock (_lock) {
                return _modulesByFilename.TryGetValue(name, out moduleTree);
            }
        }

        public IEnumerable<IJsProjectEntry> Modules {
            get {
                List<IJsProjectEntry> res = new List<IJsProjectEntry>();
                lock (_lock) {
                    EnumerateChildren(res, _modules);
                }
                return res;
            }
        }

        private static void EnumerateChildren(List<IJsProjectEntry> res, ModuleTree cur) {
            if (cur.Module != null) {
                res.Add(cur.Module.ProjectEntry);
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
                GetModuleTree(name).Module = value.ModuleValue;
                value._enqueueModuleDependencies = true;
            }
        }

        /// <summary>
        /// Attempts to resolve the required module when required from the declaring module.
        /// </summary>
        public IAnalysisSet RequireModule(Node node, AnalysisUnit unit, string moduleName, string declModule) {
            lock (_lock) {
                ModuleTree moduleTree;
                if (TryGetValue(moduleName, out moduleTree)) {
                    // exact filename match or built-in module
                    return GetExports(node, unit, moduleTree);
                }

                if (TryGetValue(declModule, out moduleTree)) {
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
                    ResolveModule(relativeTo, unit, moduleName)
                );
            } else {
                // must be in node_modules, search in the current directory
                // and up through our parents                
                do {
                    var nodeModules = relativeTo.GetChild("node_modules", unit);
                    var curTree = ResolveModule(nodeModules, unit, moduleName);

                    if (curTree != null) {
                        return GetExports(node, unit, curTree);
                    }

                    relativeTo = relativeTo.Parent;
                } while (relativeTo != null);
            }
            return AnalysisSet.Empty;
        }

        private static ModuleTree ResolveModule(ModuleTree parentTree, AnalysisUnit unit, string relativeName) {
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
                    if (nextTree.Module != null) {
                        return nextTree;
                    }
                }

                nextTree = curTree.GetChild(comp, unit);
                if (nextTree.Children.Count > 0 || nextTree.Module != null) {
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
                if (curTree.Module != null) {
                    var moduleScope = curTree.Module.EnvironmentRecord;
                    return moduleScope.Module.Get(
                        node,
                        unit,
                        "exports"
                    );
                } else if(curTree.Parent != null) {
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
        private ModuleValue _module;
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

        public ModuleValue Module {
            get {
                return _module;
            }
            set {
                _module = value;
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
            tree.AddDependency(unit);
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

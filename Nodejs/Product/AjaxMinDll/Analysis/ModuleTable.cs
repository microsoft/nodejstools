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
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Maintains the list of modules loaded into the JsAnalyzer.
    /// 
    /// This keeps track of the builtin modules as well as the user defined modules.  It's wraps
    /// up various elements we need to keep track of such as thread safety and lazy loading of built-in
    /// modules.
    /// </summary>
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

                if (curTree.ModuleReference == null) {
                    curTree.ModuleReference = new ModuleReference();
                }

                _modulesByFilename[name] = curTree;
                return curTree;
            }
        }

        internal bool Remove(string filename) {
            lock (_lock) {
                var curTree = _modules;
                foreach (var comp in GetPathComponents(filename)) {
                    ModuleTree nextTree;
                    if (!curTree.Children.TryGetValue(comp, out nextTree)) {
                        return false;
                    }

                    curTree = nextTree;
                }
                if (curTree.Parent != null) {
                    return curTree.Parent.Children.Remove(Path.GetFileName(filename));
                }
            }
            return false;
        }

        public ModuleReference GetOrAdd(string name) {
            return GetModuleTree(name).ModuleReference;
        }

        /// <summary>
        /// Attempts to resolve the required module when required from the declaring module.
        /// </summary>
        public IAnalysisSet RequireModule(Node node, AnalysisUnit unit, string moduleName, string declModule) {
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

        private IAnalysisSet RequireModule(Node node, AnalysisUnit unit, string moduleName, ModuleTree relativeTo) {
            // search relative to our declaring module.
            if (moduleName.StartsWith("./")) {
                string relativeName = moduleName.Substring(2);

                return GetExports(
                    node, 
                    unit, 
                    ResolveModule(relativeTo, relativeName)
                );
            } else if (moduleName.StartsWith("../")) {
                string relativeName = moduleName.Substring(3);

                return GetExports(
                    node,
                    unit,
                    ResolveModule(relativeTo.Parent, relativeName)
                );
            } else {
                // must be in node_modules, search in the current directory
                // and up through our parents
                ModuleTree nodeModules;
                do {
                    if (relativeTo.Children.TryGetValue("node_modules", out nodeModules)) {
                        var curTree = ResolveModule(nodeModules, moduleName);

                        if (curTree != null) {
                            return GetExports(node, unit, curTree);
                        }
                    }

                    relativeTo = relativeTo.Parent;
                } while (relativeTo != null);
            }
            return AnalysisSet.Empty;
        }

        private static ModuleTree ResolveModule(ModuleTree parentTree, string relativeName) {
            ModuleTree curTree = parentTree;
            foreach (var comp in ModuleTable.GetPathComponents(relativeName)) {
                if (comp == ".") {
                    continue;
                } else if (comp == "..") {
                    curTree = curTree.Parent;
                    continue;
                }

                ModuleTree nextTree;
                if (!curTree.Children.TryGetValue(comp, out nextTree) &&
                    !curTree.Children.TryGetValue(comp + ".js", out nextTree)) {
                    return null;
                }

                curTree = nextTree;
            }
            return curTree;
        }

        /// <summary>
        /// Gets the exports object from the module, or if we currently point to 
        /// a folder resolves to the default package.json.
        /// </summary>
        private IAnalysisSet GetExports(Node node, AnalysisUnit unit, ModuleTree curTree) {
            if (curTree != null) {
                if (curTree.ModuleReference != null &&
                    curTree.ModuleReference.Module != null) {
                    var moduleScope = curTree.ModuleReference.Module.Scope;
                    return moduleScope.CreateVariable(node, unit, "exports").Types;
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

        internal static IEnumerable<string> GetPathComponents(string path) {
            return path.Split(PathSplitter, StringSplitOptions.RemoveEmptyEntries);
        }

        private static char[] PathSplitter = new[] { '\\', '/', ':' };

#if FALSE
        internal BuiltinModule GetBuiltinModule(IPythonModule attr) {
            if (attr == null) {
                return null;
            }
            BuiltinModule res;
            if (!_builtinModuleTable.TryGetValue(attr, out res)) {
                _builtinModuleTable[attr] = res = new BuiltinModule(attr, _analyzer);
            }
            return res;
        }
#endif
#if FALSE

        #region IEnumerable<KeyValuePair<string,ModuleReference>> Members

        public IEnumerator<KeyValuePair<string, ModuleLoadState>> GetEnumerator() {
            //var unloadedNames = new HashSet<string>(_interpreter.GetModuleNames(), StringComparer.Ordinal);
            var unresolvedNames = _analyzer.GetAllUnresolvedModuleNames();

            foreach (var keyValue in _modules) {
                //unloadedNames.Remove(keyValue.Key);
                unresolvedNames.Remove(keyValue.Key);
                yield return new KeyValuePair<string, ModuleLoadState>(keyValue.Key, new InitializedModuleLoadState(keyValue.Value));
            }

#if FALSE
            foreach (var name in unloadedNames) {
                yield return new KeyValuePair<string, ModuleLoadState>(name, new UninitializedModuleLoadState(this, name));
            }
#endif

            foreach (var name in unresolvedNames) {
                yield return new KeyValuePair<string, ModuleLoadState>(name, new UnresolvedModuleLoadState());
            }
        }

        class UnresolvedModuleLoadState : ModuleLoadState {
            public override AnalysisValue Module {
                get { return null; }
            }

            public override bool HasModule {
                get { return false; }
            }

            public override bool HasReferences {
                get { return false; }
            }

            public override bool IsValid {
                get { return true; }
            }

            public override PythonMemberType MemberType {
                get { return PythonMemberType.Unknown; }
            }

            internal override bool ModuleContainsMember(string name) {
                return false;
            }
        }


        class UninitializedModuleLoadState : ModuleLoadState {
            private readonly ModuleTable _moduleTable;
            private readonly string _name;
            //private PythonMemberType? _type;

            public UninitializedModuleLoadState(ModuleTable moduleTable, string name) {
                this._moduleTable = moduleTable;
                this._name = name;
            }

            public override AnalysisValue Module {
                get {
                    ModuleReference res;
                    if (_moduleTable.TryGetValue(_name, out res)) {
                        return res.AnalysisModule;
                    }
                    return null;
                }
            }

            public override bool IsValid {
                get {
                    return true;
                }
            }

            public override bool HasReferences {
                get {
                    return false;
                }
            }

            public override bool HasModule {
                get {
                    return true;
                }
            }

            public override PythonMemberType MemberType {
                get {
#if FALSE
                    if (_type == null) {
                        var mod = _moduleTable._interpreter.ImportModule(_name);
                        if (mod != null) {
                            _type = mod.MemberType;
                        } else {
                            _type = PythonMemberType.Module;
                        }
                    }
#endif
                    return PythonMemberType.Module;
                }
            }

            internal override bool ModuleContainsMember(string name) {
#if FALSE
                var mod = _moduleTable._interpreter.ImportModule(_name);
                if (mod != null) {
                    return BuiltinModuleContainsMember(context, name, mod);
                }
#endif
                return false;
            }

        }

#if FALSE
        class InitializedModuleLoadState : ModuleLoadState {
            private readonly ModuleReference _reference;

            public InitializedModuleLoadState(ModuleReference reference) {
                _reference = reference;
            }

            public override AnalysisValue Module {
                get {
                    return _reference.AnalysisModule;
                }
            }

            public override bool HasReferences {
                get {
                    return _reference.HasReferences;
                }
            }

            public override bool IsValid {
                get {
                    return Module != null || HasReferences;
                }
            }

            public override bool HasModule {
                get {
                    return Module != null;
                }
            }

            public override PythonMemberType MemberType {
                get {
                    if (Module != null) {
                        return Module.MemberType;
                    }
                    return PythonMemberType.Module;
                }
            }

            internal override bool ModuleContainsMember(string name) {
#if FALSE
                BuiltinModule builtin = Module as BuiltinModule;
                if (builtin != null) {
                    return BuiltinModuleContainsMember(context, name, builtin.InterpreterModule);
                }
#endif

                ModuleInfo modInfo = Module as ModuleInfo;
                if (modInfo != null) {
                    VariableDef varDef;
                    if (modInfo.Scope.Variables.TryGetValue(name, out varDef) &&
                        varDef.VariableStillExists) {
                        var types = varDef.TypesNoCopy;
                        if (types.Count > 0) {
                            foreach (var type in types) {
                                if (type is ModuleInfo /*|| type is BuiltinModule*/) {
                                    // we find modules via our modules list, dont duplicate these
                                    return false;
                                }

                                foreach (var location in type.Locations) {
                                    if (location.ProjectEntry != modInfo.ProjectEntry) {
                                        // declared in another module
                                        return false;
                                    }
                                }
                            }
                        }

                        return true;
                    }
                }
                return false;
            }
        }
#endif
#if FALSE
        private static bool BuiltinModuleContainsMember(string name, IPythonModule interpModule) {
            var mem = interpModule.GetMember(context, name);
            if (mem != null) {
#if FALSE
                if (IsExcludedBuiltin(interpModule, mem)) {
                    // if a module imports a builtin and exposes it don't report it for purposes of adding imports
                    return false;
                }

                IPythonMultipleMembers multiMem = mem as IPythonMultipleMembers;
                if (multiMem != null) {
                    foreach (var innerMem in multiMem.Members) {
                        if (IsExcludedBuiltin(interpModule, innerMem)) {
                            // if something non-excludable aliased w/ something excluable we probably
                            // only care about the excluable (for example a module and None - timeit.py
                            // does this in the std lib)
                            return false;
                        }
                    }
                }
#endif
                return true;
            }
            return false;
        }
#endif

#if FALSE
        private static bool IsExcludedBuiltin(IPythonModule builtin, IMember mem) {
            if (mem is IPythonModule  // modules are handled specially
                ) {    // constant which we have no real type info for.
                return true;
            }

            return false;
        }
#endif

        #endregion
#endif
#if FALSE
        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
#endif

    }

    sealed class ModuleTree {
        public readonly ModuleTree Parent;
        public readonly string Name;
        public readonly Dictionary<string, ModuleTree> Children = new Dictionary<string, ModuleTree>(StringComparer.OrdinalIgnoreCase);
        public string DefaultPackage = "./index.js";
        public ModuleReference ModuleReference;

        public ModuleTree(ModuleTree parent, string name) {
            Parent = parent;
            Name = name;
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
    }

#if FALSE
    abstract class ModuleLoadState {
        public abstract AnalysisValue Module {
            get;
        }

        public abstract bool HasModule {
            get;
        }

        public abstract bool HasReferences {
            get;
        }

        public abstract bool IsValid {
            get;
        }

        public abstract PythonMemberType MemberType {
            get;
        }

        internal abstract bool ModuleContainsMember(string name);
    }
#endif
}

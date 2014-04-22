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
using System.Diagnostics;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
    internal class ModuleInfo : AnalysisValue, IReferenceableContainer/*, IModule */{
        private readonly string _name;
        private readonly ProjectEntry _projectEntry;
        private Dictionary<Node, IAnalysisSet> _sequences;  // sequences defined in the module
        private readonly ModuleScope _scope;
        private readonly Dictionary<Node, EnvironmentRecord> _scopes;    // scopes from Ast node to InterpreterScope
        private readonly WeakReference _weakModule;
        private Dictionary<string, WeakReference> _packageModules;
#if FALSE
        private Dictionary<string, Tuple<CallDelegate, bool>> _specialized;
#endif
        private ModuleInfo _parentPackage;
        private DependentData _definition = new DependentData();
        private readonly HashSet<ModuleReference> _referencedModules;
        private readonly HashSet<String> _unresolvedModules;

        public ModuleInfo(string moduleName, ProjectEntry projectEntry) {
            _name = moduleName;
            _projectEntry = projectEntry;
            _sequences = new Dictionary<Node, IAnalysisSet>();
            _scope = new ModuleScope(this);
            _weakModule = new WeakReference(this);
            _scopes = new Dictionary<Node, EnvironmentRecord>();
            _referencedModules = new HashSet<ModuleReference>();
            _unresolvedModules = new HashSet<string>(StringComparer.Ordinal);
        }

        internal void Clear() {
            _sequences.Clear();
            _scope.ClearLinkedVariables();
            _scope.ClearVariables();
            _scope.ClearNodeScopes();
            _referencedModules.Clear();
            _unresolvedModules.Clear();
        }

        /// <summary>
        /// Returns all the absolute module names that need to be resolved from
        /// this module.
        /// 
        /// Note that a single import statement may add multiple names to this
        /// set, and so the Count property does not accurately reflect the 
        /// actual number of imports required.
        /// </summary>
        internal ISet<string> GetAllUnresolvedModules() {
            return _unresolvedModules;
        }

#if FALSE
        internal void AddUnresolvedModule(string relativeModuleName, bool absoluteImports) {
            _unresolvedModules.UnionWith(JsAnalyzer.ResolvePotentialModuleNames(_projectEntry, relativeModuleName, absoluteImports));
            _projectEntry.Analyzer.ModuleHasUnresolvedImports(this, true);
        }

        internal void ClearUnresolvedModules() {
            _unresolvedModules.Clear();
            _projectEntry.Analyzer.ModuleHasUnresolvedImports(this, false);
        }
#endif

        public override Dictionary<string, IAnalysisSet> GetAllMembers() {
            var res = new Dictionary<string, IAnalysisSet>();
            foreach (var kvp in _scope.Variables) {
                kvp.Value.ClearOldValues();
                if (kvp.Value._dependencies.Count > 0) {
                    var types = kvp.Value.Types;
                    if (types.Count > 0) {
                        res[kvp.Key] = types;
                    }
                }
            }
            return res;
        }

        public ModuleInfo ParentPackage {
            get { return _parentPackage; }
            set { _parentPackage = value; }
        }

#if FALSE
        public IEnumerable<KeyValuePair<string, AnalysisValue>> GetChildrenPackages() {
            if (_packageModules != null) {
                foreach (var keyValue in _packageModules) {
                    var res = keyValue.Value.Target as IModule;
                    if (res != null) {
                        yield return new KeyValuePair<string, AnalysisValue>(keyValue.Key, (AnalysisValue)res);
                    }
                }
            }
        }


        public IModule GetChildPackage(string name) {
            WeakReference weakMod;
            if (_packageModules != null && _packageModules.TryGetValue(name, out weakMod)) {
                var res = weakMod.Target;
                if (res != null) {
                    return (IModule)res;
                }

                _packageModules.Remove(name);
            }
            return null;
        }
#endif
        public void AddModuleReference(ModuleReference moduleRef) {
            if (moduleRef == null) {
                Debug.Fail("moduleRef should never be null");
                throw new ArgumentNullException("moduleRef");
            }
            _referencedModules.Add(moduleRef);
            moduleRef.AddReference(this);
        }

        public void RemoveModuleReference(ModuleReference moduleRef) {
            if (_referencedModules.Remove(moduleRef)) {
                moduleRef.RemoveReference(this);
            }
        }

        public IEnumerable<ModuleReference> ModuleReferences {
            get {
                return _referencedModules;
            }
        }

#if FALSE
        public void SpecializeFunction(string name, CallDelegate callable, bool mergeOriginalAnalysis) {
            lock (this) {
                if (_specialized == null) {
                    _specialized = new Dictionary<string, Tuple<CallDelegate, bool>>();
                }
                _specialized[name] = Tuple.Create(callable, mergeOriginalAnalysis);
            }
        }

        internal void Specialize() {
            lock (this) {
                if (_specialized != null) {
                    foreach (var keyValue in _specialized) {
                        SpecializeOneFunction(keyValue.Key, keyValue.Value.Item1, keyValue.Value.Item2);
                    }
                }
            }
        }

        private void SpecializeOneFunction(string name, CallDelegate callable, bool mergeOriginalAnalysis) {
            int lastIndex;
            VariableDef def;
            if (Scope.Variables.TryGetValue(name, out def)) {
                SpecializeVariableDef(def, callable, mergeOriginalAnalysis);
            } else if ((lastIndex = name.LastIndexOf('.')) != -1 &&
                Scope.Variables.TryGetValue(name.Substring(0, lastIndex), out def)) {
                var methodName = name.Substring(lastIndex + 1, name.Length - (lastIndex + 1));
#if FALSE
                foreach (var v in def.TypesNoCopy) {
                    ClassInfo ci = v as ClassInfo;
                    if (ci != null) {
                        VariableDef methodDef;
                        if (ci.Scope.Variables.TryGetValue(methodName, out methodDef)) {
                            SpecializeVariableDef(methodDef, callable, mergeOriginalAnalysis);
                        }
                    }
                }
#endif
            }
        }

        private static void SpecializeVariableDef(VariableDef def, CallDelegate callable, bool mergeOriginalAnalysis) {
            List<AnalysisValue> items = new List<AnalysisValue>();
            foreach (var v in def.TypesNoCopy) {
                if (!(v is SpecializedNamespace) && v.DeclaringModule != null) {
                    items.Add(v);
                }
            }

            def._dependencies = default(SingleDict<IProjectEntry, TypedDependencyInfo<AnalysisValue>>);
            foreach (var item in items) {
                def.AddTypes(item.DeclaringModule, new SpecializedCallable(item, callable, mergeOriginalAnalysis).SelfSet);
            }
        }
#endif

        public override IAnalysisSet GetMember(Node node, AnalysisUnit unit, string name) {
            // Must unconditionally call the base implementation of GetMember
            var ignored = base.GetMember(node, unit, name);

            ModuleDefinition.AddDependency(unit);

            return Scope.CreateEphemeralVariable(node, unit, name).Types;
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
            var variable = Scope.CreateVariable(node, unit, name, false);
            if (variable.AddTypes(unit, value)) {
                ModuleDefinition.EnqueueDependents();
            }

            variable.AddAssignment(node, unit);
        }

        /// <summary>
        /// Gets a weak reference to this module
        /// </summary>
        public WeakReference WeakModule {
            get {
                return _weakModule;
            }
        }

        public DependentData ModuleDefinition {
            get {
                return _definition;
            }
        }

        public ModuleScope Scope {
            get {
                return _scope;
            }
        }

        public override string Name {
            get { return _name; }
        }

        public ProjectEntry ProjectEntry {
            get { return _projectEntry; }
        }

        public override PythonMemberType MemberType {
            get {
                return PythonMemberType.Module;
            }
        }

        public override string ToString() {
            return "Module " + base.ToString();
        }

        public override string ShortDescription {
            get {
                return "Python module " + Name;
            }
        }

        public override string Description {
            get {
                var result = new StringBuilder("Python module ");
                result.Append(Name);
                var doc = Documentation;
                if (!string.IsNullOrEmpty(doc)) {
                    result.Append("\n\n");
                    result.Append(doc);
                }
                return result.ToString();
            }
        }

        public override string Documentation {
            get {
#if FALSE
                if (ProjectEntry.Tree != null && ProjectEntry.Tree.Body != null) {
                    return ProjectEntry.Tree.Block.Documentation.TrimDocumentation() ?? String.Empty;
                }
#endif
                return String.Empty;
            }
        }

        public override IEnumerable<LocationInfo> Locations {
            get {
                return new[] { new LocationInfo(ProjectEntry, 1, 1) };
            }
        }

        #region IVariableDefContainer Members

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            VariableDef def;
            if (_scope.Variables.TryGetValue(name, out def)) {
                yield return def;
            }
        }

        #endregion



        public IAnalysisSet GetModuleMember(Node node, AnalysisUnit unit, string name, bool addRef = true, EnvironmentRecord linkedScope = null, string linkedName = null) {
            var importedValue = Scope.CreateVariable(node, unit, name, addRef);
            ModuleDefinition.AddDependency(unit);

            if (linkedScope != null) {
                linkedScope.GetLinkedVariables(linkedName ?? name).Add(importedValue);
            }
            return importedValue.TypesNoCopy;
        }


        public IEnumerable<string> GetModuleMemberNames() {
            return Scope.Variables.Keys;
        }

        public void Imported(AnalysisUnit unit) {
            ModuleDefinition.AddDependency(unit);
        }
    }
}

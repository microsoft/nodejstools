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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Performs analysis of multiple JavaScript code files and enables interrogation of the resulting analysis.
    /// </summary>
    public partial class JsAnalyzer : IGroupableAnalysisProject {
        private readonly ModuleTable _modules;
        internal readonly ProjectEntry _builtinEntry;
        private readonly HashSet<ModuleValue> _modulesWithUnresolvedImports;
        private readonly object _modulesWithUnresolvedImportsLock = new object();
        private readonly Dictionary<object, AnalysisValue> _itemCache;
        internal readonly NullValue _nullInst;
        internal readonly BooleanValue _trueInst, _falseInst;
        internal readonly UndefinedValue _undefined;
        internal readonly AnalysisValue _globalObject;
        internal readonly AnalysisValue _numberPrototype, _stringPrototype, _booleanPrototype, _functionPrototype;
        private readonly Deque<AnalysisUnit> _queue;
        private Action<int> _reportQueueSize;
        private int _reportQueueInterval;
        internal readonly AnalysisUnit _evalUnit;   // a unit used for evaluating when we don't otherwise have a unit available
        private readonly HashSet<string> _analysisDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private AnalysisLimits _limits;
        private static object _nullKey = new object();

        private const string AnalysisLimitsKey = @"Software\Microsoft\NodejsTools\" + AssemblyVersionInfo.VSVersion +
            @"\Analysis\Project";

        public JsAnalyzer() {
            _modules = new ModuleTable(this);
            _modulesWithUnresolvedImports = new HashSet<ModuleValue>();
            _itemCache = new Dictionary<object, AnalysisValue>();
            _builtinEntry = new ProjectEntry(this, "", null);

            try {
                using (var key = Registry.CurrentUser.OpenSubKey(AnalysisLimitsKey)) {
                    Limits = AnalysisLimits.LoadFromStorage(key);
                }
            } catch (SecurityException) {
                Limits = new AnalysisLimits();
            } catch (UnauthorizedAccessException) {
                Limits = new AnalysisLimits();
            } catch (IOException) {
                Limits = new AnalysisLimits();
            }

            _queue = new Deque<AnalysisUnit>();

            _nullInst = new NullValue(this);
            _trueInst = new BooleanValue(true, this);
            _falseInst = new BooleanValue(false, this);
            _undefined = new UndefinedValue();

            var globals = GlobalBuilder.MakeGlobal(this);
            _globalObject = globals.GlobalObject;
            _numberPrototype = globals.NumberPrototype;
            _stringPrototype = globals.StringPrototype;
            _booleanPrototype = globals.BooleanPrototype;
            _functionPrototype = globals.FunctionPrototype;
            
            var allJson = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "all.json"
            );
            

            if (File.Exists(allJson)) {
                NodejsModuleBuilder.Build(allJson, this);
            }

            _evalUnit = new AnalysisUnit(null, null, new ModuleValue("$global", _builtinEntry).Scope, true);
            AnalysisLog.NewUnit(_evalUnit);
        }

        #region Public API

        /// <summary>
        /// Adds a new module of code to the list of available modules and returns a ProjectEntry object.
        /// 
        /// This method is thread safe.
        /// </summary>
        /// <param name="moduleName">The name of the module; used to associate with imports</param>
        /// <param name="filePath">The path to the file on disk</param>
        /// <param name="cookie">An application-specific identifier for the module</param>
        /// <returns></returns>
        public IJsProjectEntry AddModule(string filePath, IAnalysisCookie cookie = null) {
            var entry = new ProjectEntry(this, filePath, cookie);

            var moduleRef = Modules.GetOrAdd(filePath);
            moduleRef.Module = entry.ModuleValue;

            return entry;
        }

        public void AddPackageJson(string filePath, string entryPoint) {
            if (Path.GetFileName(filePath) != "package.json") {
                throw new InvalidOperationException("path must be to package.json file");
            }

            Modules.GetModuleTree(Path.GetDirectoryName(filePath)).DefaultPackage = entryPoint;
        }

        /// <summary>
        /// Removes the specified project entry from the current analysis.
        /// 
        /// This method is thread safe.
        /// </summary>
        public void RemoveModule(IProjectEntry entry) {
            if (entry == null) {
                throw new ArgumentNullException("entry");
            }
            Contract.EndContractBlock();

            var pyEntry = entry as IJsProjectEntry;
            if (pyEntry != null) {
                Modules.Remove(pyEntry.FilePath);
            }
            entry.RemovedFromProject();
        }

#if FALSE
        /// <summary>
        /// Returns a sequence of project entries that import the specified
        /// module. The sequence will be empty if the module is unknown.
        /// </summary>
        /// <param name="moduleName">
        /// The absolute name of the module. This should never end with
        /// '__init__'.
        /// </param>
        public IEnumerable<IPythonProjectEntry> GetEntriesThatImportModule(string moduleName, bool includeUnresolved) {
            ModuleReference modRef;
            var entries = new List<IPythonProjectEntry>();
            if (_modules.TryGetValue(moduleName, out modRef) && modRef.HasReferences) {
                entries.AddRange(modRef.References.Select(m => m.ProjectEntry).OfType<IPythonProjectEntry>());
            }

            if (includeUnresolved) {
                // Have to iterate over modules with unresolved imports to find
                // ephemeral references.
                lock (_modulesWithUnresolvedImportsLock) {
                    foreach (var module in _modulesWithUnresolvedImports) {
                        if (module.GetAllUnresolvedModules().Contains(moduleName)) {
                            entries.Add(module.ProjectEntry);
                        }
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// Returns a sequence of absolute module names that, if available,
        /// would resolve one or more unresolved references.
        /// </summary>
        internal ISet<string> GetAllUnresolvedModuleNames() {
            var set = new HashSet<string>(StringComparer.Ordinal);
            lock (_modulesWithUnresolvedImportsLock) {
                foreach (var module in _modulesWithUnresolvedImports) {
                    set.UnionWith(module.GetAllUnresolvedModules());
                }
            }
            return set;
        }

        internal void ModuleHasUnresolvedImports(ModuleInfo module, bool hasUnresolvedImports) {
            lock (_modulesWithUnresolvedImportsLock) {
                if (hasUnresolvedImports) {
                    _modulesWithUnresolvedImports.Add(module);
                } else {
                    _modulesWithUnresolvedImports.Remove(module);
                }
            }
        }
#endif

#if FALSE
        /// <summary>
        /// Returns true if a module has been imported.
        /// </summary>
        /// <param name="importFrom">
        /// The full name of the module doing the import.
        /// </param>
        /// <param name="relativeModuleName">
        /// The absolute or relative name of the module. If a relative name is 
        /// passed here, <paramref name="importFrom"/> must be provided.
        /// </param>
        /// <param name="absoluteImports">
        /// True if Python 3.x style imports should be used.
        /// </param>
        /// <returns>
        /// True if the module was imported during analysis; otherwise, false.
        /// </returns>
        public bool IsModuleResolved(IPythonProjectEntry importFrom, string relativeModuleName, bool absoluteImports) {
            ModuleReference moduleRef;
            return ResolvePotentialModuleNames(importFrom, relativeModuleName, absoluteImports)
                .Any(m =>
                    Modules.TryGetValue(m, out moduleRef) &&
                    moduleRef != null &&
                    moduleRef.Module != null
                );
        }

        /// <summary>
        /// Returns a sequence of candidate absolute module names for the given
        /// modules.
        /// </summary>
        /// <param name="projectEntry">
        /// The project entry that is importing the module.
        /// </param>
        /// <param name="relativeModuleName">
        /// A dotted name identifying the path to the module.
        /// </param>
        /// <returns>
        /// A sequence of strings representing the absolute names of the module
        /// in order of precedence.
        /// </returns>
        internal static IEnumerable<string> ResolvePotentialModuleNames(
            IPythonProjectEntry projectEntry,
            string relativeModuleName,
            bool absoluteImports
        ) {
            string importingFrom = null;
            if (projectEntry != null) {
                importingFrom = projectEntry.ModuleName;
            }

            if (string.IsNullOrEmpty(relativeModuleName)) {
                yield break;
            }

            // Handle relative module names
            if (relativeModuleName.FirstOrDefault() == '.') {
                if (string.IsNullOrEmpty(importingFrom)) {
                    // No source to import relative to.
                    yield break;
                }

                var prefix = importingFrom.Split('.');

                if (relativeModuleName.LastOrDefault() == '.') {
                    // Last part empty means the whole name is dots, so there's
                    // nothing to concatenate.
                    yield return string.Join(".", prefix.Take(prefix.Length - relativeModuleName.Length));
                } else {
                    var suffix = relativeModuleName.Split('.');
                    var dotCount = suffix.TakeWhile(bit => string.IsNullOrEmpty(bit)).Count();
                    if (dotCount < prefix.Length) {
                        // If we have as many dots as prefix parts, the entire
                        // name will disappear. Despite what PEP 328 says, in
                        // reality this means the import will fail.
                        yield return string.Join(".", prefix.Take(prefix.Length - dotCount).Concat(suffix.Skip(dotCount)));
                    }
                }
                yield break;
            }

            // The two possible names that can be imported here are:
            // * relativeModuleName
            // * importingFrom.relativeModuleName
            // and the order they are returned depends on whether
            // absolute_import is enabled or not.

            // With absolute_import, we treat the name as complete first.
            if (absoluteImports) {
                yield return relativeModuleName;
            }

            if (!string.IsNullOrEmpty(importingFrom)) {
                var prefix = importingFrom.Split('.');

                if (prefix.Length > 1) {
                    var adjacentModuleName = string.Join(".", prefix.Take(prefix.Length - 1)) + "." + relativeModuleName;
                    yield return adjacentModuleName;
                }
            }

            // Without absolute_import, we treat the name as complete last.
            if (!absoluteImports) {
                yield return relativeModuleName;
            }
        }
#endif

#if FALSE
        /// <summary>
        /// Looks up the specified module by name.
        /// </summary>
        public MemberResult[] GetModule(string name) {
            return GetModules(modName => modName != name);
        }


        /// <summary>
        /// Gets a top-level list of all the available modules as a list of MemberResults.
        /// </summary>
        /// <returns></returns>
        public MemberResult[] GetModules(bool topLevelOnly = false) {
            return GetModules(modName => topLevelOnly && modName.IndexOf('.') != -1);
        }

        private MemberResult[] GetModules(Func<string, bool> excludedPredicate) {
            var d = new Dictionary<string, List<ModuleLoadState>>();
            foreach (var keyValue in Modules) {
                var modName = keyValue.Key;
                var moduleRef = keyValue.Value;

                if (String.IsNullOrWhiteSpace(modName) ||
                    excludedPredicate(modName)) {
                    continue;
                }

                if (moduleRef.IsValid) {
                    List<ModuleLoadState> l;
                    if (!d.TryGetValue(modName, out l)) {
                        d[modName] = l = new List<ModuleLoadState>();
                    }
                    if (moduleRef.HasModule) {
                        // The REPL shows up here with value=None
                        l.Add(moduleRef);
                    }
                }
            }

            return ModuleDictToMemberResult(d);
        }

        private static MemberResult[] ModuleDictToMemberResult(Dictionary<string, List<ModuleLoadState>> d) {
            var result = new MemberResult[d.Count];
            int pos = 0;
            foreach (var kvp in d) {
                var lazyEnumerator = new LazyModuleEnumerator(kvp.Value);
                result[pos++] = new MemberResult(
                    kvp.Key,
                    lazyEnumerator.GetLazyModules,
                    lazyEnumerator.GetModuleType
                );
            }
            return result;
        }

        class LazyModuleEnumerator {
            private readonly List<ModuleLoadState> _loaded;

            public LazyModuleEnumerator(List<ModuleLoadState> loaded) {
                _loaded = loaded;
            }

            public IEnumerable<AnalysisValue> GetLazyModules() {
                foreach (var value in _loaded) {
                    yield return value.Module;
                }
            }

            public PythonMemberType GetModuleType() {
                PythonMemberType? type = null;
                foreach (var value in _loaded) {
                    if (type == null) {
                        type = value.MemberType;
                    } else if (type != value.MemberType) {
                        type = PythonMemberType.Multiple;
                        break;
                    }
                }
                return type ?? PythonMemberType.Unknown;
            }
        }

        /// <summary>
        /// Searches all modules which match the given name and searches in the modules
        /// for top-level items which match the given name.  Returns a list of all the
        /// available names fully qualified to their name.  
        /// </summary>
        /// <param name="name"></param>
        public IEnumerable<ExportedMemberInfo> FindNameInAllModules(string name) {
            // provide module names first
            foreach (var keyValue in Modules) {
                var modName = keyValue.Key;
                var moduleRef = keyValue.Value;

                if (moduleRef.IsValid) {
                    // include modules which can be imported
                    if (modName == name || PackageNameMatches(name, modName)) {
                        yield return new ExportedMemberInfo(modName, true);
                    }
                }
            }

            // then include module members
            foreach (var keyValue in Modules) {
                var modName = keyValue.Key;
                var moduleRef = keyValue.Value;

                if (moduleRef.IsValid) {
                    // then check for members within the module.
                    if (moduleRef.ModuleContainsMember(name)) {
                        yield return new ExportedMemberInfo(modName + "." + name, true);
                    } else {
                        yield return new ExportedMemberInfo(modName + "." + name, false);
                    }
                }
            }
        }

        private static bool PackageNameMatches(string name, string modName) {
            int lastDot;
            return (lastDot = modName.LastIndexOf('.')) != -1 &&
                modName.Length == lastDot + 1 + name.Length &&
                String.Compare(modName, lastDot + 1, name, 0, name.Length) == 0;
        }

        /// <summary>
        /// returns the MemberResults associated with modules in the specified
        /// list of names.  The list of names is the path through the module, for example
        /// ['System', 'Runtime']
        /// </summary>
        /// <returns></returns>
        public MemberResult[] GetModuleMembers(string[] names, bool includeMembers = false) {
            ModuleReference moduleRef;
            if (Modules.TryGetValue(names[0], out moduleRef) && moduleRef.Module != null) {
                var module = moduleRef.Module as IModule;
                if (module != null) {
                    return GetModuleMembers(names, includeMembers, module);
                }

            }

            return new MemberResult[0];
        }

        internal static MemberResult[] GetModuleMembers(string[] names, bool includeMembers, IModule module) {
            for (int i = 1; i < names.Length && module != null; i++) {
                module = module.GetChildPackage(names[i]);
            }

            if (module != null) {
                List<MemberResult> result = new List<MemberResult>();
                if (includeMembers) {
                    foreach (var keyValue in module.GetAllMembers()) {
                        result.Add(new MemberResult(keyValue.Key, keyValue.Value));
                    }
                    return result.ToArray();
                } else {
                    foreach (var child in module.GetChildrenPackages()) {
                        result.Add(new MemberResult(child.Key, child.Key, new[] { child.Value }, PythonMemberType.Module));
                    }
                    foreach (var keyValue in module.GetAllMembers(moduleContext)) {
                        bool anyModules = false;

                        foreach(var ns in keyValue.Value.OfType<MultipleMemberInfo>()) {
                            if (ns.Members.OfType<IModule>().Any(mod => !(mod is MultipleMemberInfo))) {
                                anyModules = true;
                                break;
                            }
                        }
                        if (anyModules) {
                            result.Add(new MemberResult(keyValue.Key, keyValue.Value));
                        }
                    }
                    return result.ToArray();
                }
            }
            return new MemberResult[0];
        }
#endif
        /// <summary>
        /// Gets the list of directories which should be analyzed.
        /// 
        /// This property is thread safe.
        /// </summary>
        public IEnumerable<string> AnalysisDirectories {
            get {
                lock (_analysisDirs) {
                    return _analysisDirs.ToArray();
                }
            }
        }

        public AnalysisLimits Limits {
            get { return _limits; }
            set { _limits = value; }
        }

        #endregion

        #region Internal Implementation

        internal Deque<AnalysisUnit> Queue {
            get {
                return _queue;
            }
        }

        internal AnalysisValue GetCached(object key, Func<AnalysisValue> maker) {
            AnalysisValue result;
            if (!_itemCache.TryGetValue(key, out result)) {
                // Set the key to prevent recursion
                _itemCache[key] = null;
                _itemCache[key] = result = maker();
            }
            return result;
        }

        internal IAnalysisSet GetAnalysisSetFromObjects(object objects) {
            var typeList = objects as IEnumerable<object>;
            if (typeList == null) {
                return AnalysisSet.Empty;
            }
            return AnalysisSet.UnionAll(typeList.Select(GetConstant));
        }

        internal AnalysisValue GetAnalysisValueFromObjectsThrowOnNull(object attr) {
            if (attr == null) {
                throw new ArgumentNullException("attr");
            }
            return GetConstant(attr);
        }

        internal AnalysisValue GetConstant(object attr) {
            if (attr == null) {
                return _nullInst;
            }

            switch (Type.GetTypeCode(attr.GetType())) {
                case TypeCode.Double:
                    // TODO: What values should be cached?
                    return GetCached(attr, () => new NumberValue((double)attr, this));
                case TypeCode.Boolean:
                    if ((bool)attr) {
                        return _trueInst;
                    } else {
                        return _falseInst;
                    }
                case TypeCode.String:
                    return GetCached(attr, () => new StringValue((string)attr, this));
            }
            if (attr == Parsing.Missing.Value) {
                return _undefined;
            }

            throw new InvalidOperationException(attr.GetType().Name);
        }
#if FALSE
        internal IDictionary<string, IAnalysisSet> GetAllMembers(IMemberContainer container, IModuleContext moduleContext) {
            var names = container.GetMemberNames(moduleContext);
            var result = new Dictionary<string, IAnalysisSet>();
            foreach (var name in names) {
                result[name] = GetAnalysisValueFromObjects(container.GetMember(moduleContext, name));
            }

            return result;
        }
#endif

        internal ModuleTable Modules {
            get { return _modules; }
        }

        private static void Update<K, V>(IDictionary<K, V> dict, IDictionary<K, V> newValues) {
            foreach (var kvp in newValues) {
                dict[kvp.Key] = kvp.Value;
            }
        }

        #endregion

        #region IGroupableAnalysisProject Members

        public void AnalyzeQueuedEntries(CancellationToken cancel) {
            if (cancel.IsCancellationRequested) {
                return;
            }
            new DDG().Analyze(Queue, cancel, _reportQueueSize, _reportQueueInterval);
        }

        #endregion

        /// <summary>
        /// Specifies a callback to invoke to provide feedback on the number of
        /// items being processed.
        /// </summary>
        public void SetQueueReporting(Action<int> reportFunction, int interval = 1) {
            _reportQueueSize = reportFunction;
            _reportQueueInterval = interval;
        }

        /// <summary>
        /// Adds a directory to the list of directories being analyzed.
        /// 
        /// This method is thread safe.
        /// </summary>
        public void AddAnalysisDirectory(string dir) {
            var dirsChanged = AnalysisDirectoriesChanged;
            bool added;
            lock (_analysisDirs) {
                added = _analysisDirs.Add(dir);
            }
            if (added && dirsChanged != null) {
                dirsChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Removes a directory from the list of directories being analyzed.
        /// 
        /// This method is thread safe.
        /// 
        /// New in 1.1.
        /// </summary>
        public void RemoveAnalysisDirectory(string dir) {
            var dirsChanged = AnalysisDirectoriesChanged;
            bool removed;
            lock (_analysisDirs) {
                removed = _analysisDirs.Remove(dir);
            }
            if (removed && dirsChanged != null) {
                dirsChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event fired when the analysis directories have changed.  
        /// 
        /// This event can be fired on any thread.
        /// 
        /// New in 1.1.
        /// </summary>
        public event EventHandler AnalysisDirectoriesChanged;
    }
}

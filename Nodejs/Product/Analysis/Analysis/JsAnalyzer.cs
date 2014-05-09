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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;
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
        internal readonly ObjectValue _globalObject, _objectPrototype;
        internal readonly FunctionValue _arrayFunction;
        internal readonly AnalysisValue _numberPrototype, _stringPrototype, _booleanPrototype, _functionPrototype;
        internal readonly AnalysisValue _emptyStringValue, _zeroIntValue;
        private readonly Deque<AnalysisUnit> _queue;
        private Action<int> _reportQueueSize;
        private int _reportQueueInterval;
        internal readonly AnalysisUnit _evalUnit;   // a unit used for evaluating when we don't otherwise have a unit available
        private readonly HashSet<string> _analysisDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private AnalysisLimits _limits;
        private static object _nullKey = new object();
#if DEBUG
        private static Dictionary<object, int> _analysisCreationCount = new Dictionary<object, int>();
#endif

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

            _emptyStringValue = GetConstant("");
            _zeroIntValue = GetConstant(0.0);

            var globals = GlobalBuilder.MakeGlobal(this);
            _globalObject = globals.GlobalObject;
            _numberPrototype = globals.NumberPrototype;
            _stringPrototype = globals.StringPrototype;
            _booleanPrototype = globals.BooleanPrototype;
            _functionPrototype = globals.FunctionPrototype;
            _arrayFunction = globals.ArrayFunction;
            _objectPrototype = globals.ObjectPrototype;

            var allJson = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "all.json"
            );
            

            if (File.Exists(allJson)) {
                NodejsModuleBuilder.Build(allJson, this);
            }

            _evalUnit = new EvalAnalysisUnit(null, null, new ModuleValue("$global", _builtinEntry).EnvironmentRecord);
            AnalysisLog.NewUnit(_evalUnit);
        }

        #region Public API

        public IEnumerable<string> GlobalMembers {
            get {
                return _globalObject.Descriptors.Keys;
            }
        }

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

            Modules.AddModule(filePath, entry.ModuleValue);

            return entry;
        }

        public void AddPackageJson(string filePath, string entryPoint) {
            if (Path.GetFileName(filePath) != "package.json") {
                throw new InvalidOperationException("path must be to package.json file");
            }

            if (!entryPoint.StartsWith(".")) {
                // entry point must be a relative path
                entryPoint = "./" + entryPoint;
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
                    double dblValue = (double)attr;
                    if(dblValue < -2 || dblValue > 100 ||
                        (dblValue % 1.0) != 0) {
                        Debug.Assert(_zeroIntValue != null);
                        return _zeroIntValue;
                    }
                    return GetCached(attr, () => new NumberValue((double)attr, this));
                case TypeCode.Boolean:
                    if ((bool)attr) {
                        return _trueInst;
                    } else {
                        return _falseInst;
                    }
                case TypeCode.String:
                    var strValue = (string)attr;
                    if (DontCacheString(strValue)) {
                        Debug.Assert(_emptyStringValue != null);
                        return _emptyStringValue;
                    }
                    return GetCached(attr, () => new StringValue((string)attr, this));
            }

            if (attr == Parsing.Missing.Value) {
                return _undefined;
            } else if(attr is Parsing.InvalidNumericErrorValue) {
                return GetConstant(0.0);
            }

            throw new InvalidOperationException(attr.GetType().Name);
        }

        /// <summary>
        /// We really only care about caching strings that are likely
        /// to be identifiers.  Everything else we'll coalesce into the
        /// empty string.
        /// </summary>
        private static bool DontCacheString(string strValue) {
            if (strValue.Length > 100) {
                return true;
            }
            for (int i = 0; i < strValue.Length; i++) {
                char ch = strValue[i];
                if (ch == '-' || ch == '.' || ch == '/' || ch == '\\') {
                    // not valid identifiers, but likely to show up in
                    // require string literals, so we still care about preserving
                    // these strings.
                    continue;
                }
                if (!JSScanner.IsValidIdentifierPart(strValue[i])) {
                    return true;
                }
            }
            return false;
        }

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
        /// </summary>
        public event EventHandler AnalysisDirectoriesChanged;

        [Conditional("DEBUG")]
        internal void AnalysisValueCreated(object key) {
#if DEBUG
            int count;
            if (!_analysisCreationCount.TryGetValue(key, out count)) {
                count = 0;
            }
            _analysisCreationCount[key] = count + 1;
#endif
        }

#if DEBUG
        public string GetAnalysisStats() {
            int totalCount = 0;
            foreach (var value in _analysisCreationCount.Values) {
                totalCount += value;
            }

            StringBuilder res = new StringBuilder();
            res.AppendFormat("Total: {0}", totalCount);
            res.AppendLine();

            var counts = new List<KeyValuePair<object, int>>(_analysisCreationCount);
            counts.Sort((x, y) => x.Value - y.Value);
            res.AppendLine("Stats: ");
            foreach (var kvp in counts) {
                res.AppendFormat("{0} - {1}", kvp.Value, kvp.Key);
                res.AppendLine();
            }

            return res.ToString();
        }
#endif
    }
}

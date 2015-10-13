//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudioTools;
using Microsoft.Win32;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Performs analysis of multiple JavaScript code files and enables interrogation of the resulting analysis.
    /// </summary>
    [Serializable]
    internal partial class JsAnalyzer : IGroupableAnalysisProject, IDeserializeInitialization {
        private readonly ModuleTable _modules;
        internal readonly ProjectEntry _builtinEntry;
        private Dictionary<object, AnalysisValue> _itemCache;
        [NonSerialized]
        private AnalysisLog _log = new AnalysisLog();
        internal readonly NullValue _nullInst;
        internal readonly BooleanValue _trueInst, _falseInst;
        internal readonly UndefinedValue _undefined;
        internal readonly ObjectValue _globalObject, _objectPrototype, _immutableObject;
        internal readonly FunctionValue _arrayFunction;
        internal readonly AnalysisValue _numberPrototype, _stringPrototype, _booleanPrototype, _functionPrototype, _arrayPrototype;
        internal readonly AnalysisValue _emptyStringValue, _zeroIntValue;
        internal readonly BuiltinFunctionValue _requireFunc, _objectGetOwnPropertyDescriptor;
        private readonly Deque<AnalysisUnit> _queue;
        private readonly HashSet<string> _analysisDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private AnalysisLimits _limits;
        private int _analysisCount;
        private static byte[] _serializationVersion;
#if DEBUG
        private static Dictionary<object, int> _analysisCreationCount = new Dictionary<object, int>();
#endif

        public JsAnalyzer(AnalysisLimits limits = null) {
            _modules = new ModuleTable();
            _itemCache = new Dictionary<object, AnalysisValue>();
            _builtinEntry = new ProjectEntry(this, String.Empty, null);

            Limits = limits ?? new AnalysisLimits();

            _queue = new Deque<AnalysisUnit>();

            _nullInst = new NullValue(this);
            _trueInst = new BooleanValue(true, this);
            _falseInst = new BooleanValue(false, this);
            _undefined = new UndefinedValue(this);

            _emptyStringValue = GetConstant(String.Empty);
            _zeroIntValue = GetConstant(0.0);

            var globals = GlobalBuilder.MakeGlobal(this);
            _globalObject = globals.GlobalObject;
            _numberPrototype = globals.NumberPrototype;
            _stringPrototype = globals.StringPrototype;
            _booleanPrototype = globals.BooleanPrototype;
            _functionPrototype = globals.FunctionPrototype;
            _arrayFunction = globals.ArrayFunction;
            _objectPrototype = globals.ObjectPrototype;
            _requireFunc = globals.RequireFunction;
            _arrayPrototype = globals.ArrayPrototype;
            _objectGetOwnPropertyDescriptor = globals.ObjectGetOwnPropertyDescriptor;
            _immutableObject = new ImmutableObjectValue(_builtinEntry);

            var allJson = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "all.json"
            );


            if (File.Exists(allJson)) {
                NodejsModuleBuilder.Build(allJson, this);
            } else {
                Debug.Fail("Could not find all.json");
            }
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
        public ProjectEntry AddModule(string filePath, IAnalysisCookie cookie = null) {
            var entry = new ProjectEntry(this, filePath, cookie);

            Modules.AddModule(filePath, entry);
            
            return entry;
        }

        public IAnalyzable AddPackageJson(string filePath, string entryPoint, List<string> dependencies = null) {
            if (!Path.GetFileName(filePath).Equals("package.json", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("path must be to package.json file");
            }

            if (!entryPoint.StartsWith(".")) {
                // entry point must be a relative path
                entryPoint = "./" + entryPoint;
            }

            var tree = Modules.GetModuleTree(Path.GetDirectoryName(filePath));

            tree.DefaultPackage = entryPoint;

            ProjectEntry projectEntry = null;
            if (dependencies != null) {
                projectEntry = new ProjectEntry(this, filePath, null);
            }

            return new TreeUpdateAnalysis(tree, dependencies, projectEntry, Modules);
        }

        /// <summary>
        /// When our module tree changes any dependencies that have required
        /// that portion of the module tree need to be re-analyzed.  This
        /// analyzable will handle enqueuing those changes on the correct thread.
        /// </summary>
        [Serializable]
        internal class TreeUpdateAnalysis : IAnalyzable {
            private readonly ModuleTree _tree;
            private readonly IEnumerable<string> _dependencies;
            private readonly ProjectEntry _projectEntry;
            private readonly ModuleTable _modules;

            public TreeUpdateAnalysis(ModuleTree tree, IEnumerable<string> dependencies = null, ProjectEntry projectEntry = null, ModuleTable modules = null) {
                _tree = tree;
                _dependencies = dependencies;
                _projectEntry = projectEntry;
                _modules = modules;
            }

            public void Analyze(CancellationToken cancel) {
                if (_tree != null) {
                    _tree.EnqueueDependents();
                }
                
                if (_dependencies != null) {
                    var requireAnalysisUnits = new List<RequireAnalysisUnit>();
                    requireAnalysisUnits.AddRange(_dependencies.Select(
                        dependency => {
                            return new RequireAnalysisUnit(_tree, _modules, _projectEntry, dependency);
                        }));

                    foreach (var unit in requireAnalysisUnits) {
                        unit.AnalyzeWorker(null, cancel);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the specified project entry from the current analysis.
        /// 
        /// This method is thread safe.
        /// </summary>
        public IAnalyzable RemoveModule(IProjectEntry entry) {
            if (entry == null) {
                throw new ArgumentNullException("entry");
            }
            Contract.EndContractBlock();

            var pyEntry = entry as IJsProjectEntry;
            if (pyEntry != null) {
                var tree = Modules.Remove(pyEntry.FilePath);
                if (tree != null) {
                    return new TreeUpdateAnalysis(tree);
                }
            }
            entry.RemovedFromProject();
            return new TreeUpdateAnalysis(null);
        }

        public IJsProjectEntry this[string filename] {
            get {
                ModuleTree tree;
                if (Modules.TryGetValue(filename, out tree) && tree.ProjectEntry != null) {
                    return tree.ProjectEntry;
                }
                throw new KeyNotFoundException();
            }
        }

        public IEnumerable<ProjectEntry> AllModules {
            get {
                return Modules.Modules;
            }
        }
        
        public AnalysisLimits Limits {
            get { return _limits; }
            set { _limits = value; }
        }

        #endregion

        #region Internal Implementation

        /// <summary>
        /// Captures the version of the analyzer.  This should include how we version various
        /// built-in JavaScript objects as our intellisense information for those will get
        /// serialized in addition to the users objects.  This does not capture any information
        /// about the underlying formats of our objects which is handled entirely with the
        /// SHA256 of our internal data structures.
        /// </summary>
        private const int _analyzerVersion = 0x07;

        public static byte[] SerializationVersion {
            get {
                if (_serializationVersion == null) {
                    _serializationVersion = GetSerializationVersion();
                }
                return _serializationVersion;
            }
        }

        private static byte[] GetSerializationVersion() {
            using (new DebugTimer("SerializationVersionInitialization")) {
                SHA256 sha = SHA256Managed.Create();
                StringBuilder sb = new StringBuilder();
                var attrs = (AnalysisSerializationSupportedTypeAttribute[])typeof(AnalysisSerializationSupportedTypeAttribute).Assembly.GetCustomAttributes(typeof(AnalysisSerializationSupportedTypeAttribute), false);
                // we don't want to rely upon the order of reflection here...
                Array.Sort<AnalysisSerializationSupportedTypeAttribute>(
                    attrs,
                    (Comparison<AnalysisSerializationSupportedTypeAttribute>)((x, y) => String.CompareOrdinal(x.Type.Name, y.Type.Name))
                );

                foreach (var attr in attrs) {
                    var type = attr.Type;
                    sb.AppendLine(type.FullName);
                    foreach (FieldInfo field in AnalysisSerializer.GetSerializableMembers(type)) {
                        sb.Append(field.FieldType.FullName);
                        sb.Append(' ');
                        sb.Append(field.Name);
                        sb.AppendLine();
                    }
                }

                return BitConverter.GetBytes(_analyzerVersion)
                    .Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())))
                    .ToArray();
            }
        }


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
            return AnalysisSet.UnionAll(typeList.Select(x => GetConstant(x).Proxy));
        }

        internal AnalysisValue GetAnalysisValueFromObjectsThrowOnNull(object attr) {
            if (attr == null) {
                throw new ArgumentNullException("attr");
            }
            return GetConstant(attr);
        }

        internal AnalysisValue GetConstant(object attr, bool alwaysCache = false) {
            if (attr == null) {
                return _nullInst;
            }

            switch (Type.GetTypeCode(attr.GetType())) {
                case TypeCode.Double:
                    double dblValue = (double)attr;
                    if(dblValue < -2 || dblValue > 30 ||
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
                    if (!alwaysCache && DontCacheString(strValue)) {
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
            _analysisCount += new DDG(this).Analyze(Queue, cancel);
        }

        #endregion

        public int GetAndClearAnalysisCount() {
            // thread safety doesn't matter, just using interlocked
            // as it's simpler than a temp.
            return Interlocked.Exchange(ref _analysisCount, 0);
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

        void IDeserializeInitialization.Init() {
            _log = new AnalysisLog();
        }

        public void DumpLog(TextWriter output, bool asCsv = false) {
            _log.Dump(output, asCsv);
        }

        public int MaxLogLength {
            get {
                return _log.MaxItems;
            }
            set {
                _log.MaxItems = value;
            }
        }

        internal AnalysisLog Log {
            get {
                return _log;
            }
        }
    }
}

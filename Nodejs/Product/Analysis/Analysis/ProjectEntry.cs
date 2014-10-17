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
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Provides interactions to analysis a single file in a project and get the results back.
    /// 
    /// To analyze a file the tree should be updated with a call to UpdateTree and then PreParse
    /// should be called on all files.  Finally Parse should then be called on all files.
    /// </summary>
    [Serializable]
    internal sealed class ProjectEntry : IJsProjectEntry {
        private readonly JsAnalyzer _analyzer;
        private readonly string _filePath;
        private ModuleValue _module;
        private readonly ModuleEnvironmentRecord _moduleRecord;
        private IAnalysisCookie _cookie;
        private JsAst _tree;
        private ModuleAnalysis _currentAnalysis;
        private AnalysisUnit _unit;
        private int _analysisVersion;
        private Dictionary<object, object> _properties = new Dictionary<object, object>();
        private ManualResetEventSlim _curWaiter;
        private int _updatesPending, _waiters;
        internal bool _enqueueModuleDependencies;
        private readonly List<AnalysisProxy> _proxies = new List<AnalysisProxy>();
        internal HashSet<ProjectEntry> _visibleEntries;
        private readonly DependentData _moduleDeps = new DependentData();

        // we expect to have at most 1 waiter on updated project entries, so we attempt to share the event.
        private static ManualResetEventSlim _sharedWaitEvent = new ManualResetEventSlim(false);

        internal ProjectEntry(JsAnalyzer analyzer, string filePath, IAnalysisCookie cookie) {
            _analyzer = analyzer;
            _filePath = filePath;
            _cookie = cookie;
            _moduleRecord = new ModuleEnvironmentRecord(this);

            _unit = new AnalysisUnit(_tree, EnvironmentRecord);
        }

        public AnalysisProxy WrapAnalysisValue(AnalysisValue value) {
            var res = new AnalysisProxy(value);
            _proxies.Add(res);
            return res;
        }

        public event EventHandler<EventArgs> OnNewParseTree;
        public event EventHandler<EventArgs> OnNewAnalysis;

        public void UpdateTree(JsAst newAst, IAnalysisCookie newCookie) {
            lock (this) {
                if (_updatesPending > 0) {
                    _updatesPending--;
                }
                if (newAst == null) {
                    // there was an error in parsing, just let the waiter go...
                    if (_curWaiter != null) {
                        _curWaiter.Set();
                    }
                    _tree = null;
                    return;
                }

                _tree = newAst;
                _cookie = newCookie;

                if (_curWaiter != null) {
                    _curWaiter.Set();
                }
            }

            var newParse = OnNewParseTree;
            if (newParse != null) {
                newParse(this, EventArgs.Empty);
            }
        }

        public void GetTreeAndCookie(out JsAst tree, out IAnalysisCookie cookie) {
            lock (this) {
                tree = _tree;
                cookie = _cookie;
            }
        }

        public void BeginParsingTree() {
            lock (this) {
                _updatesPending++;
            }
        }

        public JsAst WaitForCurrentTree(int timeout = -1) {
            lock (this) {
                if (_updatesPending == 0) {
                    return Tree;
                }

                _waiters++;
                if (_curWaiter == null) {
                    _curWaiter = Interlocked.Exchange(ref _sharedWaitEvent, null);
                    if (_curWaiter == null) {
                        _curWaiter = new ManualResetEventSlim(false);
                    } else {
                        _curWaiter.Reset();
                    }
                }
            }

            bool gotNewTree = _curWaiter.Wait(timeout);

            lock (this) {
                _waiters--;
                if (_waiters == 0 &&
                    Interlocked.CompareExchange(ref _sharedWaitEvent, _curWaiter, null) != null) {
                    _curWaiter.Dispose();
                }
                _curWaiter = null;
            }

            return gotNewTree ? _tree : null;
        }

        public void Analyze(CancellationToken cancel) {
            Analyze(cancel, false);
        }

        public void Analyze(CancellationToken cancel, bool enqueueOnly) {
            if (cancel.IsCancellationRequested) {
                return;
            }
            if (_enqueueModuleDependencies) {
                var tree = _analyzer.Modules.GetModuleTree(_filePath);
                if (tree != null) {
                    tree.EnqueueDependents();
                }
                _enqueueModuleDependencies = false;
            }

            lock (this) {
                _analysisVersion++;

                Parse(enqueueOnly, cancel);
            }

            var newAnalysis = OnNewAnalysis;
            if (newAnalysis != null) {
                newAnalysis(this, EventArgs.Empty);
            }
        }

        public int AnalysisVersion {
            get {
                return _analysisVersion;
            }
        }

        public bool IsAnalyzed {
            get {
                return Analysis != null;
            }
        }

        private void Parse(bool enqueOnly, CancellationToken cancel) {
            JsAst tree;
            IAnalysisCookie cookie;
            GetTreeAndCookie(out tree, out cookie);
            if (tree == null) {
                return;
            }

            if (String.Equals(Path.GetFileName(FilePath), "gruntfile.js", StringComparison.OrdinalIgnoreCase)) {
                _unit = new GruntfileAnalysisUnit(tree, EnvironmentRecord);
            } else {
                _unit = new AnalysisUnit(tree, EnvironmentRecord);
            }
            _moduleDeps.EnqueueDependents();

            if (EnvironmentRecord.HasChildren) {
                EnvironmentRecord.Children.Clear();
            }
            EnvironmentRecord.ClearNodeEnvironments();
            EnvironmentRecord.ClearNodeValues();
#if FALSE
            MyScope.ClearUnresolvedModules();
#endif
            foreach (var wrapper in _proxies) {
                wrapper.NewVersion();
            }
            _proxies.Clear();

            _unit.Enqueue();

            InitNodejsVariables();

            // collect top-level definitions first
            var walker = new OverviewWalker(this, _unit, tree);
            tree.Walk(walker);


            if (!enqueOnly) {
                _analyzer.AnalyzeQueuedEntries(cancel);
            }

            // publish the analysis now that it's complete
            _currentAnalysis = new ModuleAnalysis(
                _unit, 
                ((ModuleEnvironmentRecord)_unit.Environment).CloneForPublish(),
                cookie);
        }

        internal ExportsValue InitNodejsVariables() {
            var filename = _analyzer.GetConstant(_filePath);
            var dirName = _analyzer.GetConstant(String.IsNullOrWhiteSpace(_filePath) ? "" : Path.GetDirectoryName(_filePath), alwaysCache: true);
            var module = _module = new ModuleValue(_filePath, _moduleRecord);
            var exports = new ExportsValue(_filePath, this);
            module.Add("exports", exports.Proxy);

            EnvironmentRecord.GetOrAddVariable("__dirname").AddTypes(this, dirName.Proxy);
            EnvironmentRecord.GetOrAddVariable("__filename").AddTypes(this, filename.Proxy);
            EnvironmentRecord.GetOrAddVariable("exports").AddTypes(this, exports.Proxy);
            EnvironmentRecord.GetOrAddVariable("module").AddTypes(this, module.Proxy);
            return exports;
        }

        public ModuleEnvironmentRecord EnvironmentRecord {
            get {
                return _moduleRecord;
            }
        }

        public IGroupableAnalysisProject AnalysisGroup {
            get {
                return _analyzer;
            }
        }

        public string GetLine(int lineNo) {
            return _cookie.GetLine(lineNo);
        }

        public ModuleAnalysis Analysis {
            get { return _currentAnalysis; }
        }

        public string FilePath {
            get { return _filePath; }
        }

        public IAnalysisCookie Cookie {
            get { return _cookie; }
        }

        internal JsAnalyzer Analyzer {
            get { return _analyzer; }
        }

        public JsAst Tree {
            get { return _tree; }
        }

        public ModuleValue GetModule(AnalysisUnit unit) {
            _moduleDeps.AddDependency(unit);
            return _module;
        }

        public string ModuleName {
            get {
                return _filePath;
            }
        }

        public Dictionary<object, object> Properties {
            get {
                if (_properties == null) {
                    _properties = new Dictionary<object, object>();
                }
                return _properties;
            }
        }


        #region IProjectEntry2 Members

        public void RemovedFromProject() {
            _analysisVersion = -1;
        }

        #endregion

        internal void Enqueue() {
            _unit.Enqueue();
        }

        internal bool IsVisible(ProjectEntry projectEntry) {
            return projectEntry == this || (_visibleEntries != null && _visibleEntries.Contains(projectEntry));
        }

        public override string ToString() {
            return "ProjectEntry: " + _filePath;
        }
    }

    /// <summary>
    /// Represents a unit of work which can be analyzed.
    /// </summary>
    internal interface IAnalyzable {
        void Analyze(CancellationToken cancel);
    }

    /// <summary>
    /// Represents a file which is capable of being analyzed.  Can be cast to other project entry types
    /// for more functionality.  See also IJsProjectEntry and IXamlProjectEntry.
    /// </summary>
    internal interface IProjectEntry : IAnalyzable {
        /// <summary>
        /// Returns true if the project entry has been parsed and analyzed.
        /// </summary>
        bool IsAnalyzed { get; }

        /// <summary>
        /// Returns the current analysis version of the project entry.
        /// </summary>
        int AnalysisVersion {
            get;
        }

        /// <summary>
        /// Returns the project entries file path.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the specified line of text from the project entry.
        /// </summary>
        string GetLine(int lineNo);

        /// <summary>
        /// Provides storage of arbitrary properties associated with the project entry.
        /// </summary>
        Dictionary<object, object> Properties {
            get;
        }

        /// <summary>
        /// Called when the project entry is removed from the project.
        /// 
        /// Implementors of this method must ensure this method is thread safe.
        /// </summary>
        void RemovedFromProject();
    }

    /// <summary>
    /// Represents a project entry which is created by an interpreter for additional
    /// files which it supports analyzing.  Provides the ParseContent method which
    /// is called when the parse queue is ready to update the file contents.
    /// </summary>
    internal interface IExternalProjectEntry : IProjectEntry {
        void ParseContent(TextReader content, IAnalysisCookie fileCookie);
    }

    /// <summary>
    /// Represents a project entry which can be analyzed together with other project entries for
    /// more efficient analysis.
    /// 
    /// To analyze the full group you call Analyze(true) on all the items in the same group (determined
    /// by looking at the identity of the AnalysGroup object).  Then you call AnalyzeQueuedEntries on the
    /// group.
    /// </summary>
    internal interface IGroupableAnalysisProjectEntry {
        /// <summary>
        /// Analyzes this project entry optionally just adding it to the queue shared by the project.
        /// </summary>
        void Analyze(CancellationToken cancel, bool enqueueOnly);

        IGroupableAnalysisProject AnalysisGroup {
            get;
        }
    }

    /// <summary>
    /// Represents a project which can support more efficent analysis of individual items via
    /// analyzing them together.
    /// </summary>
    internal interface IGroupableAnalysisProject {
        void AnalyzeQueuedEntries(CancellationToken cancel);
    }

    internal interface IJsProjectEntry : IGroupableAnalysisProjectEntry, IProjectEntry {
        /// <summary>
        /// Returns the last parsed AST.
        /// </summary>
        JsAst Tree {
            get;
        }

        string ModuleName { get; }

        ModuleAnalysis Analysis {
            get;
        }

        event EventHandler<EventArgs> OnNewParseTree;
        event EventHandler<EventArgs> OnNewAnalysis;

        /// <summary>
        /// Informs the project entry that a new tree will soon be available and will be provided by
        /// a call to UpdateTree.  Calling this method will cause WaitForCurrentTree to block until
        /// UpdateTree has been called.
        /// 
        /// Calls to BeginParsingTree should be balanced with calls to UpdateTree.
        /// 
        /// This method is thread safe.
        /// </summary>
        void BeginParsingTree();

        void UpdateTree(JsAst ast, IAnalysisCookie fileCookie);
        void GetTreeAndCookie(out JsAst ast, out IAnalysisCookie cookie);

        /// <summary>
        /// Returns the current tree if no parsing is currently pending, otherwise waits for the 
        /// current parse to finish and returns the up-to-date tree.
        /// </summary>
        JsAst WaitForCurrentTree(int timeout = -1);
    }
}

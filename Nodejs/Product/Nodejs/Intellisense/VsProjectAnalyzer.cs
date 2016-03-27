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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Options;
using Microsoft.NodejsTools.Parsing;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudioTools;
using Microsoft.Win32;
using SR = Microsoft.NodejsTools.Project.SR;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.NodejsTools.Intellisense {
#if INTERACTIVE_WINDOW
    using IReplEvaluator = IInteractiveEngine;
#endif

    /// <summary>
    /// Performs centralized parsing and analysis of Node.js source code within Visual Studio.
    /// 
    /// This class is responsible for maintaining the up-to-date analysis of the active files being worked
    /// on inside of a Visual Studio project.  
    /// 
    /// This class is built upon the core JsAnalyzer class which provides basic analysis services.  This class
    /// maintains the thread safety invarients of working with that class, handles parsing of files as they're
    /// updated via interfacing w/ the Visual Studio editor APIs, and supports adding additional files to the 
    /// analysis.
    /// 
    /// Code is parsed in parallel using the ParseQueue class.
    /// 
    /// Code is analyzed in a single thread using the analysis engine instance with the AnalysisQueue class.
    /// </summary>
    sealed partial class VsProjectAnalyzer : IDisposable {
        private AnalysisQueue _analysisQueue;
        private readonly HashSet<BufferParser> _activeBufferParsers = new HashSet<BufferParser>();
        private readonly ConcurrentDictionary<string, ProjectItem> _projectFiles;
        private JsAnalyzer _jsAnalyzer;
        private readonly bool _implicitProject;
        private readonly AutoResetEvent _queueActivityEvent = new AutoResetEvent(false);
        private readonly CodeSettings _codeSettings = new CodeSettings();
        private readonly Dictionary<IReplEvaluator, BufferParser> _replParsers = new Dictionary<IReplEvaluator, BufferParser>();

        /// <summary>
        /// This is used for storing cached analysis and is not valid for locating source files.
        /// </summary>
        private readonly string _projectFileDir;
        private readonly AnalysisLevel _analysisLevel;
        private bool _saveToDisk;
        private readonly object _contentsLock = new object();
        private readonly HashSet<IProjectEntry> _hasParseErrors = new HashSet<IProjectEntry>();
        private DateTime? _reparseDateTime;
        private int _userCount;
        private bool _fullyLoaded;
        private List<Action> _loadingDeltas = new List<Action>();

        private static AnalysisLimits _lowLimits = AnalysisLimits.MakeLowAnalysisLimits();
        private static AnalysisLimits _highLimits = new AnalysisLimits();
        private static byte[] _dbHeader;

        // Moniker strings allow the task provider to distinguish between
        // different sources of items for the same file.
        private const string ParserTaskMoniker = "Parser";

        private readonly TaskProvider _defaultTaskProvider = CreateDefaultTaskProvider();

        internal static readonly string[] _emptyCompletionContextKeywords = new string[] {
            "var", "function", "const", "let"
        };
#if FALSE
        private readonly UnresolvedImportSquiggleProvider _unresolvedSquiggles;
#endif

        internal VsProjectAnalyzer(
            string projectFileDir = null
        ) {
            _projectFiles = new ConcurrentDictionary<string, ProjectItem>(StringComparer.OrdinalIgnoreCase);
            if (NodejsPackage.Instance != null) {
                _analysisLevel = NodejsPackage.Instance.IntellisenseOptionsPage.AnalysisLevel;
                _saveToDisk = NodejsPackage.Instance.IntellisenseOptionsPage.SaveToDisk;
            } else {
                _analysisLevel = AnalysisLevel.High;
                _saveToDisk = true;
            }

            var limits = LoadLimits();
            if (projectFileDir != null) {
                _projectFileDir = projectFileDir;
                if (!LoadCachedAnalysis(limits)) {
                    CreateNewAnalyzer(limits);
                }
            } else {
                _implicitProject = true;
                CreateNewAnalyzer(limits);
            }

            if (!_saveToDisk) {
                DeleteAnalysis();
            }

            _userCount = 1;

            InitializeCodeSettings();
        }

        private void InitializeCodeSettings() {
            if (!_fullyLoaded) {
                lock (_loadingDeltas) {
                    if (!_fullyLoaded) {
                        _loadingDeltas.Add(() => InitializeCodeSettings());
                        return;
                    }
                }
            }

            foreach (var name in _jsAnalyzer.GlobalMembers) {
                _codeSettings.AddKnownGlobal(name);
            }
            _codeSettings.AddKnownGlobal("__dirname");
            _codeSettings.AddKnownGlobal("__filename");
            _codeSettings.AddKnownGlobal("module");
            _codeSettings.AddKnownGlobal("exports");

            _codeSettings.AllowShebangLine = true;
        }

        private void CreateNewAnalyzer(AnalysisLimits limits) {
            _jsAnalyzer = new JsAnalyzer(limits);
            if (ShouldEnqueue()) {
                _analysisQueue = new AnalysisQueue(this);
            }
            _fullyLoaded = true;
        }

        private bool ShouldEnqueue() {
            return _analysisLevel != AnalysisLevel.None && _analysisLevel != AnalysisLevel.Preview;
        }

        #region Public API

        public bool SaveToDisk {
            get { return _saveToDisk; }

            set {
                _saveToDisk = value;
                if (!_saveToDisk) {
                    DeleteAnalysis();
                } else if (_analysisQueue != null) {
                    _analysisQueue.ResetLastSaveTime();
                }
            }
        }

        public AnalysisLevel AnalysisLevel {
            get { return _analysisLevel; }
        }

        public void AddUser() {
            Interlocked.Increment(ref _userCount);
        }

        /// <summary>
        /// Reduces the number of known users by one and returns true if the
        /// analyzer should be disposed.
        /// </summary>
        public bool RemoveUser() {
            return Interlocked.Decrement(ref _userCount) == 0;
        }

        public EventHandler<FileEventArgs> WarningAdded;
        public EventHandler<FileEventArgs> WarningRemoved;
        public EventHandler<FileEventArgs> ErrorAdded;
        public EventHandler<FileEventArgs> ErrorRemoved;

        public void AddBuffer(ITextBuffer buffer) {
            if (!_fullyLoaded) {
                lock (_loadingDeltas) {
                    if (!_fullyLoaded) {
                        _loadingDeltas.Add(() => AddBuffer(buffer));
                        return;
                    }
                }
            }

            IReplEvaluator replEval;
            if (buffer.Properties.TryGetProperty<IReplEvaluator>(typeof(IReplEvaluator), out replEval)) {
                BufferParser replParser;
                if (_replParsers.TryGetValue(replEval, out replParser)) {
                    replParser.AddBuffer(buffer);
                    return;
                }
            }

            IProjectEntry projEntry = GetOrCreateProjectEntry(
                buffer,
                new SnapshotCookie(buffer.CurrentSnapshot)
            );

            ConnectErrorList(projEntry, buffer);

            // kick off initial processing on the buffer
            BufferParser bufferParser = EnqueueBuffer(projEntry, buffer);
            lock (_activeBufferParsers) {
                _activeBufferParsers.Add(bufferParser);
            }

            if (replEval != null) {
                _replParsers[replEval] = bufferParser;
            }
        }

        public void RemoveBuffer(ITextBuffer buffer) {
            if (!_fullyLoaded) {
                lock (_loadingDeltas) {
                    if (!_fullyLoaded) {
                        _loadingDeltas.Add(() => RemoveBuffer(buffer));
                        return;
                    }
                }
            }

            IReplEvaluator replEval;
            if (buffer.Properties.TryGetProperty<IReplEvaluator>(typeof(IReplEvaluator), out replEval)) {
                BufferParser replParser;
                if (_replParsers.TryGetValue(replEval, out replParser)) {
                    replParser.RemoveBuffer(buffer);
                    if (replParser.Buffers.Length == 0) {
                        ViewDetached(buffer, replParser);
                    }

                    return;
                }
            }

            BufferParser bufferParser;
            if (!buffer.Properties.TryGetProperty<BufferParser>(typeof(BufferParser), out bufferParser)) {
                return;
            }

            if (--bufferParser.AttachedViews == 0) {
                ViewDetached(buffer, bufferParser);
            }
        }

        private void ViewDetached(ITextBuffer buffer, BufferParser bufferParser) {
            bufferParser.RemoveBuffer(buffer);
            lock (_activeBufferParsers) {
                _activeBufferParsers.Remove(bufferParser);
            }

#if FALSE
                _unresolvedSquiggles.StopListening(bufferParser._currentProjEntry as IPythonProjectEntry);
#endif

            TaskProvider.ClearErrorSource(bufferParser._currentProjEntry, ParserTaskMoniker);

            if (_implicitProject) {
                UnloadFile(bufferParser._currentProjEntry);
            }
        }

        public void AnalyzeFile(string path, bool reportErrors = true) {
            AnalyzeFile(path, reportErrors, null);
        }

        private void AnalyzeFile(string path, bool reportErrors, ProjectItem originatingItem) {
            if (!_fullyLoaded) {
                lock (_loadingDeltas) {
                    if (!_fullyLoaded) {
                        _loadingDeltas.Add(() => AnalyzeFile(path, reportErrors, originatingItem));
                        return;
                    }
                }
            }

            ProjectItem item;
            if (!_projectFiles.TryGetValue(path, out item)) {
                var pyEntry = _jsAnalyzer.AddModule(
                    path,
                    null
                );

                pyEntry.BeginParsingTree();

                item = new ProjectItem(pyEntry);

                item.ReportErrors = reportErrors;
                item.Reloaded = true;
                _projectFiles[path] = item;
                // only parse the file if we need to report errors on it or if
                // we're analyzing.s
                if (reportErrors || _analysisQueue != null) {
                    EnqueueFile(item.Entry, path);
                } else if (item is IJsProjectEntry) {
                    // balance BeginParsingTree call...
                    ((IJsProjectEntry)item.Entry).UpdateTree(null, null);
                }
            } else {
                if (!reportErrors) {
                    TaskProvider.Clear(item.Entry, ParserTaskMoniker);
                }

                if ((_reparseDateTime != null && new FileInfo(path).LastWriteTime > _reparseDateTime.Value)
                        || (reportErrors && !item.ReportErrors) ||
                        (item.Entry.Module == null || item.Entry.Unit == null)) {
                    // this file was written to since our cached analysis was saved, reload it.
                    // Or it was just included in the project and we need to parse for reporting
                    // errors.
                    if (_analysisQueue != null) {
                        EnqueueFile(item.Entry, path);
                    }
                }
                item.ReportErrors = reportErrors;
                item.Reloaded = true;
            }

            if (_implicitProject) {
                if (originatingItem != null) {
                    // file was loaded as part of analysis of a collection of files...
                    item.ImplicitLoadCount += 1;
                    if (originatingItem.LoadedItems == null) {
                        originatingItem.LoadedItems = new List<ProjectItem>();
                    }
                    originatingItem.LoadedItems.Add(item);
                } else {
                    originatingItem.ExplicitlyLoaded = true;
                }
            }
        }

        public void AddPackageJson(string packageJsonPath) {
            string fileContents = null;
            for (int i = 0; i < 10; i++) {
                try {
                    fileContents = File.ReadAllText(packageJsonPath);
                    break;
                } catch {
                    Thread.Sleep(100);
                }
            }

            if (fileContents != null) {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> json;
                try {
                    json = serializer.Deserialize<Dictionary<string, object>>(fileContents);
                } catch {
                    return;
                }

                object mainFile;
                if (json != null && json.TryGetValue("main", out mainFile) && mainFile is string) {
                    List<string> dependencyList = GetDependencyListFromJson(json, "dependencies", "devDependencies", "optionalDependencies");
                    AddPackageJson(packageJsonPath, (string)mainFile, dependencyList);
                }
            }
        }

        private static List<string> GetDependencyListFromJson(Dictionary<string, object> json, params string[] dependencyTypes) {
            var allDependencies = new List<string>();
            foreach (var type in dependencyTypes) {
                object dependencies;
                json.TryGetValue(type, out dependencies);
                var dep = dependencies as Dictionary<string, object>;
                if (dep != null) {
                    allDependencies.AddRange(dep.Keys.ToList());
                }
            }
            return allDependencies;
        }

        public void AddPackageJson(string path, string mainFile, List<string> dependencies) {
            if (!_fullyLoaded) {
                lock (_loadingDeltas) {
                    if (!_fullyLoaded) {
                        _loadingDeltas.Add(() => AddPackageJson(path, mainFile, dependencies));
                        return;
                    }
                }
            }

            if (ShouldEnqueue()) {
                _analysisQueue.Enqueue(
                    _jsAnalyzer.AddPackageJson(path, mainFile, dependencies),
                    AnalysisPriority.Normal
                );
            }
        }

        /// <summary>
        /// Gets a ExpressionAnalysis for the expression at the provided span.  If the span is in
        /// part of an identifier then the expression is extended to complete the identifier.
        /// </summary>
        public static ExpressionAnalysis AnalyzeExpression(ITextSnapshot snapshot, ITrackingSpan span, bool forCompletion = true) {
            var buffer = snapshot.TextBuffer;
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);
            var exprRange = parser.GetExpressionRange(forCompletion);

            if (exprRange == null) {
                return ExpressionAnalysis.Empty;
            }

            string text = exprRange.Value.GetText();

            var applicableSpan = parser.Snapshot.CreateTrackingSpan(
                exprRange.Value.Span,
                SpanTrackingMode.EdgeExclusive
            );

            IJsProjectEntry entry;
            if (buffer.TryGetJsProjectEntry(out entry) && entry.Analysis != null && text.Length > 0) {
                var lineNo = parser.Snapshot.GetLineNumberFromPosition(loc.Start);
                return new ExpressionAnalysis(
                    snapshot.TextBuffer.GetAnalyzer(),
                    text,
                    entry.Analysis,
                    loc.Start,
                    applicableSpan,
                    parser.Snapshot
                );
            }

            return ExpressionAnalysis.Empty;
        }

        public static CompletionAnalysis GetRequireCompletions(ITextSnapshot snapshot, ITrackingSpan applicableSpan, ITrackingPoint point, bool quote) {
            var span = applicableSpan.GetSpan(snapshot);

            if (IsSpaceCompletion(snapshot, point) && !IntellisenseController.ForceCompletions) {
                return CompletionAnalysis.EmptyCompletionContext;
            }

            return new RequireCompletionAnalysis(
                snapshot.TextBuffer.GetAnalyzer(),
                snapshot,
                applicableSpan,
                snapshot.TextBuffer,
                quote
            );
        }

        /// <summary>
        /// Gets a CompletionList providing a list of possible members the user can dot through.
        /// </summary>
        public static CompletionAnalysis GetCompletions(ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point) {
            return TrySpecialCompletions(snapshot, span, point) ??
                GetNormalCompletionContext(snapshot, span, point);
        }

        /// <summary>
        /// Gets a list of signatuers available for the expression at the provided location in the snapshot.
        /// </summary>
        public static SignatureAnalysis GetSignatures(ITextSnapshot snapshot, ITrackingSpan span) {
            var buffer = snapshot.TextBuffer;
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);

            int paramIndex;
            SnapshotPoint? sigStart;
            string lastKeywordArg;
            bool isParameterName;
            var exprRange = parser.GetExpressionRange(1, out paramIndex, out sigStart, out lastKeywordArg, out isParameterName, forSignatureHelp: true);
            if (exprRange == null || sigStart == null) {
                return new SignatureAnalysis(String.Empty, 0, new ISignature[0]);
            }

            var text = new SnapshotSpan(exprRange.Value.Snapshot, new Span(exprRange.Value.Start, sigStart.Value.Position - exprRange.Value.Start)).GetText();
            var applicableSpan = parser.Snapshot.CreateTrackingSpan(exprRange.Value.Span, SpanTrackingMode.EdgeInclusive);

            var analysisItem = buffer.GetProjectEntry();
            if (analysisItem != null) {
                var analysis = ((IJsProjectEntry)analysisItem).Analysis;
                if (analysis != null) {
                    int index = TranslateIndex(loc.Start, snapshot, analysis);

                    IEnumerable<IOverloadResult> sigs;
                    lock (snapshot.TextBuffer.GetAnalyzer()) {
                        sigs = analysis.GetSignaturesByIndex(text, index);
                    }

                    var result = new List<ISignature>();
                    foreach (var sig in sigs) {
                        result.Add(new NodejsSignature(applicableSpan, sig, paramIndex, lastKeywordArg));
                    }

                    return new SignatureAnalysis(
                        text,
                        paramIndex,
                        result,
                        lastKeywordArg
                    );
                }
            }
            return new SignatureAnalysis(text, paramIndex, new ISignature[0]);
        }

        public static int TranslateIndex(int index, ITextSnapshot fromSnapshot, ModuleAnalysis toAnalysisSnapshot) {
            var snapshotCookie = toAnalysisSnapshot.AnalysisCookie as SnapshotCookie;
            // TODO: buffers differ in the REPL window case, in the future we should handle this better
            if (snapshotCookie != null &&
                fromSnapshot != null &&
                snapshotCookie.Snapshot.TextBuffer == fromSnapshot.TextBuffer) {

                index = new SnapshotPoint(fromSnapshot, index).TranslateTo(
                    snapshotCookie.Snapshot,
                    PointTrackingMode.Negative
                ).Position;
            }
            return index;
        }

#if FALSE
        internal static MissingImportAnalysis GetMissingImports(ITextSnapshot snapshot, ITrackingSpan span) {
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, snapshot.TextBuffer, span);
            var loc = span.GetSpan(snapshot.Version);
            int dummy;
            SnapshotPoint? dummyPoint;
            string lastKeywordArg;
            bool isParameterName;
            var exprRange = parser.GetExpressionRange(0, out dummy, out dummyPoint, out lastKeywordArg, out isParameterName);
            if (exprRange == null || isParameterName) {
                return MissingImportAnalysis.Empty;
            }

            var analysis = ((IPythonProjectEntry)snapshot.TextBuffer.GetProjectEntry()).Analysis;
            if (analysis == null) {
                return MissingImportAnalysis.Empty;
            }

            var text = exprRange.Value.GetText();
            var analyzer = analysis.ProjectState;
            var index = span.GetStartPoint(snapshot).Position;

            var expr = Statement.GetExpression(
                analysis.GetAstFromTextByIndex(
                    text,
                    TranslateIndex(
                        index,
                        snapshot,
                        analysis
                    )
                ).Body
            );

            if (expr != null && expr is NameExpression) {
                var nameExpr = (NameExpression)expr;

                if (!IsImplicitlyDefinedName(nameExpr)) {
                    var applicableSpan = parser.Snapshot.CreateTrackingSpan(
                        exprRange.Value.Span,
                        SpanTrackingMode.EdgeExclusive
                    );

                    lock (snapshot.TextBuffer.GetAnalyzer()) {
                        index = TranslateIndex(
                            index,
                            snapshot,
                            analysis
                        );
                        var variables = analysis.GetVariablesByIndex(text, index).Where(IsDefinition).Count();

                        var values = analysis.GetValuesByIndex(text, index).ToArray();

                        // if we have type information or an assignment to the variable we won't offer 
                        // an import smart tag.
                        if (values.Length == 0 && variables == 0) {
                            string name = nameExpr.Name;
                            var imports = analysis.ProjectState.FindNameInAllModules(name);

                            return new MissingImportAnalysis(imports, applicableSpan);
                        }
                    }
                }
            }

            // if we have type information don't offer to add imports
            return MissingImportAnalysis.Empty;
        }

        private static bool IsDefinition(IAnalysisVariable variable) {
            return variable.Type == VariableType.Definition;
        }
#endif

        public bool IsAnalyzing {
            get {
                return IsParsing || (_analysisQueue != null && _analysisQueue.IsAnalyzing);
            }
        }

        public void WaitForCompleteAnalysis(Action<int> itemsLeftUpdated = null, CancellationToken token = default(CancellationToken)) {
            if (IsAnalyzing) {
                while (IsAnalyzing) {
                    _queueActivityEvent.WaitOne(100);

                    int itemsLeft = ParsePending + (_analysisQueue != null ? _analysisQueue.AnalysisPending : 0);

                    if (itemsLeftUpdated != null) {
                        itemsLeftUpdated(itemsLeft);
                    }
                    if (token.IsCancellationRequested) {
                        break;
                    }
                }
            } else {
                if (itemsLeftUpdated != null) {
                    itemsLeftUpdated(0);
                }
            }
        }

        public JsAnalyzer Project {
            get {
                return _jsAnalyzer;
            }
        }

        public int MaxLogLength {
            get {
                if (_jsAnalyzer == null) {
                    return 0;
                }
                return _jsAnalyzer.MaxLogLength;
            }
            set {
                if (!_fullyLoaded) {
                    lock (_loadingDeltas) {
                        if (!_fullyLoaded) {
                            _loadingDeltas.Add(() => MaxLogLength = value);
                            return;
                        }
                    }
                }

                _jsAnalyzer.MaxLogLength = value;
            }
        }

        public void DumpLog(TextWriter output, bool asCsv = false) {
            if (_jsAnalyzer != null) {
                _jsAnalyzer.DumpLog(output, asCsv);
            } else {
                output.WriteLine("Analysis loading...");
            }
        }

        public void ReloadComplete() {
            if (!_fullyLoaded) {
                lock (_loadingDeltas) {
                    if (!_fullyLoaded) {
                        _loadingDeltas.Add(() => ReloadComplete());
                        return;
                    }
                }
            }

            foreach (var item in _projectFiles) {
                if ((!File.Exists(item.Value.Entry.FilePath) || !item.Value.Reloaded)
                    && !item.Value.Entry.IsBuiltin) {
                    UnloadFile(item.Value.Entry);
                }
            }
        }

        public void SwitchAnalyzers(VsProjectAnalyzer oldAnalyzer) {
            lock (_activeBufferParsers) {
                // copy the Keys here as ReAnalyzeTextBuffers can mutuate the dictionary
                BufferParser[] bufferParsers;
                lock (oldAnalyzer._activeBufferParsers) {
                    bufferParsers = oldAnalyzer._activeBufferParsers.ToArray();
                }
                foreach (var bufferParser in bufferParsers) {
                    _activeBufferParsers.Add(bufferParser);
                    ReAnalyzeTextBuffers(bufferParser);
                }
            }
        }

        #endregion

        private static TaskProvider CreateDefaultTaskProvider() {
            var errorList = NodejsPackage.GetGlobalService(typeof(SVsErrorList)) as IVsTaskList;
            var model = (NodejsPackage.Instance as IServiceProvider).GetComponentModel();
            var errorProvider = model != null ? model.GetService<IErrorProviderFactory>() : null;
            return new TaskProvider(NodejsPackage.Instance, errorList, errorProvider);
        }

        private TaskProvider TaskProvider {
            get {
                return _defaultTaskProvider;
            }
        }

        /// <summary>
        /// Creates a new ProjectEntry for the collection of buffers.
        /// 
        /// _openFiles must be locked when calling this function.
        /// </summary>
        private void ReAnalyzeTextBuffers(BufferParser bufferParser) {
            if (!_fullyLoaded) {
                lock (_loadingDeltas) {
                    if (!_fullyLoaded) {
                        _loadingDeltas.Add(() => ReAnalyzeTextBuffers(bufferParser));
                        return;
                    }
                }
            }

            ITextBuffer[] buffers = bufferParser.Buffers;
            if (buffers.Length > 0) {
                var projEntry = GetOrCreateProjectEntry(buffers[0], new SnapshotCookie(buffers[0].CurrentSnapshot));
                foreach (var buffer in buffers) {
                    buffer.Properties.RemoveProperty(typeof(IProjectEntry));
                    buffer.Properties.AddProperty(typeof(IProjectEntry), projEntry);

                    var classifier = buffer.GetNodejsClassifier();
                    if (classifier != null) {
                        classifier.NewVersion();
                    }

                    ConnectErrorList(projEntry, buffer);
                }

                bufferParser._currentProjEntry = projEntry;
                bufferParser._parser = this;

#if FALSE
                // TODO: Add back for navigation bar support
                foreach (var buffer in buffers) {
                    // A buffer may have multiple DropDownBarClients, given one may open multiple CodeWindows
                    // over a single buffer using Window/New Window
                    List<DropDownBarClient> clients;
                    if (buffer.Properties.TryGetProperty<List<DropDownBarClient>>(typeof(DropDownBarClient), out clients)) {
                        foreach (var client in clients) {
                            client.UpdateProjectEntry(projEntry);
                        }
                    }
                }
#endif

                bufferParser.Requeue();
            }
        }

        public void UnloadFile(string filename) {
            ProjectItem item;
            _projectFiles.TryGetValue(filename, out item);
            if (item != null) {
                UnloadFile(item.Entry);
            }
        }

        private void UnloadFile(IProjectEntry entry) {
#if FALSE
            // If we remove a Node.js module, reanalyze any other modules
            // that referenced it.
            var pyEntry = entry as IJsProjectEntry;
            IJsProjectEntry[] reanalyzeEntries = null;
            if (pyEntry != null && !string.IsNullOrEmpty(pyEntry.ModuleName)) {
                reanalyzeEntries = _pyAnalyzer.GetEntriesThatImportModule(pyEntry.ModuleName, false).ToArray();
            }
#endif

            if (_implicitProject) {
                ProjectItem item;
                if (_projectFiles.TryGetValue(entry.FilePath, out item)) {
                    if (item.LoadedItems != null) {
                        // unload any items we brought in..
                        foreach (var implicitItem in item.LoadedItems) {
                            implicitItem.ImplicitLoadCount--;
                            if (implicitItem.ImplicitLoadCount == 0) {
                                if (_analysisLevel != AnalysisLevel.None) {
                                    _analysisQueue.Enqueue(_jsAnalyzer.RemoveModule(implicitItem.Entry), AnalysisPriority.Normal);
                                }
                                ProjectItem implicitRemoved;
                                _projectFiles.TryRemove(implicitItem.Entry.FilePath, out implicitRemoved);
                            }
                        }
                    }
                    item.ExplicitlyLoaded = false;
                    if (item.ImplicitLoadCount != 0) {
                        // we were also implicitly loaded, so leave us implicitly loaded...
                        return;
                    }
                }
            }

            ClearParserTasks(entry);
            if (ShouldEnqueue()) {
                _analysisQueue.Enqueue(_jsAnalyzer.RemoveModule(entry), AnalysisPriority.Normal);
            }
            ProjectItem removed;
            _projectFiles.TryRemove(entry.FilePath, out removed);
#if FALSE
            if (reanalyzeEntries != null) {
                foreach (var existing in reanalyzeEntries) {
                    _analysisQueue.Enqueue(existing, AnalysisPriority.Normal);
                }
            }
#endif
        }

        private void ConnectErrorList(IProjectEntry projEntry, ITextBuffer buffer) {
            TaskProvider.AddBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
        }

        private void DisconnectErrorList(IProjectEntry projEntry, ITextBuffer buffer) {
            TaskProvider.RemoveBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
        }

        private IProjectEntry GetOrCreateProjectEntry(ITextBuffer buffer, IAnalysisCookie analysisCookie) {
            var replEval = buffer.GetReplEvaluator();
            if (replEval != null) {
                // We have a repl window, create an untracked module.
                return _jsAnalyzer.AddModule(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "repl" + Guid.NewGuid() + ".js"),
                    analysisCookie
                );
            }

            string path = buffer.GetFilePath();
            if (path == null) {
                return null;
            }

            ProjectItem file;
            ProjectEntry entry;
            if (!_projectFiles.TryGetValue(path, out file)) {
                if (buffer.ContentType.IsOfType(NodejsConstants.Nodejs)) {
                    entry = _jsAnalyzer.AddModule(
                        buffer.GetFilePath(),
                        analysisCookie
                    );
                } else {
                    return null;
                }

                _projectFiles[path] = file = new ProjectItem(entry);
            }

            if (_implicitProject && _analysisLevel != AnalysisLevel.None) {
                QueueDirectoryAnalysis(path, file);
            }

            return file.Entry;
        }

        private void QueueDirectoryAnalysis(string path, ProjectItem originatingItem) {
            ThreadPool.QueueUserWorkItem(x => { lock (_contentsLock) { AnalyzeDirectory(CommonUtils.NormalizeDirectoryPath(Path.GetDirectoryName(path)), originatingItem); } });
        }

        /// <summary>
        /// Analyzes a complete directory including all of the contained files and packages.
        /// </summary>
        /// <param name="dir">Directory to analyze.</param>
        /// <param name="onFileAnalyzed">If specified, this callback is invoked for every <see cref="IProjectEntry"/>
        /// that is analyzed while analyzing this directory.</param>
        /// <remarks>The callback may be invoked on a thread different from the one that this function was originally invoked on.</remarks>
        private void AnalyzeDirectory(string dir, ProjectItem originatingItem) {
            _analysisQueue.Enqueue(new AddDirectoryAnalysis(dir, this, originatingItem), AnalysisPriority.High);
        }

        class AddDirectoryAnalysis : IAnalyzable {
            private readonly string _dir;
            private readonly VsProjectAnalyzer _analyzer;
            private readonly ProjectItem _originatingItem;

            public AddDirectoryAnalysis(string dir, VsProjectAnalyzer analyzer, ProjectItem originatingItem) {
                _dir = dir;
                _analyzer = analyzer;
                _originatingItem = originatingItem;
            }

            #region IAnalyzable Members

            public void Analyze(CancellationToken cancel) {
                if (cancel.IsCancellationRequested) {
                    return;
                }

                AnalyzeDirectoryWorker(_dir, true, cancel);
            }

            #endregion

            private void AnalyzeDirectoryWorker(string dir, bool addDir, CancellationToken cancel) {
                if (_analyzer._jsAnalyzer == null) {
                    // We aren't able to analyze code.
                    return;
                }

                if (string.IsNullOrEmpty(dir)) {
                    Debug.Assert(false, "Unexpected empty dir");
                    return;
                }

                if (addDir) {
                    lock (_analyzer._contentsLock) {
                        _analyzer._jsAnalyzer.AddAnalysisDirectory(dir);
                    }
                }

                try {
                    var filenames = Directory.GetFiles(dir, "package.json", SearchOption.AllDirectories);
                    foreach (string filename in filenames) {
                        if (cancel.IsCancellationRequested) {
                            break;
                        }
                        _analyzer.AddPackageJson(filename);
                    }
                } catch (IOException) {
                    // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
                } catch (UnauthorizedAccessException) {
                }

                try {
                    var filenames = Directory.GetFiles(dir, "*.js", SearchOption.AllDirectories);
                    foreach (string filename in filenames) {
                        if (cancel.IsCancellationRequested) {
                            break;
                        }
                        _analyzer.AnalyzeFile(filename, reportErrors: false, originatingItem: _originatingItem);
                    }
                } catch (IOException) {
                    // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
                } catch (UnauthorizedAccessException) {
                }
            }
        }

        class ProjectItem {
            public readonly ProjectEntry Entry;
            public bool ReportErrors;
            public bool Reloaded;
            /// <summary>
            /// Number of items which have implicitly loaded this item in the implicit project.
            /// </summary>
            public int ImplicitLoadCount;
            /// <summary>
            /// True if this file has been explicitly loaded (possibly in addition to being implicitly loaded)
            /// </summary>
            public bool ExplicitlyLoaded;
            /// <summary>
            /// The items which this item has implicitly loaded.
            /// </summary>
            public List<ProjectItem> LoadedItems;

            public ProjectItem(ProjectEntry entry) {
                Entry = entry;
            }
        }

        private void ParseFile(IProjectEntry entry, string filename, TextReader reader, IAnalysisCookie cookie) {
            IJsProjectEntry jsEntry;
            IExternalProjectEntry externalEntry;

            if ((jsEntry = entry as IJsProjectEntry) != null) {
                JsAst ast;
                CollectingErrorSink errorSink;
                ParseNodejsCode(reader, out ast, out errorSink);

                if (ast != null) {
                    jsEntry.UpdateTree(ast, cookie);
                } else {
                    // notify that we failed to update the existing analysis
                    jsEntry.UpdateTree(null, null);
                }
                ProjectItem item;
                if (!_projectFiles.TryGetValue(filename, out item) || item.ReportErrors) {
                    // update squiggles for the buffer. snapshot may be null if we
                    // are analyzing a file that is not open
                    UpdateErrorsAndWarnings(entry, GetSnapshot(reader), errorSink);
                } else {
                    TaskProvider.Clear(entry, ParserTaskMoniker);
                }

                // enqueue analysis of the file
                if (ast != null && ShouldEnqueue()) {
                    _analysisQueue.Enqueue(jsEntry, AnalysisPriority.Normal);
                }
            } else if ((externalEntry = entry as IExternalProjectEntry) != null) {
                externalEntry.ParseContent(reader ?? reader, cookie);
                if (ShouldEnqueue()) {
                    _analysisQueue.Enqueue(entry, AnalysisPriority.Normal);
                }
            }
        }

        private static ITextSnapshot GetSnapshot(TextReader reader) {
            SnapshotSpanSourceCodeReader snapshotReader = reader as SnapshotSpanSourceCodeReader;
            if (snapshotReader != null) {
                return snapshotReader.Snapshot;
            }
            return null;
        }

        private void ParseBuffers(BufferParser bufferParser, params ITextSnapshot[] snapshots) {
            IProjectEntry entry = bufferParser._currentProjEntry;

            IJsProjectEntry jsProjEntry = entry as IJsProjectEntry;
            List<JsAst> asts = new List<JsAst>();
            foreach (var snapshot in snapshots) {
                if (snapshot.TextBuffer.Properties.ContainsProperty(NodejsReplEvaluator.InputBeforeReset)) {
                    continue;
                }

                if (snapshot.IsReplBufferWithCommand()) {
                    continue;
                }

                if (jsProjEntry != null && snapshot.TextBuffer.ContentType.IsOfType(NodejsConstants.Nodejs)) {
                    JsAst ast;
                    CollectingErrorSink errorSink;

                    var reader = new SnapshotSpanSourceCodeReader(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                    ParseNodejsCode(reader, out ast, out errorSink);

                    if (ast != null) {
                        asts.Add(ast);
                    }

                    // update squiggles for the buffer
                    UpdateErrorsAndWarnings(entry, snapshot, errorSink);
                } else {
                    // other file such as XAML
                    IExternalProjectEntry externalEntry;
                    if ((externalEntry = (entry as IExternalProjectEntry)) != null) {
                        var snapshotContent = new SnapshotSpanSourceCodeReader(new SnapshotSpan(snapshot, new Span(0, snapshot.Length)));
                        externalEntry.ParseContent(snapshotContent, new SnapshotCookie(snapshotContent.Snapshot));
                        if (ShouldEnqueue()) {
                            _analysisQueue.Enqueue(entry, AnalysisPriority.High);
                        }
                    }
                }
            }

            if (jsProjEntry != null) {
                if (asts.Count > 0) {
                    JsAst finalAst;
                    if (asts.Count == 1) {
                        finalAst = asts[0];
                    } else {
                        // multiple ASTs, merge them together
                        var block = new Block(default(EncodedSpan));
                        var statements = new List<Statement>();
                        foreach (var code in asts) {
                            statements.Add(code.Block);
                        }
                        block.Statements = statements.ToArray();
                        finalAst = asts.Last().CloneWithNewBlock(block);
                    }

                    jsProjEntry.UpdateTree(finalAst, new SnapshotCookie(snapshots[0])); // SnapshotCookie is not entirely right, we should merge the snapshots
                    if (ShouldEnqueue()) {
                        _analysisQueue.Enqueue(entry, AnalysisPriority.High);
                    }
                } else {
                    // indicate that we are done parsing.
                    JsAst prevTree;
                    IAnalysisCookie prevCookie;
                    jsProjEntry.GetTreeAndCookie(out prevTree, out prevCookie);
                    jsProjEntry.UpdateTree(prevTree, prevCookie);
                }
            }
        }

        private JSParser CreateParser(TextReader content, ErrorSink sink) {
            // TODO: JSParser should accept a TextReader
            return new JSParser(content.ReadToEnd(), sink);
        }

        private void ParseNodejsCode(TextReader content, out JsAst ast, out CollectingErrorSink errorSink) {
            ast = null;
            errorSink = new CollectingErrorSink();

            var parser = CreateParser(content, errorSink);
            ast = ParseOneFile(ast, parser);
        }

        private JsAst ParseOneFile(JsAst ast, JSParser parser) {
            if (parser != null) {
                try {
                    ast = parser.Parse(_codeSettings);
                } catch (Exception e) {
                    if (e.IsCriticalException()) {
                        throw;
                    }
                    Debug.Assert(false, String.Format("Failure in JavaScript parser: {0}", e.ToString()));
                }

            }
            return ast;
        }

        private void UpdateErrorsAndWarnings(
            IProjectEntry entry,
            ITextSnapshot snapshot,
            CollectingErrorSink errorSink
        ) {
            // Update the warn-on-launch state for this entry
            bool changed = false;
            lock (_hasParseErrors) {
                if (errorSink.Errors.Any() ? _hasParseErrors.Add(entry) : _hasParseErrors.Remove(entry)) {
                    changed = true;
                }
            }
            if (changed) {
                OnShouldWarnOnLaunchChanged(entry);
            }

            var f = new TaskProviderItemFactory(snapshot);

            // Update the parser warnings/errors
            if (errorSink.Warnings.Any() || errorSink.Errors.Any()) {
                TaskProvider.ReplaceItems(
                    entry,
                    ParserTaskMoniker,
                    errorSink.Warnings
                        .Where(ShouldIncludeWarning)
                        .Select(er => f.FromParseWarning(er))
                        .Concat(errorSink.Errors.Select(er => f.FromParseError(er)))
                        .ToList()
                );
            } else {
                TaskProvider.Clear(entry, ParserTaskMoniker);
            }
#if FALSE
            // Add a handler for the next complete analysis
            _unresolvedSquiggles.ListenForNextNewAnalysis(entry as IJsProjectEntry);
#endif
        }

        private bool ShouldIncludeWarning(ErrorResult error) {
            switch (error.ErrorCode) {
                case JSError.SemicolonInsertion:
                case JSError.ObjectLiteralKeyword:
                case JSError.OctalLiteralsDeprecated:
                case JSError.StatementBlockExpected:
                case JSError.MisplacedFunctionDeclaration:
                case JSError.KeywordUsedAsIdentifier:
                case JSError.SuspectAssignment:
                case JSError.UndeclaredFunction:
                case JSError.UndeclaredVariable:
                    // TODO: Allow the user to control what warnings are reported?
                    return false;
            }
            return true;
        }

        #region Implementation Details

        private static CompletionAnalysis TrySpecialCompletions(ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point) {
            var snapSpan = span.GetSpan(snapshot);
            var buffer = snapshot.TextBuffer;
            var classifier = buffer.GetNodejsClassifier();
            if (classifier == null) {
                return null;
            }
            var start = snapSpan.Start;

            var parser = new ReverseExpressionParser(snapshot, buffer, span);
            if (parser.IsInGrouping()) {
                var range = parser.GetExpressionRange(nesting: 1);
                if (range != null) {
                    start = range.Value.Start;
                }
            }

            // Get the classifiers from beginning of the line to the beginning of snapSpan.
            // The contents of snapSpan differ depending on what is determined in
            // CompletionSource.GetApplicableSpan.
            //
            // In the case of:
            //      var myIdentifier<cursor>
            // the applicable span will be "myIdentifier", so GetClassificationSpans will operate on "var "
            // 
            // In the case of comments and string literals, the applicable span will be empty,
            // so snapSpan.Start will occur at the current cursor position. 
            var tokens = classifier.GetClassificationSpans(new SnapshotSpan(start.GetContainingLine().Start, snapSpan.Start));
            if (tokens.Count > 0) {
                // Check for context-sensitive intellisense
                var lastClass = tokens[tokens.Count - 1];

                if (lastClass.ClassificationType == classifier.Provider.Comment ||
                    lastClass.ClassificationType == classifier.Provider.StringLiteral ||
                    (lastClass.ClassificationType == classifier.Provider.Keyword &&
                    _emptyCompletionContextKeywords.Contains(lastClass.Span.GetText()))) {
                    // No completions in comments, strings, or directly after certain keywords.
                    return CompletionAnalysis.EmptyCompletionContext;
                }
                return null;
            }

            return null;
        }

        private static CompletionAnalysis GetNormalCompletionContext(ITextSnapshot snapshot, ITrackingSpan applicableSpan, ITrackingPoint point) {
            var span = applicableSpan.GetSpan(snapshot);

            if (IsSpaceCompletion(snapshot, point) && !IntellisenseController.ForceCompletions) {
                return CompletionAnalysis.EmptyCompletionContext;
            }

            GetMemberOptions options = GetMemberOptions.IncludeExpressionKeywords | GetMemberOptions.IncludeStatementKeywords;
            var parser = new ReverseExpressionParser(snapshot, snapshot.TextBuffer, applicableSpan);
            if (parser.IsInGrouping()) {
                options &= ~GetMemberOptions.IncludeStatementKeywords;
            }

            return new NormalCompletionAnalysis(
                snapshot.TextBuffer.GetAnalyzer(),
                snapshot,
                applicableSpan,
                snapshot.TextBuffer,
                options
            );
        }

        private static bool IsSpaceCompletion(ITextSnapshot snapshot, ITrackingPoint loc) {
            var pos = loc.GetPosition(snapshot);
            if (pos > 0) {
                return snapshot.GetText(pos - 1, 1) == " ";
            }
            return false;
        }

        private static Stopwatch MakeStopWatch() {
            var res = new Stopwatch();
            res.Start();
            return res;
        }


        private void OnWarningAdded(string path) {
            var evt = WarningAdded;
            if (evt != null) {
                evt(this, new FileEventArgs(path));
            }
        }

        private void OnWarningRemoved(string path) {
            var evt = WarningRemoved;
            if (evt != null) {
                evt(this, new FileEventArgs(path));
            }
        }

        private void OnErrorAdded(string path) {
            var evt = ErrorAdded;
            if (evt != null) {
                evt(this, new FileEventArgs(path));
            }
        }

        private void OnErrorRemoved(string path) {
            var evt = ErrorRemoved;
            if (evt != null) {
                evt(this, new FileEventArgs(path));
            }
        }

        private void ClearParserTasks(IProjectEntry entry) {
            if (entry != null) {
                TaskProvider.Clear(entry, ParserTaskMoniker);

                bool changed;
                lock (_hasParseErrors) {
                    changed = _hasParseErrors.Remove(entry);
                }

                if (changed) {
                    OnShouldWarnOnLaunchChanged(entry);
                }
            }
        }

        internal bool ShouldWarnOnLaunch(IProjectEntry entry) {
            return _hasParseErrors.Contains(entry);
        }

        private void OnShouldWarnOnLaunchChanged(IProjectEntry entry) {
            var evt = ShouldWarnOnLaunchChanged;
            if (evt != null) {
                evt(this, new EntryEventArgs(entry));
            }
        }

        internal event EventHandler<EntryEventArgs> ShouldWarnOnLaunchChanged;

        #endregion

        #region IDisposable Members

        public void Dispose() {
            foreach (var file in _projectFiles.Values) {
                TaskProvider.Clear(file.Entry, ParserTaskMoniker);
            }

            if (_analysisQueue != null) {
                _analysisQueue.Stop();
            }

            TaskProvider.Dispose();
        }

        #endregion

        #region Cached Analysis

        private bool LoadCachedAnalysis(AnalysisLimits limits) {
            string analysisDb = GetAnalysisPath();
            if (File.Exists(analysisDb) && ShouldEnqueue()) {
                FileStream stream = null;
                bool disposeStream = true;
                try {
                    stream = new FileStream(analysisDb, FileMode.Open);
                    byte[] header = new byte[DbHeader.Length];
                    stream.Read(header, 0, header.Length);
                    if (DbHeader.SequenceEqual(header)) {
                        var statusbar = (IVsStatusbar)NodejsPackage.GetGlobalService(typeof(SVsStatusbar));
                        if (statusbar != null) {
                            statusbar.SetText(SR.GetString(SR.StatusAnalysisLoading));
                        }

                        Task.Run(() => {
                            try {
                                using (new DebugTimer("LoadAnalysis")) {
                                    var serializer = new AnalysisSerializer();
                                    var analyzer = (JsAnalyzer)serializer.Deserialize(stream);
                                    AnalysisQueue queue;
                                    if (analyzer.Limits.Equals(limits)) {
                                        queue = new AnalysisQueue(this);
                                        foreach (var entry in analyzer.AllModules) {
                                            _projectFiles[entry.FilePath] = new ProjectItem(entry);
                                        }
                                        _reparseDateTime = new FileInfo(analysisDb).LastWriteTime;

                                        _analysisQueue = queue;
                                        _jsAnalyzer = analyzer;

                                        if (statusbar != null) {
                                            statusbar.SetText(SR.GetString(SR.StatusAnalysisLoaded));
                                        }
                                    }
                                }
                            } catch (InvalidOperationException) {
                                // corrupt or invalid DB
                                if (statusbar != null) {
                                    statusbar.SetText(SR.GetString(SR.StatusAnalysisLoadFailed));
                                }
                            } catch (Exception e) {
                                Debug.Fail(String.Format("Unexpected exception while loading analysis: {0}", e));
                                // bug in deserialization
                                if (statusbar != null) {
                                    statusbar.SetText(SR.GetString(SR.StatusAnalysisLoadFailed));
                                }
                            } finally {
                                stream.Dispose();

                                // apply any changes 
                                lock (_loadingDeltas) {
                                    if (_jsAnalyzer == null) {
                                        // we failed to load the cached analysis, create a new
                                        // analyzer now...
                                        CreateNewAnalyzer(LoadLimits());
                                    }

                                    _fullyLoaded = true;
                                    foreach (var delta in _loadingDeltas) {
                                        delta();
                                    }
                                }
                            }
                        }).HandleAllExceptions(SR.GetString(SR.NodejsToolsForVisualStudio)).DoNotWait();
                        disposeStream = false;
                        return true;
                    }
                } catch (IOException) {
                } finally {
                    if (stream != null && disposeStream) {
                        stream.Dispose();
                    }
                }
            }
            return false;
        }

        private static byte[] DbHeader {
            get {
                if (_dbHeader == null) {
                    _dbHeader = new byte[] { (byte)'J', (byte)'S', (byte)'A', (byte)'N' }
                        .Concat(JsAnalyzer.SerializationVersion)
                        .ToArray();
                }
                return _dbHeader;
            }
        }

        private string GetAnalysisPath() {
            return Path.Combine(_projectFileDir, ".ntvs_analysis.dat");
        }

        private void DeleteAnalysis() {
            if (!_implicitProject && _projectFileDir != null) {
                try {
                    var path = GetAnalysisPath();
                    File.Delete(path);
                } catch (IOException) {
                } catch (UnauthorizedAccessException) {
                }
            }
        }

        private void SaveAnalysis() {
            if (_implicitProject || _projectFileDir == null || !_saveToDisk) {
                return;
            }

            try {
                using (new DebugTimer("SaveAnalysis")) {
                    var path = GetAnalysisPath();
                    var tempPath = path + ".tmp";
                    bool failed = false;
                    if (File.Exists(tempPath)) {
                        // Create doesn't overwrite hidden files, so delete it
                        File.Delete(tempPath);
                    }
                    using (FileStream fs = new FileStream(tempPath, FileMode.Create)) {
                        new FileInfo(tempPath).Attributes |= FileAttributes.Hidden;
                        fs.Write(DbHeader, 0, DbHeader.Length);
                        try {
                            var serializer = new AnalysisSerializer();
                            serializer.Serialize(fs, _jsAnalyzer);
                        } catch (Exception e) {
                            Debug.Fail("Failed to save analysis " + e);
                            failed = true;
                        }
                    }

                    if (!failed) {
                        if (File.Exists(path)) {
                            File.Delete(path);
                        }
                        File.Move(tempPath, path);
                    } else {
                        File.Delete(tempPath);
                    }
                }
            } catch (IOException) {
            } catch (UnauthorizedAccessException) {
            }
        }

        #endregion

        #region Analysis Limits

        private const string AnalysisLimitsKey = @"Software\Microsoft\NodejsTools\" + AssemblyVersionInfo.VSVersion +
    @"\Analysis\Project";

        private AnalysisLimits LoadLimits() {
            AnalysisLimits defaults = null;

            if (NodejsPackage.Instance != null) {
                switch (_analysisLevel) {
                    case Options.AnalysisLevel.Medium:
                        defaults = AnalysisLimits.MakeMediumAnalysisLimits();
                        break;
                    case Options.AnalysisLevel.Low:
                        defaults = _lowLimits;
                        break;
                }
            }
            if (defaults == null) {
                defaults = _highLimits;
            }

            try {
                using (var key = Registry.CurrentUser.OpenSubKey(AnalysisLimitsKey)) {
                    return LoadLimitsFromStorage(key, defaults);
                }
            } catch (SecurityException) {
            } catch (UnauthorizedAccessException) {
            } catch (IOException) {
            }
            return defaults;
        }


        /// <summary>
        /// Loads a new instance from the specified registry key.
        /// </summary>
        /// <param name="key">
        /// The key to load settings from. If
        /// null, all settings are assumed to be unspecified and the default
        /// values are used.
        /// </param>
        /// <param name="defaults">
        /// The default analysis limits if they're not available in the regkey.
        /// </param>
        private static AnalysisLimits LoadLimitsFromStorage(RegistryKey key, AnalysisLimits defaults) {
            AnalysisLimits limits = new AnalysisLimits();

            limits.ReturnTypes = GetSetting(key, ReturnTypesId) ?? defaults.ReturnTypes;
            limits.InstanceMembers = GetSetting(key, InstanceMembersId) ?? defaults.InstanceMembers;
            limits.DictKeyTypes = GetSetting(key, DictKeyTypesId) ?? defaults.DictKeyTypes;
            limits.DictValueTypes = GetSetting(key, DictValueTypesId) ?? defaults.DictValueTypes;
            limits.IndexTypes = GetSetting(key, IndexTypesId) ?? defaults.IndexTypes;
            limits.AssignedTypes = GetSetting(key, AssignedTypesId) ?? defaults.AssignedTypes;
            limits.NestedModulesLimit = GetSetting(key, NestedModulesLimitId) ?? defaults.NestedModulesLimit;

            return limits;
        }

        private static int? GetSetting(RegistryKey key, string setting) {
            if (key != null) {
                return key.GetValue(ReturnTypesId) as int?;
            }
            return null;
        }


        private const string ReturnTypesId = "ReturnTypes";
        private const string InstanceMembersId = "InstanceMembers";
        private const string DictKeyTypesId = "DictKeyTypes";
        private const string DictValueTypesId = "DictValueTypes";
        private const string IndexTypesId = "IndexTypes";
        private const string AssignedTypesId = "AssignedTypes";
        private const string NestedModulesLimitId = "NestedModulesLimit";

        #endregion
    }
}

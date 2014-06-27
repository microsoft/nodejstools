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
using System.Reflection;
using System.Threading;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Parsing;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudioTools;

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
    /// </summary>
    public sealed class VsProjectAnalyzer : IDisposable {
        private readonly ParseQueue _queue;
        private readonly AnalysisQueue _analysisQueue;
        private readonly Dictionary<BufferParser, IProjectEntry> _openFiles = new Dictionary<BufferParser, IProjectEntry>();
        private readonly ConcurrentDictionary<string, IProjectEntry> _projectFiles;
        private readonly JsAnalyzer _jsAnalyzer;
        private readonly bool _implicitProject;
        private readonly AutoResetEvent _queueActivityEvent = new AutoResetEvent(false);
        private readonly CodeSettings _codeSettings = new CodeSettings();
        private readonly string _projectDir;
        private DateTime? _reparseDateTime;

        private int _userCount;

        internal readonly HashSet<IProjectEntry> _hasParseErrors = new HashSet<IProjectEntry>();

        // Moniker strings allow the task provider to distinguish between
        // different sources of items for the same file.
        private const string ParserTaskMoniker = "Parser";
        internal const string UnresolvedImportMoniker = "UnresolvedImport";


        internal static Lazy<TaskProvider> ReplaceTaskProviderForTests(Lazy<TaskProvider> newProvider) {
            return Interlocked.Exchange(ref _taskProvider, newProvider);
        }

        private static Lazy<TaskProvider> _taskProvider;
        private static readonly Lazy<TaskProvider> _defaultTaskProvider = new Lazy<TaskProvider>(() => {
            var errorList = NodejsPackage.GetGlobalService(typeof(SVsErrorList)) as IVsTaskList;
            var model = NodejsPackage.ComponentModel;
            var errorProvider = model != null ? model.GetService<IErrorProviderFactory>() : null;
            return new TaskProvider(errorList, errorProvider);
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static Lazy<TaskProvider> TaskProvider {
            get {
                return _taskProvider ?? _defaultTaskProvider;
            }
        }
#if FALSE
        private readonly UnresolvedImportSquiggleProvider _unresolvedSquiggles;
#endif

        private object _contentsLock = new object();

        const int _curDbVersion = 2;
#if DEBUG
        const int _dbVersion = unchecked((int)(0x80000000 | _curDbVersion));
#else
        const int _dbVersion = _curDbVersion;
#endif

        private static byte[] _dbHeader = new byte[] { (byte)'J', (byte)'S', (byte)'A', (byte)'N' }.Concat(BitConverter.GetBytes(_dbVersion)).ToArray();

        internal VsProjectAnalyzer(
            string projectDir = null
        ) {
            _queue = new ParseQueue(this);
            _projectFiles = new ConcurrentDictionary<string, IProjectEntry>(StringComparer.OrdinalIgnoreCase);

            if (projectDir != null) {
                _projectDir = projectDir;
                string analysisDb = GetAnalysisPath();
                if (File.Exists(analysisDb)) {

                    using (FileStream stream = new FileStream(analysisDb, FileMode.Open)) {
                        byte[] header = new byte[_dbHeader.Length];
                        stream.Read(header, 0, header.Length);
                        bool match = true;
                        for (int i = 0; i < header.Length; i++) {
                            if (header[i] != _dbHeader[i]) {
                                match = false;
                                break;
                            }
                        }
                        if (match) {
                            try {
                                var serializer = new AnalysisSerializer();
                                _jsAnalyzer = (JsAnalyzer)serializer.Deserialize(stream);
                                _analysisQueue = new AnalysisQueue(this, serializer, stream);
                                foreach (var file in _jsAnalyzer.AllModules) {
                                    _projectFiles[file.FilePath] = file;
                                }
                                _reparseDateTime = new FileInfo(analysisDb).LastWriteTime;
                            } catch (InvalidOperationException) {
                                // corrupt or invalid DB
                                _jsAnalyzer = null;
                                _analysisQueue = null;
                            } catch (IOException) {
                                _jsAnalyzer = null;
                                _analysisQueue = null;
                            }
                        }
                    }
                }
            } else {
                _implicitProject = true;
            }

            if (_jsAnalyzer == null) {
                _jsAnalyzer = new JsAnalyzer();
                _analysisQueue = new AnalysisQueue(this);
            }

            _userCount = 1;

            foreach (var name in _jsAnalyzer.GlobalMembers) {
                _codeSettings.AddKnownGlobal(name);
            }
            _codeSettings.AddKnownGlobal("__dirname");
            _codeSettings.AddKnownGlobal("__filename");
            _codeSettings.AddKnownGlobal("module");
            _codeSettings.AddKnownGlobal("exports");

            _codeSettings.AllowShebangLine = true;
        }

        private string GetAnalysisPath() {
            return Path.Combine(_projectDir, ".ntvs_analysis.dat");
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

#if FALSE
        /// <summary>
        /// Creates a new ProjectEntry for the collection of buffers.
        /// 
        /// _openFiles must be locked when calling this function.
        /// </summary>
        internal void ReAnalyzeTextBuffers(BufferParser bufferParser) {
            ITextBuffer[] buffers = bufferParser.Buffers;
            if (buffers.Length > 0) {
                var projEntry = CreateProjectEntry(buffers[0], new SnapshotCookie(buffers[0].CurrentSnapshot));
                foreach (var buffer in buffers) {
                    buffer.Properties.RemoveProperty(typeof(IProjectEntry));
                    buffer.Properties.AddProperty(typeof(IProjectEntry), projEntry);

                    var classifier = buffer.GetPythonClassifier();
                    if (classifier != null) {
                        classifier.NewVersion();
                    }

                    ConnectErrorList(projEntry, buffer);
                }

                bufferParser._currentProjEntry = _openFiles[bufferParser] = projEntry;
                bufferParser._parser = this;

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

                bufferParser.Requeue();
            }
        }

        internal void SwitchAnalyzers(VsProjectAnalyzer oldAnalyzer) {
            lock (_openFiles) {
                // copy the Keys here as ReAnalyzeTextBuffers can mutuate the dictionary
                foreach (var bufferParser in oldAnalyzer._openFiles.Keys.ToArray()) {
                    ReAnalyzeTextBuffers(bufferParser);
                }
            }
        }
#endif

        public static void ConnectErrorList(IProjectEntry projEntry, ITextBuffer buffer) {
            TaskProvider.Value.AddBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
        }

        public static void DisconnectErrorList(IProjectEntry projEntry, ITextBuffer buffer) {
            TaskProvider.Value.RemoveBufferForErrorSource(projEntry, ParserTaskMoniker, buffer);
        }

        /// <summary>
        /// Starts monitoring a buffer for changes so we will re-parse the buffer to update the analysis
        /// as the text changes.
        /// </summary>
        internal MonitoredBufferResult MonitorTextBuffer(ITextView textView, ITextBuffer buffer) {
            IProjectEntry projEntry = CreateProjectEntry(buffer, new SnapshotCookie(buffer.CurrentSnapshot));

            ConnectErrorList(projEntry, buffer);

            if (!buffer.Properties.ContainsProperty(typeof(IReplEvaluator))) {
                TaskProvider.Value.AddBufferForErrorSource(projEntry, UnresolvedImportMoniker, buffer);
            }

            // kick off initial processing on the buffer
            lock (_openFiles) {
                var bufferParser = _queue.EnqueueBuffer(projEntry, textView, buffer);
                _openFiles[bufferParser] = projEntry;
                return new MonitoredBufferResult(bufferParser, textView, projEntry);
            }
        }

        internal void StopMonitoringTextBuffer(BufferParser bufferParser, ITextView textView) {
            bufferParser.StopMonitoring();
            lock (_openFiles) {
                _openFiles.Remove(bufferParser);
            }

#if FALSE
            _unresolvedSquiggles.StopListening(bufferParser._currentProjEntry as IPythonProjectEntry);
#endif

            if (TaskProvider.IsValueCreated) {
                TaskProvider.Value.ClearErrorSource(bufferParser._currentProjEntry, ParserTaskMoniker);
                TaskProvider.Value.ClearErrorSource(bufferParser._currentProjEntry, UnresolvedImportMoniker);

                if (ImplicitProject) {
                    // remove the file from the error list
                    TaskProvider.Value.Clear(bufferParser._currentProjEntry, ParserTaskMoniker);
                    TaskProvider.Value.Clear(bufferParser._currentProjEntry, UnresolvedImportMoniker);
                }
            }
        }

        private IProjectEntry CreateProjectEntry(ITextBuffer buffer, IAnalysisCookie analysisCookie) {
            if (_jsAnalyzer == null) {
                // We aren't able to analyze code, so don't create an entry.
                return null;
            }

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

            IProjectEntry entry;
            if (!_projectFiles.TryGetValue(path, out entry)) {
                if (buffer.ContentType.IsOfType(NodejsConstants.Nodejs)) {
                    entry = _jsAnalyzer.AddModule(
                        buffer.GetFilePath(),
                        analysisCookie
                    );
                } else {
                    return null;
                }

                _projectFiles[path] = entry;

                if (ImplicitProject) {
                    QueueDirectoryAnalysis(path);
                }
            }

            return entry;
        }

        private void QueueDirectoryAnalysis(string path) {
            ThreadPool.QueueUserWorkItem(x => { lock (_contentsLock) { AnalyzeDirectory(CommonUtils.NormalizeDirectoryPath(Path.GetDirectoryName(path))); } });
        }

        internal IProjectEntry AnalyzeFile(string path) {
            if (_jsAnalyzer == null) {
                // We aren't able to analyze code, so don't create an entry.
                return null;
            }

            IProjectEntry item;
            if (!_projectFiles.TryGetValue(path, out item)) {
                if (NodejsProjectNode.IsNodejsFile(path)) {
                    var pyEntry = _jsAnalyzer.AddModule(
                        path,
                        null
                    );

                    pyEntry.BeginParsingTree();

                    item = pyEntry;
                }

                if (item != null) {
                    _projectFiles[path] = item;
                    _queue.EnqueueFile(item, path);
                }
            } else {
                if (_reparseDateTime != null && new FileInfo(path).LastWriteTime > _reparseDateTime.Value) {
                    // this file was written to since our cached analysis was saved, reload it.
                    _queue.EnqueueFile(item, path);
                }
            }

            return item;
        }

        internal void AddPackageJson(string path, string mainFile) {
            _analysisQueue.Enqueue(
                _jsAnalyzer.AddPackageJson(path, mainFile),
                AnalysisPriority.Normal
            );
        }

        internal IEnumerable<KeyValuePair<string, IProjectEntry>> LoadedFiles {
            get {
                return _projectFiles;
            }
        }

        internal IProjectEntry GetEntryFromFile(string path) {
            IProjectEntry res;
            if (_projectFiles.TryGetValue(path, out res)) {
                return res;
            }
            return null;
        }

        /// <summary>
        /// Gets a ExpressionAnalysis for the expression at the provided span.  If the span is in
        /// part of an identifier then the expression is extended to complete the identifier.
        /// </summary>
        internal static ExpressionAnalysis AnalyzeExpression(ITextSnapshot snapshot, ITrackingSpan span, bool forCompletion = true) {
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

        /// <summary>
        /// Gets a CompletionList providing a list of possible members the user can dot through.
        /// </summary>
        internal static CompletionAnalysis GetCompletions(ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point) {
            return TrySpecialCompletions(snapshot, span, point) ??
                GetNormalCompletionContext(snapshot, span, point);
        }

        /// <summary>
        /// Gets a list of signatuers available for the expression at the provided location in the snapshot.
        /// </summary>
        internal static SignatureAnalysis GetSignatures(ITextSnapshot snapshot, ITrackingSpan span) {
            var buffer = snapshot.TextBuffer;
            ReverseExpressionParser parser = new ReverseExpressionParser(snapshot, buffer, span);

            var loc = parser.Span.GetSpan(parser.Snapshot.Version);

            int paramIndex;
            SnapshotPoint? sigStart;
            string lastKeywordArg;
            bool isParameterName;
            var exprRange = parser.GetExpressionRange(1, out paramIndex, out sigStart, out lastKeywordArg, out isParameterName);
            if (exprRange == null || sigStart == null) {
                return new SignatureAnalysis("", 0, new ISignature[0]);
            }

            var text = new SnapshotSpan(exprRange.Value.Snapshot, new Span(exprRange.Value.Start, sigStart.Value.Position - exprRange.Value.Start)).GetText();
            var applicableSpan = parser.Snapshot.CreateTrackingSpan(exprRange.Value.Span, SpanTrackingMode.EdgeInclusive);
#if FALSE
            if (snapshot.TextBuffer.GetAnalyzer().ShouldEvaluateForCompletion(text)) {
                var liveSigs = TryGetLiveSignatures(snapshot, paramIndex, text, applicableSpan, lastKeywordArg);
                if (liveSigs != null) {
                    return liveSigs;
                }
            }
#endif

            var start = Stopwatch.ElapsedMilliseconds;

            var analysisItem = buffer.GetProjectEntry();
            if (analysisItem != null) {
                var analysis = ((IJsProjectEntry)analysisItem).Analysis;
                if (analysis != null) {
                    int index = TranslateIndex(loc.Start, snapshot, analysis);

                    IEnumerable<IOverloadResult> sigs;
                    lock (snapshot.TextBuffer.GetAnalyzer()) {
                        sigs = analysis.GetSignaturesByIndex(text, index);
                    }
                    var end = Stopwatch.ElapsedMilliseconds;

                    if (/*Logging &&*/ (end - start) > CompletionAnalysis.TooMuchTime) {
                        Trace.WriteLine(String.Format("{0} lookup time {1} for signatures", text, end - start));
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

        internal static int TranslateIndex(int index, ITextSnapshot fromSnapshot, ModuleAnalysis toAnalysisSnapshot) {
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

        private static bool IsImplicitlyDefinedName(NameExpression nameExpr) {
            return nameExpr.Name == "__all__" ||
                nameExpr.Name == "__file__" ||
                nameExpr.Name == "__doc__" ||
                nameExpr.Name == "__name__";
        }
#endif
        internal bool IsAnalyzing {
            get {
                return _queue.IsParsing || _analysisQueue.IsAnalyzing;
            }
        }

        internal void WaitForCompleteAnalysis(Func<int, bool> itemsLeftUpdated) {
            if (IsAnalyzing) {
                while (IsAnalyzing) {
                    QueueActivityEvent.WaitOne(100);

                    int itemsLeft = _queue.ParsePending + _analysisQueue.AnalysisPending;

                    if (!itemsLeftUpdated(itemsLeft)) {
                        break;
                    }
                }
            } else {
                itemsLeftUpdated(0);
            }
        }

        internal AutoResetEvent QueueActivityEvent {
            get {
                return _queueActivityEvent;
            }
        }

        /// <summary>
        /// True if the project is an implicit project and it should model files on disk in addition
        /// to files which are explicitly added.
        /// </summary>
        internal bool ImplicitProject {
            get {
                return _implicitProject;
            }
        }

        public JsAnalyzer Project {
            get {
                return _jsAnalyzer;
            }
        }

        internal JsAst ParseSnapshot(ITextSnapshot snapshot) {
            var parser = CreateParser(
                new SnapshotSpanSourceCodeReader(
                    new SnapshotSpan(snapshot, 0, snapshot.Length)
                ),
                new ErrorSink()
            );
            return parser.Parse(_codeSettings);
        }

        internal ITextSnapshot GetOpenSnapshot(IProjectEntry entry) {
            if (entry == null) {
                return null;
            }

            lock (_openFiles) {
                var item = _openFiles.FirstOrDefault(kv => kv.Value == entry);
                if (item.Value == null) {
                    return null;
                }
                var document = item.Key.Document;

                return document != null ? document.TextBuffer.CurrentSnapshot : null;
            }
        }

        internal void ParseFile(IProjectEntry entry, string filename, Stream content) {
            IJsProjectEntry pyEntry;
            IExternalProjectEntry externalEntry;

            TextReader reader = null;
            ITextSnapshot snapshot = GetOpenSnapshot(entry);
            IAnalysisCookie cookie;
            if (snapshot != null) {
                cookie = new SnapshotCookie(snapshot);
                reader = new SnapshotSpanSourceCodeReader(new SnapshotSpan(snapshot, 0, snapshot.Length));
            } else {
                cookie = new FileCookie(filename);
            }
            
            if ((pyEntry = entry as IJsProjectEntry) != null) {
                JsAst ast;
                CollectingErrorSink errorSink;
                if (reader != null) {
                    ParseNodejsCode(reader, out ast, out errorSink);
                } else {
                    ParseNodejsCode(content, out ast, out errorSink);
                }

                if (ast != null) {
                    pyEntry.UpdateTree(ast, cookie);
                } else {
                    // notify that we failed to update the existing analysis
                    pyEntry.UpdateTree(null, null);
                }

                // update squiggles for the buffer. snapshot may be null if we
                // are analyzing a file that is not open
                UpdateErrorsAndWarnings(entry, snapshot, errorSink);

                // enqueue analysis of the file
                if (ast != null) {
                    _analysisQueue.Enqueue(pyEntry, AnalysisPriority.Normal);
                }
            } else if ((externalEntry = entry as IExternalProjectEntry) != null) {
                externalEntry.ParseContent(reader ?? new StreamReader(content), cookie);
                _analysisQueue.Enqueue(entry, AnalysisPriority.Normal);
            }
        }

        internal void ParseBuffers(BufferParser bufferParser, params ITextSnapshot[] snapshots) {
            IProjectEntry entry = bufferParser._currentProjEntry;

            IJsProjectEntry pyProjEntry = entry as IJsProjectEntry;
            List<JsAst> asts = new List<JsAst>();
            foreach (var snapshot in snapshots) {
                if (snapshot.TextBuffer.Properties.ContainsProperty(NodejsReplEvaluator.InputBeforeReset)) {
                    continue;
                }

                if (snapshot.IsReplBufferWithCommand()) {
                    continue;
                }

                if (pyProjEntry != null && snapshot.TextBuffer.ContentType.IsOfType(NodejsConstants.Nodejs)) {
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
                        _analysisQueue.Enqueue(entry, AnalysisPriority.High);
                    }
                }
            }

            if (pyProjEntry != null) {
                if (asts.Count > 0) {
                    JsAst finalAst;
                    if (asts.Count == 1) {
                        finalAst = asts[0];
                    } else {
                        // multiple ASTs, merge them together
                        var block = new Block(default(IndexSpan));
                        foreach (var code in asts) {
                            block.Append(code.Block);
                        }
                        finalAst = asts.Last().CloneWithNewBlock(block);
                    }

                    pyProjEntry.UpdateTree(finalAst, new SnapshotCookie(snapshots[0])); // SnapshotCookie is not entirely right, we should merge the snapshots
                    _analysisQueue.Enqueue(entry, AnalysisPriority.High);
                } else {
                    // indicate that we are done parsing.
                    JsAst prevTree;
                    IAnalysisCookie prevCookie;
                    pyProjEntry.GetTreeAndCookie(out prevTree, out prevCookie);
                    pyProjEntry.UpdateTree(prevTree, prevCookie);
                }
            }
        }

        private void ParseNodejsCode(Stream content, out JsAst ast, out CollectingErrorSink errorSink) {
            ast = null;
            errorSink = new CollectingErrorSink();

            try {
                ast = CreateParser(new StreamReader(content), errorSink).Parse(_codeSettings);
            } catch (Exception e) {
                if (e.IsCriticalException()) {
                    throw;
                }
                FileStream stream = content as FileStream;
                string file = "";
                if (stream != null) {
                    file = stream.Name + Environment.NewLine + Environment.NewLine;
                }
                Debug.Assert(false, String.Format(file + "Failure in JavaScript parser: {0}", e.ToString()));
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
                TaskProvider.Value.ReplaceItems(
                    entry,
                    ParserTaskMoniker,
                    errorSink.Warnings
                        .Where(ShouldIncludeWarning)
                        .Select(er => f.FromParseWarning(er))
                        .Concat(errorSink.Errors.Select(er => f.FromParseError(er)))
                        .ToList()
                );
            } else if (TaskProvider.IsValueCreated) {
                TaskProvider.Value.Clear(entry, ParserTaskMoniker);
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
                    // TODO: Allow the user to control what warnings are reported?
                    return false;
            }
            return true;
        }

        #region Implementation Details

        private static Stopwatch _stopwatch = MakeStopWatch();

        internal static Stopwatch Stopwatch {
            get {
                return _stopwatch;
            }
        }

#if FALSE
        private static SignatureAnalysis TryGetLiveSignatures(ITextSnapshot snapshot, int paramIndex, string text, ITrackingSpan applicableSpan, string lastKeywordArg) {
            IReplEvaluator eval;
            IPythonReplIntellisense dlrEval;
            if (snapshot.TextBuffer.Properties.TryGetProperty<IReplEvaluator>(typeof(IReplEvaluator), out eval) &&
                (dlrEval = eval as IPythonReplIntellisense) != null) {
                if (text.EndsWith("(")) {
                    text = text.Substring(0, text.Length - 1);
                }
                var liveSigs = dlrEval.GetSignatureDocumentation(text);

                if (liveSigs != null && liveSigs.Length > 0) {
                    return new SignatureAnalysis(text, paramIndex, GetLiveSignatures(text, liveSigs, paramIndex, applicableSpan, lastKeywordArg), lastKeywordArg);
                }
            }
            return null;
        }

        private static ISignature[] GetLiveSignatures(string text, ICollection<OverloadDoc> liveSigs, int paramIndex, ITrackingSpan span, string lastKeywordArg) {
            ISignature[] res = new ISignature[liveSigs.Count];
            int i = 0;
            foreach (var sig in liveSigs) {
                res[i++] = new PythonSignature(
                    span,
                    new LiveOverloadResult(text, sig.Documentation, sig.Parameters),
                    paramIndex,
                    lastKeywordArg
                );
            }
            return res;
        }

        class LiveOverloadResult : IOverloadResult {
            private readonly string _name, _doc;
            private readonly ParameterResult[] _parameters;

            public LiveOverloadResult(string name, string documentation, ParameterResult[] parameters) {
                _name = name;
                _doc = documentation;
                _parameters = parameters;
            }

            #region IOverloadResult Members

            public string Name {
                get { return _name; }
            }

            public string Documentation {
                get { return _doc; }
            }

            public ParameterResult[] Parameters {
                get { return _parameters; }
            }

            #endregion
        }

        internal bool ShouldEvaluateForCompletion(string source) {
            if (PythonToolsPackage.Instance != null) {
                switch (PythonToolsPackage.Instance.InteractiveOptionsPage.GetOptions(_interpreterFactory).ReplIntellisenseMode) {
                    case ReplIntellisenseMode.AlwaysEvaluate: return true;
                    case ReplIntellisenseMode.NeverEvaluate: return false;
                    case ReplIntellisenseMode.DontEvaluateCalls:
                        var parser = Parser.CreateParser(new StringReader(source), _interpreterFactory.GetLanguageVersion());

                        var stmt = parser.ParseSingleStatement();
                        var exprWalker = new ExprWalker();

                        stmt.Walk(exprWalker);
                        return exprWalker.ShouldExecute;
                    default: throw new InvalidOperationException();
                }
            }
            return false;
        }

        class ExprWalker : PythonWalker {
            public bool ShouldExecute = true;

            public override bool Walk(CallExpression node) {
                ShouldExecute = false;
                return base.Walk(node);
            }
        }

        private static CompletionAnalysis TrySpecialCompletions(ITextSnapshot snapshot, ITrackingSpan span, ITrackingPoint point, CompletionOptions options) {
            var snapSpan = span.GetSpan(snapshot);
            var buffer = snapshot.TextBuffer;
            var classifier = buffer.GetPythonClassifier();
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

            var tokens = classifier.GetClassificationSpans(new SnapshotSpan(start.GetContainingLine().Start, snapSpan.Start));
            if (tokens.Count > 0) {
                // Check for context-sensitive intellisense
                var lastClass = tokens[tokens.Count - 1];

                if (lastClass.ClassificationType == classifier.Provider.Comment) {
                    // No completions in comments
                    return CompletionAnalysis.EmptyCompletionContext;
                } else if (lastClass.ClassificationType == classifier.Provider.StringLiteral) {
                    // String completion
                    if (lastClass.Span.Start.GetContainingLine().LineNumber == lastClass.Span.End.GetContainingLine().LineNumber) {
                        return new StringLiteralCompletionList(span, buffer, options);
                    } else {
                        // multi-line string, no string completions.
                        return CompletionAnalysis.EmptyCompletionContext;
                    }
                } else if (lastClass.ClassificationType == classifier.Provider.Operator &&
                    lastClass.Span.GetText() == "@") {

                    return new DecoratorCompletionAnalysis(span, buffer, options);
                } else if (CompletionAnalysis.IsKeyword(lastClass, "raise") || CompletionAnalysis.IsKeyword(lastClass, "except")) {
                    return new ExceptionCompletionAnalysis(span, buffer, options);
                } else if (CompletionAnalysis.IsKeyword(lastClass, "def")) {
                    return new OverrideCompletionAnalysis(span, buffer, options);
                }

                // Import completions
                var first = tokens[0];
                if (CompletionAnalysis.IsKeyword(first, "import")) {
                    return ImportCompletionAnalysis.Make(tokens, span, buffer, options);
                } else if (CompletionAnalysis.IsKeyword(first, "from")) {
                    return FromImportCompletionAnalysis.Make(tokens, span, buffer, options);
                }
                return null;
            } else if ((tokens = classifier.GetClassificationSpans(snapSpan.Start.GetContainingLine().ExtentIncludingLineBreak)).Count > 0 &&
               tokens[0].ClassificationType == classifier.Provider.StringLiteral) {
                // multi-line string, no string completions.
                return CompletionAnalysis.EmptyCompletionContext;
            } else if (snapshot.IsReplBufferWithCommand()) {
                return CompletionAnalysis.EmptyCompletionContext;
            }

            return null;
        }
#endif

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

            var tokens = classifier.GetClassificationSpans(new SnapshotSpan(start.GetContainingLine().Start, snapSpan.Start));
            if (tokens.Count > 0) {
                // Check for context-sensitive intellisense
                var lastClass = tokens[tokens.Count - 1];

                if (lastClass.ClassificationType == classifier.Provider.Comment) {
                    // No completions in comments
                    return CompletionAnalysis.EmptyCompletionContext;
                } else if (lastClass.ClassificationType == classifier.Provider.StringLiteral) {
                    // String completion
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

#if FALSE
            var parser = new ReverseExpressionParser(snapshot, snapshot.TextBuffer, applicableSpan);
#endif
#if FALSE
            if (parser.IsInGrouping()) {
                options = options.Clone();
                options.IncludeStatementKeywords = false;
            }
#endif

            return new NormalCompletionAnalysis(
                snapshot.TextBuffer.GetAnalyzer(),
                snapshot,
                applicableSpan,
                snapshot.TextBuffer
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

        /// <summary>
        /// Analyzes a complete directory including all of the contained files and packages.
        /// </summary>
        /// <param name="dir">Directory to analyze.</param>
        /// <param name="onFileAnalyzed">If specified, this callback is invoked for every <see cref="IProjectEntry"/>
        /// that is analyzed while analyzing this directory.</param>
        /// <remarks>The callback may be invoked on a thread different from the one that this function was originally invoked on.</remarks>
        public void AnalyzeDirectory(string dir, Action<IProjectEntry> onFileAnalyzed = null) {
            _analysisQueue.Enqueue(new AddDirectoryAnalysis(dir, onFileAnalyzed, this), AnalysisPriority.High);
        }

        class AddDirectoryAnalysis : IAnalyzable {
            private readonly string _dir;
            private readonly Action<IProjectEntry> _onFileAnalyzed;
            private readonly VsProjectAnalyzer _analyzer;

            public AddDirectoryAnalysis(string dir, Action<IProjectEntry> onFileAnalyzed, VsProjectAnalyzer analyzer) {
                _dir = dir;
                _onFileAnalyzed = onFileAnalyzed;
                _analyzer = analyzer;
            }

            #region IAnalyzable Members

            public void Analyze(CancellationToken cancel) {
                if (cancel.IsCancellationRequested) {
                    return;
                }

                _analyzer.AnalyzeDirectoryWorker(_dir, true, _onFileAnalyzed, cancel);
            }

            #endregion
        }

        private void AnalyzeDirectoryWorker(string dir, bool addDir, Action<IProjectEntry> onFileAnalyzed, CancellationToken cancel) {
            if (_jsAnalyzer == null) {
                // We aren't able to analyze code.
                return;
            }

            if (string.IsNullOrEmpty(dir)) {
                Debug.Assert(false, "Unexpected empty dir");
                return;
            }

            if (addDir) {
                lock (_contentsLock) {
                    _jsAnalyzer.AddAnalysisDirectory(dir);
                }
            }

            try {
                foreach (string filename in Directory.GetFiles(dir, "*.py")) {
                    if (cancel.IsCancellationRequested) {
                        break;
                    }
                    IProjectEntry entry = AnalyzeFile(filename);
                    if (onFileAnalyzed != null) {
                        onFileAnalyzed(entry);
                    }
                }
            } catch (IOException) {
                // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
            } catch (UnauthorizedAccessException) {
            }

            try {
                foreach (string filename in Directory.GetFiles(dir, "*.pyw")) {
                    if (cancel.IsCancellationRequested) {
                        break;
                    }
                    IProjectEntry entry = AnalyzeFile(filename);
                    if (onFileAnalyzed != null) {
                        onFileAnalyzed(entry);
                    }
                }
            } catch (IOException) {
                // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
            } catch (UnauthorizedAccessException) {
            }

            try {
                foreach (string innerDir in Directory.GetDirectories(dir)) {
                    if (cancel.IsCancellationRequested) {
                        break;
                    }
                    if (File.Exists(Path.Combine(innerDir, "__init__.py"))) {
                        AnalyzeDirectoryWorker(innerDir, false, onFileAnalyzed, cancel);
                    }
                }
            } catch (IOException) {
                // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
            } catch (UnauthorizedAccessException) {
            }
        }

        internal void StopAnalyzingDirectory(string directory) {
            if (_jsAnalyzer == null) {
                // We aren't able to analyze code.
                return;
            }

            lock (_contentsLock) {
                _jsAnalyzer.RemoveAnalysisDirectory(directory);
            }
        }

        internal void Cancel() {
            _analysisQueue.Stop();
        }

        internal void UnloadFile(IProjectEntry entry) {
            if (_jsAnalyzer == null) {
                // We aren't able to analyze code.
                return;
            }

            if (entry != null) {
                // If we remove a Node.js module, reanalyze any other modules
                // that referenced it.
#if FALSE
                var pyEntry = entry as IJsProjectEntry;
                IJsProjectEntry[] reanalyzeEntries = null;
                if (pyEntry != null && !string.IsNullOrEmpty(pyEntry.ModuleName)) {
                    reanalyzeEntries = _pyAnalyzer.GetEntriesThatImportModule(pyEntry.ModuleName, false).ToArray();
                }
#endif

                ClearParserTasks(entry);
                _analysisQueue.Enqueue(_jsAnalyzer.RemoveModule(entry), AnalysisPriority.Normal);
                IProjectEntry removed;
                _projectFiles.TryRemove(entry.FilePath, out removed);
#if FALSE
                if (reanalyzeEntries != null) {
                    foreach (var existing in reanalyzeEntries) {
                        _analysisQueue.Enqueue(existing, AnalysisPriority.Normal);
                    }
                }
#endif
            }
        }

        internal void RemoveErrors(IProjectEntry entry, bool suppressUpdate) {
            if (entry != null && entry.FilePath != null) {
                if (_taskProvider.IsValueCreated) {
                    // _taskProvider may not be created if we've never opened a Node.js file and
                    // none of the project files have errors
                    //_taskProvider.Value.Clear(entry.FilePath, !suppressUpdate);
                }
                OnWarningRemoved(entry.FilePath);
                OnErrorRemoved(entry.FilePath);
            }
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

        internal EventHandler<FileEventArgs> WarningAdded;
        internal EventHandler<FileEventArgs> WarningRemoved;
        internal EventHandler<FileEventArgs> ErrorAdded;
        internal EventHandler<FileEventArgs> ErrorRemoved;

        internal void ClearParserTasks(IProjectEntry entry) {
            if (entry != null) {
                if (TaskProvider.IsValueCreated) {
                    // TaskProvider may not be created if we've never opened a
                    // Node.js file and none of the project files have errors
                    TaskProvider.Value.Clear(entry, ParserTaskMoniker);
                }
                bool changed;
                lock (_hasParseErrors) {                    
                    changed = _hasParseErrors.Remove(entry);
                }

                if (changed) {
                    OnShouldWarnOnLaunchChanged(entry);
                }
            }
        }

        internal void ClearAllTasks() {
            if (TaskProvider.IsValueCreated) {
                TaskProvider.Value.ClearAll();
            }
            lock (_hasParseErrors) {
                _hasParseErrors.Clear();
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
            if (TaskProvider.IsValueCreated) {
                foreach (var entry in _projectFiles.Values) {
                    TaskProvider.Value.Clear(entry, ParserTaskMoniker);
                    TaskProvider.Value.Clear(entry, UnresolvedImportMoniker);
                }
            }

            _analysisQueue.Stop();

            if (_projectDir != null) {
                ThreadPool.QueueUserWorkItem(_ => SaveAnalysis());
            }
        }

        private void SaveAnalysis() {
            try {
                using (FileStream fs = new FileStream(GetAnalysisPath(), FileMode.Create)) {
                    fs.Write(_dbHeader, 0, _dbHeader.Length);
                    try {
                        var serializer = new AnalysisSerializer();
                        serializer.Serialize(fs, _jsAnalyzer);
                        _analysisQueue.Serialize(serializer, fs);
                    } catch (Exception e) {
                        Debug.Fail("Failed to save analysis " + e);
                    }
                }
                new FileInfo(GetAnalysisPath()).Attributes |= FileAttributes.Hidden;
            } catch (UnauthorizedAccessException) {
            }
        }

        #endregion
    }
}

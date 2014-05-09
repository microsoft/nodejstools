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
using System.Windows;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Formatting;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;

namespace Microsoft.NodejsTools {
    internal sealed class EditFilter : IOleCommandTarget {
        private readonly ITextView _textView;
        private readonly IEditorOperations _editorOps;
        private readonly IIntellisenseSessionStack _intellisenseStack;
        private readonly IComponentModel _compModel;
        private readonly IClassifier _classifier;
        private readonly IIncrementalSearch _incSearch;
        private readonly ICompletionBroker _broker;
        private readonly IEditorOptions _editorOptions;
        private IOleCommandTarget _next;
        private ICompletionSession _activeSession;

        public EditFilter(ITextView textView, IEditorOperations editorOps, IEditorOptions editorOptions, IIntellisenseSessionStack intellisenseStack, IComponentModel compModel) {
            _textView = textView;
            _editorOps = editorOps;            
            _intellisenseStack = intellisenseStack;
            _compModel = compModel;
            var agg = _compModel.GetService<IClassifierAggregatorService>();
            _classifier = agg.GetClassifier(textView.TextBuffer);
            _incSearch = _compModel.GetService<IIncrementalSearchFactoryService>().GetIncrementalSearch(_textView);
            _broker = _compModel.GetService<ICompletionBroker>();
            _editorOptions = editorOptions;
            textView.Closed += TextViewClosed;
            string path = GetFilePath();
            if (path != null) {
                System.Diagnostics.Debug.Assert(JavaScriptFormattingService.Instance != null);
                JavaScriptFormattingService.Instance.AddDocument(path, textView.TextBuffer);
            }
        }

        private void TextViewClosed(object sender, EventArgs e) {
            string path = GetFilePath();
            if (path != null) {
                JavaScriptFormattingService.Instance.RemoveDocument(path);
            }
        }

        private bool ShouldTriggerRequireIntellisense() {
            return CompletionSource.ShouldTriggerRequireIntellisense(
                _textView.Caret.Position.BufferPosition,
                _classifier,
                false
            );
        }

        internal void AttachKeyboardFilter(IVsTextView vsTextView) {
            if (_next == null) {
                ErrorHandler.ThrowOnFailure(vsTextView.AddCommandFilter(this, out _next));
            }
        }

        private void OnCompletionSessionDismissedOrCommitted(object sender, System.EventArgs e) {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _activeSession.Committed -= OnCompletionSessionDismissedOrCommitted;
            _activeSession.Dismissed -= OnCompletionSessionDismissedOrCommitted;
            _activeSession = null;
        }


        private static HashSet<char> _commitChars = new HashSet<char>("{}[](),:;+-*%&|^~=<>#\\");

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            int hr;
            // disable JavaScript language services auto formatting features, this is because
            // they are not aware that we have an extra level of indentation
            if (pguidCmdGroup == VSConstants.VSStd2K) {
                switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                    case VSConstants.VSStd2KCmdID.FORMATSELECTION: FormatSelection(); return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.FORMATDOCUMENT: FormatDocument(); return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.RETURN:
                        if (_intellisenseStack.TopSession != null && 
                            _intellisenseStack.TopSession is ICompletionSession &&
                            !_intellisenseStack.TopSession.IsDismissed) {
                            ((ICompletionSession)_intellisenseStack.TopSession).Commit();
                        } else {
                            hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            FormatAfterTyping('\n');
                            return hr;
                        }
                        return VSConstants.S_OK;
                    case VSConstants.VSStd2KCmdID.TYPECHAR:
                        if (!_incSearch.IsActive) {
                            var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);
                            int res = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                            switch(ch) {
                                case '}':
                                case ':':
                                    FormatAfterTyping(ch);
                                    break;
                            }

                            if (_activeSession != null && !_activeSession.IsDismissed) {
                                _activeSession.Filter();
                            }

                            return res;
                        }
                        break;
                    case VSConstants.VSStd2KCmdID.PASTE: 
                        var curVersion = _textView.TextBuffer.CurrentSnapshot;
                        hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                        if (ErrorHandler.Succeeded(hr)) {
                            FormatAfterPaste(curVersion);
                        }
                        return hr;
                }
            } else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch ((VSConstants.VSStd97CmdID)nCmdID) {
                    case VSConstants.VSStd97CmdID.GotoDefn: return GotoDefinition();
                    case VSConstants.VSStd97CmdID.FindReferences: return FindAllReferences();
                    case VSConstants.VSStd97CmdID.Paste:
                        var curVersion = _textView.TextBuffer.CurrentSnapshot;
                        hr = _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                        if (ErrorHandler.Succeeded(hr)) {
                            FormatAfterPaste(curVersion);
                        }
                        return hr;
                }
            }

            return _next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// Implements Goto Definition.  Called when the user selects Goto Definition from the 
        /// context menu or hits the hotkey associated with Goto Definition.
        /// 
        /// If there is 1 and only one definition immediately navigates to it.  If there are
        /// no references displays a dialog box to the user.  Otherwise it opens the find
        /// symbols dialog with the list of results.
        /// </summary>
        private int GotoDefinition() {
            UpdateStatusForIncompleteAnalysis();

            var analysis = AnalyzeExpression();

            Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
            GetDefsRefsAndValues(analysis, out definitions, out references, out values);

            if ((values.Count + definitions.Count) == 1) {
                if (values.Count != 0) {
                    foreach (var location in values.Keys) {
                        GotoLocation(location);
                        break;
                    }
                } else {
                    foreach (var location in definitions.Keys) {
                        GotoLocation(location);
                        break;
                    }
                }
            } else if (values.Count + definitions.Count == 0) {
                if (String.IsNullOrWhiteSpace(analysis.Expression)) {
                    MessageBox.Show(String.Format("Cannot go to definition.  The cursor is not on a symbol."), "Python Tools for Visual Studio");
                } else {
                    MessageBox.Show(String.Format("Cannot go to definition \"{0}\"", analysis.Expression), "Python Tools for Visual Studio");
                }
            } else if (definitions.Count == 0) {
                ShowFindSymbolsDialog(analysis, new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values));
            } else if (values.Count == 0) {
                ShowFindSymbolsDialog(analysis, new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values));
            } else {
                ShowFindSymbolsDialog(analysis,
                    new LocationCategory("Goto Definition",
                        new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
                        new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values)
                    )
                );
            }

            return VSConstants.S_OK;
        }

        private ExpressionAnalysis AnalyzeExpression() {
            return VsProjectAnalyzer.AnalyzeExpression(
                _textView.TextBuffer.CurrentSnapshot,
                _textView.GetCaretSpan(),
                false
            );
        }

        /// <summary>
        /// Moves the caret to the specified location, staying in the current text view 
        /// if possible.
        /// 
        /// https://pytools.codeplex.com/workitem/1649
        /// </summary>
        private void GotoLocation(LocationInfo location) {
            Debug.Assert(location != null);
            Debug.Assert(location.Line > 0);
            Debug.Assert(location.Column > 0);

            if (CommonUtils.IsSamePath(location.FilePath, _textView.GetFilePath())) {
                var adapterFactory = NodejsPackage.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
                var viewAdapter = adapterFactory.GetViewAdapter(_textView);
                viewAdapter.SetCaretPos(location.Line - 1, location.Column - 1);
                viewAdapter.CenterLines(location.Line - 1, 1);
            } else {
                location.GotoSource();
            }
        }

        /// <summary>
        /// Implements Find All References.  Called when the user selects Find All References from
        /// the context menu or hits the hotkey associated with find all references.
        /// 
        /// Always opens the Find Symbol Results box to display the results.
        /// </summary>
        private int FindAllReferences() {
            UpdateStatusForIncompleteAnalysis();

            var analysis = AnalyzeExpression();

            var locations = GetFindRefLocations(analysis);

            ShowFindSymbolsDialog(analysis, locations);

            return VSConstants.S_OK;
        }

        internal static LocationCategory GetFindRefLocations(ExpressionAnalysis analysis) {
            Dictionary<LocationInfo, SimpleLocationInfo> references, definitions, values;
            GetDefsRefsAndValues(analysis, out definitions, out references, out values);

            var locations = new LocationCategory("Find All References",
                    new SymbolList("Definitions", StandardGlyphGroup.GlyphLibrary, definitions.Values),
                    new SymbolList("Values", StandardGlyphGroup.GlyphForwardType, values.Values),
                    new SymbolList("References", StandardGlyphGroup.GlyphReference, references.Values)
                );
            return locations;
        }

        private static void GetDefsRefsAndValues(ExpressionAnalysis provider, out Dictionary<LocationInfo, SimpleLocationInfo> definitions, out Dictionary<LocationInfo, SimpleLocationInfo> references, out Dictionary<LocationInfo, SimpleLocationInfo> values) {
            references = new Dictionary<LocationInfo, SimpleLocationInfo>();
            definitions = new Dictionary<LocationInfo, SimpleLocationInfo>();
            values = new Dictionary<LocationInfo, SimpleLocationInfo>();

            foreach (var v in provider.Variables) {
                if (v.Location.FilePath == null) {
                    // ignore references in the REPL
                    continue;
                }

                switch (v.Type) {
                    case VariableType.Definition:
                        values.Remove(v.Location);
                        definitions[v.Location] = new SimpleLocationInfo(provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField);
                        break;
                    case VariableType.Reference:
                        references[v.Location] = new SimpleLocationInfo(provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField);
                        break;
                    case VariableType.Value:
                        if (!definitions.ContainsKey(v.Location)) {
                            values[v.Location] = new SimpleLocationInfo(provider.Expression, v.Location, StandardGlyphGroup.GlyphGroupField);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Opens the find symbols dialog with a list of results.  This is done by requesting
        /// that VS does a search against our library GUID.  Our library then responds to
        /// that request by extracting the prvoided symbol list out and using that for the
        /// search results.
        /// </summary>
        private static void ShowFindSymbolsDialog(ExpressionAnalysis provider, IVsNavInfo symbols) {
            // ensure our library is loaded so find all references will go to our library
            //Package.GetGlobalService(typeof(IPythonLibraryManager));

            if (provider.Expression != "") {
                var findSym = (IVsFindSymbol)NodejsPackage.GetGlobalService(typeof(SVsObjectSearch));
                VSOBSEARCHCRITERIA2 searchCriteria = new VSOBSEARCHCRITERIA2();
                searchCriteria.eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD;
                searchCriteria.pIVsNavInfo = symbols;
                searchCriteria.grfOptions = (uint)_VSOBSEARCHOPTIONS2.VSOBSO_LISTREFERENCES;
                searchCriteria.szName = provider.Expression;

                Guid guid = Guid.Empty;
                //  new Guid("{a5a527ea-cf0a-4abf-b501-eafe6b3ba5c6}")
                ErrorHandler.ThrowOnFailure(findSym.DoSearch(new Guid(CommonConstants.LibraryGuid), new VSOBSEARCHCRITERIA2[] { searchCriteria }));
            } else {
                var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
                statusBar.SetText("The caret must be on valid expression to find all references.");
            }
        }



        internal class LocationCategory : SimpleObjectList<SymbolList>, IVsNavInfo, ICustomSearchListProvider {
            private readonly string _name;

            internal LocationCategory(string name, params SymbolList[] locations) {
                _name = name;

                foreach (var location in locations) {
                    if (location.Children.Count > 0) {
                        Children.Add(location);
                    }
                }
            }

            public override uint CategoryField(LIB_CATEGORY lIB_CATEGORY) {
                return (uint)(_LIB_LISTTYPE.LLT_HIERARCHY | _LIB_LISTTYPE.LLT_MEMBERS | _LIB_LISTTYPE.LLT_PACKAGE);
            }

            #region IVsNavInfo Members

            public int EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SymbolList>(Children);
                return VSConstants.S_OK;
            }

            public int EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SymbolList>(Children);
                return VSConstants.S_OK;
            }

            public int GetLibGuid(out Guid pGuid) {
                pGuid = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetSymbolType(out uint pdwType) {
                pdwType = (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion

            #region ICustomSearchListProvider Members

            public IVsSimpleObjectList2 GetSearchList() {
                return this;
            }

            #endregion
        }

        internal class SimpleLocationInfo : SimpleObject, IVsNavInfoNode {
            private readonly LocationInfo _locationInfo;
            private readonly StandardGlyphGroup _glyphType;
            private readonly string _pathText, _lineText;

            public SimpleLocationInfo(string searchText, LocationInfo locInfo, StandardGlyphGroup glyphType) {
                _locationInfo = locInfo;
                _glyphType = glyphType;
                _pathText = GetSearchDisplayText();
                _lineText = _locationInfo.ProjectEntry.GetLine(_locationInfo.Line);
            }

            public override string Name {
                get {
                    return _locationInfo.FilePath;
                }
            }

            public override string GetTextRepresentation(VSTREETEXTOPTIONS options) {
                if (options == VSTREETEXTOPTIONS.TTO_DEFAULT) {
                    return _pathText + _lineText.Trim();
                }
                return String.Empty;
            }

            private string GetSearchDisplayText() {
                return String.Format("{0} - ({1}, {2}): ",
                    _locationInfo.FilePath,
                    _locationInfo.Line,
                    _locationInfo.Column);
            }

            public override string UniqueName {
                get {
                    return _locationInfo.FilePath;
                }
            }

            public override bool CanGoToSource {
                get {
                    return true;
                }
            }

            public override VSTREEDISPLAYDATA DisplayData {
                get {
                    var res = new VSTREEDISPLAYDATA();
                    res.Image = res.SelectedImage = (ushort)_glyphType;
                    res.State = (uint)_VSTREEDISPLAYSTATE.TDS_FORCESELECT;

                    // This code highlights the text but it gets the wrong region.  This should be re-enabled
                    // and highlight the correct region.

                    //res.ForceSelectStart = (ushort)(_pathText.Length + _locationInfo.Column - 1);
                    //res.ForceSelectLength = (ushort)_locationInfo.Length;
                    return res;
                }
            }

            public override void GotoSource(VSOBJGOTOSRCTYPE SrcType) {
                _locationInfo.GotoSource();
            }

            #region IVsNavInfoNode Members

            public int get_Name(out string pbstrName) {
                pbstrName = _locationInfo.FilePath;
                return VSConstants.S_OK;
            }

            public int get_Type(out uint pllt) {
                pllt = 16; // (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion
        }

        internal class SymbolList : SimpleObjectList<SimpleLocationInfo>, IVsNavInfo, IVsNavInfoNode, ICustomSearchListProvider, ISimpleObject {
            private readonly string _name;
            private readonly StandardGlyphGroup _glyphGroup;

            internal SymbolList(string description, StandardGlyphGroup glyphGroup, IEnumerable<SimpleLocationInfo> locations) {
                _name = description;
                _glyphGroup = glyphGroup;
                Children.AddRange(locations);
            }

            public override uint CategoryField(LIB_CATEGORY lIB_CATEGORY) {
                return (uint)(_LIB_LISTTYPE.LLT_MEMBERS | _LIB_LISTTYPE.LLT_PACKAGE);
            }

            #region ISimpleObject Members

            public bool CanDelete {
                get { return false; }
            }

            public bool CanGoToSource {
                get { return false; }
            }

            public bool CanRename {
                get { return false; }
            }

            public string Name {
                get { return _name; }
            }

            public string UniqueName {
                get { return _name; }
            }

            public string FullName {
                get {
                    return _name;
                }
            }

            public string GetTextRepresentation(VSTREETEXTOPTIONS options) {
                switch (options) {
                    case VSTREETEXTOPTIONS.TTO_DISPLAYTEXT:
                        return _name;
                }
                return null;
            }

            public string TooltipText {
                get { return null; }
            }

            public object BrowseObject {
                get { return null; }
            }

            public System.ComponentModel.Design.CommandID ContextMenuID {
                get { return null; }
            }

            public VSTREEDISPLAYDATA DisplayData {
                get {
                    var res = new VSTREEDISPLAYDATA();
                    res.Image = res.SelectedImage = (ushort)_glyphGroup;
                    return res;
                }
            }

            public void Delete() {
            }

            public void DoDragDrop(OleDataObject dataObject, uint grfKeyState, uint pdwEffect) {
            }

            public void Rename(string pszNewName, uint grfFlags) {
            }

            public void GotoSource(VSOBJGOTOSRCTYPE SrcType) {
            }

            public void SourceItems(out IVsHierarchy ppHier, out uint pItemid, out uint pcItems) {
                ppHier = null;
                pItemid = 0;
                pcItems = 0;
            }

            public uint EnumClipboardFormats(_VSOBJCFFLAGS _VSOBJCFFLAGS, VSOBJCLIPFORMAT[] rgcfFormats) {
                return VSConstants.S_OK;
            }

            public void FillDescription(_VSOBJDESCOPTIONS _VSOBJDESCOPTIONS, IVsObjectBrowserDescription3 pobDesc) {
            }

            public IVsSimpleObjectList2 FilterView(uint ListType) {
                return this;
            }

            #endregion

            #region IVsNavInfo Members

            public int EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SimpleLocationInfo>(Children);
                return VSConstants.S_OK;
            }

            public int EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<SimpleLocationInfo>(Children);
                return VSConstants.S_OK;
            }

            public int GetLibGuid(out Guid pGuid) {
                pGuid = Guid.Empty;
                return VSConstants.S_OK;
            }

            public int GetSymbolType(out uint pdwType) {
                pdwType = (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion

            #region ICustomSearchListProvider Members

            public IVsSimpleObjectList2 GetSearchList() {
                return this;
            }

            #endregion

            #region IVsNavInfoNode Members

            public int get_Name(out string pbstrName) {
                pbstrName = "name";
                return VSConstants.S_OK;
            }

            public int get_Type(out uint pllt) {
                pllt = 16; // (uint)_LIB_LISTTYPE2.LLT_MEMBERHIERARCHY;
                return VSConstants.S_OK;
            }

            #endregion
        }

        class NodeEnumerator<T> : IVsEnumNavInfoNodes where T : IVsNavInfoNode {
            private readonly List<T> _locations;
            private IEnumerator<T> _locationEnum;

            public NodeEnumerator(List<T> locations) {
                _locations = locations;
                Reset();
            }

            #region IVsEnumNavInfoNodes Members

            public int Clone(out IVsEnumNavInfoNodes ppEnum) {
                ppEnum = new NodeEnumerator<T>(_locations);
                return VSConstants.S_OK;
            }

            public int Next(uint celt, IVsNavInfoNode[] rgelt, out uint pceltFetched) {
                pceltFetched = 0;
                while (celt-- != 0 && _locationEnum.MoveNext()) {
                    rgelt[pceltFetched++] = _locationEnum.Current;
                }
                return VSConstants.S_OK;
            }

            public int Reset() {
                _locationEnum = _locations.GetEnumerator();
                return VSConstants.S_OK;
            }

            public int Skip(uint celt) {
                while (celt-- != 0) {
                    _locationEnum.MoveNext();
                }
                return VSConstants.S_OK;
            }

            #endregion
        }

        private void UpdateStatusForIncompleteAnalysis() {
            var statusBar = (IVsStatusbar)CommonPackage.GetGlobalService(typeof(SVsStatusbar));
            var analyzer = _textView.GetAnalyzer();
            if (analyzer != null && analyzer.IsAnalyzing) {
                statusBar.SetText("Python source analysis is not up to date");
            }
        }

        private void Dismiss() {
            while (_intellisenseStack.TopSession != null) {
                _intellisenseStack.TopSession.Dismiss();
            }
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd97CmdID.GotoDefn:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd97CmdID.FindReferences:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            } else if (pguidCmdGroup == VSConstants.VSStd2K) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                        case VSConstants.VSStd2KCmdID.FORMATSELECTION:
                            string path = GetFilePath();
                            if (path != null) {
                                prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                                return VSConstants.S_OK;
                            }
                            break;
                    }
                }
            }
            return _next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #region Formatting support

        private FormatCodeOptions CreateFormatOptions() {
            FormatCodeOptions res = new FormatCodeOptions();
            res.TabSize = _editorOptions.GetTabSize();
            res.IndentSize = _editorOptions.GetIndentSize();
            res.ConvertTabsToSpaces = _editorOptions.IsConvertTabsToSpacesEnabled();
            res.NewLineCharacter = _editorOptions.GetNewLineCharacter();

            res.InsertSpaceAfterCommaDelimiter = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterComma;
            res.InsertSpaceAfterSemicolonInForStatements = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterSemicolonInFor;
            res.InsertSpaceBeforeAndAfterBinaryOperators = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceBeforeAndAfterBinaryOperator;
            res.InsertSpaceAfterKeywordsInControlFlowStatements = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterKeywordsInControlFlow;
            res.InsertSpaceAfterFunctionKeywordForAnonymousFunctions = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterFunctionKeywordForAnonymousFunctions;
            res.InsertSpaceAfterOpeningAndBeforeClosingNonemptyParenthesis = NodejsPackage.Instance.FormattingSpacingOptionsPage.SpaceAfterOpeningAndBeforeClosingNonEmptyParens;

            res.PlaceOpenBraceOnNewLineForFunctions = NodejsPackage.Instance.FormattingBracesOptionsPage.BraceOnNewLineForFunctions;
            res.PlaceOpenBraceOnNewLineForControlBlocks = NodejsPackage.Instance.FormattingBracesOptionsPage.BraceOnNewLineForControlBlocks;

            return res;
        }

        private void FormatSelection() {
            string path = GetFilePath();
            if (path != null) {
                ApplyEdits(
                    _textView.TextBuffer,
                    JavaScriptFormattingService.Instance.GetFormattingEditsForRange(
                        path,
                        _textView.Selection.Start.Position,
                        _textView.Selection.End.Position,
                        CreateFormatOptions()
                    )
                );
            }
        }

        private void FormatDocument() {
            string path;
            path = GetFilePath();
            if (path != null) {
                ApplyEdits(
                    _textView.TextBuffer,
                    JavaScriptFormattingService.Instance.GetFormattingEditsForDocument(
                        path,
                        0,
                        _textView.TextBuffer.CurrentSnapshot.Length,
                        CreateFormatOptions()
                    )
                );
            }
        }

        private void FormatAfterTyping(char ch) {
            if (ShouldFormatOnCharacter(ch)) {
                string path = GetFilePath();
                if (path != null) {
                    ApplyEdits(
                        _textView.TextBuffer,
                        JavaScriptFormattingService.Instance.GetFormattingEditsAfterKeystroke(
                            path,
                            _textView.Caret.Position.BufferPosition.Position,
                            ch.ToString(),
                            CreateFormatOptions()
                        )
                    );
                }
            }
        }

        private bool ShouldFormatOnCharacter(char ch) {
#if FALSE
            switch (ch) {
                case '}':
                    return NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnCloseBrace;
                case ';':
                    return NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnSemiColon;
                case '\n':
                    return NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnEnter;
            }
#endif
            return false;
        }

        private void FormatAfterPaste(ITextSnapshot curVersion) {
            if (NodejsPackage.Instance.FormattingGeneralOptionsPage.FormatOnPaste) {
                // calculate the range for the paste...
                var afterVersion = _textView.TextBuffer.CurrentSnapshot;
                int start = afterVersion.Length, end = 0;
                for (var version = curVersion.Version;
                    version != afterVersion.Version;
                    version = version.Next) {
                    foreach (var change in version.Changes) {
                        int oldStart = version.CreateTrackingPoint(
                            change.OldPosition,
                            PointTrackingMode.Negative
                        ).GetPosition(afterVersion.Version);

                        start = Math.Min(oldStart, start);

                        int newEnd = version.Next.CreateTrackingPoint(
                            change.NewSpan.End,
                            PointTrackingMode.Positive
                        ).GetPosition(afterVersion.Version);

                        end = Math.Max(newEnd, end);
                    }
                }

                if (start < end) {
                    // then format it
                    string path = GetFilePath();
                    if (path != null) {
                        ApplyEdits(
                            _textView.TextBuffer,
                            JavaScriptFormattingService.Instance.GetFormattingEditsOnPaste(
                                path,
                                start,
                                end,
                                CreateFormatOptions()
                            )
                        );
                    }
                }
            }
        }

        private string GetFilePath() {
            return _textView.TextBuffer.GetFilePath();
        }

        internal static void ApplyEdits(ITextBuffer buffer, TextEdit[] edits) {
            using (var edit = buffer.CreateEdit()) {
                foreach (var toApply in edits) {
                    edit.Replace(
                        Span.FromBounds(toApply.MinChar, toApply.LimChar),
                        toApply.Text
                    );
                }
                edit.Apply();
            }
        }

        #endregion

    }
}

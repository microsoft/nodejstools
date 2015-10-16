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
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Editor.Core;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.IncrementalSearch;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.NodejsTools.Intellisense {

    internal sealed class IntellisenseController : IIntellisenseController, IOleCommandTarget {
        private readonly ITextView _textView;
        private readonly IntellisenseControllerProvider _provider;
        private readonly IIncrementalSearch _incSearch;
        private readonly ExpansionClient _expansionClient;
        private readonly System.IServiceProvider _serviceProvider;
        private readonly IVsExpansionManager _expansionMgr;
        private readonly IClassifier _classifier;
        private ICompletionSession _activeSession;
        private ISignatureHelpSession _sigHelpSession;
        private IQuickInfoSession _quickInfoSession;
        private IOleCommandTarget _oldTarget;
        private IEditorOperations _editOps;
        private static string[] _allStandardSnippetTypes = { ExpansionClient.Expansion, ExpansionClient.SurroundsWith };
        private static string[] _surroundsWithSnippetTypes = { ExpansionClient.SurroundsWith };
        [ThreadStatic]
        internal static bool ForceCompletions;

        /// <summary>
        /// Attaches events for invoking Statement completion 
        /// </summary>
        public IntellisenseController(IntellisenseControllerProvider provider, ITextView textView, System.IServiceProvider serviceProvider) {
            _textView = textView;
            _provider = provider;
            _classifier = _provider._classifierAgg.GetClassifier(_textView.TextBuffer);
            _editOps = provider._EditOperationsFactory.GetEditorOperations(textView);
            _incSearch = provider._IncrementalSearch.GetIncrementalSearch(textView);
            _textView.MouseHover += TextViewMouseHover;
            _serviceProvider = serviceProvider;

            if (textView.TextBuffer.IsNodeJsContent()) {
                try {
                    _expansionClient = new ExpansionClient(textView, provider._adaptersFactory, _serviceProvider);
                    var textMgr = (IVsTextManager2)_serviceProvider.GetService(typeof(SVsTextManager));
                    textMgr.GetExpansionManager(out _expansionMgr);
                } catch (ArgumentException ex) {
                    // No expansion client for this buffer, but we can continue without it
                    Debug.Fail(ex.ToString());
                }
            }
            textView.Properties.AddProperty(typeof(IntellisenseController), this);  // added so our key processors can get back to us
        }

        private void TextViewMouseHover(object sender, MouseHoverEventArgs e) {
            if (_quickInfoSession != null && !_quickInfoSession.IsDismissed) {
                _quickInfoSession.Dismiss();
            }
            var pt = e.TextPosition.GetPoint(EditorExtensions.IsNodeJsContent, PositionAffinity.Successor);
            if (pt != null) {
                _quickInfoSession = _provider._QuickInfoBroker.TriggerQuickInfo(
                    _textView,
                    pt.Value.Snapshot.CreateTrackingPoint(pt.Value.Position, PointTrackingMode.Positive),
                    true);
            }
        }

        internal void TriggerQuickInfo() {
            if (_quickInfoSession != null && !_quickInfoSession.IsDismissed) {
                _quickInfoSession.Dismiss();
            }
            _quickInfoSession = _provider._QuickInfoBroker.TriggerQuickInfo(_textView);
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer) {
            subjectBuffer.GetAnalyzer().AddBuffer(subjectBuffer);
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer) {
            subjectBuffer.GetAnalyzer().RemoveBuffer(subjectBuffer);
        }

        /// <summary>
        /// Detaches the events
        /// </summary>
        /// <param name="textView"></param>
        public void Detach(ITextView textView) {
            if (_textView == null) {
                throw new InvalidOperationException("Already detached from text view");
            }
            if (textView != _textView) {
                throw new ArgumentException("Not attached to specified text view", "textView");
            }

            _textView.MouseHover -= TextViewMouseHover;
            _textView.Properties.RemoveProperty(typeof(IntellisenseController));

            DetachKeyboardFilter();
        }

        /// <summary>
        /// Triggers Statement completion when appropriate keys are pressed
        /// The key combination is CTRL-J or "."
        /// The intellisense window is dismissed when one presses ESC key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreprocessKeyDown(object sender, TextCompositionEventArgs e) {
            // We should only receive pre-process events from our text view
            Debug.Assert(sender == _textView);

            // TODO: We should handle = for signature completion of keyword arguments

            string text = e.Text;
            if (text.Length == 1) {
                HandleChar(text[0]);
            }
        }

        private void HandleChar(char ch) {
            // We trigger completions when the user types . or space.  Called via our IOleCommandTarget filter
            // on the text view.
            //
            // We trigger signature help when we receive a "(".  We update our current sig when 
            // we receive a "," and we close sig help when we receive a ")".

            if (!_incSearch.IsActive) {
                switch (ch) {
                    case '.':
                    case ' ':
                        if (NodejsPackage.Instance.LangPrefs.AutoListMembers) {
                            TriggerCompletionSession(false);
                        }
                        break;
                    case '/':
                    case '\'':
                    case '"':
                        if (CompletionSource.ShouldTriggerRequireIntellisense(_textView.Caret.Position.BufferPosition, _classifier, true, true)) {
                            TriggerCompletionSession(false);
                        }
                        break;
                    case '(':
                        if (CompletionSource.ShouldTriggerRequireIntellisense(_textView.Caret.Position.BufferPosition, _classifier, true)) {
                            TriggerCompletionSession(false);
                        } else if (NodejsPackage.Instance.LangPrefs.AutoListParams) {
                            OpenParenStartSignatureSession();
                        }
                        break;
                    case ')':
                        if (_sigHelpSession != null) {
                            _sigHelpSession.Dismiss();
                            _sigHelpSession = null;
                        }

                        if (NodejsPackage.Instance.LangPrefs.AutoListParams) {
                            // trigger help for outer call if there is one
                            TriggerSignatureHelp();
                        }
                        break;
                    case '=':
                    case ',':
                        if (_sigHelpSession == null) {
                            if (NodejsPackage.Instance.LangPrefs.AutoListParams) {
                                CommaStartSignatureSession();
                            }
                        } else {
                            UpdateCurrentParameter();
                        }
                        break;
                    default:
                        if (IsIdentifierFirstChar(ch) && _activeSession == null
                            && NodejsPackage.Instance.LangPrefs.AutoListMembers
                            && NodejsPackage.Instance.IntellisenseOptionsPage.ShowCompletionListAfterCharacterTyped) {
                            TriggerCompletionSession(false);
                        }
                        break;
                }
            }
        }

        private bool Backspace() {
            if (_sigHelpSession != null) {
                if (_textView.Selection.IsActive && !_textView.Selection.IsEmpty) {
                    // when deleting a selection don't do anything to pop up signature help again
                    _sigHelpSession.Dismiss();
                    return false;
                }

                SnapshotPoint? caretPoint = _textView.BufferGraph.MapDownToFirstMatch(
                    _textView.Caret.Position.BufferPosition,
                    PointTrackingMode.Positive,
                    EditorExtensions.IsNodeJsContent,
                    PositionAffinity.Predecessor
                );

                if (caretPoint != null && caretPoint.Value.Position != 0) {
                    var deleting = caretPoint.Value.Snapshot[caretPoint.Value.Position - 1];
                    if (deleting == ',') {
                        caretPoint.Value.Snapshot.TextBuffer.Delete(new Span(caretPoint.Value.Position - 1, 1));
                        UpdateCurrentParameter();
                        return true;
                    } else if (deleting == '(' || deleting == ')') {
                        _sigHelpSession.Dismiss();
                        // delete the ( before triggering help again
                        caretPoint.Value.Snapshot.TextBuffer.Delete(new Span(caretPoint.Value.Position - 1, 1));

                        // Pop to an outer nesting of signature help
                        if (NodejsPackage.Instance.LangPrefs.AutoListParams) {
                            TriggerSignatureHelp();
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        private void OpenParenStartSignatureSession() {
            if (_activeSession != null) {
                _activeSession.Dismiss();
            }
            if (_sigHelpSession != null) {
                _sigHelpSession.Dismiss();
            }

            TriggerSignatureHelp();
        }

        private void CommaStartSignatureSession() {
            TriggerSignatureHelp();
        }

        /// <summary>
        /// Updates the current parameter for the caret's current position.
        /// 
        /// This will analyze the buffer for where we are currently located, find the current
        /// parameter that we're entering, and then update the signature.  If our current
        /// signature does not have enough parameters we'll find a signature which does.
        /// </summary>
        private void UpdateCurrentParameter() {
            if (_sigHelpSession == null) {
                // we moved out of the original span for sig help, re-trigger based upon the position
                TriggerSignatureHelp();
                return;
            }

            int position = _textView.Caret.Position.BufferPosition.Position;
            // we advance to the next parameter
            // TODO: need to parse and see if we have keyword arguments entered into the current signature yet
            NodejsSignature sig = _sigHelpSession.SelectedSignature as NodejsSignature;
            if (sig != null) {
                var prevBuffer = sig.ApplicableToSpan.TextBuffer;
                var textBuffer = _textView.TextBuffer;

                var targetPt = _textView.BufferGraph.MapDownToFirstMatch(
                    new SnapshotPoint(_textView.TextBuffer.CurrentSnapshot, position),
                    PointTrackingMode.Positive,
                    EditorExtensions.IsNodeJsContent,
                    PositionAffinity.Successor
                );

                if (targetPt != null) {
                    var span = targetPt.Value.Snapshot.CreateTrackingSpan(targetPt.Value.Position, 0, SpanTrackingMode.EdgeInclusive);

                    var sigs = VsProjectAnalyzer.GetSignatures(targetPt.Value.Snapshot, span);
                    bool retrigger = false;
                    if (sigs.Signatures.Count == _sigHelpSession.Signatures.Count) {
                        for (int i = 0; i < sigs.Signatures.Count && !retrigger; i++) {
                            var leftSig = sigs.Signatures[i];
                            var rightSig = _sigHelpSession.Signatures[i];

                            if (leftSig.Parameters.Count == rightSig.Parameters.Count) {
                                for (int j = 0; j < leftSig.Parameters.Count; j++) {
                                    var leftParam = leftSig.Parameters[j];
                                    var rightParam = rightSig.Parameters[j];

                                    if (leftParam.Name != rightParam.Name || leftParam.Documentation != rightParam.Documentation) {
                                        retrigger = true;
                                        break;
                                    }
                                }
                            }

                            if (leftSig.Content != rightSig.Content || leftSig.Documentation != rightSig.Documentation) {
                                retrigger = true;
                            }
                        }
                    } else {
                        retrigger = true;
                    }

                    if (retrigger) {
                        _sigHelpSession.Dismiss();
                        TriggerSignatureHelp();
                    } else {
                        int curParam = sigs.ParameterIndex;
                        if (sigs.LastKeywordArgument != null) {
                            curParam = Int32.MaxValue;
                            for (int i = 0; i < sig.Parameters.Count; i++) {
                                if (sig.Parameters[i].Name == sigs.LastKeywordArgument) {
                                    curParam = i;
                                    break;
                                }
                            }
                        }

                        if (curParam < sig.Parameters.Count) {
                            sig.SetCurrentParameter(sig.Parameters[curParam]);
                        } else if (sigs.LastKeywordArgument == String.Empty) {
                            sig.SetCurrentParameter(null);
                        } else {
                            CommaFindBestSignature(curParam, sigs.LastKeywordArgument);
                        }
                    }
                }
            }
        }

        private void CommaFindBestSignature(int curParam, string lastKeywordArg) {
            // see if we have a signature which accomodates this...

            // TODO: We should also take into account param arrays
            // TODO: We should also get the types of the arguments and use that to
            // pick the best signature when the signature includes types.
            foreach (var availableSig in _sigHelpSession.Signatures) {
                if (lastKeywordArg != null) {
                    for (int i = 0; i < availableSig.Parameters.Count; i++) {
                        if (availableSig.Parameters[i].Name == lastKeywordArg) {
                            _sigHelpSession.SelectedSignature = availableSig;

                            NodejsSignature sig = availableSig as NodejsSignature;
                            if (sig != null) {
                                sig.SetCurrentParameter(sig.Parameters[i]);
                            }
                            break;
                        }
                    }
                } else if (availableSig.Parameters.Count > curParam) {
                    _sigHelpSession.SelectedSignature = availableSig;

                    NodejsSignature sig = availableSig as NodejsSignature;
                    if (sig != null) {
                        sig.SetCurrentParameter(sig.Parameters[curParam]);
                    }
                    break;
                }
            }
        }

        internal void TriggerCompletionSession(bool completeWord) {
            Dismiss();

            _activeSession = CompletionBroker.TriggerCompletion(_textView);

            if (_activeSession != null) {
                FuzzyCompletionSet set;
                if (completeWord &&
                    _activeSession.CompletionSets.Count == 1 &&
                    (set = _activeSession.CompletionSets[0] as FuzzyCompletionSet) != null &&
                    set.SelectSingleBest()) {
                    _activeSession.Commit();
                    _activeSession = null;
                } else {
                    _activeSession.Filter();
                    _activeSession.Dismissed += OnCompletionSessionDismissedOrCommitted;
                    _activeSession.Committed += OnCompletionSessionDismissedOrCommitted;
                }
            }
        }

        internal void TriggerSignatureHelp() {
            if (_sigHelpSession != null) {
                _sigHelpSession.Dismiss();
            }

            _sigHelpSession = SignatureBroker.TriggerSignatureHelp(_textView);

            if (_sigHelpSession != null) {
                _sigHelpSession.Dismissed += OnSignatureSessionDismissed;
                ISignature sig;
                if (_sigHelpSession.Properties.TryGetProperty(typeof(NodejsSignature), out sig)) {
                    _sigHelpSession.SelectedSignature = sig;

                    IParameter param;
                    if (_sigHelpSession.Properties.TryGetProperty(typeof(NodejsParameter), out param)) {
                        ((NodejsSignature)sig).SetCurrentParameter(param);
                    }
                }
            }
        }

        private void OnCompletionSessionDismissedOrCommitted(object sender, System.EventArgs e) {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _activeSession.Committed -= OnCompletionSessionDismissedOrCommitted;
            _activeSession.Dismissed -= OnCompletionSessionDismissedOrCommitted;
            _activeSession = null;
        }

        private void OnSignatureSessionDismissed(object sender, System.EventArgs e) {
            // We've just been told that our active session was dismissed.  We should remove all references to it.
            _sigHelpSession.Dismissed -= OnSignatureSessionDismissed;
            _sigHelpSession = null;
        }

        private void DeleteSelectedSpans() {
            if (!_textView.Selection.IsEmpty) {
                _editOps.Delete();
            }
        }

        private void Dismiss() {
            if (_activeSession != null) {
                _activeSession.Dismiss();
            }
        }

        internal ICompletionBroker CompletionBroker {
            get {
                return _provider._CompletionBroker;
            }
        }

        internal IVsEditorAdaptersFactoryService AdaptersFactory {
            get {
                return _provider._adaptersFactory;
            }
        }

        internal ISignatureHelpBroker SignatureBroker {
            get {
                return _provider._SigBroker;
            }
        }

        #region IOleCommandTarget Members

        // we need this because VS won't give us certain keyboard events as they're handled before our key processor.  These
        // include enter and tab both of which we want to complete.

        internal void AttachKeyboardFilter() {
            if (_oldTarget == null) {
                var viewAdapter = AdaptersFactory.GetViewAdapter(_textView);
                if (viewAdapter != null) {
                    ErrorHandler.ThrowOnFailure(viewAdapter.AddCommandFilter(this, out _oldTarget));
                }
            }
        }

        private void DetachKeyboardFilter() {
            if (_oldTarget != null) {
                ErrorHandler.ThrowOnFailure(AdaptersFactory.GetViewAdapter(_textView).RemoveCommandFilter(this));
                _oldTarget = null;
            }
        }

        private IVsTextView GetViewAdapter() {
            return _provider._adaptersFactory.GetViewAdapter(_textView);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (int)VSConstants.VSStd2KCmdID.TYPECHAR) {
                var ch = (char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn);

                if (_activeSession != null && !_activeSession.IsDismissed) {
                    if (_activeSession.SelectedCompletionSet.SelectionStatus.IsSelected) {
                        var completion = _activeSession.SelectedCompletionSet.SelectionStatus.Completion;

                        string committedBy = String.Empty;
                        if (_activeSession.SelectedCompletionSet.Moniker == CompletionSource.NodejsRequireCompletionSetMoniker) {
                            if (completion.InsertionText.StartsWith("'")) { // require(
                                committedBy = ")";
                            } else if (completion.InsertionText.EndsWith("'")) { // require('
                                committedBy = "'";
                            } else if (completion.InsertionText.EndsWith("\"")) { // require("
                                committedBy = "\"";
                            }
                        } else {
                            committedBy = NodejsPackage.Instance != null && NodejsPackage.Instance.IntellisenseOptionsPage.OnlyTabOrEnterToCommit ?
                                string.Empty :
                                NodejsConstants.DefaultIntellisenseCompletionCommittedBy;
                        }

                        if (committedBy.IndexOf(ch) != -1) {
                            _activeSession.Commit();
                            if ((completion.InsertionText.EndsWith("'") && ch == '\'') ||
                                (completion.InsertionText.EndsWith("\"") && ch == '"')) {
                                // https://nodejstools.codeplex.com/workitem/960
                                // ' triggers the completion, but we don't want to insert the quote.
                                return VSConstants.S_OK;
                            }
                        }
                    } else if (_activeSession.SelectedCompletionSet.Moniker.Equals(CompletionSource.NodejsRequireCompletionSetMoniker) && !IsRequireIdentifierChar(ch)) {
                        _activeSession.Dismiss();
                    } else if (!_activeSession.SelectedCompletionSet.Moniker.Equals(CompletionSource.NodejsRequireCompletionSetMoniker) && !IsIdentifierChar(ch)) {
                        _activeSession.Dismiss();
                    }
                }

                int res = _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                if (_activeSession == null || !_activeSession.SelectedCompletionSet.Moniker.Equals(CompletionSource.NodejsRequireCompletionSetMoniker)) {
                    //Only process the char if we are not in a require completion
                    HandleChar((char)(ushort)System.Runtime.InteropServices.Marshal.GetObjectForNativeVariant(pvaIn));
                }

                if (_activeSession != null && !_activeSession.IsDismissed) {
                    _activeSession.Filter();
                }

                return res;
            }

            if (_activeSession != null) {
                if (pguidCmdGroup == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                        case VSConstants.VSStd2KCmdID.RETURN:
                            if (/*NodejsPackage.Instance.AdvancedEditorOptionsPage.EnterCommitsIntellisense*/ true &&
                                !_activeSession.IsDismissed &&
                                _activeSession.SelectedCompletionSet.SelectionStatus.IsSelected) {

                                // If the user has typed all of the characters as the completion and presses
                                // enter we should dismiss & let the text editor receive the enter.  For example 
                                // when typing "import sys[ENTER]" completion starts after the space.  After typing
                                // sys the user wants a new line and doesn't want to type enter twice.

                                bool enterOnComplete = /*NodejsPackage.Instance.AdvancedEditorOptionsPage.AddNewLineAtEndOfFullyTypedWord*/true &&
                                         EnterOnCompleteText();

                                _activeSession.Commit();

                                if (!enterOnComplete) {
                                    return VSConstants.S_OK;
                                }
                            } else {
                                _activeSession.Dismiss();
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.TAB:
                            if (!_activeSession.IsDismissed) {
                                _activeSession.Commit();
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                        case VSConstants.VSStd2KCmdID.DELETE:
                        case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                        case VSConstants.VSStd2KCmdID.DELETEWORDRIGHT:
                            int res = _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            if (_activeSession != null && !_activeSession.IsDismissed) {
                                _activeSession.Filter();
                            }
                            return res;
                    }
                }
            } else if (_sigHelpSession != null) {
                if (pguidCmdGroup == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                        case VSConstants.VSStd2KCmdID.BACKSPACE:
                            bool fDeleted = Backspace();
                            if (fDeleted) {
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.LEFT:
                            _editOps.MoveToPreviousCharacter(false);
                            UpdateCurrentParameter();
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.RIGHT:
                            _editOps.MoveToNextCharacter(false);
                            UpdateCurrentParameter();
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.HOME:
                        case VSConstants.VSStd2KCmdID.BOL:
                        case VSConstants.VSStd2KCmdID.BOL_EXT:
                        case VSConstants.VSStd2KCmdID.EOL:
                        case VSConstants.VSStd2KCmdID.EOL_EXT:
                        case VSConstants.VSStd2KCmdID.END:
                        case VSConstants.VSStd2KCmdID.WORDPREV:
                        case VSConstants.VSStd2KCmdID.WORDPREV_EXT:
                        case VSConstants.VSStd2KCmdID.DELETEWORDLEFT:
                            _sigHelpSession.Dismiss();
                            _sigHelpSession = null;
                            break;
                    }
                }
            }
                if (pguidCmdGroup == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)nCmdID) {
                        case VSConstants.VSStd2KCmdID.QUICKINFO:
                            TriggerQuickInfo();
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.PARAMINFO:
                            TriggerSignatureHelp();
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.RETURN:
                            if (_expansionMgr != null && _expansionClient.InSession && ErrorHandler.Succeeded(_expansionClient.EndCurrentExpansion(false))) {
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.TAB:
                            if (_expansionMgr != null && _expansionClient.InSession && ErrorHandler.Succeeded(_expansionClient.NextField())) {
                                return VSConstants.S_OK;
                            }
                            if (_textView.Selection.IsEmpty && _textView.Caret.Position.BufferPosition > 0) {
                                if (TryTriggerExpansion()) {
                                    return VSConstants.S_OK;
                                }
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.BACKTAB:
                            if (_expansionMgr != null && _expansionClient.InSession && ErrorHandler.Succeeded(_expansionClient.PreviousField())) {
                                return VSConstants.S_OK;
                            }
                            break;
                        case VSConstants.VSStd2KCmdID.SURROUNDWITH:
                        case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
                            TriggerSnippet(nCmdID);
                            return VSConstants.S_OK;
                        case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                            ForceCompletions = true;
                            try {
                                TriggerCompletionSession((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.COMPLETEWORD
                                    && !NodejsPackage.Instance.IntellisenseOptionsPage.OnlyTabOrEnterToCommit);
                            } finally {
                                ForceCompletions = false;
                            }
                            return VSConstants.S_OK;
                    }
            }
            return _oldTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private void TriggerSnippet(uint nCmdID) {
            if (_expansionMgr != null) {
                string prompt;
                string[] snippetTypes;
                if ((VSConstants.VSStd2KCmdID)nCmdID == VSConstants.VSStd2KCmdID.SURROUNDWITH) {
                    prompt = SR.GetString(SR.SurroundWith);
                    snippetTypes = _surroundsWithSnippetTypes;
                } else {
                    prompt = SR.GetString(SR.InsertSnippet);
                    snippetTypes = _allStandardSnippetTypes;
                }

                _expansionMgr.InvokeInsertionUI(
                    GetViewAdapter(),
                    _expansionClient,
                    Guids.NodejsLanguageInfo,
                    snippetTypes,
                    snippetTypes.Length,
                    0,
                    null,
                    0,
                    0,
                    prompt,
                    ">"
                );
            }
        }

        private bool TryTriggerExpansion() {
            if (_expansionMgr != null) {
                var snapshot = _textView.TextBuffer.CurrentSnapshot;
                var span = new SnapshotSpan(snapshot, new Span(_textView.Caret.Position.BufferPosition.Position - 1, 1));
                var classification = _textView.TextBuffer.GetNodejsClassifier().GetClassificationSpans(span);
                if (classification.Count == 1) {
                    var clsSpan = classification.First().Span;
                    var text = classification.First().Span.GetText();

                    TextSpan[] textSpan = new TextSpan[1];
                    textSpan[0].iStartLine = clsSpan.Start.GetContainingLine().LineNumber;
                    textSpan[0].iStartIndex = clsSpan.Start.Position - clsSpan.Start.GetContainingLine().Start;
                    textSpan[0].iEndLine = clsSpan.End.GetContainingLine().LineNumber;
                    textSpan[0].iEndIndex = clsSpan.End.Position - clsSpan.End.GetContainingLine().Start;

                    string expansionPath, title;
                    int hr = _expansionMgr.GetExpansionByShortcut(
                        _expansionClient,
                        Guids.NodejsLanguageInfo,
                        text,
                        GetViewAdapter(),
                        textSpan,
                        1,
                        out expansionPath,
                        out title
                    );
                    if (ErrorHandler.Succeeded(hr)) {
                        // hr may be S_FALSE if there are multiple expansions,
                        // so we don't want to InsertNamedExpansion yet. VS will
                        // pop up a selection dialog in this case.
                        if (hr == VSConstants.S_OK) {
                            return ErrorHandler.Succeeded(_expansionClient.InsertNamedExpansion(title, expansionPath, textSpan[0]));
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsRequireIdentifierChar(char ch) {
            return ch == '_'
                || ch == '.'
                || ch == '/'
                || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9');
        }

        private static bool IsIdentifierFirstChar(char ch) {
            return ch == '_' || ch == '$'|| (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
        }

        private static bool IsIdentifierChar(char ch) {
            return IsIdentifierFirstChar(ch) || (ch >= '0' && ch <= '9');
        }

        private bool EnterOnCompleteText() {
            SnapshotPoint? point = _activeSession.GetTriggerPoint(_textView.TextBuffer.CurrentSnapshot);
            if (point.HasValue) {
                int chars = _textView.Caret.Position.BufferPosition.Position - point.Value.Position;
                var selectionStatus = _activeSession.SelectedCompletionSet.SelectionStatus;
                if (chars == selectionStatus.Completion.InsertionText.Length) {
                    string text = _textView.TextSnapshot.GetText(point.Value.Position, chars);

                    if (String.Compare(text, selectionStatus.Completion.InsertionText, true) == 0) {
                        return true;
                    }
                }
            }

            return false;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == VSConstants.VSStd2K) {
                for (int i = 0; i < cCmds; i++) {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd2KCmdID.SHOWMEMBERLIST:
                        case VSConstants.VSStd2KCmdID.COMPLETEWORD:
                        case VSConstants.VSStd2KCmdID.QUICKINFO:
                        case VSConstants.VSStd2KCmdID.PARAMINFO:
                        case VSConstants.VSStd2KCmdID.SURROUNDWITH:
                        case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                            return VSConstants.S_OK;
                    }
                }
            }

            return _oldTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion
    }
}


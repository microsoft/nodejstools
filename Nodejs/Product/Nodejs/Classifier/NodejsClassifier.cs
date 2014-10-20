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
 * ***************************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Classifier;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Classifier {
    /// <summary>
    /// Provides classification based upon the DLR TokenCategory enum.
    /// </summary>
    internal class NodejsClassifier : IClassifier {
        private readonly TokenCache _tokenCache;
        private readonly NodejsClassifierProvider _provider;
        private readonly ITextBuffer _buffer;

        [ThreadStatic]
        private static JSScanner _scanner;    // JSScanner for each version, shared between all buffers

        internal NodejsClassifier(NodejsClassifierProvider provider, ITextBuffer buffer) {
            buffer.Changed += BufferChanged;
            buffer.ContentTypeChanged += BufferContentTypeChanged;

            _tokenCache = new TokenCache();
            _provider = provider;
            _buffer = buffer;
        }

        internal void NewVersion() {
            _tokenCache.Clear();
            var changed = ClassificationChanged;
            if (changed != null) {
                var snapshot = _buffer.CurrentSnapshot;

                changed(this, new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
            }
        }

        #region IDlrClassifier

        // This event gets raised if the classification of existing test changes.
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        /// <summary>
        /// This method classifies the given snapshot span.
        /// </summary>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) {
            var classifications = new List<ClassificationSpan>();
            var snapshot = span.Snapshot;


            if (span.Length > 0) {
                // don't add classifications for REPL commands.
                if (!span.Snapshot.IsReplBufferWithCommand()) {
                    AddClassifications(GetJSScanner(), classifications, span);
                }
            }

            return classifications;
        }

        private JSScanner GetJSScanner() {
            if (_scanner == null) {
                _scanner = new JSScanner(new CodeSettings() { AllowShebangLine = true });
            }
            return _scanner;
        }

        public NodejsClassifierProvider Provider {
            get {
                return _provider;
            }
        }

        #endregion

        #region Private Members

        private Dictionary<TokenCategory, IClassificationType> CategoryMap {
            get {
                return _provider.CategoryMap;
            }
        }

        private void BufferContentTypeChanged(object sender, ContentTypeChangedEventArgs e) {
            _tokenCache.Clear();
            _buffer.Changed -= BufferChanged;
            _buffer.ContentTypeChanged -= BufferContentTypeChanged;
            _buffer.Properties.RemoveProperty(typeof(NodejsClassifier));
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e) {
            var snapshot = e.After;
            if (!snapshot.IsReplBufferWithCommand()) {
                _tokenCache.EnsureCapacity(snapshot.LineCount);

                var JSScanner = GetJSScanner();
                foreach (var change in e.Changes) {
                    if (change.LineCountDelta > 0) {
                        _tokenCache.InsertLines(snapshot.GetLineNumberFromPosition(change.NewEnd) + 1 - change.LineCountDelta, change.LineCountDelta);
                    } else if (change.LineCountDelta < 0) {
                        _tokenCache.DeleteLines(snapshot.GetLineNumberFromPosition(change.NewEnd) + 1, -change.LineCountDelta);
                    }

                    ApplyChange(JSScanner, snapshot, change.NewSpan);
                }
            }
        }

        /// <summary>
        /// Adds classification spans to the given collection.
        /// Scans a contiguous sub-<paramref name="span"/> of a larger code span which starts at <paramref name="codeStartLine"/>.
        /// </summary>
        private void AddClassifications(JSScanner JSScanner, List<ClassificationSpan> classifications, SnapshotSpan span) {
            Debug.Assert(span.Length > 0);

            var snapshot = span.Snapshot;
            int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
            int lastLine = snapshot.GetLineNumberFromPosition(span.End - 1);

            Contract.Assert(firstLine >= 0);

            _tokenCache.EnsureCapacity(snapshot.LineCount);

            // find the closest line preceding firstLine for which we know categorizer state, stop at the codeStartLine:
            LineTokenization lineTokenization;
            int currentLine = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenization) + 1;
            object state = lineTokenization.State;

            // track the previous 2 tokens to adjust our classifications of keywords
            // when they shouldn't be displayed as keywords...
            TokenInfo? prevToken = null, prevPrevToken = null;

            // initialize the previous tokens so we can handle things like:
            //      foo.
            //          get()
            // even if we're called on the line for get()
            int prevLine = currentLine - 1;
            while (prevLine >= 0 && prevToken == null) {
                LineTokenization prevLineTokenization = GetPreviousTokenization(JSScanner, snapshot, firstLine, prevLine);
                for (int i = prevLineTokenization.Tokens.Length - 1; i >= 0 && prevToken == null; i--) {
                    var tempToken = prevLineTokenization.Tokens[i];
                    if (IsValidPreviousToken(ref tempToken)) {
                        prevToken = prevPrevToken;
                        prevPrevToken = tempToken;
                    }
                }
                prevLine--;
            }

            while (currentLine <= lastLine) {
                if (!_tokenCache.TryGetTokenization(currentLine, out lineTokenization)) {
                    lineTokenization = TokenizeLine(JSScanner, snapshot, state, currentLine);
                    _tokenCache[currentLine] = lineTokenization;
                }

                state = lineTokenization.State;

                for (int i = 0; i < lineTokenization.Tokens.Length; i++) {
                    var token = lineTokenization.Tokens[i];

                    if (token.Category == TokenCategory.IncompleteMultiLineStringLiteral || token.Category == TokenCategory.Comment) {
                        IClassificationType type;
                        switch (token.Category) {
                            case TokenCategory.IncompleteMultiLineStringLiteral:
                                type = _provider.StringLiteral;
                                break;
                            case TokenCategory.Comment:
                                type = _provider.Comment;
                                break;
                            default:
                                type = null;
                                break;
                        }

                        Debug.Assert(type != null, "We should have a defined ClassificationType for every token.");

                        // we need to walk backwards to find the start of this multi-line string...
                        TokenInfo startToken = token;
                        int validPrevLine;
                        int length = startToken.SourceSpan.Length;
                        if (i == 0) {
                            length += GetLeadingMultiLineTokens(JSScanner, snapshot, token.Category, firstLine, currentLine, out validPrevLine, ref startToken);
                        } else {
                            validPrevLine = currentLine;
                        }

                        if (i == lineTokenization.Tokens.Length - 1) {
                            length += GetTrailingMultiLineTokens(JSScanner, snapshot, token.Category, currentLine, state);
                        }

                        var tokenSpan = new Span(SnapshotSpanToSpan(snapshot, startToken, validPrevLine).Start, length);
                        var intersection = span.Intersection(tokenSpan);

                        if ((intersection != null && intersection.Value.Length > 0) ||
                            (span.Length == 0 && tokenSpan.Contains(span.Start)) // handle zero-length spans
                        ) {
                            classifications.Add(new ClassificationSpan(new SnapshotSpan(snapshot, tokenSpan), type));
                        }
                    } else {
                        ClassificationSpan classification = null;
                        if (token.Category == TokenCategory.Keyword) {
                            // check and see if we're not really a keyword...
                            if (IsKeywordInIdentifierContext(snapshot, prevToken, prevPrevToken, token, currentLine)) {
                                classification = GetClassificationSpan(
                                    span,
                                    token,
                                    currentLine,
                                    CategoryMap[TokenCategory.Identifier]
                                );
                            }
                        }
                        if (classification == null) {
                            classification = ClassifyToken(span, token, currentLine);
                        }

                        if (classification != null) {
                            classifications.Add(classification);
                        }
                    }

                    if (IsValidPreviousToken(ref token)) {
                        prevPrevToken = prevToken;
                        prevToken = token;
                    }
                }

                currentLine++;
            }
        }

        private static bool IsValidPreviousToken(ref TokenInfo token) {
            return token.Category != TokenCategory.Comment &&
                   token.Category != TokenCategory.LineComment &&
                   token.Category != TokenCategory.None;
        }

        private static bool IsKeywordInIdentifierContext(ITextSnapshot snapshot, TokenInfo? prevToken, TokenInfo? prevPrevToken, TokenInfo token, int lineNumber) {
            if (prevToken != null) {
                var prevValue = prevToken.Value;
                if (prevValue.Category == TokenCategory.Operator &&
                    prevValue.Trigger == TokenTriggers.MemberSelect) {
                    // https://nodejstools.codeplex.com/workitem/967
                    // member.get
                    return true;
                }
                
                if (prevValue.Category == TokenCategory.Keyword &&
                    snapshot.GetText(SnapshotSpanToSpan(snapshot, prevValue, lineNumber)) == "function") {
                    // https://nodejstools.codeplex.com/workitem/976
                    // function static() { }
                    return true;
                }

                if (prevPrevToken != null && prevValue.Category == TokenCategory.Operator) {
                    var prevSpan = SnapshotSpanToSpan(snapshot, prevValue, lineNumber);
                    if (snapshot.GetText(prevSpan) == "*") {
                        var prevPrevValue = prevPrevToken.Value;
                        var prevPrevSpan = SnapshotSpanToSpan(snapshot, prevPrevValue, lineNumber);
                        if (snapshot.GetText(prevPrevSpan) == "function") {
                            // https://nodejstools.codeplex.com/workitem/976
                            // This time with a generator function...
                            // function *static() { }
                            return true;
                        }
                    }
                }
            }
            

            return false;
        }

        private int GetLeadingMultiLineTokens(JSScanner JSScanner, ITextSnapshot snapshot, TokenCategory tokenCategory, int firstLine, int currentLine, out int validPrevLine, ref TokenInfo startToken) {
            validPrevLine = currentLine;
            int prevLine = currentLine - 1;
            int length = 0;

            while (prevLine >= 0) {
                LineTokenization prevLineTokenization = GetPreviousTokenization(JSScanner, snapshot, firstLine, prevLine);

                if (prevLineTokenization.Tokens.Length != 0) {
                    if (prevLineTokenization.Tokens[prevLineTokenization.Tokens.Length - 1].Category != tokenCategory) {
                        break;
                    }

                    startToken = prevLineTokenization.Tokens[prevLineTokenization.Tokens.Length - 1];
                    length += startToken.SourceSpan.Length;
                }

                validPrevLine = prevLine;
                prevLine--;

                if (prevLineTokenization.Tokens.Length > 1) {
                    // http://pytools.codeplex.com/workitem/749
                    // if there are multiple tokens on this line then our multi-line string
                    // is terminated.
                    break;
                }
            }
            return length;
        }

        private LineTokenization GetPreviousTokenization(JSScanner JSScanner, ITextSnapshot snapshot, int firstLine, int prevLine) {
            LineTokenization prevLineTokenization;
            if (!_tokenCache.TryGetTokenization(prevLine, out prevLineTokenization)) {
                LineTokenization lineTokenizationTemp;
                int currentLineTemp = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenizationTemp) + 1;
                object stateTemp = lineTokenizationTemp.State;

                while (currentLineTemp <= snapshot.LineCount) {
                    if (!_tokenCache.TryGetTokenization(currentLineTemp, out lineTokenizationTemp)) {
                        lineTokenizationTemp = TokenizeLine(JSScanner, snapshot, stateTemp, currentLineTemp);
                        _tokenCache[currentLineTemp] = lineTokenizationTemp;
                    }

                    stateTemp = lineTokenizationTemp.State;
                }

                prevLineTokenization = TokenizeLine(JSScanner, snapshot, stateTemp, prevLine);
                _tokenCache[prevLine] = prevLineTokenization;
            }
            return prevLineTokenization;
        }

        private int GetTrailingMultiLineTokens(JSScanner JSScanner, ITextSnapshot snapshot, TokenCategory tokenCategory, int currentLine, object state) {
            int nextLine = currentLine + 1;
            var prevState = state;
            int length = 0;
            while (nextLine < snapshot.LineCount) {
                LineTokenization nextLineTokenization;
                if (!_tokenCache.TryGetTokenization(nextLine, out nextLineTokenization)) {
                    nextLineTokenization = TokenizeLine(JSScanner, snapshot, prevState, nextLine);
                    prevState = nextLineTokenization.State;
                    _tokenCache[nextLine] = nextLineTokenization;
                }

                if (nextLineTokenization.Tokens.Length != 0) {
                    if (nextLineTokenization.Tokens[0].Category != tokenCategory) {
                        break;
                    }

                    length += nextLineTokenization.Tokens[0].SourceSpan.Length;
                }
                nextLine++;
            }
            return length;
        }

        /// <summary>
        /// Rescans the part of the buffer affected by a change. 
        /// Scans a contiguous sub-<paramref name="span"/> of a larger code span which starts at <paramref name="codeStartLine"/>.
        /// </summary>
        private void ApplyChange(JSScanner JSScanner, ITextSnapshot snapshot, Span span) {
            int firstLine = snapshot.GetLineNumberFromPosition(span.Start);
            int lastLine = snapshot.GetLineNumberFromPosition(span.Length > 0 ? span.End - 1 : span.End);

            Contract.Assert(firstLine >= 0);

            // find the closest line preceding firstLine for which we know categorizer state, stop at the codeStartLine:
            LineTokenization lineTokenization;
            firstLine = _tokenCache.IndexOfPreviousTokenization(firstLine, 0, out lineTokenization) + 1;
            object state = lineTokenization.State;

            int currentLine = firstLine;
            object previousState;
            while (currentLine < snapshot.LineCount) {
                previousState = _tokenCache.TryGetTokenization(currentLine, out lineTokenization) ? lineTokenization.State : null;
                _tokenCache[currentLine] = lineTokenization = TokenizeLine(JSScanner, snapshot, state, currentLine);
                state = lineTokenization.State;

                // stop if we visted all affected lines and the current line has no tokenization state or its previous state is the same as the new state:
                if (currentLine > lastLine && (previousState == null || previousState.Equals(state))) {
                    break;
                }

                currentLine++;
            }

            // classification spans might have changed between the start of the first and end of the last visited line:
            int changeStart = snapshot.GetLineFromLineNumber(firstLine).Start;
            int changeEnd = (currentLine < snapshot.LineCount) ? snapshot.GetLineFromLineNumber(currentLine).End : snapshot.Length;
            if (changeStart < changeEnd) {
                var classificationChanged = ClassificationChanged;
                if (classificationChanged != null) {
                    var args = new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, new Span(changeStart, changeEnd - changeStart)));
                    classificationChanged(this, args);
                }
            }
        }

        private LineTokenization TokenizeLine(JSScanner JSScanner, ITextSnapshot snapshot, object previousLineState, int lineNo) {
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNo);
            SnapshotSpan lineSpan = new SnapshotSpan(snapshot, line.Start, line.LengthIncludingLineBreak);

            var tcp = new SnapshotSpanSourceCodeReader(lineSpan);

            JSScanner.Initialize(
                lineSpan.GetText(),
                previousLineState,
                new SourceLocation(0, lineNo + 1, 1)
            );
            try {
                var tokens = JSScanner.ReadTokens(lineSpan.Length).Select(ToTokenKind).ToArray();
                return new LineTokenization(tokens, JSScanner.CurrentState);
            } finally {
                JSScanner.Uninitialize();
            }
        }

        private static TokenInfo ToTokenKind(TokenWithSpan context) {
            switch (context.Token) {
                case JSToken.Semicolon:                      // ;
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Delimiter,
                        TokenTriggers.None
                    );
                case JSToken.RightCurly:                     // }
                case JSToken.LeftCurly:                      // {
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Grouping,
                        TokenTriggers.MatchBraces
                    );
                case JSToken.Debugger:
                case JSToken.Var:
                case JSToken.If:
                case JSToken.For:
                case JSToken.Do:
                case JSToken.While:
                case JSToken.Continue:
                case JSToken.Break:
                case JSToken.Return:
                case JSToken.With:
                case JSToken.Switch:
                case JSToken.Throw:
                case JSToken.Try:
                case JSToken.Function:
                case JSToken.Else:
                case JSToken.Null:
                case JSToken.True:
                case JSToken.False:
                case JSToken.This:
                case JSToken.Void:
                case JSToken.TypeOf:
                case JSToken.Delete:
                case JSToken.Case:
                case JSToken.Catch:
                case JSToken.Default:
                case JSToken.Finally:
                case JSToken.New:
                case JSToken.Class:
                case JSToken.Const:
                case JSToken.Enum:
                case JSToken.Export:
                case JSToken.Extends:
                case JSToken.Import:
                case JSToken.Super:

                // ECMA strict reserved words
                case JSToken.Implements:
                case JSToken.Interface:
                case JSToken.Let:
                case JSToken.Package:
                case JSToken.Private:
                case JSToken.Protected:
                case JSToken.Public:
                case JSToken.Static:
                case JSToken.Yield:
                // always okay for identifiers
                case JSToken.Get:
                case JSToken.Set:

                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Keyword,
                        TokenTriggers.None
                    );

                // used by both statement and expression switches

                // main expression switch
                case JSToken.Identifier:
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Identifier,
                        TokenTriggers.None
                    );

                case JSToken.StringLiteral:
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.StringLiteral,
                        TokenTriggers.None
                    );
                case JSToken.IntegerLiteral:
                case JSToken.NumericLiteral:
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.NumericLiteral,
                        TokenTriggers.None
                    );

                case JSToken.LeftParenthesis:                // (
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Grouping,
                        TokenTriggers.ParameterStart | TokenTriggers.MatchBraces
                    );
                case JSToken.RightParenthesis:               // )
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Grouping,
                        TokenTriggers.ParameterEnd | TokenTriggers.MatchBraces
                    );
                case JSToken.LeftBracket:                    // [
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Grouping,
                        TokenTriggers.MatchBraces
                    );
                case JSToken.RightBracket:                   // ]
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Grouping,
                        TokenTriggers.MatchBraces
                    );
                case JSToken.AccessField:                    // .
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Operator,
                        TokenTriggers.MemberSelect
                    );
                // unary ops
                case JSToken.Increment:                      // ++
                case JSToken.Decrement:                      // --
                case JSToken.LogicalNot:     // !
                case JSToken.BitwiseNot:                     // ~
                case JSToken.Plus:     // +
                case JSToken.Minus:                          // -
                case JSToken.Multiply:                       // *
                case JSToken.Divide:                         // /
                case JSToken.Modulo:                         // %
                case JSToken.BitwiseAnd:                     // &
                case JSToken.BitwiseOr:                      // |
                case JSToken.BitwiseXor:                     // ^
                case JSToken.LeftShift:                      // <<
                case JSToken.RightShift:                     // >>
                case JSToken.UnsignedRightShift:             // >>>

                case JSToken.Equal:                          // ==
                case JSToken.NotEqual:                       // !=
                case JSToken.StrictEqual:                    // ===
                case JSToken.StrictNotEqual:                 // !==
                case JSToken.LessThan:                       // <
                case JSToken.LessThanEqual:                  // <=
                case JSToken.GreaterThan:                    // >
                case JSToken.GreaterThanEqual:               // >=

                case JSToken.LogicalAnd:                     // &&
                case JSToken.LogicalOr:                      // ||

                case JSToken.Assign:                         // =
                case JSToken.PlusAssign:                     // +=
                case JSToken.MinusAssign:                    // -=
                case JSToken.MultiplyAssign:                 // *=
                case JSToken.DivideAssign:                   // /=
                case JSToken.ModuloAssign:                   // %=
                case JSToken.BitwiseAndAssign:               // &=
                case JSToken.BitwiseOrAssign:                // |=
                case JSToken.BitwiseXorAssign:               // ^=
                case JSToken.LeftShiftAssign:                // <<=
                case JSToken.RightShiftAssign:               // >>=
                case JSToken.UnsignedRightShiftAssign:       // >>>=

                case JSToken.ConditionalIf:                  // ? // MUST FOLLOW LastBinaryOp
                case JSToken.Colon:                          // :
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Operator,
                        TokenTriggers.None
                    );

                case JSToken.InstanceOf:
                case JSToken.In:
                    break;
                case JSToken.Comma:                          // :
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Delimiter,
                        TokenTriggers.ParameterNext
                    );
                case JSToken.SingleLineComment:              // for authoring
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.LineComment,
                        TokenTriggers.None
                    );
                case JSToken.MultipleLineComment:            // for authoring
                case JSToken.UnterminatedComment:            // for authoring
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.Comment,
                        TokenTriggers.None
                    );
                case JSToken.RegularExpression: // only returned if the RawTokens flag is set on the scanner
                    return new TokenInfo(
                        GetSpan(context),
                        TokenCategory.RegularExpressionLiteral,
                        TokenTriggers.None
                    );
            }
            return new TokenInfo(
                GetSpan(context),
                TokenCategory.None,
                TokenTriggers.None
            );
        }

        private static SourceSpan GetSpan(TokenWithSpan context) {
            return new SourceSpan(
                new SourceLocation(
                    context.Start,
                    context.StartLine,
                    context.StartColumn
                ),
                new SourceLocation(
                    context.End,
                    context.EndLine,
                    context.EndColumn
                )
            );
        }

        private ClassificationSpan ClassifyToken(SnapshotSpan span, TokenInfo token, int lineNumber) {
            IClassificationType classification = null;

            if (token.Category == TokenCategory.Operator) {
                if (token.Trigger == TokenTriggers.MemberSelect) {
                    classification = _provider.DotClassification;
                }
            } else if (token.Category == TokenCategory.Grouping) {
                if ((token.Trigger & TokenTriggers.MatchBraces) != 0) {
                    classification = _provider.GroupingClassification;
                }
            } else if (token.Category == TokenCategory.Delimiter) {
                if (token.Trigger == TokenTriggers.ParameterNext) {
                    classification = _provider.CommaClassification;
                }
            }

            if (classification == null) {
                CategoryMap.TryGetValue(token.Category, out classification);
            }

            if (classification != null) {
                return GetClassificationSpan(span, token, lineNumber, classification);
            }

            return null;
        }

        private static ClassificationSpan GetClassificationSpan(SnapshotSpan span, TokenInfo token, int lineNumber, IClassificationType classification) {
            var tokenSpan = SnapshotSpanToSpan(span.Snapshot, token, lineNumber);
            var intersection = span.Intersection(tokenSpan);

            if (intersection != null && intersection.Value.Length > 0 ||
                (span.Length == 0 && tokenSpan.Contains(span.Start))) { // handle zero-length spans which Intersect and Overlap won't return true on ever.
                return new ClassificationSpan(new SnapshotSpan(span.Snapshot, tokenSpan), classification);
            }
            return null;
        }

        private static Span SnapshotSpanToSpan(ITextSnapshot snapshot, TokenInfo token, int lineNumber) {
            var line = snapshot.GetLineFromLineNumber(lineNumber);
            var index = line.Start.Position + token.SourceSpan.Start.Column - 1;
            var tokenSpan = new Span(index, token.SourceSpan.Length);
            return tokenSpan;
        }

        #endregion
    }

    internal static class ClassifierExtensions {
        public static NodejsClassifier GetNodejsClassifier(this ITextBuffer buffer) {
            NodejsClassifier res;
            if (buffer.Properties.TryGetProperty<NodejsClassifier>(typeof(NodejsClassifier), out res)) {
                return res;
            }
            return null;
        }
    }
}

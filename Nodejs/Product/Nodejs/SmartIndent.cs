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
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.NodejsTools {
    sealed class SmartIndent : ISmartIndent {
        private readonly ITextView _textView;
        private readonly IEditorOptions _editorOptions;
        private readonly ITagger<ClassificationTag> _classifier;

        private static HashSet<string> _dedentKeywords = new HashSet<string>(new[] { "return", "continue", "break", "throw" });
        private static HashSet<string> _indentKeywords = new HashSet<string>(
            new[] { "try", "if", "while", "for", "do", "else", "catch" }
            );


        public SmartIndent(ITextView textView, IEditorOptions editorOptions, ITagger<ClassificationTag> classifier) {
            _textView = textView;
            _editorOptions = editorOptions;
            _classifier = classifier;
            _textView.Properties.AddProperty(typeof(SmartIndent), this);
        }

        #region ISmartIndent Members

        public int? GetDesiredIndentation(VisualStudio.Text.ITextSnapshotLine line) {
            var dte = (EnvDTE.DTE)NodejsPackage.GetGlobalService(typeof(EnvDTE.DTE));

            var props = dte.get_Properties("TextEditor", "JavaScript");
            switch ((EnvDTE._vsIndentStyle)(int)props.Item("IndentStyle").Value) {
                case EnvDTE._vsIndentStyle.vsIndentStyleNone:
                    return null;
                case EnvDTE._vsIndentStyle.vsIndentStyleDefault:
                    return DoBlockIndent(line);
                case EnvDTE._vsIndentStyle.vsIndentStyleSmart:
                    return DoSmartIndent(line);
            }

            return null;
        }

        #endregion

        private struct LineInfo {
            public static readonly LineInfo Empty = new LineInfo();
            public bool NeedsUpdate;
            public int? Indentation;
            public int? DedentTo;
            public bool WasIndentKeyword;
            /// <summary>
            /// When we have an indent keyword this tracks whether or not it was "do",
            /// which we need to know so that we don't do an indent after the closing while
            /// if the user does:
            /// do
            ///     42
            /// while(false);
            /// </summary>
            public bool WasDoKeyword;
            public bool ShouldDedentAfter;
        }

        private int? DoSmartIndent(ITextSnapshotLine line) {
            if (line.LineNumber == 0) {
                return null;
            }
            int? indentation = GetLeadingWhiteSpace(line.GetText());
            var classifications = EnumerateClassificationsInReverse(
                _classifier,
                line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1).End
            );

            if (classifications.MoveNext()) {
                var starting = classifications.Current;

                // first check to see if we're in an unterminated multiline token
                if (starting != null) {
                    if (starting.Tag.ClassificationType.Classification == "comment" &&
                        starting.Span.GetText().StartsWith("/*") &&
                        (!starting.Span.GetText().EndsWith("*/") || starting.Span.End.GetContainingLine() == line)) {
                        // smart indent in comment, dont indent
                        return null;
                    } else if (starting.Tag.ClassificationType.Classification == "string") {
                        var text = starting.Span.GetText();
                        if (!text.EndsWith("\"") && !text.EndsWith("'")) {
                            // smart indent in multiline string, no indent
                            return null;
                        }
                    }
                }

                // walk backwards and collect all of the possible tokens that could
                // be useful here...
                var tokenStack = new System.Collections.Generic.Stack<ITagSpan<ClassificationTag>>();
                tokenStack.Push(null);  // end with an implicit newline
                bool endAtNextNull = false;

                do {
                    var token = classifications.Current;
                    tokenStack.Push(token);
                    if (token == null && endAtNextNull) {
                        break;
                    } else if (token != null &&
                        token.Span.GetText() == "{") {
                        endAtNextNull = true;
                    }
                } while (classifications.MoveNext());

                var indentStack = new System.Collections.Generic.Stack<LineInfo>();
                var current = LineInfo.Empty;

                while (tokenStack.Count > 0) {
                    var token = tokenStack.Pop();
                    if (token == null) {
                        current.NeedsUpdate = true;
                    } else if (IsOpenGrouping(token)) {
                        if (current.WasIndentKeyword && token.Span.GetText() == "{") {
                            // the indentation statement is followed by braces, go ahead
                            // and remove the level of indentation now
                            current.WasIndentKeyword = false;
                            current.Indentation -= _editorOptions.GetTabSize();
                        }
                        indentStack.Push(current);
                        var start = token.Span.Start;
                        current = new LineInfo {
                            Indentation = GetLeadingWhiteSpace(start.GetContainingLine().GetText()) + _editorOptions.GetTabSize()
                        };
                    } else if (_indentKeywords.Contains(token.Span.GetText()) && !current.WasDoKeyword) {
                        // if (foo) 
                        //      console.log('hi')
                        // We should indent here
                        var start = token.Span.Start;
                        int dedentTo = GetLeadingWhiteSpace(start.GetContainingLine().GetText());
                        if (current.DedentTo != null && token.Span.GetText() != "if") {
                            // https://nodejstools.codeplex.com/workitem/1176
                            // if (true)
                            //     while (true)
                            //         ;
                            //
                            // We should dedent to the if (true)
                            // But for:
                            // if (true)
                            //     if (true)
                            //          ;
                            // We still want to dedent to our current level for the else
                            dedentTo = current.DedentTo.Value;
                        }
                        current = new LineInfo {
                            Indentation = GetLeadingWhiteSpace(start.GetContainingLine().GetText()) + _editorOptions.GetTabSize(),
                            DedentTo = dedentTo,
                            WasIndentKeyword = true,
                            WasDoKeyword = token.Span.GetText() == "do"
                        };
                    } else if (IsCloseGrouping(token)) {
                        if (indentStack.Count > 0) {
                            current = indentStack.Pop();
                        } else {
                            current = new LineInfo {
                                Indentation = GetLeadingWhiteSpace(token.Span.Start.GetContainingLine().GetText())
                            };
                        }
                    } else if (IsMultilineStringOrComment(token)) {
                        while (token != null && tokenStack.Count > 0) {
                            token = tokenStack.Pop();
                        }
                    } else if (current.WasIndentKeyword) {
                        // we've encountered a token after the opening of the indented
                        // statement, go ahead and decrement our indentation level now.
                        current = new LineInfo {
                            Indentation = current.DedentTo,
                            DedentTo = current.DedentTo - _editorOptions.GetTabSize(),
                            WasDoKeyword = current.WasDoKeyword
                        };
                    } else if (current.NeedsUpdate) {
                        var line2 = token.Span.Start.GetContainingLine();
                        current = new LineInfo {
                            Indentation = GetLeadingWhiteSpace(line2.GetText())
                        };
                    }

                    if (token != null && _dedentKeywords.Contains(token.Span.GetText())) {     // dedent after some statements
                        current.ShouldDedentAfter = true;
                    }
                }
                return current.Indentation -
                    (current.ShouldDedentAfter ? _editorOptions.GetTabSize() : 0);
            }

            return null;
        }

        private static bool IsMultilineStringOrComment(ITagSpan<ClassificationTag> token) {
            if (token.Tag.ClassificationType.Classification == "string" &&
                !(token.Span.GetText().StartsWith("'") || token.Span.GetText().StartsWith("\""))) {
                return true;
            }
            if (token.Tag.ClassificationType.Classification == "comment" &&
                !(token.Span.GetText().StartsWith("//") || token.Span.GetText().StartsWith("/*"))) {
                return true;
            }
            return false;
        }

        private bool IsOpenGrouping(ITagSpan<ClassificationTag> token) {
            var text = token.Span.GetText();

            return text == "(" || text == "{" || text == "[";
        }

        private bool IsCloseGrouping(ITagSpan<ClassificationTag> token) {
            var text = token.Span.GetText();

            return text == ")" || text == "}" || text == "]";
        }

        /// <summary>
        /// Enumerates all of the classifications in reverse starting at start to the beginning of the file.
        /// </summary>
        private static IEnumerator<ITagSpan<ClassificationTag>> EnumerateClassificationsInReverse(ITagger<ClassificationTag> classifier, SnapshotPoint start) {
            var curLine = start.GetContainingLine();
            var spanEnd = start;

            for (; ; ) {
                var classifications = classifier.GetTags(
                    new NormalizedSnapshotSpanCollection(new SnapshotSpan(curLine.Start, spanEnd))
                );
                foreach (var classification in classifications.Reverse()) {
                    yield return classification;
                }

                yield return null;

                if (curLine.LineNumber == 0) {
                    break;
                }

                curLine = start.Snapshot.GetLineFromLineNumber(curLine.LineNumber - 1);
                spanEnd = curLine.End;
            }
        }

        private int? DoBlockIndent(ITextSnapshotLine line) {
            ITextSnapshotLine previousLine = null;

            for (int lineNumber = line.LineNumber - 1; lineNumber >= 0; --lineNumber) {
                previousLine = line.Snapshot.GetLineFromLineNumber(lineNumber);

                string text = previousLine.GetText();

                if (text.Length > 0) {
                    return GetLeadingWhiteSpace(text);
                }

            }

            return null;
        }

        private int GetLeadingWhiteSpace(string text) {
            int size = 0;
            foreach (var ch in text) {
                if (ch == '\t') {
                    size += (size + _editorOptions.GetTabSize()) - (size % _editorOptions.GetTabSize());
                } else if (ch == ' ') {
                    size++;
                } else {
                    break;
                }
            }

            return size;
        }


        #region IDisposable Members

        public void Dispose() {
        }

        #endregion
    }
}

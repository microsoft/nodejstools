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
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(NodejsConstants.Nodejs)]
    [Name("NodejsSmartIndent")]
    class SmartIndentProvider : ISmartIndentProvider {
        private readonly IEditorOptionsFactoryService _editorOptionsFactory;
        private readonly ITaggerProvider _taggerProvider;
        private static HashSet<string> _dedentKeywords = new HashSet<string>(new[] { "return", "continue", "break" });
        private static HashSet<string> _stmtKeywords = new HashSet<string>(
                new[] { "try", "if", "switch", "while", "for", "do", "break", "return", "throw", "default", "var" }
            );

        [ImportingConstructor]
        public SmartIndentProvider(IEditorOptionsFactoryService editorOptionsFactory,
            [ImportMany(typeof(ITaggerProvider))]Lazy<ITaggerProvider, ClassifierMetadata>[] classifierProviders) {
            _editorOptionsFactory = editorOptionsFactory;

            // we use a tagger provider here instead of an IClassifierProvider because the 
            // JS language service doesn't actually implement IClassifierProvider and instead implemnets
            // ITaggerProvider<ClassificationTag> instead.  We can get those tags via IClassifierAggregatorService
            // but that merges together adjacent tokens of the same type, so we go straight to the
            // source here.
            _taggerProvider = classifierProviders.Where(
                provider =>
                    provider.Metadata.ContentTypes.Contains(NodejsConstants.JavaScript) &&
                    provider.Metadata.TagTypes.Any(tagType => tagType.IsSubclassOf(typeof(ClassificationTag)))
            ).First().Value;
        }

        #region ISmartIndentProvider Members

        public ISmartIndent CreateSmartIndent(ITextView textView) {
            return new SmartIndent(
                textView,
                _editorOptionsFactory.GetOptions(textView),
                _taggerProvider.CreateTagger<ClassificationTag>(textView.TextBuffer)
            );
        }

        #endregion

        sealed class SmartIndent : ISmartIndent {
            private readonly ITextView _textView;
            private readonly IEditorOptions _editorOptions;
            private readonly ITagger<ClassificationTag> _classifier;

            public SmartIndent(ITextView textView, IEditorOptions editorOptions, ITagger<ClassificationTag> classifier) {
                _textView = textView;
                _editorOptions = editorOptions;
                _classifier = classifier;
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
                //public bool ShouldIndentAfter;
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
                            token.Tag.ClassificationType.IsOfType("keyword") &&
                            IsStmtKeyword(token.Span.GetText())) {
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
                            indentStack.Push(current);
                            var start = token.Span.Start;
                            current = new LineInfo {
                                Indentation = GetLeadingWhiteSpace(start.GetContainingLine().GetText()) + _editorOptions.GetTabSize()
                            };
                        } else if (IsCloseGrouping(token)) {
                            if (indentStack.Count > 0) {
                                current = indentStack.Pop();
                            } else {
                                current.NeedsUpdate = true;
                            }
                        } else if (IsMultilineStringOrComment(token)) {
                            while (token != null && tokenStack.Count > 0) {
                                token = tokenStack.Pop();
                            }
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

            private bool IsStmtKeyword(string keyword) {
                return _stmtKeywords.Contains(keyword);
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

            private int? SameLevel(ITagSpan<ClassificationTag> current) {
                return GetLeadingWhiteSpace(current.Span.Start.GetContainingLine().GetText());
            }

            private int? IndentLevel(ITagSpan<ClassificationTag> current) {
                var res = GetLeadingWhiteSpace(current.Span.Start.GetContainingLine().GetText());
                if (res != null) {
                    return res + _editorOptions.GetTabSize();
                }
                return null;
            }

            private int? DedentLevel(ITagSpan<ClassificationTag> current) {
                var res = GetLeadingWhiteSpace(current.Span.Start.GetContainingLine().GetText());
                if (res != null && res.Value > _editorOptions.GetTabSize()) {
                    return res - _editorOptions.GetTabSize();
                }
                return null;
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

            private int? GetLeadingWhiteSpace(string text) {
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
}

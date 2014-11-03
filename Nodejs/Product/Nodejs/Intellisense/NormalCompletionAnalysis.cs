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
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    internal class NormalCompletionAnalysis : CompletionAnalysis {
        private readonly VsProjectAnalyzer _analyzer;
        private readonly ITextSnapshot _snapshot;
        private readonly ITrackingSpan _applicableSpan;
        private readonly ITextBuffer _textBuffer;
        private readonly GetMemberOptions _options;

        public NormalCompletionAnalysis(VsProjectAnalyzer vsProjectAnalyzer, ITextSnapshot snapshot, VisualStudio.Text.ITrackingSpan applicableSpan, VisualStudio.Text.ITextBuffer textBuffer, GetMemberOptions options)
            : base(applicableSpan, textBuffer) {
            _analyzer = vsProjectAnalyzer;
            _snapshot = snapshot;
            _applicableSpan = applicableSpan;
            _textBuffer = textBuffer;
            _options = options;
        }

        public override CompletionSet GetCompletions(IGlyphService glyphService) {
            var analysis = GetAnalysisEntry();

            var members = Enumerable.Empty<MemberResult>();

            var text = PrecedingExpression;
            if (!string.IsNullOrEmpty(text)) {
                string fixedText = FixupCompletionText(text);
                if (analysis != null && fixedText != null) {
                    members = analysis.GetMembersByIndex(
                        fixedText,
                        VsProjectAnalyzer.TranslateIndex(
                            Span.GetEndPoint(_snapshot).Position,
                            _snapshot,
                            analysis
                        ),
                        _options
                    );
                }
            } else if (analysis != null) {
                members = analysis.GetAllAvailableMembersByIndex(
                    VsProjectAnalyzer.TranslateIndex(
                        Span.GetStartPoint(_snapshot).Position,
                        _snapshot,
                        analysis
                    ),
                    _options
                );
            }

            return new FuzzyCompletionSet(
                "Node.js",
                "Node.js",
                Span,
                members.Select(m => JsCompletion(glyphService, m)),
                CompletionComparer.UnderscoresLast,
                matchInsertionText: true
            );
        }

        internal static DynamicallyVisibleCompletion JsCompletion(IGlyphService service, MemberResult memberResult) {
            return new DynamicallyVisibleCompletion(memberResult.Name,
                memberResult.Completion,
                () => memberResult.Documentation,
                () => service.GetGlyph(GetGlyphGroup(memberResult), StandardGlyphItem.GlyphItemPublic),
                String.Empty
            );
        }

        private string FixupCompletionText(string exprText) {
            if (exprText.EndsWith(".")) {
                exprText = exprText.Substring(0, exprText.Length - 1);
                if (exprText.Length == 0) {
                    // don't return all available members on empty dot.
                    return null;
                }
            } else {
                int cut = exprText.LastIndexOfAny(new[] { '.', ']', ')' });
                if (cut != -1) {
                    exprText = exprText.Substring(0, cut);
                } else {
                    exprText = String.Empty;
                }
            }
            return exprText;
        }

        internal string PrecedingExpression {
            get {
                var startSpan = _snapshot.CreateTrackingSpan(Span.GetSpan(_snapshot).Start.Position, 0, SpanTrackingMode.EdgeInclusive);
                var parser = new ReverseExpressionParser(_snapshot, _snapshot.TextBuffer, startSpan);
                var sourceSpan = parser.GetExpressionRange();
                if (sourceSpan.HasValue && sourceSpan.Value.Length > 0) {
                    return sourceSpan.Value.GetText();
                }
                return string.Empty;
            }
        }

    }
}

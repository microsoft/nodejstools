using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    class NormalCompletionAnalysis : CompletionAnalysis {
        private readonly VsProjectAnalyzer _analyzer;
        private readonly ITextSnapshot _snapshot;
        private readonly ITrackingSpan _applicableSpan;
        private readonly ITextBuffer _textBuffer;

        public NormalCompletionAnalysis(VsProjectAnalyzer vsProjectAnalyzer, ITextSnapshot snapshot, VisualStudio.Text.ITrackingSpan applicableSpan, VisualStudio.Text.ITextBuffer textBuffer)
            : base(applicableSpan, textBuffer) {
            _analyzer = vsProjectAnalyzer;
            _snapshot = snapshot;
            _applicableSpan = applicableSpan;
            _textBuffer = textBuffer;
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
                        )
                    );
                }
            }

            return new FuzzyCompletionSet(
                "Node.js",
                "Node.js",
                Span,
                members.Select(m => JsCompletion(glyphService, m)),
                CompletionComparer.UnderscoresLast);
        }

        internal static DynamicallyVisibleCompletion JsCompletion(IGlyphService service, MemberResult memberResult) {
            return new DynamicallyVisibleCompletion(memberResult.Name,
                memberResult.Completion,
                () => memberResult.Documentation,
                () => service.GetGlyph(GetGlyphGroup(memberResult), StandardGlyphItem.GlyphItemPublic),
                ""
            );
        }

        private static StandardGlyphGroup GetGlyphGroup(MemberResult result) {
            switch(result.MemberType) {
                case JsMemberType.Function:
                    return StandardGlyphGroup.GlyphGroupMethod;
                case JsMemberType.Keyword:
                    return StandardGlyphGroup.GlyphKeyword;
                case JsMemberType.Module:
                    return StandardGlyphGroup.GlyphGroupModule;
                case JsMemberType.Multiple:
                case JsMemberType.Object:
                    return StandardGlyphGroup.GlyphGroupClass;
                case JsMemberType.Boolean:
                case JsMemberType.String:
                case JsMemberType.Number:
                    return StandardGlyphGroup.GlyphGroupValueType;
                case JsMemberType.Undefined:
                    return StandardGlyphGroup.GlyphGroupException;
                case JsMemberType.Null:
                    return StandardGlyphGroup.GlyphGroupConstant;
                default:
                    return StandardGlyphGroup.GlyphGroupUnknown;
            }
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

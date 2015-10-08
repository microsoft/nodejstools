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
using System.Collections.Generic;
using System.Linq;
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

        public override CompletionSet GetCompletions(IGlyphService glyphService, IEnumerable<DynamicallyVisibleCompletion> snippetCompletions) {
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
                members.Select(m => JsCompletion(glyphService, m)).Union(snippetCompletions),
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

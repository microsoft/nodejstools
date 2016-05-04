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
using System.Linq;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    internal class RequireCompletionAnalysis : CompletionAnalysis {
        private readonly ITextSnapshot _snapshot;
        private readonly bool _quote;

        public RequireCompletionAnalysis(VsProjectAnalyzer vsProjectAnalyzer, ITextSnapshot snapshot, VisualStudio.Text.ITrackingSpan applicableSpan, VisualStudio.Text.ITextBuffer textBuffer, bool quote)
            : base(applicableSpan, textBuffer) {
            _snapshot = snapshot;
            _quote = quote;
        }

        private static string GetInsertionQuote(bool quote, string filename) {
            return quote ? filename : "\'" + filename + "\'";
        }

        internal static DynamicallyVisibleCompletion JsCompletion(IGlyphService service, MemberResult memberResult, bool quote) {
            return new DynamicallyVisibleCompletion(memberResult.Name,
                GetInsertionQuote(quote, memberResult.Completion),
                memberResult.Documentation,
                service.GetGlyph(GetGlyphGroup(memberResult), StandardGlyphItem.GlyphItemPublic),
                String.Empty
            );
        }

        public override CompletionSet GetCompletions(IGlyphService glyphService) {
            var text = PrecedingExpression;

            var completions = GetModules().Select(m => JsCompletion(glyphService, m, _quote));

            var res = new FuzzyCompletionSet(CompletionSource.NodejsRequireCompletionSetMoniker, "Node.js", Span, completions, CompletionComparer.UnderscoresFirst, true);

            return res;
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

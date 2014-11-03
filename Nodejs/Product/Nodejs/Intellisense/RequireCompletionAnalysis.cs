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
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.NodejsTools.Intellisense {
    internal class RequireCompletionAnalysis : CompletionAnalysis {
        private readonly VsProjectAnalyzer _analyzer;
        private readonly ITextSnapshot _snapshot;
        private readonly ITrackingSpan _applicableSpan;
        private readonly ITextBuffer _textBuffer;
        private readonly bool? _doubleQuote;

        public RequireCompletionAnalysis(VsProjectAnalyzer vsProjectAnalyzer, ITextSnapshot snapshot, VisualStudio.Text.ITrackingSpan applicableSpan, VisualStudio.Text.ITextBuffer textBuffer, bool? doubleQuote)
            : base(applicableSpan, textBuffer) {
            _analyzer = vsProjectAnalyzer;
            _snapshot = snapshot;
            _applicableSpan = applicableSpan;
            _textBuffer = textBuffer;
            _doubleQuote = doubleQuote;
        }

        private static string GetInsertionQuote(bool? doubleQuote, string filename) {
            return doubleQuote == null ?
                "\'" + filename + "\'" :
                doubleQuote.Value ? filename + "\"" : filename + "'";
        }

        internal static DynamicallyVisibleCompletion JsCompletion(IGlyphService service, MemberResult memberResult, bool? doubleQuote) {
            return new DynamicallyVisibleCompletion(memberResult.Name,
                GetInsertionQuote(doubleQuote,memberResult.Completion),
                memberResult.Documentation,
                service.GetGlyph(GetGlyphGroup(memberResult), StandardGlyphItem.GlyphItemPublic),
                String.Empty
            );
        }

        public override CompletionSet GetCompletions(IGlyphService glyphService) {
            var text = PrecedingExpression;

            var completions = GetModules().Select(m => JsCompletion(glyphService, m, _doubleQuote));

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

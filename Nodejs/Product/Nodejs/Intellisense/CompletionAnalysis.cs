using System;
using System.Diagnostics;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Provides various completion services after the text around the current location has been
    /// processed. The completion services are specific to the current context
    /// </summary>
    public class CompletionAnalysis {
        private readonly ITrackingSpan _span;
        private readonly ITextBuffer _textBuffer;
        internal const Int64 TooMuchTime = 50;
        protected static Stopwatch _stopwatch = MakeStopWatch();

        internal static CompletionAnalysis EmptyCompletionContext = new CompletionAnalysis(null, null);

        internal CompletionAnalysis(ITrackingSpan span, ITextBuffer textBuffer) {
            _span = span;
            _textBuffer = textBuffer;
        }

        public ITextBuffer TextBuffer {
            get {
                return _textBuffer;
            }
        }

        public ITrackingSpan Span {
            get {
                return _span;
            }
        }

        public virtual CompletionSet GetCompletions(IGlyphService glyphService) {
            return null;
        }

        private static Stopwatch MakeStopWatch() {
            var res = new Stopwatch();
            res.Start();
            return res;
        }

        internal ModuleAnalysis GetAnalysisEntry() {
            IJsProjectEntry entry;
            return TextBuffer.TryGetJsProjectEntry(out entry) && entry != null ?
                entry.Analysis :
                null;
        }

    }
}

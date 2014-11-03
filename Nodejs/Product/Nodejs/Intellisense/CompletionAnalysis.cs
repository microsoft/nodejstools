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
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

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

        internal static StandardGlyphGroup GetGlyphGroup(MemberResult result) {
            switch (result.MemberType) {
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
        
        internal IEnumerable<MemberResult> GetModules() {
            var analysis = GetAnalysisEntry();

            if (analysis != null) {
                return analysis.GetModules().Distinct(CompletionComparer.MemberEquality);
            }
            return Enumerable.Empty<MemberResult>();
        }
    }
}

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

        public virtual CompletionSet GetCompletions(IGlyphService glyphService, IEnumerable<DynamicallyVisibleCompletion> snippetCompletions) {
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

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
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using FunctionExpression = Microsoft.NodejsTools.Parsing.FunctionExpression;
using FunctionObject = Microsoft.NodejsTools.Parsing.FunctionObject;

namespace Microsoft.NodejsTools.Outlining {
    [Export(typeof(ITaggerProvider)), ContentType(NodejsConstants.Nodejs)]
    [TagType(typeof(IOutliningRegionTag))]
    internal class JavaScriptOutliningTaggerProvider : ITaggerProvider {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag {
            return (ITagger<T>)(buffer.GetOutliningTagger() ?? new OutliningTagger(buffer));
        }

        internal class OutliningTagger : ITagger<IOutliningRegionTag> {
            private readonly ITextBuffer _buffer;
            private bool _enabled, _eventHooked;

            public OutliningTagger(ITextBuffer buffer) {
                _buffer = buffer;
                _buffer.Properties[typeof (OutliningTagger)] = this;

                if (NodejsPackage.Instance != null) {
                    _enabled = NodejsPackage.Instance.AdvancedEditorOptionsPage.EnterOutliningOnOpen;
                }
            }

            public bool Enabled {
                get { return _enabled; }
            }

            public void Enable() {
                _enabled = true;
                OnTagsUpdated();
            }

            public void Disable() {
                _enabled = false;
                OnTagsUpdated();
            }

            private void OnTagsUpdated() {
                var snapshot = _buffer.CurrentSnapshot;
                var tagsChanged = TagsChanged;
                if (tagsChanged != null) {
                    tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
                }
            }

            #region ITagger<IOutliningRegionTag> Members

            public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
                IJsProjectEntry entry;
                if (_enabled && _buffer.TryGetJsProjectEntry(out entry)) {
                    if (!_eventHooked) {
                        entry.NewParseTree += OnNewParseTree;
                        _eventHooked = true;
                    }

                    JsAst ast;
                    IAnalysisCookie cookie;
                    entry.GetTreeAndCookie(out ast, out cookie);
                    var snapCookie = cookie as SnapshotCookie;

                    if (ast != null &&
                        snapCookie != null &&
                        // buffer could have changed if file was closed and reopened
                        snapCookie.Snapshot.TextBuffer == spans[0].Snapshot.TextBuffer) {
                        var visitor = new OutliningVisitor(ast, snapCookie.Snapshot);
                        ast.Walk(visitor);
                        return visitor.OutliningTagSpans;
                    }
                }

                return new ITagSpan<IOutliningRegionTag>[0];
            }

            private void OnNewParseTree(object sender, EventArgs e) {
                IJsProjectEntry entry;
                if (_buffer.TryGetJsProjectEntry(out entry)) {
                    OnTagsUpdated();
                }
            }

            public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

            private class OutliningVisitor : AstVisitor {
                private readonly JsAst _ast;
                private readonly ITextSnapshot _snapshot;
                public readonly List<ITagSpan<IOutliningRegionTag>> OutliningTagSpans;

                public OutliningVisitor(JsAst ast, ITextSnapshot snapshot) {
                    _ast = ast;
                    _snapshot = snapshot;
                    OutliningTagSpans = new List<ITagSpan<IOutliningRegionTag>>();
                }

                public override bool Walk(FunctionObject node) {
                    // FunctionExpressions contain FunctionObjects.
                    // Therefore we only need to handle FunctionObjects to handle both.
                    if (node.Body.Braces != BraceState.StartAndEnd) {
                        return false;
                    }
                    TagSpan span = GetFunctionSpan(_ast, _snapshot, node);
                    OutliningTagSpans.Add(span);
                    return base.Walk(node);
                }

                private static TagSpan GetFunctionSpan(JsAst ast, ITextSnapshot snapshot, FunctionObject functionObject) {
                    IndexSpan indexSpan = functionObject.Body.GetSpan(ast.LocationResolver);
                    return GetTagSpan(snapshot, indexSpan.Start, indexSpan.End, functionObject.ParameterEnd);
                }

                private static TagSpan GetTagSpan(ITextSnapshot snapshot, int start, int end, int headerIndex) {
                    TagSpan tagSpan = null;
                    try {
                        int testLen = headerIndex - start + 1;
                        if (start != -1 && end != -1) {
                            int length = end - start - testLen;
                            if (length > 0) {
                                var span = new Span(start + testLen, length);

                                tagSpan = new TagSpan(
                                    new SnapshotSpan(snapshot, span),
                                    new OutliningTag(snapshot, span, true)
                                    );
                            }
                        }
                    }
                    catch (ArgumentException) {
                        // sometimes the parser gives us bad spans, ignore those and fix the parser
                        Debug.Assert(false, "bad argument when making span/tag");
                    }

                    return tagSpan;
                }
            }

            #endregion
        }
    }

   
    class TagSpan : ITagSpan<IOutliningRegionTag> {
        private readonly SnapshotSpan _span;
        private readonly OutliningTag _tag;

        public TagSpan(SnapshotSpan span, OutliningTag tag) {
            _span = span;
            _tag = tag;
        }

        #region ITagSpan<IOutliningRegionTag> Members

        public SnapshotSpan Span {
            get { return _span; }
        }

        public IOutliningRegionTag Tag {
            get { return _tag; }
        }

        #endregion
    }

    class OutliningTag : IOutliningRegionTag {
        private readonly ITextSnapshot _snapshot;
        private readonly Span _span;
        private readonly bool _isImplementation;

        public OutliningTag(ITextSnapshot iTextSnapshot, Span span, bool isImplementation) {
            _snapshot = iTextSnapshot;
            _span = span;
            _isImplementation = isImplementation;
        }

        #region IOutliningRegionTag Members

        public object CollapsedForm {
            get { return "..."; }
        }

        public object CollapsedHintForm {
            get {
                string collapsedHint = _snapshot.GetText(_span);

                string[] lines = collapsedHint.Split(new[] { "\r\n" }, StringSplitOptions.None);
                // remove any leading white space for the preview
                if (lines.Length > 0) {
                    int smallestWhiteSpace = Int32.MaxValue;
                    foreach (string curLine in lines) {
                        for (int j = 0; j < curLine.Length; j++) {
                            if (curLine[j] != ' ') {
                                smallestWhiteSpace = Math.Min(j, smallestWhiteSpace);
                                break;
                            }
                        }
                    }

                    for (int i = 0; i < lines.Length; i++) {
                        if (lines[i].Length >= smallestWhiteSpace) {
                            lines[i] = lines[i].Substring(smallestWhiteSpace);
                        }
                    }

                    return String.Join("\r\n", lines);
                }
                return collapsedHint;
            }
        }

        public bool IsDefaultCollapsed {
            get { return false; }
        }

        public bool IsImplementation {
            get { return _isImplementation; }
        }

        #endregion
    }
  

    static class OutliningTaggerProviderExtensions {
        public static JavaScriptOutliningTaggerProvider.OutliningTagger GetOutliningTagger(this ITextView self) {
            return self.TextBuffer.GetOutliningTagger();
        }

        public static JavaScriptOutliningTaggerProvider.OutliningTagger GetOutliningTagger(this ITextBuffer self) {
            JavaScriptOutliningTaggerProvider.OutliningTagger res;
            if (self.Properties.TryGetProperty(typeof(JavaScriptOutliningTaggerProvider.OutliningTagger), out res)) {
                return res;
            }
            return null;
        }
    }
}

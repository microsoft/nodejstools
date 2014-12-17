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

using System.Collections.Generic;
using System.Linq;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Formatting {
    /// <summary>
    /// Provides formatting for JavaScript code
    /// </summary>
    internal static class Formatter {
        /// <summary>
        /// Returns a list of edits which should be applied to the specified code.
        /// </summary>
        public static Edit[] GetEditsForDocument(string code, FormattingOptions options = null) {
            using (new DebugTimer("FormatDocument")) {
                var edits = GetEdits(code, options);
                return edits.ToArray();
            }
        }

        public static Edit[] GetEditsAfterKeystroke(string code, int position, char ch, FormattingOptions options = null) {
            using (new DebugTimer("FormatKeyStroke")) {
                if (ch == ';' || ch == '}') {
                    var ast = new JSParser(code).Parse(new CodeSettings() { AllowShebangLine = true });
                    var visitor = new RangeVisitor(ch, position, ast);
                    ast.Walk(visitor);
                    if (visitor.Span != default(IndexSpan)) {
                        return FilterRange(
                            visitor.Span.Start,
                            visitor.Span.End,
                            GetEdits(code, options, ast)
                        );
                    }
                }
                return new Edit[0];
            }
        }

        public static Edit[] GetEditsAfterEnter(string code, int start, int end, FormattingOptions options = null) {
            using (new DebugTimer("FormatRange")) {
                var edits = GetEdits(code, options, true);
                return FilterRange(start, end, edits);
            }
        }

        public static Edit[] GetEditsForRange(string code, int start, int end, FormattingOptions options = null) {
            using (new DebugTimer("FormatRange")) {
                var edits = GetEdits(code, options);
                return FilterRange(start, end, edits);
            }
        }

        class RangeVisitor : AstVisitor {
            private readonly char _typedChar;
            private readonly int _position;
            private readonly JsAst _tree;
            public IndexSpan Span;

            public RangeVisitor(char typedChar, int position, JsAst tree) {
                _typedChar = typedChar;
                _position = position;
                _tree = tree;
            }

            public override bool Walk(ContinueNode node) {
                CheckStatement(node);
                return base.Walk(node);
            }

            public override bool Walk(Break node) {
                CheckStatement(node);
                return base.Walk(node);
            }

            public override bool Walk(ExpressionStatement node) {
                CheckStatement(node);
                return base.Walk(node);
            }

            public override bool Walk(Var node) {
                CheckStatement(node);
                return base.Walk(node);
            }

            public override bool Walk(DebuggerNode node) {
                CheckStatement(node);
                return base.Walk(node);
            }

            public override bool Walk(ThrowNode node) {
                CheckStatement(node);
                return base.Walk(node);
            }

            public override bool Walk(ReturnNode node) {
                CheckStatement(node);
                return base.Walk(node);
            }

            private void CheckStatement(Statement node) {
                if (_typedChar == ';' && node.GetEndIndex(_tree.LocationResolver) == _position) {
                    // if(1)if(1)if(1)if(1)x+=2;
                    // We want to reformat all of the if statements that are nested
                    // so walk up the parent nodes as long as they are all terminated
                    // at the same semicolon.                   
                    Span = GetTargetStatement(node).GetSpan(_tree.LocationResolver);
                }
            }

            private Statement GetTargetStatement(Statement node) {
                Statement targetNode = node;
                while (targetNode.Parent != null &&
                    targetNode.Parent.GetEndIndex(_tree.LocationResolver) == node.GetEndIndex(_tree.LocationResolver)) {
                    if (targetNode.Parent != null && targetNode.Parent.Parent is JsAst) {
                        // https://nodejstools.codeplex.com/workitem/1102
                        // We don't want to reformat the entire document just because someone
                        // is doing something at the end of the document.
                        break;
                    }
                    targetNode = targetNode.Parent;
                }
                return targetNode;
            }

            public override bool Walk(Switch node) {
                if (_typedChar == '}' && node.GetEndIndex(_tree.LocationResolver) == _position) {
                    Span = GetTargetStatement(node).GetSpan(_tree.LocationResolver);
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(TryNode node) {
                if (CheckBlock(node.TryBlock) ||
                    CheckBlock(node.FinallyBlock) ||
                    CheckBlock(node.CatchBlock)) {
                        Span = GetTargetStatement(node).GetSpan(_tree.LocationResolver);
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(ForIn node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).GetSpan(_tree.LocationResolver);
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(ForNode node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).GetSpan(_tree.LocationResolver);
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(WhileNode node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).GetSpan(_tree.LocationResolver);
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(WithNode node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).GetSpan(_tree.LocationResolver);
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(Block block) {
                if (CheckBlock(block)) {
                    Span = GetTargetStatement(block).GetSpan(_tree.LocationResolver);
                    return false;
                }
                return true;
            }

            public override bool Walk(ObjectLiteral node) {
                if (_typedChar == '}' && node.GetEndIndex(_tree.LocationResolver) == _position) {
                    Span = node.GetSpan(_tree.LocationResolver);
                    return false;
                }
                return base.Walk(node);
            }

            private bool CheckBlock(Block block) {
                if (_typedChar == '}' && 
                    block != null && 
                    block.Braces != BraceState.None && 
                    block.GetEndIndex(_tree.LocationResolver) == _position) {
                    return true;
                }
                return false;
            }
        }

        private static Edit[] FilterRange(int start, int end, List<Edit> edits) {
            return edits.Where(edit => edit.Start + edit.Length >= start && edit.Start + edit.Length < end).ToArray();
        }

        private static List<Edit> GetEdits(string code, FormattingOptions options, bool onEnter = false) {
            var ast = new JSParser(code).Parse(new CodeSettings() { AllowShebangLine = true });
            return GetEdits(code, options, ast, onEnter);
        }

        private static List<Edit> GetEdits(string code, FormattingOptions options, JsAst ast, bool onEnter = false) {
            var visitor = new FormattingVisitor(code, ast, options, onEnter);
            visitor.Format(ast);
            return visitor.Edits;
        }
    }
}

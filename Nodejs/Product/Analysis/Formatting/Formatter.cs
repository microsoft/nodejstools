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

using System.Collections.Generic;
using System.Linq;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Formatting {
    /// <summary>
    /// Provides formatting for JavaScript code
    /// </summary>
    public static class Formatter {
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
                    var visitor = new RangeVisitor(ch, position);
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
            public IndexSpan Span;

            public RangeVisitor(char typedChar, int position) {
                _typedChar = typedChar;
                _position = position;
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
                if (_typedChar == ';' && node.EndIndex == _position) {
                    // if(1)if(1)if(1)if(1)x+=2;
                    // We want to reformat all of the if statements that are nested
                    // so walk up the parent nodes as long as they are all terminated
                    // at the same semicolon.                   
                    Span = GetTargetStatement(node).Span;
                }
            }

            private static Statement GetTargetStatement(Statement node) {
                Statement targetNode = node;
                while (targetNode.Parent != null &&
                    targetNode.Parent.EndIndex == node.EndIndex) {
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
                if (_typedChar == '}' && node.EndIndex == _position) {
                    Span = GetTargetStatement(node).Span;
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(TryNode node) {
                if (CheckBlock(node.TryBlock) ||
                    CheckBlock(node.FinallyBlock) ||
                    CheckBlock(node.CatchBlock)) {
                    Span = GetTargetStatement(node).Span;
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(ForIn node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).Span;
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(ForNode node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).Span;
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(WhileNode node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).Span;
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(WithNode node) {
                if (CheckBlock(node.Body)) {
                    Span = GetTargetStatement(node).Span;
                    return false;
                }
                return base.Walk(node);
            }

            public override bool Walk(Block block) {
                if (CheckBlock(block)) {
                    Span = GetTargetStatement(block).Span;
                    return false;
                }
                return true;
            }

            public override bool Walk(ObjectLiteral node) {
                if (_typedChar == '}' && node.EndIndex == _position) {
                    Span = node.Span;
                    return false;
                }
                return base.Walk(node);
            }

            private bool CheckBlock(Block block) {
                if (_typedChar == '}' && block != null && block.Braces != BraceState.None && block.EndIndex == _position) {
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
            var visitor = new FormattingVisitor(code, options, onEnter);
            visitor.Format(ast);
            return visitor.Edits;
        }
    }
}

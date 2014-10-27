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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Formatting {
    sealed class FormattingVisitor : AstVisitor {
        private readonly string _code;
        private readonly JsAst _tree;
        private readonly FormattingOptions _options;
        private readonly List<Edit> _edits = new List<Edit>();
        private readonly List<string> _whitespace = new List<string>();
        private readonly bool _onEnter;
        private int _indentLevel;

        // various terminators when we're replacing formatting
        private static char[] _semicolon = new[] { ';' };
        private static char[] _newlines = new[] { '\n', '\r' };
        private static char[] _comma = new[] { ',' };
        private static char[] _openParen = new[] { '(' };
        private static char[] _closeBrace = new[] { '}' };
        private static char[] _openBracket = new[] { '[' };
        public FormattingVisitor(string code, JsAst tree, FormattingOptions options = null, bool onEnter = false) {
            _code = code;
            _options = options ?? new FormattingOptions();
            _onEnter = onEnter;
            _tree = tree;
        }

        public void Format(JsAst ast) {
            RemoveTrailingWhiteSpace(ast.GetStartIndex(_tree.LocationResolver));
            WalkStatements(ast, ast.Block.Statements, false);
            if (ast.Block.Count > 0) {
                FixStatementIndentation(ast.Block[ast.Block.Count - 1].GetEndIndex(_tree.LocationResolver), ast.GetEndIndex(_tree.LocationResolver));
            }
            ReplacePreceedingWhiteSpace(ast.GetEndIndex(_tree.LocationResolver));
        }

        public List<Edit> Edits {
            get {
                return _edits;
            }
        }

        #region Complex Statements

        public override bool Walk(ForNode node) {
            ReplaceControlFlowWhiteSpace(node, "for".Length);

            if (node.Initializer != null) {
                ReplacePreceedingWhiteSpace(
                    node.Initializer.GetStartIndex(_tree.LocationResolver),
                    _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : "",
                    _openParen
                );

                node.Initializer.Walk(this);
            }

            if (node.Condition != null) {
                ReplacePreceedingWhiteSpace(
                    node.Condition.GetStartIndex(_tree.LocationResolver),
                    _options.SpaceAfterSemiColonInFor ? " " : "",
                    _semicolon
                );

                node.Condition.Walk(this);
            }
            if (node.Incrementer != null) {
                ReplacePreceedingWhiteSpace(node.Incrementer.GetStartIndex(_tree.LocationResolver), _options.SpaceAfterSemiColonInFor ? " " : "", _semicolon);
                node.Incrementer.Walk(this);

                ReplaceFollowingWhiteSpace(
                    node.Incrementer.GetEndIndex(_tree.LocationResolver),
                    _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
                );
            }

            if (node.HeaderEnd != -1) {
                WalkFlowControlBlockWithOptionalParens(node.Body, node.HeaderEnd, false);
            }
            return false;
        }

        private void WalkFlowControlBlockWithOptionalParens(Block block, int previousExpressionEnd, bool inParens) {
            WalkFlowControlBlockWithOptionalParens(block, ((Statement)block.Parent).GetStartIndex(_tree.LocationResolver), previousExpressionEnd, inParens);
        }

        private void WalkFlowControlBlockWithOptionalParens(Block block, int startIndex, int previousExpressionEnd, bool inParens) {
            if (block != null) {
                if (block.Braces == BraceState.None) {
                    // braces are omitted...

                    // if (foo) 
                    //      blah
                    // vs
                    // if (foo) blah

                    // TODO: https://nodejstools.codeplex.com/workitem/1475 causes a failure as our end < start.  Parser is failing and until fixed will cause a failure here.
                    bool multiLine = ContainsLineFeed(previousExpressionEnd, block.GetStartIndex(_tree.LocationResolver));
                    if (multiLine) {
                        // remove trailing whitespace at the end of this line
                        bool followedBySingleLineComment;
                        int startOfWhiteSpace, whiteSpaceCount;
                        ParseEndOfLine(previousExpressionEnd, inParens, out followedBySingleLineComment, out startOfWhiteSpace, out whiteSpaceCount);
                        if (startOfWhiteSpace != -1) {
                            _edits.Add(new Edit(startOfWhiteSpace, whiteSpaceCount, ""));
                        }
                        Indent();
                    }

                    WalkStatements(startIndex, block.Statements, false);
                    if (multiLine) {
                        Dedent();
                    }
                } else {
                    ReplacePreceedingIncludingNewLines(block.GetStartIndex(_tree.LocationResolver), GetFlowControlBraceInsertion(previousExpressionEnd, false));

                    WalkBlock(block);
                }
            }
        }

        public override bool Walk(ForIn node) {
            ReplaceControlFlowWhiteSpace(node, "for".Length);

            ReplacePreceedingWhiteSpace(
                node.Variable.GetStartIndex(_tree.LocationResolver),
                _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : "",
                _openParen
            );
            node.Variable.Walk(this);

            ReplaceFollowingWhiteSpace(
                node.Variable.GetEndIndex(_tree.LocationResolver),
                " "
            );

            ReplacePreceedingWhiteSpace(
                node.Collection.GetStartIndex(_tree.LocationResolver),
                " "
            );

            node.Collection.Walk(this);

            ReplaceFollowingWhiteSpace(
                node.Collection.GetEndIndex(_tree.LocationResolver),
                _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
            );

            WalkFlowControlBlockWithOptionalParens(node.Body, node.Collection.GetEndIndex(_tree.LocationResolver), true);
            return false;
        }

        public override bool Walk(IfNode node) {
            ReplaceControlFlowWhiteSpace(node, "if".Length);

            EnsureSpacesAroundParenthesisedExpression(node.Condition);

            if (node.TrueBlock != null) {
                WalkFlowControlBlockWithOptionalParens(node.TrueBlock, node.Condition.GetEndIndex(_tree.LocationResolver), true);
            }
            if (node.FalseBlock != null) {
                ReplacePreceedingWhiteSpaceMaybeMultiline(node.ElseStart);
                WalkFlowControlBlockWithOptionalParens(node.FalseBlock, node.ElseStart, node.ElseStart + "else".Length, false);
            }
            return false;
        }

        public override bool Walk(TryNode node) {
            ReplacePreceedingIncludingNewLines(
                node.TryBlock.GetStartIndex(
                _tree.LocationResolver),
                GetFlowControlBraceInsertion(
                    node.GetStartIndex(_tree.LocationResolver) + "try".Length,
                    false
                )
            );
            WalkBlock(node.TryBlock);
            if (node.CatchParameter != null) {
                if (node.CatchStart != -1) {
                    ReplacePreceedingWhiteSpace(node.CatchStart, " ", _closeBrace);
                    ReplaceFollowingWhiteSpace(node.CatchStart + "catch".Length, " ");
                }

                ReplacePreceedingWhiteSpace(
                    node.CatchParameter.GetStartIndex(_tree.LocationResolver),
                    _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : "",
                    _openParen
                );

                ReplaceFollowingWhiteSpace(
                    node.CatchParameter.GetEndIndex(_tree.LocationResolver),
                    _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
                );

                ReplacePreceedingIncludingNewLines(
                    node.CatchBlock.GetStartIndex(_tree.LocationResolver),
                    GetFlowControlBraceInsertion(node.CatchParameter.GetEndIndex(_tree.LocationResolver), true)
                );
                WalkBlock(node.CatchBlock);
            }

            if (node.FinallyBlock != null && node.FinallyStart != -1) {
                if (node.FinallyStart != -1) {
                    ReplacePreceedingWhiteSpace(node.FinallyStart, " ", _closeBrace);
                }
                ReplacePreceedingIncludingNewLines(
                    node.FinallyBlock.GetStartIndex(_tree.LocationResolver),
                    GetFlowControlBraceInsertion(node.FinallyStart + "finally".Length, false)
                );
                WalkBlock(node.FinallyBlock);
            }
            return false;
        }

        public override bool Walk(Microsoft.NodejsTools.Parsing.Switch node) {
            ReplaceControlFlowWhiteSpace(node, "switch".Length);

            EnsureSpacesAroundParenthesisedExpression(node.Expression);
            bool isMultiLine = false;
            if (node.BlockStart != -1) {
                ReplacePreceedingIncludingNewLines(node.BlockStart, GetFlowControlBraceInsertion(node.Expression.GetEndIndex(_tree.LocationResolver), true));

                isMultiLine = ContainsLineFeed(node.BlockStart, node.GetEndIndex(_tree.LocationResolver));
            }

            // very similar to walking a block w/o a block
            Indent();
            for (int i = 0; i < node.Cases.Length; i++) {

                var caseNode = node.Cases[i];

                if (i == 0 && isMultiLine) {
                    EnsureNewLinePreceeding(caseNode.GetStartIndex(_tree.LocationResolver));
                } else {
                    ReplacePreceedingWhiteSpace(caseNode.GetStartIndex(_tree.LocationResolver));
                }

                if (caseNode.CaseValue != null) {
                    ReplacePreceedingWhiteSpace(caseNode.CaseValue.GetStartIndex(_tree.LocationResolver), " ");
                    caseNode.CaseValue.Walk(this);
                    ReplaceFollowingWhiteSpace(caseNode.CaseValue.GetEndIndex(_tree.LocationResolver), "");
                }

                if (caseNode.ColonIndex != -1 &&
                    caseNode.Statements.Count > 0 &&
                    ContainsLineFeed(caseNode.ColonIndex, caseNode.Statements[0].GetStartIndex(_tree.LocationResolver))) {
                    ReplaceFollowingWhiteSpace(caseNode.ColonIndex + ":".Length, "");
                }

                bool indent = caseNode.Statements.Count == 0 ||
                              ShouldIndentForChild(caseNode, caseNode.Statements[0]);
                if (indent) {
                    Indent();
                }
                WalkStatements(
                    caseNode,
                    caseNode.Statements.Statements,
                    false
                );
                if (indent) {
                    Dedent();
                }
            }
            Dedent();

            ReplacePreceedingWhiteSpace(node.GetEndIndex(_tree.LocationResolver) - 1);

            return false;
        }

        public override bool Walk(DoWhile node) {
            WalkFlowControlBlockWithOptionalParens(node.Body, node.GetStartIndex(_tree.LocationResolver) + "do".Length, false);

            ReplaceFollowingWhiteSpace(node.Body.GetEndIndex(_tree.LocationResolver), " ");

            EnsureSpacesAroundParenthesisedExpression(node.Condition);

            return false;
        }

        public override bool Walk(WhileNode node) {
            ReplaceControlFlowWhiteSpace(node, "while".Length);

            EnsureSpacesAroundParenthesisedExpression(node.Condition);

            if (node.Body != null) {
                WalkFlowControlBlockWithOptionalParens(node.Body, node.Condition.GetEndIndex(_tree.LocationResolver), true);
            }
            return false;
        }

        public override bool Walk(WithNode node) {
            ReplaceControlFlowWhiteSpace(node, "with".Length);

            EnsureSpacesAroundParenthesisedExpression(node.WithObject);

            WalkFlowControlBlockWithOptionalParens(node.Body, node.WithObject.GetEndIndex(_tree.LocationResolver), true);
            return false;
        }

        public override bool Walk(FunctionObject node) {
            if (node.Name == null) {
                ReplaceFollowingWhiteSpace(
                    node.GetStartIndex(_tree.LocationResolver) + "function".Length,
                    _options.SpaceAfterFunctionInAnonymousFunctions ? " " : ""
                );
            } else {
                ReplaceFollowingWhiteSpace(
                    node.GetNameSpan(_tree.LocationResolver).End,
                    ""
                );
            }

            if (node.ParameterDeclarations != null && node.ParameterDeclarations.Length > 0) {
                ReplaceFollowingWhiteSpace(
                    node.ParameterStart + 1,
                    _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
                );

                for (int i = 1; i < node.ParameterDeclarations.Length; i++) {
                    ReplacePreceedingWhiteSpace(node.ParameterDeclarations[i].GetStartIndex(_tree.LocationResolver), _options.SpaceAfterComma ? " " : "", _comma);
                }

                ReplacePreceedingWhiteSpace(
                    node.ParameterEnd - 1,
                    _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
                );
            } else {
                ReplaceFollowingWhiteSpace(
                    node.ParameterStart + 1,
                    ""
                );
            }

            if (!_onEnter) {
                ReplacePreceedingIncludingNewLines(
                    node.Body.GetStartIndex(_tree.LocationResolver),
                    _options.OpenBracesOnNewLineForFunctions || FollowedBySingleLineComment(node.ParameterEnd, false) ?
                        ReplaceWith.InsertNewLineAndIndentation :
                        ReplaceWith.InsertSpace
                );
            }

            WalkBlock(node.Body);

            return false;
        }

        public override bool Walk(Block node) {
            Debug.Assert(node.Braces != BraceState.None);
            WalkBlock(node);
            return false;
        }

        /// <summary>
        /// Replaces the whitespace for a control flow node.  Updates the current indentation
        /// level and then updates the whitespace after the keyword based upon the format
        /// options.
        /// </summary>
        private void ReplaceControlFlowWhiteSpace(Statement node, int keywordLength) {
            ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + keywordLength, _options.SpaceAfterKeywordsInControlFlowStatements ? " " : "");
        }

        #endregion

        #region Simple Statements

        public override bool Walk(LabeledStatement node) {
            if (node.Statement != null) {
                // don't indent block statements that start on the same line
                // as the label such as:
                // label: {
                //      code
                // }
                var block = node.Statement;

                bool indent = ShouldIndentForChild(node, block);
                if (indent) {
                    Indent();
                }
                ReplacePreceedingWhiteSpaceMaybeMultiline(node.Statement.GetStartIndex(_tree.LocationResolver));
                node.Statement.Walk(this);
                if (indent) {
                    Dedent();
                }
            }
            return false;
        }

        private bool ShouldIndentForChild(Node parent, Node block) {
            // if the child is a block which has braces and starts on the
            // same line as our parent we don't want to indent, for example:
            // case foo: {
            //     code
            // }
            if (block is Block) {
                return ((Block)block).Braces == BraceState.None ||
                    ContainsLineFeed(parent.GetStartIndex(_tree.LocationResolver), block.GetStartIndex(_tree.LocationResolver));
            } else if (block is ExpressionStatement) {
                return ShouldIndentForChild(parent, ((ExpressionStatement)block).Expression);
            } else if (IsMultiLineBracketedNode(block)) {
                if (parent is CallNode && ((CallNode)parent).Arguments.Count() > 1) {
                    // For indenting any child of call node we really are if the first argument is on the next line
                    // or the same line as the parent.  For this case, we completely ignore the block argument itself.
                    return ContainsLineFeed(parent.GetStartIndex(_tree.LocationResolver), ((CallNode)parent).Arguments[0].GetStartIndex(_tree.LocationResolver));
                }

                return ContainsLineFeed(parent.GetStartIndex(_tree.LocationResolver), block.GetStartIndex(_tree.LocationResolver));
            }

            return true;
        }

        private static bool IsMultiLineBracketedNode(Node block) {
            return block is ArrayLiteral ||
                block is ObjectLiteral ||
                block is FunctionExpression ||
                block is FunctionObject;
        }

        public override bool Walk(Break node) {
            if (node.Label != null) {
                ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "break".Length, " ");
            }
            RemoveSemiColonWhiteSpace(node.GetEndIndex(_tree.LocationResolver));
            return base.Walk(node);
        }

        /// <summary>
        /// Removes any white space proceeding a semi colon
        /// </summary>
        /// <param name="endIndex"></param>
        private void RemoveSemiColonWhiteSpace(int endIndex) {
            if (_code[endIndex - 1] == ';') {
                ReplacePreceedingWhiteSpaceMaybeMultiline(endIndex - 1, empty: true);
            }
        }

        public override bool Walk(ContinueNode node) {
            if (node.Label != null) {
                ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "continue".Length, " ");
            }
            RemoveSemiColonWhiteSpace(node.GetEndIndex(_tree.LocationResolver));
            return base.Walk(node);
        }

        public override bool Walk(DebuggerNode node) {
            ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "debugger".Length, "");
            return base.Walk(node);
        }

        public override bool Walk(Var node) {
            ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "var".Length, " ");

            if (node.Count > 0) {

                FormatVariableDeclaration(node[0]);

                IndentSpaces(4);
                for (int i = 1; i < node.Count; i++) {
                    var curDecl = node[i];

                    ReplacePreceedingWhiteSpaceMaybeMultiline(curDecl.GetSpan(_tree.LocationResolver).Start);

                    FormatVariableDeclaration(curDecl);
                }
                DedentSpaces(4);

                if (!node[node.Count - 1].HasInitializer) {
                    // if we have an initializer the whitespace was
                    // cleared between the end of the initializer and the
                    // semicolon
                    RemoveSemiColonWhiteSpace(node.GetEndIndex(_tree.LocationResolver));
                }
            }

            return false;
        }

        public override bool Walk(ReturnNode node) {
            if (node.Operand != null) {
                ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "return".Length, " ");
                node.Operand.Walk(this);
            }
            RemoveSemiColonWhiteSpace(node.GetEndIndex(_tree.LocationResolver));
            return false;
        }

        public override bool Walk(YieldExpression node) {
            if (node.Operand != null) {
                ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "yield".Length, " ");
                node.Operand.Walk(this);
            }
            RemoveSemiColonWhiteSpace(node.GetEndIndex(_tree.LocationResolver));
            return false;
        }

        public override bool Walk(ExpressionStatement node) {
            node.Expression.Walk(this);
            RemoveSemiColonWhiteSpace(node.GetEndIndex(_tree.LocationResolver));
            return false;
        }

        public override bool Walk(ThrowNode node) {
            if (node.Operand != null) {
                ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "throw".Length, " ");
            }
            RemoveSemiColonWhiteSpace(node.GetEndIndex(_tree.LocationResolver));
            return base.Walk(node);
        }

        #endregion

        #region Expressions

        public override bool Walk(ObjectLiteralProperty node) {
            if (node.Name is GetterSetter) {
                ReplaceFollowingWhiteSpace(node.Name.GetStartIndex(_tree.LocationResolver) + "get".Length, " ");
                node.Value.Walk(this);
            } else {
                node.Name.Walk(this);
                ReplacePreceedingWhiteSpace(node.Value.GetStartIndex(_tree.LocationResolver), " ");
                node.Value.Walk(this);
            }
            return false;
        }

        public override bool Walk(ObjectLiteral node) {
            if (node.Properties.Length == 0) {
                ReplacePreceedingWhiteSpace(node.GetEndIndex(_tree.LocationResolver) - 1, "");
            } else {
                Indent();
                bool isMultiLine = ContainsLineFeed(node.GetStartIndex(_tree.LocationResolver), node.GetEndIndex(_tree.LocationResolver));
                if (node.Properties.Length > 0) {
                    if (isMultiLine) {
                        // multiline block statement, make sure the 1st statement
                        // starts on a new line
                        EnsureNewLineFollowing(node.GetStartIndex(_tree.LocationResolver) + "{".Length);
                    }

                    WalkStatements(node, node.Properties, isMultiLine);
                }
                Dedent();

                if (isMultiLine) {
                    ReplacePreceedingIncludingNewLines(node.GetEndIndex(_tree.LocationResolver) - 1, ReplaceWith.InsertNewLineAndIndentation);
                } else {
                    ReplacePreceedingWhiteSpace(node.GetEndIndex(_tree.LocationResolver) - 1, " ");
                }
            }
            return false;
        }

        public override bool Walk(ArrayLiteral node) {
            if (node.Elements.Length == 0) {
                ReplacePreceedingWhiteSpace(node.GetEndIndex(_tree.LocationResolver) - 1, "");
            } else {
                Indent();
                // Only correct indentation if we're correcting it for every element...
                // var x = [[1,2],
                //          [...
                //
                // vs.
                // var x = [
                //              [1,2],
                //              [2,3]
                //
                // If we fix up the 1st one we misalign the users indentation

                bool firstElementOnNewLine = ContainsLineFeed(node.GetStartIndex(_tree.LocationResolver), node.Elements[0].GetStartIndex(_tree.LocationResolver));

                for (int i = 0; i < node.Elements.Length; i++) {
                    var curExpr = node.Elements[i];

                    // Fix spacing of commas and '['
                    if (i == 0) {
                        // There should be no spacing between the [ and the first element
                        ReplacePreceedingWhiteSpace(curExpr.GetStartIndex(_tree.LocationResolver), "", _openBracket);
                    } else {
                        ReplacePreceedingWhiteSpace(
                            curExpr.GetStartIndex(_tree.LocationResolver),
                            _options.SpaceAfterComma ? " " : string.Empty,
                            _comma);
                    }

                    // if we have elements on separate lines but the first element has a line feed (separate from '[')
                    if (firstElementOnNewLine) {
                        ReplacePreceedingWhiteSpace(curExpr.GetStartIndex(_tree.LocationResolver));
                    }
                    curExpr.Walk(this);
                }
                Dedent();

                if (ContainsLineFeed(node.Elements[node.Elements.Length - 1].GetEndIndex(_tree.LocationResolver), node.GetEndIndex(_tree.LocationResolver))) {
                    ReplacePreceedingIncludingNewLines(node.GetEndIndex(_tree.LocationResolver) - 1, ReplaceWith.InsertNewLineAndIndentation);
                } else {
                    ReplacePreceedingWhiteSpace(node.GetEndIndex(_tree.LocationResolver) - 1, "");
                }
            }
            return false;
        }

        public override bool Walk(GroupingOperator node) {
            ReplaceFollowingWhiteSpace(
                node.GetStartIndex(_tree.LocationResolver) + 1,
                _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
            );

            if (node.Operand != null) {
                node.Operand.Walk(this);
            }

            ReplacePreceedingWhiteSpace(
                node.GetEndIndex(_tree.LocationResolver) - 1,
                _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
            );

            return false;
        }

        public override bool Walk(Member node) {
            node.Root.Walk(this);
            ReplaceFollowingWhiteSpace(node.Root.GetEndIndex(_tree.LocationResolver), "");
            ReplaceFollowingWhiteSpace(node.NameSpan.Start + 1, "");
            return false;
        }

        public override bool Walk(CallNode node) {
            node.Function.Walk(this);

            if (node.IsConstructor) {
                ReplaceFollowingWhiteSpace(node.GetStartIndex(_tree.LocationResolver) + "new".Length, " ");
            }

            if (!node.InBrackets) {
                ReplaceFollowingWhiteSpace(
                    node.Function.GetEndIndex(_tree.LocationResolver),
                    ""
                );
            }

            bool isMultiLine = ContainsLineFeed(node.GetStartIndex(_tree.LocationResolver), node.GetEndIndex(_tree.LocationResolver));
            if (node.Arguments != null && node.Arguments.Length > 0) {
                // https://nodejstools.codeplex.com/workitem/1465 node.Arguments[0] is null.
                Debug.Assert(node.Arguments[0] != null);
                if (node.Arguments[0] != null) {
                    if (isMultiLine && ShouldIndentForChild(node, node.Arguments[0])) {
                        Indent();
                    }

                    ReplacePreceedingWhiteSpaceMaybeMultiline(
                        node.Arguments[0].GetStartIndex(_tree.LocationResolver),
                        '(',
                        !_options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis
                    );

                    node.Arguments[0].Walk(this);
                    if (isMultiLine && ShouldIndentForChild(node, node.Arguments[0])) {
                        Dedent();
                    }

                    for (int i = 1; i < node.Arguments.Length; i++) {
                        if (isMultiLine && ShouldIndentForChild(node, node.Arguments[i])) {
                            Indent();
                        }

                        ReplacePreceedingWhiteSpace(
                            node.Arguments[i].GetStartIndex(_tree.LocationResolver),
                            _options.SpaceAfterComma ? " " : string.Empty,
                            _comma);

                        node.Arguments[i].Walk(this);
                        if (isMultiLine && ShouldIndentForChild(node, node.Arguments[i])) {
                            Dedent();
                        }
                    }
                }
            }

            if (!node.InBrackets) {
                ReplacePreceedingWhiteSpaceMaybeMultiline(
                    node.GetEndIndex(_tree.LocationResolver) - 1,
                    empty: !_options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis
                );
            }

            return false;
        }

        public override bool Walk(CommaOperator node) {
            if (node.Expressions != null && node.Expressions.Length != 0) {
                node.Expressions[0].Walk(this);

                for (int i = 1; i < node.Expressions.Length; i++) {
                    ReplacePreceedingWhiteSpace(node.Expressions[i].GetStartIndex(_tree.LocationResolver), _options.SpaceAfterComma ? " " : "", _comma);
                    node.Expressions[i].Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(UnaryOperator node) {
            if (!node.IsPostfix) {
                if (node.OperatorToken == JSToken.Void ||
                    node.OperatorToken == JSToken.TypeOf ||
                    node.OperatorToken == JSToken.Delete) {
                    ReplacePreceedingWhiteSpace(node.Operand.GetStartIndex(_tree.LocationResolver), " ");
                } else {
                    ReplacePreceedingWhiteSpace(node.Operand.GetStartIndex(_tree.LocationResolver), "");
                }
            }
            node.Operand.Walk(this);
            if (node.IsPostfix) {
                ReplaceFollowingWhiteSpace(node.Operand.GetEndIndex(_tree.LocationResolver), "");
            }
            return false;
        }

        public override bool Walk(BinaryOperator node) {
            node.Operand1.Walk(this);

            ReplaceFollowingWhiteSpace(
                node.Operand1.GetEndIndex(_tree.LocationResolver),
                _options.SpaceBeforeAndAfterBinaryOperator ? " " : null
            );

            ReplacePreceedingWhiteSpace(
                node.Operand2.GetStartIndex(_tree.LocationResolver),
                _options.SpaceBeforeAndAfterBinaryOperator ? " " : null,
                null,
                _newlines
            );

            node.Operand2.Walk(this);

            return false;
        }

        #endregion

        #region Formatting Infrastructure

        private bool ContainsLineFeed(int start, int end) {
            Debug.Assert(start <= end, "ContainsLineFeed was given a start ({0}) greater than the end ({1})".FormatInvariant(start, end));
            if (start > end) { // if our span is negative, we can't contain a line feed.
                return false;
            }
            return _code.IndexOfAny(_newlines, start, end - start) != -1;
        }


        /// <summary>
        /// Parses the end of the line getting the range of trailing whitespace and if the line is terminated with
        /// a single line comment.
        /// </summary>
        private void ParseEndOfLine(int startIndex, bool inParens, out bool followedbySingleLineComment, out int startOfTerminatingWhiteSpace, out int whiteSpaceCount) {
            followedbySingleLineComment = false;
            startOfTerminatingWhiteSpace = -1;
            whiteSpaceCount = 0;
            for (int i = startIndex; i < _code.Length; i++) {
                if (_code[i] == ' ' || _code[i] == '\t') {
                    if (!inParens && startOfTerminatingWhiteSpace == -1) {
                        startOfTerminatingWhiteSpace = i;
                        whiteSpaceCount = 0;
                    }
                    whiteSpaceCount++;
                    continue;
                } else if (inParens && _code[i] == ')') {
                    // we were in a parenthesised expression, now we're out
                    // of it and can continue scanning for the single line
                    // comment
                    inParens = false;
                    continue;
                } else if (_code[i] == '\r' || _code[i] == '\n') {
                    if (!inParens && startOfTerminatingWhiteSpace == -1) {
                        startOfTerminatingWhiteSpace = i;
                        whiteSpaceCount = 0;
                    }
                    return;
                } else if (_code[i] == '/') {
                    if (i + 1 < _code.Length) {
                        if (_code[i + 1] == '/') {
                            followedbySingleLineComment = true;
                            startOfTerminatingWhiteSpace = -1;
                            return;
                        } else if (_code[i + 1] == '*') {
                            // need to skip this comment
                            int endComment = _code.IndexOf("*/", i + 2);
                            if (endComment == -1 || ContainsLineFeed(i + 2, endComment)) {
                                startOfTerminatingWhiteSpace = -1;
                                return;
                            }

                            i = endComment + 1;
                            continue;
                        }
                    }
                } else {
                    startOfTerminatingWhiteSpace = -1;
                }
            }
        }

        private bool FollowedBySingleLineComment(int startIndex, bool inParens) {
            bool followedBySingleLineComment;
            int startOfWhiteSpace, whiteSpaceCount;
            ParseEndOfLine(startIndex, inParens, out followedBySingleLineComment, out startOfWhiteSpace, out whiteSpaceCount);
            return followedBySingleLineComment;
        }

        private void IndentSpaces(int spaces) {
            if (_options.SpacesPerIndent == null) {
                Indent();
            } else {
                for (int i = 0; i < spaces / _options.SpacesPerIndent.Value; i++) {
                    Indent();
                }
            }
        }

        private void DedentSpaces(int spaces) {
            if (_options.SpacesPerIndent == null) {
                Dedent();
            } else {
                for (int i = 0; i < spaces / _options.SpacesPerIndent.Value; i++) {
                    Dedent();
                }
            }
        }

        private void FormatVariableDeclaration(VariableDeclaration curDecl) {
            if (curDecl.HasInitializer) {
                ReplaceFollowingWhiteSpace(
                    curDecl.GetNameSpan(_tree.LocationResolver).End,
                    _options.SpaceBeforeAndAfterBinaryOperator ? " " : ""
                );

                ReplacePreceedingWhiteSpace(
                    curDecl.Initializer.GetStartIndex(_tree.LocationResolver),
                    _options.SpaceBeforeAndAfterBinaryOperator ? " " : ""
                );

                curDecl.Initializer.Walk(this);

                ReplaceFollowingWhiteSpace(
                    curDecl.Initializer.GetEndIndex(_tree.LocationResolver),
                    ""
                );
            }
        }

        private string GetIndentation() {
            if (_indentLevel < _whitespace.Count &&
                _whitespace[_indentLevel] != null) {
                return _whitespace[_indentLevel];
            }

            while (_indentLevel >= _whitespace.Count) {
                _whitespace.Add(null);
            }

            if (_options.SpacesPerIndent != null) {
                return _whitespace[_indentLevel] = new string(
                    ' ',
                    _indentLevel * _options.SpacesPerIndent.Value
                );
            }

            return _whitespace[_indentLevel] = new string('\t', _indentLevel);
        }

        private void Indent() {
            _indentLevel++;
        }

        private void Dedent() {
            _indentLevel--;
        }

        enum ReplaceWith {
            None,
            InsertNewLineAndIndentation,
            InsertSpace,
        }

        private string GetBraceNewLineFormatting(ReplaceWith format) {
            switch (format) {
                case ReplaceWith.InsertNewLineAndIndentation:
                    return _options.NewLine + GetIndentation();
                case ReplaceWith.InsertSpace:
                    return " ";
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Reformats a block node.  If the block has braces then the formatting will be updated
        /// appropriately, if forceNewLine
        /// </summary>
        /// <param name="block"></param>
        /// <param name="braceOnNewline"></param>
        private void WalkBlock(Block block) {
            Debug.Assert(block == null || block.Braces != BraceState.None);
            if (block != null && block.Braces != BraceState.None) {
                bool isMultiLine = ContainsLineFeed(block.GetStartIndex(_tree.LocationResolver), block.GetEndIndex(_tree.LocationResolver));
                if (block.Count > 0 && isMultiLine) {
                    // multiline block statement, make sure the 1st statement
                    // starts on a new line
                    EnsureNewLineFollowing(block.GetStartIndex(_tree.LocationResolver) + "{".Length);
                }

                var parent = block.Parent;
                Indent();

                WalkStatements(block, block.Statements, isMultiLine);

                Dedent();

                if (block.Braces == BraceState.StartAndEnd) {
                    if (isMultiLine) {
                        EnsureNewLinePreceeding(block.GetEndIndex(_tree.LocationResolver) - 1);
                    } else {
                        ReplacePreceedingWhiteSpaceMaybeMultiline(block.GetEndIndex(_tree.LocationResolver) - 1);
                    }
                }
            }
        }

        private void WalkStatements(Node node, IEnumerable<Node> stmts, bool isMultiLine) {
            WalkStatements(node.GetStartIndex(_tree.LocationResolver), stmts, isMultiLine);
        }

        private void WalkStatements(int startIndex, IEnumerable<Node> stmts, bool isMultiLine) {
            int prevStart = startIndex;
            int i = 0;
            foreach (var curStmt in stmts) {
                if (i == 0 && isMultiLine && !ContainsLineFeed(startIndex, curStmt.GetStartIndex(_tree.LocationResolver))) {
                    // force a newline before the 1st statement begins
                    ReplacePreceedingWhiteSpace(curStmt.GetStartIndex(_tree.LocationResolver), terminators: null);
                } else {
                    // fix up whitespace for any interleaving blank / comment lines
                    if (!FixStatementIndentation(prevStart, curStmt.GetStartIndex(_tree.LocationResolver))) {
                        if (curStmt is EmptyStatement) {
                            // if (blah); shouldn't get a space...
                            // abort terminators prevents foo;; from getting extra whitespace
                            ReplacePreceedingWhiteSpace(curStmt.GetStartIndex(_tree.LocationResolver), null, abortTerminators: _semicolon);
                        } else {
                            ReplacePreceedingWhiteSpaceMaybeMultiline(curStmt.GetStartIndex(_tree.LocationResolver));
                        }
                    }
                }

                curStmt.Walk(this);

                RemoveTrailingWhiteSpace(curStmt.GetEndIndex(_tree.LocationResolver));

                prevStart = curStmt.GetEndIndex(_tree.LocationResolver);
                i++;
            }
        }

        private bool FixStatementIndentation(int prevStart, int end) {
            bool newlines = false;
            int newLine;
            // It should never be the case that end is before the previous start.  If this happens our spans are not in
            // the correct order.  This likely points to an issue with the parser.  We will catch it here to avoid crashes,
            Debug.Assert(end - prevStart >= 0,
                "Before looping through characters, we have specified the end point before the start point.  " +
                "We can't look at a negative range to find the index of newlines.");
            while (end - prevStart >= 0 && (newLine = _code.IndexOfAny(_newlines, prevStart, end - prevStart)) != -1) {
                bool endsInSingleLineComment;
                int startTerminatingWhiteSpace, whiteSpaceCount;
                ParseEndOfLine(prevStart, false, out endsInSingleLineComment, out startTerminatingWhiteSpace, out whiteSpaceCount);
                if (!endsInSingleLineComment && startTerminatingWhiteSpace == -1) {
                    // don't fix up white space in lines with comments
                    break;
                }
                newlines = true;
                if (_code[newLine] == '\n' ||
                    (_code[newLine] == '\r' &&
                    newLine != _code.Length - 1 &&
                    _code[newLine + 1] != '\n')) {
                    prevStart = newLine + 1;
                } else {
                    prevStart = newLine + 2;
                }
                ReplaceFollowingWhiteSpace(prevStart, GetIndentation());
                Debug.Assert(end - prevStart >= 0,
                    "While looping through characters, we have specified the end point before the start point.  " +
                    "We can't look at a negative range to find the index of newlines.");
            }
            return newlines;
        }

        private void EnsureNewLineFollowing(int start) {
            for (int i = start; i < _code.Length; i++) {
                if (_code[i] == ' ' || _code[i] == '\t') {
                    continue;
                } else if (_code[i] == '\r') {
                    if (i + 1 < _code.Length && _code[i + 1] == '\n') {
                        MaybeReplaceText(
                            start,
                            i + 2,
                            _options.NewLine
                        );
                    } else {
                        MaybeReplaceText(
                            start,
                            i + 1,
                            _options.NewLine
                        );
                    }
                    return;
                } else if (_code[i] == '\n') {
                    MaybeReplaceText(
                        start,
                        i + 1,
                        _options.NewLine
                    );
                    return;
                } else {
                    MaybeReplaceText(
                        start,
                        start,
                        _options.NewLine
                    );
                    return;
                }
            }

            MaybeReplaceText(_code.Length, _code.Length, _options.NewLine);
        }

        private void EnsureNewLinePreceeding(int start) {
            for (int i = start - 1; i >= 0; i--) {
                if (_code[i] == ' ' || _code[i] == '\t') {
                    continue;
                } else if (_code[i] == '\n') {
                    if (i >= 1 && _code[i - 1] == '\r') {
                        MaybeReplaceText(
                            i - 1,
                            start,
                            _options.NewLine + GetIndentation()
                        );
                        break;
                    }
                    MaybeReplaceText(
                        i,
                        start,
                        _options.NewLine + GetIndentation()
                    );
                    break;
                } else if (_code[i] == '\r') {
                    MaybeReplaceText(
                        i,
                        start,
                        _options.NewLine + GetIndentation()
                    );
                    break;
                } else {
                    MaybeReplaceText(
                        i + 1,
                        start,
                        _options.NewLine + GetIndentation()
                    );
                    break;
                }
            }
        }

        private void ReplacePreceedingIncludingNewLines(int start, ReplaceWith braceOnNewline) {
            int codeIndex;
            for (codeIndex = start - 1; codeIndex >= 0; codeIndex--) {
                if (_code[codeIndex] == '\r' || _code[codeIndex] == '\n') {
                    // new lines are always ok to replace...
                    continue;
                } else if (_code[codeIndex] == ' ' || _code[codeIndex] == '\t') {
                    // spaces are ok as long as we're not just trying to fix up newlines...
                    continue;
                } else {
                    // hit a newline, replace the indentation with new indentation
                    MaybeReplaceText(
                        codeIndex + 1,
                        start,
                        GetBraceNewLineFormatting(braceOnNewline)
                    );
                    break;
                }
            }
            if (codeIndex == -1) {
                MaybeReplaceText(
                    0,
                    start,
                    GetBraceNewLineFormatting(braceOnNewline)
                );
            }
        }

        /// <summary>
        /// Gets the brace insertion style for a control flow keyword which
        /// is followed by an expression and then the brace.
        /// </summary>
        /// <returns></returns>
        private ReplaceWith GetFlowControlBraceInsertion(int previousExpressionEnd, bool inParens) {
            // By default we follow the option, but if we have code like:

            // if(x) // comment
            // {

            // Then we need to force/keep the brace on the next line with proper indentation

            if (_options.OpenBracesOnNewLineForControl ||
                FollowedBySingleLineComment(previousExpressionEnd, inParens)) {
                return ReplaceWith.InsertNewLineAndIndentation;
            }

            return ReplaceWith.InsertSpace;
        }

        private void ReplaceFollowingWhiteSpace(int startIndex, string whiteSpace) {
            for (int i = startIndex; i < _code.Length; i++) {
                if (_code[i] != ' ' && _code[i] != '\t') {
                    MaybeReplaceText(startIndex, i, whiteSpace);
                    break;
                }
            }
        }

        private void RemoveTrailingWhiteSpace(int startIndex) {
            for (int i = startIndex; i < _code.Length; i++) {
                if (_code[i] == ' ' || _code[i] == '\t') {
                    continue;
                } else if (_code[i] == '\r' || _code[i] == '\n') {
                    MaybeReplaceText(startIndex, i, "");
                    break;
                } else {
                    break;
                }
            }
        }

        /// <summary>
        /// Replaces the whitespace in from of start with the current indentation level
        /// if it terminates at a newline character.
        /// </summary>
        private void ReplacePreceedingWhiteSpace(int start) {
            ReplacePreceedingWhiteSpace(start, terminators: _newlines);
        }

        /// <summary>
        /// Replaces the preceeding whitespace characters with the current indentation or specified whitespace.
        /// 
        /// If terminators are provided then one of the specified characters must be encountered
        /// to do the replacement.  Otherwise if any other non-whitespace character is 
        /// encountered then the replacement will not occur.
        /// </summary>
        /// <param name="start">the starting position to search backwards from to replace</param>
        /// <param name="newWhiteSpace">The new whitespace or null to use the current indentation</param>
        /// <param name="terminators">null to replace when any non-whitespace character is encountered or 
        /// a list of characters which must be encountered to do the replacement.</param>
        private void ReplacePreceedingWhiteSpace(int start, string newWhiteSpace = null, char[] terminators = null, char[] abortTerminators = null) {
            int codeIndex;
            for (codeIndex = start - 1; codeIndex >= 0; codeIndex--) {
                if (_code[codeIndex] == ' ' || _code[codeIndex] == '\t') {
                    continue;
                } else if (abortTerminators != null && abortTerminators.Contains(_code[codeIndex])) {
                    break;
                } else if (terminators == null || terminators.Contains(_code[codeIndex])) {
                    // hit a terminator replace the indentation with new indentation
                    MaybeReplaceText(codeIndex + 1, start, newWhiteSpace);
                    break;
                } else {
                    break;
                }
            }
            if (codeIndex == -1) {
                MaybeReplaceText(0, start, newWhiteSpace);
            }
        }

        /// <summary>
        /// Replaces the preceeding whitespace updating it to one string if we hit a newline, or another string
        /// if we don't.
        /// </summary>
        private bool ReplacePreceedingWhiteSpaceMaybeMultiline(int start, char replaceOn = '\0', bool empty = false) {
            int codeIndex;
            for (codeIndex = start - 1; codeIndex >= 0; codeIndex--) {
                if (_code[codeIndex] == ' ' || _code[codeIndex] == '\t') {
                    continue;
                } else if (_code[codeIndex] == '\r' || _code[codeIndex] == '\n') {
                    // hit a newline, replace the indentation with new indentation
                    MaybeReplaceText(codeIndex + 1, start, GetIndentation());
                    return true;
                } else if (replaceOn == 0 || _code[codeIndex] == replaceOn) {
                    MaybeReplaceText(codeIndex + 1, start, empty ? "" : " ");
                    break;
                } else {
                    break;
                }
            }
            if (codeIndex == -1) {
                MaybeReplaceText(0, start, GetIndentation());
            }
            return false;
        }

        /// <summary>
        /// Generates an edit to replace the text in the provided range with the
        /// new text if the text has changed.
        /// </summary>
        private void MaybeReplaceText(int start, int end, string newText) {
            string indentation = newText ?? GetIndentation();
            int existingWsLength = end - start;

            if (existingWsLength != indentation.Length ||
                String.Compare(_code, start, indentation, 0, indentation.Length) != 0) {
                Debug.Assert(_edits.Count == 0 || _edits[_edits.Count - 1].Start <= start, "edits should be provided in order");

                _edits.Add(new Edit(start, existingWsLength, indentation));
            }
        }

        private void EnsureSpacesAroundParenthesisedExpression(Expression expr) {
            ReplacePreceedingWhiteSpace(
                expr.GetStartIndex(_tree.LocationResolver),
                _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : "",
                _openParen
            );
            expr.Walk(this);
            ReplaceFollowingWhiteSpace(
                expr.GetEndIndex(_tree.LocationResolver),
                _options.SpaceAfterOpeningAndBeforeClosingNonEmptyParenthesis ? " " : ""
            );
        }

        #endregion
    }
}

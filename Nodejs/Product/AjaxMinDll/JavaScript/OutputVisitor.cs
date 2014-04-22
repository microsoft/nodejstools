// OutputVisitor.cs
//
// Copyright 2011 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.NodejsTools.Parsing
{
#if FALSE
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class OutputVisitor : AstVisitor
    {
        private TextWriter m_outputStream;

        private char m_lastCharacter;
        private bool m_lastCountOdd;
        private bool m_onNewLine;
        private bool m_startOfStatement;
        private bool m_outputCCOn;
        private bool m_doneWithGlobalDirectives;
        private bool m_needsStrictDirective;
        private bool m_noLineBreaks;

        private int m_indentLevel;
        private int m_lineLength;
        private int m_lineCount;

        // needed when generating map files
        private Stack<string> m_functionStack = new Stack<string>();
        private int m_segmentStartLine;
        private int m_segmentStartColumn;

        // if this function is set, before outputting a character will pass it to this
        // function and insert a space if it returns true. Then we reset the function.
        private Func<char, bool> m_addSpaceIfTrue;

        // normally false; gets set to true if we are in a no-in scenario
        // (in-operator not directly allowed)
        private bool m_noIn;

        public CodeSettings Settings { get; set; }

        // this is a regular expression that we'll use to minimize numeric values
        // that don't employ the e-notation
        private static Regex s_decimalFormat = new Regex(
            @"^\s*\+?(?<neg>\-)?0*(?<mag>(?<sig>\d*[1-9])(?<zer>0*))?(\.(?<man>\d*[1-9])?0*)?(?<exp>E\+?(?<eng>\-?)0*(?<pow>[1-9]\d*))?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public OutputVisitor(TextWriter writer)
            : this(writer, null)
        {
        }

        public OutputVisitor(TextWriter writer, CodeSettings settings)
        {
            m_outputStream = writer;
            Settings = settings ?? new CodeSettings();
            m_onNewLine = true;
        }

        public static void Apply(TextWriter writer, Node node, CodeSettings settings)
        {
            if (node != null)
            {
                var outputVisitor = new OutputVisitor(writer, settings);
                node.Accept(outputVisitor);
            }
        }

        #region IVisitor Members

        public override void Visit(ArrayLiteral node)
        {
            var isNoIn = m_noIn;
            m_noIn = false;

            if (node != null)
            {
                var symbol = StartSymbol(node);

                OutputPossibleLineBreak('[');
                MarkSegment(node, null, node.Context);

                m_startOfStatement = false;

                if (node.Elements.Count > 0)
                {
                    Indent();

                    Node element = null;
                    for (var ndx = 0; ndx < node.Elements.Count; ++ndx)
                    {
                        if (ndx > 0)
                        {
                            OutputPossibleLineBreak(',');
                            MarkSegment(node, null, element.IfNotNull(e => e.TerminatingContext));

                            if (Settings.OutputMode == OutputMode.MultipleLines)
                            {
                                OutputPossibleLineBreak(' ');
                            }
                        }

                        element = node.Elements[ndx];
                        if (element != null)
                        {
                            AcceptNodeWithParens(element, element.Precedence == OperatorPrecedence.Comma);
                        }
                    }

                    Unindent();
                }

                Output(']');
                MarkSegment(node, null, node.Context);

                EndSymbol(symbol);
            }

            m_noIn = isNoIn;
        }

        public override void Visit(AstNodeList node)
        {
            if (node != null && node.Count > 0)
            {
                var symbol = StartSymbol(node);

                // see if this parent is a comma operator whose parent in turn is a block.
                // if so, then these expressions were expression statements that we've combined.
                // if that's the case, we're going to put newlines in so it's a little easier
                // to read in multi-line mode
                var addNewLines = node.Parent is CommaOperator
                    && node.Parent.Parent is Block
                    && Settings.OutputMode == OutputMode.MultipleLines;

                // output as comma-separated expressions starting with the first one
                node[0].Accept(this);

                // this should never be the first element of the line, but
                // just in case, reset the flag after the first expression.
                m_startOfStatement = false;

                // if we aren't breaking them up by newlines, indent now in case
                // one of the items causes a newline to be inserted.
                if (!addNewLines)
                {
                    Indent();
                }

                for (var ndx = 1; ndx < node.Count; ++ndx)
                {
                    // output a comma
                    OutputPossibleLineBreak(',');
                    MarkSegment(node, null, node[ndx-1].IfNotNull(n => n.TerminatingContext));

                    if (addNewLines)
                    {
                        NewLine();
                    }
                    else if (Settings.OutputMode == OutputMode.MultipleLines)
                    {
                        OutputPossibleLineBreak(' ');
                    }

                    // output the next node
                    node[ndx].Accept(this);
                }

                // if we aren't breaking by newlines, unindent our previous indent
                if (!addNewLines)
                {
                    Unindent();
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(CommaOperator node)
        {
            if (node != null)
            {
                for (int i = 0; i < node.Expressions.Length; i++)  {
                    var child = node.Expressions[i];
                    if (child == null)
                    {
                        continue;
                    }
                    child.Accept(this);
                    if (i != node.Expressions.Length - 1) { 
                        OutputPossibleLineBreak(',');
                        MarkSegment(node, null, child.TerminatingContext);
                        m_startOfStatement = false;
                        // if the parent is a block, then the comma operator is separating
                        // expression statements -- so break it on the line
                        if (node.Parent is Block)
                        {
                            NewLine();
                        }
                        else if (Settings.OutputMode == OutputMode.MultipleLines)
                        {
                            OutputPossibleLineBreak(' ');
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override void Visit(BinaryOperator node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                var ourPrecedence = node.Precedence;
                var isNoIn = m_noIn;
                if (isNoIn)
                {
                    if (node.OperatorToken == JSToken.In)
                    {
                        // we're in a no-in situation, but our operator is an in-operator.
                        // so we need to wrap this operator in parens
                        OutputPossibleLineBreak('(');
                        m_noIn = false;
                    }
                    else
                    {
                        m_noIn = ourPrecedence <= OperatorPrecedence.Relational;
                    }
                }

                if (node.Operand1 != null)
                {
                    AcceptNodeWithParens(node.Operand1, node.Operand1.Precedence < ourPrecedence);
                }

                m_startOfStatement = false;

                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    // treat the comma-operator special, since we combine expression statements
                    // with it very often
                    if (node.OperatorToken != JSToken.Comma)
                    {
                        // anything other than a comma operator has a space before it, too
                        OutputPossibleLineBreak(' ');
                    }

                    Output(OperatorString(node.OperatorToken));
                    MarkSegment(node, null, node.OperatorContext);

                    BreakLine(false);
                    if (!m_onNewLine)
                    {
                        OutputPossibleLineBreak(' ');
                    }
                }
                else
                {
                    Output(OperatorString(node.OperatorToken));
                    MarkSegment(node, null, node.OperatorContext);
                    BreakLine(false);
                }

                if (node.OperatorToken == JSToken.Divide)
                {
                    // add a function that will check if the next character is also
                    // a forward slash. If it is, the output methods will separate them
                    // with a space so they don't get interpreted as the start of a
                    // single-line comment.
                    m_addSpaceIfTrue = c => c == '/';
                }

                if (node.Operand2 != null)
                {
                    var rightPrecedence = node.Operand2.Precedence;
                    var rightNeedsParens = rightPrecedence < ourPrecedence;

                    var rightHandBinary = node.Operand2 as BinaryOperator;
                    if (rightHandBinary != null)
                    {
                        // they are BOTH binary expressions. This is where it gets complicated.
                        // because most binary tokens (except assignment) are evaluated from left to right,
                        // if we have a binary expression with the same precedence on the RIGHT, then that means the
                        // developer must've put parentheses around it. For some operators, those parentheses 
                        // may not be needed (associative operators like multiply and logical AND or logical OR).
                        // Non-associative operators (divide) will need those parens, so we will want to say they
                        // are a higher relative precedence because of those parentheses.
                        // The plus operator is a special case. It is the same physical token, but it can be two
                        // operations depending on the runtime data: numeric addition or string concatenation.
                        // Because of that ambiguity, let's also calculate the precedence for it as if it were
                        // non-associate as well.
                        // commas never need the parens -- they always evaluate left to right and always return the
                        // right value, so any parens will always be unneccessary.
                        if (ourPrecedence == rightPrecedence
                            && ourPrecedence != OperatorPrecedence.Assignment
                            && ourPrecedence != OperatorPrecedence.Comma)
                        {
                            if (node.OperatorToken == rightHandBinary.OperatorToken)
                            {
                                // the tokens are the same and we're not assignment or comma operators.
                                // so for a few associative operators, we're going to say the relative precedence
                                // is the same so unneeded parens are removed. But for all others, we'll say the
                                // right-hand side is a higher precedence so we maintain the sematic structure
                                // of the expression
                                switch (node.OperatorToken)
                                {
                                    case JSToken.Multiply:
                                    case JSToken.BitwiseAnd:
                                    case JSToken.BitwiseXor:
                                    case JSToken.BitwiseOr:
                                    case JSToken.LogicalAnd:
                                    case JSToken.LogicalOr:
                                        // these are the same regardless
                                        rightNeedsParens = false;
                                        break;

                                    // TODO: the plus operator: if we can prove that it is a numeric operator
                                    // or a string operator on BOTH sides, then it can be associative, too. But
                                    // if one side is a string and the other numeric, or if we can't tell at 
                                    // compile-time, then we need to preserve the structural precedence.
                                    default:
                                        // all other operators are structurally a lower precedence when they
                                        // are on the right, so they need to be evaluated first
                                        rightNeedsParens = true;
                                        break;
                                }
                            }
                            else
                            {
                                // they have the same precedence, but the tokens are different.
                                // and the developer had purposely put parens around the right-hand side
                                // to get them on the right (otherwise with the same precedence they
                                // would've ended up on the left. Keep the parens; must've been done for
                                // a purpose.
                                rightNeedsParens = true;
                            }
                        }
                        else
                        {
                            // different precedence -- just base the decision on the relative precedence values
                            rightNeedsParens = rightPrecedence < ourPrecedence;
                        }
                    }

                    m_noIn = isNoIn && ourPrecedence <= OperatorPrecedence.Relational;
                    AcceptNodeWithParens(node.Operand2, rightNeedsParens);
                }

                if (isNoIn && node.OperatorToken == JSToken.In)
                {
                    // we're in a no-in situation, but our operator is an in-operator.
                    // so we need to wrap this entire operator in parens
                    OutputPossibleLineBreak(')');
                }
                m_noIn = isNoIn;

                EndSymbol(symbol);
            }
        }

        public override void Visit(Block node)
        {
            if (node != null)
            {
                // don't create a symbol for the root node -- it can encompass any of the input files
                var symbol = node.Parent != null ? StartSymbol(node) : null;

                if (node.Parent != null)
                {
                    // not the root block.
                    // if the parent is a function node, we will need a "use strict" directive
                    // if the function's scope is strict but the parent scope is not
                    var parentFunction = node.Parent as FunctionObject;
                    if (parentFunction != null
                        && parentFunction.FunctionScope.UseStrict
                        && !parentFunction.FunctionScope.Parent.UseStrict)
                    {
                        m_needsStrictDirective = true;
                    }

                    // always enclose in curly-braces
                    OutputPossibleLineBreak('{');
                    MarkSegment(node, null, node.Context);
                    Indent();
                }
                else
                {
                    // root block.
                    // we will need a "use strict" directive IF this scope is strict and we
                    // haven't already gone past where we can insert global directive prologues
                    m_needsStrictDirective = node.EnclosingScope.UseStrict && !m_doneWithGlobalDirectives;
                }

                Node prevStatement = null;
                for (var ndx = 0; ndx < node.Count; ++ndx)
                {
                    var statement = node[ndx];
                    if (statement != null && !statement.HideFromOutput)
                    {
                        if (prevStatement != null && prevStatement.RequiresSeparator)
                        {
                            OutputPossibleLineBreak(';');
                            MarkSegment(prevStatement, null, prevStatement.TerminatingContext);
                        }

                        if (!(statement is DirectivePrologue))
                        {
                            if (m_needsStrictDirective)
                            {
                                // we need a strict directive, but we didn't have one.
                                // add it now
                                Output("\"use strict\";");
                                m_needsStrictDirective = false;
                            }

                            m_doneWithGlobalDirectives = true;
                        }

                        NewLine();
                        m_startOfStatement = true;
                        statement.Accept(this);
                        prevStatement = statement;
                    }
                }

                if (node.Parent != null)
                {
                    Unindent();

                    // if there weren't any statements, the curly-braces will be on the same line.
                    // otherwise we want them on a new line
                    if (node.Count > 0)
                    {
                        NewLine();
                    }

                    OutputPossibleLineBreak('}');
                    MarkSegment(node, null, node.Context);
                }
                else if (prevStatement != null && prevStatement.RequiresSeparator && Settings.TermSemicolons)
                {
                    // this is the root block (parent is null) and we want to make sure we end
                    // with a terminating semicolon, so don't replace it
                    OutputPossibleLineBreak(';');
                    MarkSegment(prevStatement, null, prevStatement.TerminatingContext);
                }

                if (symbol != null)
                {
                    EndSymbol(symbol);
                }
            }
        }

        public override void Visit(Break node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("break");
                MarkSegment(node, null, node.Context);

                m_startOfStatement = false;
                if (!string.IsNullOrEmpty(node.Label))
                {
                    // NO PAGE BREAKS ALLOWED HERE
                    m_noLineBreaks = true;
                    if (Settings.LocalRenaming != LocalRenaming.KeepAll
                        && Settings.IsModificationAllowed(TreeModifications.LocalRenaming))
                    {
                        // minify the label -- only depends on nesting level
                        Output(CrunchEnumerator.CrunchedLabel(node.NestLevel) ?? node.Label);
                    }
                    else
                    {
                        // not minified -- just output label
                        Output(node.Label);
                    }

                    MarkSegment(node, null, node.LabelContext);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(CallNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                var isNoIn = m_noIn;
                m_noIn = false;

                if (node.IsConstructor)
                {
                    Output("new");
                    MarkSegment(node, null, node.Context);

                    m_startOfStatement = false;
                }

                if (node.Function != null)
                {
                    var needsParens = node.Function.Precedence < node.Precedence;

                    // if we think we don't need parens, we need to make one more check:
                    // if the function is a new operator with no argument list, then we 
                    // need to wrap it so our argument list doesn't get mistaken for the new-operator's
                    // argument list
                    if (!needsParens)
                    {
                        // because if the new-operator associates to the right and the ()-operator associates
                        // to the left, we need to be careful that we don't change the precedence order when the 
                        // function of a new operator is itself a call or contains a call. In that case, the call will have it's own
                        // parameters (and therefore parentheses) that will need to be associated with the call
                        // and NOT the new -- the call will need to be surrounded with parens to keep that association.
                        // (if we are already going to wrap it in parens, no need to check further)
                        if (node.IsConstructor)
                        {
                            // check the constructor function of our new operator to see if 
                            // it requires parens so we don't get the precedence all screwed up.
                            // pass in whether or not WE have any arguments -- will make a difference when we have embedded
                            // constructors that don't have arguments
                            needsParens = NewParensVisitor.NeedsParens(node.Function, node.Arguments == null || node.Arguments.Count == 0);
                        }
                        else
                        {
                            var newExpression = node.Function as CallNode;
                            if (newExpression != null && newExpression.IsConstructor
                                && (newExpression.Arguments == null || newExpression.Arguments.Count == 0))
                            {
                                needsParens = true;
                            }
                        }
                    }

                    AcceptNodeWithParens(node.Function, needsParens);
                }

                if (!node.IsConstructor || node.Arguments.Count > 0)
                {
                    OutputPossibleLineBreak(node.InBrackets ? '[' : '(');
                    MarkSegment(node, null, node.Arguments.Context);

                    Node argument = null;
                    for (var ndx = 0; ndx < node.Arguments.Count; ++ndx)
                    {
                        if (ndx > 0)
                        {
                            OutputPossibleLineBreak(',');
                            MarkSegment(node.Arguments, null, argument.IfNotNull(a => a.TerminatingContext) ?? node.Arguments.Context);

                            if (Settings.OutputMode == OutputMode.MultipleLines)
                            {
                                OutputPossibleLineBreak(' ');
                            }
                        }

                        argument = node.Arguments[ndx];
                        if (argument != null)
                        {
                            AcceptNodeWithParens(argument, argument.Precedence <= OperatorPrecedence.Comma);
                        }
                    }

                    Output(node.InBrackets ? ']' : ')');
                    MarkSegment(node, null, node.Arguments.Context);
                }

                m_noIn = isNoIn;

                EndSymbol(symbol);
            }
        }

        public override void Visit(ConditionalCompilationComment node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                // if we have already output a cc_on and we don't want to keep any dupes, let's
                // skip over any @cc_on statements at the beginning now
                var ndx = 0;
                if (m_outputCCOn && Settings.IsModificationAllowed(TreeModifications.RemoveUnnecessaryCCOnStatements))
                {
                    while (ndx < node.Statements.Count
                        && (node.Statements[ndx].HideFromOutput || node.Statements[ndx] is ConditionalCompilationOn))
                    {
                        ++ndx;
                    }
                }

                // if there's anything left....
                if (ndx < node.Statements.Count)
                {
                    // start of comment
                    Output("/*");
                    MarkSegment(node, null, node.Context);

                    // get the next statement, which will be the first one we output
                    var statement = node.Statements[ndx];
                    if (statement is ConditionalCompilationStatement
                        || statement is ConstantWrapperPP)
                    {
                        // the next statement STARTS with an @-sign, so just output it. It will add the @ sign to begin
                        // the conditional-compilation comment
                        statement.Accept(this);
                    }
                    else
                    {
                        // next statement does NOT start with an @-sign, so add one now.
                        // outputting an @-sign as the last character will ensure that a
                        // space is inserted before any identifier character coming after.
                        OutputPossibleLineBreak('@');

                        // and then output the first statement
                        statement.Accept(this);
                    }

                    // go through the rest of the statements (if any)
                    Node prevStatement = statement;
                    while (++ndx < node.Statements.Count)
                    {
                        statement = node.Statements[ndx];
                        if (statement != null && !statement.HideFromOutput)
                        {
                            if (prevStatement != null && prevStatement.RequiresSeparator)
                            {
                                OutputPossibleLineBreak(';');
                                MarkSegment(prevStatement, null, prevStatement.TerminatingContext);
                            }

                            NewLine();
                            m_startOfStatement = true;
                            statement.Accept(this);
                            prevStatement = statement;
                        }
                    }

                    // output the closing comment
                    Output("@*/");
                    MarkSegment(node, null, node.Context);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(ConditionalCompilationElse node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                
                Output("@else");
                MarkSegment(node, null, node.Context);

                EndSymbol(symbol);
            }
        }

        public override void Visit(ConditionalCompilationElseIf node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("@elif(");
                MarkSegment(node, null, node.Context);

                m_startOfStatement = false;
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                OutputPossibleLineBreak(')');
                EndSymbol(symbol);
            }
        }

        public override void Visit(ConditionalCompilationEnd node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("@end");
                MarkSegment(node, null, node.Context);
                EndSymbol(symbol);
            }
        }

        public override void Visit(ConditionalCompilationIf node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("@if(");
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                OutputPossibleLineBreak(')');
                EndSymbol(symbol);
            }
        }

        public override void Visit(ConditionalCompilationOn node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                if (!m_outputCCOn
                    || !Settings.IsModificationAllowed(TreeModifications.RemoveUnnecessaryCCOnStatements))
                {
                    m_outputCCOn = true;
                    Output("@cc_on");
                    MarkSegment(node, null, node.Context);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(ConditionalCompilationSet node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("@set");
                MarkSegment(node, null, node.Context);

                m_startOfStatement = false;
                Output(node.VariableName);
                Output('=');

                // if the value is an operator of any kind, we need to wrap it in parentheses
                // so it gets properly parsed
                if (node.Value is BinaryOperator || node.Value is UnaryOperator)
                {
                    Output('(');
                    node.Value.Accept(this);
                    OutputPossibleLineBreak(')');
                }
                else if (node.Value != null)
                {
                    node.Value.Accept(this);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(Conditional node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                var isNoIn = m_noIn;

                if (node.Condition != null)
                {
                    AcceptNodeWithParens(node.Condition, node.Condition.Precedence < OperatorPrecedence.LogicalOr);
                }

                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                    OutputPossibleLineBreak('?');
                    MarkSegment(node, null, node.QuestionContext ?? node.Context);
                    BreakLine(false);
                    if (!m_onNewLine)
                    {
                        OutputPossibleLineBreak(' ');
                    }
                }
                else
                {
                    OutputPossibleLineBreak('?');
                    MarkSegment(node, null, node.QuestionContext ?? node.Context);
                }

                m_startOfStatement = false;
                if (node.TrueExpression != null)
                {
                    m_noIn = isNoIn;
                    AcceptNodeWithParens(node.TrueExpression, node.TrueExpression.Precedence < OperatorPrecedence.Assignment);
                }

                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                    OutputPossibleLineBreak(':');
                    MarkSegment(node, null, node.ColonContext ?? node.Context);
                    BreakLine(false);
                    if (!m_onNewLine)
                    {
                        OutputPossibleLineBreak(' ');
                    }
                }
                else
                {
                    OutputPossibleLineBreak(':');
                    MarkSegment(node, null, node.ColonContext ?? node.Context);
                }

                if (node.FalseExpression != null)
                {
                    m_noIn = isNoIn;
                    AcceptNodeWithParens(node.FalseExpression, node.FalseExpression.Precedence < OperatorPrecedence.Assignment);
                }

                m_noIn = isNoIn;

                EndSymbol(symbol);
            }
        }

        public override void Visit(ConstantWrapper node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                var isNoIn = m_noIn;
                m_noIn = false;

                switch (node.PrimitiveType)
                {
                    case PrimitiveType.Boolean:
                        Output(node.ToBoolean() ? "true" : "false");
                        break;

                    case PrimitiveType.Null:
                        Output("null");
                        break;

                    case PrimitiveType.Number:
                        if (node.Context == null || !node.Context.HasCode
                            || (!node.MayHaveIssues && Settings.IsModificationAllowed(TreeModifications.MinifyNumericLiterals)))
                        {
                            // apply minification to the literal to get it as small as possible
                            Output(NormalizeNumber(node.ToNumber(), node.Context));
                        }
                        else
                        {
                            // context is not null but we don't want to minify numeric literals.
                            // just use the original literal from the context.
                            Output(node.Context.Code);
                        }
                        break;

                    case PrimitiveType.Other:
                        Output(node.Value.ToString());
                        break;

                    case PrimitiveType.String:
                        if (node.Context == null || !node.Context.HasCode)
                        {
                            // escape the string value because we don't have a raw context value
                            // to show anyways
                            Output(InlineSafeString(EscapeString(node.Value.ToString())));
                        }
                        else if (!Settings.IsModificationAllowed(TreeModifications.MinifyStringLiterals))
                        {
                            // we don't want to modify the strings at all!
                            Output(node.Context.Code);
                        }
                        else if (node.MayHaveIssues
                            || (Settings.AllowEmbeddedAspNetBlocks && node.StringContainsAspNetReplacement))
                        {
                            // we'd rather show the raw string, but make sure it's safe for inlining
                            Output(InlineSafeString(node.Context.Code));
                        }
                        else
                        {
                            // we'd rather show the escaped string
                            Output(InlineSafeString(EscapeString(node.Value.ToString())));
                        }

                        break;
                }

                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                m_noIn = isNoIn;

                EndSymbol(symbol);
            }
        }

        public override void Visit(ConstantWrapperPP node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                if (node.ForceComments)
                {
                    Output("/*");
                }

                // varname must include the @ sign
                Output(node.VarName);
                m_startOfStatement = false;

                if (node.ForceComments)
                {
                    Output("@*/");
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(ConstStatement node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("const");
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                Indent();

                for (var ndx = 0; ndx < node.Count; ++ndx)
                {
                    var decl = node[ndx];
                    if (decl != null)
                    {
                        if (ndx > 0)
                        {
                            OutputPossibleLineBreak(',');
                            NewLine();
                        }

                        decl.Accept(this);
                    }
                }
                Unindent();
                EndSymbol(symbol);
            }
        }

        public override void Visit(ContinueNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("continue");
                MarkSegment(node, null, node.Context);

                m_startOfStatement = false;
                if (!string.IsNullOrEmpty(node.Label))
                {
                    // NO PAGE BREAKS ALLOWED HERE
                    m_noLineBreaks = true;
                    if (Settings.LocalRenaming != LocalRenaming.KeepAll
                        && Settings.IsModificationAllowed(TreeModifications.LocalRenaming))
                    {
                        // minify the label -- only depends on nesting level
                        Output(CrunchEnumerator.CrunchedLabel(node.NestLevel) ?? node.Label);
                    }
                    else
                    {
                        // not minified -- just output label
                        Output(node.Label);
                    }

                    MarkSegment(node, null, node.LabelContext);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(CustomNode node)
        {
            if (node != null)
            {
                // custom nodes override the ToCode method to return a blank string.
                // nodes DERIVED from CustomNode should override ToCode is they want
                // to introduce anything into the output stream.
                var code = node.ToCode();
                if (!code.IsNullOrWhiteSpace())
                {
                    var symbol = StartSymbol(node);
                    Output(node.ToCode());
                    MarkSegment(node, null, node.Context);
                    EndSymbol(symbol);
                }
            }
        }

        public override void Visit(DebuggerNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("debugger");
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                EndSymbol(symbol);
            }
        }

        public override void Visit(DirectivePrologue node)
        {
            if (node != null)
            {
                // always output directive prologues that aren't strict; only output
                // the use-strict directive if we need one
                node.IsRedundant = node.UseStrict && !m_needsStrictDirective;
                if (!node.IsRedundant)
                {
                    Visit((ConstantWrapper)node);
                    if (node.UseStrict)
                    {
                        // just output a strict directive -- don't need one anymore
                        m_needsStrictDirective = false;
                    }
                }
            }
        }

        public override void Visit(DoWhile node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("do");
                MarkSegment(node, null, node.Context);

                if (node.Body == null || node.Body.Count == 0)
                {
                    // semicolon-replacement cannot create an empty statement
                    OutputPossibleLineBreak(';');
                }
                else if (node.Body.Count == 1 && !node.Body.EncloseBlock(EncloseBlockType.SingleDoWhile))
                {
                    // there's only one statement, which means we don't need curley braces.
                    // HOWEVER, if the one statement ends in a do-while statement, then we DO need curley-braces
                    // because of an IE bug. IE parses the semi-colon that terminates the do-while as an empty
                    // statement FOLLOWING the do-while, which means the while-clause of the do-while is in the 
                    // wrong spot. We *could* leave the semi-colon out and all browsers will parse it properly, but
                    // that isn't strictly correct JS. So just wrap it in curly-braces to remain proper AND work in 
                    // all browsers.
                    Indent();
                    NewLine();
                    m_startOfStatement = true;
                    node.Body[0].Accept(this);

                    if (node.Body[0].RequiresSeparator)
                    {
                        // because the next thing we are going to output is a while keyword, if the
                        // semicolon would be at the end of a line, we can skip it and just let the
                        // end of line trigger the semicolon-insertion rules.
                        if (ReplaceableSemicolon())
                        {
                            MarkSegment(node.Body[0], null, node.Body[0].TerminatingContext);
                        }
                    }

                    Unindent();
                    NewLine();
                }
                else
                {
                    if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                        || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.Body.BraceOnNewLine))
                    {
                        NewLine();
                    }
                    else if (Settings.OutputMode == OutputMode.MultipleLines)
                    {
                        OutputPossibleLineBreak(' ');
                    }
                    
                    node.Body.Accept(this);

                    if (Settings.OutputMode == OutputMode.MultipleLines)
                    {
                        OutputPossibleLineBreak(' ');
                    }
                }

                Output("while");
                MarkSegment(node, null, node.WhileContext);
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('(');
                m_startOfStatement = false;
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                Output(')');

                EndSymbol(symbol);
            }
        }

        public override void Visit(EmptyStatement node)
        {
            if (node != null)
            {
                // empty statement is just a semicolon
                OutputPossibleLineBreak(';');
                MarkSegment(node, null, node.Context);
            }
        }

        public override void Visit(ForIn node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("for");
                MarkSegment(node, null, node.Context);

                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('(');
                m_startOfStatement = false;
                if (node.Variable != null)
                {
                    // we have a no-in scenario for the variable
                    m_noIn = true;
                    node.Variable.Accept(this);
                    m_noIn = false;
                }

                if (node.OperatorContext != null
                    && !node.OperatorContext.Code.IsNullOrWhiteSpace())
                {
                    Output(node.OperatorContext.Code);
                }
                else
                {
                    // assume this is a for-in operator
                    Output("in");
                }

                MarkSegment(node, null, node.OperatorContext);

                if (node.Collection != null)
                {
                    node.Collection.Accept(this);
                }

                OutputPossibleLineBreak(')');
                MarkSegment(node, null, node.Context);
                OutputBlock(node.Body);

                EndSymbol(symbol);
            }
        }

        public override void Visit(ForNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("for");
                MarkSegment(node, null, node.Context);
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('(');
                m_startOfStatement = false;
                if (node.Initializer != null)
                {
                    // we have a no-in scenario for the initializer
                    m_noIn = true;
                    node.Initializer.Accept(this);
                    m_noIn = false;
                }

                // NEVER do without these semicolons
                OutputPossibleLineBreak(';');
                MarkSegment(node, null, node.Separator1Context ?? node.Context); 
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                OutputPossibleLineBreak(';');
                MarkSegment(node, null, node.Separator2Context ?? node.Context);
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                if (node.Incrementer != null)
                {
                    node.Incrementer.Accept(this);
                }

                OutputPossibleLineBreak(')');
                MarkSegment(node, null, node.Context);

                OutputBlock(node.Body);
                EndSymbol(symbol);
            }
        }

        public override void Visit(FunctionObject node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                var isNoIn = m_noIn;
                m_noIn = false;

                var encloseInParens = node.IsExpression && m_startOfStatement;
                if (encloseInParens)
                {
                    OutputPossibleLineBreak('(');
                }

                // get the function name we will use for symbol references.
                // use the function's real name if:
                //    1. there is one AND
                //      2a. the function is a declaration OR
                //      2b. the refcount is greater than zero OR
                //      2c. we aren't going to remove function expression names
                // otherwise use the name guess.
                var hasName = !node.Name.IsNullOrWhiteSpace()
                        && (!node.IsExpression
                        || node.RefCount > 0
                        || !Settings.RemoveFunctionExpressionNames
                        || !Settings.IsModificationAllowed(TreeModifications.RemoveFunctionExpressionNames));
                var fullFunctionName = hasName
                        ? node.Name
                        : node.NameGuess;

                Output("function");
                MarkSegment(node, fullFunctionName, node.Context);

                m_startOfStatement = false;
                bool isAnonymous = true;
                if (!node.Name.IsNullOrWhiteSpace())
                {
                    isAnonymous = false;
                    var minFunctionName = node.VariableField != null
                        ? node.VariableField.ToString()
                        : node.Name;
                    if (Settings.SymbolsMap != null)
                    {
                        m_functionStack.Push(minFunctionName);
                    }

                    if (hasName)
                    {
                        // all identifier should be treated as if they start with a valid
                        // identifier character. That might not always be the case, like when
                        // we consider an ASP.NET block to output the start of an identifier.
                        // so let's FORCE the insert-space logic here.
                        if (JSScanner.IsValidIdentifierPart(m_lastCharacter))
                        {
                            Output(' ');
                        }

                        Output(minFunctionName);
                        MarkSegment(node, node.Name, node.IdContext);
                    }
                }

                if (Settings.SymbolsMap != null && isAnonymous)
                {
                    BinaryOperator binaryOperator = node.Parent as BinaryOperator;
                    if (binaryOperator != null && binaryOperator.Operand1 is Lookup)
                    {
                        m_functionStack.Push("(anonymous) [{0}]".FormatInvariant(binaryOperator.Operand1));
                    }
                    else
                    {
                        m_functionStack.Push("(anonymous)");
                    }
                }

                OutputFunctionArgsAndBody(node, Settings.RemoveUnneededCode
                    && node.EnclosingScope.IsKnownAtCompileTime
                    && Settings.MinifyCode
                    && Settings.IsModificationAllowed(TreeModifications.RemoveUnusedParameters));

                if (encloseInParens)
                {
                    OutputPossibleLineBreak(')');
                }

                m_noIn = isNoIn;

                EndSymbol(symbol);

                if (Settings.SymbolsMap != null)
                {
                    m_functionStack.Pop();
                }
            }
        }

        public override void Visit(GetterSetter node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output(node.IsGetter ? "get" : "set");
                MarkSegment(node, node.Value.ToString(), node.Context);

                m_startOfStatement = false;
                Output(node.Value.ToString());

                EndSymbol(symbol);
            }
        }

        public virtual void Visit(GroupingOperator node)
        {
            if (node != null)
            {
                // don't output a possible line-break here.
                Output('(');
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;

                if (node.Operand != null)
                {
                    node.Operand.Accept(this);
                }

                OutputPossibleLineBreak(')');
                MarkSegment(node, null, node.Context);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override void Visit(IfNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("if");
                MarkSegment(node, null, node.Context);

                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('(');
                m_startOfStatement = false;
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }

                OutputPossibleLineBreak(')');

                if (node.TrueBlock == null || node.TrueBlock.Count == 0)
                {
                    // no true-block; just output an empty statement
                    OutputPossibleLineBreak(';');
                }
                else if (node.TrueBlock.Count == 1
                    && (node.FalseBlock == null || (!node.TrueBlock.EncloseBlock(EncloseBlockType.IfWithoutElse) && !node.TrueBlock.EncloseBlock(EncloseBlockType.SingleDoWhile)))
                    && (!Settings.MacSafariQuirks || !(node.TrueBlock[0] is FunctionObject)))
                {
                    // we only have a single statement in the true-branch; normally
                    // we wouldn't wrap that statement in braces. However, if there 
                    // is an else-branch, we need to make sure that single statement 
                    // doesn't end with an if-statement that doesn't have an else-branch
                    // because otherwise OUR else-branch will get associated with that
                    // other if-statement. AND it can't end in a do-while statement because then
                    // we run into IE issues with the strict terminating semi-colon.
                    // AND if we are being safari-strict, we want to wrap a single function declaration in
                    // curly-braces, too.
                    Indent();
                    NewLine();

                    m_startOfStatement = true;
                    node.TrueBlock[0].Accept(this);
                    if (node.TrueBlock[0] is ImportantComment)
                    {
                        // the true-block only contained a single important comment.
                        // that's not a true statement, so terminate it with an empty-statement
                        // semicolon
                        OutputPossibleLineBreak(';');
                    }

                    if (node.FalseBlock != null && node.FalseBlock.Count > 0
                        && node.TrueBlock[0].RequiresSeparator)
                    {
                        // we have only one statement, we did not wrap it in braces,
                        // and we have an else-block, and the one true-statement needs
                        // a semicolon; add it now. But because we're going to be outputting
                        // and ELSE keyword next, if we are at the end of a line, we can omit the
                        // semicolon and just output the line-break, because semicolon-insertion
                        // rules will kick in here.
                        if (ReplaceableSemicolon())
                        {
                            MarkSegment(node.TrueBlock[0], null, node.TrueBlock[0].TerminatingContext);
                        }
                    }

                    Unindent();
                }
                else
                {
                    if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                        || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.TrueBlock.BraceOnNewLine))
                    {
                        NewLine();
                    }
                    else if (Settings.OutputMode == OutputMode.MultipleLines)
                    {
                        OutputPossibleLineBreak(' ');
                    }

                    node.TrueBlock.Accept(this);
                }

                if (node.FalseBlock != null && node.FalseBlock.Count > 0)
                {
                    NewLine();
                    Output("else");
                    MarkSegment(node, null, node.ElseContext);
                    if (node.FalseBlock.Count == 1)
                    {
                        var statement = node.FalseBlock[0];
                        if (statement is IfNode)
                        {
                            // this is an else-if construct. Don't newline or indent, just
                            // handle the if-statement directly. 
                            statement.Accept(this);
                        }
                        else
                        {
                            Indent();
                            NewLine();
                            m_startOfStatement = true;
                            statement.Accept(this);
                            Unindent();
                        }
                    }
                    else
                    {
                        if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                            || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.FalseBlock.BraceOnNewLine))
                        {
                            NewLine();
                        }
                        else if (Settings.OutputMode == OutputMode.MultipleLines)
                        {
                            OutputPossibleLineBreak(' ');
                        }

                        node.FalseBlock.Accept(this);
                    }
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(ImportantComment node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                // make sure we force the important comments to start on a new line, regardless
                // of whether or not we are in multi- or single-line mode, and the statement after
                // should also be on a new line.
                BreakLine(true);

                // the output method assumes any text we send it's way doesn't contain any line feed
                // characters. The important comment, however, may contain some. We don't want to count
                // the entire comment as a single line, AND we want to normalize the line-feed characters,
                // so lets process the comment line-by-line
                var lineFeedChars = new[] { '\n', '\r', '\u2028', '\u2029' };
                var startIndex = 0;
                var firstLF = node.Comment.IndexOfAny(lineFeedChars, startIndex);
                if (firstLF < 0)
                {
                    // no line-breaks at all!
                    Output(node.Comment);
                }
                else
                {
                    // output the first segment -- from start to first line break
                    Output(node.Comment.Substring(0, firstLF));
                    while (true)
                    {
                        // advance the next segment pointer
                        if (node.Comment[firstLF] == '\r'
                            && firstLF < node.Comment.Length - 1
                            && node.Comment[firstLF + 1] == '\n')
                        {
                            startIndex = firstLF + 2;
                        }
                        else
                        {
                            startIndex = firstLF + 1;
                        }

                        // force the line-break in the output
                        BreakLine(true);

                        // look for the next line break
                        firstLF = node.Comment.IndexOfAny(lineFeedChars, startIndex);

                        if (firstLF > startIndex)
                        {
                            // only output something if there was something before the next line break
                            Output(node.Comment.Substring(startIndex, firstLF - startIndex));
                        }
                        else if (firstLF < 0)
                        {
                            // no more line-breaks -- output the last segment and break out of the loop
                            Output(node.Comment.Substring(startIndex));
                            break;
                        }
                    }
                }

                // force a line-break AFTER teh important comment as well
                BreakLine(true);
                EndSymbol(symbol);
            }
        }

        public override void Visit(LabeledStatement node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                if (Settings.LocalRenaming != LocalRenaming.KeepAll
                    && Settings.IsModificationAllowed(TreeModifications.LocalRenaming))
                {
                    // we're minifying the labels.
                    // we want to output our label as per our nested level.
                    // top-level is "a", next level is "b", etc.
                    // we don't need to worry about collisions with variables.
                    Output(CrunchEnumerator.CrunchedLabel(node.NestCount) ?? node.Label);
                }
                else
                {
                    // not minifying -- just output our label
                    Output(node.Label);
                }

                MarkSegment(node, null, node.Context);
                OutputPossibleLineBreak(':');
                MarkSegment(node, null, node.ColonContext);
                if (node.Statement != null && !node.Statement.HideFromOutput)
                {
                    m_startOfStatement = true;
                    node.Statement.Accept(this);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(LexicalDeclaration node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                // save the no-in state -- we'll reset before processing each initializer
                var isNoIn = m_noIn;

                Output(OperatorString(node.StatementToken));
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                Indent();
                var useNewLines = !(node.Parent is ForNode);

                for (var ndx = 0; ndx < node.Count; ++ndx)
                {
                    var decl = node[ndx];
                    if (decl != null)
                    {
                        if (ndx > 0)
                        {
                            OutputPossibleLineBreak(',');
                            if (useNewLines)
                            {
                                NewLine();
                            }
                            else if (Settings.OutputMode == OutputMode.MultipleLines)
                            {
                                OutputPossibleLineBreak(' ');
                            }
                        }

                        // be sure to set the no-in state to whatever it was when we entered
                        // this node, because each declaration might reset it as it's outputting
                        // its child nodes
                        m_noIn = isNoIn;
                        decl.Accept(this);
                    }
                }

                Unindent();
                EndSymbol(symbol);
            }
        }

        public override void Visit(Lookup node)
        {
            if (node != null)
            {
                // all identifier should be treated as if they start with a valid
                // identifier character. That might not always be the case, like when
                // we consider an ASP.NET block to output the start of an identifier.
                // so let's FORCE the insert-space logic here.
                if (JSScanner.IsValidIdentifierPart(m_lastCharacter))
                {
                    OutputSpaceOrLineBreak();
                }

                var symbol = StartSymbol(node);

                Output(node.VariableField != null
                    ? node.VariableField.ToString()
                    : node.Name);
                MarkSegment(node, node.Name, node.Context);
                m_startOfStatement = false;

                EndSymbol(symbol);
            }
        }

        public override void Visit(Member node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                var isNoIn = m_noIn;
                m_noIn = false;

                if (node.Root != null)
                {
                    var constantWrapper = node.Root as ConstantWrapper;
                    if (constantWrapper != null && constantWrapper.IsFiniteNumericLiteral)
                    {
                        // numeric constant wrapper that isn't NaN or Infinity - get the formatted text version
                        string numericText;
                        if (constantWrapper.Context == null
                            || !constantWrapper.Context.HasCode
                            || Settings.IsModificationAllowed(TreeModifications.MinifyNumericLiterals))
                        {
                            // apply minification to the literal to get it as small as possible
                            numericText = NormalizeNumber(constantWrapper.ToNumber(), constantWrapper.Context);
                        }
                        else
                        {
                            // context is not null but we don't want to minify numeric literals.
                            // just use the original literal from the context.
                            numericText = constantWrapper.Context.Code;
                        }

                        // if there is no decimal point in the number, we need to add one at the end
                        // so the member-dot operator doesn't get mistaken for the decimal point and
                        // generate a syntax error
                        Output(numericText);
                        if (numericText.IndexOf('.') < 0
                            && !numericText.StartsWith("(", StringComparison.Ordinal))
                        {
                            // no decimal point - make sure to add one
                            Output('.');
                        }
                    }
                    else
                    {
                        // not a numeric constant wrapper
                        var needsParens = node.Root.Precedence < node.Precedence;
                        if (!needsParens)
                        {
                            // if the root is a new operator with no arguments, then we need to wrap
                            var callNode = node.Root as CallNode;
                            if (callNode != null
                                && callNode.IsConstructor
                                && (callNode.Arguments == null || callNode.Arguments.Count == 0))
                            {
                                needsParens = true;
                            }
                        }

                        AcceptNodeWithParens(node.Root, needsParens);
                    }
                }

                OutputPossibleLineBreak('.');
                MarkSegment(node, node.Name, node.NameContext);
                Output(node.Name);
                m_startOfStatement = false;
                m_noIn = isNoIn;

                EndSymbol(symbol);
            }
        }

        public override void Visit(ObjectLiteral node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                var isNoIn = m_noIn;
                m_noIn = false;

                // if start of statement, need to enclose in parens
                var encloseInParens = m_startOfStatement;
                if (encloseInParens)
                {
                    OutputPossibleLineBreak('(');
                }

                OutputPossibleLineBreak('{');
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                Indent();

                var count = node.Properties.IfNotNull(p => p.Count);
                if (count > 1)
                {
                    NewLine();
                }

                // output each key/value pair
                if (node.Properties != null)
                {
                    node.Properties.Accept(this);
                }

                Unindent();
                if (count > 1)
                {
                    NewLine();
                }

                Output('}');
                MarkSegment(node, null, node.Context);
                if (encloseInParens)
                {
                    Output(')');
                }

                m_noIn = isNoIn;

                EndSymbol(symbol);
            }
        }

        public override void Visit(ObjectLiteralField node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                if (Settings.QuoteObjectLiteralProperties)
                {
                    // we always want to quote object literal property names, no matter whether
                    // they are valid JS identifiers, numbers, or whatever. Typically this is done
                    // because we're generating JSON output, which requires quotes.
                    if (node.PrimitiveType == PrimitiveType.String)
                    {
                        // strings are always quoted anyway
                        Visit(node as ConstantWrapper);
                    }
                    else
                    {
                        // output quotes around it
                        Output('"');
                        Visit(node as ConstantWrapper);
                        Output('"');
                    }
                }
                else if (node.PrimitiveType == PrimitiveType.String)
                {
                    // call the base to format the value
                    // determine whether we need quotes or not
                    var propertyName = node.ToString();
                    if (!string.IsNullOrEmpty(propertyName)
                        && JSScanner.IsSafeIdentifier(propertyName)
                        && !JSScanner.IsKeyword(propertyName, node.EnclosingScope.UseStrict))
                    {
                        Output(propertyName);
                        MarkSegment(node, null, node.Context);
                    }
                    else
                    {
                        // base implementation adds quotes
                        Visit(node as ConstantWrapper);
                    }
                }
                else
                {
                    // not a string -- just output it
                    Visit(node as ConstantWrapper);
                }

                OutputPossibleLineBreak(':');
                MarkSegment(node, null, node.ColonContext);

                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(ObjectLiteralProperty node)
        {
            if (node != null)
            {
                if (node.Name != null)
                {
                    node.Name.Accept(this);
                }

                if (node.Name is GetterSetter)
                {
                    // always output the parameters
                    OutputFunctionArgsAndBody((node.Value as FunctionExpression).Function, false);
                }
                else if (node.Value != null)
                {
                    AcceptNodeWithParens(node.Value, node.Value.Precedence == OperatorPrecedence.Comma);
                }
            }
        }

        public override void Visit(ParameterDeclaration node)
        {
            if (node != null)
            {
                // just output the node's name
                Output(node.VariableField == null ? node.Name : node.VariableField.ToString());
                MarkSegment(node, node.Name, node.Context);
            }
        }

        public override void Visit(RegExpLiteral node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                m_startOfStatement = false;

                // cannot have a line break anywhere in this node
                Output('/');
                MarkSegment(node, null, node.Context);
                Output(node.Pattern);
                Output('/');
                if (!string.IsNullOrEmpty(node.PatternSwitches))
                {
                    Output(node.PatternSwitches);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(ReturnNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("return");
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                if (node.Operand != null)
                {
                    if (Settings.OutputMode == OutputMode.MultipleLines)
                    {
                        Output(' ');
                    }

                    // no page breaks allowed here
                    m_noLineBreaks = true;
                    Indent();
                    node.Operand.Accept(this);
                    Unindent();
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(Switch node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("switch");
                MarkSegment(node, null, node.Context);
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('(');
                m_startOfStatement = false;
                if (node.Expression != null)
                {
                    node.Expression.Accept(this);
                }
                OutputPossibleLineBreak(')');
                if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                    || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.BraceOnNewLine))
                {
                    NewLine();
                }
                else if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('{');
                MarkSegment(node, null, node.BraceContext); 
                Indent();

                Node prevSwitchCase = null;
                for (var ndx = 0; ndx < node.Cases.Count; ++ndx)
                {
                    var switchCase = node.Cases[ndx];
                    if (switchCase != null)
                    {
                        if (prevSwitchCase != null && prevSwitchCase.RequiresSeparator)
                        {
                            // because the next switch-case will always start with either the case or default
                            // keyword, if the semicolon we are about the output would be at the end of a newline,
                            // we can omit the semicolon altogether and just let the semicolon-insertion rules
                            // kick in.
                            if (ReplaceableSemicolon())
                            {
                                MarkSegment(prevSwitchCase, null, prevSwitchCase.TerminatingContext);
                            }
                        }

                        NewLine();
                        switchCase.Accept(this);
                        prevSwitchCase = switchCase;
                    }
                }

                Unindent();
                NewLine();
                OutputPossibleLineBreak('}');
                MarkSegment(node, null, node.BraceContext);

                EndSymbol(symbol);
            }
        }

        public override void Visit(SwitchCase node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                if (node.CaseValue != null)
                {
                    Output("case");
                    MarkSegment(node, null, node.Context);

                    m_startOfStatement = false;
                    node.CaseValue.Accept(this);
                }
                else
                {
                    Output("default");
                    MarkSegment(node, null, node.Context);
                }

                OutputPossibleLineBreak(':');
                MarkSegment(node, null, node.ColonContext);
                if (node.Statements != null && node.Statements.Count > 0)
                {
                    Indent();
                    Node prevStatement = null;
                    for (var ndx = 0; ndx < node.Statements.Count; ++ndx)
                    {
                        var statement = node.Statements[ndx];
                        if (statement != null && !statement.HideFromOutput)
                        {
                            if (prevStatement != null && prevStatement.RequiresSeparator)
                            {
                                OutputPossibleLineBreak(';');
                                MarkSegment(prevStatement, null, prevStatement.TerminatingContext);
                            }

                            NewLine();
                            m_startOfStatement = true;
                            statement.Accept(this);
                            prevStatement = statement;
                        }
                    }

                    Unindent();
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(ThisLiteral node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("this");
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                EndSymbol(symbol);
            }
        }

        public override void Visit(ThrowNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("throw");
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                if (node.Operand != null)
                {
                    m_noLineBreaks = true;
                    node.Operand.Accept(this);
                }

                if (Settings.MacSafariQuirks)
                {
                    // force the statement ending with a semicolon
                    OutputPossibleLineBreak(';');
                    MarkSegment(node, null, node.TerminatingContext);
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(TryNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                OutputTryBranch(node);

                var hasCatchBlock = false;
                if (!string.IsNullOrEmpty(node.CatchVarName))
                {
                    hasCatchBlock = true;
                    OutputCatchBranch(node);
                }

                if (!hasCatchBlock || (node.FinallyBlock != null && node.FinallyBlock.Count > 0))
                {
                    OutputFinallyBranch(node);
                }

                EndSymbol(symbol);
            }
        }

        private void OutputTryBranch(TryNode node)
        {
            Output("try");
            MarkSegment(node, null, node.Context);
            if (node.TryBlock == null || node.TryBlock.Count == 0)
            {
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                Output("{}");
                BreakLine(false);
            }
            else
            {
                if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                    || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.TryBlock.BraceOnNewLine))
                {
                    NewLine();
                }
                else if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                node.TryBlock.Accept(this);
            }
        }

        private void OutputCatchBranch(TryNode node)
        {
            NewLine();
            Output("catch(");
            MarkSegment(node, null, node.CatchVarContext);
            if (node.CatchParameter != null)
            {
                node.CatchParameter.Accept(this);
            }

            OutputPossibleLineBreak(')');

            if (node.CatchBlock == null || node.CatchBlock.Count == 0)
            {
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                Output("{}");
                BreakLine(false);
            }
            else
            {
                if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                    || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.CatchBlock.BraceOnNewLine))
                {
                    NewLine();
                }
                else if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                node.CatchBlock.Accept(this);
            }
        }

        private void OutputFinallyBranch(TryNode node)
        {
            NewLine();
            Output("finally");
            MarkSegment(node, null, node.FinallyContext);
            if (node.FinallyBlock == null || node.FinallyBlock.Count == 0)
            {
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                Output("{}");
                BreakLine(false);
            }
            else
            {
                if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                    || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.FinallyBlock.BraceOnNewLine))
                {
                    NewLine();
                }
                else if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                node.FinallyBlock.Accept(this);
            }
        }

        public override void Visit(Var node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                // save the no-in state -- we'll reset before processing each initializer
                var isNoIn = m_noIn;

                Output("var");
                MarkSegment(node, null, node.Context);
                m_startOfStatement = false;
                Indent();
                var useNewLines = !(node.Parent is ForNode);

                for (var ndx = 0; ndx < node.Count; ++ndx)
                {
                    var decl = node[ndx];
                    if (decl != null)
                    {
                        if (ndx > 0)
                        {
                            OutputPossibleLineBreak(',');
                            if (useNewLines)
                            {
                                NewLine();
                            }
                            else if (Settings.OutputMode == OutputMode.MultipleLines)
                            {
                                OutputPossibleLineBreak(' ');
                            }
                        }

                        // be sure to set the no-in state to whatever it was when we entered
                        // this node, because each declaration might reset it as it's outputting
                        // its child nodes
                        m_noIn = isNoIn;
                        decl.Accept(this);
                    }
                }
                Unindent();

                EndSymbol(symbol);
            }
        }

        public override void Visit(VariableDeclaration node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                // output the name (use the field is possible)
                Output(node.VariableField != null ? node.VariableField.ToString() : node.Identifier);
                MarkSegment(node, node.Name, node.Context);

                m_startOfStatement = false;
                if (node.Initializer != null)
                {
                    if (node.IsCCSpecialCase)
                    {
                        // we haven't output a cc_on yet -- output it now.
                        // if we have, we really only need to output one if we had one to begin with AND
                        // we are NOT removing unnecessary ones
                        if (!m_outputCCOn
                            || (node.UseCCOn && !Settings.IsModificationAllowed(TreeModifications.RemoveUnnecessaryCCOnStatements)))
                        {
                            Output("/*@cc_on=");
                            m_outputCCOn = true;
                        }
                        else
                        {
                            Output("/*@=");
                        }
                    }
                    else
                    {

                        if (Settings.OutputMode == OutputMode.MultipleLines && Settings.IndentSize > 0)
                        {
                            OutputPossibleLineBreak(' ');
                            OutputPossibleLineBreak('=');
                            BreakLine(false);
                            if (!m_onNewLine)
                            {
                                OutputPossibleLineBreak(' ');
                            }
                        }
                        else
                        {
                            OutputPossibleLineBreak('=');
                        }
                    }

                    AcceptNodeWithParens(node.Initializer, node.Initializer.Precedence == OperatorPrecedence.Comma);

                    if (node.IsCCSpecialCase)
                    {
                        Output("@*/");
                    }
                }

                EndSymbol(symbol);
            }
        }

        public override void Visit(UnaryOperator node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                var isNoIn = m_noIn;
                m_noIn = false;

                if (node.IsPostfix)
                {
                    if (node.Operand != null)
                    {
                        AcceptNodeWithParens(node.Operand, node.Operand.Precedence < node.Precedence);
                    }

                    // the only postfix unary operators are ++ and --, and when in the postfix position,
                    // line breaks are NOT allowed between the operand and the operator.
                    // doesn't seem to need this flag set here, but set it anyways just in case.
                    m_noLineBreaks = true;
                    Output(OperatorString(node.OperatorToken));
                    MarkSegment(node, null, node.OperatorContext);
                    m_startOfStatement = false;
                }
                else
                {
                    if (node.OperatorInConditionalCompilationComment)
                    {
                        // if we haven't output a cc_on yet, we ALWAYS want to do it now, whether or not the 
                        // sources had one. Otherwise, we only only want to output one if we had one and we aren't
                        // removing unneccesary ones.
                        if (!m_outputCCOn
                            || (node.ConditionalCommentContainsOn && !Settings.IsModificationAllowed(TreeModifications.RemoveUnnecessaryCCOnStatements)))
                        {
                            // output it now and set the flag that we have output them
                            Output("/*@cc_on");
                            m_outputCCOn = true;
                        }
                        else
                        {
                            Output("/*@");
                        }

                        Output(OperatorString(node.OperatorToken));
                        MarkSegment(node, null, node.OperatorContext);
                        Output("@*/");
                    }
                    else
                    {
                        Output(OperatorString(node.OperatorToken));
                        MarkSegment(node, null, node.OperatorContext ?? node.Context);
                    }

                    m_startOfStatement = false;
                    if (node.Operand != null)
                    {
                        AcceptNodeWithParens(node.Operand, node.Operand.Precedence < node.Precedence);
                    }
                }

                m_noIn = isNoIn;
                EndSymbol(symbol);
            }
        }

        public override void Visit(WhileNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);

                Output("while");
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('(');
                m_startOfStatement = false;
                if (node.Condition != null)
                {
                    node.Condition.Accept(this);
                }
                OutputPossibleLineBreak(')');

                OutputBlock(node.Body);

                EndSymbol(symbol);
            }
        }

        public override void Visit(WithNode node)
        {
            if (node != null)
            {
                var symbol = StartSymbol(node);
                Output("with");
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                OutputPossibleLineBreak('(');
                m_startOfStatement = false;
                if (node.WithObject != null)
                {
                    node.WithObject.Accept(this);
                }
                OutputPossibleLineBreak(')');

                OutputBlock(node.Body);
                EndSymbol(symbol);
            }
        }

        #endregion

        #region output methods

        private void Output([Localizable(false)] string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // insert a space if needed, then the character
                InsertSpaceIfNeeded(text);

                // save the start of this segment
                m_segmentStartLine = m_lineCount;
                m_segmentStartColumn = m_lineLength;

                m_outputStream.Write(text);
                m_lineLength += text.Length;
                m_noLineBreaks = false;

                // if it ends in a newline, we're still on a newline
                m_onNewLine = (text[text.Length - 1] == '\n' || text[text.Length - 1] == '\r'); ;

                // now set the "last character" state
                SetLastCharState(text);
            }
        }

        private void Output(char ch)
        {
            // insert a space if needed, then the character
            InsertSpaceIfNeeded(ch);

            // save the start of this segment
            m_segmentStartLine = m_lineCount;
            m_segmentStartColumn = m_lineLength;

            m_outputStream.Write(ch);
            ++m_lineLength;
            m_noLineBreaks = false;

            // determine if this was a newline character
            m_onNewLine = (ch == '\n' || ch == '\r');

            // now set the "last character" state
            SetLastCharState(ch);
        }

        private void OutputSpaceOrLineBreak()
        {
            if (m_noLineBreaks)
            {
                m_outputStream.Write(' ');
                m_lastCharacter = ' ';
                ++m_lineLength;
            }
            else
            {
                OutputPossibleLineBreak(' ');
            }
        }

        private void InsertSpaceIfNeeded(char ch)
        {
            // shortcut a space character -- we never need a space before a space!
            if (ch != ' ')
            {
                if (m_addSpaceIfTrue != null)
                {
                    if (m_addSpaceIfTrue(ch))
                    {
                        OutputSpaceOrLineBreak();
                    }

                    // reset the function
                    m_addSpaceIfTrue = null;
                }
                else if ((ch == '+' || ch == '-') && m_lastCharacter == ch)
                {
                    // if the current character is a + or - and the last character was the same.
                    // if the previous character was an ODD number of the same character, 
                    // then we need to add a space so it doesn't get read as ++ (or --)
                    if (m_lastCountOdd)
                    {
                        OutputSpaceOrLineBreak();
                    }
                }
                else if ((m_lastCharacter == '@' || JSScanner.IsValidIdentifierPart(m_lastCharacter)) && JSScanner.IsValidIdentifierPart(ch))
                {
                    // either the last character is a valid part of an identifier and the current character is, too;
                    // OR the last part was numeric and the current character is a .
                    // we need to separate those with spaces as well
                    OutputSpaceOrLineBreak();
                }
            }
        }

        private void InsertSpaceIfNeeded(string text)
        {
            // if the current character is a + or - and the last character was the same....
            var ch = text[0];
            if (m_addSpaceIfTrue != null)
            {
                if (m_addSpaceIfTrue(ch))
                {
                    OutputSpaceOrLineBreak();
                }

                // reset the function
                m_addSpaceIfTrue = null;
            }
            else if ((ch == '+' || ch == '-') && m_lastCharacter == ch)
            {
                // if we want to put a + or a - in the stream, and the previous character was
                // an odd number of the same, then we need to add a space so it doesn't
                // get read as ++ (or --)
                if (m_lastCountOdd)
                {
                    OutputSpaceOrLineBreak();
                }
            }
            else if ((m_lastCharacter == '@' || JSScanner.IsValidIdentifierPart(m_lastCharacter)) && JSScanner.IsValidIdentifierPart(text))
            {
                // either the last character is a valid part of an identifier and the current character is, too;
                // OR the last part was numeric and the current character is a .
                // we need to separate those with spaces as well
                OutputSpaceOrLineBreak();
            }
        }

        private void SetLastCharState(char ch)
        {
            // if it's a + or a -, we need to adjust the odd state
            if (ch == '+' || ch == '-')
            {
                if (ch == m_lastCharacter)
                {
                    // same as the last string -- so we're adding one to it.
                    // if it was odd before, it's now even; if it was even before,
                    // it's now odd
                    m_lastCountOdd = !m_lastCountOdd;
                }
                else
                {
                    // not the same as last time, so this is a string of 1
                    // characters, which is odd
                    m_lastCountOdd = true;
                }
            }
            else
            {
                // neither + nor -; reset the odd state
                m_lastCountOdd = false;
            }

            m_lastCharacter = ch;
        }

        private void SetLastCharState(string text)
        {
            // ignore empty strings
            if (!string.IsNullOrEmpty(text))
            {
                // get the last character
                char lastChar = text[text.Length - 1];

                // if it's not a plus or a minus, we don't care
                if (lastChar == '+' || lastChar == '-')
                {
                    // see HOW MANY of those characters were at the end of the string
                    var ndxDifferent = text.Length - 1;
                    while (--ndxDifferent >= 0)
                    {
                        if (text[ndxDifferent] != lastChar)
                        {
                            break;
                        }
                    }

                    // if the first diff index is less than zero, then the whole string is one of
                    // these two special characters
                    if (ndxDifferent < 0 && m_lastCharacter == lastChar)
                    {
                        // the whole string is the same character, AND it's the same character 
                        // at the end of the last time we output stuff. We need to take into 
                        // account the previous state when we set the current state.
                        // it's a logical XOR -- if the two values are the same, m_lastCountOdd is false;
                        // it they are different, m_lastCountOdd is true.
                        m_lastCountOdd = (text.Length % 2 == 1) ^ m_lastCountOdd;
                    }
                    else
                    {
                        // either the whole string wasn't the same character, OR the previous ending
                        // wasn't the same character. Either way, the current state is determined 
                        // exclusively by the number of characters we found at the end of this string
                        // get the number of same characters ending this string, mod by 2, and if the
                        // result is 1, it's an odd number of characters.
                        m_lastCountOdd = (text.Length - 1 - ndxDifferent) % 2 == 1;
                    }
                }
                else
                {
                    // say we weren't odd
                    m_lastCountOdd = false;
                }

                // save the last character for next time
                m_lastCharacter = lastChar;
            }
        }

        private void Indent()
        {
            ++m_indentLevel;
        }

        private void Unindent()
        {
            --m_indentLevel;
        }

        private void OutputPossibleLineBreak(char ch)
        {
            if (ch == ' ')
            {
                // break the line if it's already too long, but don't force it
                BreakLine(false);

                // if we aren't on a new line, then output our space character
                if (!m_onNewLine)
                {
                    m_outputStream.Write(ch);
                    m_lastCharacter = ch;
                    ++m_lineLength;
                }
            }
            else
            {
                // always output the character, although we can line-break
                // after it if needed
                InsertSpaceIfNeeded(ch);

                // save the start of this segment
                m_segmentStartLine = m_lineCount;
                m_segmentStartColumn = m_lineLength;
                
                m_outputStream.Write(ch);
                m_onNewLine = false;
                m_lastCharacter = ch;
                ++m_lineLength;

                // break the line if it's too long, but don't force it
                BreakLine(false);
            }
        }

        private bool ReplaceableSemicolon()
        {
            var outputSemicolon = false;

            // this is a terminating semicolon that might be replaced with a line-break
            // if needed. Semicolon-insertion would suffice to reconstitute it.
            if (m_lineLength < Settings.LineBreakThreshold)
            {
                // save the start of this segment
                m_segmentStartLine = m_lineCount;
                m_segmentStartColumn = m_lineLength;

                // output the semicolon
                m_outputStream.Write(';');
                m_onNewLine = false;
                m_lastCharacter = ';';
                ++m_lineLength;
                outputSemicolon = true;
            }

            // break the line if it's too long, but don't force it
            BreakLine(false);
            return outputSemicolon;
        }

        private void BreakLine(bool forceBreak)
        {
            if (!m_onNewLine && (forceBreak || m_lineLength >= Settings.LineBreakThreshold))
            {
                if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    NewLine();
                }
                else
                {
                    // terminate the line and start a new one
                    m_outputStream.Write('\n');
                    m_lineCount++;

                    // set the appropriate newline state
                    m_lineLength = 0;
                    m_onNewLine = true;
                    m_lastCharacter = ' ';
                }
            }
        }

        private void NewLine()
        {
            if (Settings.OutputMode == OutputMode.MultipleLines && !m_onNewLine)
            {
                // output the newline character
                m_outputStream.WriteLine();
                m_lineCount++;

                // if the indent level is greater than zero, output the indent spaces
                if (m_indentLevel > 0)
                {
                    var numSpaces = m_indentLevel * Settings.IndentSize;
                    m_lineLength = numSpaces;
                    while (numSpaces-- > 0)
                    {
                        m_outputStream.Write(' ');
                    }
                }
                else
                {
                    m_lineLength = 0;
                }

                // say our last character was a space
                m_lastCharacter = ' ';

                // we just output a newline
                m_onNewLine = true;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="Big but simple case statement")]
        public static string OperatorString(JSToken token)
        {
            switch (token)
            {
                case JSToken.Decrement: return "--";
                case JSToken.Delete: return "delete";
                case JSToken.Increment: return "++";
                case JSToken.TypeOf: return "typeof";
                case JSToken.Void: return "void";
                case JSToken.LogicalNot: return "!";
                case JSToken.BitwiseNot: return "~";
                case JSToken.Minus: return "-";
                case JSToken.Plus: return "+";
                case JSToken.Multiply: return "*";
                case JSToken.BitwiseAnd: return "&";
                case JSToken.BitwiseOr: return "|";
                case JSToken.BitwiseXor: return "^";
                case JSToken.LogicalAnd: return "&&";
                case JSToken.LogicalOr: return "||";
                case JSToken.Assign: return "=";
                case JSToken.BitwiseAndAssign: return "&=";
                case JSToken.BitwiseOrAssign: return "|=";
                case JSToken.BitwiseXorAssign: return "^=";
                case JSToken.Comma: return ",";
                case JSToken.Equal: return "==";
                case JSToken.GreaterThan: return ">";
                case JSToken.GreaterThanEqual: return ">=";
                case JSToken.In: return "in";
                case JSToken.InstanceOf: return "instanceof";
                case JSToken.LeftShift: return "<<";
                case JSToken.LeftShiftAssign: return "<<=";
                case JSToken.LessThan: return "<";
                case JSToken.LessThanEqual: return "<=";
                case JSToken.MinusAssign: return "-=";
                case JSToken.Modulo: return "%";
                case JSToken.ModuloAssign: return "%=";
                case JSToken.MultiplyAssign: return "*=";
                case JSToken.NotEqual: return "!=";
                case JSToken.PlusAssign: return "+=";
                case JSToken.RightShift: return ">>";
                case JSToken.RightShiftAssign: return ">>=";
                case JSToken.StrictEqual: return "===";
                case JSToken.StrictNotEqual: return "!==";
                case JSToken.UnsignedRightShift: return ">>>";
                case JSToken.UnsignedRightShiftAssign: return ">>>=";
                case JSToken.Divide: return "/";
                case JSToken.DivideAssign: return "/=";
                case JSToken.Let: return "let";
                case JSToken.Const: return "const";

                default: return string.Empty;
            }
        }

        #endregion

        #region Helper methods

        private void AcceptNodeWithParens(Node node, bool needsParens)
        {
            // if we need parentheses, add the opening
            if (needsParens)
            {
                OutputPossibleLineBreak('(');

                // because we output an open paren, reset the start flag
                m_startOfStatement = false;

                // and because we are outputting a paren, we are no longer in a no-in scenario
                m_noIn = false;
            }

            // now output the node
            node.Accept(this);

            // if we need parentheses, add the closing and restore whatever noin state we had
            if (needsParens)
            {
                Output(')');
            }

            // make SURE the start flag is reset
            m_startOfStatement = false;
        }

        /// <summary>
        /// Output everything for a function except the initial keyword
        /// </summary>
        /// <param name="node"></param>
        private void OutputFunctionArgsAndBody(FunctionObject node, bool removeUnused)
        {
            if (node != null)
            {
                m_startOfStatement = false;
                if (node.ParameterDeclarations != null)
                {
                    Indent();
                    OutputPossibleLineBreak('(');
                    MarkSegment(node, null, node.ParametersContext); 

                    // figure out the last referenced argument so we can skip
                    // any that aren't actually referenced
                    int lastRef = node.ParameterDeclarations.Count - 1;

                    // if we're not known at compile time, then we can't leave off unreferenced parameters
                    // (also don't leave things off if we're not hypercrunching)
                    // (also check the kill flag for removing unused parameters)
                    if (removeUnused)
                    {
                        while (lastRef >= 0)
                        {
                            // we want to loop backwards until we either find a parameter that is referenced.
                            // at that point, lastRef will be the index of the last referenced parameter so
                            // we can output from 0 to lastRef
                            var argumentField = (node.ParameterDeclarations[lastRef] as ParameterDeclaration).IfNotNull(p => p.VariableField);
                            if (argumentField != null && !argumentField.IsReferenced)
                            {
                                --lastRef;
                            }
                            else
                            {
                                // found a referenced parameter, or something weird -- stop looking
                                break;
                            }
                        }
                    }

                    Node paramDecl = null;
                    for (var ndx = 0; ndx <= lastRef; ++ndx)
                    {
                        if (ndx > 0)
                        {
                            OutputPossibleLineBreak(',');
                            MarkSegment(node, null, paramDecl.IfNotNull(p => p.TerminatingContext) ?? node.ParametersContext);
                            if (Settings.OutputMode == OutputMode.MultipleLines)
                            {
                                OutputPossibleLineBreak(' ');
                            }
                        }

                        paramDecl = node.ParameterDeclarations[ndx];
                        if (paramDecl != null)
                        {
                            paramDecl.Accept(this);
                        }
                    }

                    Unindent();
                    OutputPossibleLineBreak(')');
                    MarkSegment(node, null, node.ParametersContext); 
                }

                if (node.Body == null || node.Body.Count == 0)
                {
                    Output("{}");
                    MarkSegment(node, null, node.Body.IfNotNull(b => b.Context));
                    BreakLine(false);
                }
                else
                {
                    if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                        || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && node.Body.BraceOnNewLine))
                    {
                        NewLine();
                    }
                    else if (Settings.OutputMode == OutputMode.MultipleLines)
                    {
                        OutputPossibleLineBreak(' ');
                    }

                    node.Body.Accept(this);
                }
            }
        }

        /// <summary>
        /// outputs a semicolon for an empty block, just the statement for a single-statement block,
        /// and recurses to the Block visitor for mutiple-statement blocks
        /// </summary>
        /// <param name="block">block to output</param>
        private void OutputBlock(Block block)
        {
            if (block == null || block.Count == 0)
            {
                // semicolon-replacement cannot generate an empty statement
                OutputPossibleLineBreak(';');
                MarkSegment(block, null, block.IfNotNull(b => b.Context));
            }
            else if (block.Count == 1)
            {
                Indent();
                NewLine();
                if (block[0].HideFromOutput)
                {
                    // semicolon-replacement cannot generate an empty statement
                    OutputPossibleLineBreak(';');
                    MarkSegment(block, null, block.Context);
                }
                else if (block[0] is ImportantComment)
                {
                    // not a REAL statement, so follow the comment with a semicolon to
                    // be the actual statement for this block.
                    block[0].Accept(this);
                    OutputPossibleLineBreak(';');
                    MarkSegment(block, null, block.Context);
                }
                else
                {
                    m_startOfStatement = true;
                    block[0].Accept(this);
                }
                Unindent();
            }
            else
            {
                if (Settings.BlocksStartOnSameLine == BlockStart.NewLine
                    || (Settings.BlocksStartOnSameLine == BlockStart.UseSource && block.BraceOnNewLine))
                {
                    NewLine();
                }
                else if (Settings.OutputMode == OutputMode.MultipleLines)
                {
                    OutputPossibleLineBreak(' ');
                }

                block.Accept(this);
            }
        }

        private string InlineSafeString(string text)
        {
            if (Settings.InlineSafeStrings)
            {
                // if there are ANY potential XML closing tags, which might confuse the browser
                // as to where the end of the inline script really is. Go conservative; the specs
                // say </ should be escaped, even though most browsers are smart enough to look for
                // </script. Also escape any XML CDATA closing tags.
                if (text.IndexOf("</", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // replace all of them with an escaped version so a text-compare won't match
                    text = text.Replace("</", @"<\/");
                }

                // if there are ANY closing CDATA strings...
                if (text.IndexOf("]]>", StringComparison.Ordinal) >= 0)
                {
                    // replace all of them with an escaped version so a text-compare won't match
                    text = text.Replace("]]>", @"]\]>");
                }
            }

            return text;
        }

        #endregion

        #region numeric formatting methods

        public static string NormalizeNumber(double numericValue, Context originalContext)
        {
            // numerics are doubles in JavaScript, so force it now as a shortcut
            if (double.IsNaN(numericValue) || double.IsInfinity(numericValue))
            {
                // weird number -- just return the original source code as-is. 
                if (originalContext != null && !string.IsNullOrEmpty(originalContext.Code)
                    && !originalContext.Document.IsGenerated)
                {
                    return originalContext.Code;
                }

                // Hmmm... don't have an original source. 
                // Must be generated. Just generate the proper JS literal.
                //
                // DANGER! If we just output NaN and Infinity and -Infinity blindly, that assumes
                // that there aren't any local variables in this scope chain with that
                // name, and we're pulling the GLOBAL properties. Might want to use properties
                // on the Number object -- which, of course, assumes that Number doesn't
                // resolve to a local variable...
                string objectName = double.IsNaN(numericValue) ? "NaN" : "Infinity";

                // get the enclosing lexical environment
                /*var enclosingScope = constant.EnclosingLexicalEnvironment;
                if (enclosingScope != null)
                {
                    var reference = enclosingScope.GetIdentifierReference(objectName, null);
                    if (reference.Category != BindingCategory.Predefined)
                    {
                        // NaN/Infinity didn't resolve to the global predefined values!
                        // see if Number does
                        reference = enclosingScope.GetIdentifierReference("Number", null);
                        if (reference.Category == BindingCategory.Predefined)
                        {
                            // use the properties off this object. Not very compact, but accurate.
                            // I don't think there will be any precedence problems with these constructs --
                            // the member-dot operator is pretty high on the precedence scale.
                            if (double.IsPositiveInfinity(doubleValue))
                            {
                                return "Number.POSITIVE_INFINITY";
                            }
                            if (double.IsNegativeInfinity(doubleValue))
                            {
                                return "Number.NEGATIVE_INFINITY";
                            }
                            return "Number.NaN";
                        }
                        else
                        {
                            // that doesn't resolve to the global Number object, either!
                            // well, extreme circumstances. Let's use literals to generate those values.
                            if (double.IsPositiveInfinity(doubleValue))
                            {
                                // 1 divided by zero is +Infinity
                                return "(1/0)";
                            }
                            if (double.IsNegativeInfinity(doubleValue))
                            {
                                // 1 divided by negative zero is -Infinity
                                return "(1/-0)";
                            }
                            // the unary plus converts to a number, and "x" will generate NaN
                            return "(+'x')";
                        }
                    }
                }*/

                // we're good to go -- just return the name because it will resolve to the
                // global properties (make a special case for negative infinity)
                return double.IsNegativeInfinity(numericValue) ? "-Infinity" : objectName;
            }
            else if (numericValue == 0)
            {
                // special case zero because we don't need to go through all those
                // gyrations to get a "0" -- and because negative zero is different
                // than a positive zero
                return 1 / numericValue < 0 ? "-0" : "0";
            }
            else
            {
                // normal string representations
                string normal = GetSmallestRep(numericValue.ToStringInvariant("R"));

                // if this is an integer (no decimal portion)....
                if (Math.Floor(numericValue) == numericValue)
                {
                    // then convert to hex and see if it's smaller.
                    // only really big numbers might be smaller in hex.
                    string hex = NormalOrHexIfSmaller(numericValue, normal);
                    if (hex.Length < normal.Length)
                    {
                        normal = hex;
                    }
                }
                return normal;
            }
        }

        private static string GetSmallestRep(string number)
        {
            Match match = s_decimalFormat.Match(number);
            if (match.Success)
            {
                string mantissa = match.Result("${man}");
                if (string.IsNullOrEmpty(match.Result("${exp}")))
                {
                    if (string.IsNullOrEmpty(mantissa))
                    {
                        // no decimal portion
                        if (string.IsNullOrEmpty(match.Result("${sig}")))
                        {
                            // no non-zero digits in the magnitude either -- must be a zero
                            number = match.Result("${neg}") + "0";
                        }
                        else
                        {
                            // see if there are trailing zeros
                            // that we can use e-notation to make smaller
                            int numZeros = match.Result("${zer}").Length;
                            if (numZeros > 2)
                            {
                                number = match.Result("${neg}") + match.Result("${sig}")
                                    + 'e' + numZeros.ToStringInvariant();
                            }
                        }
                    }
                    else
                    {
                        // there is a decimal portion. Put it back together
                        // with the bare-minimum stuff -- no plus-sign, no leading magnitude zeros,
                        // no trailing mantissa zeros. A zero magnitude won't show up, either.
                        number = match.Result("${neg}") + match.Result("${mag}") + '.' + mantissa;
                    }
                }
                else if (string.IsNullOrEmpty(mantissa))
                {
                    // there is an exponent, but no significant mantissa
                    number = match.Result("${neg}") + match.Result("${mag}")
                        + "e" + match.Result("${eng}") + match.Result("${pow}");
                }
                else
                {
                    // there is an exponent and a significant mantissa
                    // we want to see if we can eliminate it and save some bytes

                    // get the integer value of the exponent
                    int exponent;
                    if ((match.Result("${eng}") + match.Result("${pow}")).TryParseIntInvariant(NumberStyles.Integer, out exponent))
                    {
                        // slap the mantissa directly to the magnitude without a decimal point.
                        // we'll subtract the number of characters we just added to the magnitude from
                        // the exponent
                        number = match.Result("${neg}") + match.Result("${mag}") + mantissa
                            + 'e' + (exponent - mantissa.Length).ToStringInvariant();
                    }
                    else
                    {
                        // should n't get here, but it we do, go with what we have
                        number = match.Result("${neg}") + match.Result("${mag}") + '.' + mantissa
                            + 'e' + match.Result("${eng}") + match.Result("${pow}");
                    }
                }
            }
            return number;
        }

        private static string NormalOrHexIfSmaller(double doubleValue, string normal)
        {
            // keep track of the maximum number of characters we can have in our
            // hexadecimal number before it'd be longer than the normal version.
            // subtract two characters for the 0x
            int maxValue = normal.Length - 2;

            int sign = Math.Sign(doubleValue);
            if (sign < 0)
            {
                // negate the value so it's positive
                doubleValue = -doubleValue;
                // subtract another character for the minus sign
                --maxValue;
            }

            // we don't want to get larger -- or even the same size, so we know
            // the maximum length is the length of the normal string less one
            char[] charArray = new char[normal.Length - 1];
            // point PAST the last character in the array because we will decrement
            // the position before we add a character. that way position will always
            // point to the first valid character in the array.
            int position = charArray.Length;

            while (maxValue > 0 && doubleValue > 0)
            {
                // get the right-most hex character
                int digit = (int)(doubleValue % 16);

                // if the digit is less than ten, then we want to add it to '0' to get the decimal character.
                // otherwise we want to add (digit - 10) to 'a' to get the alphabetic hex digit
                charArray[--position] = (char)((digit < 10 ? '0' : 'a' - 10) + digit);

                // next character
                doubleValue = Math.Floor(doubleValue / 16);
                --maxValue;
            }

            // if the max value is still greater than zero, then the hex value
            // will be shorter than the normal value and we want to go with it
            if (maxValue > 0)
            {
                // add the 0x prefix
                charArray[--position] = 'x';
                charArray[--position] = '0';

                // add the sign if negative
                if (sign < 0)
                {
                    charArray[--position] = '-';
                }

                // create a new string starting at the current position
                normal = new string(charArray, position, charArray.Length - position);
            }
            return normal;
        }

        #endregion

        #region string formatting methods

        public static string EscapeString(string text)
        {
            // the quote factor is a calculation based on the relative number of
            // double-quotes in the string in relation to single-quotes. If the factor is
            // less than zero, then there are more double-quotes than single-quotes and
            // we can save bytes by using single-quotes as the delimiter. If it's greater
            // than zero, then there are more single quotes than double-quotes and we should
            // use double-quotes for the delimiter. If it's exactly zero, then 
            // there are exactly the same number, so it doesn't matter which delimiter
            // we use. In that case, use double-quotes because I think it's easier to read.
            // More like other languages (C/C++, C#, Java) that way.
            var delimiterCharacter = QuoteFactor(text) < 0 ? '\'' : '"';

            // we also don't want to build a new string builder object if we don't have to.
            // and we only need to if we end up escaping characters. 
            var rawStart = 0;
            StringBuilder sb = null;
            string escapedText = string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                // check each character of the string
                for (var index = 0; index < text.Length; ++index)
                {
                    var ch = text[index];
                    switch (ch)
                    {
                        case '\'':
                        case '"':
                            // we only need to escape whichever one we chose as our delimiter
                            if (ch == delimiterCharacter)
                            {
                                // need to escape instances of the delimiter character
                                goto case '\\';
                            }

                            break;

                        case '\b':
                            // output "\b"
                            ch = 'b';
                            goto case '\\';

                        case '\t':
                            // output "\t"
                            ch = 't';
                            goto case '\\';

                        case '\n':
                            // output "\n"
                            ch = 'n';
                            goto case '\\';

                        case '\v':
                            // w3c-strict can encode this character as a \v escape. 
                            // BUT... IE<9 doesn't recognize that escape sequence,
                            // so encode is as hex for maximum compatibility.
                            // if the source actually had "\v" in it, it wouldn't been
                            // marked as having issues and not get encoded anyway.
                            goto default;

                        case '\f':
                            // output "\f"
                            ch = 'f';
                            goto case '\\';

                        case '\r':
                            // output "\r"
                            ch = 'r';
                            goto case '\\';

                        case '\\':
                            // we need to output an escape, so create the string builder
                            // if we haven't already
                            if (sb == null)
                            {
                                sb = new StringBuilder();
                            }

                            // output the block of raw characters we have since the last time
                            if (rawStart < index)
                            {
                                sb.Append(text.Substring(rawStart, index - rawStart));
                            }

                            // set raw start to the next character
                            rawStart = index + 1;

                            // output the escape character, then the escaped character
                            sb.Append('\\');
                            sb.Append(ch);
                            break;

                        case '\x2028':
                        case '\x2029':
                            // issue #14398 - unescaped, these characters (Unicode LineSeparator and ParagraphSeparator)
                            // would introduce a line-break in the string.  they ALWAYS need to be escaped, 
                            // no matter what output encoding we may use.
                            if (sb == null)
                            {
                                sb = new StringBuilder();
                            }

                            // output the block of raw characters we have since the last time
                            if (rawStart < index)
                            {
                                sb.Append(text.Substring(rawStart, index - rawStart));
                            }

                            // set raw start to the next character
                            rawStart = index + 1;

                            // output the escape character, a "u", then the four-digit escaped character
                            sb.Append(@"\u");
                            sb.Append(((int)ch).ToStringInvariant("x4"));
                            break;

                        default:
                            if (ch < ' ')
                            {
                                // need to escape control codes that aren't handled
                                // by the single-letter escape codes
                                // create the string builder if we haven't already
                                if (sb == null)
                                {
                                    sb = new StringBuilder();
                                }

                                // output the block of raw characters we have since the last time
                                if (rawStart < index)
                                {
                                    sb.Append(text.Substring(rawStart, index - rawStart));
                                }

                                // set raw start to the next character
                                rawStart = index + 1;

                                // strict ECMA-262 does not support octal escapes, but octal will
                                // crunch down a full character more here than hexadecimal. Plus, if we do
                                // octal, we'll still need to escape these characters to hex for RexExp
                                // constructor strings so they don't get confused with back references.
                                // minifies smaller, but octal is too much trouble.
                                int intValue = ch;
                                //if (noOctalEscapes)
                                {
                                    // output the hex escape sequence
                                    sb.Append(@"\x");
                                    sb.Append(intValue.ToStringInvariant("x2"));
                                }
                                //else
                                //{
                                //    // octal representation of 0 through 31 are \0 through \37
                                //    sb.Append('\\');
                                //    if (intValue < 8)
                                //    {
                                //        // single octal digit
                                //        sb.Append(intValue.ToStringInvariant());
                                //    }
                                //    else
                                //    {
                                //        // two octal digits
                                //        sb.Append((intValue / 8).ToStringInvariant());
                                //        sb.Append((intValue % 8).ToStringInvariant());
                                //    }
                                //}
                            }

                            break;
                    }
                }

                if (sb != null)
                {
                    // we had escapes; use the string builder
                    // but first make sure the last batch of raw text is output
                    if (rawStart < text.Length)
                    {
                        sb.Append(text.Substring(rawStart));
                    }

                    escapedText = sb.ToString();
                }
                else
                {
                    // no escaped needed; just use the text as-is
                    escapedText = text;
                }
            }

            return delimiterCharacter + escapedText + delimiterCharacter;
        }

        /// <summary>
        /// Counts the number of double-quotes and single-quotes in a string
        /// and returns a numeric indicator for which one should be used as
        /// the string delimiter.
        /// </summary>
        /// <param name="text">string to test</param>
        /// <returns>less than zero use single-quotes, zero or more, use double-quotes</returns>
        private static int QuoteFactor(string text)
        {
            // determine the delimiter to use based on the quote factor.
            // a value less than zero means there are more double-quotes than single-quotes,
            // therefore we should use single-quotes for the delimiter.
            // otherwise there are more single-quotes than double-quotes (or equal values)
            // and it's okay to use double-quotes
            int quoteFactor = 0;
            for (int index = 0; index < text.Length; ++index)
            {
                if (text[index] == '\'')
                {
                    ++quoteFactor;
                }
                else if (text[index] == '"')
                {
                    --quoteFactor;
                }
            }

            return quoteFactor;
        }

        #endregion

        #region Map file methods

        private object StartSymbol(Node node)
        {
            if (Settings.SymbolsMap != null)
            {
                return Settings.SymbolsMap.StartSymbol(node, m_lineCount, m_lineLength);
            }

            return null;
        }

        private void MarkSegment(Node node, string name, Context context)
        {
            if (Settings.SymbolsMap != null && node != null)
            {
                Settings.SymbolsMap.MarkSegment(node, m_segmentStartLine, m_segmentStartColumn, name, context);
            }
        }

        private void EndSymbol(object symbol)
        {
            if (Settings.SymbolsMap != null && symbol != null)
            {
                string parentFunction = null;
                if (m_functionStack.Count > 0)
                {
                    parentFunction = m_functionStack.Peek();
                }

                Settings.SymbolsMap.EndSymbol(symbol, m_lineCount, m_lineLength, parentFunction);
            }
        }

        #endregion Map file methods
    }
#endif
}

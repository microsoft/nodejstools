// jsparser.cs
//
// Copyright 2010 Microsoft Corporation
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Ajax.Utilities;

namespace Microsoft.NodejsTools.Parsing
{
    /// <summary>
    /// Class used to parse JavaScript source code into an abstract syntax tree.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class JSParser
    {
        private const int c_MaxSkippedTokenNumber = 50;

        internal readonly ErrorSink m_errorSink;
        private readonly string _source;
        private JSScanner m_scanner;

        // used for errors to flag that the same token has to be returned.
        // We could have used just a boolean but having a Context does not
        // add any overhead and allow to really save the info, if that will ever be needed
        private bool m_useCurrentForNext;
        private int m_tokensSkipped;
        private NoSkipTokenSet m_noSkipTokenSet;
        private long m_goodTokensProcessed;

        // we're going to copy the debug lookups from the settings passed to us,
        // then use this collection, because we might programmatically add more
        // as we process the code, and we don't want to change the settings object.
        public ICollection<string> DebugLookups { get; private set; }

        // label related info
        private List<BlockType> m_blockType;
        private Dictionary<string, LabelInfo> m_labelTable;
        enum BlockType { Block, Loop, Switch, Finally }
        private int m_finallyEscaped;

        private bool m_foundEndOfLine;
        private CodeSettings m_settings;// = null;

        private int m_breakRecursion;// = 0;
        private int m_severity;
        private JSToken _curToken;
        private IndexSpan _curSpan;
        private string _curDoclet;
        private GlobalScope _globalScope;
        private bool _inGenerator;

        // Doclet associated with a given expression. It's not stored directly on the Expression object to conserve memory.
        private readonly Dictionary<Expression, string> _doclets = new Dictionary<Expression, string>();

        /// <summary>
        /// Creates an instance of the JSParser class that can be used to parse the given source code.
        /// </summary>
        /// <param name="source">Source code to parse.</param>
        public JSParser(string source, ErrorSink errorSink = null) {
            _source = source;
            m_severity = 5;
            m_blockType = new List<BlockType>(16);
            m_labelTable = new Dictionary<string, LabelInfo>();
            m_noSkipTokenSet = new NoSkipTokenSet();

            m_errorSink = errorSink ?? new ErrorSink();
        }

        private void InitializeScanner(CodeSettings settings) {
            // save the settings
            // if we are passed null, just create a default settings object
            m_settings = settings = settings ?? new CodeSettings();
            m_scanner = new JSScanner(_source, m_errorSink, m_settings);
        }

        private class LabelInfo
        {
            public readonly int BlockIndex;
            public readonly int NestLevel;

            public LabelInfo(int blockIndex, int nestLevel)
            {
                BlockIndex = blockIndex;
                NestLevel = nestLevel;
            }
        }

        /// <summary>
        /// Parse the source code using the given settings, getting back an abstract syntax tree Block node as the root
        /// representing the list of statements in the source code.
        /// </summary>
        /// <param name="settings">code settings to use to process the source code</param>
        /// <returns>root Block node representing the top-level statements</returns>
        public JsAst Parse(CodeSettings settings)
        {
            // initialize the scanner with our settings
            // make sure the RawTokens setting is OFF or we won't be able to create our AST
            InitializeScanner(settings);

            var ast = new JsAst(
                EncodeSpan(new IndexSpan(0, _source.Length)), 
                m_scanner.LocationResolver
            );
            var globalScope = new GlobalScope(ast, m_errorSink);
            _globalScope = globalScope;
            // make sure we initialize the global scope's strict mode to our flag, whether or not it
            // is true. This means if the setting is false, we will RESET the flag to false if we are 
            // reusing the scope and a previous Parse call had code that set it to strict with a 
            // program directive. 
            globalScope.UseStrict = m_settings.StrictMode;

            // make sure the global scope knows about our known global names
            globalScope.SetAssumedGlobals(m_settings);

            Block scriptBlock;
            Block returnBlock;
            switch (m_settings.SourceMode)
            {
                case JavaScriptSourceMode.Program:
                    // simply parse a block of statements
                    returnBlock = scriptBlock = ParseStatements();
                    break;
                    
                case JavaScriptSourceMode.Expression:
                    // create a block, get the first token, add in the parse of a single expression, 
                    // and we'll go fron there.
                    returnBlock = scriptBlock = new Block(CurrentPositionEncodedSpan());
                    GetNextToken();
                    try
                    {
                        var expr = ParseExpression();
                        if (expr != null)
                        {
                            var exprStmt = new ExpressionStatement(expr.EncodedSpan);
                            exprStmt.Expression = expr;
                            scriptBlock.Statements = new[] { exprStmt };
                            UpdateWithOtherNode(scriptBlock, expr);
                        }
                    }
                    catch (EndOfFileException)
                    {
                        Debug.WriteLine("EOF");
                    }
                    break;
                default:
                    Debug.Fail("Unexpected source mode enumeration");
                    return null;
            }

            // resolve everything
            ResolutionVisitor.Apply(scriptBlock, globalScope, m_scanner.LocationResolver, m_errorSink);

            if (returnBlock.Parent != null)
            {
                returnBlock.Parent = null;
            }
            ast.Block = returnBlock;

            return ast;
        }

#if FALSE
        /// <summary>
        /// Parse an expression from the source code and return a block node containing just that expression.
        /// The block node is needed because we might perform optimization on the expression that creates
        /// a new expression, and we need a parent to contain it.
        /// </summary>
        /// <param name="codeSettings">settings to use</param>
        /// <returns>a block node containing the parsed expression as its only child</returns>
        [Obsolete("This property is deprecated; call Parse with CodeSettings.SourceMode set to JavaScriptSourceMode.Expression instead")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Block ParseExpression(CodeSettings settings)
        {
            // we need to make sure the settings object has the expression source mode property set,
            // but let's not modify the settings object passed in. So clone it, set the property on the
            // clone, and use that object for parsing.
            settings = settings == null ? new CodeSettings() : settings.Clone();
            settings.SourceMode = JavaScriptSourceMode.Expression;
            return Parse(settings);
        }
#endif
        //---------------------------------------------------------------------------------------
        // ParseStatements
        //
        // statements :
        //   <empty> |
        //   statement statements
        //
        //---------------------------------------------------------------------------------------
        private Block ParseStatements()
        {
            var program = new Block(CurrentPositionEncodedSpan());
            List<Statement> statements = new List<Statement>();
            m_blockType.Add(BlockType.Block);
            m_useCurrentForNext = false;
            try
            {
                // get the first token
                GetNextToken();
                
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_TopLevelNoSkipTokenSet);

                try
                {
                    var possibleDirectivePrologue = true;
                    int lastEndPosition = _curSpan.End;
                    while (_curToken != JSToken.EndOfFile)
                    {
                        Statement ast = null;
                        try
                        {
                            // parse a statement -- pass true because we really want a SourceElement,
                            // which is a Statement OR a FunctionDeclaration. Technically, FunctionDeclarations
                            // are not statements!
                            ast = ParseStatement();

                            // if we are still possibly looking for directive prologues
                            if (possibleDirectivePrologue)
                            {
                                var constantWrapper = Statement.GetExpression(ast) as ConstantWrapper;
                                if (constantWrapper != null && constantWrapper.Value is string)
                                {
                                    if (!(constantWrapper is DirectivePrologue))
                                    {
                                        // use a directive prologue node instead
                                        var exprStmt = new ExpressionStatement(constantWrapper.EncodedSpan);
                                        var directive = new DirectivePrologue(constantWrapper.Value.ToString(), ast.EncodedSpan);
                                        exprStmt.Expression = directive;
                                        if (directive.UseStrict) {
                                            _globalScope.UseStrict = true;
                                        }
                                        ast = exprStmt;
                                    }
                                }
                                else
                                {
                                    // nope -- no longer finding directive prologues
                                    possibleDirectivePrologue = false;
                                }
                            }
                        }
                        catch (RecoveryTokenException exc)
                        {
                            if (TokenInList(NoSkipTokenSet.s_TopLevelNoSkipTokenSet, exc)
                                || TokenInList(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exc))
                            {
                                ast = exc.PartiallyComputedStatement;
                                GetNextToken();
                            }
                            else
                            {
                                m_useCurrentForNext = false;
                                do
                                {
                                    GetNextToken();
                                } while (_curToken != JSToken.EndOfFile && !TokenInList(NoSkipTokenSet.s_TopLevelNoSkipTokenSet, _curToken)
                                  && !TokenInList(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, _curToken));
                            }
                        }

                        if (null != ast)
                        {
                            // append the token to the program
                            statements.Add(ast);

                            // set the last end position to be the start of the current token.
                            // if we parse the next statement and the end is still the start, we know
                            // something is up and might get into an infinite loop.
                            lastEndPosition = _curSpan.End;
                        }
                        else if (!m_scanner.IsEndOfFile && 
                            _curSpan.Start == lastEndPosition)
                        {
                            // didn't parse a statement, we're not at the EOF, and we didn't move
                            // anywhere in the input stream. If we just keep looping around, we're going
                            // to get into an infinite loop. Break it.
                            m_errorSink.HandleError(JSError.ApplicationError, _curSpan, m_scanner.LocationResolver, true);
                            break;
                        }
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_TopLevelNoSkipTokenSet);
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                }

            }
            catch (EndOfFileException)
            {
            }
            program.Statements = statements.ToArray();
            UpdateWithCurrentSpan(program);
            //program.UpdateWith(CurrentPositionSpan());
            return program;
        }

        //---------------------------------------------------------------------------------------
        // ParseStatement
        //
        //  OptionalStatement:
        //    Statement |
        //    <empty>
        //
        //  Statement :
        //    Block |
        //  VariableStatement |
        //  EmptyStatement |
        //  ExpressionStatement |
        //  IfStatement |
        //  IterationStatement |
        //  ContinueStatement |
        //  BreakStatement |
        //  ReturnStatement |
        //  WithStatement |
        //  LabeledStatement |
        //  SwitchStatement |
        //  ThrowStatement |
        //  TryStatement |
        //  FunctionDeclaration
        //
        // IterationStatement :
        //    'for' '(' ForLoopControl ')' |                  ===> ForStatement
        //    'do' Statement 'while' '(' Expression ')' |     ===> DoStatement
        //    'while' '(' Expression ')' Statement            ===> WhileStatement
        //
        //---------------------------------------------------------------------------------------

        // ParseStatement deals with the end of statement issue (EOL vs ';') so if any of the
        // ParseXXX routine does it as well, it should return directly from the switch statement
        // without any further execution in the ParseStatement
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Statement ParseStatement()
        {
            Statement statement = null;

            String id = null;

            switch (_curToken)
            {
                case JSToken.EndOfFile:
                    throw EOFError(JSError.ErrorEndOfFile);
                case JSToken.Semicolon:
                    // make an empty statement
                    statement = new EmptyStatement(EncodeCurrentSpan());
                    GetNextToken();
                    return statement;
                case JSToken.RightCurly:
                    ReportError(JSError.SyntaxError);
                    SkipTokensAndThrow();
                    break;
                case JSToken.LeftCurly:
                    return ParseBlock();
                case JSToken.Debugger:
                    return ParseDebuggerStatement();
                case JSToken.Let:
                    if (JSScanner.IsKeyword("let", _globalScope.UseStrict)) {
                        return ParseVariableStatement();
                    }
                    goto default;
                case JSToken.Var:
                case JSToken.Const:
                    return ParseVariableStatement();
                case JSToken.If:
                    return ParseIfStatement();
                case JSToken.For:
                    return ParseForStatement();
                case JSToken.Do:
                    return ParseDoStatement();
                case JSToken.While:
                    return ParseWhileStatement();
                case JSToken.Continue:
                    return ParseContinueStatement();
                case JSToken.Break:
                    return ParseBreakStatement();
                case JSToken.Return:
                    return ParseReturnStatement();
                case JSToken.With:
                    return ParseWithStatement();
                case JSToken.Switch:
                    return ParseSwitchStatement();
                case JSToken.Throw:
                    statement = ParseThrowStatement();
                    break;
                case JSToken.Try:
                    return ParseTryStatement();
                case JSToken.Function:
                    // parse a function declaration
                    FunctionObject function = ParseFunction(FunctionType.Declaration, _curSpan);
                    return function;
                case JSToken.Else:
                    ReportError(JSError.InvalidElse);
                    SkipTokensAndThrow();
                    break;
                default:
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                    bool exprError = false;
                    try
                    {
                        bool bAssign;
                        Expression expr = ParseUnaryExpression(out bAssign, false);

                        // look for labels
                        if (expr is Lookup && JSToken.Colon == _curToken)
                        {
                            // can be a label
                            id = expr.ToString();
                            if (m_labelTable.ContainsKey(id))
                            {
                                // there is already a label with that name. Ignore the current label
                                ReportError(JSError.BadLabel, expr.EncodedSpan, true);
                                id = null;
                                GetNextToken(); // skip over ':'
                                return new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                            }
                            else
                            {
                                GetNextToken();
                                int labelNestCount = m_labelTable.Count + 1;
                                m_labelTable.Add(id, new LabelInfo(m_blockType.Count, labelNestCount));
                                if (JSToken.EndOfFile != _curToken)
                                {
                                    // ignore any important comments between the label and its statement
                                    // because important comments are treated like statements, and we want
                                    // to make sure the label is attached to the right REAL statement.
                                    var labelTarget = ParseStatement();
                                    statement = new LabeledStatement(expr.EncodedSpan)
                                        {
                                            Label = id,
                                            NestCount = labelNestCount,
                                            Statement = labelTarget
                                        };
                                    if (labelTarget != null) {
                                        statement.EncodedSpan = new EncodedSpan(
                                            m_scanner.LocationResolver, 
                                            GetSpan(statement).UpdateWith(GetSpan(labelTarget))
                                        );
                                    }
                                }
                                else
                                {
                                    // end of the file!
                                    //just pass null for the labeled statement
                                    statement = new LabeledStatement(expr.EncodedSpan)
                                        {
                                            Label = id,
                                            NestCount = labelNestCount
                                        };
                                }
                                m_labelTable.Remove(id);
                                return statement;
                            }
                        }
                        Debug.Assert(statement == null, "losing statement");
                        expr = ParseExpression(expr, false, bAssign, JSToken.None);

                        var binaryOp = expr as BinaryOperator;
                        if (binaryOp != null
                            && (binaryOp.OperatorToken == JSToken.Equal || binaryOp.OperatorToken == JSToken.StrictEqual))
                        {
                            // an expression statement with equality operator? Doesn't really do anything.
                            // Did the developer intend this to be an assignment operator instead? Low-pri warning.
                            m_errorSink.HandleError(JSError.SuspectEquality, GetSpan(binaryOp), m_scanner.LocationResolver);
                        }

                        // we just parsed an expression statement. Now see if we have an appropriate
                        // semicolon to terminate it.
                        var span = expr.EncodedSpan;
                        if (JSToken.Semicolon == _curToken)
                        {
                            span = CombineWithCurrentSpan(span);
                            GetNextToken();
                        }
                        else if (m_foundEndOfLine || JSToken.RightCurly == _curToken || JSToken.EndOfFile == _curToken)
                        {
                            // semicolon insertion rules
                            // (if there was no statement parsed, then don't fire a warning)
                            // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                            // Just too common and doesn't really warrant a warning (in my opinion)
                            if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken)
                            {
                                ReportError(JSError.SemicolonInsertion, GetSpan(expr).FlattenToEnd(), true);
                            }
                        }
                        else
                        {
                            ReportError(JSError.NoSemicolon, true);
                        }
                        var exprStatement = new ExpressionStatement(span);
                        exprStatement.Expression = expr;
                        statement = exprStatement;
                    }
                    catch (RecoveryTokenException exc)
                    {
                        if (exc._partiallyComputedNode != null) {
                            statement = exc.PartiallyComputedStatement;
                        }

                        if (statement == null)
                        {
                            m_noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                            exprError = true;
                            SkipTokensAndThrow();
                        }

                        if (IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exc) == -1)
                        {
                            exc._partiallyComputedNode = statement;
                            throw;
                        }
                    }
                    finally
                    {
                        if (!exprError)
                            m_noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                    }
                    break;
            }

            return statement;
        }

        //---------------------------------------------------------------------------------------
        // ParseBlock
        //
        //  Block :
        //    '{' OptionalStatements '}'
        //---------------------------------------------------------------------------------------
        Block ParseBlock()
        {
            m_blockType.Add(BlockType.Block);
            Block codeBlock = new Block(EncodeCurrentSpan());
            var statements = new List<Statement>();
            codeBlock.Braces = BraceState.Start;
            GetNextToken();

            m_noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
            m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
            try {
                try {
                    try {
                        while (JSToken.RightCurly != _curToken) {
                            try {
                                // pass false because we really only want Statements, and FunctionDeclarations
                                // are technically not statements. We'll still handle them, but we'll issue a warning.
                                statements.Add(ParseStatement());
                            } catch (RecoveryTokenException exc) {
                                if (exc._partiallyComputedNode != null)
                                    statements.Add(exc.PartiallyComputedStatement);
                                if (IndexOfToken(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exc) == -1)
                                    throw;
                            }
                        }
                        codeBlock.Braces = BraceState.StartAndEnd;
                    } catch (RecoveryTokenException exc) {
                        if (IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exc) == -1) {
                            codeBlock.Statements = statements.ToArray();
                            exc._partiallyComputedNode = codeBlock;
                            throw;
                        }
                    }
                } finally {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                    m_blockType.RemoveAt(m_blockType.Count - 1);
                }
            } catch (EndOfFileException) {
                // unclosed block
                m_errorSink.HandleError(JSError.UnclosedBlock, GetSpan(codeBlock), m_scanner._locationResolver, true);
            }

            // update the block context
            UpdateWithCurrentSpan(codeBlock);
            codeBlock.Statements = statements.ToArray();
            GetNextToken();
            return codeBlock;
        }

        //---------------------------------------------------------------------------------------
        // ParseDebuggerStatement
        //
        //  DebuggerStatement :
        //    'debugger'
        //
        // This function may return a null AST under error condition. The caller should handle
        // that case.
        // Regardless of error conditions, on exit the parser points to the first token after
        // the debugger statement
        //---------------------------------------------------------------------------------------
        private Statement ParseDebuggerStatement()
        {
            // clone the current context and skip it
            var node = new DebuggerNode(EncodeCurrentSpan());
            GetNextToken();

            // this token can only be a stand-alone statement
            if (JSToken.Semicolon == _curToken)
            {
                // add the semicolon to the cloned context and skip it
                UpdateWithCurrentSpan(node);
                GetNextToken();
            }
            else if (m_foundEndOfLine || JSToken.RightCurly == _curToken || JSToken.EndOfFile == _curToken)
            {
                // semicolon insertion rules applied
                // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                // Just too common and doesn't really warrant a warning (in my opinion)
                if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken)
                {
                    ReportError(JSError.SemicolonInsertion, GetSpan(node).FlattenToEnd(), true);
                }
            }
            else
            {
                // if it is anything else, it's an error
                ReportError(JSError.NoSemicolon, true);
            }

            // return the new AST object
            return node;
        }

        //---------------------------------------------------------------------------------------
        // ParseVariableStatement
        //
        //  VariableStatement :
        //    'var' VariableDeclarationList
        //    or
        //    'const' VariableDeclarationList
        //    or
        //    'let' VariableDeclarationList
        //
        //  VariableDeclarationList :
        //    VariableDeclaration |
        //    VariableDeclaration ',' VariableDeclarationList
        //
        //  VariableDeclaration :
        //    Identifier Initializer
        //
        //  Initializer :
        //    <empty> |
        //    '=' AssignmentExpression
        //---------------------------------------------------------------------------------------
        private Statement ParseVariableStatement()
        {
            string curDoclet = _curDoclet;

            // create the appropriate statement: var- or const-statement
            Declaration decl;
            if (_curToken == JSToken.Var)
            {
                decl = new Var(EncodeCurrentSpan());
            }
            else if (_curToken == JSToken.Const || _curToken == JSToken.Let)
            {
                if (_curToken == JSToken.Const && m_settings.ConstStatementsMozilla)
                {
                    decl = new ConstStatement(EncodeCurrentSpan());
                }
                else
                {
                    decl = new LexicalDeclaration(EncodeCurrentSpan())
                        {
                            StatementToken = _curToken
                        };
                }
            }
            else
            {
                Debug.Fail("shouldn't get here");
                return null; 
            }

            Node vdecl = null;
            VariableDeclaration identInit = null;
            List<VariableDeclaration> varList = new List<VariableDeclaration>();
            for (; ; )
            {
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfLineToken);
                try
                {
                    identInit = ParseIdentifierInitializer(JSToken.None, curDoclet);
                }
                catch (RecoveryTokenException exc)
                {
                    // an exception is passing by, possibly bringing some info, save the info if any
                    if (IndexOfToken(NoSkipTokenSet.s_EndOfLineToken, exc) == -1) {
                        if (exc._partiallyComputedNode != null) {
                            varList.Add((VariableDeclaration)exc._partiallyComputedNode);
                            UpdateWithOtherNode(decl, exc._partiallyComputedNode);
                            exc._partiallyComputedNode = decl;
                        }
                        decl.Variables = varList.ToArray();
                        throw;
                    } else {
                        identInit = (VariableDeclaration)exc._partiallyComputedNode;
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfLineToken);
                    curDoclet = null;
                }

                if (identInit != null)
                {
                    vdecl = identInit;
                    varList.Add(identInit);
                }

                if (_curToken != JSToken.Comma)
                {
                    if (_curToken == JSToken.Semicolon)
                    {
                        UpdateWithCurrentSpan(decl);
                        GetNextToken();
                        break;
                    }
                    else if (m_foundEndOfLine || _curToken == JSToken.RightCurly || _curToken == JSToken.EndOfFile)
                    {
                        // semicolon insertion rules
                        // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                        // Just too common and doesn't really warrant a warning (in my opinion)
                        if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken)
                        {
                            ReportError(JSError.SemicolonInsertion, GetSpan(decl).FlattenToEnd(), true);
                        }
                        break;
                    }
                    else
                    {
                        ReportError(JSError.NoSemicolon, true);
                        break;
                    }
                }
            }

            if (vdecl != null)
            {
                UpdateWithOtherNode(decl, vdecl);
            }
            decl.Variables = varList.ToArray();
            return decl;
        }

        //---------------------------------------------------------------------------------------
        // ParseIdentifierInitializer
        //
        //  Does the real work of parsing a single variable declaration.
        //  inToken is JSToken.In whenever the potential expression that initialize a variable
        //  cannot contain an 'in', as in the for statement. inToken is JSToken.None otherwise
        //---------------------------------------------------------------------------------------
        private VariableDeclaration ParseIdentifierInitializer(JSToken inToken, string doclet = null)
        {
            string variableName = null;
            Expression assignmentExpr = null;
            RecoveryTokenException except = null;

            GetNextToken();
            if (JSToken.Identifier != _curToken)
            {
                String identifier = CanBeIdentifier(_curToken);
                if (null != identifier)
                {
                    variableName = identifier;
                }
                else
                {
                    // make up an identifier assume we're done with the var statement
                    if (JSScanner.IsValidIdentifier(GetCode(_curSpan)))
                    {
                        // it's probably just a keyword
                        ReportError(JSError.NoIdentifier, _curSpan, true);
                        variableName = GetCode(_curSpan);
                    }
                    else
                    {
                        ReportError(JSError.NoIdentifier);
                        return null;
                    }
                }
            }
            else
            {
                variableName = m_scanner.Identifier;
            }
            IndexSpan idContext = _curSpan;
            IndexSpan span = _curSpan;

            GetNextToken();

            m_noSkipTokenSet.Add(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
            try
            {
                if (JSToken.Assign == _curToken || JSToken.Equal == _curToken)
                {
                    if (JSToken.Equal == _curToken)
                    {
                        ReportError(JSError.NoEqual, true);
                    }


                    // move past the equals sign
                    GetNextToken();

                    try
                    {
                        assignmentExpr = ParseExpression(true, inToken);
                    }
                    catch (RecoveryTokenException exc)
                    {
                        assignmentExpr = (Expression)exc._partiallyComputedNode;
                        throw;
                    }
                    finally
                    {
                        if (null != assignmentExpr)
                        {
                            span = span.UpdateWith(GetSpan(assignmentExpr));
                        }
                    }

                    var funExpr = assignmentExpr as FunctionExpression;
                    if (funExpr != null) {
                        funExpr.Function.NameGuess = variableName;
                        if (funExpr.Function.Doclet == null) {
                            funExpr.Function.Doclet = doclet;
                    }
                }
            }
            }
            catch (RecoveryTokenException exc)
            {
                // If the exception is in the vardecl no-skip set then we successfully
                // recovered to the end of the declaration and can just return
                // normally.  Otherwise we re-throw after constructing the partial result.  
                if (IndexOfToken(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet, exc) == -1)
                    except = exc;
            }
            finally
            {
                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
            }

            VariableDeclaration result = new VariableDeclaration(EncodeSpan(span))
                {
                    Identifier = variableName,
                    NameSpan = idContext,
                    Initializer = assignmentExpr
                };

            if (null != except)
            {
                except._partiallyComputedNode = result;
                throw except;
            }

            return result;
        }

        //---------------------------------------------------------------------------------------
        // ParseIfStatement
        //
        //  IfStatement :
        //    'if' '(' Expression ')' Statement ElseStatement
        //
        //  ElseStatement :
        //    <empty> |
        //    'else' Statement
        //---------------------------------------------------------------------------------------
        private IfNode ParseIfStatement()
        {
            IndexSpan ifSpan = _curSpan;
            Expression condition = null;
            Statement trueBranch = null;
            Statement falseBranch = null;
            int elseStart = -1;

            m_blockType.Add(BlockType.Block);
            try
            {
                // parse condition
                GetNextToken();
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    if (JSToken.LeftParenthesis != _curToken)
                        ReportError(JSError.NoLeftParenthesis);
                    GetNextToken();
                    condition = ParseExpression();

                    // parse statements
                    if (JSToken.RightParenthesis != _curToken) {
                        ifSpan = ifSpan.UpdateWith(GetSpan(condition));
                        ReportError(JSError.NoRightParenthesis);
                    } else {
                        ifSpan = ifSpan.UpdateWith(_curSpan);
                    }

                    GetNextToken();
                }
                catch (RecoveryTokenException exc)
                {
                    // make up an if condition
                    if (exc._partiallyComputedNode != null)
                        condition = (Expression)exc._partiallyComputedNode;
                    else
                        condition = new ConstantWrapper(True, CurrentPositionEncodedSpan());

                    if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) == -1)
                    {
                        exc._partiallyComputedNode = null; // really not much to pass up
                        // the if condition was so bogus we do not have a chance to make an If node, give up
                        throw;
                    }
                    else
                    {
                        if (exc._token == JSToken.RightParenthesis)
                            GetNextToken();
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }

                // if this is an assignment, throw a warning in case the developer
                // meant to use == instead of =
                // but no warning if the condition is wrapped in parens.
                var binOp = condition as BinaryOperator;
                if (binOp != null && binOp.OperatorToken == JSToken.Assign)
                {
                    m_errorSink.HandleError(JSError.SuspectAssignment, GetSpan(condition), m_scanner.LocationResolver);
                }

                m_noSkipTokenSet.Add(NoSkipTokenSet.s_IfBodyNoSkipTokenSet);
                try {
                    try {
                        if (JSToken.Semicolon == _curToken) {
                            m_errorSink.HandleError(JSError.SuspectSemicolon,
                                _curSpan,
                                m_scanner.LocationResolver
                            );
                        } else if (JSToken.LeftCurly != _curToken) {
                            // if the statements aren't withing curly-braces, throw a possible error
                            ReportError(JSError.StatementBlockExpected, CurrentPositionSpan(), true);
                        }

                        try {
                            // parse a Statement, not a SourceElement
                            trueBranch = ParseStatement();
                        } catch (RecoveryTokenException exc) {
                            // make up a block for the if part
                            if (exc._partiallyComputedNode != null)
                                trueBranch = exc.PartiallyComputedStatement;
                            else
                                trueBranch = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                            if (IndexOfToken(NoSkipTokenSet.s_IfBodyNoSkipTokenSet, exc) == -1) {
                                // we have to pass the exception to someone else, make as much as you can from the if
                                exc._partiallyComputedNode = new IfNode(EncodeSpan(ifSpan)) {
                                    Condition = condition,
                                    TrueBlock = Node.ForceToBlock(trueBranch)
                                };
                                throw;
                            }
                        }
                    } finally {
                        if (trueBranch != null) {
                            ifSpan = ifSpan.UpdateWith(GetSpan(trueBranch));
                        }

                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_IfBodyNoSkipTokenSet);
                    }
                } catch (EndOfFileException) {
                    // we still want to produce the if node...
                }

                // parse else, if any
                if (JSToken.Else == _curToken)
                {
                    elseStart = _curSpan.Start;
                    ifSpan = ifSpan.UpdateWith(_curSpan);
                    GetNextToken();
                    try {
                        if (JSToken.Semicolon == _curToken) {
                            m_errorSink.HandleError(JSError.SuspectSemicolon, _curSpan, m_scanner.LocationResolver);
                        } else if (JSToken.LeftCurly != _curToken
                            && JSToken.If != _curToken) {
                            // if the statements aren't withing curly-braces (or start another if-statement), throw a possible error
                            ReportError(JSError.StatementBlockExpected, CurrentPositionSpan(), true);
                        }

                        try {
                            // parse a Statement, not a SourceElement
                            falseBranch = ParseStatement();
                        } catch (RecoveryTokenException exc) {
                            // make up a block for the else part
                            if (exc._partiallyComputedNode != null)
                                falseBranch = exc.PartiallyComputedStatement;
                            else
                                falseBranch = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                            exc._partiallyComputedNode = new IfNode(EncodeSpan(ifSpan)) {
                                Condition = condition,
                                TrueBlock = Node.ForceToBlock(trueBranch),
                                FalseBlock = Node.ForceToBlock(falseBranch),
                                ElseStart = elseStart
                            };
                            throw;
                        } finally {
                            if (falseBranch != null) {
                                ifSpan = ifSpan.UpdateWith(falseBranch.GetSpan(m_scanner.LocationResolver));
                            }
                        }
                    } catch (EndOfFileException) {
                        // we still want to produce the if node...
                    }
                }
            }
            finally
            {
                m_blockType.RemoveAt(m_blockType.Count - 1);
            }

            return new IfNode(EncodeSpan(ifSpan))
                {
                    Condition = condition,
                    TrueBlock = Node.ForceToBlock(trueBranch),
                    FalseBlock = Node.ForceToBlock(falseBranch),
                    ElseStart = elseStart
                };
        }

        //---------------------------------------------------------------------------------------
        // ParseForStatement
        //
        //  ForStatement :
        //    'for' '(' OptionalExpressionNoIn ';' OptionalExpression ';' OptionalExpression ')'
        //    'for' '(' 'var' VariableDeclarationListNoIn ';' OptionalExpression ';' OptionalExpression ')'
        //    'for' '(' LeftHandSideExpression 'in' Expression')'
        //    'for' '(' 'var' Identifier OptionalInitializerNoIn 'in' Expression')'
        //
        //  OptionalExpressionNoIn :
        //    <empty> |
        //    ExpressionNoIn // same as Expression but does not process 'in' as an operator
        //
        //  OptionalInitializerNoIn :
        //    <empty> |
        //    InitializerNoIn // same as initializer but does not process 'in' as an operator
        //---------------------------------------------------------------------------------------
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Statement ParseForStatement()
        {
            m_blockType.Add(BlockType.Loop);
            Statement forNode = null;
            try
            {
                IndexSpan forSpan = _curSpan;
                GetNextToken();
                if (JSToken.LeftParenthesis != _curToken)
                {
                    ReportError(JSError.NoLeftParenthesis);
                }

                GetNextToken();
                bool isForIn = false, recoveryInForIn = false;
                Expression condOrColl = null, increment = null;                
                Statement lhs = null;
                Statement initializer = null;
                int headerEnd = -1;
                List<VariableDeclaration> varList = new List<VariableDeclaration>();
                try
                {
                    if (JSToken.Var == _curToken
                        || JSToken.Let == _curToken
                        || JSToken.Const == _curToken)
                    {
                        isForIn = true;
                        Declaration declaration;
                        if (_curToken == JSToken.Var)
                        {
                            declaration = new Var(EncodeCurrentSpan());
                        }
                        else
                        {
                            declaration = new LexicalDeclaration(EncodeCurrentSpan())
                                {
                                    StatementToken = _curToken
                                };
                        }

                        var varInitializer = ParseIdentifierInitializer(JSToken.In);
                        varList.Add(varInitializer);
                        UpdateWithOtherNode(declaration, varInitializer);

                        // a list of variable initializers is allowed only in a for(;;)
                        while (JSToken.Comma == _curToken)
                        {
                            isForIn = false;
                            varInitializer = ParseIdentifierInitializer(JSToken.In);
                            varList.Add(varInitializer);
                            UpdateWithOtherNode(declaration, initializer);
                            //initializer = new Comma(initializer.context.CombineWith(var.context), initializer, var);
                        }

                        initializer = declaration;
                        declaration.Variables = varList.ToArray();

                        // if it could still be a for..in, now it's time to get the 'in'
                        // TODO: for ES6 might be 'of'
                        if (isForIn)
                        {
                            if (JSToken.In == _curToken
                                /*|| (_curToken == JSToken.Identifier && string.CompareOrdinal(GetCode(_curSpan), "of") == 0)*/)
                            {
                                GetNextToken();
                                condOrColl = ParseExpression();
                            }
                            else
                            {
                                isForIn = false;
                            }
                        }
                    }
                    else
                    {
                        if (JSToken.Semicolon != _curToken)
                        {
                            bool isLHS;
                            var unary = ParseUnaryExpression(out isLHS, false);
                            if (isLHS && (JSToken.In == _curToken
                                /*|| (_curToken == JSToken.Identifier && string.CompareOrdinal(GetCode(_curSpan), "of") == 0)*/))
                            {
                                isForIn = true;

                                var exprStmt = new ExpressionStatement(unary.EncodedSpan);
                                exprStmt.Expression = unary;
                                lhs = exprStmt;
                                GetNextToken();
                                m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                                try
                                {
                                    condOrColl = ParseExpression();
                                }
                                catch (RecoveryTokenException exc)
                                {
                                    if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) == -1)
                                    {
                                        exc._partiallyComputedNode = null;
                                        throw;
                                    }
                                    else
                                    {
                                        if (exc._partiallyComputedNode == null)
                                            condOrColl = new ConstantWrapper(True, CurrentPositionEncodedSpan()); // what could we put here?
                                        else
                                            condOrColl = (Expression)exc._partiallyComputedNode;
                                    }
                                    if (exc._token == JSToken.RightParenthesis)
                                    {
                                        GetNextToken();
                                        recoveryInForIn = true;
                                    }
                                }
                                finally
                                {
                                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                                }
                            }
                            else
                            {
                                var initializerExpr = ParseExpression(unary, false, isLHS, JSToken.In);
                                initializer =
                                    new ExpressionStatement(
                                        initializerExpr.EncodedSpan
                                    ) { Expression = initializerExpr };
                            }
                        }
                    }
                }
                catch (RecoveryTokenException exc)
                {
                    // error is too early abort for
                    exc._partiallyComputedNode = null;
                    throw;
                }

                // at this point we know whether or not is a for..in
                if (isForIn)
                {
                    if (!recoveryInForIn)
                    {
                        if (JSToken.RightParenthesis != _curToken)
                            ReportError(JSError.NoRightParenthesis);
                        forSpan = forSpan.UpdateWith(_curSpan);
                        GetNextToken();
                    }

                    Statement body = null;
                    // if the statements aren't withing curly-braces, throw a possible error
                    if (JSToken.LeftCurly != _curToken)
                    {
                        ReportError(JSError.StatementBlockExpected, CurrentPositionSpan(), true);
                    }
                    try
                    {
                        // parse a Statement, not a SourceElement
                        body = ParseStatement();
                    }
                    catch (RecoveryTokenException exc)
                    {
                        if (exc._partiallyComputedNode == null)
                            body = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                        else
                            body = exc.PartiallyComputedStatement;
                        exc._partiallyComputedNode = new ForIn(EncodeSpan(forSpan))
                            {
                                Variable = (lhs != null ? lhs : initializer),
                                Collection = condOrColl,
                                Body = Node.ForceToBlock(body),
                            };
                        throw;
                    }

                    // for (a in b)
                    //      lhs = a, initializer = null
                    // for (var a in b)
                    //      lhs = null, initializer = var a
                    forNode = new ForIn(EncodeSpan(forSpan.UpdateWith(GetSpan(body))))
                        {
                            Variable = (lhs != null ? lhs : initializer),
                            Collection = condOrColl,
                            Body = Node.ForceToBlock(body),
                        };
                }
                else
                {
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                    try
                    {
                        //  Find Initializer.  We need to check if we are at the end of the intializer.
                        if (JSToken.Semicolon != _curToken) {
                            ReportError(JSError.NoSemicolon, true);
                            if (JSToken.Colon == _curToken) 
                            {
                                m_noSkipTokenSet.Add(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
                                try 
                                {
                                    SkipTokensAndThrow();
                                } 
                                catch (RecoveryTokenException) 
                                {
                                    if (JSToken.Semicolon == _curToken) 
                                    {
                                        m_useCurrentForNext = false;
                                    } 
                                    else 
                                    {
                                        throw;
                                    }
                                } 
                                finally 
                                {
                                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_VariableDeclNoSkipTokenSet);
                                }
                            } else {
                                // we should try to advance to the end of the initializer.
                                SkipToNextToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                            }
                        }

                        // Find Condition
                        GetNextToken();
                        if (JSToken.Semicolon != _curToken)
                        {
                            condOrColl = ParseExpression();
                            if (JSToken.Semicolon != _curToken)
                            {
                                ReportError(JSError.NoSemicolon, true);

                                // we should try to advance to the end of the condition.
                                SkipToNextToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                            }
                        }

                        // Find Incrementer
                        GetNextToken();
                        if (JSToken.RightParenthesis != _curToken)
                        {
                            increment = ParseExpression();
                        }

                        bool missingRightParen = false;
                        if (JSToken.RightParenthesis != _curToken)
                        {
                            missingRightParen = true;
                            ReportError(JSError.NoRightParenthesis, true);

                            // we should try to advance to the end of the incrementer..
                            SkipToNextToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                        }
                        headerEnd = _curSpan.End;

                        forSpan = forSpan.UpdateWith(_curSpan);
                        if (!missingRightParen) {
                            GetNextToken();
                        }
                    }
                    catch (RecoveryTokenException exc)
                    {
                        if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) == -1)
                        {
                            exc._partiallyComputedNode = null;
                            throw;
                        }
                        else
                        {
                            // discard any partial info, just genrate empty condition and increment and keep going
                            exc._partiallyComputedNode = null;
                            if (condOrColl == null)
                                condOrColl = new ConstantWrapper(True, CurrentPositionEncodedSpan());
                        }
                        if (exc._token == JSToken.RightParenthesis)
                        {
                            GetNextToken();
                        }
                    }
                    finally
                    {
                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                    }

                    // if this is an assignment, throw a warning in case the developer
                    // meant to use == instead of =
                    // but no warning if the condition is wrapped in parens.
                    var binOp = condOrColl as BinaryOperator;
                    if (binOp != null && binOp.OperatorToken == JSToken.Assign)
                    {
                        m_errorSink.HandleError(JSError.SuspectAssignment, GetSpan(condOrColl), m_scanner.LocationResolver);
                    }

                    Statement body = null;
                    // if the statements aren't withing curly-braces, throw a possible error
                    if (JSToken.LeftCurly != _curToken)
                    {
                        ReportError(JSError.StatementBlockExpected, CurrentPositionSpan(), true);
                    }
                    try
                    {
                        // parse a Statement, not a SourceElement
                        body = ParseStatement();
                    }
                    catch (RecoveryTokenException exc)
                    {
                        if (exc._partiallyComputedNode == null) {
                            body = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                        } else {
                            body = exc.PartiallyComputedStatement;
                        }
                        exc._partiallyComputedNode = new ForNode(EncodeSpan(forSpan.UpdateWith(GetSpan(body))))
                            {
                                Initializer = initializer,
                                Condition = condOrColl,
                                Incrementer = increment,
                                Body = Node.ForceToBlock(body),
                                HeaderEnd = headerEnd
                            };
                        throw;
                    }
                    forNode = new ForNode(EncodeSpan(forSpan.UpdateWith(GetSpan(body))))
                        {
                            Initializer = initializer,
                            Condition = condOrColl,
                            Incrementer = increment,
                            Body = Node.ForceToBlock(body),
                            HeaderEnd = headerEnd
                        };
                }
            }
            finally
            {
                m_blockType.RemoveAt(m_blockType.Count - 1);
            }

            return forNode;
        }

        //---------------------------------------------------------------------------------------
        // SkipToNextToken
        //
        // Given a token list, we try to advance until we find one of these tokens.
        // If we fail to find one, we throw.
        //---------------------------------------------------------------------------------------
        private void SkipToNextToken(JSToken[] tokenToAddToSet) {
            // Try to find the end of this statment using the tokenset included to determine this.
            try {
                m_noSkipTokenSet.Add(tokenToAddToSet);
                SkipTokensAndThrow();
            } catch (RecoveryTokenException) {
                // if this token we are on was in our list, we can use current for next
                if(Array.Exists(tokenToAddToSet, (t) => t == _curToken)) {
                    m_useCurrentForNext = false;
                } else {
                    throw;
                }
            } finally {
                m_noSkipTokenSet.Remove(tokenToAddToSet);
            }
        }

        //---------------------------------------------------------------------------------------
        // ParseDoStatement
        //
        //  DoStatement:
        //    'do' Statement 'while' '(' Expression ')'
        //---------------------------------------------------------------------------------------
        private DoWhile ParseDoStatement()
        {
            var doSpan = _curSpan;
            IndexSpan whileSpan = new IndexSpan();
            Statement body = null;
            Node condition = null;
            m_blockType.Add(BlockType.Loop);
            try
            {
                GetNextToken();
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_DoWhileBodyNoSkipTokenSet);
                // if the statements aren't withing curly-braces, throw a possible error
                if (JSToken.LeftCurly != _curToken)
                {
                    ReportError(JSError.StatementBlockExpected, CurrentPositionSpan(), true);
                }
                try
                {
                    // parse a Statement, not a SourceElement
                    body = ParseStatement();
                }
                catch (RecoveryTokenException exc)
                {
                    // make up a block for the do while
                    if (exc._partiallyComputedNode != null)
                        body = exc.PartiallyComputedStatement;
                    else
                        body = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                    if (IndexOfToken(NoSkipTokenSet.s_DoWhileBodyNoSkipTokenSet, exc) == -1)
                    {
                        // we have to pass the exception to someone else, make as much as you can from the 'do while'
                        exc._partiallyComputedNode = new DoWhile(EncodeSpan(doSpan.UpdateWith(CurrentPositionSpan())))
                            {
                                Body = Node.ForceToBlock(body),
                                Condition = new ConstantWrapper(False, CurrentPositionEncodedSpan())
                            };
                        throw;
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_DoWhileBodyNoSkipTokenSet);
                }

                if (JSToken.While != _curToken)
                {
                    ReportError(JSError.NoWhile);
                }

                whileSpan = _curSpan;
                doSpan = doSpan.UpdateWith(whileSpan);
                GetNextToken();

                if (JSToken.LeftParenthesis != _curToken)
                {
                    ReportError(JSError.NoLeftParenthesis);
                }

                GetNextToken();
                // catch here so the body of the do_while is not thrown away
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    condition = ParseExpression();
                    if (JSToken.RightParenthesis != _curToken)
                    {
                        ReportError(JSError.NoRightParenthesis);
                        doSpan = doSpan.UpdateWith(GetSpan(condition));
                    }
                    else
                    {
                        doSpan = doSpan.UpdateWith(_curSpan);
                    }

                    GetNextToken();
                }
                catch (RecoveryTokenException exc)
                {
                    // make up a condition
                    if (exc._partiallyComputedNode != null)
                        condition = exc._partiallyComputedNode;
                    else
                        condition = new ConstantWrapper(False, CurrentPositionEncodedSpan());

                    if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) == -1)
                    {
                        exc._partiallyComputedNode = new DoWhile(EncodeSpan(doSpan))
                            {
                                Body = Node.ForceToBlock(body),
                                Condition = (Expression)condition
                            };
                        throw;
                    }
                    else
                    {
                        if (JSToken.RightParenthesis == _curToken)
                            GetNextToken();
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }
                if (JSToken.Semicolon == _curToken)
                {
                    // JScript 5 allowed statements like
                    //   do{print(++x)}while(x<10) print(0)
                    // even though that does not strictly follow the automatic semicolon insertion
                    // rules for the required semi after the while().  For backwards compatibility
                    // we should continue to support this.
                    GetNextToken();
                }
            }
            finally
            {
                m_blockType.RemoveAt(m_blockType.Count - 1);
            }

            // if this is an assignment, throw a warning in case the developer
            // meant to use == instead of =
            // but no warning if the condition is wrapped in parens.
            var binOp = condition as BinaryOperator;
            if (binOp != null && binOp.OperatorToken == JSToken.Assign)
            {
                m_errorSink.HandleError(JSError.SuspectAssignment, GetSpan(condition), m_scanner.LocationResolver);
            }

            return new DoWhile(EncodeSpan(doSpan))
                {
                    Body = Node.ForceToBlock(body),
                    Condition = (Expression)condition,
                };
        }

        //---------------------------------------------------------------------------------------
        // ParseWhileStatement
        //
        //  WhileStatement :
        //    'while' '(' Expression ')' Statement
        //---------------------------------------------------------------------------------------
        private WhileNode ParseWhileStatement()
        {
            IndexSpan whileSpan = _curSpan;
            Expression condition = null;
            Statement body = null;
            m_blockType.Add(BlockType.Loop);
            try
            {
                GetNextToken();
                if (JSToken.LeftParenthesis != _curToken)
                {
                    ReportError(JSError.NoLeftParenthesis);
                }
                GetNextToken();
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    condition = ParseExpression();
                    if (JSToken.RightParenthesis != _curToken) {
                        ReportError(JSError.NoRightParenthesis);
                        whileSpan = whileSpan.UpdateWith(condition.GetSpan(m_scanner.LocationResolver));
                    } else {
                        whileSpan = whileSpan.UpdateWith(_curSpan);
                    }

                    GetNextToken();
                }
                catch (RecoveryTokenException exc)
                {
                    if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) == -1)
                    {
                        // abort the while there is really no much to do here
                        exc._partiallyComputedNode = null;
                        throw;
                    }
                    else
                    {
                        // make up a condition
                        if (exc._partiallyComputedNode != null)
                            condition = (Expression)exc._partiallyComputedNode;
                        else
                            condition = new ConstantWrapper(False, CurrentPositionEncodedSpan());

                        if (JSToken.RightParenthesis == _curToken)
                            GetNextToken();
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }

                // if this is an assignment, throw a warning in case the developer
                // meant to use == instead of =
                // but no warning if the condition is wrapped in parens.
                var binOp = condition as BinaryOperator;
                if (binOp != null && binOp.OperatorToken == JSToken.Assign)
                {
                    m_errorSink.HandleError(JSError.SuspectAssignment, GetSpan(condition), m_scanner.LocationResolver);
                }

                // if the statements aren't withing curly-braces, throw a possible error
                try {
                    if (JSToken.LeftCurly != _curToken)
                    {
                        ReportError(JSError.StatementBlockExpected, CurrentPositionSpan(), true);
                    }
                    whileSpan = whileSpan.UpdateWith(_curSpan);
                    try {
                        // parse a Statement, not a SourceElement
                        body = ParseStatement();
                        whileSpan = whileSpan.UpdateWith(body.GetSpan(m_scanner.LocationResolver));
                    } catch (RecoveryTokenException exc) {
                        if (exc._partiallyComputedNode != null)
                            body = exc.PartiallyComputedStatement;
                        else
                            body = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };

                        exc._partiallyComputedNode = new WhileNode(EncodeSpan(whileSpan)) {
                            Condition = condition,
                            Body = Node.ForceToBlock(body)
                        };
                        throw;
                    }
                } catch (EndOfFileException) {
                    // still produce the while block...
                }

            }
            finally
            {
                m_blockType.RemoveAt(m_blockType.Count - 1);
            }

            return new WhileNode(EncodeSpan(whileSpan))
                {
                    Condition = condition,
                    Body = Node.ForceToBlock(body)
                };
        }

        //---------------------------------------------------------------------------------------
        // ParseContinueStatement
        //
        //  ContinueStatement :
        //    'continue' OptionalLabel
        //
        //  OptionalLabel :
        //    <empty> |
        //    Identifier
        //
        // This function may return a null AST under error condition. The caller should handle
        // that case.
        // Regardless of error conditions, on exit the parser points to the first token after
        // the continue statement
        //---------------------------------------------------------------------------------------
        private ContinueNode ParseContinueStatement()
        {
            var continueNode = new ContinueNode(EncodeCurrentSpan());
            GetNextToken();

            var blocks = 0;
            string label = null;
            if (!m_foundEndOfLine && (JSToken.Identifier == _curToken || (label = CanBeIdentifier(_curToken)) != null))
            {
                UpdateWithCurrentSpan(continueNode);
                continueNode.LabelSpan = _curSpan;
                continueNode.Label = label ?? m_scanner.Identifier;

                // get the label block
                if (!m_labelTable.ContainsKey(continueNode.Label))
                {
                    // the label does not exist. Continue anyway
                    ReportError(JSError.NoLabel, true);
                }
                else
                {
                    var labelInfo = m_labelTable[continueNode.Label];
                    continueNode.NestLevel = labelInfo.NestLevel;

                    blocks = labelInfo.BlockIndex;
                    if (blocks >= m_blockType.Count || m_blockType[blocks] != BlockType.Loop) {
                        ReportError(JSError.BadContinue, continueNode.EncodedSpan, true);
                    }
                }

                GetNextToken();
            }
            else
            {
                blocks = m_blockType.Count - 1;
                while (blocks >= 0 && m_blockType[blocks] != BlockType.Loop) blocks--;
                if (blocks < 0)
                {
                    // the continue is malformed. Continue as if there was no continue at all
                    ReportError(JSError.BadContinue, continueNode.EncodedSpan, true);
                }
            }

            if (JSToken.Semicolon == _curToken)
            {
                UpdateWithCurrentSpan(continueNode);
                GetNextToken();
            }
            else if (m_foundEndOfLine || _curToken == JSToken.RightCurly || _curToken == JSToken.EndOfFile)
            {
                // semicolon insertion rules
                // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                // Just too common and doesn't really warrant a warning (in my opinion)
                if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken)
                {
                    ReportError(JSError.SemicolonInsertion, GetSpan(continueNode).FlattenToEnd(), true);
                }
            }
            else
            {
                ReportError(JSError.NoSemicolon, true);
            }

            if (blocks >= 0) {
                // must ignore the Finally block
                var finallyNum = 0;
                for (int i = blocks, n = m_blockType.Count; i < n; i++) {
                    if (m_blockType[i] == BlockType.Finally) {
                        blocks++;
                        finallyNum++;
                    }
                }

                if (finallyNum > m_finallyEscaped) {
                    m_finallyEscaped = finallyNum;
                }
            }

            return continueNode;
        }

        //---------------------------------------------------------------------------------------
        // ParseBreakStatement
        //
        //  BreakStatement :
        //    'break' OptionalLabel
        //
        // This function may return a null AST under error condition. The caller should handle
        // that case.
        // Regardless of error conditions, on exit the parser points to the first token after
        // the break statement.
        //---------------------------------------------------------------------------------------
        private Break ParseBreakStatement()
        {
            var breakNode = new Break(EncodeCurrentSpan());
            GetNextToken();

            var blocks = 0;
            string label = null;
            if (!m_foundEndOfLine && (JSToken.Identifier == _curToken || (label = CanBeIdentifier(_curToken)) != null))
            {
                UpdateWithCurrentSpan(breakNode);
                breakNode.LabelSpan = _curSpan;
                breakNode.Label = label ?? m_scanner.Identifier;

                // get the label block
                if (!m_labelTable.ContainsKey(breakNode.Label))
                {
                    // as if it was a non label case
                    ReportError(JSError.NoLabel, true);
                }
                else
                {
                    LabelInfo labelInfo = m_labelTable[breakNode.Label];
                    breakNode.NestLevel = labelInfo.NestLevel;
                    blocks = labelInfo.BlockIndex - 1; // the outer block
                    Debug.Assert(m_blockType[blocks] != BlockType.Finally);
                }

                GetNextToken();
            }
            else
            {
                blocks = m_blockType.Count - 1;
                // search for an enclosing loop, if there is no loop it is an error
                while ((m_blockType[blocks] == BlockType.Block || m_blockType[blocks] == BlockType.Finally) && --blocks >= 0) ;
                --blocks;
                if (blocks < 0)
                {
                    ReportError(JSError.BadBreak, breakNode.EncodedSpan, true);
                }
            }

            if (JSToken.Semicolon == _curToken)
            {
                UpdateWithCurrentSpan(breakNode);
                GetNextToken();
            }
            else if (m_foundEndOfLine || _curToken == JSToken.RightCurly || _curToken == JSToken.EndOfFile)
            {
                // semicolon insertion rules
                // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                // Just too common and doesn't really warrant a warning (in my opinion)
                if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken)
                {
                    ReportError(JSError.SemicolonInsertion, GetSpan(breakNode).FlattenToEnd(), true);
                }
            }
            else
            {
                ReportError(JSError.NoSemicolon, true);
            }

            if (blocks >= 0) {
                // must ignore the Finally block
                var finallyNum = 0;
                for (int i = blocks, n = m_blockType.Count; i < n; i++) {
                    if (m_blockType[i] == BlockType.Finally) {
                        blocks++;
                        finallyNum++;
                    }
                }

                if (finallyNum > m_finallyEscaped) {
                    m_finallyEscaped = finallyNum;
                }
            }

            return breakNode;
        }

        private EncodedSpan EncodeCurrentSpan() {
            return EncodeSpan(_curSpan);
        }

        internal EncodedSpan EncodeSpan(IndexSpan span) {
            return new EncodedSpan(m_scanner.LocationResolver, span);
        }

        private void UpdateWithCurrentSpan(Node node) {
            node.EncodedSpan = CombineWithCurrentSpan(node.EncodedSpan);
        }

        private EncodedSpan CombineWithCurrentSpan(EncodedSpan span) {            
            return new EncodedSpan(
                m_scanner.LocationResolver,
                span.GetSpan(m_scanner.LocationResolver).UpdateWith(_curSpan)
            );
        }

        private EncodedSpan CombineWithOtherSpan(EncodedSpan span, EncodedSpan other) {
            return span.CombineWith(
                m_scanner.LocationResolver,
                other
            );
        }

        private void UpdateWithOtherNode(Node node, Node otherNode) {
            var span = node.GetSpan(m_scanner.LocationResolver);
            if (otherNode != null) {
                span = span.UpdateWith(otherNode.GetSpan(m_scanner.LocationResolver));
            }
            node.EncodedSpan = new EncodedSpan(
                m_scanner.LocationResolver,
                span
            );
        }

        //---------------------------------------------------------------------------------------
        // ParseReturnStatement
        //
        //  ReturnStatement :
        //    'return' Expression
        //
        // This function may return a null AST under error condition. The caller should handle
        // that case.
        // Regardless of error conditions, on exit the parser points to the first token after
        // the return statement.
        //---------------------------------------------------------------------------------------
        private ReturnNode ParseReturnStatement()
        {
            var returnNode = new ReturnNode(EncodeCurrentSpan());
            GetNextToken();

            if (!m_foundEndOfLine)
            {
                if (JSToken.Semicolon != _curToken && JSToken.RightCurly != _curToken)
                {
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                    try
                    {
                        returnNode.Operand = ParseExpression();
                    }
                    catch (RecoveryTokenException exc)
                    {
                        returnNode.Operand = (Expression)exc._partiallyComputedNode;
                        if (IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exc) == -1)
                        {
                            exc._partiallyComputedNode = returnNode;
                            throw;
                        }
                    }
                    finally
                    {
                        if (returnNode.Operand != null)
                        {
                            UpdateWithOtherNode(returnNode, returnNode.Operand);
                        }

                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                    }
                }

                if (JSToken.Semicolon == _curToken)
                {
                    UpdateWithCurrentSpan(returnNode);
                    GetNextToken();
                }
                else if (m_foundEndOfLine || _curToken == JSToken.RightCurly || _curToken == JSToken.EndOfFile)
                {
                    // semicolon insertion rules
                    // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                    // Just too common and doesn't really warrant a warning (in my opinion)
                    if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken)
                    {
                        ReportError(JSError.SemicolonInsertion, GetSpan(returnNode).FlattenToEnd(), true);
                    }
                }
                else
                {
                    ReportError(JSError.NoSemicolon, true);
                }
            }

            return returnNode;
        }

        private YieldExpression ParseYieldExpression() {
            var yieldNode = new YieldExpression(EncodeCurrentSpan());
            GetNextToken();
            if (_curToken == JSToken.Multiply) {
                yieldNode.YieldFromIndex = _curSpan.Start;
                GetNextToken();
            }

            m_noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
            try {
                yieldNode.Operand = ParseExpression();
            } catch (RecoveryTokenException exc) {
                yieldNode.Operand = (Expression)exc._partiallyComputedNode;
                if (IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exc) == -1) {
                    exc._partiallyComputedNode = yieldNode;
                    throw;
                }
            } finally {
                if (yieldNode.Operand != null) {
                    UpdateWithOtherNode(yieldNode, yieldNode.Operand);
                }

                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
            }
            

            if (JSToken.Semicolon == _curToken) {
                UpdateWithCurrentSpan(yieldNode);
                GetNextToken();
            } else if (m_foundEndOfLine || _curToken == JSToken.RightCurly || _curToken == JSToken.EndOfFile) {
                // semicolon insertion rules
                // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                // Just too common and doesn't really warrant a warning (in my opinion)
                if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken) {
                    ReportError(JSError.SemicolonInsertion, GetSpan(yieldNode).FlattenToEnd(), true);
                }
            } else {
                ReportError(JSError.NoSemicolon, true);
            }
            

            return yieldNode;
        }


        private static readonly object True = true;
        private static readonly object False = false;

        //---------------------------------------------------------------------------------------
        // ParseWithStatement
        //
        //  WithStatement :
        //    'with' '(' Expression ')' Statement
        //---------------------------------------------------------------------------------------
        private WithNode ParseWithStatement()
        {
            IndexSpan withSpan = _curSpan;
            Node obj = null;
            Block block = null;
            m_blockType.Add(BlockType.Block);
            try
            {
                GetNextToken();
                if (JSToken.LeftParenthesis != _curToken)
                    ReportError(JSError.NoLeftParenthesis);
                GetNextToken();
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                try
                {
                    obj = ParseExpression();
                    if (JSToken.RightParenthesis != _curToken) {
                        withSpan = withSpan.UpdateWith(GetSpan(obj));
                        ReportError(JSError.NoRightParenthesis);
                    } else {
                        withSpan = withSpan.UpdateWith(_curSpan);
                    }
                    GetNextToken();
                }
                catch (RecoveryTokenException exc)
                {
                    if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) == -1)
                    {
                        // give up
                        exc._partiallyComputedNode = null;
                        throw;
                    }
                    else
                    {
                        if (exc._partiallyComputedNode == null) {
                            obj = new ConstantWrapper(True, CurrentPositionEncodedSpan());
                        } else {
                            obj = exc._partiallyComputedNode;
                        }
                        withSpan = withSpan.UpdateWith(GetSpan(obj));

                        if (exc._token == JSToken.RightParenthesis)
                            GetNextToken();
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }

                // if the statements aren't withing curly-braces, throw a possible error
                if (JSToken.LeftCurly != _curToken)
                {
                    ReportError(JSError.StatementBlockExpected, CurrentPositionSpan(), true);
                }

                try
                {
                    // parse a Statement, not a SourceElement
                    block = EnsureBlock(ParseStatement());
                }
                catch (RecoveryTokenException exc)
                {
                    if (exc._partiallyComputedNode == null)
                    {
                        block = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                    }
                    else
                    {
                        block = EnsureBlock(exc.PartiallyComputedStatement);
                    }
                    
                    exc._partiallyComputedNode = new WithNode(EncodeSpan(withSpan.UpdateWith(GetSpan(block))))
                        {
                            WithObject = (Expression)obj,
                            Body = block
                        };
                    throw;
                }
            }
            finally
            {
                m_blockType.RemoveAt(m_blockType.Count - 1);
            }

            return new WithNode(EncodeSpan(withSpan.UpdateWith(GetSpan(block))))
                {
                    WithObject = (Expression)obj,
                    Body = block
                };
        }

        private IndexSpan GetSpan(Node block) {
            return block.GetSpan(m_scanner.LocationResolver);
        }

        private Block EnsureBlock(Statement statement) {
            // make sure we save it as a block
            var block = statement as Block;
            if (block == null) {
                block = new Block(statement.EncodedSpan) { Statements = new[] { statement } };
            }
            return block;
        }

        //---------------------------------------------------------------------------------------
        // ParseSwitchStatement
        //
        //  SwitchStatement :
        //    'switch' '(' Expression ')' '{' CaseBlock '}'
        //
        //  CaseBlock :
        //    CaseList DefaultCaseClause CaseList
        //
        //  CaseList :
        //    <empty> |
        //    CaseClause CaseList
        //
        //  CaseClause :
        //    'case' Expression ':' OptionalStatements
        //
        //  DefaultCaseClause :
        //    <empty> |
        //    'default' ':' OptionalStatements
        //---------------------------------------------------------------------------------------
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private Statement ParseSwitchStatement()
        {
            IndexSpan switchSpan = _curSpan;
            Node expr = null;
            List<SwitchCase> cases = null;
            m_blockType.Add(BlockType.Switch);
            int blockStart = -1;
            try
            {
                // read switch(expr)
                GetNextToken();
                if (JSToken.LeftParenthesis != _curToken) {
                    ReportError(JSError.NoLeftParenthesis);
                }
                GetNextToken();
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                try
                {
                    expr = ParseExpression();

                    if (JSToken.RightParenthesis != _curToken)
                    {
                        ReportError(JSError.NoRightParenthesis);
                    }

                    GetNextToken();
                    blockStart = _curSpan.Start;
                    if (JSToken.LeftCurly != _curToken)
                    {
                        ReportError(JSError.NoLeftCurly);
                    }

                    GetNextToken();

                }
                catch (RecoveryTokenException exc)
                {
                    if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) == -1
                          && IndexOfToken(NoSkipTokenSet.s_SwitchNoSkipTokenSet, exc) == -1)
                    {
                        // give up
                        exc._partiallyComputedNode = null;
                        throw;
                    }
                    else
                    {
                        if (exc._partiallyComputedNode == null)
                            expr = new ConstantWrapper(True, CurrentPositionEncodedSpan());
                        else
                            expr = exc._partiallyComputedNode;

                        if (IndexOfToken(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet, exc) != -1)
                        {
                            if (exc._token == JSToken.RightParenthesis)
                                GetNextToken();

                            if (JSToken.LeftCurly != _curToken)
                            {
                                ReportError(JSError.NoLeftCurly);
                            }
                            GetNextToken();
                        }

                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                }

                // parse the switch body
                cases = new List<SwitchCase>();
                bool defaultStatement = false;
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                try
                {
                    while (JSToken.RightCurly != _curToken)
                    {
                        SwitchCase caseClause = null;
                        Node caseValue = null;
                        int colonIndex = -1;
                        var caseSpan = _curSpan;
                        m_noSkipTokenSet.Add(NoSkipTokenSet.s_CaseNoSkipTokenSet);
                        try
                        {
                            if (JSToken.Case == _curToken)
                            {
                                // get the case
                                GetNextToken();
                                caseValue = ParseExpression();
                            }
                            else if (JSToken.Default == _curToken)
                            {
                                // get the default
                                if (defaultStatement)
                                {
                                    // we report an error but we still accept the default
                                    ReportError(JSError.DupDefault, true);
                                }
                                else
                                {
                                    defaultStatement = true;
                                }
                                GetNextToken();
                            }
                            else
                            {
                                // This is an error, there is no case or default. Assume a default was missing and keep going
                                defaultStatement = true;
                                ReportError(JSError.BadSwitch);
                            }

                            if (JSToken.Colon != _curToken) {
                                ReportError(JSError.NoColon);
                            } else {
                                colonIndex = _curSpan.Start;
                            }

                            // read the statements inside the case or default
                            GetNextToken();
                        }
                        catch (RecoveryTokenException exc)
                        {
                            // right now we can only get here for the 'case' statement
                            if (IndexOfToken(NoSkipTokenSet.s_CaseNoSkipTokenSet, exc) == -1)
                            {
                                // ignore the current case or default
                                exc._partiallyComputedNode = null;
                                throw;
                            }
                            else
                            {
                                caseValue = exc._partiallyComputedNode;

                                if (exc._token == JSToken.Colon)
                                {
                                    GetNextToken();
                                }
                            }
                        }
                        finally
                        {
                            m_noSkipTokenSet.Remove(NoSkipTokenSet.s_CaseNoSkipTokenSet);
                        }

                        m_blockType.Add(BlockType.Block);
                        try
                        {
                            var block = new Block(EncodeCurrentSpan());
                            var statements = new List<Statement>();
                            m_noSkipTokenSet.Add(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                            m_noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                            try
                            {
                                while (JSToken.RightCurly != _curToken && JSToken.Case != _curToken && JSToken.Default != _curToken)
                                {
                                    try
                                    {
                                        // parse a Statement, not a SourceElement
                                        var caseStmt = ParseStatement();
                                        statements.Add(caseStmt);
                                        UpdateWithOtherNode(block, caseStmt);
                                    }
                                    catch (RecoveryTokenException exc)
                                    {
                                        if (exc._partiallyComputedNode != null)
                                        {
                                            statements.Add(exc.PartiallyComputedStatement);
                                            exc._partiallyComputedNode = null;
                                        }

                                        if (IndexOfToken(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exc) == -1)
                                        {
                                            throw;
                                        }
                                    }
                                }
                            }
                            catch (RecoveryTokenException exc)
                            {
                                if (IndexOfToken(NoSkipTokenSet.s_SwitchNoSkipTokenSet, exc) == -1)
                                {
                                    caseClause = new SwitchCase(EncodeSpan(caseSpan.UpdateWith(block.GetSpan(m_scanner.LocationResolver))))
                                        {
                                            CaseValue = (Expression)caseValue,
                                            Statements = block,
                                            ColonIndex = colonIndex
                                        };
                                    cases.Add(caseClause);
                                    throw;
                                }
                            }
                            finally
                            {
                                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_SwitchNoSkipTokenSet);
                                block.Statements = statements.ToArray();
                            }

                            var blockSpan = block.GetSpan(m_scanner.LocationResolver);
                            caseSpan = caseSpan.UpdateWith(blockSpan);
                            caseClause = new SwitchCase(EncodeSpan(caseSpan.UpdateWith(blockSpan)))
                                {
                                    CaseValue = (Expression)caseValue,
                                    Statements = block,
                                    ColonIndex = colonIndex
                                };
                            cases.Add(caseClause);
                        }
                        finally
                        {
                            m_blockType.RemoveAt(m_blockType.Count - 1);
                        }
                    }
                }
                catch (RecoveryTokenException exc)
                {
                    if (IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exc) == -1)
                    {
                        //save what you can a rethrow
                        switchSpan = switchSpan.UpdateWith(CurrentPositionSpan());
                        exc._partiallyComputedNode = new Switch(EncodeSpan(switchSpan))
                            {
                                Expression = (Expression)expr,
                                Cases = ToArray(cases),
                                BlockStart = blockStart
                            };
                        throw;
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                }
                switchSpan = switchSpan.UpdateWith(_curSpan);
                GetNextToken();
            }
            finally
            {
                m_blockType.RemoveAt(m_blockType.Count - 1);
            }

            return new Switch(EncodeSpan(switchSpan))
                {
                    Expression = (Expression)expr,
                    Cases = ToArray(cases),
                    BlockStart = blockStart
                };
        }

        //---------------------------------------------------------------------------------------
        // ParseThrowStatement
        //
        //  ThrowStatement :
        //    throw |
        //    throw Expression
        //---------------------------------------------------------------------------------------
        private ThrowNode ParseThrowStatement()
        {
            var throwNode = new ThrowNode(EncodeCurrentSpan());
            GetNextToken();

            if (!m_foundEndOfLine)
            {
                if (JSToken.Semicolon != _curToken)
                {
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                    try
                    {
                        throwNode.Operand = ParseExpression();
                    }
                    catch (RecoveryTokenException exc)
                    {
                        throwNode.Operand = (Expression)exc._partiallyComputedNode;
                        if (IndexOfToken(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet, exc) == -1)
                        {
                            exc._partiallyComputedNode = throwNode;
                            throw;
                        }
                    }
                    finally
                    {
                        if (throwNode.Operand != null)
                        {
                            UpdateWithOtherNode(throwNode, throwNode.Operand);
                        }

                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_EndOfStatementNoSkipTokenSet);
                    }
                }

                if (_curToken == JSToken.Semicolon)
                {
                    UpdateWithCurrentSpan(throwNode);
                    GetNextToken();
                }
                else if (m_foundEndOfLine || _curToken == JSToken.RightCurly || _curToken == JSToken.EndOfFile)
                {
                    // semicolon insertion rules
                    // a right-curly or an end of line is something we don't WANT to throw a warning for. 
                    // Just too common and doesn't really warrant a warning (in my opinion)
                    if (JSToken.RightCurly != _curToken && JSToken.EndOfFile != _curToken)
                    {
                        ReportError(JSError.SemicolonInsertion, GetSpan(throwNode).FlattenToEnd(), true);
                    }
                }
                else
                {
                    ReportError(JSError.NoSemicolon, true);
                }
            }

            return throwNode;
        }

        //---------------------------------------------------------------------------------------
        // ParseTryStatement
        //
        //  TryStatement :
        //    'try' Block Catch Finally
        //
        //  Catch :
        //    <empty> | 'catch' '(' Identifier ')' Block
        //
        //  Finally :
        //    <empty> |
        //    'finally' Block
        //---------------------------------------------------------------------------------------
        private Statement ParseTryStatement()
        {
            IndexSpan trySpan = _curSpan;
            Block body = null;
            ParameterDeclaration catchParameter = null;
            Block handler = null;
            Block finally_block = null;
            RecoveryTokenException excInFinally = null;
            m_blockType.Add(BlockType.Block);
            int catchStart = -1, finallyStart = -1;
            try
            {
                bool catchOrFinally = false;
                GetNextToken();
                if (JSToken.LeftCurly != _curToken)
                {
                    ReportError(JSError.NoLeftCurly);
                }
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_NoTrySkipTokenSet);
                try
                {
                    body = ParseBlock();
                }
                catch (RecoveryTokenException exc)
                {
                    if (IndexOfToken(NoSkipTokenSet.s_NoTrySkipTokenSet, exc) == -1)
                    {
                        // do nothing and just return the containing block, if any
                        throw;
                    }
                    else
                    {
                        body = EnsureBlock(exc.PartiallyComputedStatement);
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_NoTrySkipTokenSet);
                }
                if (JSToken.Catch == _curToken)
                {
                    catchStart = _curSpan.Start;
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_NoTrySkipTokenSet);
                    try
                    {
                        catchOrFinally = true;
                        GetNextToken();
                        if (JSToken.LeftParenthesis != _curToken)
                        {
                            ReportError(JSError.NoLeftParenthesis);
                        }

                        GetNextToken();
                        if (JSToken.Identifier != _curToken)
                        {
                            string identifier = CanBeIdentifier(_curToken);
                            if (null != identifier)
                            {
                                catchParameter = new ParameterDeclaration(EncodeCurrentSpan()) {
                                    Name = identifier
                                };
                            }
                            else
                            {
                                ReportError(JSError.NoIdentifier);
                                catchParameter = new ParameterDeclaration(EncodeCurrentSpan()) {
                                    Name = GetCode(_curSpan)
                                };
                            }
                        }
                        else
                        {
                            catchParameter = new ParameterDeclaration(EncodeCurrentSpan()) {
                                Name = m_scanner.Identifier
                            };
                        }

                        GetNextToken();
                        m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                        try
                        {
                            if (JSToken.RightParenthesis != _curToken)
                            {
                                ReportError(JSError.NoRightParenthesis);
                            }
                            GetNextToken();
                        }
                        finally
                        {
                            m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockConditionNoSkipTokenSet);
                        }

                        if (JSToken.LeftCurly != _curToken)
                        {
                            ReportError(JSError.NoLeftCurly);
                        }

                        // parse the block
                        handler = ParseBlock();
                        
                        trySpan = trySpan.UpdateWith(GetSpan(handler));
                    }
                    catch (RecoveryTokenException exc)
                    {
                        if (exc._partiallyComputedNode == null)
                        {
                            handler = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements };
                        }
                        else
                        {
                            handler = EnsureBlock(exc.PartiallyComputedStatement);
                        }
                        if (IndexOfToken(NoSkipTokenSet.s_NoTrySkipTokenSet, exc) == -1)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_NoTrySkipTokenSet);
                    }
                }

                try
                {
                    if (JSToken.Finally == _curToken)
                    {
                        finallyStart = _curSpan.Start;
                        GetNextToken();
                        m_blockType.Add(BlockType.Finally);
                        try
                        {
                            finally_block = ParseBlock();
                            catchOrFinally = true;
                        }
                        finally
                        {
                            m_blockType.RemoveAt(m_blockType.Count - 1);
                        }
                        trySpan = trySpan.UpdateWith(GetSpan(finally_block));
                    }
                }
                catch (RecoveryTokenException exc)
                {
                    excInFinally = exc; // thrown later so we can execute code below
                }

                if (!catchOrFinally)
                {
                    ReportError(JSError.NoCatch, true);
                    finally_block = new Block(CurrentPositionEncodedSpan()) { Statements = Block.EmptyStatements }; // make a dummy empty block
                }
            }
            finally
            {
                m_blockType.RemoveAt(m_blockType.Count - 1);
            }

            var span = trySpan;
            if (handler != null) {
                span = trySpan.UpdateWith(GetSpan(handler));
            }
            if (finally_block != null) {
                span = trySpan.UpdateWith(GetSpan(finally_block));
            }

            if (excInFinally != null)
            {
                excInFinally._partiallyComputedNode = new TryNode(EncodeSpan(span))
                    {
                        TryBlock = body,
                        CatchParameter = catchParameter,
                        CatchBlock = handler,
                        FinallyBlock = finally_block,
                        CatchStart = catchStart,
                        FinallyStart = finallyStart
                    };
                throw excInFinally;
            }
            return new TryNode(EncodeSpan(span))
                {
                    TryBlock = body,
                    CatchParameter = catchParameter,
                    CatchBlock = handler,
                    FinallyBlock = finally_block,
                    CatchStart = catchStart,
                    FinallyStart = finallyStart
                };
        }

        private string GetCode(IndexSpan context) {
            return GetCode(context, _source);
        }

        internal static string GetCode(IndexSpan context, string source) {
            return (context.End > context.Start &&
                context.End <= source.Length)
              ? source.Substring(context.Start, context.End - context.Start)
              : null;
        }

        //---------------------------------------------------------------------------------------
        // ParseFunction
        //
        //  FunctionDeclaration :
        //    VisibilityModifier 'function' Identifier '('
        //                          FormalParameterList ')' '{' FunctionBody '}'
        //
        //  FormalParameterList :
        //    <empty> |
        //    IdentifierList Identifier
        //
        //  IdentifierList :
        //    <empty> |
        //    Identifier, IdentifierList
        //---------------------------------------------------------------------------------------
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private FunctionObject ParseFunction(FunctionType functionType, IndexSpan fncSpan) {
            List<ParameterDeclaration> formalParameters = null;
            Block body = null;
            bool inExpression = (functionType == FunctionType.Expression);
            IndexSpan paramsSpan = new IndexSpan();

            string doclet = _curDoclet;

            GetNextToken();
            int generatorIndex = -1;
            if (_curToken == JSToken.Multiply) {
                generatorIndex = _curSpan.Start;
                GetNextToken();
            }

            bool wasInGenerator = _inGenerator;
            try {
                _inGenerator = generatorIndex != -1;
                // get the function name or make an anonymous function if in expression "position"
                Lookup name = ParseBindingIdentifier();
                if (name == null && !inExpression) {
                    // if this isn't a function expression, then we need to throw an error because
                    // function DECLARATIONS always need a valid identifier name
                    ReportError(JSError.NoIdentifier, _curSpan, true);

                    // BUT if the current token is a left paren, we don't want to use it as the name.
                    // (fix for issue #14152)
                    if (_curToken != JSToken.LeftParenthesis && _curToken != JSToken.LeftCurly) {
                        string identifier = GetCode(_curSpan);
                        name = new Lookup(EncodeSpan(CurrentPositionSpan())) {
                            Name = identifier
                        };
                        GetNextToken();
                    }
                }

                // make a new state and save the old one
                List<BlockType> blockType = m_blockType;
                m_blockType = new List<BlockType>(16);
                Dictionary<string, LabelInfo> labelTable = m_labelTable;
                m_labelTable = new Dictionary<string, LabelInfo>();

                try {
                    // get the formal parameters
                    if (JSToken.LeftParenthesis != _curToken) {
                        // we expect a left paren at this point for standard cross-browser support.
                        // BUT -- some versions of IE allow an object property expression to be a function name, like window.onclick. 
                        // we still want to throw the error, because it syntax errors on most browsers, but we still want to
                        // be able to parse it and return the intended results. 
                        // Skip to the open paren and use whatever is in-between as the function name. Doesn't matter that it's 
                        // an invalid identifier; it won't be accessible as a valid field anyway.
                        bool expandedIndentifier = false;
                        while (_curToken != JSToken.LeftParenthesis
                            && _curToken != JSToken.LeftCurly
                            && _curToken != JSToken.Semicolon
                                && _curToken != JSToken.EndOfFile) {
                            if (name != null) {
                                UpdateWithCurrentSpan(name);
                            }
                            GetNextToken();
                            expandedIndentifier = true;
                        }

                        // if we actually expanded the identifier context, then we want to report that
                        // the function name needs to be an identifier. Otherwise we didn't expand the 
                        // name, so just report that we expected an open paren at this point.
                        if (expandedIndentifier) {
                            if (name != null) {
                                name.Name = GetCode(GetSpan(name));
                                m_errorSink.HandleError(JSError.FunctionNameMustBeIdentifier, GetSpan(name), m_scanner.LocationResolver);
                            } else {
                                m_errorSink.HandleError(JSError.FunctionNameMustBeIdentifier, CurrentPositionSpan(), m_scanner.LocationResolver);
                            }
                        } else {
                            ReportError(JSError.NoLeftParenthesis, true);
                        }
                    }

                    if (_curToken == JSToken.LeftParenthesis) {
                        // create the parameter list
                        formalParameters = new List<ParameterDeclaration>();
                        paramsSpan = _curSpan;

                        // skip the open paren
                        GetNextToken();

                        // create the list of arguments and update the context
                        while (JSToken.RightParenthesis != _curToken) {
                            String id = null;
                            m_noSkipTokenSet.Add(NoSkipTokenSet.s_FunctionDeclNoSkipTokenSet);
                            try {
                                ParameterDeclaration paramDecl = null;
                                if (JSToken.Identifier != _curToken && (id = CanBeIdentifier(_curToken)) == null) {
                                    if (JSToken.LeftCurly == _curToken) {
                                        ReportError(JSError.NoRightParenthesis);
                                        break;
                                    } else if (JSToken.Comma == _curToken) {
                                        // We're missing an argument (or previous argument was malformed and
                                        // we skipped to the comma.)  Keep trying to parse the argument list --
                                        // we will skip the comma below.
                                        ReportError(JSError.SyntaxError, true);
                                    } else {
                                        ReportError(JSError.SyntaxError, true);
                                        SkipTokensAndThrow();
                                    }
                                } else {
                                    if (null == id) {
                                        id = m_scanner.Identifier;
                                    }

                                    paramDecl = new ParameterDeclaration(EncodeCurrentSpan()) {
                                        Name = id,
                                        Position = formalParameters.Count
                                    };
                                    paramsSpan = paramsSpan.UpdateWith(_curSpan);
                                    formalParameters.Add(paramDecl);
                                    GetNextToken();
                                }

                                // got an arg, it should be either a ',' or ')'
                                if (JSToken.RightParenthesis == _curToken) {
                                    paramsSpan = paramsSpan.UpdateWith(_curSpan);
                                    break;
                                } else if (JSToken.Comma == _curToken) {
                                    // append the comma context as the terminator for the parameter
                                } else {
                                    // deal with error in some "intelligent" way
                                    if (JSToken.LeftCurly == _curToken) {
                                        ReportError(JSError.NoRightParenthesis);
                                        break;
                                    } else {
                                        if (JSToken.Identifier == _curToken) {
                                            // it's possible that the guy was writing the type in C/C++ style (i.e. int x)
                                            ReportError(JSError.NoCommaOrTypeDefinitionError);
                                        } else
                                            ReportError(JSError.NoComma);
                                    }
                                }

                                GetNextToken();
                            } catch (RecoveryTokenException exc) {
                                if (IndexOfToken(NoSkipTokenSet.s_FunctionDeclNoSkipTokenSet, exc) == -1)
                                    throw;
                            } finally {
                                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_FunctionDeclNoSkipTokenSet);
                            }
                        }

                        fncSpan = fncSpan.UpdateWith(_curSpan);
                        GetNextToken();
                    }

                    // read the function body of non-abstract functions.
                    if (JSToken.LeftCurly != _curToken)
                        ReportError(JSError.NoLeftCurly, true);

                    m_blockType.Add(BlockType.Block);
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                    body = new Block(EncodeCurrentSpan());
                    List<Statement> statements = new List<Statement>();
                    body.Braces = BraceState.Start;
                    try {
                        // parse the block locally to get the exact end of function
                        GetNextToken();

                        var possibleDirectivePrologue = true;
                        while (JSToken.RightCurly != _curToken) {
                            try {
                                // function body's are SourceElements (Statements + FunctionDeclarations)
                                var statement = ParseStatement();
                                if (possibleDirectivePrologue && !(statement is ReturnNode)) {
                                    var constantWrapper = Statement.GetExpression(statement) as ConstantWrapper;
                                    if (constantWrapper != null && constantWrapper.Value is string) {
                                        // if it's already a directive prologues, we're good to go
                                        // make the statement a directive prologue instead of a constant wrapper
                                        var exprStmt = new ExpressionStatement(statement.EncodedSpan);
                                        exprStmt.Expression = new DirectivePrologue(constantWrapper.Value.ToString(), constantWrapper.EncodedSpan);
                                        statement = exprStmt;
                                    } else {
                                        // no longer considering constant wrappers
                                        possibleDirectivePrologue = false;
                                    }
                                }
                                // add it to the body
                                statements.Add(statement);
                            } catch (RecoveryTokenException exc) {
                                if (exc._partiallyComputedNode != null) {
                                    statements.Add(exc.PartiallyComputedStatement);
                                }
                                if (IndexOfToken(NoSkipTokenSet.s_StartStatementNoSkipTokenSet, exc) == -1) {
                                    body.Statements = statements.ToArray();
                                    throw;
                                }
                            }
                        }
                        body.Braces = BraceState.StartAndEnd;
                        UpdateWithCurrentSpan(body);
                        body.Statements = statements.ToArray();
                        fncSpan = fncSpan.UpdateWith(_curSpan);
                    } catch (EndOfFileException) {
                        // if we get an EOF here, we never had a chance to find the closing curly-brace
                        m_errorSink.HandleError(JSError.UnclosedFunction, fncSpan, m_scanner._locationResolver, true);
                        body.Statements = statements.ToArray();
                        UpdateWithCurrentSpan(body);
                        fncSpan = fncSpan.UpdateWith(_curSpan);
                    } catch (RecoveryTokenException exc) {
                        fncSpan = fncSpan.UpdateWith(_curSpan);
                        if (IndexOfToken(NoSkipTokenSet.s_BlockNoSkipTokenSet, exc) == -1) {
                            UpdateWithCurrentSpan(body);
                            body.Statements = statements.ToArray();
                            exc._partiallyComputedNode = new FunctionObject(EncodeSpan(fncSpan)) {
                                FunctionType = (inExpression ? FunctionType.Expression : FunctionType.Declaration),
                                NameSpan = name != null ? GetSpan(name) : default(IndexSpan),
                                Name = name != null ? name.Name : null,
                                ParameterDeclarations = ToArray(formalParameters),
                                ParametersSpan = paramsSpan,
                                Body = body,
                                GeneratorIndex = generatorIndex
                            };
                            throw;
                        }
                    } finally {
                        m_blockType.RemoveAt(m_blockType.Count - 1);
                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_StartStatementNoSkipTokenSet);
                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BlockNoSkipTokenSet);
                    }

                    GetNextToken();
                } finally {
                    // restore state
                    m_blockType = blockType;
                    m_labelTable = labelTable;
                }

                return new FunctionObject(EncodeSpan(fncSpan)) {
                    FunctionType = functionType,
                    NameSpan = name != null ? GetSpan(name) : default(IndexSpan),
                    Name = name != null ? name.Name : null,
                    ParameterDeclarations = ToArray(formalParameters),
                    ParametersSpan = paramsSpan,
                    Body = body,
                    Doclet = doclet,
                    GeneratorIndex = generatorIndex
                };
            } finally {
                _inGenerator = wasInGenerator;
            }
        }

        private Lookup ParseBindingIdentifier() {
            Lookup name = null;
            if (JSToken.Identifier == _curToken) {
                name = new Lookup(EncodeCurrentSpan()) {
                    Name = m_scanner.Identifier
                };
                GetNextToken();
            } else {
                string identifier = CanBeIdentifier(_curToken);
                if (null != identifier) {
                    name = new Lookup(EncodeCurrentSpan()) {
                        Name = identifier
                };
                    GetNextToken();
                } else {
                    name = null;
                }
            }

            return name;
        }

        static class EmptyArray<T> {
            public static readonly T[] Instance = new T[0];
        }

        private static T[] ToArray<T>(List<T> list) {
            if (list == null) {
                return null;
            }
            if (list.Count == 0) {
                return EmptyArray<T>.Instance;
            }
            return list.ToArray();
        }


        //---------------------------------------------------------------------------------------
        // ParseExpression
        //
        //  Expression :
        //    AssignmentExpressionList AssignmentExpression
        //
        //  AssignmentExpressionList :
        //    <empty> |
        //    AssignmentExpression ',' AssignmentExpressionList
        //
        //  AssignmentExpression :
        //    ConditionalExpression |
        //    YieldExpression |
        //    LeftHandSideExpression AssignmentOperator AssignmentExpression
        //
        //  ConditionalExpression :
        //    LogicalORExpression OptionalConditionalExpression
        //
        //  OptionalConditionalExpression :
        //    <empty> |
        //    '?' AssignmentExpression ':' AssignmentExpression
        //
        //  LogicalORExpression :
        //    LogicalANDExpression OptionalLogicalOrExpression
        //
        //  OptionalLogicalOrExpression :
        //    <empty> |
        //    '||' LogicalANDExpression OptionalLogicalOrExpression
        //
        //  LogicalANDExpression :
        //    BitwiseORExpression OptionalLogicalANDExpression
        //
        //  OptionalLogicalANDExpression :
        //    <empty> |
        //    '&&' BitwiseORExpression OptionalLogicalANDExpression
        //
        //  BitwiseORExpression :
        //    BitwiseXORExpression OptionalBitwiseORExpression
        //
        //  OptionalBitwiseORExpression :
        //    <empty> |
        //    '|' BitwiseXORExpression OptionalBitwiseORExpression
        //
        //  BitwiseXORExpression :
        //    BitwiseANDExpression OptionalBitwiseXORExpression
        //
        //  OptionalBitwiseXORExpression :
        //    <empty> |
        //    '^' BitwiseANDExpression OptionalBitwiseXORExpression
        //
        //  BitwiseANDExpression :
        //    EqualityExpression OptionalBitwiseANDExpression
        //
        //  OptionalBitwiseANDExpression :
        //    <empty> |
        //    '&' EqualityExpression OptionalBitwiseANDExpression
        //
        //  EqualityExpression :
        //    RelationalExpression |
        //    RelationalExpression '==' EqualityExpression |
        //    RelationalExpression '!=' EqualityExpression |
        //    RelationalExpression '===' EqualityExpression |
        //    RelationalExpression '!==' EqualityExpression
        //
        //  RelationalExpression :
        //    ShiftExpression |
        //    ShiftExpression '<' RelationalExpression |
        //    ShiftExpression '>' RelationalExpression |
        //    ShiftExpression '<=' RelationalExpression |
        //    ShiftExpression '>=' RelationalExpression
        //
        //  ShiftExpression :
        //    AdditiveExpression |
        //    AdditiveExpression '<<' ShiftExpression |
        //    AdditiveExpression '>>' ShiftExpression |
        //    AdditiveExpression '>>>' ShiftExpression
        //
        //  AdditiveExpression :
        //    MultiplicativeExpression |
        //    MultiplicativeExpression '+' AdditiveExpression |
        //    MultiplicativeExpression '-' AdditiveExpression
        //
        //  MultiplicativeExpression :
        //    UnaryExpression |
        //    UnaryExpression '*' MultiplicativeExpression |
        //    UnaryExpression '/' MultiplicativeExpression |
        //    UnaryExpression '%' MultiplicativeExpression
        //---------------------------------------------------------------------------------------
        private Expression ParseExpression()
        {
            bool bAssign;
            var lhs = ParseUnaryExpression(out bAssign, false);
            return ParseExpression(lhs, false, bAssign, JSToken.None);
        }

        private Expression ParseExpression(bool single)
        {
            bool bAssign;
            var lhs = ParseUnaryExpression(out bAssign, false);
            return ParseExpression(lhs, single, bAssign, JSToken.None);
        }

        private Expression ParseExpression(bool single, JSToken inToken)
        {
            bool bAssign;
            var lhs = ParseUnaryExpression(out bAssign, false);
            return ParseExpression(lhs, single, bAssign, inToken);
        }

        private Expression ParseExpression(Expression leftHandSide, bool single, bool bCanAssign, JSToken inToken)
        {
            // new op stack with dummy op
            Stack<JSToken> opsStack = new Stack<JSToken>();
            opsStack.Push(JSToken.None);

            // term stack, push left-hand side onto it
            Stack<Expression> termStack = new Stack<Expression>();
            termStack.Push(leftHandSide);

            Expression expr = null;

            try
            {
                for (; ; )
                {
                    // if 'binary op' or 'conditional'
                    // if we are looking for a single expression, then also bail when we hit a comma
                    // inToken is a special case because of the for..in syntax. When ParseExpression is called from
                    // for, inToken = JSToken.In which excludes JSToken.In from the list of operators, otherwise
                    // inToken = JSToken.None which is always true if the first condition is true
                    if (JSScanner.IsProcessableOperator(_curToken)
                        && inToken != _curToken
                        && (!single || _curToken != JSToken.Comma))
                    {
                        // for the current token, get the operator precedence and whether it's a right-association operator
                        var prec = JSScanner.GetOperatorPrecedence(_curToken);
                        bool rightAssoc = JSScanner.IsRightAssociativeOperator(_curToken);

                        // while the current operator has lower precedence than the operator at the top of the stack
                        // or it has the same precedence and it is left associative (that is, no 'assign op' or 'conditional')
                        var stackPrec = JSScanner.GetOperatorPrecedence(opsStack.Peek());
                        while (prec < stackPrec || prec == stackPrec && !rightAssoc)
                        {
                            // pop the top two elements off the stack along with the current operator, 
                            // combine them, then push the results back onto the term stack
                            Expression operand2 = termStack.Pop();
                            Expression operand1 = termStack.Pop();
                            expr = CreateExpressionNode(opsStack.Pop(), operand1, operand2);
                            termStack.Push(expr);

                            // get the precendence of the current item on the top of the op stack
                            stackPrec = JSScanner.GetOperatorPrecedence(opsStack.Peek());
                        }

                        // now the current operator has higher precedence that every scanned operators on the stack, or
                        // it has the same precedence as the one at the top of the stack and it is right associative
                        // push operator and next term

                        // but first: special case conditional '?:'
                        if (JSToken.ConditionalIf == _curToken)
                        {
                            // pop term stack
                            Expression condition = (Expression)termStack.Pop();

                            // if this is an assignment, throw a warning in case the developer
                            // meant to use == instead of =
                            // but no warning if the condition is wrapped in parens.
                            var binOp = condition as BinaryOperator;
                            if (binOp != null && binOp.OperatorToken == JSToken.Assign)
                            {
                                m_errorSink.HandleError(JSError.SuspectAssignment, GetSpan(condition), m_scanner.LocationResolver);
                            }

                            GetNextToken();

                            // get expr1 in logOrExpr ? expr1 : expr2
                            Expression operand1 = ParseExpression(true);

                            if (JSToken.Colon != _curToken)
                            {
                                ReportError(JSError.NoColon);
                            }

                            GetNextToken();

                            // get expr2 in logOrExpr ? expr1 : expr2
                            Expression operand2 = ParseExpression(true, inToken);

                            expr = new Conditional(EncodeSpan(GetSpan(condition).CombineWith(GetSpan(operand2))))
                                {
                                    Condition = condition,
                                    TrueExpression = operand1,
                                    FalseExpression = operand2
                                };
                            termStack.Push(expr);
                        }
                        else
                        {
                            if (JSScanner.IsAssignmentOperator(_curToken))
                            {
                                if (!bCanAssign)
                                {
                                    ReportError(JSError.IllegalAssignment);
                                    SkipTokensAndThrow();
                                }
                            }
                            else
                            {
                                // if the operator is a comma, we can get another assign; otherwise we can't
                                bCanAssign = (_curToken == JSToken.Comma);
                            }

                            // push the operator onto the operators stack
                            opsStack.Push(_curToken);

                            // push new term
                            GetNextToken();
                            if (bCanAssign)
                            {
                                termStack.Push(ParseUnaryExpression(out bCanAssign, false));
                            }
                            else
                            {
                                bool dummy;
                                termStack.Push(ParseUnaryExpression(out dummy, false));
                            }
                        }
                    }
                    else
                    {
                        // done with expression; go and unwind the stack of expressions/operators
                        break; 
                    }
                }

                // there are still operators to be processed
                while (opsStack.Peek() != JSToken.None)
                {
                    // pop the top two term and the top operator, combine them into a new term,
                    // and push the results back onto the term stack
                    Expression operand2 = termStack.Pop();
                    Expression operand1 = termStack.Pop();
                    expr = CreateExpressionNode(opsStack.Pop(), operand1, operand2);

                    // push node onto the stack
                    termStack.Push(expr);
                }

                Debug.Assert(termStack.Count == 1);
                return (Expression)termStack.Pop();
            }
            catch (RecoveryTokenException exc)
            {
                exc._partiallyComputedNode = leftHandSide;
                throw;
            }
        }

        //---------------------------------------------------------------------------------------
        // ParseUnaryExpression
        //
        //  UnaryExpression :
        //    PostfixExpression |
        //    'delete' UnaryExpression |
        //    'void' UnaryExpression |
        //    'typeof' UnaryExpression |
        //    '++' UnaryExpression |
        //    '--' UnaryExpression |
        //    '+' UnaryExpression |
        //    '-' UnaryExpression |
        //    '~' UnaryExpression |
        //    '!' UnaryExpression
        //
        //---------------------------------------------------------------------------------------
        private Expression ParseUnaryExpression(out bool isLeftHandSideExpr, bool isMinus)
        {
            isLeftHandSideExpr = false;
            bool dummy = false;
            IndexSpan exprSpan = _curSpan;
            Expression expr = null;

            string doclet = _curDoclet;

            Expression ast = null;
            switch (_curToken)
            {
                case JSToken.Yield:
                    if (_inGenerator) {
                        // yield is only valid in a generator function...
                        ast = ParseYieldExpression();
                        break;
                    }
                    goto default;
                case JSToken.Void:
                case JSToken.TypeOf:
                case JSToken.Plus:
                case JSToken.Minus:
                case JSToken.BitwiseNot:
                case JSToken.LogicalNot:
                case JSToken.Delete:
                case JSToken.Increment:
                case JSToken.Decrement:
                    var token = _curToken;

                    GetNextToken();
                    expr = ParseUnaryExpression(out dummy, false);
                    ast = new UnaryOperator(EncodeSpan(exprSpan.UpdateWith(GetSpan(expr))))
                        {
                            Operand = expr,
                            OperatorToken = token
                        };
                    break;
                default:
                    m_noSkipTokenSet.Add(NoSkipTokenSet.s_PostfixExpressionNoSkipTokenSet);
                    try
                    {
                        ast = ParseLeftHandSideExpression(isMinus);
                    }
                    catch (RecoveryTokenException exc)
                    {
                        if (IndexOfToken(NoSkipTokenSet.s_PostfixExpressionNoSkipTokenSet, exc) == -1)
                        {
                            throw;
                        }
                        else
                        {
                            if (exc._partiallyComputedNode == null)
                                SkipTokensAndThrow();
                            else
                                ast = (Expression)exc._partiallyComputedNode;
                        }
                    }
                    finally
                    {
                        m_noSkipTokenSet.Remove(NoSkipTokenSet.s_PostfixExpressionNoSkipTokenSet);
                    }
                    ast = ParsePostfixExpression(ast, out isLeftHandSideExpr);
                    break;
            }

            if (doclet != null) {
                _doclets[ast] = doclet;
            }

            return ast;
        }

        //---------------------------------------------------------------------------------------
        // ParsePostfixExpression
        //
        //  PostfixExpression:
        //    LeftHandSideExpression |
        //    LeftHandSideExpression '++' |
        //    LeftHandSideExpression  '--'
        //
        //---------------------------------------------------------------------------------------
        private Expression ParsePostfixExpression(Expression ast, out bool isLeftHandSideExpr)
        {
            isLeftHandSideExpr = true;
            IndexSpan exprSpan;
            if (null != ast)
            {
                if (!m_foundEndOfLine)
                {
                    if (JSToken.Increment == _curToken || JSToken.Decrement == _curToken)
                    {
                        isLeftHandSideExpr = false;
                        exprSpan = GetSpan(ast);
                        exprSpan = exprSpan.UpdateWith(_curSpan);
                        ast = new UnaryOperator(EncodeSpan(exprSpan))
                            {
                                Operand = ast,
                                OperatorToken = _curToken,
                                IsPostfix = true
                            };
                        GetNextToken();
                    }
                }
            }
            return ast;
        }

        //---------------------------------------------------------------------------------------
        // ParseLeftHandSideExpression
        //
        //  LeftHandSideExpression :
        //    PrimaryExpression Accessor  |
        //    'new' LeftHandSideExpression |
        //    FunctionExpression
        //
        //  PrimaryExpression :
        //    'this' |
        //    Identifier |
        //    Literal |
        //    '(' Expression ')'
        //
        //  FunctionExpression :
        //    'function' OptionalFuncName '(' FormalParameterList ')' { FunctionBody }
        //
        //  OptionalFuncName :
        //    <empty> |
        //    Identifier
        //---------------------------------------------------------------------------------------
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Expression ParseLeftHandSideExpression(bool isMinus)
        {
            Expression ast = null;
            bool skipToken = true;
            List<IndexSpan> newSpans = null;

            // new expression
            while (JSToken.New == _curToken)
            {
                if (null == newSpans)
                    newSpans = new List<IndexSpan>(4);
                newSpans.Add(_curSpan);
                GetNextToken();
            }
            JSToken token = _curToken;
            switch (token)
            {
                // primary expression
                case JSToken.Identifier:
                    ast = new Lookup(EncodeCurrentSpan())
                        {
                            Name = m_scanner.Identifier
                        };
                    break;

                case JSToken.This:
                    ast = new ThisLiteral(EncodeCurrentSpan());
                    break;

                case JSToken.StringLiteral:
                    ast = new ConstantWrapper(m_scanner.StringLiteralValue, EncodeCurrentSpan());
                    break;

                case JSToken.IntegerLiteral:
                case JSToken.NumericLiteral:
                    {
                        IndexSpan numericSpan = _curSpan;
                        double doubleValue;
                        if (ConvertNumericLiteralToDouble(GetCode(_curSpan), (token == JSToken.IntegerLiteral), out doubleValue))
                        {
                            // conversion worked fine
                            // check for some boundary conditions
                            if (doubleValue == double.MaxValue)
                            {
                                ReportError(JSError.NumericMaximum, numericSpan, true);
                            }
                            else if (isMinus && -doubleValue == double.MinValue)
                            {
                                ReportError(JSError.NumericMinimum, numericSpan, true);
                            }

                            // create the constant wrapper from the value
                            ast = new ConstantWrapper(BoxDouble(doubleValue), EncodeSpan(numericSpan));
                        }
                        else
                        {
                            // check to see if we went overflow
                            if (double.IsInfinity(doubleValue))
                            {
                                ReportError(JSError.NumericOverflow, numericSpan, true);
                            }

                            // regardless, we're going to create a special constant wrapper
                            // that simply echos the input as-is
                            ast = new ConstantWrapper(new InvalidNumericErrorValue(GetCode(_curSpan)), EncodeSpan(numericSpan));
                        }
                        break;
                    }

                case JSToken.True:
                    ast = new ConstantWrapper(True, EncodeCurrentSpan());
                    break;

                case JSToken.False:
                    ast = new ConstantWrapper(False, EncodeCurrentSpan());
                    break;

                case JSToken.Null:
                    ast = new ConstantWrapper(null, EncodeCurrentSpan());
                    break;

                case JSToken.DivideAssign:
                // normally this token is not allowed on the left-hand side of an expression.
                // BUT, this might be the start of a regular expression that begins with an equals sign!
                // we need to test to see if we can parse a regular expression, and if not, THEN
                // we can fail the parse.

                case JSToken.Divide:
                    // could it be a regexp?
                    var start = _curSpan;
                    String source = m_scanner.ScanRegExp();
                    if (source != null)
                    {
                        // parse the flags (if any)
                        String flags = m_scanner.ScanRegExpFlags();
                        // create the literal
                        var regExpSpan = start.UpdateWith(
                            m_scanner.CurrentSpan
                        );
                        ast = new RegExpLiteral(EncodeSpan(regExpSpan))
                            {
                                Pattern = source,
                                PatternSwitches = flags
                            };
                        break;
                    }
                    goto default;

                // expression
                case JSToken.LeftParenthesis:
                    {
                        var groupingOp = new GroupingOperator(EncodeCurrentSpan());
                        ast = groupingOp;
                        GetNextToken();

                        if (_curToken == JSToken.RightParenthesis) {
                            // empty bodied grouping operator statement
                            groupingOp.Operand = null;
                            UpdateWithCurrentSpan(ast);
                        }

                        m_noSkipTokenSet.Add(NoSkipTokenSet.s_ParenExpressionNoSkipToken);
                        try
                        {
                            // parse an expression
                            groupingOp.Operand = ParseExpression();
                            if (JSToken.RightParenthesis != _curToken)
                            {
                                ReportError(JSError.NoRightParenthesis);
                            }
                            else
                            {
                                // add the closing paren to the expression context
                                UpdateWithCurrentSpan(ast);
                            }
                        }
                        catch (RecoveryTokenException exc)
                        {
                            if (IndexOfToken(NoSkipTokenSet.s_ParenExpressionNoSkipToken, exc) == -1)
                                throw;
                            else
                                groupingOp.Operand = (Expression)exc._partiallyComputedNode;
                        }
                        finally
                        {
                            m_noSkipTokenSet.Remove(NoSkipTokenSet.s_ParenExpressionNoSkipToken);
                        }
                    }
                    break;

                // array initializer
                case JSToken.LeftBracket:
                    IndexSpan listSpan = _curSpan;
                    GetNextToken();
                    List<Expression> list = new List<Expression>();
                    while (JSToken.RightBracket != _curToken)
                    {
                        if (JSToken.Comma != _curToken)
                        {
                            m_noSkipTokenSet.Add(NoSkipTokenSet.s_ArrayInitNoSkipTokenSet);
                            try
                            {
                                var expression = ParseExpression(true);
                                list.Add(expression);
                                if (JSToken.Comma != _curToken)
                                {
                                    if (JSToken.RightBracket != _curToken)
                                    {
                                        ReportError(JSError.NoRightBracket, true);
                                        SkipToNextToken(NoSkipTokenSet.s_ArrayInitNoSkipTokenSet);
                                    }

                                    break;
                                }
                                else
                                {
                                    // we have a comma -- skip it after adding it as a terminator
                                    // on the previous expression
                                    GetNextToken();
                                }
                            }
                            catch (RecoveryTokenException exc)
                            {
                                if (exc._partiallyComputedNode != null)
                                    list.Add((Expression)exc._partiallyComputedNode);
                                if (IndexOfToken(NoSkipTokenSet.s_ArrayInitNoSkipTokenSet, exc) == -1)
                                {
                                    listSpan = listSpan.UpdateWith(CurrentPositionSpan());
                                    exc._partiallyComputedNode = new ArrayLiteral(EncodeSpan(listSpan))
                                        {
                                            Elements = ToArray(list)
                                        };
                                    throw;
                                }
                                else
                                {
                                    if (JSToken.RightBracket == _curToken)
                                        break;
                                }
                            }
                            finally
                            {
                                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_ArrayInitNoSkipTokenSet);
                            }
                        }
                        else
                        {
                            // comma -- missing array item in the list
                            list.Add(new ConstantWrapper(Missing.Value, EncodeCurrentSpan()));

                            // skip over the comma
                            GetNextToken();

                            // if the next token is the closing brace, then we end with a comma -- and we need to
                            // add ANOTHER missing value to make sure this last comma doesn't get left off.
                            // TECHNICALLY, that puts an extra item into the array for most modern browsers, but not ALL.
                            if (_curToken == JSToken.RightBracket)
                            {
                                list.Add(new ConstantWrapper(Missing.Value, EncodeCurrentSpan()));
                            }
                        }
                    }
                    listSpan = listSpan.UpdateWith(_curSpan);
                    ast = new ArrayLiteral(EncodeSpan(listSpan))
                        {
                            Elements = ToArray(list)
                        };
                    break;

                // object initializer
                case JSToken.LeftCurly:
                    ast = ParseObjectLiteral();
                    break;

                // function expression
                case JSToken.Function:
                    var fExpr = ParseFunctionExpression();
                    ast = fExpr;
                    skipToken = false;
                    break;

                default:
                    string identifier = CanBeIdentifier(_curToken);
                    if (null != identifier)
                    {
                        ast = new Lookup(EncodeCurrentSpan())
                            {
                                Name = identifier
                            };
                    }
                    else
                    {
                        ReportError(JSError.ExpressionExpected);
                        SkipTokensAndThrow();
                    }
                    break;
            }

            // can be a CallNode, that is, followed by '.' or '(' or '['
            if (skipToken)
                GetNextToken();

            return MemberExpression(ast, newSpans);
        }

        private Expression ParseObjectLiteral() {
            IndexSpan objSpan = _curSpan;
            GetNextToken();

            var propertyList = new List<ObjectLiteralProperty>();

            if (JSToken.RightCurly != _curToken) {
                try {
                    for (; ; ) {
                        ObjectLiteralField field = null;
                        Expression value = null;
                        bool getterSetter = false;
                        string ident;

                        switch (_curToken) {
                            case JSToken.Identifier:
                                field = new ObjectLiteralField(m_scanner.Identifier, EncodeCurrentSpan());
                                break;

                            case JSToken.StringLiteral:
                                field = new ObjectLiteralField(m_scanner.StringLiteralValue, EncodeCurrentSpan());
                                break;

                            case JSToken.IntegerLiteral:
                            case JSToken.NumericLiteral: {
                                    double doubleValue;
                                    if (ConvertNumericLiteralToDouble(GetCode(_curSpan), (_curToken == JSToken.IntegerLiteral), out doubleValue)) {
                                        // conversion worked fine
                                        field = new ObjectLiteralField(
                                          doubleValue,
                                          EncodeCurrentSpan()
                                        );
                                    } else {
                                        // something went wrong and we're not sure the string representation in the source is 
                                        // going to convert to a numeric value well
                                        if (double.IsInfinity(doubleValue)) {
                                            ReportError(JSError.NumericOverflow, _curSpan, true);
                                        }

                                        // use the source as the field name, not the numeric value
                                        field = new ObjectLiteralField(
                                            new InvalidNumericErrorValue(GetCode(_curSpan)),
                                            EncodeCurrentSpan()
                                        );
                                    }
                                    break;
                                }

                            case JSToken.Get:
                            case JSToken.Set:
                                if (PeekToken() == JSToken.Colon) {
                                    // the field is either "get" or "set" and isn't the special Mozilla getter/setter
                                    field = new ObjectLiteralField(GetCode(_curSpan), EncodeCurrentSpan());
                                } else {
                                    // ecma-script get/set property construct
                                    getterSetter = true;
                                    bool isGet = (_curToken == JSToken.Get);
                                    var span = _curSpan;
                                    var functionExpr = ParseFunctionExpression(JSToken.Get == _curToken ? FunctionType.Getter : FunctionType.Setter);
                                    value = functionExpr;
                                    // getter/setter is just the literal name with a get/set flag
                                    field = new GetterSetter(
                                        functionExpr.Function.Name,
                                        isGet,
                                        EncodeSpan(span),
                                        this
                                    );
                                }
                                break;

                            default:
                                // NOT: identifier token, string, number, or getter/setter.
                                // see if it's a token that COULD be an identifierName.
                                ident = m_scanner.Identifier;
                                if (JSScanner.IsValidIdentifier(ident)) {
                                    // BY THE SPEC, if it's a valid identifierName -- which includes reserved words -- then it's
                                    // okay for object literal syntax. However, reserved words here won't work in all browsers,
                                    // so if it is a reserved word, let's throw a low-sev cross-browser warning on the code.
                                    if (CanBeIdentifier(_curToken) == null) {
                                        ReportError(JSError.ObjectLiteralKeyword, _curSpan, true);
                                    }

                                    field = new ObjectLiteralField(ident, EncodeCurrentSpan());
                                } else {
                                    // throw an error but use it anyway, since that's what the developer has going on
                                    ReportError(JSError.NoMemberIdentifier, _curSpan, true);
                                    field = new ObjectLiteralField(GetCode(_curSpan), EncodeCurrentSpan());
                                }
                                break;
                        }

                        if (field != null) {
                            if (!getterSetter) {
                                GetNextToken();
                            }

                            m_noSkipTokenSet.Add(NoSkipTokenSet.s_ObjectInitNoSkipTokenSet);
                            try {
                                if (!getterSetter) {
                                    // get the value
                                    if (JSToken.Colon != _curToken) {
                                        ReportError(JSError.NoColon, true);
                                        value = ParseExpression(true);
                                    } else {
                                        GetNextToken();
                                        value = ParseExpression(true);
                                    }
                                }

                                // put the pair into the list of fields
                                
                                var property = new ObjectLiteralProperty(CombineWithOtherSpan(field.EncodedSpan, value.EncodedSpan)) {
                                    Name = field,
                                    Value = value
                                };

                                FunctionExpression functionExpr = value as FunctionExpression;
                                if (functionExpr != null) {
                                    string fieldName = field.Value as string;
                                    if (fieldName != null) {
                                        functionExpr.Function.NameGuess = fieldName;
                                    }
                                }

                                propertyList.Add(property);

                                if (JSToken.RightCurly == _curToken) {
                                    break;
                                } else {
                                    if (JSToken.Comma == _curToken) {
                                        // skip the comma after adding it to the property as a terminating context
                                        GetNextToken();

                                        // if the next token is the right-curly brace, then we ended 
                                        // the list with a comma, which is perfectly fine
                                        if (_curToken == JSToken.RightCurly) {
                                            break;
                                        }
                                    } else {
                                        if (m_foundEndOfLine) {
                                            ReportError(JSError.NoRightCurly);
                                        } else
                                            ReportError(JSError.NoComma, true);
                                        SkipTokensAndThrow();
                                    }
                                }
                            } catch (RecoveryTokenException exc) {
                                if (exc._partiallyComputedNode != null) {
                                    // the problem was in ParseExpression trying to determine value
                                    value = (Expression)exc._partiallyComputedNode;

                                    var property = new ObjectLiteralProperty(CombineWithOtherSpan(field.EncodedSpan, value.EncodedSpan)) {
                                        Name = field,
                                        Value = value
                                    };

                                    propertyList.Add(property);
                                }

                                if (IndexOfToken(NoSkipTokenSet.s_ObjectInitNoSkipTokenSet, exc) == -1) {
                                    exc._partiallyComputedNode = new ObjectLiteral(EncodeSpan(objSpan)) {
                                        Properties = ToArray(propertyList)
                                    };
                                    throw;
                                } else {
                                    if (JSToken.Comma == _curToken)
                                        GetNextToken();
                                    if (JSToken.RightCurly == _curToken)
                                        break;
                                }
                            } finally {
                                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_ObjectInitNoSkipTokenSet);
                            }
                        }
                    }
                } catch (EndOfFileException) {
                    m_errorSink.HandleError(JSError.UnclosedObjectLiteral, objSpan, m_scanner._locationResolver, true);
                }
            }
            objSpan = objSpan.UpdateWith(_curSpan);
            return new ObjectLiteral(EncodeSpan(objSpan)) {
                Properties = ToArray(propertyList)
            };
        }

        private static object[] _cachedDoubles = new object[200];
        private const int _smallestNegative = 2;

        internal static object BoxDouble(double doubleValue) {
            if ((doubleValue % 1) == 0 &&
                doubleValue > -_smallestNegative &&
                doubleValue < _cachedDoubles.Length - (_smallestNegative + 1)) {
                int index = (int)doubleValue + _smallestNegative;
                object value;
                if ((value = _cachedDoubles[index]) == null) {
                    value = _cachedDoubles[index] = (object)doubleValue;
                }
                return value;
            }
            return doubleValue;
        }

        private FunctionExpression ParseFunctionExpression(FunctionType functionType = FunctionType.Expression)
        {
            var span = _curSpan;
            string doclet = _curDoclet;
            try {
                var function = ParseFunction(functionType, _curSpan);
                function.Doclet = doclet;
                var functionExpr = new FunctionExpression(function.EncodedSpan);
                functionExpr.Function = function;
                return functionExpr;
            } catch (RecoveryTokenException exc) {
                if (exc._partiallyComputedNode is FunctionObject) {
                    var functionExpr = new FunctionExpression(((FunctionObject)exc._partiallyComputedNode).EncodedSpan);
                    functionExpr.Function = (FunctionObject)exc._partiallyComputedNode;
                    return functionExpr;
                }
                throw;
            }
        }

        /// <summary>
        /// Convert the given numeric string to a double value
        /// </summary>
        /// <param name="str">string representation of a number</param>
        /// <param name="isInteger">we should know alreasdy if it's an integer or not</param>
        /// <param name="doubleValue">output value</param>
        /// <returns>true if there were no problems; false if there were</returns>
        private bool ConvertNumericLiteralToDouble(string str, bool isInteger, out double doubleValue)
        {
            try
            {
                if (isInteger)
                {
                    if (str[0] == '0' && str.Length > 1)
                    {
                        if (str[1] == 'x' || str[1] == 'X')
                        {
                            if (str.Length == 2)
                            {
                                // 0x???? must be a parse error. Just return zero
                                doubleValue = 0;
                                return false;
                            }

                            // parse the number as a hex integer, converted to a double
                            doubleValue = (double)System.Convert.ToInt64(str, 16);
                        }
                        else
                        {
                            // might be an octal value... try converting to octal
                            // and if it fails, just convert to decimal
                            try
                            {
                                doubleValue = (double)System.Convert.ToInt64(str, 8);

                                // if we got here, we successfully converted it to octal.
                                // now, octal literals are deprecated -- not all JS implementations will
                                // decode them. If this decoded as an octal, it can also be a decimal. Check
                                // the decimal value, and if it's the same, then we'll just treat it
                                // as a normal decimal value. Otherwise we'll throw a warning and treat it
                                // as a special no-convert literal.
                                double decimalValue = (double)System.Convert.ToInt64(str, 10);
                                if (decimalValue != doubleValue)
                                {
                                    // throw a warning!
                                    ReportError(JSError.OctalLiteralsDeprecated, _curSpan, true);

                                    return true;
                                }
                            }
                            catch (FormatException)
                            {
                                // ignore the format exception and fall through to parsing
                                // the value as a base-10 decimal value
                                doubleValue = Convert.ToDouble(str, CultureInfo.InvariantCulture);
                            }
                        }
                    }
                    else
                    {
                        // just parse the integer as a decimal value
                        doubleValue = Convert.ToDouble(str, CultureInfo.InvariantCulture);
                    }

                    // check for out-of-bounds integer values -- if the integer can't EXACTLY be represented
                    // as a double, then we don't want to consider it "successful"
                    if (doubleValue < -0x20000000000000 || 0x20000000000000 < doubleValue)
                    {
                        return false;
                    }
                }
                else
                {
                    // use the system to convert the string to a double
                    doubleValue = Convert.ToDouble(str, CultureInfo.InvariantCulture);
                }

                // if we got here, we should have an appropriate value in doubleValue
                return true;
            }
            catch (OverflowException)
            {
                // overflow mean just return one of the infinity values
                doubleValue = (str[0] == '-'
                  ? Double.NegativeInfinity
                  : Double.PositiveInfinity
                  );

                // and it wasn't "successful"
                return false;
            }
            catch (FormatException)
            {
                // format exception converts to NaN
                doubleValue = double.NaN;

                // not successful
                return false;
            }
        }

        //---------------------------------------------------------------------------------------
        // MemberExpression
        //
        // Accessor :
        //  <empty> |
        //  Arguments Accessor
        //  '[' Expression ']' Accessor |
        //  '.' Identifier Accessor |
        //
        //  Don't have this function throwing an exception without checking all the calling sites.
        //  There is state in instance variable that is saved on the calling stack in some function
        //  (i.e ParseFunction and ParseClass) and you don't want to blow up the stack
        //---------------------------------------------------------------------------------------
        private Expression MemberExpression(Expression expression, List<IndexSpan> newSpans)
        {
            for (; ; )
            {
                m_noSkipTokenSet.Add(NoSkipTokenSet.s_MemberExprNoSkipTokenSet);
                try
                {
                    switch (_curToken)
                    {
                        case JSToken.LeftParenthesis:
                            List<Expression> args = null;
                            RecoveryTokenException callError;
                            m_noSkipTokenSet.Add(NoSkipTokenSet.s_ParenToken);
                            try
                            {
                                args = ParseExpressionList(JSToken.RightParenthesis, out callError);
                                if (callError != null && IndexOfToken(NoSkipTokenSet.s_ParenToken, callError) != -1) {
                                    callError = null;
                                }
                            }
                            finally
                            {
                                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_ParenToken);
                            }

                            EncodedSpan callSpan = expression.EncodedSpan;                          
                            if (!m_useCurrentForNext) {
                                // If we said don't use current for next, we can just combine 
                                // with the current span. For instance, if there is already a right paren.
                                callSpan = CombineWithCurrentSpan(callSpan);
                            } else if (args.Count > 0) {
                                // If we have args left after parsing the expression list, append to the last arg span.
                                callSpan = CombineWithOtherSpan(
                                    callSpan,
                                    args[args.Count - 1].EncodedSpan
                                );
                            }

                            expression = new CallNode(callSpan)
                                {
                                    Function = expression,
                                    Arguments = ToArray(args),
                                    InBrackets = false
                                };

                            if (null != newSpans && newSpans.Count > 0)
                            {                                
                                newSpans[newSpans.Count - 1] = newSpans[newSpans.Count - 1].UpdateWith(GetSpan(expression));
                                expression.EncodedSpan = new EncodedSpan(m_scanner.LocationResolver, newSpans[newSpans.Count - 1]);

                                ((CallNode)expression).IsConstructor = true;
                                newSpans.RemoveAt(newSpans.Count - 1);
                            }

                            if (callError != null)
                            {
                                callError._partiallyComputedNode = expression;
                                throw callError;
                            }

                            GetNextToken();
                            break;

                        case JSToken.LeftBracket:
                            m_noSkipTokenSet.Add(NoSkipTokenSet.s_BracketToken);
                            args = new List<Expression>();
                            try
                            {
                                //
                                // ROTOR parses a[b,c] as a call to a, passing in the arguments b and c.
                                // the correct parse is a member lookup on a of c -- the "b,c" should be
                                // a single expression with a comma operator that evaluates b but only
                                // returns c.
                                // So we'll change the default behavior from parsing an expression list to
                                // parsing a single expression, but returning a single-item list (or an empty
                                // list if there is no expression) so the rest of the code will work.
                                //
                                //args = ParseExpressionList(JSToken.RightBracket);
                                GetNextToken();

                                Expression accessor = ParseExpression();
                                if (accessor != null)
                                {
                                    args.Add(accessor);
                                }
                            }
                            catch (RecoveryTokenException exc)
                            {
                                if (IndexOfToken(NoSkipTokenSet.s_BracketToken, exc) == -1) {
                                    if (exc._partiallyComputedNode != null) {
                                        args.Add((Expression)exc._partiallyComputedNode);
                                        exc._partiallyComputedNode =
                                           new CallNode(CombineWithCurrentSpan(expression.EncodedSpan)) {
                                               Function = expression,
                                               Arguments = ToArray(args),
                                               InBrackets = true
                                           };
                                    } else {
                                        exc._partiallyComputedNode = expression;
                                    }
                                    throw;
                                } else {
                                    if (exc._partiallyComputedNode != null) {
                                        args.Add((Expression)exc._partiallyComputedNode);
                                    }
                                }
                            }
                            finally
                            {
                                m_noSkipTokenSet.Remove(NoSkipTokenSet.s_BracketToken);
                            }
                            expression = new CallNode(CombineWithCurrentSpan(expression.EncodedSpan))
                                {
                                    Function = expression,
                                    Arguments = ToArray(args),
                                    InBrackets = true
                                };

                            // there originally was code here in the ROTOR sources that checked the new context list and
                            // changed this member call to a constructor call, effectively combining the two. I believe they
                            // need to remain separate.

                            // remove the close bracket token
                            GetNextToken();
                            break;

                        case JSToken.AccessField:
                            ConstantWrapper id = null;
                            IndexSpan nameSpan = _curSpan;
                            GetNextToken();
                            if (JSToken.Identifier != _curToken)
                            {
                                string identifier = CanBeIdentifier(_curToken);
                                if (null != identifier)
                                {
                                    // don't report an error here -- it's actually okay to have a property name
                                    // that is a keyword which is okay to be an identifier. For instance,
                                    // jQuery has a commonly-used method named "get" to make an ajax request
                                    //ForceReportInfo(JSError.KeywordUsedAsIdentifier);
                                    id = new ConstantWrapper(identifier, EncodeCurrentSpan());
                                }
                                else if (JSScanner.IsValidIdentifier(GetCode(_curSpan)))
                                {
                                    // it must be a keyword, because it can't technically be an identifier,
                                    // but it IS a valid identifier format. Throw a warning but still
                                    // create the constant wrapper so we can output it as-is
                                    ReportError(JSError.KeywordUsedAsIdentifier, _curSpan, true);
                                    id = new ConstantWrapper(GetCode(_curSpan), EncodeCurrentSpan());
                                }
                                else
                                {
                                    ReportError(JSError.NoIdentifier);
                                    SkipTokensAndThrow(expression);
                                }
                            }
                            else
                            {
                                id = new ConstantWrapper(m_scanner.Identifier, EncodeCurrentSpan());
                            }
                            GetNextToken();
                            var idSpan = GetSpan(id);
                            expression = new Member(EncodeSpan(GetSpan(expression).CombineWith(idSpan)))
                                {
                                    Root = expression,
                                    Name = GetCode(idSpan),
                                    NameSpan = nameSpan.CombineWith(idSpan)
                                };
                            break;
                        default:
                            if (null != newSpans)
                            {
                                while (newSpans.Count > 0)
                                {
                                    newSpans[newSpans.Count - 1] = newSpans[newSpans.Count - 1].UpdateWith(GetSpan(expression));
                                    expression = new CallNode(EncodeSpan(newSpans[newSpans.Count - 1]))
                                        {
                                            Function = expression,
                                            Arguments = Expression.EmptyArray
                                        };
                                    ((CallNode)expression).IsConstructor = true;
                                    newSpans.RemoveAt(newSpans.Count - 1);
                                }
                            }
                            if (_curToken == JSToken.NumericLiteral) {
                                // This is an error case.  We shouldn't be here with numeric literals.
                                // The code likely resembles x.42 which is invalid
                                ReportError(JSError.NoIdentifier, true);
                                SkipTokensAndThrow(expression);
                            }
                            return expression;
                    }
                }
                catch (RecoveryTokenException exc)
                {
                    if (IndexOfToken(NoSkipTokenSet.s_MemberExprNoSkipTokenSet, exc) != -1) {
                        expression = (Expression)exc._partiallyComputedNode;
                    } else {
                        throw;
                    }
                }
                finally
                {
                    m_noSkipTokenSet.Remove(NoSkipTokenSet.s_MemberExprNoSkipTokenSet);
                }
            }
        }

        //---------------------------------------------------------------------------------------
        // ParseExpressionList
        //
        //  Given a starting this.currentToken '(' or '[', parse a list of expression separated by
        //  ',' until matching ')' or ']'
        //---------------------------------------------------------------------------------------
        private List<Expression> ParseExpressionList(JSToken terminator, out RecoveryTokenException exception)
        {
            exception = null;
            IndexSpan listSpan = _curSpan;
            GetNextToken();
            var list = new List<Expression>();
            try {
                if (terminator != _curToken) {
                    for (; ; ) {
                        m_noSkipTokenSet.Add(NoSkipTokenSet.s_ExpressionListNoSkipTokenSet);
                        try {
                            Expression item;
                            if (JSToken.Comma == _curToken) {
                                item = new ConstantWrapper(Missing.Value, EncodeCurrentSpan());
                                list.Add(item);
                            } else if (terminator == _curToken) {
                                break;
                            } else {
                                item = ParseExpression(true);
                                list.Add(item);
                            }

                            if (terminator == _curToken) {
                                break;
                            } else if (JSToken.Comma != _curToken) {
                                ReportError(JSError.NoRightBracketOrComma);

                                SkipTokensAndThrow();
                            }
                        } catch (RecoveryTokenException exc) {
                            if (exc._partiallyComputedNode != null)
                                list.Add((Expression)exc._partiallyComputedNode);
                            if (IndexOfToken(NoSkipTokenSet.s_ExpressionListNoSkipTokenSet, exc) == -1) { 
                                exception = exc;
                                break;
                            }
                        } finally {
                            m_noSkipTokenSet.Remove(NoSkipTokenSet.s_ExpressionListNoSkipTokenSet);
                        }
                        GetNextToken();
                    }
                }
            } catch (EndOfFileException) {
                // return the partially completed list...
            }
            return list;
        }

        //---------------------------------------------------------------------------------------
        // CreateExpressionNode
        //
        //  Create the proper AST object according to operator
        //---------------------------------------------------------------------------------------
        private Expression CreateExpressionNode(JSToken token, Expression operand1, Expression operand2)
        {
            IndexSpan span = GetSpan(operand1).CombineWith(GetSpan(operand2));
            switch (token)
            {
                case JSToken.Assign: {
                        var funcExpr = operand2 as FunctionExpression;
                        if (funcExpr != null) {
                            var member = operand1 as Member;
                            if (member != null) {
                                funcExpr.Function.NameGuess = member.Name;
                            }

                            var lookup = operand1 as Lookup;
                            if (lookup != null) {
                                funcExpr.Function.NameGuess = lookup.Name;
                    }
                    
                            if (funcExpr.Function.Doclet == null) {
                                string doclet;
                                if (_doclets.TryGetValue(operand1, out doclet)) {
                                    funcExpr.Function.Doclet = doclet;
                                }
                            }
                    }

                    goto case JSToken.BitwiseAnd;
                    }
                case JSToken.BitwiseAnd:
                case JSToken.BitwiseAndAssign:
                case JSToken.BitwiseOr:
                case JSToken.BitwiseOrAssign:
                case JSToken.BitwiseXor:
                case JSToken.BitwiseXorAssign:
                case JSToken.Divide:
                case JSToken.DivideAssign:
                case JSToken.Equal:
                case JSToken.GreaterThan:
                case JSToken.GreaterThanEqual:
                case JSToken.In:
                case JSToken.InstanceOf:
                case JSToken.LeftShift:
                case JSToken.LeftShiftAssign:
                case JSToken.LessThan:
                case JSToken.LessThanEqual:
                case JSToken.LogicalAnd:
                case JSToken.LogicalOr:
                case JSToken.Minus:
                case JSToken.MinusAssign:
                case JSToken.Modulo:
                case JSToken.ModuloAssign:
                case JSToken.Multiply:
                case JSToken.MultiplyAssign:
                case JSToken.NotEqual:
                case JSToken.Plus:
                case JSToken.PlusAssign:
                case JSToken.RightShift:
                case JSToken.RightShiftAssign:
                case JSToken.StrictEqual:
                case JSToken.StrictNotEqual:
                case JSToken.UnsignedRightShift:
                case JSToken.UnsignedRightShiftAssign: {
                    // regular binary operator
                        var result = new BinaryOperator(EncodeSpan(span)) {
                            Operand1 = operand1,
                            Operand2 = operand2,
                            OperatorToken = token,
                        };

                        // propagate doclet from LHS to the entire expression
                        string doclet;
                        if (_doclets.TryGetValue(operand1, out doclet)) {
                            _doclets[result] = doclet;
                        }

                        return result;
                    }

                case JSToken.Comma:
                    // use the special comma-operator class derived from binary operator.
                    // it has special logic to combine adjacent comma operators into a single
                    // node with an ast node list rather than nested binary operators
                    return CommaOperator.CombineWithComma(span, this, operand1, operand2);

                default:
                    // shouldn't get here!
                    Debug.Assert(false);
                    return null;
            }
        }

        //---------------------------------------------------------------------------------------
        // GetNextToken
        //
        //  Return the next token or peeked token if this.errorToken is not null.
        //  Usually this.errorToken is set by AddError even though any code can look ahead
        //  by assigning this.errorToken.
        //  At this point the context is not saved so if position information is needed
        //  they have to be saved explicitely
        //---------------------------------------------------------------------------------------
        private void GetNextToken()
        {
            JSToken curToken;
            string curDoclet;

            if (m_useCurrentForNext)
            {
                // we just want to keep using the current token.
                // but don't get into an infinite loop -- after a while,
                // give up and grab the next token from the scanner anyway
                m_useCurrentForNext = false;
                if (m_breakRecursion++ > 10) {
                    curToken = ScanNextToken(out curDoclet);
                } else {
                    return;
                }
            }
            else
            {
                m_goodTokensProcessed++;
                m_breakRecursion = 0;

                // the scanner reuses the same context object for performance,
                // so if we ever mean to hold onto it later, we need to clone it.
                curToken = ScanNextToken(out curDoclet);
            }

            _curToken = curToken;
            _curSpan = m_scanner.CurrentSpan;
            _curDoclet = curDoclet;
        }

        private JSToken ScanNextToken(out string doclet)
        {
            m_foundEndOfLine = false;
            doclet = null;

            var nextToken = m_scanner.ScanNextToken(false);
            while (nextToken == JSToken.WhiteSpace
                || nextToken == JSToken.EndOfLine
                || nextToken == JSToken.SingleLineComment
                || nextToken == JSToken.MultipleLineComment
                || nextToken == JSToken.Error)
            {
                if (nextToken == JSToken.EndOfLine)
                {
                    m_foundEndOfLine = true;
                }

                if (m_scanner.IsDoclet) {
                    doclet = m_scanner.Identifier;
                }

                if (m_scanner.UnterminatedComment) {
                    ReportError(JSError.NoCommentEnd);
                }
                nextToken = m_scanner.ScanNextToken(false);
            }

            if (nextToken == JSToken.EndOfFile)
            {
                m_foundEndOfLine = true;
            }

            return nextToken;
        }

        private JSToken PeekToken()
        {
            // clone the scanner and get the next token
            var clonedScanner = m_scanner.Clone();
            var peekToken = clonedScanner.ScanNextTokenWithSpan(false);

            // there are some tokens we really don't care about when we peek
            // for the next token
            while (peekToken.Token == JSToken.WhiteSpace
                || peekToken.Token == JSToken.EndOfLine
                || peekToken.Token == JSToken.Error
                || peekToken.Token == JSToken.SingleLineComment
                || peekToken.Token == JSToken.MultipleLineComment
                || peekToken.Token == JSToken.ConditionalIf)
            {
                peekToken = clonedScanner.ScanNextTokenWithSpan(false);
            }

            // return the token type
            return peekToken.Token;
        }

        private IndexSpan CurrentPositionSpan() {
            return _curSpan.FlattenToStart();
        }

        private EncodedSpan CurrentPositionEncodedSpan() {
            return EncodeSpan(_curSpan.FlattenToStart());
        }

        //---------------------------------------------------------------------------------------
        // ReportError
        //
        //  Generate a parser error.
        //  When no context is provided the token is missing so the context is the current position
        //---------------------------------------------------------------------------------------
        private void ReportError(JSError errorId)
        {
            ReportError(errorId, false);
        }

        //---------------------------------------------------------------------------------------
        // ReportError
        //
        //  Generate a parser error.
        //  When no context is provided the token is missing so the context is the current position
        //  The function is told whether or not next call to GetToken() should return the same
        //  token or not
        //---------------------------------------------------------------------------------------
        private void ReportError(JSError errorId, bool skipToken)
        {
            // get the current position token
            ReportError(errorId, _curSpan, skipToken);
        }

        //---------------------------------------------------------------------------------------
        // ReportError
        //
        //  Generate a parser error.
        //  The function is told whether or not next call to GetToken() should return the same
        //  token or not
        //---------------------------------------------------------------------------------------
        private void ReportError(JSError errorId, EncodedSpan location, bool skipToken) {
            ReportError(errorId, location.GetSpan(m_scanner.LocationResolver), skipToken);
        }

        private void ReportError(JSError errorId, IndexSpan span, bool skipToken) {
            int previousSeverity = m_severity;
            m_severity = JScriptException.GetSeverity(errorId);
            // EOF error is special and it's the last error we can possibly get
            if (JSToken.EndOfFile == _curToken) {
                throw EOFError(errorId); // EOF context is special
            } else {
                // report the error if not in error condition and the
                // error for this token is not worse than the one for the
                // previous token
                if (m_goodTokensProcessed > 0 || m_severity < previousSeverity) {
                    m_errorSink.HandleError(errorId, span, m_scanner.LocationResolver);
                }

                // reset proper info
                if (skipToken)
                    m_goodTokensProcessed = -1;
                else {
                    m_useCurrentForNext = true;
                    m_goodTokensProcessed = 0;
                }
            }
        }

        //---------------------------------------------------------------------------------------
        // EOFError
        //
        //  Create a context for EOF error. The created context points to the end of the source
        //  code. Assume the the scanner actually reached the end of file
        //---------------------------------------------------------------------------------------
        private Exception EOFError(JSError errorId)
        {
            m_errorSink.HandleError(errorId, new IndexSpan(_source.Length, 0), m_scanner.LocationResolver);
            return new EndOfFileException(); // this exception terminates the parser
        }

        //---------------------------------------------------------------------------------------
        // SkipTokensAndThrow
        //
        //  Skip tokens until one in the no skip set is found.
        //  A call to this function always ends in a throw statement that will be caught by the
        //  proper rule
        //---------------------------------------------------------------------------------------
        private void SkipTokensAndThrow(Expression partialAST = null)
        {
            m_useCurrentForNext = false; // make sure we go to the next token
            bool checkForEndOfLine = m_noSkipTokenSet.HasToken(JSToken.EndOfLine);
            while (!m_noSkipTokenSet.HasToken(_curToken))
            {
                if (checkForEndOfLine)
                {
                    if (m_foundEndOfLine)
                    {
                        m_useCurrentForNext = true;
                        throw new RecoveryTokenException(JSToken.EndOfLine, partialAST);
                    }
                }
                GetNextToken();
                if (++m_tokensSkipped > c_MaxSkippedTokenNumber)
                {
                    m_errorSink.HandleError(JSError.TooManyTokensSkipped, _curSpan, m_scanner.LocationResolver);
                    throw new EndOfFileException();
                }
                if (JSToken.EndOfFile == _curToken)
                    throw new EndOfFileException();
            }

            m_useCurrentForNext = true;
            // got a token in the no skip set, throw
            throw new RecoveryTokenException(_curToken, partialAST);
        }

        internal string CanBeIdentifier(JSToken keyword) {
            switch (keyword) {
                // always allowed
                case JSToken.Get: return "get";
                case JSToken.Set: return "set";

                // not in strict mode
                case JSToken.Implements: return "implements";
                case JSToken.Interface: return "interface";
                case JSToken.Let: return "let";
                case JSToken.Package: return "package";
                case JSToken.Private: return "private";
                case JSToken.Protected: return "protected";
                case JSToken.Public: return "public";
                case JSToken.Static: return "static";
                case JSToken.Yield:
                    if (!_inGenerator) {
                        return "yield";
                    }
                    return null;

                // no other tokens can be identifiers
                default: return null;
            }
        }


        //---------------------------------------------------------------------------------------
        // IndexOfToken
        //
        //  check whether the recovery token is a good one for the caller
        //---------------------------------------------------------------------------------------
        private int IndexOfToken(JSToken[] tokens, RecoveryTokenException exc)
        {
            return IndexOfToken(tokens, exc._token);
        }

        private int IndexOfToken(JSToken[] tokens, JSToken token)
        {
            for (int i = 0; i < tokens.Length; i++) {
                if (tokens[i] == token) {
                    // assume that the caller will deal with the token so move the state back to normal
                    m_useCurrentForNext = false;
                    return i;
                }
            }
            return -1;
        }

        private bool TokenInList(JSToken[] tokens, JSToken token)
        {
            return (-1 != IndexOfToken(tokens, token));
        }

        private bool TokenInList(JSToken[] tokens, RecoveryTokenException exc)
        {
            return (-1 != IndexOfToken(tokens, exc._token));
        }
    }

    // helper classes
    //***************************************************************************************
    //
    //***************************************************************************************
#if !SILVERLIGHT
    [Serializable]
#endif
    internal class ParserException : Exception
    {
        private static string s_errorMsg = JScript.JSParserException;

        public ParserException() : base(s_errorMsg) { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception innerException) : base(message, innerException) { }

#if !SILVERLIGHT
        protected ParserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    internal class UnexpectedTokenException : ParserException
    {
        public UnexpectedTokenException() : base() { }
        public UnexpectedTokenException(string message) : base(message) { }
        public UnexpectedTokenException(string message, Exception innerException) : base(message, innerException) { }
#if !SILVERLIGHT
        protected UnexpectedTokenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    internal class EndOfFileException : ParserException
    {
        public EndOfFileException() : base() { }
        public EndOfFileException(string message) : base(message) { }
        public EndOfFileException(string message, Exception innerException) : base(message, innerException) { }
#if !SILVERLIGHT
        protected EndOfFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

#if !SILVERLIGHT
    [Serializable]
#endif
    internal class RecoveryTokenException : ParserException
    {
        internal JSToken _token;
        internal Node _partiallyComputedNode;

        internal RecoveryTokenException(JSToken token, Node partialAST)
            : base()
        {
            _token = token;
            _partiallyComputedNode = partialAST;
        }

        public Statement PartiallyComputedStatement {
            get {
                var expr = _partiallyComputedNode as Expression;
                if (expr != null) {
                    return new ExpressionStatement(expr.EncodedSpan) { Expression = expr };
                } 
                return _partiallyComputedNode as Statement;
            }
        }

#if !SILVERLIGHT
        protected RecoveryTokenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }

    //***************************************************************************************
    // NoSkipTokenSet
    //
    //  This class is a possible implementation of the no skip token set. It relies on the
    //  fact that the array passed in are static. Should you change it, this implementation
    //  should change as well.
    //  It keeps a linked list of token arrays that are passed in during parsing, on error
    //  condition the list is traversed looking for a matching token. If a match is found
    //  the token should not be skipped and an exception is thrown to let the proper
    //  rule deal with the token
    //***************************************************************************************
    internal class NoSkipTokenSet
    {
        private List<JSToken[]> m_tokenSetList;

        internal NoSkipTokenSet()
        {
            m_tokenSetList = new List<JSToken[]>();
        }

        internal void Add(JSToken[] tokens)
        {
            m_tokenSetList.Add(tokens);
        }

        internal void Remove(JSToken[] tokens)
        {
            bool wasRemoved = m_tokenSetList.Remove(tokens);
            Debug.Assert(wasRemoved, "Token set not in no-skip list");
        }

        internal bool HasToken(JSToken token)
        {
            foreach (JSToken[] tokenSet in m_tokenSetList)
            {
                for (int ndx = 0; ndx < tokenSet.Length; ++ndx)
                {
                    if (tokenSet[ndx] == token)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // list of static no skip token set for specifc rules
        internal static readonly JSToken[] s_ArrayInitNoSkipTokenSet = new JSToken[]{JSToken.RightBracket,
                                                                                           JSToken.Comma};
        internal static readonly JSToken[] s_BlockConditionNoSkipTokenSet = new JSToken[]{JSToken.RightParenthesis,
                                                                                           JSToken.LeftCurly,
                                                                                           JSToken.EndOfLine};
        internal static readonly JSToken[] s_BlockNoSkipTokenSet = new JSToken[] { JSToken.RightCurly };
        internal static readonly JSToken[] s_BracketToken = new JSToken[] { JSToken.RightBracket };
        internal static readonly JSToken[] s_CaseNoSkipTokenSet = new JSToken[]{JSToken.Case,
                                                                                           JSToken.Default,
                                                                                           JSToken.Colon,
                                                                                           JSToken.EndOfLine};
        internal static readonly JSToken[] s_DoWhileBodyNoSkipTokenSet = new JSToken[] { JSToken.While };
        internal static readonly JSToken[] s_EndOfLineToken = new JSToken[] { JSToken.EndOfLine };
        internal static readonly JSToken[] s_EndOfStatementNoSkipTokenSet = new JSToken[]{JSToken.Semicolon,
                                                                                           JSToken.EndOfLine};
        internal static readonly JSToken[] s_ExpressionListNoSkipTokenSet = new JSToken[] { JSToken.Comma };
        internal static readonly JSToken[] s_FunctionDeclNoSkipTokenSet = new JSToken[]{JSToken.RightParenthesis,
                                                                                           JSToken.LeftCurly,
                                                                                           JSToken.Comma};
        internal static readonly JSToken[] s_IfBodyNoSkipTokenSet = new JSToken[] { JSToken.Else };
        internal static readonly JSToken[] s_MemberExprNoSkipTokenSet = new JSToken[]{JSToken.LeftBracket,
                                                                                           JSToken.LeftParenthesis,
                                                                                           JSToken.AccessField};
        internal static readonly JSToken[] s_NoTrySkipTokenSet = new JSToken[]{JSToken.Catch,
                                                                                           JSToken.Finally};
        internal static readonly JSToken[] s_ObjectInitNoSkipTokenSet = new JSToken[]{JSToken.RightCurly,
                                                                                           JSToken.Comma};
        internal static readonly JSToken[] s_ParenExpressionNoSkipToken = new JSToken[] { JSToken.RightParenthesis };
        internal static readonly JSToken[] s_ParenToken = new JSToken[] { JSToken.RightParenthesis };
        internal static readonly JSToken[] s_PostfixExpressionNoSkipTokenSet = new JSToken[]{JSToken.Increment,
                                                                                           JSToken.Decrement};
        internal static readonly JSToken[] s_StartStatementNoSkipTokenSet = new JSToken[]{JSToken.LeftCurly,
                                                                                           JSToken.Var,
                                                                                           JSToken.Const,
                                                                                           JSToken.If,
                                                                                           JSToken.For,
                                                                                           JSToken.Do,
                                                                                           JSToken.While,
                                                                                           JSToken.With,
                                                                                           JSToken.Switch,
                                                                                           JSToken.Try};
        internal static readonly JSToken[] s_SwitchNoSkipTokenSet = new JSToken[]{JSToken.Case,
                                                                                           JSToken.Default};
        internal static readonly JSToken[] s_TopLevelNoSkipTokenSet = new JSToken[]{JSToken.Function};
        internal static readonly JSToken[] s_VariableDeclNoSkipTokenSet = new JSToken[]{JSToken.Comma,
                                                                                           JSToken.Semicolon};
    }

    public enum FunctionType
    {
        Declaration,
        Expression,
        Getter,
        Setter
    }
}

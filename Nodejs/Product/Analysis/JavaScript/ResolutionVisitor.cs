// ResolutionVisitor.cs
//
// Copyright 2012 Microsoft Corporation
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

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.NodejsTools.Parsing {
    /// <summary>
    /// Traverse the tree to build up scope lexically-declared names, var-declared names,
    /// and lookups, then resolve everything.
    /// </summary>
    internal sealed class ResolutionVisitor : AstVisitor
    {
        /// <summary>depth level of with-statements, needed so we can treat decls within with-scopes specially</summary>
        private int m_withDepth;
        /// <summary>stack to maintain the current lexical scope as we traverse the tree</summary>
        private Stack<ActivationObject> m_lexicalStack;
        /// <summary>stack to maintain the current variable scope as we traverse the tree</summary>
        private Stack<ActivationObject> m_variableStack;
        private ErrorSink _errorSink;
        internal readonly LocationResolver _locationResolver;
        private Dictionary<Node, ActivationObject> _scopes = new Dictionary<Node, ActivationObject>();

        private ResolutionVisitor(ActivationObject rootScope, LocationResolver indexResolver, ErrorSink errorSink) {
            // create the lexical and variable scope stacks and push the root scope onto them
            m_lexicalStack = new Stack<ActivationObject>();
            m_lexicalStack.Push(rootScope);

            m_variableStack = new Stack<ActivationObject>();
            m_variableStack.Push(rootScope);

            _locationResolver = indexResolver;
            _errorSink = errorSink;
        }

        #region private properties

        /// <summary>Current lexical scope</summary>
        private ActivationObject CurrentLexicalScope
        {
            get
            {
                return m_lexicalStack.Peek();
            }
        }

        /// <summary>current variable scope</summary>
        private ActivationObject CurrentVariableScope
        {
            get
            {
                return m_variableStack.Peek();
            }
        }

        #endregion

        public static void Apply(Node node, ActivationObject scope, LocationResolver indexResolver, ErrorSink errorSink)
        {
            if (node != null && scope != null)
            {
                // create the visitor and run it. This will create all the child
                // scopes and populate all the scopes with the var-decl, lex-decl,
                // and lookup references within them.
                var visitor = new ResolutionVisitor(scope, indexResolver, errorSink);
                node.Walk(visitor);

                // now that all the scopes are created and they all know what decls
                // they contains, create all the fields
                visitor.CreateFields(scope);

                // now that all the fields have been created in all the scopes,
                // let's go through and resolve all the references
                visitor.ResolveLookups(scope);

                // now that everything is declared and resolved as per the language specs,
                // we need to go back and add ghosted fields for older versions of IE that
                // incorrectly implement catch-variables and named function expressions.
                visitor.AddGhostedFields(scope);
            }
        }

        #region private static methods

        private static void CollapseBlockScope(ActivationObject blockScope)
        {
            // copy over the stuff we want to carry over to the parent
            blockScope.ScopeLookups.CopyItemsTo(blockScope.Parent.ScopeLookups);
            blockScope.VarDeclaredNames.CopyItemsTo(blockScope.Parent.VarDeclaredNames);
            blockScope.ChildScopes.CopyItemsTo(blockScope.Parent.ChildScopes);
            blockScope.GhostedCatchParameters.CopyItemsTo(blockScope.Parent.GhostedCatchParameters);
            blockScope.GhostedFunctions.CopyItemsTo(blockScope.Parent.GhostedFunctions);

            // remove it from its parent's collection of child scopes
            blockScope.Parent.ChildScopes.Remove(blockScope);
        }

        private void CreateFields(ActivationObject scope)
        {
            // declare this scope
            scope.DeclareScope(this);

            // and recurse
            foreach (var childScope in scope.ChildScopes)
            {
                CreateFields(childScope);
            }
        }

        private void ResolveLookups(ActivationObject scope)
        {
            // resolve each lookup this scope contains
            foreach (var lookup in scope.ScopeLookups)
            {
                ResolveLookup(scope, lookup);
            }

            // and recurse
            foreach (var childScope in scope.ChildScopes)
            {
                ResolveLookups(childScope);
            }
        }

        private void ResolveLookup(ActivationObject scope, Lookup lookup)
        {
            // resolve lookup via the lexical scope
            lookup.VariableField = scope.FindReference(lookup.Name);
            if (lookup.VariableField.FieldType == FieldType.UndefinedGlobal)
            {
                
                    _errorSink.HandleUndeclaredVariable(
                        lookup.Name,
                        lookup.GetSpan(_locationResolver),
                        _locationResolver
                    );
                    
            }
        }

        private void AddGhostedFields(ActivationObject scope)
        {
            foreach (var catchParameter in scope.GhostedCatchParameters)
            {
                ResolveGhostedCatchParameter(scope, catchParameter);
            }

            foreach (var ghostFunc in scope.GhostedFunctions)
            {
                ResolveGhostedFunctions(scope, ghostFunc);
            }

            // recurse
            foreach (var childScope in scope.ChildScopes)
            {
                AddGhostedFields(childScope);
            }
        }

        private void ResolveGhostedCatchParameter(ActivationObject scope, ParameterDeclaration catchParameter)
        {
            // check to see if the name exists in the outer variable scope.
            var ghostField = scope[catchParameter.Name];
            if (ghostField == null)
            {
                // set up a ghost field to keep track of the relationship
                ghostField = new JSVariableField(FieldType.GhostCatch, catchParameter.Name);

                scope.AddField(ghostField);
            }
            else if (ghostField.FieldType == FieldType.GhostCatch)
            {
                // there is, but it's another ghost catch variable. That's fine; just use it.
                // don't even flag it as ambiguous because if someone is later referencing the error variable
                // used in a couple catch variables, we'll say something then because other browsers will have that
                // variable undefined or from an outer scope.
            }
            else
            {
                // there is, and it's NOT another ghosted catch variable. Possible naming
                // collision in IE -- if an error happens, it will clobber the existing field's value,
                // although that MAY be the intention; we don't know for sure. But it IS a cross-
                // browser behavior difference.

                if (ghostField.OuterField != null)
                {
                    // and to make matters worse, it's actually bound to an OUTER field
                    // in modern browsers, but will bind to this catch variable in older
                    // versions of IE! Definitely a cross-browser difference!
                    // throw a cross-browser issue error.
                    _errorSink.HandleError(JSError.AmbiguousCatchVar, catchParameter.GetSpan(_locationResolver), _locationResolver);
                }
            }

            // link them so they all keep the same name going forward
            // (since they are named the same in the sources)
            catchParameter.VariableField.OuterField = ghostField;
        }

        private void ResolveGhostedFunctions(ActivationObject scope, FunctionObject funcObject)
        {
            var functionField = funcObject.VariableField;

            // let's check on ghosted names in the outer variable scope
            var ghostField = scope[funcObject.Name];
            if (ghostField == null)
            {
                // nothing; good to go. Add a ghosted field to keep track of it.
                ghostField = new JSVariableField(FieldType.GhostFunction, funcObject.Name);

                scope.AddField(ghostField);
            }
            else if (ghostField.FieldType == FieldType.GhostFunction)
            {
                // there is, but it's another ghosted function expression.
                // what if a lookup is resolved to this field later? We probably still need to
                // at least flag it as ambiguous. We will only need to throw an error, though,
                // if someone actually references the outer ghost variable. 
            }
            else
            {
                // something already exists. Could be a naming collision for IE or at least a
                // a cross-browser behavior difference if it's not coded properly.
                // mark this field as a function, even if it wasn't before

                if (ghostField.OuterField != null)
                {
                    // if the pre-existing field we are ghosting is a reference to
                    // an OUTER field, then we actually have a problem that creates a BIG
                    // difference between older IE browsers and everything else.
                    // modern browsers will have the link to the outer field, but older
                    // IE browsers will link to this function expression!
                    // fire a cross-browser error warning
                    _errorSink.HandleError(
                        JSError.AmbiguousNamedFunctionExpression,
                        funcObject.GetNameSpan(_locationResolver), 
                        _locationResolver
                    );
                }
            }

            // link them so they all keep the same name going forward
            // (since they are named the same in the sources)
            functionField.OuterField = ghostField;
        }

        #endregion

        #region IVisitor Members

        public override bool Walk(ArrayLiteral node)
        {
            if (node != null)
            {
                if (node.Elements != null)
                {
                    foreach (var element in node.Elements) {
                        element.Walk(this);
                    }
                }
            }
            return false;
        }

        public override bool Walk(BinaryOperator node)
        {
            if (node != null)
            {
                if (node.Operand1 != null)
                {
                    node.Operand1.Walk(this);
                }

                if (node.Operand2 != null)
                {
                    node.Operand2.Walk(this);
                }
            }
            return false;
        }

        internal ActivationObject GetScope(Node node) {
            ActivationObject scope;
            if (_scopes.TryGetValue(node, out scope) && scope != null) {
                return scope;
            }
            return null;
        }

        private void SetScope(Node node, ActivationObject scope) {
            _scopes[node] = scope;
        }

        public override bool Walk(Block node)
        {
            if (node != null)
            {
                if (GetScope(node) == null
                    && node.Parent != null
                    && !(node.Parent is SwitchCase)
                    && !(node.Parent is FunctionObject))
                {
                    SetScope(
                        node,
                        new BlockScope(node, CurrentLexicalScope, _errorSink)
                        {
                            IsInWithScope = m_withDepth > 0
                        }
                    );
                }

                if (GetScope(node) != null)
                {
                    m_lexicalStack.Push(GetScope(node));
                }

                try
                {
                    // recurse the block statements
                    for (var ndx = 0; ndx < node.Count; ++ndx)
                    {
                        var statement = node[ndx];
                        if (statement != null)
                        {
                            statement.Walk(this);
                        }
                    }
                }
                finally
                {

                    if (GetScope(node) != null)
                    {
                        Debug.Assert(CurrentLexicalScope == GetScope(node));
                        m_lexicalStack.Pop();
                    }
                }

                // now, if the block has no lex-decls, we really don't need a separate scope.
                if (GetScope(node) != null
                    && !(GetScope(node) is WithScope)
                    && !(GetScope(node) is CatchScope)
                    && GetScope(node).LexicallyDeclaredNames.Count == 0)
                {
                    CollapseBlockScope(GetScope(node));
                    SetScope(node, null);
                }
            }
            return false;
        }

        public override bool Walk(Break node)
        {
            return false;
        }

        public override bool Walk(CallNode node)
        {
            if (node != null)
            {
                if (node.Function != null)
                {
                    node.Function.Walk(this);
                }

                if (node.Arguments != null)
                {
                    foreach (var arg in node.Arguments) {
                        if (arg != null) {
                            arg.Walk(this);
                        }
                    }
                }
            }
            return false;
        }

        public override bool Walk(Conditional node)
        {
            if (node != null)
            {
                if (node.Condition != null)
                {
                    node.Condition.Walk(this);
                }

                if (node.TrueExpression != null)
                {
                    node.TrueExpression.Walk(this);
                }

                if (node.FalseExpression != null)
                {
                    node.FalseExpression.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(ConstantWrapper node)
        {
            return false;
        }

        public override bool Walk(ConstStatement node)
        {
            if (node != null)
            {
                // the statement itself doesn't get executed, but the initializers do
                for (var ndx = 0; ndx < node.Count; ++ndx)
                {
                    var item = node[ndx];
                    if (item != null)
                    {
                        item.Walk(this);
                    }
                }
            }
            return false;
        }

        public override bool Walk(ContinueNode node)
        {
            return false;
        }

        public override bool Walk(DebuggerNode node)
        {
            return false;
        }

        public override bool Walk(DirectivePrologue node)
        {
            if (node != null)
            {
                if (node.UseStrict)
                {
                    CurrentVariableScope.UseStrict = true;
                }
            }
            return false;
        }

        public override bool Walk(DoWhile node)
        {
            if (node != null)
            {
                if (node.Body != null)
                {
                    node.Body.Walk(this);
                }

                if (node.Condition != null)
                {
                    node.Condition.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(EmptyStatement node)
        {
            // nothing to do
            return false;
        }

        public override bool Walk(ForIn node)
        {
            if (node != null)
            {
                if (node.Collection != null)
                {
                    node.Collection.Walk(this);
                }

                if (node.Variable != null)
                {
                    // if the variable portion of the for-in statement is a lexical
                    // declaration, then we will create the block node for its body right now
                    // and add the declaration. This will prevent the body from deleting
                    // an empty lexical scope.
                    var lexDeclaration = node.Variable as LexicalDeclaration;
                    if (lexDeclaration != null)
                    {
                        // create the scope on the block
                        SetScope(
                            node,
                            new BlockScope(node, CurrentLexicalScope, _errorSink)
                            {
                                IsInWithScope = m_withDepth > 0
                            }
                        );
                        m_lexicalStack.Push(GetScope(node));
                    }
                }

                try
                {
                    if (node.Variable != null)
                    {
                        node.Variable.Walk(this);
                    }

                    if (node.Body != null)
                    {
                        node.Body.Walk(this);
                    }
                }
                finally
                {
                    if (GetScope(node) != null)
                    {
                        Debug.Assert(CurrentLexicalScope == GetScope(node));
                        m_lexicalStack.Pop();
                    }
                }
            }
            return false;
        }

        public override bool Walk(ForNode node)
        {
            if (node != null)
            {
                if (node.Initializer != null)
                {
                    // if the variable portion of the for-in statement is a lexical
                    // declaration, then we will create the block node for its body right now
                    // and add the declaration. This will prevent the body from both creating
                    // a new lexical scope and from deleting an empty one.
                    var lexDeclaration = node.Initializer as LexicalDeclaration;
                    if (lexDeclaration != null)
                    {
                        // create the scope on the block
                        SetScope(
                            node,
                            new BlockScope(node, CurrentLexicalScope, _errorSink)
                            {
                                IsInWithScope = m_withDepth > 0
                            }
                        );
                        m_lexicalStack.Push(GetScope(node));
                    }
                }

                try
                {
                    if (node.Initializer != null)
                    {
                        node.Initializer.Walk(this);
                    }

                    if (node.Condition != null)
                    {
                        node.Condition.Walk(this);
                    }

                    if (node.Body != null)
                    {
                        node.Body.Walk(this);
                    }

                    if (node.Incrementer != null)
                    {
                        node.Incrementer.Walk(this);
                    }
                }
                finally
                {
                    if (GetScope(node) != null)
                    {
                        Debug.Assert(CurrentLexicalScope == GetScope(node));
                        m_lexicalStack.Pop();
                    }
                }
            }
            return false;
        }

        public override bool Walk(FunctionObject node)
        {
            if (node != null)
            {

                // create a function scope, assign it to the function object,
                // and push it on the stack
                var parentScope = CurrentLexicalScope;
                if (node.FunctionType == FunctionType.Expression 
                    && !string.IsNullOrEmpty(node.Name))
                {                    
                    // add this function object to the list of function objects the variable scope
                    // will need to ghost later
                    CurrentVariableScope.GhostedFunctions.Add(node);
                }

                SetScope(
                    node,
                    new FunctionScope(node, parentScope, node.FunctionType != FunctionType.Declaration, node, _errorSink)
                    {
                        IsInWithScope = m_withDepth > 0
                    }
                );
                m_lexicalStack.Push(GetScope(node));
                m_variableStack.Push(GetScope(node));

                try
                {
                    // recurse into the function to handle it after saving the current index and resetting it
                    if (node.Body != null)
                    {
                        node.Body.Walk(this);
                    }
                }
                finally
                {
                    Debug.Assert(CurrentLexicalScope == GetScope(node));
                    m_lexicalStack.Pop();
                    m_variableStack.Pop();
                }

                // nothing to add to the var-decl list.
                // but add the function name to the current lex-decl list
                // IF it is a declaration and it has a name (and it SHOULD unless there was an error)
                if (node.FunctionType == FunctionType.Declaration && 
                    !string.IsNullOrEmpty(node.Name))
                {
                    var lexicalScope = CurrentLexicalScope;
                    lexicalScope.LexicallyDeclaredNames.Add(node);

                    if (lexicalScope != CurrentVariableScope)
                    {
                        // the current lexical scope is the variable scope.
                        // this is ES6 syntax: a function declaration inside a block scope. Not allowed
                        // in ES5 code, so throw a warning and ghost this function in the outer variable scope 
                        // to make sure that we don't generate any naming collisions.
                        _errorSink.HandleError(JSError.MisplacedFunctionDeclaration, node.GetNameSpan(_locationResolver), _locationResolver);
                        CurrentVariableScope.GhostedFunctions.Add(node);
                    }
                }
            }
            return false;
        }

        public override bool Walk(GetterSetter node)
        {
            // nothing to do
            return true;
        }

        public override bool Walk(GroupingOperator node)
        {
            if (node != null)
            {
                if (node.Operand != null)
                {
                    node.Operand.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(IfNode node)
        {
            if (node != null)
            {
                if (node.Condition != null)
                {
                    node.Condition.Walk(this);
                }

                // make true and false block numbered from the same starting point?
                if (node.TrueBlock != null)
                {
                    node.TrueBlock.Walk(this);
                }

                if (node.FalseBlock != null)
                {
                    node.FalseBlock.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(LabeledStatement node)
        {
            if (node != null)
            {
                if (node.Statement != null)
                {
                    node.Statement.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(LexicalDeclaration node)
        {
            if (node != null)
            {
                for (var ndx = 0; ndx < node.Count; ++ndx)
                {
                    var decl = node[ndx];
                    if (decl != null)
                    {
                        decl.Walk(this);
                    }
                }
            }
            return false;
        }

        public override bool Walk(Lookup node)
        {
            if (node != null)
            {
                // add the lookup node to the current lexical scope, because
                // that's the starting point for this node's lookup resolution.
                CurrentLexicalScope.ScopeLookups.Add(node);
            }
            return false;
        }

        public override bool Walk(Member node)
        {
            if (node != null)
            {
                if (node.Root != null)
                {
                    node.Root.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(ObjectLiteral node)
        {
            if (node != null)
            {
                foreach (var prop in node.Properties) {
                    prop.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(ObjectLiteralField node)
        {
            // nothing to do
            return true;
        }

        public override bool Walk(ObjectLiteralProperty node)
        {
            if (node != null)
            {
                // don't care about the property names; just recurse the values
                if (node.Value != null)
                {
                    node.Value.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(ParameterDeclaration node)
        {
            // nothing to do
            return true;
        }

        public override bool Walk(RegExpLiteral node)
        {
            return true;
        }

        public override bool Walk(ReturnNode node)
        {
            if (node != null)
            {
                if (node.Operand != null)
                {
                    node.Operand.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(Switch node)
        {
            if (node != null)
            {
                if (node.Expression != null)
                {
                    node.Expression.Walk(this);
                }

                // the switch has its own block scope to use for all the blocks that are under
                // its child switch-case nodes
                SetScope(
                    node,
                    new BlockScope(node, CurrentLexicalScope, _errorSink)
                    {
                        IsInWithScope = m_withDepth > 0
                    }
                );
                m_lexicalStack.Push(GetScope(node));

                try
                {
                    if (node.Cases != null)
                    {
                        foreach (var caseNode in node.Cases) {
                            caseNode.Walk(this);
                        }
                    }
                }
                finally
                {
                    Debug.Assert(CurrentLexicalScope == GetScope(node));
                    m_lexicalStack.Pop();
                }

                // if the block has no lex-decls, we really don't need a separate scope.
                if (GetScope(node).LexicallyDeclaredNames.Count == 0)
                {
                    CollapseBlockScope(GetScope(node));
                    SetScope(node, null);
                }
            }
            return false;
        }

        public override bool Walk(SwitchCase node)
        {
            return true;
        }

        public override bool Walk(ThisLiteral node)
        {
            return true;
        }

        public override bool Walk(ThrowNode node)
        {
            if (node != null)
            {
                if (node.Operand != null)
                {
                    node.Operand.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(TryNode node)
        {
            if (node != null)
            {
                if (node.TryBlock != null)
                {
                    node.TryBlock.Walk(this);
                }

                // add this catch parameter to the list of catch parameters the variable
                // scope will need to ghost later.
                if (node.CatchParameter != null)
                {
                    CurrentVariableScope.GhostedCatchParameters.Add(node.CatchParameter);
                }

                if (node.CatchBlock != null)
                {
                    // create the catch-scope, add the catch parameter to it, and recurse the catch block.
                    // the block itself will push the scope onto the stack and pop it off, so we don't have to.
                    SetScope(
                        node.CatchBlock, 
                        new CatchScope(node.CatchBlock, CurrentLexicalScope, node.CatchParameter, _errorSink)
                        {
                            IsInWithScope = m_withDepth > 0
                        }
                    );
                    GetScope(node.CatchBlock).LexicallyDeclaredNames.Add(node.CatchParameter);
                    node.CatchBlock.Walk(this);
                }

                if (node.FinallyBlock != null)
                {
                    node.FinallyBlock.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(UnaryOperator node)
        {
            if (node != null)
            {
                if (node.Operand != null)
                {
                    node.Operand.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(Var node)
        {
            if (node != null)
            {
                for (var ndx = 0; ndx < node.Count; ++ndx)
                {
                    var decl = node[ndx];
                    if (decl != null)
                    {
                        decl.Walk(this);
                    }
                }
            }
            return false;
        }

        public override bool Walk(VariableDeclaration node)
        {
            if (node != null)
            {
                if (node.Parent is LexicalDeclaration)
                {
                    // ES6 let or const declaration. Only add to the current lexical scope.
                    CurrentLexicalScope.LexicallyDeclaredNames.Add(node);
                }
                else
                {
                    // must be var or const (mozilla-style). Add to both the lexical scope
                    // and the variable scope. The variable scope will actually use this node
                    // to create a field; the lexical stack will just use it to detect conflicts
                    // with lex-decls
                    CurrentLexicalScope.VarDeclaredNames.Add(node);
                    CurrentVariableScope.VarDeclaredNames.Add(node);
                }

                if (node.Initializer != null)
                {
                    // recurse the initializer
                    node.Initializer.Walk(this);
                }
            }
            return false;
        }

        public override bool Walk(WhileNode node)
        {
            return true;
        }

        public override bool Walk(WithNode node)
        {
            if (node != null)
            {
                if (node.WithObject != null)
                {
                    node.WithObject.Walk(this);
                }

                if (node.Body != null)
                {
                    // create the with-scope and recurse the block.
                    // the block itself will push the scope onto the stack and pop it off, so we don't have to.
                    SetScope(
                        node.Body, 
                        new WithScope(node.Body, CurrentLexicalScope, _errorSink)
                    );

                    try
                    {
                        ++m_withDepth;
                        node.Body.Walk(this);
                    }
                    finally
                    {
                        --m_withDepth;
                    }
                }
            }
            return false;
        }

        #endregion

        public override bool Walk(CommaOperator node)
        {
            return true;
        }

        public override bool Walk(JsAst jsAst)
        {
            return true;
        }

        public override bool Walk(FunctionExpression functionExpression)
        {
            return true;
        }
    }
}

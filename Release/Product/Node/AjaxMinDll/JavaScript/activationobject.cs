// activationobject.cs
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
using System.Globalization;
using System.Reflection;

namespace Microsoft.Ajax.Utilities
{
    public abstract class ActivationObject
    {
        #region private fields

        private bool m_useStrict;//= false;
        private bool m_isKnownAtCompileTime;
        private CodeSettings m_settings;

        #endregion

        #region public properties

        public bool UseStrict
        {
            get
            {
                return m_useStrict;
            }
            set
            {
                // can set it to true, but can't set it to false
                if (value)
                {
                    // set our value
                    m_useStrict = value;

                    // and all our child scopes (recursive)
                    foreach (var child in ChildScopes)
                    {
                        child.UseStrict = value;
                    }
                }
            }
        }

        public bool IsKnownAtCompileTime
        {
            get { return m_isKnownAtCompileTime; }
            set 
            { 
                m_isKnownAtCompileTime = value;
                if (!value 
                    && m_settings.EvalTreatment == EvalTreatment.MakeAllSafe)
                {
                    // are we a function scope?
                    var funcScope = this as FunctionScope;
                    if (funcScope == null)
                    {
                        // we are not a function, so the parent scope is unknown too
                        Parent.IfNotNull(p => p.IsKnownAtCompileTime = false);
                    }
                    else
                    {
                        // we are a function, check to see if the function object is actually
                        // referenced. (we don't want to mark the parent as unknown if this function 
                        // isn't even referenced).
                        if (funcScope.FunctionObject.IsReferenced)
                        {
                            Parent.IsKnownAtCompileTime = false;
                        }
                    }
                }
            }
        }

        public ActivationObject Parent { get; private set; }
        public bool IsInWithScope { get; set; }

        public IDictionary<string, JSVariableField> NameTable { get; private set; }

        public IList<ActivationObject> ChildScopes { get; private set; }

        public ICollection<Lookup> ScopeLookups { get; private set; }
        public ICollection<INameDeclaration> VarDeclaredNames { get; private set; }
        public ICollection<INameDeclaration> LexicallyDeclaredNames { get; private set; }

        public ICollection<ParameterDeclaration> GhostedCatchParameters { get; private set; }
        public ICollection<FunctionObject> GhostedFunctions { get; private set; }

        #endregion

        protected ActivationObject(ActivationObject parent, CodeSettings codeSettings)
        {
            m_isKnownAtCompileTime = true;
            m_useStrict = false;
            m_settings = codeSettings;

            Parent = parent;
            NameTable = new Dictionary<string, JSVariableField>();
            ChildScopes = new List<ActivationObject>();

            // if our parent is a scope....
            if (parent != null)
            {
                // add us to the parent's list of child scopes
                parent.ChildScopes.Add(this);

                // if the parent is strict, so are we
                UseStrict = parent.UseStrict;
            }

            // create the two lists of declared items for this scope
            ScopeLookups = new HashSet<Lookup>();
            VarDeclaredNames = new HashSet<INameDeclaration>();
            LexicallyDeclaredNames = new HashSet<INameDeclaration>();

            GhostedCatchParameters = new HashSet<ParameterDeclaration>();
            GhostedFunctions = new HashSet<FunctionObject>();
        }

        #region scope setup methods

        /// <summary>
        /// Set up this scope's fields from the declarations it contains
        /// </summary>
        public abstract void DeclareScope();

        protected void DefineLexicalDeclarations()
        {
            foreach (var lexDecl in LexicallyDeclaredNames)
            {
                // use the function as the field value if it's a function
                DefineField(lexDecl, lexDecl as FunctionObject);
            }
        }

        protected void DefineVarDeclarations()
        {
            foreach (var varDecl in VarDeclaredNames)
            {
                // var-decls are always initialized to null
                DefineField(varDecl, null);
            }
        }

        private void DefineField(INameDeclaration nameDecl, FunctionObject fieldValue)
        {
            var field = this[nameDecl.Name];
            if (nameDecl is ParameterDeclaration)
            {
                // function parameters are handled separately, so if this is a parameter declaration,
                // then it must be a catch variable. 
                if (field == null)
                {
                    // no collision - create the catch-error field
                    field = new JSVariableField(FieldType.CatchError, nameDecl.Name, 0, null)
                    {
                        OriginalContext = nameDecl.NameContext.Clone(),
                        IsDeclared = true
                    };

                    this.AddField(field);
                }
                else
                {
                    // it's an error to declare anything in the catch scope with the same name as the
                    // error variable
                    field.OriginalContext.HandleError(JSError.DuplicateCatch, true);
                }
            }
            else
            {
                if (field == null)
                {
                    // could be global or local depending on the scope, so let the scope create it.
                    field = this.CreateField(nameDecl.Name, null, 0);
                    field.OriginalContext = nameDecl.NameContext.Clone();
                    field.IsDeclared = true;
                    field.IsFunction = (nameDecl is FunctionObject);
                    field.FieldValue = fieldValue;

                    // if this field is a constant, mark it now
                    var lexDeclaration = nameDecl.Parent as LexicalDeclaration;
                    field.InitializationOnly = nameDecl.Parent is ConstStatement
                        || (lexDeclaration != null && lexDeclaration.StatementToken == JSToken.Const);

                    this.AddField(field);
                }
                else
                {
                    // already defined! 
                    // if this is a lexical declaration, then it's an error because we have two
                    // lexical declarations with the same name in the same scope.
                    if (nameDecl.Parent is LexicalDeclaration)
                    {
                        nameDecl.NameContext.HandleError(JSError.DuplicateLexicalDeclaration, true);
                    }

                    if (nameDecl.Initializer != null)
                    {
                        // if this is an initialized declaration, then the var part is
                        // superfluous and the "initializer" is really a lookup assignment. 
                        // So bump up the ref-count for those cases.
                        var nameReference = nameDecl as INameReference;
                        if (nameReference != null)
                        {
                            field.AddReference(nameReference);
                        }
                    }

                    // don't clobber an existing field value with null. For instance, the last 
                    // function declaration is the winner, so always set the value if we have something,
                    // but a var following a function shouldn't reset it to null.
                    if (fieldValue != null)
                    {
                        field.FieldValue = fieldValue;
                    }
                }
            }

            nameDecl.VariableField = field;
            field.Declarations.Add(nameDecl);

            // if this scope is within a with-statement, or if the declaration was flagged
            // as not being renamable, then mark the field as not crunchable
            if (IsInWithScope || nameDecl.RenameNotAllowed)
            {
                field.CanCrunch = false;
            }
        }

        #endregion

        #region AnalyzeScope functionality

        internal void AnalyzeScope()
        {
            // check for unused local fields or arguments if this isn't the global scope.
            // also remove unused lexical function declaration in with-scopes.
            if (!(this is GlobalScope))
            {
                AnalyzeNonGlobalScope();
            }

            // rename fields if we need to
            ManualRenameFields();

            // recurse 
            foreach (var activationObject in ChildScopes)
            {
                activationObject.AnalyzeScope();
            }
        }

        private void AnalyzeNonGlobalScope()
        {
            foreach (var variableField in NameTable.Values)
            {
                // not referenced, not generated, and has an original context so not added after the fact.
                // and we don't care if catch-error fields are unreferenced.
                if (!variableField.IsReferenced
                    && !variableField.IsGenerated
                    && variableField.OuterField == null
                    && variableField.FieldType != FieldType.CatchError
                    && variableField.FieldType != FieldType.GhostCatch
                    && variableField.OriginalContext != null)
                {
                    UnreferencedVariableField(variableField);
                }
                else if (variableField.RefCount == 1
                    && this.IsKnownAtCompileTime
                    && m_settings.RemoveUnneededCode
                    && m_settings.IsModificationAllowed(TreeModifications.RemoveUnusedVariables))
                {
                    SingleReferenceVariableField(variableField);
                }
            }
        }

        private void UnreferencedVariableField(JSVariableField variableField)
        {
            // see if the value is a function
            var functionObject = variableField.FieldValue as FunctionObject;
            if (functionObject != null)
            {
                UnreferencedFunction(variableField, functionObject);
            }
            else if (variableField.FieldType == FieldType.Argument)
            {
                UnreferencedArgument(variableField);
            }
            else if (!variableField.WasRemoved)
            {
                UnreferencedVariable(variableField);
            }
        }

        private void UnreferencedFunction(JSVariableField variableField, FunctionObject functionObject)
        {
            // if there is no name, then ignore this declaration because it's malformed.
            // (won't be a function expression because those are automatically referenced).
            // also ignore ghosted function fields.
            if (functionObject.Name != null && variableField.FieldType != FieldType.GhostFunction)
            {
                // if the function name isn't a simple identifier, then leave it there and mark it as
                // not renamable because it's probably one of those darn IE-extension event handlers or something.
                if (JSScanner.IsValidIdentifier(functionObject.Name))
                {
                    // unreferenced function declaration. fire a warning.
                    var ctx = functionObject.IdContext ?? variableField.OriginalContext;
                    ctx.HandleError(JSError.FunctionNotReferenced, false);

                    // hide it from the output if our settings say we can.
                    // we don't want to delete it, per se, because we still want it to 
                    // show up in the scope report so the user can see that it was unreachable
                    // in case they are wondering where it went.
                    // ES6 has the notion of block-scoped function declarations. ES5 says functions can't
                    // be defined inside blocks -- only at the root level of the global scope or function scopes.
                    // so if this is a block scope, don't hide the function, even if it is unreferenced because
                    // of the cross-browser difference.
                    if (this.IsKnownAtCompileTime
                        && m_settings.MinifyCode
                        && m_settings.RemoveUnneededCode
                        && !(this is BlockScope))
                    {
                        functionObject.HideFromOutput = true;
                    }
                }
                else
                {
                    // not a valid identifier name for this function. Don't rename it because it's
                    // malformed and we don't want to mess up the developer's intent.
                    variableField.CanCrunch = false;
                }
            }
        }

        private void UnreferencedArgument(JSVariableField variableField)
        {
            // unreferenced argument. We only want to throw a warning if there are no referenced arguments
            // AFTER this unreferenced argument. Also, we're assuming that if this is an argument field,
            // this scope MUST be a function scope.
            var functionScope = this as FunctionScope;
            if (functionScope != null)
            {
                if (functionScope.FunctionObject.IfNotNull(func => func.IsArgumentTrimmable(variableField)))
                {
                    // if we are planning on removing unreferenced function parameters, mark it as removed
                    // so we don't waste a perfectly good auto-rename name on it later.
                    if (m_settings.RemoveUnneededCode
                        && m_settings.IsModificationAllowed(TreeModifications.RemoveUnusedParameters))
                    {
                        variableField.WasRemoved = true;
                    }

                    variableField.OriginalContext.HandleError(
                        JSError.ArgumentNotReferenced,
                        false);
                }
            }
        }

        private void UnreferencedVariable(JSVariableField variableField)
        {
            var throwWarning = true;

            // not a function, not an argument, not a catch-arg, not a global.
            // not referenced. If there's a single definition, and it either has no
            // initializer or the initializer is constant, get rid of it. 
            // (unless we aren't removing unneeded code, or the scope is unknown)
            if (variableField.Declarations.Count == 1
                && this.IsKnownAtCompileTime)
            {
                var varDecl = variableField.OnlyDeclaration as VariableDeclaration;
                if (varDecl != null)
                {
                    var declaration = varDecl.Parent as Declaration;
                    if (declaration != null
                        && (varDecl.Initializer == null || varDecl.Initializer.IsConstant))
                    {
                        // if the decl parent is a for-in and the decl is the variable part
                        // of the statement, then just leave it alone. Don't even throw a warning
                        var forInStatement = declaration.Parent as ForIn;
                        if (forInStatement != null
                            && declaration == forInStatement.Variable)
                        {
                            // just leave it alone, and don't even throw a warning for it.
                            // TODO: try to reuse some pre-existing variable, or maybe replace
                            // this vardecl with a ref to an unused parameter if this is inside
                            // a function.
                            throwWarning = false;
                        }
                        else if (m_settings.RemoveUnneededCode
                            && m_settings.IsModificationAllowed(TreeModifications.RemoveUnusedVariables))
                        {
                            variableField.Declarations.Remove(varDecl);

                            // don't "remove" the field if it's a ghost to another field
                            if (variableField.GhostedField == null)
                            {
                                variableField.WasRemoved = true;
                            }

                            // remove the vardecl from the declaration list, and if the
                            // declaration list is now empty, remove it, too
                            declaration.Remove(varDecl);
                            if (declaration.Count == 0)
                            {
                                declaration.Parent.ReplaceChild(declaration, null);
                            }
                        }
                    }
                    else if (varDecl.Parent is ForIn)
                    {
                        // then this is okay
                        throwWarning = false;
                    }
                }
            }

            if (throwWarning && variableField.Declarations.Count > 0)
            {
                // not referenced -- throw a warning, assuming it hasn't been "removed" 
                // via an optimization or something.
                variableField.OriginalContext.HandleError(
                    JSError.VariableDefinedNotReferenced,
                    false);
            }
        }

        private static void SingleReferenceVariableField(JSVariableField variableField)
        {
            // local fields that don't reference an outer field, have only one refcount
            // and one declaration
            if (variableField.FieldType == FieldType.Local
                && variableField.OuterField == null
                && variableField.Declarations.Count == 1)
            {
                // there should only be one, it should be a vardecl, and 
                // either no initializer or a constant initializer
                var varDecl = variableField.OnlyDeclaration as VariableDeclaration;
                if (varDecl != null
                    && varDecl.Initializer != null
                    && varDecl.Initializer.IsConstant)
                {
                    // there should only be one
                    var reference = variableField.OnlyReference;
                    if (reference != null)
                    {
                        // if the reference is not being assigned to, it is not an outer reference
                        // (meaning the lookup is in the same scope as the declaration), and the
                        // lookup is after the declaration
                        if (!reference.IsAssignment
                            && reference.VariableField != null
                            && reference.VariableField.OuterField == null
                            && reference.VariableField.CanCrunch
                            && varDecl.Index < reference.Index
                            && !IsIterativeReference(varDecl.Initializer, reference))
                        {
                            // so we have a declaration assigning a constant value, and only one
                            // reference reading that value. replace the reference with the constant
                            // and get rid of the declaration.
                            // transform: var lookup=constant;lookup   ==>   constant
                            // remove the vardecl
                            var declaration = varDecl.Parent as Declaration;
                            if (declaration != null)
                            {
                                // replace the reference with the constant
                                variableField.References.Remove(reference);
                                var refNode = reference as AstNode;
                                refNode.Parent.IfNotNull(p => p.ReplaceChild(refNode, varDecl.Initializer));

                                // we're also going to remove the declaration itself
                                variableField.Declarations.Remove(varDecl);
                                variableField.WasRemoved = true;

                                // remove the vardecl from the declaration list
                                // and if the declaration is now empty, remove it, too
                                declaration.Remove(varDecl);
                                if (declaration.Count == 0)
                                {
                                    declaration.Parent.IfNotNull(p => p.ReplaceChild(declaration, null));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsIterativeReference(AstNode initializer, INameReference reference)
        {
            // we only care about array and regular expressions with the global switch at this point.
            // if it's not one of those types, then go ahead and assume iterative reference doesn't matter.
            var regExp = initializer as RegExpLiteral;
            if (initializer is ArrayLiteral 
                || (regExp != null && regExp.PatternSwitches != null && regExp.PatternSwitches.IndexOf("g", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                // get the parent block for the initializer. We'll use this as a stopping point in our loop.
                var parentBlock = GetParentBlock(initializer);

                // walk up the parent chain from the reference. If we find a while, a for, or a do-while,
                // then we know this reference is iteratively called.
                // stop when the parent is null, the same block containing the initializer, or a function object.
                // (because a function object will step out of scope, and we know we should be in the same scope)
                var child = reference as AstNode;
                var parent = child.Parent;
                while (parent != null && parent != parentBlock && !(parent is FunctionObject))
                {
                    // while or do-while is iterative -- the condition and the body are both called repeatedly.
                    if (parent is WhileNode || parent is DoWhile)
                    {
                        return true;
                    }

                    // for-statements call the condition, the incrementer, and the body repeatedly, but not the
                    // initializer.
                    var forNode = parent as ForNode;
                    if (forNode != null && child != forNode.Initializer)
                    {
                        return true;
                    }

                    // in forin-statements, only the body is repeated, the collection is evaluated only once.
                    var forInStatement = parent as ForIn;
                    if (forInStatement != null && child == forInStatement.Body)
                    {
                        return true;
                    }

                    // go up
                    child = parent;
                    parent = parent.Parent;
                }
            }

            return false;
        }

        /// <summary>
        /// Return the first Block node in the tree starting from the given node and working up through the parent nodes.
        /// </summary>
        /// <param name="node">initial node</param>
        /// <returns>first block node in the node tree</returns>
        private static Block GetParentBlock(AstNode node)
        {
            while(node != null)
            {
                // see if the current node is a block, and if so, return it.
                var block = node as Block;
                if (block != null)
                {
                    return block;
                }

                // try the parent
                node = node.Parent;
            }

            // if we get here, we never found a parent block.
            return null;
        }

        private void ManualRenameFields()
        {
            // if the local-renaming kill switch is on, we won't be renaming ANYTHING, so we'll have nothing to do.
            if (m_settings.IsModificationAllowed(TreeModifications.LocalRenaming))
            {
                // if the parser settings has a list of rename pairs, we will want to go through and rename
                // any matches
                if (m_settings.HasRenamePairs)
                {
                    // go through the list of fields in this scope. Anything defined in the script that
                    // is in the parser rename map should be renamed and the auto-rename flag reset so
                    // we don't change it later.
                    foreach (var varField in NameTable.Values)
                    {
                        // don't rename outer fields (only actual fields), 
                        // and we're only concerned with global or local variables --
                        // those which are defined by the script (not predefined, not the arguments object)
                        if (varField.OuterField == null 
                            && (varField.FieldType != FieldType.Arguments && varField.FieldType != FieldType.Predefined))
                        {
                            // see if the name is in the parser's rename map
                            string newName = m_settings.GetNewName(varField.Name);
                            if (!string.IsNullOrEmpty(newName))
                            {
                                // it is! Change the name of the field, but make sure we reset the CanCrunch flag
                                // or setting the "crunched" name won't work.
                                // and don't bother making sure the name doesn't collide with anything else that
                                // already exists -- if it does, that's the developer's fault.
                                // TODO: should we at least throw a warning?
                                varField.CanCrunch = true;
                                varField.CrunchedName = newName;

                                // and make sure we don't crunch it later
                                varField.CanCrunch = false;
                            }
                        }
                    }
                }

                // if the parser settings has a list of no-rename names, then we will want to also mark any
                // fields that match and are still slated to rename as uncrunchable so they won't get renamed.
                // if the settings say we're not going to renaming anything automatically (KeepAll), then we 
                // have nothing to do.
                if (m_settings.LocalRenaming != LocalRenaming.KeepAll)
                {
                    foreach (var noRename in m_settings.NoAutoRenameCollection)
                    {
                        // don't rename outer fields (only actual fields), 
                        // and we're only concerned with fields that can still
                        // be automatically renamed. If the field is all that AND is listed in
                        // the collection, set the CanCrunch to false
                        JSVariableField varField;
                        if (NameTable.TryGetValue(noRename, out varField)
                            && varField.OuterField == null
                            && varField.CanCrunch)
                        {
                            // no, we don't want to crunch this field
                            varField.CanCrunch = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region crunching methods

        internal virtual void ValidateGeneratedNames()
        {
            // check all the variables defined within this scope.
            // we're looking for uncrunched generated fields.
            foreach (JSVariableField variableField in NameTable.Values)
            {
                if (variableField.IsGenerated
                    && variableField.CrunchedName == null)
                {
                    // we need to rename this field.
                    // first we need to walk all the child scopes depth-first
                    // looking for references to this field. Once we find a reference,
                    // we then need to add all the other variables referenced in those
                    // scopes and all above them (from here) so we know what names we
                    // can't use.
                    var avoidTable = new HashSet<string>();
                    GenerateAvoidList(avoidTable, variableField.Name);

                    // now that we have our avoid list, create a crunch enumerator from it
                    CrunchEnumerator crunchEnum = new CrunchEnumerator(avoidTable);

                    // and use it to generate a new name
                    variableField.CrunchedName = crunchEnum.NextName();
                }
            }

            // recursively traverse through our children
            foreach (ActivationObject scope in ChildScopes)
            {
                scope.ValidateGeneratedNames();
            }
        }

        private bool GenerateAvoidList(HashSet<string> table, string name)
        {
            // our reference flag is based on what was passed to us
            bool isReferenced = false;

            // depth first, so walk all the children
            foreach (ActivationObject childScope in ChildScopes)
            {
                // if any child returns true, then it or one of its descendents
                // reference this variable. So we reference it, too
                if (childScope.GenerateAvoidList(table, name))
                {
                    // we'll return true because we reference it
                    isReferenced = true;
                }
            }

            if (!isReferenced)
            {
                // none of our children reference the scope, so see if we do
                isReferenced = NameTable.ContainsKey(name);
            }

            if (isReferenced)
            {
                // if we reference the name or are in line to reference the name,
                // we need to add all the variables we reference to the list
                foreach (var variableField in NameTable.Values)
                {
                    table.Add(variableField.ToString());
                }
            }

            // return whether or not we are in the reference chain
            return isReferenced;
        }

        internal virtual void AutoRenameFields()
        {
            // if we're not known at compile time, then we can't crunch
            // the local variables in this scope, because we can't know if
            // something will reference any of it at runtime.
            // eval is something that will make the scope unknown because we
            // don't know what eval will evaluate to until runtime
            if (m_isKnownAtCompileTime)
            {
                // get an array of all the uncrunched local variables defined in this scope
                var localFields = GetUncrunchedLocals();
                if (localFields != null)
                {
                    // create a crunch-name enumerator, taking into account any fields within our
                    // scope that have already been crunched.
                    var avoidSet = new HashSet<string>();
                    foreach (var field in NameTable.Values)
                    {
                        // if the field can't be crunched, or if it can but we've already crunched it,
                        // add it to the avoid list so we don't reuse that name
                        if (!field.CanCrunch || field.CrunchedName != null)
                        {
                            avoidSet.Add(field.ToString());
                        }
                    }

                    var crunchEnum = new CrunchEnumerator(avoidSet);
                    foreach (var localField in localFields)
                    {
                        // if we are an unambiguous reference to a named function expression and we are not
                        // referenced by anyone else, then we can just skip this variable because the
                        // name will be stripped from the output anyway.
                        // we also always want to crunch "placeholder" fields.
                        if (localField.CanCrunch
                            && (localField.RefCount > 0 || localField.IsDeclared || localField.IsPlaceholder
                            || !(m_settings.RemoveFunctionExpressionNames && m_settings.IsModificationAllowed(TreeModifications.RemoveFunctionExpressionNames))))
                        {
                            localField.CrunchedName = crunchEnum.NextName();
                        }
                    }
                }
            }

            // then traverse through our children
            foreach (ActivationObject scope in ChildScopes)
            {
                scope.AutoRenameFields();
            }
        }

        internal IEnumerable<JSVariableField> GetUncrunchedLocals()
        {
            // there can't be more uncrunched fields than total fields
            var list = new List<JSVariableField>(NameTable.Count);
            foreach (var variableField in NameTable.Values)
            {
                // if the field is defined in this scope and hasn't been crunched
                // AND can still be crunched AND wasn't removed during the optimization process
                if (variableField != null && variableField.OuterField == null && variableField.CrunchedName == null
                    && variableField.CanCrunch && !variableField.WasRemoved)
                {
                    // if local renaming is not crunch all, then it must be crunch all but localization
                    // (we don't get called if we aren't crunching anything). 
                    // SO for the first clause:
                    // IF we are crunch all, we're good; but if we aren't crunch all, then we're only good if
                    //    the name doesn't start with "L_".
                    // The second clause is only computed IF we already think we're good to go.
                    // IF we aren't preserving function names, then we're good. BUT if we are, we're
                    // only good to go if this field doesn't represent a function object.
                    if ((m_settings.LocalRenaming == LocalRenaming.CrunchAll
                        || !variableField.Name.StartsWith("L_", StringComparison.Ordinal))
                        && !(m_settings.PreserveFunctionNames && variableField.IsFunction))
                    {
                        // don't add to our list if it's a function that's going to be hidden anyway
                        FunctionObject funcObject;
                        if (!variableField.IsFunction
                            || (funcObject = variableField.FieldValue as FunctionObject) == null
                            || !funcObject.HideFromOutput)
                        {
                            list.Add(variableField);
                        }
                    }
                }
            }

            if (list.Count == 0)
            {
                return null;
            }

            // sort the array and return it
            list.Sort(ReferenceComparer.Instance);
            return list;
        }

        #endregion

        #region field-management methods

        public virtual JSVariableField this[string name]
        {
            get
            {
                JSVariableField variableField;
                // check to see if this name is already defined in this scope
                if (!NameTable.TryGetValue(name, out variableField))
                {
                    // not in this scope
                    variableField = null;
                }
                return variableField;
            }
        }

        public JSVariableField FindReference(string name)
        {
            // see if we have it
            var variableField = this[name];

            // if we didn't find anything and this scope has a parent
            if (variableField == null)
            {
                if (this.Parent != null)
                {
                    // recursively go up the scope chain to find a reference,
                    // then create an inner field to point to it and we'll return
                    // that one.
                    variableField = CreateInnerField(this.Parent.FindReference(name));

                    // mark it as a placeholder. we might be going down a chain of scopes,
                    // where we will want to reserve the variable name, but not actually reference it.
                    // at the end where it is actually referenced we will reset the flag.
                    variableField.IsPlaceholder = true;
                }
                else
                {
                    // must be global scope. the field is undefined!
                    variableField = AddField(new JSVariableField(FieldType.UndefinedGlobal, name, 0, null));
                }
            }

            return variableField;
        }

        public virtual JSVariableField DeclareField(string name, object value, FieldAttributes attributes)
        {
            JSVariableField variableField;
            if (!NameTable.TryGetValue(name, out variableField))
            {
                variableField = CreateField(name, value, attributes);
                AddField(variableField);
            }
            return variableField;
        }

        public virtual JSVariableField CreateField(JSVariableField outerField)
        {
            // use the same type as the outer field by default
            return outerField.IfNotNull(o => new JSVariableField(o.FieldType, o));
        }

        public abstract JSVariableField CreateField(string name, object value, FieldAttributes attributes);

        public virtual JSVariableField CreateInnerField(JSVariableField outerField)
        {
            JSVariableField innerField = null;
            if (outerField != null)
            {
                // create a new inner field to be added to our scope
                innerField = CreateField(outerField);
                AddField(innerField);
            }

            return innerField;
        }

        internal JSVariableField AddField(JSVariableField variableField)
        {
            // add it to our name table 
            NameTable[variableField.Name] = variableField;

            // set the owning scope to this is we are the outer field, or the outer field's
            // owning scope if this is an inner field
            variableField.OwningScope = variableField.OuterField == null ? this : variableField.OuterField.OwningScope;
            return variableField;
        }

        public INameDeclaration VarDeclaredName(string name)
        {
            // check each var-decl name from inside this scope
            foreach (var varDecl in this.VarDeclaredNames)
            {
                // if the name matches, return the field
                if (string.CompareOrdinal(varDecl.Name, name) == 0)
                {
                    return varDecl;
                }
            }

            // if we get here, we didn't find a match
            return null;
        }

        public INameDeclaration LexicallyDeclaredName(string name)
        {
            // check each var-decl name from inside this scope
            foreach (var lexDecl in this.LexicallyDeclaredNames)
            {
                // if the name matches, return the field
                if (string.CompareOrdinal(lexDecl.Name, name) == 0)
                {
                    return lexDecl;
                }
            }

            // if we get here, we didn't find a match
            return null;
        }

        public void AddGlobal(string name)
        {
            // first, go up to the global scope
            var scope = this;
            while (scope.Parent != null)
            {
                scope = scope.Parent;
            }

            // now see if there is a field with that name already; 
            // will return a non-null field object if there is.
            var field = scope[name];
            if (field == null)
            {
                // nothing with this name. Add it as a global field
                scope.AddField(scope.CreateField(name, null, 0));
            }
        }

        #endregion
    }
}
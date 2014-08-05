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

namespace Microsoft.NodejsTools.Parsing
{
    public abstract class ActivationObject
    {
        private bool m_useStrict;//= false;
        private readonly ErrorSink _errorSink;
        private readonly Statement _node;
        public ActivationObject Parent { get; private set; }
        public bool IsInWithScope { get; set; }
        public IDictionary<string, JSVariableField> NameTable { get; private set; }
        public IList<ActivationObject> ChildScopes { get; private set; }
        public ICollection<Lookup> ScopeLookups { get; private set; }
        public ICollection<INameDeclaration> VarDeclaredNames { get; private set; }
        public ICollection<INameDeclaration> LexicallyDeclaredNames { get; private set; }
        public ICollection<ParameterDeclaration> GhostedCatchParameters { get; private set; }
        public ICollection<FunctionObject> GhostedFunctions { get; private set; }

        protected ActivationObject(Statement node, ActivationObject parent, ErrorSink errorSink) {
            _node = node;
            m_useStrict = false;

            Parent = parent;
            NameTable = new Dictionary<string, JSVariableField>();
            ChildScopes = new List<ActivationObject>();

            // if our parent is a scope....
            if (parent != null) {
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

            _errorSink = errorSink;
        }


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
        
        
        #endregion

        
        #region scope setup methods

        /// <summary>
        /// Set up this scope's fields from the declarations it contains
        /// </summary>
        public abstract void DeclareScope(ResolutionVisitor resolutionVisitor);

        protected void DefineLexicalDeclarations(ResolutionVisitor resolutionVisitor)
        {
            foreach (var lexDecl in LexicallyDeclaredNames)
            {
                // use the function as the field value if it's a function
                DefineField(resolutionVisitor, lexDecl);
            }
        }

        protected void DefineVarDeclarations(ResolutionVisitor resolutionVisitor)
        {
            foreach (var varDecl in VarDeclaredNames)
            {
                // var-decls are always initialized to null
                DefineField(resolutionVisitor, varDecl);
            }
        }

        private void DefineField(ResolutionVisitor resolutionVisitor, INameDeclaration nameDecl)
        {
            if (nameDecl == null) {
                // malformed code, for example catch w/o a variable.
                return;
            }
            var field = this[nameDecl.Name];
            if (nameDecl is ParameterDeclaration)
            {
                // function parameters are handled separately, so if this is a parameter declaration,
                // then it must be a catch variable. 
                if (field == null)
                {
                    // no collision - create the catch-error field
                    field = new JSVariableField(FieldType.CatchError, nameDecl.Name);

                    this.AddField(field);
                }
                else
                {
                    // it's an error to declare anything in the catch scope with the same name as the
                    // error variable
                    ErrorSink.HandleError(JSError.DuplicateCatch, nameDecl.NameSpan, resolutionVisitor._indexResolver, true);
                }
            }
            else
            {
                if (field == null)
                {
                    // could be global or local depending on the scope, so let the scope create it.
                    field = this.CreateField(nameDecl.Name);
                    
                    // if this field is a constant, mark it now
                    var lexDeclaration = nameDecl.Parent as LexicalDeclaration;

                    this.AddField(field);
                }
                else
                {
                    // already defined! 
                    // if this is a lexical declaration, then it's an error because we have two
                    // lexical declarations with the same name in the same scope.
                    if (nameDecl.Parent is LexicalDeclaration)
                    {
                        _errorSink.HandleError(JSError.DuplicateLexicalDeclaration, nameDecl.NameSpan, resolutionVisitor._indexResolver, true);
                    }
                }
            }

            nameDecl.VariableField = field;
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
                }
                else
                {
                    // must be global scope. the field is undefined!
                    variableField = AddField(new JSVariableField(FieldType.UndefinedGlobal, name));
                }
            }

            return variableField;
        }

        public virtual JSVariableField CreateField(JSVariableField outerField)
        {
            // use the same type as the outer field by default
            return outerField.IfNotNull(o => new JSVariableField(o.FieldType, o));
        }

        public abstract JSVariableField CreateField(string name);

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
            variableField.Scope = variableField.OuterField == null ? _node : variableField.OuterField.Scope;
            return variableField;
        }

        #endregion

        public ErrorSink ErrorSink {
            get {
                return _errorSink;
            }
        }
    }
}
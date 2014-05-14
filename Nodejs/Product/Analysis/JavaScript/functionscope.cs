// functionscope.cs
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
using System.Reflection;

namespace Microsoft.NodejsTools.Parsing
{
    public sealed class FunctionScope : ActivationObject
    {
        public FunctionObject FunctionObject { get; private set; }

        private HashSet<ActivationObject> m_refScopes;

        internal FunctionScope(Statement node, ActivationObject parent, bool isExpression, FunctionObject funcObj, ErrorSink errorSink)
            : base(node, parent, errorSink)
        {
            m_refScopes = new HashSet<ActivationObject>();
            if (isExpression)
            {
                // parent scopes automatically reference enclosed function expressions
                AddReference(Parent);
            }

            FunctionObject = funcObj;
        }

        #region scope setup methods

        /// <summary>
        /// Set up this scopes lexically- and var-declared fields, plus formal parameters and the arguments object
        /// </summary>
        public override void DeclareScope(ResolutionVisitor resolutionVisitor)
        {
            // we are a function expression that points to a function object. 
            // if the function object points back to us, then this is the main
            // function scope. But if it doesn't, then this is actually the parent
            // scope for named function expressions that should contain just a field
            // for the function name
            if (resolutionVisitor.GetScope(FunctionObject) == this)
            {
                // first bind any parameters
                DefineParameters();

                // bind lexical declarations next
                DefineLexicalDeclarations(resolutionVisitor);

                // bind the arguments object if this is a function scope
                DefineArgumentsObject();

                // bind the variable declarations
                DefineVarDeclarations(resolutionVisitor);

                DefineFunctionExpressionName();
            }
        }

        private void DefineFunctionExpressionName()
        {
            if (FunctionObject.Name != null) {
                // add a field for the function expression name so it can be self-referencing.
                var functionField = this.CreateField(FunctionObject.Name);
                functionField.OriginalSpan = FunctionObject.NameSpan;

                FunctionObject.VariableField = functionField;

                this.AddField(functionField);
            }
        }

        private void DefineParameters()
        {
            if (FunctionObject.ParameterDeclarations != null)
            {
                // for each parameter...
                foreach (ParameterDeclaration parameter in FunctionObject.ParameterDeclarations)
                {
                    // see if it's already defined
                    var argumentField = this[parameter.Name];
                    if (argumentField == null)
                    {
                        // not already defined -- create a field now
                        argumentField = new JSArgumentField(FieldType.Argument, parameter.Name, parameter.Position)
                        {
                            OriginalSpan = parameter.Span,
                        };

                        this.AddField(argumentField);
                    }

                    // make the parameter reference the field and the field reference
                    // the parameter as its declaration
                    parameter.VariableField = argumentField;
                }
            }
        }

        private void DefineArgumentsObject()
        {
            // this one is easy: if it's not already defined, define it now
            const string name = "arguments";
            if (this[name] == null)
            {
                this.AddField(new JSVariableField(FieldType.Arguments, name));
            }
        }

        #endregion

        public override JSVariableField CreateField(string name)
        {
            return new JSVariableField(FieldType.Local, name);
        }

        internal void AddReference(ActivationObject scope)
        {
            // we don't want to include block scopes or with scopes -- they are really
            // contained within their parents
            while (scope != null && scope is BlockScope)
            {
                scope = scope.Parent;
            }

            if (scope != null)
            {
                // add the scope to the hash
                m_refScopes.Add(scope);
            }
        }
    }
}

// variabledeclaration.cs
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

using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Ajax.Utilities
{
    public sealed class VariableDeclaration : AstNode, INameDeclaration, INameReference
    {
        private AstNode m_initializer;

        public AstNode Initializer
        {
            get { return m_initializer; }
            set
            {
                m_initializer.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_initializer = value;
                m_initializer.IfNotNull(n => n.Parent = this);
            }
        }

        public string Identifier { get; set; }
        public Context NameContext { get; set; }

        public Context AssignContext { get; set; }

        public bool HasInitializer { get { return Initializer != null; } }

        public JSVariableField VariableField { get; set; }
        public bool IsCCSpecialCase { get; set; }
        public bool UseCCOn { get; set; }

        public string Name
        {
            get { return Identifier; }
        }

        public bool RenameNotAllowed
        {
            get
            {
                return VariableField == null ? true : !VariableField.CanCrunch;
            }
        }

        private bool m_isGenerated;
        public bool IsGenerated
        {
            get { return m_isGenerated; }
            set
            {
                m_isGenerated = value;
                if (VariableField != null)
                {
                    VariableField.IsGenerated = m_isGenerated;
                }
            }
        }

        public bool IsAssignment
        {
            get
            {
                // if there is an initializer, we are an assignment
                return Initializer != null;
            }
        }

        public AstNode AssignmentValue
        {
            get
            {
                return Initializer;
            }
        }

        public VariableDeclaration(Context context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override bool IsExpression
        {
            get
            {
                // sure. treat a vardecl like an expression. normally this wouldn't be anywhere but
                // in a var statement, but sometimes the special-cc case might be moved into an expression
                // statement
                return true;
            }
        }

        internal override string GetFunctionGuess(AstNode target)
        {
            return Identifier;
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Initializer);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Initializer == oldNode)
            {
                Initializer = newNode;
                return true;
            }
            return false;
        }

        public override bool IsEquivalentTo(AstNode otherNode)
        {
            JSVariableField otherField = null;
            Lookup otherLookup;
            var otherVarDecl = otherNode as VariableDeclaration;
            if (otherVarDecl != null)
            {
                otherField = otherVarDecl.VariableField;
            }
            else if ((otherLookup = otherNode as Lookup) != null)
            {
                otherField = otherLookup.VariableField;
            }

            // if we get here, we're not equivalent
            return this.VariableField != null && this.VariableField.IsSameField(otherField);
        }

        #region INameReference Members

        public ActivationObject VariableScope
        {
            get
            {
                // if we don't have a field, return null. Otherwise it's the field's owning scope.
                return this.VariableField.IfNotNull(f => f.OwningScope);
            }
        }

        #endregion
    }
}

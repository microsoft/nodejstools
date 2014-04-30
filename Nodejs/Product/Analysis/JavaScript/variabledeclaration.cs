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

namespace Microsoft.NodejsTools.Parsing
{
    public sealed class VariableDeclaration : Node, INameDeclaration, INameReference
    {
        private Expression m_initializer;
        public string Identifier { get; set; }
        public IndexSpan NameSpan { get; set; }
        public JSVariableField VariableField { get; set; }

        public VariableDeclaration(IndexSpan span)
            : base(span) {
        }

        public Expression Initializer
        {
            get { return m_initializer; }
            set
            {
                m_initializer.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_initializer = value;
                m_initializer.IfNotNull(n => n.Parent = this);
            }
        }

        public bool HasInitializer { get { return Initializer != null; } }

        public string Name
        {
            get { return Identifier; }
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                if (m_initializer != null) {
                    m_initializer.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        internal override string GetFunctionGuess(Node target)
        {
            return Identifier;
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Initializer);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Initializer == oldNode)
            {
                Initializer = (Expression)newNode;
                return true;
            }
            return false;
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

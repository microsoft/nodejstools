// member.cs
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
using System.Text;

namespace Microsoft.Ajax.Utilities
{

    public sealed class Member : Expression
    {
        private AstNode m_root;

        public AstNode Root
        {
            get { return m_root; }
            set
            {
                m_root.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_root = value;
                m_root.IfNotNull(n => n.Parent = this);
            }
        }

        public string Name { get; set; }
        public Context NameContext { get; set; }

        public Member(Context context, JSParser parser)
            : base(context, parser)
        {
        }

        public override OperatorPrecedence Precedence
        {
            get
            {
                return OperatorPrecedence.FieldAccess;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override bool IsEquivalentTo(AstNode otherNode)
        {
            var otherMember = otherNode as Member;
            return otherMember != null
                && string.CompareOrdinal(this.Name, otherMember.Name) == 0
                && this.Root.IsEquivalentTo(otherMember.Root);
        }

        internal override string GetFunctionGuess(AstNode target)
        {
            return Root.GetFunctionGuess(this) + '.' + Name;
        }

        internal override bool IsDebuggerStatement
        {
            get
            {
                // depends on whether the root is
                return Root.IsDebuggerStatement;
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Root);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Root == oldNode)
            {
                Root = newNode;
                return true;
            }
            return false;
        }

        public override AstNode LeftHandSide
        {
            get
            {
                // the root object is on the left
                return Root.LeftHandSide;
            }
        }
    }
}

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

namespace Microsoft.NodejsTools.Parsing
{

    public sealed class Member : Expression
    {
        private Expression m_root;

        public Expression Root
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
        public TokenWithSpan NameContext { get; set; }

        public Member(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_root.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        internal override string GetFunctionGuess(Node target)
        {
            return Root.GetFunctionGuess(this) + '.' + Name;
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Root);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Root == oldNode)
            {
                Root = (Expression)newNode;
                return true;
            }
            return false;
        }
    }
}

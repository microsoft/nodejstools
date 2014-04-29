// switch.cs
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
    public sealed class Switch : Statement
    {
        private Expression m_expression;
        private AstNodeList<SwitchCase> m_cases;

        public Expression Expression
        {
            get { return m_expression; }
            set
            {
                m_expression.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_expression = value;
                m_expression.IfNotNull(n => n.Parent = this);
            }
        }

        public AstNodeList<SwitchCase> Cases
        {
            get { return m_cases; }
            set
            {
                m_cases.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_cases = value;
                m_cases.IfNotNull(n => n.Parent = this);
            }
        }

        public bool BraceOnNewLine { get; set; }
        public TokenWithSpan BraceContext { get; set; }

        public ActivationObject BlockScope { get; set; }

        public Switch(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                Expression.Walk(visitor);
                foreach (var switchCase in m_cases) {
                    switchCase.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Expression, Cases);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Expression == oldNode)
            {
                Expression = (Expression)newNode;
                return true;
            }
            if (Cases == oldNode)
            {
                AstNodeList<SwitchCase> newList = newNode as AstNodeList<SwitchCase>;
                if (newNode == null || newList != null)
                {
                    // remove it
                    Cases = newList;
                    return true;
                }
            }

            return false;
        }
    }
}

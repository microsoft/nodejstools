// grouping.cs
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

namespace Microsoft.NodejsTools.Parsing
{
    using System.Collections.Generic;

    /// <summary>
    /// Implementation of parenthetical '(' expr ')' operators
    /// </summary>
    public class GroupingOperator : Expression
    {
        private Expression m_operand;

        public Expression Operand
        {
            get { return m_operand; }
            set
            {
                m_operand.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_operand = value;
                m_operand.IfNotNull(n => n.Parent = this);
            }
        }

        public GroupingOperator(IndexSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_operand.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Operand);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Operand == oldNode)
            {
                Operand = (Expression)newNode;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return '(' + (Operand == null ? "<null>" : Operand.ToString()) + ')';
        }
    }
}

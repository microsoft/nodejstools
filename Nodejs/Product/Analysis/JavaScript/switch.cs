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

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    [Serializable]
    public sealed class Switch : Statement
    {
        private Expression m_expression;
        private AstNodeList<SwitchCase> m_cases;

        public Expression Expression
        {
            get { return m_expression; }
            set
            {
                m_expression.ClearParent(this);
                m_expression = value;
                m_expression.AssignParent(this);
            }
        }

        public AstNodeList<SwitchCase> Cases
        {
            get { return m_cases; }
            set
            {
                m_cases.ClearParent(this);
                m_cases = value;
                m_cases.AssignParent(this);
            }
        }

        public Switch(IndexSpan span)
            : base(span)
        {
        }

        /// <summary>
        /// Gets the index where the switch block starts
        /// </summary>
        public int BlockStart {
            get;
            set;
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
    }
}

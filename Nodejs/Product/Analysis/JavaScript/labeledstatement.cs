// labeledstatement.cs
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
    public sealed class LabeledStatement : Statement
    {
        private Statement m_statement;

        public Statement Statement
        {
            get { return m_statement; }
            set
            {
                m_statement.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_statement = value;
                m_statement.IfNotNull(n => n.Parent = this);
            }
        }

        public int NestCount { get; set; }
        public string Label { get; set; }

        public LabeledStatement(IndexSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                if (m_statement != null) {
                    m_statement.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Statement);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Statement == oldNode)
            {
                Statement = (Statement)newNode;
                return true;
            }
            return false;
        }
    }
}

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

namespace Microsoft.Ajax.Utilities
{
    public sealed class LabeledStatement : AstNode
    {
        private AstNode m_statement;

        public AstNode Statement
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

        public Context ColonContext { get; set; }

        public LabeledStatement(Context context, JSParser parser)
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

        internal override bool RequiresSeparator
        {
            get
            {
                // requires a separator if the statement does
                return (Statement != null ? Statement.RequiresSeparator : false);
            }
        }

        public override AstNode LeftHandSide
        {
            get
            {
                // the label is on the left, but it's sorta ignored
                return (Statement != null ? Statement.LeftHandSide : null);
            }
        }

        internal override bool EncloseBlock(EncloseBlockType type)
        {
            // pass the query on to the statement
            return (Statement != null ? Statement.EncloseBlock(type) : false);
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Statement);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Statement == oldNode)
            {
                Statement = newNode;
                return true;
            }
            return false;
        }
    }
}

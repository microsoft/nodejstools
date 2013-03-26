// cccomment.cs
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

namespace Microsoft.Ajax.Utilities
{
    public class ConditionalCompilationComment : AstNode
    {
        private Block m_statements;
        public Block Statements
        {
            get { return m_statements; }
            set
            {
                m_statements.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_statements = value;
                m_statements.IfNotNull(n => n.Parent = this);
            }
        }

        public ConditionalCompilationComment(Context context, JSParser parser)
            : base(context, parser)
        {
            Statements = new Block(null, parser);
        }


        internal override bool RequiresSeparator
        {
            get
            {
                return Statements.Count > 0 ? Statements[Statements.Count - 1].RequiresSeparator : true;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public void Append(AstNode statement)
        {
            if (statement != null)
            {
                Context.UpdateWith(statement.Context);
                Statements.Append(statement);
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Statements);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Statements == oldNode)
            {
                Statements = ForceToBlock(newNode);
                return true;
            }

            return false;
        }
    }
}

// switchcase.cs
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
    public sealed class SwitchCase : Statement
    {
        private Expression m_caseValue;
        private Block m_statements;

        public Expression CaseValue
        {
            get { return m_caseValue; }
            set
            {
                m_caseValue.ClearParent(this);
                m_caseValue = value;
                m_caseValue.AssignParent(this);
            }
        }

        public Block Statements
        {
            get { return m_statements; }
            set
            {
                m_statements.ClearParent(this);
                m_statements = value;
                m_statements.AssignParent(this);
            }
        }

        internal bool IsDefault
        {
            get { return (CaseValue == null); }
        }

        public SwitchCase(IndexSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                if (m_caseValue != null) {
                    m_caseValue.Walk(visitor);
                }
                m_statements.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(CaseValue, Statements);
            }
        }
    }
}
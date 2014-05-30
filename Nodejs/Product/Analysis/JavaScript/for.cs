// for.cs
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

    public sealed class ForNode : IterationStatement
    {
        private Statement m_initializer;
        private Expression m_condition;
        private Expression m_incrementer;

        public Statement Initializer
        {
            get { return m_initializer; }
            set
            {
                m_initializer.ClearParent(this);
                m_initializer = value;
                m_initializer.AssignParent(this);
            }
        }

        public Expression Condition
        {
            get { return m_condition; }
            set
            {
                m_condition.ClearParent(this);
                m_condition = value;
                m_condition.AssignParent(this);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Incrementer")]
        public Expression Incrementer
        {
            get { return m_incrementer; }
            set
            {
                m_incrementer.ClearParent(this);
                m_incrementer = value;
                m_incrementer.AssignParent(this);
            }
        }

        public ForNode(IndexSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                if (m_initializer != null) {
                    m_initializer.Walk(visitor);
                }
                if (m_condition != null) {
                    m_condition.Walk(visitor);
                }
                if (Body != null) {
                    Body.Walk(visitor);
                }
                if (m_incrementer != null) {
                    m_incrementer.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Initializer, Condition, Incrementer, Body);
            }
        }
    }
}

// if.cs
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

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    internal sealed class IfNode : Statement
    {
        private Expression m_condition;
        private Block m_trueBlock;
        private Block m_falseBlock;

        public Expression Condition
        {
            get { return m_condition; }
            set
            {
                m_condition = value;
            }
        }

        public Block TrueBlock
        {
            get { return m_trueBlock; }
            set
            {
                m_trueBlock.ClearParent(this);
                m_trueBlock = value;
                m_trueBlock.AssignParent(this);
            }
        }

        public Block FalseBlock
        {
            get { return m_falseBlock; }
            set
            {
                m_falseBlock.ClearParent(this);
                m_falseBlock = value;
                m_falseBlock.AssignParent(this);
            }
        }

        public int ElseStart {
            get;
            set;
        }

        public IfNode(EncodedSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_condition.Walk(visitor);
                if (TrueBlock != null) {
                    TrueBlock.Walk(visitor);
                }
                if (FalseBlock != null) {
                    FalseBlock.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Condition, TrueBlock, FalseBlock);
            }
        }
    }
}
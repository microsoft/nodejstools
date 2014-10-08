// try.cs
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
    internal sealed class TryNode : Statement
    {
        private Block m_tryBlock;
        private Block m_catchBlock;
        private Block m_finallyBlock;
        private ParameterDeclaration m_catchParameter;

        public Block TryBlock
        {
            get { return m_tryBlock; }
            set
            {
                m_tryBlock.ClearParent(this);
                m_tryBlock = value;
                m_tryBlock.AssignParent(this);
            }
        }

        public Block CatchBlock
        {
            get { return m_catchBlock; }
            set
            {
                m_catchBlock.ClearParent(this);
                m_catchBlock = value;
                m_catchBlock.AssignParent(this);
            }
        }

        /// <summary>
        /// Gets the index where the catch keyword begins
        /// </summary>
        public int CatchStart {
            get;
            set;
        }

        public Block FinallyBlock
        {
            get { return m_finallyBlock; }
            set
            {
                m_finallyBlock.ClearParent(this);
                m_finallyBlock = value;
                m_finallyBlock.AssignParent(this);
            }
        }

        /// <summary>
        /// Gets the index where the finally keyword begins
        /// </summary>
        public int FinallyStart {
            get;
            set;
        }

        public ParameterDeclaration CatchParameter
        {
            get { return m_catchParameter; }
            set
            {
                m_catchParameter.ClearParent(this);
                m_catchParameter = value;
                m_catchParameter.AssignParent(this);
            }
        }

        public string CatchVarName
        {
            get
            {
                return CatchParameter.IfNotNull(v => v.Name);
            }
        }

        public TryNode(EncodedSpan span)
            : base(span)
        {
        }

        public void SetCatchVariable(JSVariableField field)
        {
            CatchParameter.VariableField = field;
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_tryBlock.Walk(visitor);
                if (m_catchParameter != null) {
                    m_catchParameter.Walk(visitor);
                }
                if (m_catchBlock != null) {
                    m_catchBlock.Walk(visitor);
                }
                if (m_finallyBlock != null) {
                    m_finallyBlock.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(TryBlock, CatchParameter, CatchBlock, FinallyBlock);
            }
        }
    }
}

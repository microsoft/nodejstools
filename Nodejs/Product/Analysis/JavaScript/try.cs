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

using System.Collections.Generic;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    public sealed class TryNode : Statement
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
                m_tryBlock.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_tryBlock = value;
                m_tryBlock.IfNotNull(n => n.Parent = this);
            }
        }

		public Block CatchBlock
        {
            get { return m_catchBlock; }
            set
            {
                m_catchBlock.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_catchBlock = value;
                m_catchBlock.IfNotNull(n => n.Parent = this);
            }
        }

		public Block FinallyBlock
        {
            get { return m_finallyBlock; }
            set
            {
                m_finallyBlock.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_finallyBlock = value;
                m_finallyBlock.IfNotNull(n => n.Parent = this);
            }
        }

        public ParameterDeclaration CatchParameter
        {
            get { return m_catchParameter; }
            set
            {
                m_catchParameter.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_catchParameter = value;
                m_catchParameter.IfNotNull(n => n.Parent = this);
            }
        }

        public string CatchVarName
        {
            get
            {
                return CatchParameter.IfNotNull(v => v.Name);
            }
        }

        public TokenWithSpan CatchContext { get; set; }

        public TokenWithSpan CatchVarContext
        {
            get
            {
                return CatchParameter.IfNotNull(v => v.Context);
            }
        }

        public TokenWithSpan FinallyContext { get; set; }

        public TryNode(TokenWithSpan context, JSParser parser)
            : base(context, parser)
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

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (TryBlock == oldNode)
            {
                TryBlock = ForceToBlock((Statement)newNode);
                return true;
            }
            if (CatchParameter == oldNode)
            {
                CatchParameter = newNode as ParameterDeclaration;
                return true;
            }
            if (CatchBlock == oldNode)
            {
                CatchBlock = ForceToBlock((Statement)newNode);
                return true;
            }
            if (FinallyBlock == oldNode)
            {
                FinallyBlock = ForceToBlock((Statement)newNode);
                return true;
            }
            return false;
        }
    }
}

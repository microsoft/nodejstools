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
                m_initializer.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_initializer = value;
                m_initializer.IfNotNull(n => n.Parent = this);
            }
        }

        public Expression Condition
        {
            get { return m_condition; }
            set
            {
                m_condition.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_condition = value;
                m_condition.IfNotNull(n => n.Parent = this);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Incrementer")]
        public Expression Incrementer
        {
            get { return m_incrementer; }
            set
            {
                m_incrementer.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_incrementer = value;
                m_incrementer.IfNotNull(n => n.Parent = this);
            }
        }

        /// <summary>Context for the first semicolon, separating the initializer and the condition</summary>
        public TokenWithSpan Separator1Context { get; set; }

        /// <summary>Context for the second semicolon, separating the condition and the incrementor</summary>
        public TokenWithSpan Separator2Context { get; set; }

        public BlockScope BlockScope { get; set; }

        public override TokenWithSpan TerminatingContext
        {
            get
            {
                // if we have one, return it. If not, return what the body has (if any)
                return base.TerminatingContext ?? Body.IfNotNull(b => b.TerminatingContext);
            }
        }

        public ForNode(TokenWithSpan context, JSParser parser)
            : base(context, parser)
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

		internal override bool RequiresSeparator
        {
            get
            {
                // requires a separator if the body does
                return Body == null ? false : Body.RequiresSeparator;
            }
        }

        internal override bool EncloseBlock(EncloseBlockType type)
        {
            // pass the query on to the body
            return Body == null || Body.Count == 0 ? false : Body.EncloseBlock(type);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Initializer, Condition, Incrementer, Body);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Initializer == oldNode)
            {
                Initializer = (Statement)newNode;
                return true;
            }
            if (Condition == oldNode)
            {
                Condition = (Expression)newNode;
                return true;
            }
            if (Incrementer == oldNode)
            {
                Incrementer = (Expression)newNode;
                return true;
            }
            if (Body == oldNode)
            {
                Body = ForceToBlock((Statement)newNode);
                return true;
            }
            return false;
        }
    }
}

// with.cs
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
    internal sealed class WithNode : Statement
    {
        private Expression m_withObject;
        private Block m_body;

        public Expression WithObject
        {
            get { return m_withObject; }
            set
            {
                m_withObject = value;
            }
        }

        public Block Body
        {
            get { return m_body; }
            set
            {
                m_body.ClearParent(this);
                m_body = value;
                m_body.AssignParent(this);
            }
        }

        public WithNode(EncodedSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_withObject.Walk(visitor);
                m_body.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(WithObject, Body);
            }
        }
    }
}
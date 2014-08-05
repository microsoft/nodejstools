// forin.cs
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
    public sealed class ForIn : IterationStatement
    {
        private Statement m_variable;
        private Expression m_collection;

        public Statement Variable
        {
            get { return m_variable; }
            set
            {
                m_variable.ClearParent(this);
                m_variable = value;
                m_variable.AssignParent(this);
            }
        }

        public Expression Collection
        {
            get { return m_collection; }
            set
            {
                m_collection = value;
            }
        }

        public ForIn(IndexSpan span)
            : base(span)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_collection.Walk(visitor);
                m_variable.Walk(visitor);
                Body.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Variable, Collection, Body);
            }
        }
    }
}

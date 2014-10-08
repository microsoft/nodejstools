// variabledeclaration.cs
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
using System.Reflection;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    [Serializable]
    internal sealed class VariableDeclaration : Statement, INameDeclaration, INameReference
    {
        private Expression m_initializer;
        public string Identifier { get; set; }
        public IndexSpan NameSpan { get; set; }
        public JSVariableField VariableField { get; set; }

        public VariableDeclaration(EncodedSpan span)
            : base(span) {
        }

        public IndexSpan GetNameSpan(LocationResolver locationResolver) {
            return NameSpan;
        }

        public Expression Initializer
        {
            get { return m_initializer; }
            set
            {
                m_initializer = value;
            }
        }

        public bool HasInitializer { get { return Initializer != null; } }

        public string Name
        {
            get { return Identifier; }
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                if (m_initializer != null) {
                    m_initializer.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Initializer);
            }
        }
    }
}

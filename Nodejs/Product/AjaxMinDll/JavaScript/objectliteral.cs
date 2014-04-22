// objectliteral.cs
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
    public sealed class ObjectLiteral : Expression
    {
        private AstNodeList<ObjectLiteralProperty> m_properties;

        public AstNodeList<ObjectLiteralProperty> Properties
        {
            get { return m_properties; }
            set
            {
                m_properties.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_properties = value;
                m_properties.IfNotNull(n => n.Parent = this);
            }
        }

        public override bool IsConstant
        {
            get
            {
                // we are NOT constant if any one property value isn't constant.
                // no properties means an empty object literal, which is constant.
                if (Properties != null)
                {
                    foreach (var property in Properties)
                    {
                        if (!property.IsConstant)
                        {
                            return false;
                        }
                    }
                }

                // if we got here, they're all constant
                return true;
            }
        }

        public ObjectLiteral(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                foreach (var prop in m_properties) {
                    prop.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(m_properties);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (oldNode == m_properties)
            {
                var properties = newNode as AstNodeList<ObjectLiteralProperty>;
                if (newNode == null || properties != null)
                {
                    Properties = properties;
                }
            }
            return false;
        }
    }
}


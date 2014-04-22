// ObjectLiteralProperty.cs
//
// Copyright 2012 Microsoft Corporation
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
    public class ObjectLiteralProperty : Node
    {
        private ObjectLiteralField m_propertyName;
        private Expression m_propertyValue;

        public ObjectLiteralField Name
        {
            get { return m_propertyName; }
            set
            {
                m_propertyName.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_propertyName = value;
                m_propertyName.IfNotNull(n => n.Parent = this);
            }
        }

        public Expression Value
        {
            get { return m_propertyValue; }
            set
            {
                m_propertyValue.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_propertyValue = value;
                m_propertyValue.IfNotNull(n => n.Parent = this);
            }
        }

        public override bool IsConstant
        {
            get
            {
                // we are constant if our value is constant.
                // If we don't have a value, then assume it's constant?
                return Value != null ? Value.IsConstant : true;
            }
        }

        public ObjectLiteralProperty(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_propertyName.Walk(visitor);
                m_propertyValue.Walk(visitor);
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Name, Value);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Name == oldNode)
            {
                var objectField = newNode as ObjectLiteralField;
                if (newNode == null || objectField != null)
                {
                    Name = objectField;
                }
                return true;
            }

            if (Value == oldNode)
            {
                Value = (Expression)newNode;
                return true;
            }

            return false;
        }

        internal override string GetFunctionGuess(Node target)
        {
            return Name.ToString();
        }
    }
}

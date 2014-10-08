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

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    [Serializable]
    internal class ObjectLiteralProperty : Node
    {
        private ObjectLiteralField m_propertyName;
        private Expression m_propertyValue;

        public ObjectLiteralField Name
        {
            get { return m_propertyName; }
            set
            {
                m_propertyName = value;
            }
        }

        public Expression Value
        {
            get { return m_propertyValue; }
            set
            {
                m_propertyValue = value;
            }
        }

        public ObjectLiteralProperty(EncodedSpan span)
            : base(span)
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
    }
}

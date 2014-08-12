// arrayliteral.cs
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
using System.Globalization;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    [Serializable]
    public sealed class ArrayLiteral : Expression
    {
        private Expression[] _elements;

        public Expression[] Elements 
        {
            get { return _elements; }
            set
            {
                _elements = value;
            }
        }

        public ArrayLiteral(EncodedSpan span)
            : base(span)
        {
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Elements);
            }
        }

        public override void Walk(AstVisitor walker) {
            if (walker.Walk(this)) {
                foreach (var node in _elements) {
                    node.Walk(walker);
                }
            }
        }
    }
}

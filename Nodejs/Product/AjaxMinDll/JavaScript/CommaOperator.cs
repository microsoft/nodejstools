// binaryop.cs
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
    public class CommaOperator : Expression
    {
        private Expression[] _expressions;

        public CommaOperator(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override PrimitiveType FindPrimitiveType() {
            return _expressions[_expressions.Length - 1].FindPrimitiveType();
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return _expressions;
            }
        }

        public Expression[] Expressions
        {
            get
            {
                return _expressions;
            }
            set
            {
                _expressions = value;
            }
        }

        public static Expression CombineWithComma(TokenWithSpan context, JSParser parser, Expression operand1, Expression operand2)
        {
            var comma = new CommaOperator(context, parser);

            List<Expression> res = new List<Expression>();
            CommaOperator left = operand1 as CommaOperator;
            CommaOperator right = operand2 as CommaOperator;
            if (left != null)
            {
                res.AddRange(left.Expressions);
            }
            else
            {
                res.Add(operand1);
            }

            if (right != null)
            {
                res.AddRange(right.Expressions);
            }
            else
            {
                res.Add(operand2);
            }
            comma.Expressions = res.ToArray();
            return comma;
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                foreach (var expr in _expressions) {
                    expr.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }
    }
}

using Microsoft.NodejsTools.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    public class FunctionExpression : Expression
    {
        private FunctionObject _function;

        public FunctionExpression(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public FunctionObject Function
        {
            get
            {
                return _function;
            }
            set
            {
                _function = value;
                _function.Parent = this;
            }
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                _function.Walk(visitor);
            }
            visitor.PostWalk(this);
        }
    }
}

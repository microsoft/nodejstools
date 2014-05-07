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

        public FunctionExpression(IndexSpan span)
            : base(span)
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
                _function.IsExpression = true;
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

using Microsoft.NodejsTools.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
  public class ExpressionStatement : Statement
  {
    private Expression _expression;

    public ExpressionStatement(TokenWithSpan context, JSParser parser)
      : base(context, parser)
    {
      
    }

    public Expression Expression
    {
      get
      {
        return _expression;
      }
      set
      {
        _expression = value;
        _expression.Parent = this;
      }
    }

    public override void Walk(AstVisitor visitor) {
        if (visitor.Walk(this)) {
            _expression.Walk(visitor);
        }
        visitor.PostWalk(this);
    }
  }
}

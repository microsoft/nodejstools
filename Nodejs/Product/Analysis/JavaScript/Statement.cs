using Microsoft.NodejsTools.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    public abstract class Statement : Node
    {
        protected Statement(IndexSpan span)
            : base(span)
        {
        }

        public static Expression GetExpression(Statement statement)
        {
            if (statement is Block)
            {
                if (((Block)statement).Count == 1)
                {
                    return GetExpression(((Block)statement)[0]);
                }
            }
            else if (statement is ExpressionStatement)
            {
                var exprStmt = (ExpressionStatement)statement;
                return exprStmt.Expression;
            }
            else if (statement is ReturnNode)
            {
                return ((ReturnNode)statement).Operand;
            }
            return null;
        }

    }
}

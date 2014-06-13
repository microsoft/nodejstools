/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;

namespace Microsoft.NodejsTools.Parsing
{
    [Serializable]
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

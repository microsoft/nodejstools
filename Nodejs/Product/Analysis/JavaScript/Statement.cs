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
using System.Diagnostics;

namespace Microsoft.NodejsTools.Parsing
{
    [Serializable]
    public abstract class Statement : Node
    {
        protected Statement(EncodedSpan location)
            : base(location) {
            EncodedSpan = location;
        }

        /// <summary>
        /// Gets or sets the parent node of this node in the abstract syntax tree
        /// </summary>
        public Statement Parent { get; set; }

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

        private JsAst GlobalParent {
            get {
                var res = this;
                while (res != null && !(res is JsAst)) {
                    res = res.Parent;
                }
                return (JsAst)res;
            }
        }

        public override string ToString() {
            if (GlobalParent != null) {
                return String.Format("{0} {1} {2}", GetType().Name, GetStart(GlobalParent.LocationResolver), GetEnd(GlobalParent.LocationResolver));
            }
            return base.ToString();
        }
    }
}

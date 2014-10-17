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
    internal class FunctionExpression : Expression
    {
        private FunctionObject _function;

        public FunctionExpression(EncodedSpan span)
            : base(span) {
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

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
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    internal class ExpressionStatement : Statement {
        private Expression _expression;

        public ExpressionStatement(EncodedSpan location)
            : base(location) {

        }

        public override IEnumerable<Node> Children {
            get {
                return new[] { Expression };
            }
        }

        public Expression Expression {
            get {
                return _expression;
            }
            set {
                _expression = value;
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

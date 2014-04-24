// lookup.cs
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

namespace Microsoft.NodejsTools.Parsing
{
    public enum ReferenceType
    {
        Variable,
        Function,
        Constructor
    }


    public sealed class Lookup : Expression, INameReference
    {
        public JSVariableField VariableField { get; internal set; }

        public bool IsGenerated { get; set; }
        public ReferenceType RefType { get; set; }
        public string Name { get; set; }

        public bool IsAssignment
        {
            get
            {
                var isAssign = false;

                // see if our parent is a binary operator.
                var binaryOp = Parent as BinaryOperator;
                if (binaryOp != null)
                {
                    // if we are, we are an assignment lookup if the binary operator parent is an assignment
                    // and we are the left-hand side.
                    isAssign = binaryOp.IsAssign && binaryOp.Operand1 == this;
                }
                else
                {
                    // not a binary op -- but we might still be an "assignment" if we are an increment or decrement operator.
                    var unaryOp = Parent as UnaryOperator;
                    isAssign = unaryOp != null
                        && (unaryOp.OperatorToken == JSToken.Increment || unaryOp.OperatorToken == JSToken.Decrement);

                    if (!isAssign)
                    {
                        // AND if we are the variable of a for-in statement, we are an "assignment".
                        // (if the forIn variable is a var, then it wouldn't be a lookup, so we don't have to worry about
                        // going up past a var-decl intermediate node)
                        var forIn = Parent as ForIn;
                        isAssign = forIn != null && this == Statement.GetExpression(forIn.Variable);
                    }
                }

                return isAssign;
            }
        }

        public Node AssignmentValue
        {
            get
            {
                Node value = null;

                // see if our parent is a binary operator.
                var binaryOp = Parent as BinaryOperator;
                if (binaryOp != null)
                {
                    // the parent is a binary operator. If it is an assignment operator 
                    // (not including any of the op-assign which depend on an initial value)
                    // then the value we are assigning is the right-hand side of the = operator.
                    value = binaryOp.OperatorToken == JSToken.Assign && binaryOp.Operand1 == this ? binaryOp.Operand2 : null;
                }

                return value;
            }
        }

        public Lookup(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
            RefType = ReferenceType.Variable;
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
            }
            visitor.PostWalk(this);
        }

        internal override string GetFunctionGuess(Node target)
        {
            // return the source name
            return Name;
        }

        private static bool MatchMemberName(Node node, string lookup, int startIndex, int endIndex)
        {
            // the node needs to be a Member node, and if it is, the appropriate portion of the lookup
            // string should match the name of the member.
            var member = node as Member;
            return member != null && string.CompareOrdinal(member.Name, 0, lookup, startIndex, endIndex - startIndex) == 0;
        }

        private static bool MatchesMemberChain(Node parent, string lookup, int startIndex)
        {
            // get the NEXT period
            var period = lookup.IndexOf('.', startIndex);

            // loop until we run out of periods
            while (period > 0)
            {
                // if the parent isn't a member, or if the name of the parent doesn't match
                // the current identifier in the chain, then we're no match and can bail
                if (!MatchMemberName(parent, lookup, startIndex, period))
                {
                    return false;
                }

                // next parent, next segment, and find the next period
                parent = parent.Parent;
                startIndex = period + 1;
                period = lookup.IndexOf('.', startIndex);
            }

            // now check the last segment, from start to the end of the string
            return MatchMemberName(parent, lookup, startIndex, lookup.Length);
        }

        //code in parser relies on this.name being returned from here
        public override String ToString()
        {
            return Name;
        }

        #region INameReference Members

        public ActivationObject VariableScope
        {
            get
            {
                // get the enclosing scope from the node, but that might be 
                // a block scope -- we only want variable scopes: functions or global.
                // so walk up until we find one.
                var enclosingScope = this.EnclosingScope;
                while (enclosingScope is BlockScope)
                {
                    enclosingScope = enclosingScope.Parent;
                }

                return enclosingScope;
            }
        }

        #endregion
    }
}

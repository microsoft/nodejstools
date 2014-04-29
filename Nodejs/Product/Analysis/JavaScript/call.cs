// call.cs
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
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    public sealed class CallNode : Expression
    {
        private Expression m_function;
        private AstNodeList<Expression> m_arguments;

        public Expression Function
        {
            get { return m_function; }
            set
            {
                m_function.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_function = value;
                m_function.IfNotNull(n => n.Parent = this);
            }
        }

        public AstNodeList<Expression> Arguments
        {
            get { return m_arguments; }
            set
            {
                m_arguments.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_arguments = value;
                m_arguments.IfNotNull(n => n.Parent = this);
            }
        }

        public bool IsConstructor
        {
            get;
            set;
        }

        public bool InBrackets
        {
            get;
            set;
        }

        public CallNode(TokenWithSpan context, JSParser parser)
            : base(context, parser)
        {
        }

        public override OperatorPrecedence Precedence
        {
            get
            {
                // new-operator is the unary precedence; () and [] are  field access
                return /*IsConstructor ? OperatorPrecedence.Unary :*/ OperatorPrecedence.FieldAccess;
            }
        }

        public override bool IsExpression
        {
            get
            {
                // normall this would be an expression. BUT we want to check for a
                // call to a member function that is in the "onXXXX" pattern and passing
                // parameters. This is because of a bug in IE that will throw a script error 
                // if you call a native event handler like onclick and pass in a parameter
                // IN A LOGICAL EXPRESSION. For some reason, the simple statement:
                // elem.onclick(e) will work, but elem&&elem.onclick(e) will not. So treat
                // calls to any member operator where the property name starts with "on" and
                // we are passing in arguments as if it were NOT an expression, and it won't
                // get combined.
                Member callMember = Function as Member;
                if (callMember != null
                    && callMember.Name.StartsWith("on", StringComparison.Ordinal)
                    && Arguments.Count > 0)
                {
                    // popped positive -- don't treat it like an expression.
                    return false;
                }

                // otherwise it's okay -- it's an expression and can be combined.
                return true;
            }
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_function.Walk(visitor);
                foreach (var param in m_arguments) {
                    param.Walk(visitor);
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(Function, Arguments);
            }
        }

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            if (Function == oldNode)
            {
                Function = (Expression)newNode;
                return true;
            }
            if (Arguments == oldNode)
            {
                if (newNode == null)
                {
                    // remove it
                    Arguments = null;
                    return true;
                }
                else
                {
                    // if the new node isn't an AstNodeList, ignore it
                    var newList = newNode as AstNodeList<Expression>;
                    if (newList != null)
                    {
                        Arguments = newList;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
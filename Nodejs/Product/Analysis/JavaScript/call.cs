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
    [Serializable]
    internal sealed class CallNode : Expression
    {
        private Expression m_function;
        private Expression[] m_arguments;

        public Expression Function
        {
            get { return m_function; }
            set
            {
                m_function = value;
            }
        }

        public Expression[] Arguments
        {
            get { return m_arguments; }
            set
            {
                m_arguments = value;
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

        public CallNode(EncodedSpan span)
            : base(span) {
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                m_function.Walk(visitor);
                foreach (var param in m_arguments) {
                    if (param != null) {
                        param.Walk(visitor);
                    }
                }
            }
            visitor.PostWalk(this);
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                if (Function != null) {
                    yield return Function;
                }
                foreach (var arg in Arguments) {
                    if (arg != null) {
                        yield return arg;
                    }
                }
            }
        }
    }
}
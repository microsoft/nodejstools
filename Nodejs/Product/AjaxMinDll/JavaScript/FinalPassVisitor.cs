// FinalPassVisitor.cs
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.Ajax.Utilities
{
    internal class FinalPassVisitor : TreeVisitor
    {
        private JSParser m_parser;

        public FinalPassVisitor(JSParser parser)
        {
            m_parser = parser;
        }

        public override void Visit(ConstantWrapper node)
        {
            if (node != null)
            {
                // no children, so don't bother calling the base.
                if (node.PrimitiveType == PrimitiveType.Boolean
                    && m_parser.Settings.IsModificationAllowed(TreeModifications.BooleanLiteralsToNotOperators))
                {
                    node.Parent.ReplaceChild(node, new UnaryOperator(node.Context, m_parser)
                        {
                            Operand = new ConstantWrapper(node.ToBoolean() ? 0 : 1, PrimitiveType.Number, node.Context, m_parser),
                            OperatorToken = JSToken.LogicalNot
                        });
                }
            }
        }
    }
}

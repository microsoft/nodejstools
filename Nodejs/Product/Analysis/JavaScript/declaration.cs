// declaration.cs
//
// Copyright 2012 Microsoft Corporation
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

namespace Microsoft.NodejsTools.Parsing {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "AST statement")]
    [Serializable]
    internal abstract class Declaration : Statement, IEnumerable<VariableDeclaration>
    {
        private VariableDeclaration[] m_list;

        public int Count
        {
            get { return m_list.Length; }
        }

        public VariableDeclaration this[int index]
        {
            get { return m_list[index]; }
        }

        protected Declaration(EncodedSpan span)
            : base(span)
        {
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(m_list);
            }
        }

        public VariableDeclaration[] Variables {
            get {
                return m_list;
            }

            set {
                for (int i = 0; i < value.Length; i++) {
                    value[i].Parent = this;
                }
                m_list = value;
            }
        }

        #region IEnumerable<VariableDeclaration> Members

        public IEnumerator<VariableDeclaration> GetEnumerator()
        {
            return ((IEnumerable<VariableDeclaration>)m_list).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        #endregion
    }
}

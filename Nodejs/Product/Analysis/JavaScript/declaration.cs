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
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "AST statement")]
    public abstract class Declaration : Statement, IEnumerable<VariableDeclaration>
    {
        private List<VariableDeclaration> m_list;

        public int Count
        {
            get { return m_list.Count; }
        }

        public VariableDeclaration this[int index]
        {
            get { return m_list[index]; }
        }

        protected Declaration(IndexSpan span)
            : base(span)
        {
            m_list = new List<VariableDeclaration>();
        }

        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(m_list);
            }
        }

        internal void Append(Node elem)
        {
            VariableDeclaration decl = elem as VariableDeclaration;
            if (decl != null)
            {
                // first check the list for existing instances of this name.
                // if there are no duplicates (indicated by returning true), add it to the list.
                // if there is a dup (indicated by returning false) then that dup
                // has an initializer, and we DON'T want to add this new one if it doesn't
                // have it's own initializer.
                if (HandleDuplicates(decl.Identifier)
                    || decl.Initializer != null)
                {
                    // set the parent and add it to the list
                    decl.Parent = this;
                    m_list.Add(decl);
                    UpdateWith(decl.Span);
                }
            }
            else
            {
                // TODO: what should we do if we try to add a const to a var, or a var to a const???
                var otherVar = elem as Declaration;
                if (otherVar != null)
                {
                    for (int ndx = 0; ndx < otherVar.m_list.Count; ++ndx)
                    {
                        Append(otherVar.m_list[ndx]);
                    }
                }
            }
        }

        private bool HandleDuplicates(string name)
        {
            var hasInitializer = true;
            // walk backwards because we'll be removing items from the list
            for (var ndx = m_list.Count - 1; ndx >= 0 ; --ndx)
            {
                VariableDeclaration varDecl = m_list[ndx];

                // if the name is a match...
                if (string.CompareOrdinal(varDecl.Identifier, name) == 0)
                {
                    // check the initializer. If there is no initializer, then
                    // we want to remove it because we'll be adding a new one.
                    // but if there is an initializer, keep it but return false
                    // to indicate that there is still a duplicate in the list, 
                    // and that dup has an initializer.
                    if (varDecl.Initializer != null)
                    {
                        hasInitializer = false;
                    }
                    else
                    {
                        varDecl.Parent = null;
                        m_list.RemoveAt(ndx);
                    }
                }
            }

            return hasInitializer;
        }

        #region IEnumerable<VariableDeclaration> Members

        public IEnumerator<VariableDeclaration> GetEnumerator()
        {
            return m_list.GetEnumerator();
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

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
            set
            {
                m_list[index].IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                if (value != null)
                {
                    m_list[index] = value;
                    m_list[index].Parent = this;
                }
                else
                {
                    m_list.RemoveAt(index);
                }
            }
        }

        public ActivationObject Scope { get; set; }

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

        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            for (int ndx = 0; ndx < m_list.Count; ++ndx)
            {
                if (m_list[ndx] == oldNode)
                {
                    // if the new node isn't a variabledeclaration, ignore the call
                    VariableDeclaration newDecl = newNode as VariableDeclaration;
                    if (newNode == null || newDecl != null)
                    {
                        m_list[ndx].IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                        m_list[ndx] = newDecl;
                        m_list[ndx].IfNotNull(n => n.Parent = this);
                        return true;
                    }

                    break;
                }
            }
            return false;
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

        internal void InsertAt(int index, Node elem)
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
                    m_list.Insert(index, decl);
                }
            }
            else
            {
                // TODO: what should we do if we try to add a const to a var, or a var to a const???
                var otherVar = elem as Declaration;
                if (otherVar != null)
                {
                    // walk the source backwards so they end up in the right order
                    for (int ndx = otherVar.m_list.Count - 1; ndx >= 0; --ndx)
                    {
                        InsertAt(index, otherVar.m_list[ndx]);
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

        public void RemoveAt(int index)
        {
            if (0 <= index & index < m_list.Count)
            {
                m_list[index].IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_list.RemoveAt(index);
            }
        }

        public void Remove(VariableDeclaration variableDeclaration)
        {
            // remove the vardecl from the list. If it was there and was
            // successfully remove, Remove will return true. At that point, if the
            // vardecl still thinks we are the parent, reset the parent pointer.
            if (variableDeclaration != null && m_list.Remove(variableDeclaration) && variableDeclaration.Parent == this)
            {
                variableDeclaration.Parent = null;
            }
        }

        public bool Contains(string name)
        {
            // look at each vardecl in our list
            foreach(var varDecl in m_list)
            {
                // if it matches the target name exactly...
                if (string.CompareOrdinal(varDecl.Identifier, name) == 0)
                {
                    // ...we found a match
                    return true;
                }
            }
            // if we get here, we didn't find any matches
            return false;
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

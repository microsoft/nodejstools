// astlist.cs
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
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class AstNodeList<T>: Node, IEnumerable<T> where T : Node
    {
      private List<T> m_list;

      public override TokenWithSpan TerminatingContext
      {
        get
        {
          // if we have one, return it. If not, see if we are empty, and if not,
          // return the last item's terminator
          return base.TerminatingContext ?? (m_list.Count > 0 ? m_list[m_list.Count - 1].TerminatingContext : null);
        }
      }

      public AstNodeList(TokenWithSpan context, JSParser parser)
        : base(context, parser)
      {
        m_list = new List<T>();
      }
        
      public override void Walk(AstVisitor visitor) {
          if (visitor.Walk(this)) {
              foreach (var node in m_list) {
                  node.Walk(visitor);
              }
          }
          visitor.PostWalk(this);
      }

        public override OperatorPrecedence Precedence
      {
        get
        {
          // the only time this should be called is when we are outputting a
          // comma-operator, so the list should have the comma precedence.
          return OperatorPrecedence.Comma;
        }
      }

      public int Count
      {
        get { return m_list.Count; }
      }

      public override IEnumerable<Node> Children
      {
        get
        {
          return EnumerateNonNullNodes(m_list);
        }
      }
        /*
      public new IEnumerable<T> Children
      {
          get
          {
              return EnumerateNonNullNodes(m_list).Cast<T>();
          }
      }*/

      public override bool ReplaceChild(Node oldNode, Node newNode)
      {
        for (int ndx = 0; ndx < m_list.Count; ++ndx)
        {
          if (m_list[ndx] == (T)oldNode)
          {
            oldNode.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);

            if (newNode == null)
            {
              // remove it
              m_list.RemoveAt(ndx);
            }
            else
            {
              // replace with the new node
              m_list[ndx] = (T)newNode;
              newNode.Parent = this;
            }

            return true;
          }
        }

        return false;
      }

      internal AstNodeList<T> Append(T node)
      {
        var list = node as AstNodeList<T>;
        if (list != null)
        {
          // another list -- append each item, not the whole list
          for (var ndx = 0; ndx < list.Count; ++ndx)
          {
            Append(list[ndx]);
          }
        }
        else if (node != null)
        {
          // not another list
          node.Parent = this;
          m_list.Add(node);
          Context = Context.UpdateWith(node.Context);
        }

        return this;
      }

      public AstNodeList<T> Insert(int position, T node)
      {
        var list = node as AstNodeList<T>;
        if (list != null)
        {
          // another list. 
          for (var ndx = 0; ndx < list.Count; ++ndx)
          {
            Insert(position + ndx, list[ndx]);
          }
        }
        else if (node != null)
        {
          // not another list
          node.Parent = this;
          m_list.Insert(position, (T)node);
          Context = Context.UpdateWith(node.Context);
        }

        return this;
      }

      internal void RemoveAt(int position)
      {
        m_list[position].IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
        m_list.RemoveAt(position);
      }

      public T this[int index]
      {
        get
        {
          return m_list[index];
        }
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

      public bool IsSingleConstantArgument(string argumentValue)
      {
        if (m_list.Count == 1)
        {
          ConstantWrapper constantWrapper = m_list[0] as ConstantWrapper;
          if (constantWrapper != null
              && string.CompareOrdinal(constantWrapper.Value.ToString(), argumentValue) == 0)
          {
            return true;
          }
        }
        return false;
      }

      public string SingleConstantArgument
      {
        get
        {
          string constantValue = null;
          if (m_list.Count == 1)
          {
            ConstantWrapper constantWrapper = m_list[0] as ConstantWrapper;
            if (constantWrapper != null)
            {
              constantValue = constantWrapper.ToString();
            }
          }
          return constantValue;
        }
      }

      public override bool IsConstant
      {
        get
        {
          foreach (var item in m_list)
          {
            if (item != null)
            {
              if (!item.IsConstant)
              {
                return false;
              }
            }
          }

          return true;
        }
      }

      public override string ToString()
      {
        var sb = new StringBuilder();
        if (m_list.Count > 0)
        {
          // output the first one; then all subsequent, each prefaced with a comma
          sb.Append(m_list[0].ToString());
          for (var ndx = 1; ndx < m_list.Count; ++ndx)
          {
            sb.Append(" , ");
            sb.Append(m_list[ndx].ToString());
          }
        }

        return sb.ToString();
      }

      #region IEnumerable<AstNode> Members

      public IEnumerator<T> GetEnumerator()
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

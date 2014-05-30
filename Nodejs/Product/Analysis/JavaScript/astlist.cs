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

      public AstNodeList(IndexSpan span)
        : base(span)
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
          Span = Span.UpdateWith(node.Span);
        }

        return this;
      }

      public T this[int index]
      {
        get
        {
          return m_list[index];
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

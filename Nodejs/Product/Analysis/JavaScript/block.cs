// block.cs
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
    public sealed class Block : Statement
    {
        private List<Statement> m_list;

        /// <summary>
        /// Gets a particular statement in the list of statements making up this block
        /// </summary>
        /// <param name="index">zero-based index of the desired statement</param>
        /// <returns>abstract syntax tree node</returns>
        public Statement this[int index]
        {
            get { return m_list[index]; }
            set
            {
                m_list[index].ClearParent(this);
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

        public IList<Statement> Statements {
            get {
                return m_list;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether this block has braces (a normal block) or
        /// if it was forced to be a block even w/o braces.
        /// </summary>
        public bool HasBraces { get; set; }

        public Block(IndexSpan span)
            : base(span)
        {
            m_list = new List<Statement>();
        }

        public override void Walk(AstVisitor walker) {
            if (walker.Walk(this)) {
                foreach (var node in m_list) {
                    node.Walk(walker);
                }
            }
            walker.PostWalk(this);
        }

        /// <summary>
        /// Gets the count of statements making up this block
        /// </summary>
        public int Count
        {
            get { return m_list.Count; }
        }

        /// <summary>
        /// Gets an enumerator for the syntax tree nodes making up this block
        /// </summary>
        public override IEnumerable<Node> Children
        {
            get
            {
                return EnumerateNonNullNodes(m_list);
            }
        }

        /// <summary>
        /// Append the given statement node to the end of the block
        /// </summary>
        /// <param name="element">node to add to the block</param>
        public void Append(Statement element)
        {
            if (element != null)
            {
                element.Parent = this;
                m_list.Add(element);
            }
        }
    }
}
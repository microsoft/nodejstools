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

        private BlockScope m_blockScope;
        internal BlockScope BlockScope
        {
            get { return m_blockScope; }
            set { m_blockScope = value; }
        }

        /// <summary>
        /// Returns the enclosing scope of this block
        /// </summary>
        public override ActivationObject EnclosingScope
        {
            get
            {
                return m_blockScope != null ? m_blockScope : base.EnclosingScope;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the brace for this block (if there was one) started
        /// on a newline (true) or the same line as the statement to which it belongs (false)
        /// </summary>
        public bool BraceOnNewLine { get; set; }

        public override TokenWithSpan TerminatingContext
        {
            get
            {
                // if we have one, return it. If not, see if there's only one
                // line in our block, and if so, return it's terminator.
                return base.TerminatingContext ?? (m_list.Count == 1 ? m_list[0].TerminatingContext : null);
            }
        }

        public Block(TokenWithSpan context, JSParser parser)
            : base(context, parser)
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
        /// Replace the existing direct child node of the block with a new node.
        /// </summary>
        /// <param name="oldNode">existing statement node to replace.</param>
        /// <param name="newNode">node with which to replace the existing node.</param>
        /// <returns>true if the replacement was a succeess; false otherwise</returns>
        public override bool ReplaceChild(Node oldNode, Node newNode)
        {
            for (int ndx = m_list.Count - 1; ndx >= 0; --ndx)
            {
                if (m_list[ndx] == oldNode)
                {
                    m_list[ndx].IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                    if (newNode == null)
                    {
                        // just remove it
                        m_list.RemoveAt(ndx);
                    }
                    else
                    {
                        Block newBlock = newNode as Block;
                        if (newBlock != null)
                        {
                            // the new "statement" is a block. That means we need to insert all
                            // the statements from the new block at the location of the old item.
                            m_list.RemoveAt(ndx);
                            InsertRange(ndx, newBlock.m_list);
                        }
                        else
                        {
                            // not a block -- slap it in there
                            m_list[ndx] = (Statement)newNode;
                            newNode.Parent = this;
                        }
                    }

                    return true;
                }
            }

            return false;
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

        /// <summary>
        /// Gets the zero-based index of the given syntax tree node within the block, or -1 if the node is not a direct child of the block
        /// </summary>
        /// <param name="child">node to find</param>
        /// <returns>zero-based index of the node in the block, or -1 if the node is not a direct child of the block</returns>
        public int IndexOf(Statement child)
        {
            return m_list.IndexOf(child);
        }

        /// <summary>
        /// Insert the given statement node after an existing node in the block.
        /// </summary>
        /// <param name="after">exisitng child node of the block</param>
        /// <param name="item">node to insert after the existing node</param>
        public void InsertAfter(Statement after, Statement item)
        {
            if (item != null)
            {
                int index = m_list.IndexOf(after);
                if (index >= 0)
                {
                    var block = item as Block;
                    if (block != null)
                    {
                        // don't insert a block into a block -- insert the new block's
                        // children instead (don't want nested blocks)
                        InsertRange(index + 1, block.m_list);
                    }
                    else
                    {
                        item.Parent = this;
                        m_list.Insert(index + 1, item);
                    }
                }
            }
        }

        /// <summary>
        /// Insert a new node into the given position index within the block
        /// </summary>
        /// <param name="position">zero-based index into which the new node will be inserted</param>
        /// <param name="item">new node to insert into the block</param>
        public void Insert(int position, Statement item)
        {
            if (item != null)
            {
                var block = item as Block;
                if (block != null)
                {
                    InsertRange(position, block.m_list);
                }
                else
                {
                    item.Parent = this;
                    m_list.Insert(position, item);
                }
            }
        }

        /// <summary>
        /// Remove the last node in the block
        /// </summary>
        public void RemoveLast()
        {
            RemoveAt(m_list.Count - 1);
        }

        /// <summary>
        /// Remove the node at the given position index.
        /// </summary>
        /// <param name="index">Zero-based position index</param>
        public void RemoveAt(int index)
        {
            if (0 <= index && index < m_list.Count)
            {
                m_list[index].IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Insert a set of nodes into the block at the given position
        /// </summary>
        /// <param name="index">Zero-based position into which the new nodes will be inserted.</param>
        /// <param name="newItems">Collection of statements to insert</param>
        public void InsertRange(int index, IEnumerable<Statement> newItems)
        {
            if (newItems != null)
            {
                m_list.InsertRange(index, newItems);
                foreach (Node newItem in newItems)
                {
                    newItem.Parent = this;
                }
            }
        }

        /// <summary>
        /// Remove all statements from the Block
        /// </summary>
        public void Clear()
        {
            foreach (var item in m_list)
            {
                item.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
            }

            m_list.Clear();
        }
    }
}
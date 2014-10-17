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
    internal sealed class Block : Statement
    {
        private Statement[] m_list;
        internal static readonly Statement[] EmptyStatements = new Statement[0];

        /// <summary>
        /// Gets a particular statement in the list of statements making up this block
        /// </summary>
        /// <param name="index">zero-based index of the desired statement</param>
        /// <returns>abstract syntax tree node</returns>
        public Statement this[int index]
        {
            get { return m_list[index]; }
        }

        public Statement[] Statements {
            get {
                return m_list;
            }
            set {
                foreach (var node in value) {
                    node.Parent = this;
                }
                m_list = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether this block has braces (a normal block) or
        /// if it was forced to be a block even w/o braces.
        /// </summary>
        public BraceState Braces { get; set; }

        public Block(EncodedSpan span)
            : base(span) {
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
            get {
                return m_list.Length; 
            }
        }

        /// <summary>
        /// Gets an enumerator for the syntax tree nodes making up this block
        /// </summary>
        public override IEnumerable<Node> Children
        {
            get
            {
                if (m_list == null) {
                }
                return EnumerateNonNullNodes(m_list);
            }
        }
    }

    public enum BraceState {
        None,
        Start,
        StartAndEnd
    }
}
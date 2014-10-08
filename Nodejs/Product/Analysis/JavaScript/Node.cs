// ast.cs
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.NodejsTools.Analysis;

namespace Microsoft.NodejsTools.Parsing {
    /// <summary>
    /// Abstract syntax tree node
    /// </summary>
    [Serializable]
    internal abstract class Node {
        // this is used in the child enumeration for nodes that don't have any children
        private static readonly IEnumerable<Node> s_emptyChildrenCollection = new Node[0];

        public EncodedSpan EncodedSpan {
            get;
            set;
        }

        protected Node(EncodedSpan location) {
            EncodedSpan = location;
        }

        public IndexSpan GetSpan(LocationResolver parent) {
            return EncodedSpan.GetSpan(parent);
        }

        public int GetStartIndex(LocationResolver parent) {
            return GetSpan(parent).Start;
        }

        public int GetEndIndex(LocationResolver parent) {
            return GetSpan(parent).End;
        }

        internal SourceLocation GetStart(LocationResolver jsAst) {
            return jsAst.IndexToLocation(GetSpan(jsAst).Start);
        }

        internal SourceLocation GetEnd(LocationResolver jsAst) {
            return jsAst.IndexToLocation(GetSpan(jsAst).End);
        }

        public static Block ForceToBlock(Statement node) {
            // if the node is null or already a block, then we're 
            // good to go -- just return it.
            var block = node as Block;
            if (block == null && node != null) {
                // it's not a block, so create a new block, append the astnode
                // and return the block
                block = new Block(node.EncodedSpan) { Statements = new[] { node } };
            }

            return block;
        }

        /// <summary>
        /// Gets an enumeration representing the child nodes of this node in the abstract syntax tree
        /// </summary>
        public virtual IEnumerable<Node> Children {
            get { return s_emptyChildrenCollection; }
        }

        internal static IEnumerable<Node> EnumerateNonNullNodes<T>(IList<T> nodes) where T : Node {
            for (int ndx = 0; ndx < nodes.Count; ++ndx) {
                if (nodes[ndx] != null) {
                    yield return nodes[ndx];
                }
            }
        }

        internal static IEnumerable<Node> EnumerateNonNullNodes(Node n1, Node n2 = null, Node n3 = null, Node n4 = null) {
            return EnumerateNonNullNodes(new[] { n1, n2, n3, n4 });
        }

        public abstract void Walk(AstVisitor walker);
    }

    internal static class NodeExtensions {
        internal static void ClearParent(this Statement self, Statement parent) {
            self.IfNotNull(n => n.Parent = (n.Parent == parent) ? null : n.Parent);
        }

        internal static void AssignParent(this Statement self, Statement parent) {
            self.IfNotNull(n => n.Parent = parent);
        }
    }
}

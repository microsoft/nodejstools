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

namespace Microsoft.NodejsTools.Parsing {
    /// <summary>
    /// Abstract syntax tree node
    /// </summary>
    public abstract class Node {
        // this is used in the child enumeration for nodes that don't have any children
        private static readonly IEnumerable<Node> s_emptyChildrenCollection = new Node[0];

        /// <summary>
        /// Gets or sets the parent node of this node in the abstract syntax tree
        /// </summary>
        public Node Parent { get; set; }

        /// <summary>
        /// Gets or sets the source context of this node
        /// </summary>
        public TokenWithSpan Context { get; set; }

        /// <summary>
        /// Gets a reference to the JSParser object that generated this node
        /// </summary>
        public JSParser Parser { get; private set; }

        /// <summary>the context of any terminating character parsed after this node
        /// e.g. the semicolon after a statement or a comma in a parameter list</summary>
        private TokenWithSpan m_terminatingContext;

        public virtual TokenWithSpan TerminatingContext {
            get { return m_terminatingContext; }
            set { m_terminatingContext = value; }
        }

        protected Node(TokenWithSpan context, JSParser parser) {
            Parser = parser;
            if (context != null) {
                Context = context;
            } else {
                // generate a bogus context
                Context = new TokenWithSpan();
            }
        }

        public int StartIndex {
            get {
                return Context.StartPosition;
            }
        }

        public int EndIndex {
            get {
                return Context.EndPosition;
            }
        }

        public static Block ForceToBlock(Statement node) {
            // if the node is null or already a block, then we're 
            // good to go -- just return it.
            var block = node as Block;
            if (block == null && node != null) {
                // it's not a block, so create a new block, append the astnode
                // and return the block
                block = new Block(node.Context, node.Parser);
                block.Append(node);
            }

            return block;
        }

        internal virtual string GetFunctionGuess(Node target) {
            // most objects serived from AST return an empty string
            return string.Empty;
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

        /// <summary>
        /// Replace this node's specified child with another given node. 
        /// </summary>
        /// <param name="oldNode">Child node to be replaced</param>
        /// <param name="newNode">New node with which to replace the existing child node</param>
        /// <returns>true if the replacement succeeded; false otherwise</returns>
        public virtual bool ReplaceChild(Node oldNode, Node newNode) {
            return false;
        }

        /// <summary>
        /// Get the enclosing lexical scope for this node.
        /// </summary>
        public virtual ActivationObject EnclosingScope {
            get {
                // if we don't have a parent, then we are in the global scope.
                // otherwise, just ask our parent. Nodes with scope will override this property.
                return Parent != null ? Parent.EnclosingScope : Parser.GlobalScope;
            }
        }

        public abstract void Walk(AstVisitor walker);

        public void UpdateWith(TokenWithSpan context) {
            if (context != null) {
                if (this.Context == null) {
                    this.Context = context;
                } else {
                    Context = Context.UpdateWith(context);
                }
            }
        }

        public JsAst GlobalParent {
            get {
                var res = this;
                while (!(res is JsAst)) {
                    Debug.Assert(res != null);
                    res = res.Parent;
                }
                return (JsAst)res;
            }
        }

        internal SourceLocation GetStart(JsAst jsAst) {
            return new SourceLocation(Context.EndPosition, Context.StartLineNumber, Context.StartColumn);
        }

        internal SourceLocation GetEnd(JsAst jsAst) {
            return new SourceLocation(Context.EndPosition, Context.EndLineNumber, Context.EndColumn);
        }

        internal IndexSpan IndexSpan {
            get {
                return new IndexSpan(Context.StartPosition, Context.EndPosition - Context.StartPosition);
            }
        }

        public override string ToString() {
            return String.Format("{0} {1} {2}", GetType().Name, GetStart(GlobalParent), GetEnd(GlobalParent));
        }
    }
}

/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.Analysis;

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    internal class JsAst : Statement {
        private Block _block;
        private readonly LocationResolver _locationResolver;

        internal JsAst(EncodedSpan span, LocationResolver indexResolver)
            : base(span) {
            _locationResolver = indexResolver;
        }

        public Block Block {
            get {
                return _block;
            }
            internal set {
                _block = value;
                _block.Parent = this;
            }
        }

        internal SourceLocation IndexToLocation(int index) {
            return _locationResolver.IndexToLocation(index);
        }

        internal LocationInfo ResolveLocation(ProjectEntry project, object location) {
            var loc = _locationResolver.IndexToLocation(((Node)location).GetSpan(project.Tree.LocationResolver).Start);
            return new LocationInfo(
                project,
                loc.Line,
                loc.Column
            );
        }

        public LocationResolver LocationResolver {
            get {
                return _locationResolver;
            }
        }

        public JsAst CloneWithNewBlock(Block block) {
            return new JsAst(EncodedSpan, _locationResolver) {
                Block = block
            };
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                Block.Walk(visitor);
            }
            visitor.PostWalk(this);
        }
   }
}
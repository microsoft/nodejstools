using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Parsing;
using System;

namespace Microsoft.NodejsTools.Parsing {

    public class JsAst : Statement, ILocationResolver {
        private Block _block;
        private readonly IndexResolver _indexResolver;
        private GlobalScope _globalScope;

        internal JsAst(IndexSpan span, IndexResolver indexResolver)
            : base(span) {
            _indexResolver = indexResolver;
        }

        public GlobalScope GlobalScope {
            get {
                return _globalScope;
            }
            internal set {
                _globalScope = value;
            }
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
            return _indexResolver.IndexToLocation(index);
        }

        public LocationInfo ResolveLocation(IProjectEntry project, object location) {
            var loc = _indexResolver.IndexToLocation(((Node)location).Span.Start);
            return new LocationInfo(
                project,
                loc.Line,
                loc.Column
            );
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                Block.Walk(visitor);
            }
            visitor.PostWalk(this);
        }
    }
}
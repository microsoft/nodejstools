using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Parsing;
using System;

namespace Microsoft.NodejsTools.Parsing {

    public class JsAst : Statement, ILocationResolver {
        private readonly Block _block;
        private readonly IndexResolver _indexResolver;
        private readonly GlobalScope _globalScope;

        internal JsAst(Block block, IndexSpan span, GlobalScope scope, IndexResolver indexResolver)
            : base(span) {
            _block = block;
            _block.Parent = this;
            _indexResolver = indexResolver;
            _globalScope = scope;
        }

        public GlobalScope GlobalScope {
            get {
                return _globalScope;
            }
        }

        public Block Block {
            get {
                return _block;
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
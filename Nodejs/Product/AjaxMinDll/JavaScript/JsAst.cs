using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Parsing;
using System;

namespace Microsoft.NodejsTools.Parsing {

    public class JsAst : Statement, ILocationResolver {
        private readonly Block _block;

        public JsAst(Block block, TokenWithSpan context, JSParser parser)
            : base(context, parser) {
            _block = block;
            _block.Parent = this;
        }

        public Block Block {
            get {
                return _block;
            }
        }

        internal SourceLocation IndexToLocation(int startIndex) {
            throw new NotImplementedException();
        }

        public LocationInfo ResolveLocation(IProjectEntry project, object location) {
            return new LocationInfo(
                project,
                ((Node)location).Context.StartLineNumber,
                ((Node)location).Context.StartColumn
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
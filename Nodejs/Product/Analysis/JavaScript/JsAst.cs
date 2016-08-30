//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
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
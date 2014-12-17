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
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    internal class ExpressionStatement : Statement {
        private Expression _expression;

        public ExpressionStatement(EncodedSpan location)
            : base(location) {

        }

        public override IEnumerable<Node> Children {
            get {
                return new[] { Expression };
            }
        }

        public Expression Expression {
            get {
                return _expression;
            }
            set {
                _expression = value;
            }
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                _expression.Walk(visitor);
            }
            visitor.PostWalk(this);
        }
    }
}

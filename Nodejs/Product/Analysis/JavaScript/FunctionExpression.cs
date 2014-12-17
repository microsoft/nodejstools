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

namespace Microsoft.NodejsTools.Parsing
{
    [Serializable]
    internal class FunctionExpression : Expression
    {
        private FunctionObject _function;

        public FunctionExpression(EncodedSpan span)
            : base(span) {
        }

        public FunctionObject Function
        {
            get
            {
                return _function;
            }
            set
            {
                _function = value;
                _function.IsExpression = true;
            }
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
                _function.Walk(visitor);
            }
            visitor.PostWalk(this);
        }
    }
}

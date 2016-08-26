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

namespace Microsoft.NodejsTools.Parsing {
    [Serializable]
    internal abstract class Statement : Node
    {
        protected Statement(EncodedSpan location)
            : base(location) {
            EncodedSpan = location;
        }

        /// <summary>
        /// Gets or sets the parent node of this node in the abstract syntax tree
        /// </summary>
        public Statement Parent { get; set; }

        public static Expression GetExpression(Statement statement)
        {
            if (statement is Block)
            {
                if (((Block)statement).Count == 1)
                {
                    return GetExpression(((Block)statement)[0]);
                }
            }
            else if (statement is ExpressionStatement)
            {
                var exprStmt = (ExpressionStatement)statement;
                return exprStmt.Expression;
            }
            else if (statement is ReturnNode)
            {
                return ((ReturnNode)statement).Operand;
            }
            return null;
        }

        private JsAst GlobalParent {
            get {
                var res = this;
                while (res != null && !(res is JsAst)) {
                    res = res.Parent;
                }
                return (JsAst)res;
            }
        }

        public override string ToString() {
            if (GlobalParent != null) {
                return String.Format("{0} {1} {2}", GetType().Name, GetStart(GlobalParent.LocationResolver), GetEnd(GlobalParent.LocationResolver));
            }
            return base.ToString();
        }
    }
}

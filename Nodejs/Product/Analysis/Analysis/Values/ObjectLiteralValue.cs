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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    internal class ObjectLiteralValue : ObjectValue {
        private readonly Node _node;

        public ObjectLiteralValue(ProjectEntry projectEntry, Node node, string description = null)
            : base(projectEntry, null, description) {
            _node = node;
            projectEntry.Analyzer.AnalysisValueCreated(typeof(ObjectLiteralValue));
        }

        public override string ToString() {
            return String.Format(
                "Object literal: {0} - {1}\r\n{2}",
                _node.GetStart(ProjectEntry.Tree.LocationResolver),
                _node.GetEnd(ProjectEntry.Tree.LocationResolver),
                ProjectEntry.FilePath
            );
        }

        internal override bool UnionEquals(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null) {
                    return true;
                }
            }

            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null) {
                    // two literals from the same node, these
                    // literals were created by independent function
                    // analysis, merge them together now.
                    return literal._node == _node;
                }
            }
            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                return typeof(ObjectLiteralValue).GetHashCode();
            }

            if (strength >= MergeStrength.ToBaseClass) {
                return _node.GetHashCode();
            }
            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null) {
                    return this;
                }
            }
            
            if (strength >= MergeStrength.ToBaseClass) {
                var literal = av as ObjectLiteralValue;
                if (literal != null && literal._node == _node) {
                    return this;
                }
            }

            return base.UnionMergeTypes(av, strength);
        }
    }
}

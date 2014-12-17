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
    sealed class StringValue : NonObjectValue {
        internal readonly string _value;
        private readonly JsAnalyzer _analyzer;

        public StringValue(string value, JsAnalyzer javaScriptAnalyzer)
            : base(javaScriptAnalyzer._builtinEntry) {
            _value = value;
            _analyzer = javaScriptAnalyzer;
            javaScriptAnalyzer.AnalysisValueCreated(typeof(StringValue));
        }

        public override IAnalysisSet BinaryOperation(BinaryOperator node, AnalysisUnit unit, IAnalysisSet value) {
            if (node.OperatorToken == JSToken.Plus) {
                return _analyzer._emptyStringValue.SelfSet;
            }
            return base.BinaryOperation(node, unit, value);
        }

        public override string Description {
            get {
                return "string";
            }
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.String;
            }
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.String;
            }
        }

        public override AnalysisValue Prototype {
            get { return _analyzer._stringPrototype; }
        }

        private const int StringUnionStrength = MergeStrength.ToObject;

        internal override bool UnionEquals(AnalysisValue av, int strength) {
            if (strength >= StringUnionStrength) {
                return av is StringValue;
            }
            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= StringUnionStrength) {
                return _analyzer._stringPrototype.GetHashCode();
            }
            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            if (strength >= StringUnionStrength) {
                return _analyzer._emptyStringValue;
            }

            return base.UnionMergeTypes(av, strength);
        }

        public override string ToString() {
            return "string: " + _value;
        }
    }
}

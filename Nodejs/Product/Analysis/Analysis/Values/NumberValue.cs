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

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class NumberValue : NonObjectValue {
        internal double _value;
        private JsAnalyzer _analyzer;

        public NumberValue(double p, JsAnalyzer analyzer)
            : base(analyzer._builtinEntry) {
            _value = p;
            _analyzer = analyzer;
            analyzer.AnalysisValueCreated(typeof(NumberValue));
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Number;
            }
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Number;
            }
        }

        public override string Description {
            get {
                return "number";
            }
        }

        public override AnalysisValue Prototype {
            get { return _analyzer._numberPrototype; }
        }


        internal override bool UnionEquals(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                return av is NumberValue;
            }
            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                return _analyzer._numberPrototype.GetHashCode();
            }
            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                return _analyzer._zeroIntValue;
            }

            return base.UnionMergeTypes(av, strength);
        }

        public override string ToString() {
            return "number: " + _value;
        }
    }
}

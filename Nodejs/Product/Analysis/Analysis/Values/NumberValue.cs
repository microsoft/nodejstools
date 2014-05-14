/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace Microsoft.NodejsTools.Analysis.Values {
    class NumberValue : NonObjectValue {
        private double _value;
        private JsAnalyzer _analyzer;

        public NumberValue(double p, JsAnalyzer javaScriptAnalyzer) {
            _value = p;
            _analyzer = javaScriptAnalyzer;
            javaScriptAnalyzer.AnalysisValueCreated(typeof(NumberValue));
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

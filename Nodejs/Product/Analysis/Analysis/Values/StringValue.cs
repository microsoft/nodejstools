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
    class StringValue : NonObjectValue {
        private readonly string _value;
        private readonly JsAnalyzer _analyzer;

        public StringValue(string value, JsAnalyzer javaScriptAnalyzer) {
            _value = value;
            _analyzer = javaScriptAnalyzer;
            javaScriptAnalyzer.AnalysisValueCreated(typeof(StringValue));
        }

        public override string Description {
            get {
                return "string";
            }
        }

        public override object GetConstantValue() {
            return _value;
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

        internal override bool UnionEquals(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                return av is StringValue;
            }
            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= MergeStrength.ToObject) {
                return _analyzer._stringPrototype.GetHashCode();
            }
            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToObject) {
                return _analyzer._emptyStringValue;
            }

            return base.UnionMergeTypes(av, strength);
        }

        public override string ToString() {
            return "string: " + _value;
        }
    }
}

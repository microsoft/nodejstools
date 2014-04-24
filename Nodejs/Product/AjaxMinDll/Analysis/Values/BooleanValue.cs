using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Interpreter;

namespace Microsoft.NodejsTools.Analysis.Values {
    class BooleanInfo : NonObjectValue {
        private readonly bool _value;
        private readonly JsAnalyzer _analyzer;

        public BooleanInfo(bool value, JsAnalyzer javaScriptAnalyzer) {
            _value = value;
            _analyzer = javaScriptAnalyzer;
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Boolean;
            }
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Boolean;
            }
        }

        public override AnalysisValue Prototype {
            get { return _analyzer._booleanPrototype; }
        }
    }
}

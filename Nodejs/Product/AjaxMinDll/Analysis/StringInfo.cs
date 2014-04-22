using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Interpreter;

namespace Microsoft.NodejsTools.Analysis.Values {
    class StringInfo : AnalysisValue {
        private readonly string _value;
        private readonly JsAnalyzer _analyzer;

        public StringInfo(string value, JsAnalyzer javaScriptAnalyzer) {
            _value = value;
            _analyzer = javaScriptAnalyzer;
        }

        public override object GetConstantValue() {
            return _value;
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.String;
            }
        }
    }
}

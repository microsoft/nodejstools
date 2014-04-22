using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Interpreter;
namespace Microsoft.NodejsTools.Analysis.Values {
    class NumberInfo : AnalysisValue {
        private double _value;
        private JsAnalyzer _analyzer;

        public NumberInfo(double p, JsAnalyzer javaScriptAnalyzer) {
            _value = p;
            _analyzer = javaScriptAnalyzer;
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Number;
            }
        }
    }
}

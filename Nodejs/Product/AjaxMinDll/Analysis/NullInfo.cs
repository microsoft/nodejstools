using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Interpreter;

namespace Microsoft.NodejsTools.Analysis.Values {

    class NullInfo : AnalysisValue {
        private readonly JsAnalyzer _analyzer;

        public NullInfo(JsAnalyzer javaScriptAnalyzer) {
            _analyzer = javaScriptAnalyzer;
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Null;
            }
        }
    }
}

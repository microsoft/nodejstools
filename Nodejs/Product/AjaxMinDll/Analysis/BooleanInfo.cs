using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Interpreter;

namespace Microsoft.NodejsTools.Analysis.Values {
    class BooleanInfo : AnalysisValue {
        private readonly bool _value;
        private readonly JsAnalyzer _analyzer;

        public BooleanInfo(bool value, JsAnalyzer javaScriptAnalyzer) {
            _value = value;
            _analyzer = javaScriptAnalyzer;
            //AddMember("length", new GetPrototypeOf(projectEntry));
            //AddMember("name", new GetPrototypeOf(projectEntry));
            //AddMember("arguments", new GetPrototypeOf(projectEntry));
            //AddMember("caller", new GetPrototypeOf(projectEntry));
            //AddMember("prototype", new GetPrototypeOf(projectEntry));
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Boolean;
            }
        }
    }
}

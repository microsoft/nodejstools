using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    class SpecializedUserFunctionValue : UserFunctionValue {
        private readonly CallDelegate _call;
        private readonly bool _callBase;

        public SpecializedUserFunctionValue(CallDelegate call, FunctionObject node, AnalysisUnit declUnit, EnvironmentRecord declScope, bool callBase) :
            base(node, declUnit, declScope) {
            _call = call;
            _callBase = callBase;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            var res = _call(this, node, unit, @this, args);

            if (_callBase) {
                return res.Union(base.Call(node, unit, @this, args));
            }
            return res;
        }
    }
}

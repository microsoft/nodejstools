using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    class SpecializedUserFunctionValue : UserFunctionValue {
        private readonly CallDelegate _call;

        public SpecializedUserFunctionValue(CallDelegate call, FunctionObject node, AnalysisUnit declUnit, EnvironmentRecord declScope) :
            base(node, declUnit, declScope) {
                _call = call;
        }

        public override IAnalysisSet Call(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            var res = _call(this, node, unit, @this, args);
            
            return res.Union(base.Call(node, unit, @this, args));
        }
    }
}

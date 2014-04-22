using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Interpreter;

namespace Microsoft.NodejsTools.Analysis.Values {
    class UndefinedInfo : AnalysisValue {
        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Undefined;
            }
        }
    }
}

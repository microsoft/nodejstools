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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
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

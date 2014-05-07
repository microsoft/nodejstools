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
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis {
    partial class NodejsModuleBuilder {
        private static Dictionary<string, Dictionary<string, CallDelegate>> _specializations = new Dictionary<string, Dictionary<string, CallDelegate>>() { 
            { 
                "util", 
                new Dictionary<string, CallDelegate>() {
                    { "inherits", UtilInherits }
                }
            }
        };

        private static IAnalysisSet UtilInherits(FunctionValue func, Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            // function inherits(ctor, superCtor)
            // sets ctor.super_ to superCtor
            // sets ctor.prototype = {copy of superCtor.prototype}
            // We skip the copy here which is cheating but lack of flow control
            // means even if we did copy we'd continue to need to copy, so it's fine.
            if (args.Length >= 2) {
                args[0].SetMember(node, unit, "super_", args[1]);

                args[0].SetMember(node, unit, "prototype", args[1].GetMember(node, unit, "prototype"));
            }
            return AnalysisSet.Empty;
        }

    }
}

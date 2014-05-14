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

namespace Microsoft.NodejsTools.Analysis.Values {
    class InstanceValue : ObjectValue {
        public InstanceValue(ProjectEntry projectEntry, FunctionValue creator, string description = null)
            : base(projectEntry, creator, description) {
            projectEntry.Analyzer.AnalysisValueCreated(typeof(InstanceValue));
        }

        public override string ObjectDescription {
            get {
                var name = _creator.Name;
                if (name != null) {
                    return name + " object";
                }

                return base.ObjectDescription;
            }
        }
    }
}

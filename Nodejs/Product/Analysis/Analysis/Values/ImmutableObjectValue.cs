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

using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Value that we promote instance values to when merging which doesn't allow any
    /// assignments.
    /// </summary>
    class ImmutableObjectValue : InstanceValue {
        public ImmutableObjectValue(ProjectEntry projectEntry)
            : base(projectEntry, null) {
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
        }

        public override void SetIndex(Node node, AnalysisUnit unit, IAnalysisSet index, IAnalysisSet value) {
        }

        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return base.Get(node, unit, name, false);
        }

        public override string ObjectDescription {
            get {
                return "object";
            }
        }
    }
}

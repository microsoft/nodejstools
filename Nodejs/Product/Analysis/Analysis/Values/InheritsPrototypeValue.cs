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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// A unique value which gets created for util.inherits.  This wraps the value
    /// which we're inheriting from and forwards all of it's accesses to it but
    /// keeps member assignments amongst it's self.  This means that the value
    /// we inherit from won't pick up values assigned to the new prototype.
    /// </summary>
    [Serializable]
    class InheritsPrototypeValue : ObjectValue {
        private IAnalysisSet _prototypes;

        public InheritsPrototypeValue(ProjectEntry projectEntry, IAnalysisSet values)
            : base(projectEntry) {
            _prototypes = values;
            projectEntry.Analyzer.AnalysisValueCreated(typeof(InheritsPrototypeValue));
        }

        public void AddPrototypes(IAnalysisSet values) {
            _prototypes = _prototypes.Union(values);
        }

        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            var res = base.Get(node, unit, name, addRef);
            foreach (var value in _prototypes) {
                if (value.Push()) {
                    try {
                        res = res.Union(value.Get(node, unit, name, addRef));
                    } finally {
                        value.Pop();
                    }
                }
            }
            return res;
        }

        public override Dictionary<string, IAnalysisSet> GetAllMembers() {
            var res = base.GetAllMembers();
            foreach (var value in _prototypes) {
                if (value.Push()) {
                    try {
                        foreach (var keyValue in value.GetAllMembers()) {
                            MergeTypes(res, keyValue.Key, keyValue.Value);
                        }
                    } finally {
                        value.Pop();
                    }
                }
            }
            return res;
        }
    }
}

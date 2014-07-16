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

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// A built-in function where some of the members are locked and
    /// cannot be mutated.  This is used to prevent replacement of
    /// things like Object.create and other APIs which get augmented
    /// for improving browser support but the augmentation only makes
    /// our analysis more difficult.
    /// </summary>
    [Serializable]
    class PartiallyImmutableBuiltinFunctionValue : BuiltinFunctionValue {
        private readonly HashSet<string> _immutableFields = new HashSet<string>();

        public PartiallyImmutableBuiltinFunctionValue(ProjectEntry projectEntry,
            string name,
            string[] immutableFields,
            string documentation = null,
            bool createPrototype = true,
            params ParameterResult[] signature)
            : base(projectEntry, name, documentation, createPrototype, signature) {
            _immutableFields = new HashSet<string>(immutableFields);
        }

        public override void SetMember(Parsing.Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
            if (_immutableFields.Contains(name)) {
                // we still want to track the reference...
                VariableDef varRef = GetValuesDef(name);
                varRef.AddAssignment(node, unit);
                return;
            }

            base.SetMember(node, unit, name, value);
        }
    }
}

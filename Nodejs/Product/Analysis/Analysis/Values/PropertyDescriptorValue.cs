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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents the descriptor state for a given property.
    /// 
    /// We track all of Values, Get, Set and merge them together,
    /// so if a property is changing between the two we'll see
    /// the union.
    /// 
    /// We don't currently track anything like writable/enumerable/configurable but
    /// we do return a boolean value when accessing them as 1st class objects.
    /// 
    /// We also don't support handing out property descriptors from property
    /// descriptors.  That is:
    /// 
    /// Object.getOwnPropertyDescriptor(Object.getOwnPropertyDescriptor(x, 'abc'), 'value')
    /// 
    /// Will fail.  This prevents us introducing an infinite amount of objects
    /// into the analysis system by recursive property descriptor creation.
    /// </summary>
    [Serializable]
    class PropertyDescriptorValue : AnalysisValue, IPropertyDescriptor {
        public VariableDef Values, Getter, Setter;
        public ProjectEntry _projectEntry;

        public PropertyDescriptorValue(ProjectEntry projectEntry)
            : base(projectEntry) {
            _projectEntry = projectEntry;
        }

        internal override ProjectEntry DeclaringModule {
            get {
                return _projectEntry;
            }
        }

        public IAnalysisSet GetValue(Node node, AnalysisUnit unit, ProjectEntry declaringScope, IAnalysisSet @this, bool addRef) {
            if (Values == null) {
                Values = new EphemeralVariableDef();
            }

            var res = Values.GetTypes(unit, declaringScope);

            if (res.Count > 0) {
                // Don't add references to ephemeral values...  If they
                // gain types we'll re-enqueue and the reference will be
                // added then.
                if (addRef && !Values.IsEphemeral) {
                    Values.AddReference(node, unit);
                }
            }

            if (Getter != null) {
                res = res.Union(Getter.GetTypesNoCopy(unit, declaringScope).Call(node, unit, @this, ExpressionEvaluator.EmptySets));
            }

            return res;
        }

        public bool IsEphemeral {
            get {
                return Values == null || Values.IsEphemeral;
            }
        }


        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            switch (name) {
                case "value":
                    if (Values != null) {
                        return Values.GetTypes(unit, _projectEntry);
                    }
                    break;
                case "get":
                    if (Getter != null) {
                        return Getter.GetTypes(unit, _projectEntry);
                    }
                    break;
                case "set":
                    if (Setter != null) {
                        return Setter.GetTypes(unit, _projectEntry);
                    }
                    break;
                case "writable":
                case "enumerable":
                case "configurable":
                    return unit.Analyzer._trueInst.SelfSet;
            }
            return AnalysisSet.Empty;
        }
    }
}

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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents a value which is not an object (number, string, bool)
    /// </summary>
    [Serializable]
    abstract class NonObjectValue : AnalysisValue, IReferenceableContainer {
        public NonObjectValue(ProjectEntry projectEntry)
            : base(projectEntry) {
        }

        public abstract AnalysisValue Prototype {
            get;
        }

        internal override Dictionary<string, IAnalysisSet> GetAllMembers(ProjectEntry accessor) {
            return Prototype.GetAllMembers(accessor);
        }

        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return Prototype.Get(node, unit, name);
        }

        internal override IPropertyDescriptor GetProperty(Node node, AnalysisUnit unit, string name) {
            return Prototype.GetProperty(node, unit, name);
        }

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            var proto = Prototype as IReferenceableContainer;
            if (proto != null) {
                return proto.GetDefinitions(name);
            }
            return new IReferenceable[0];
        }
    }
}

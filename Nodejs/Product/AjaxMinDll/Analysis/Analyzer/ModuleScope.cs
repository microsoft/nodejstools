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

using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    sealed class ModuleScope : EnvironmentRecord {

        public ModuleScope(ModuleInfo moduleInfo)
            : base(moduleInfo, null) {
        }

        private ModuleScope(ModuleScope scope)
            : base(scope.AnalysisValue, scope, true) {
        }

        public ModuleInfo Module { get { return AnalysisValue as ModuleInfo; } }

        public override string Name {
            get { return Module.Name; }
        }

        public override bool AssignVariable(string name, Node location, AnalysisUnit unit, IAnalysisSet values) {
            if (base.AssignVariable(name, location, unit, values)) {
                Module.ModuleDefinition.EnqueueDependents();
                return true;
            }

            return false;
        }

        public ModuleScope CloneForPublish() {
            return new ModuleScope(this);
        }
    }
}

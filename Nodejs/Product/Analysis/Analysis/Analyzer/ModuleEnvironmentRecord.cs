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
    sealed class ModuleEnvironmentRecord : DeclarativeEnvironmentRecord {
        private readonly ProjectEntry _projectEntry;
        private readonly ModuleValue _module;   // the object which corresponds to "module" in the users module

        public ModuleEnvironmentRecord(ModuleValue module, ProjectEntry projectEntry)
            : base(null) {
            _module = module;
            _projectEntry = projectEntry;
        }

        private ModuleEnvironmentRecord(ModuleEnvironmentRecord scope)
            : base(scope, true) {
            _module = scope.Module;
            _projectEntry = scope.ProjectEntry;
        }

        public ModuleValue Module { get { return _module; } }

        public override AnalysisValue AnalysisValue {
            get {
                return _module;
            }
        }

        public override string Name {
            get { return _projectEntry.FilePath; }
        }

        public ModuleEnvironmentRecord CloneForPublish() {
            return new ModuleEnvironmentRecord(this);
        }

        public ProjectEntry ProjectEntry {
            get {
                return _projectEntry;
            }
        }
    }
}

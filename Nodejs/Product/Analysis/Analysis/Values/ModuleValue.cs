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
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    internal class ModuleValue : ObjectValue {
        private readonly string _name;
        private readonly ModuleEnvironmentRecord _scope;

        public ModuleValue(string moduleName, ProjectEntry projectEntry)
            : base(projectEntry) {
            _name = moduleName;
            _scope = new ModuleEnvironmentRecord(this, projectEntry);
        }

        public ModuleEnvironmentRecord EnvironmentRecord {
            get {
                return _scope;
            }
        }

        public override string Name {
            get { return _name; }
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Module;
            }
        }

        public override string ToString() {
            return "Module " + base.ToString();
        }

        public override string ShortDescription {
            get {
                return "Node.js module " + Name;
            }
        }

        public override string ObjectDescription {
            get {
                return String.Format("module ({0})", Path.GetFileName(_name));
            }
        }

        public override string Documentation {
            get {
#if FALSE
                if (ProjectEntry.Tree != null && ProjectEntry.Tree.Body != null) {
                    return ProjectEntry.Tree.Block.Documentation.TrimDocumentation() ?? String.Empty;
                }
#endif
                return String.Empty;
            }
        }

        public override IEnumerable<LocationInfo> Locations {
            get {
                return new[] { new LocationInfo(ProjectEntry, 1, 1) };
            }
        }

        /// <summary>
        /// Modules are fixed in time, we re-use the same ModuleInfo
        /// across all instances of the analysis.
        /// </summary>
        public override int DeclaringVersion {
            get {
                return -1;
            }
        }

        /// <summary>
        /// Modules are fixed in time, we re-use the same ModuleInfo
        /// across all instances of the analysis.
        /// </summary>
        public override IJsProjectEntry DeclaringModule {
            get {
                return null;
            }
        }
    }
}

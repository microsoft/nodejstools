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
using System.IO;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// The object that is created for a Node.js module's
    /// exports variable.  We create this so that we can show
    /// a different icon in intellisense for modules.

    /// </summary>
    [Serializable]
    class ExportsValue : ObjectValue {
        private readonly string _name;

        public ExportsValue(string name, ProjectEntry projectEntry)
            : base(projectEntry) {
            _name = name;
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Module;
            }
        }

        public override string ObjectDescription {
            get {
                return "exports from " + Path.GetFileName(_name);
            }
        }

        public override IEnumerable<LocationInfo> Locations {
            get {
                return new[] { new LocationInfo(ProjectEntry, 1, 1) };
            }
        }
    }
}

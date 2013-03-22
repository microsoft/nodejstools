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
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project {
    [Guid("62E8E091-6914-498E-A47B-6F198DC1873D")]
    class NodeGeneralPropertyPage : CommonPropertyPage {
        public override System.Windows.Forms.Control Control {
            get { throw new NotImplementedException(); }
        }

        public override void Apply() {
        }

        public override void LoadSettings() {
        }

        public override string Name {
            get { return "General"; }
        }
    }
}

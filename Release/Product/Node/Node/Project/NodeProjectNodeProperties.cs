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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.Project.Automation;

namespace Microsoft.NodejsTools.Project {
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("04726C27-8125-471A-BAC0-2301D273DB5E")]
    public class NodeProjectNodeProperties : CommonProjectNodeProperties {
        public NodeProjectNodeProperties(ProjectNode node)
            : base(node) {
        }

        [PropertyNameAttribute("WebApplication.AspNetDebugging")]
        [Browsable(false)]
        public bool AspNetDebugging {
            get {
                return true;
            }
        }

        [PropertyNameAttribute("WebApplication.NativeDebugging")]
        [Browsable(false)]
        public bool NativeDebugging {
            get {
                return false;
            }
        }

        [Browsable(false)]
        public uint TargetFramework {
            get {
                return 0x40000;
            }
        }
    }
}

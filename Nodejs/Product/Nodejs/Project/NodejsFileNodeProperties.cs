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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.NodejsTools.Project {

    [ComVisible(true)]
    public class NodejsIncludedFileNodeProperties : IncludedFileNodeProperties {
        internal NodejsIncludedFileNodeProperties(HierarchyNode node)
            : base(node) {
        }

        [SRCategoryAttribute(SR.Advanced)]
        [LocDisplayName(SR.HasTests)]
        [SRDescriptionAttribute(SR.HasTestsDescription)]
        public bool HasTests {
            get {
                var publish = this.HierarchyNode.ItemNode.GetMetadata(SR.HasTests);
                if (String.IsNullOrEmpty(publish)) {
                    return false;
                }
                return Convert.ToBoolean(publish);
            }
            set {

                this.HierarchyNode.ItemNode.SetMetadata(SR.HasTests, value.ToString());
            }
        }
    }

    [ComVisible(true)]
    public class NodejsLinkFileNodeProperties : LinkFileNodeProperties {
        internal NodejsLinkFileNodeProperties(HierarchyNode node)
            : base(node) {
        }

        [SRCategoryAttribute(SR.Advanced)]
        [LocDisplayName(SR.HasTests)]
        [SRDescriptionAttribute(SR.HasTestsDescription)]
        public bool HasTests {
            get {
                var hasTests = this.HierarchyNode.ItemNode.GetMetadata(SR.HasTests);
                if (String.IsNullOrEmpty(hasTests)) {
                    return false;
                }
                return Convert.ToBoolean(hasTests);
            }
            set {

                this.HierarchyNode.ItemNode.SetMetadata(SR.HasTests, value.ToString());
            }
        }
    }
}

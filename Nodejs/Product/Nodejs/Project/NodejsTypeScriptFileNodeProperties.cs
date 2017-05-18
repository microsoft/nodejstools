// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.NodejsTools.Project
{
    [ComVisible(true)]
    public class NodejsTypeScriptFileNodeProperties : IncludedFileNodeProperties
    {
        internal NodejsTypeScriptFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }

        [SRCategory(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [ResourcesDescription(nameof(Resources.TestFrameworkDescription))]
        public string TestFramework
        {
            get
            {
                var framework = this.HierarchyNode.ItemNode.GetMetadata(SR.TestFramework);
                if (string.IsNullOrWhiteSpace(framework))
                {
                    return string.Empty;
                }
                return Convert.ToString(framework);
            }
            set
            {
                this.HierarchyNode.ItemNode.SetMetadata(SR.TestFramework, value.ToString());
            }
        }
    }

    [ComVisible(true)]
    public class NodejsTypeScriptLinkFileNodeProperties : LinkFileNodeProperties
    {
        internal NodejsTypeScriptLinkFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }

        [SRCategory(SR.Advanced)]
        [LocDisplayName(SR.TestFramework)]
        [ResourcesDescription(nameof(Resources.TestFrameworkDescription))]
        public string TestFramework
        {
            get
            {
                var framework = this.HierarchyNode.ItemNode.GetMetadata(SR.TestFramework);
                if (string.IsNullOrEmpty(framework))
                {
                    return string.Empty;
                }
                return Convert.ToString(framework);
            }
            set
            {
                this.HierarchyNode.ItemNode.SetMetadata(SR.TestFramework, value.ToString());
            }
        }
    }
}


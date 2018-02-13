// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal class ProjectLibraryNode : LibraryNode
    {
        public ProjectLibraryNode(CommonProjectNode project)
            : base(null, project.Caption, project.Caption, LibraryNodeType.PhysicalContainer)
        {
        }

        public override uint CategoryField(LIB_CATEGORY category)
        {
            switch (category)
            {
                case LIB_CATEGORY.LC_NODETYPE:
                    return (uint)_LIBCAT_NODETYPE.LCNT_PROJECT;
            }
            return base.CategoryField(category);
        }

        public override VSTREEDISPLAYDATA DisplayData
        {
            get
            {
                var res = new VSTREEDISPLAYDATA
                {
                    // Use the default Reference icon for projects
                    hImageList = IntPtr.Zero,
                    Image = 192,
                    SelectedImage = 192
                };
                return res;
            }
        }

        public override StandardGlyphGroup GlyphType => StandardGlyphGroup.GlyphCoolProject;
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.Editors.PropertyPages;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Base class for property pages based on a WinForm control.
    /// </summary>
    public abstract class CommonPropertyPage : PropPageBase
    {
        protected override Size DefaultSize { get; set; } = new Size(800, 600);

        public abstract void LoadSettings();

        public abstract Control Control { get; }

        internal virtual CommonProjectNode Project { get; set; }

        protected override void SetObjects(uint count, object[] objects)
        {
            if (objects == null)
            {
                return;
            }

            if (count > 0)
            {
                if (this.Project == null)
                {
                    if (objects[0] is CommonProjectConfig projectConfig)
                    {
                        this.Project = (CommonProjectNode)projectConfig.ProjectMgr;
                    }
                    else if (objects[0] is NodeProperties properties)
                    {
                        this.Project = (CommonProjectNode)(properties).HierarchyNode.ProjectMgr;
                    }
                }
            }
            else
            {
                this.Project = null;
            }

            if (this.Project != null)
            {
                LoadSettings();
            }
        }
    }
}
